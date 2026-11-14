using UnityEngine;

namespace Assets._Herman.Scripts.Rooms
{
    public class Pedestal : MonoBehaviour
    {
        [Tooltip("Точка, где будет лежать предмет")]
        [SerializeField] private Transform itemSlot;

        public Transform ItemSlot => itemSlot;
        public GameObject CurrentItem { get; private set; }

        // TODO: генерация предмета — вызвать отсюда, когда будут предметы
        public void PlaceItem(GameObject itemPrefab)
        {
        }
    }
}