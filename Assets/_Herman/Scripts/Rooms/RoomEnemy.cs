using UnityEngine;

namespace Assets._Herman.Scripts.Rooms
{
    // Метка, которую комната вешает на заспавненного моба.
    // Когда моба уничтожают или выключают — сообщает комнате.
    public class RoomEnemy : MonoBehaviour
    {
        public Room Room { get; private set; }

        public void Init(Room room) => Room = room;

        private void OnDisable()
        {
            if (Room != null)
                Room.OnEnemyRemoved(this);
        }
    }
}