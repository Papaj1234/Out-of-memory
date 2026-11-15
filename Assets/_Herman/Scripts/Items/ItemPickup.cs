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

        [Header("Size")]
        [Tooltip("Размер иконки в клетках (по большей стороне), любая картинка подгоняется под него")]
        [SerializeField] private float iconSize = 0.7f;
        [Tooltip("Радиус подбора в клетках")]
        [SerializeField] private float pickupRadius = 0.4f;

        [Header("Bobbing")]
        [Tooltip("Насколько высоко качается (в клетках)")]
        [SerializeField] private float bobHeight = 0.08f;
        [Tooltip("Скорость покачивания")]
        [SerializeField] private float bobSpeed = 2.5f;

        public ItemData Item => item;

        private bool isPickedUp;
        private Vector3 baseLocalPos;
        private float bobPhase;

        private void Awake()
        {
            GetComponent<CircleCollider2D>().isTrigger = true;
            RefreshIcon();
        }

        private void Start()
        {
            if (iconRenderer == null) return;
            baseLocalPos = iconRenderer.transform.localPosition;
            bobPhase = Random.value * Mathf.PI * 2f; // у каждого предмета своя фаза
        }

        private void Update()
        {
            if (iconRenderer == null || isPickedUp) return;

            float offset = Mathf.Sin(Time.time * bobSpeed + bobPhase) * bobHeight;
            iconRenderer.transform.localPosition = baseLocalPos + Vector3.up * offset;
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

            FitIconSize();
        }

        // Любая картинка (любой размер и PPU) -> iconSize клеток по большей стороне
        private void FitIconSize()
        {
            var sprite = iconRenderer.sprite;
            if (sprite == null) return;

            Vector2 size = sprite.bounds.size;
            float biggest = Mathf.Max(size.x, size.y);
            if (biggest <= 0f) return;

            float scale = iconSize / biggest;
            iconRenderer.transform.localScale = new Vector3(scale, scale, 1f);

            // если иконка на том же объекте, что и коллайдер — не даём радиусу подбора уехать
            var col = GetComponent<CircleCollider2D>();
            if (iconRenderer.transform == transform)
                col.radius = pickupRadius / scale;
            else
                col.radius = pickupRadius;
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