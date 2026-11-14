using UnityEngine;
using Assets._Herman.Scripts.Rooms;
using UnityEngine.SceneManagement;
using Assets._Herman.Scripts.UI;

namespace Assets._Herman.Scripts.Level
{
    // Переходы между комнатами как в Isaac: видна только текущая комната,
    // открытая дверь переносит героя в соседнюю, камера перескакивает.
    public class RoomNavigator : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private Camera cam;
        [Tooltip("Защита от двойного перехода")]
        [SerializeField] private float travelCooldown = 0.3f;
        [Tooltip("Через сколько секунд после смерти начинать новый забег")]
        [SerializeField] private float restartDelay = 2f;
        [SerializeField] private float fadeOutDuration = 1f;
        private bool restarting;

        public Room CurrentRoom { get; private set; }

        private LevelBuilder level;
        private float lastTravelTime = -999f;

        private PlayerHealth playerHealth;

        private void OnEnable()
        {
            LevelBuilder.Built += OnLevelBuilt;
            Door.PlayerTouched += OnDoorTouched;

            if (player != null)
                playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.Died += OnPlayerDied;
        }

        private void OnDisable()
        {
            LevelBuilder.Built -= OnLevelBuilt;
            Door.PlayerTouched -= OnDoorTouched;

            if (playerHealth != null)
                playerHealth.Died -= OnPlayerDied;
        }

        private void OnPlayerDied()
        {
            if (restarting) return;
            restarting = true;
            StartCoroutine(DeathSequence());
        }

        private System.Collections.IEnumerator DeathSequence()
        {
            yield return new WaitForSeconds(restartDelay);   // пауза, видно что умер

            if (ScreenFader.Instance != null)
                yield return ScreenFader.Instance.FadeOut(fadeOutDuration);   // затемнение

            RestartRun();   // новый уровень, он сам плавно проявится
        }

        private void RestartRun()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void OnLevelBuilt(LevelBuilder builder)
        {
            level = builder;
            StopAllCoroutines();
            StartCoroutine(EnterStartNextFrame());
        }

        // ждём один кадр, чтобы все Start() (в т.ч. у героя) уже отработали
        private System.Collections.IEnumerator EnterStartNextFrame()
        {
            yield return null;
            if (level != null && level.StartRoom != null)
                EnterRoom(level.StartRoom, level.StartRoom.CenterWorld);
        }

        private void OnDoorTouched(Door door)
        {
            if (level == null || door.Owner != CurrentRoom) return;
            if (Time.time - lastTravelTime < travelCooldown) return;

            var target = level.GetNeighbour(door.Owner, door.Side);
            if (target == null) return;

            lastTravelTime = Time.time;

            // от двери выхода в ту же сторону: дверь соседа -> край пола -> ещё клетка
            Vector3 dir = (Vector2)LevelBuilder.Offset(door.Side);
            Vector3 entry = door.transform.position + dir * 2f;

            Debug.Log($"Travel {door.Side}: {CurrentRoom.name} -> {target.name}, entry {entry}");
            EnterRoom(target, entry, keepVelocity: true);
        }

        private void EnterRoom(Room room, Vector3 playerPos, bool keepVelocity = false)
        {
            // включаем только нужную комнату
            foreach (var r in level.Rooms)
                if (r != null) r.gameObject.SetActive(r == room);

            CurrentRoom = room;
            MovePlayer(playerPos, keepVelocity);
            MoveCamera(room.CenterWorld);
        }

        private void MovePlayer(Vector3 pos, bool keepVelocity)
        {
            if (player == null) return;

            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.position = pos;
                if (!keepVelocity) rb.linearVelocity = Vector2.zero;
            }
            player.position = pos;
        }

        private void MoveCamera(Vector3 pos)
        {
            if (cam == null) return;
            cam.transform.position = new Vector3(pos.x, pos.y, cam.transform.position.z);
        }

        private static Doors Opposite(Doors side)
        {
            switch (side)
            {
                case Doors.Up: return Doors.Down;
                case Doors.Down: return Doors.Up;
                case Doors.Left: return Doors.Right;
                default: return Doors.Left;
            }
        }
    }
}