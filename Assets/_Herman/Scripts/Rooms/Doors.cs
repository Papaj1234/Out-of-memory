using UnityEngine;

namespace Assets._Herman.Scripts.Rooms
{
    [System.Flags]
    public enum Doors
    {
        None = 0,
        Up = 1,
        Down = 2,
        Left = 4,
        Right = 8,
        All = Up | Down | Left | Right
    }
}
