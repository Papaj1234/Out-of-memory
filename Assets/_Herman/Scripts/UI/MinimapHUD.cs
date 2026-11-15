using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets._Herman.Scripts.Level;
using Assets._Herman.Scripts.Rooms;

namespace Assets._Herman.Scripts.UI
{
    // Мини-карта как в Isaac: квадратики из стандартных UI Image
    public class MinimapHUD : MonoBehaviour
    {
        [Tooltip("Пусто = найдёт сам")]
        [SerializeField] private RoomNavigator navigator;
        [Tooltip("Можно оставить пустым — будут простые прямоугольники")]
        [SerializeField] private Sprite cellSprite;

        [SerializeField] private Vector2 cellSize = new Vector2(26f, 16f);
        [SerializeField] private float spacing = 4f;

        [Header("Colors")]
        [SerializeField] private Color currentColor = new Color(1f, 1f, 1f, 1f);
        [SerializeField] private Color visitedColor = new Color(0.6f, 0.6f, 0.65f, 0.9f);
        [SerializeField] private Color unknownColor = new Color(0.25f, 0.25f, 0.3f, 0.8f);
        [SerializeField] private Color treasureColor = new Color(1f, 0.85f, 0.2f, 1f);
        [SerializeField] private Color bossColor = new Color(0.9f, 0.15f, 0.2f, 1f);

        private LevelBuilder level;
        private readonly Dictionary<Room, Image> cells = new Dictionary<Room, Image>();
        private readonly HashSet<Room> visited = new HashSet<Room>();
        private Room lastCurrent;

        private void Awake()
        {
            if (navigator == null) navigator = FindAnyObjectByType<RoomNavigator>();
        }

        private void OnEnable() => LevelBuilder.Built += OnLevelBuilt;
        private void OnDisable() => LevelBuilder.Built -= OnLevelBuilt;

        private void OnLevelBuilt(LevelBuilder builder)
        {
            level = builder;

            foreach (var img in cells.Values)
                if (img != null) Destroy(img.gameObject);
            cells.Clear();
            visited.Clear();
            lastCurrent = null;

            foreach (var room in level.Rooms)
            {
                var go = new GameObject($"Map {room.GridPos}", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(transform, false);

                var img = go.GetComponent<Image>();
                img.sprite = cellSprite;
                img.raycastTarget = false;
                img.rectTransform.sizeDelta = cellSize;
                go.SetActive(false);

                cells[room] = img;

                Debug.Log($"[Minimap] {room.name}: {level.GetRole(room)}");
            }
        }

        private void Update()
        {
            if (level == null || navigator == null) return;

            var current = navigator.CurrentRoom;
            if (current == null || current == lastCurrent) return;

            lastCurrent = current;
            visited.Add(current);
            Refresh(current);
        }

        private void Refresh(Room current)
        {
            Vector2 step = cellSize + Vector2.one * spacing;

            foreach (var pair in cells)
            {
                Room room = pair.Key;
                Image img = pair.Value;

                bool isVisited = visited.Contains(room);
                bool isKnown = isVisited || IsNextToVisited(room);
                img.gameObject.SetActive(isKnown);
                if (!isKnown) continue;

                // текущая комната — в центре карты
                Vector2 offset = room.GridPos - current.GridPos;
                img.rectTransform.anchoredPosition = Vector2.Scale(offset, step);

                img.color = ColorFor(room, room == current, isVisited);
            }
        }

        private Color ColorFor(Room room, bool isCurrent, bool isVisited)
        {
            if (isCurrent) return currentColor;

            switch (level.GetRole(room))
            {
                case RoomRole.Treasure: return treasureColor;
                case RoomRole.Boss: return bossColor;
            }

            return isVisited ? visitedColor : unknownColor;
        }

        // соседние клетки на сетке всегда соединены дверью (так строится уровень)
        private bool IsNextToVisited(Room room)
        {
            foreach (var v in visited)
            {
                var d = room.GridPos - v.GridPos;
                if (Mathf.Abs(d.x) + Mathf.Abs(d.y) == 1) return true;
            }
            return false;
        }
    }
}