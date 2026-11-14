using UnityEngine;

namespace Assets._Herman.Scripts.Level
{
    // Ставит героя и камеру в центр стартовой комнаты, когда уровень построен
    public class PlayerSpawner : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private Camera cam;

        private void OnEnable() => LevelBuilder.Built += OnLevelBuilt;
        private void OnDisable() => LevelBuilder.Built -= OnLevelBuilt;

        private void OnLevelBuilt(LevelBuilder level)
        {
            var room = level.StartRoom;
            if (room == null || player == null) return;

            Vector3 pos = room.CellToWorld(room.Layout.width / 2, room.Layout.height / 2);

            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.position = pos;
                rb.linearVelocity = Vector2.zero;
            }
            player.position = pos;

            if (cam != null)
                cam.transform.position = new Vector3(pos.x, pos.y, cam.transform.position.z);
        }
    }
}