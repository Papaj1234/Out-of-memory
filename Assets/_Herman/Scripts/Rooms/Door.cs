using UnityEngine;

namespace Assets._Herman.Scripts.Rooms
{
    public class Door : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Collider2D blocker;
        [SerializeField] private Sprite openSprite;
        [SerializeField] private Sprite closedSprite;

        public bool IsOpen { get; private set; }
        public Doors Side { get; private set; }
        public Room Owner { get; private set; }

        private void Reset()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            blocker = GetComponent<Collider2D>();
        }

        public void Init(Room owner, Doors side, bool open)
        {
            Owner = owner;
            Side = side;
            SetOpen(open);
        }

        public void Open() => SetOpen(true);
        public void Close() => SetOpen(false);

        public void SetOpen(bool open)
        {
            IsOpen = open;
            if (spriteRenderer != null) spriteRenderer.sprite = open ? openSprite : closedSprite;
            if (blocker != null) blocker.enabled = !open;
            // TODO: анимация и звук открытия/закрытия
        }
    }
}