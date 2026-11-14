using UnityEngine;

namespace Assets._Herman.Scripts.Rooms
{
    // Невидимая зона внутри комнаты: герой вошёл -> комната начинает бой
    public class RoomTrigger : MonoBehaviour
    {
        private Room room;

        public void Init(Room owner) => room = owner;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (room == null) return;
            if (other.GetComponentInParent<PlayerStats>() == null) return;
            room.OnPlayerEnter();
        }
    }
}
