using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets._Herman.Scripts.Rooms
{
    public class RoomDebug : MonoBehaviour
    {
        [SerializeField] private Room room;
        [SerializeField] private RoomType type;
        [SerializeField] private Doors doors = Doors.All;
        [Tooltip("0 = random")]
        [SerializeField] private int seed = 0;

        private void Start() => Build();

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
                Build();
        }

        private void Build()
        {
            int s = seed != 0 ? seed : Random.Range(1, int.MaxValue);
            Debug.Log($"[{type.name}] seed: {s}");
            room.Build(type, RoomGenerator.Generate(type, doors, s));
        }
    }
}