using System.Collections;
using UnityEngine;

namespace Assets._Herman.Scripts.Rooms
{
    public abstract class RoomFeature : ScriptableObject
    {
        public virtual void ModifyLayout(RoomLayout layout, System.Random rng) { }

        public virtual void Populate(Room room, RoomLayout layout) { }
    }
}