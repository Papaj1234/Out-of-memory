using UnityEngine;

namespace Assets._Herman.Scripts.Items
{
    // Предмет на полу. Подбирается при касании героя (нужен PlayerStats на герое).
    [RequireComponent(typeof(CircleCollider2D))]
    public class ItemPickup : MonoBehaviour
    {
        [SerializeField] private ItemData item;
        [Tooltip("Если задан — сюда автоматически ставится иконка предмета")]
        [SerializeField] private SpriteRenderer iconRenderer;
        [Tooltip("Показывается, если у предмета нет иконки")]
        [SerializeField] private Sprite fallbackIcon;

        public ItemData Item => item;

        private bool isPickedUp;

        private void Awake()
        {
            GetComponent<CircleCollider2D>().isTrigger = true;
            RefreshIcon();
        }

        private void Reset()
        {
            GetComponent<CircleCollider2D>().isTrigger = true;
            iconRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        // Для пьедестала: положить конкретный предмет
        public void SetItem(ItemData newItem)
        {
            item = newItem;
            RefreshIcon();
        }

        private void RefreshIcon()
        {
            if (iconRenderer == null) return;

            Sprite icon = item != null ? item.Icon : null;
            iconRenderer.sprite = icon != null ? icon : fallbackIcon;
        }

        private void OnTriggerEnter2D(Collider2D other) => TryPickUp(other);
        private void OnTriggerStay2D(Collider2D other) => TryPickUp(other);

        private void TryPickUp(Collider2D other)
        {
            if (isPickedUp || item == null)
                return;

            PlayerStats stats = other.GetComponentInParent<PlayerStats>();
            if (stats == null)
                return;

            // защита от повторного срабатывания с нескольких коллайдеров героя
            isPickedUp = true;

            PlayerHealth health = stats.GetComponent<PlayerHealth>();
            float maxBefore = health != null ? health.MaxHealth : 0f;

            stats.AddItem(item);

            // предмет добавил сердца — сразу заполняем их, как в Isaac
            if (health != null)
            {
                float gained = health.MaxHealth - maxBefore;
                if (gained > 0f)
                    health.Heal(gained);
            }

            Destroy(gameObject);
        }
    }
}