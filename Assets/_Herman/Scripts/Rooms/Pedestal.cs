using System.Collections.Generic;
using UnityEngine;
using Assets._Herman.Scripts.Items;

namespace Assets._Herman.Scripts.Rooms
{
    public class Pedestal : MonoBehaviour
    {
        [Tooltip("Точка, где лежит предмет")]
        [SerializeField] private Transform itemSlot;
        [Tooltip("Префаб предмета на полу (с ItemPickup)")]
        [SerializeField] private ItemPickup pickupPrefab;
        [Tooltip("Из каких предметов выбирать")]
        [SerializeField] private List<ItemData> itemPool = new List<ItemData>();

        public Transform ItemSlot => itemSlot;
        public ItemPickup CurrentItem { get; private set; }

        private void Start()
        {
            if (itemPool.Count > 0)
                PlaceItem(itemPool[Random.Range(0, itemPool.Count)]);
        }

        public void PlaceItem(ItemData item)
        {
            if (pickupPrefab == null || item == null) return;

            if (CurrentItem != null)
                Destroy(CurrentItem.gameObject);

            CurrentItem = Instantiate(pickupPrefab, itemSlot.position, Quaternion.identity, itemSlot);
            CurrentItem.SetItem(item);
        }
    }
}