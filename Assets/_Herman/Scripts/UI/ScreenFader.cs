using System.Collections;
using UnityEngine;

namespace Assets._Herman.Scripts.UI
{
    // Чёрный экран поверх всего: плавное затемнение / появление
    public class ScreenFader : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private float fadeInDuration = 0.6f;

        public static ScreenFader Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            if (group != null)
            {
                group.alpha = 1f;              // сцена начинается с чёрного
                group.blocksRaycasts = false;  // не мешает кликам
            }
        }

        private void Start()
        {
            StartCoroutine(Fade(1f, 0f, fadeInDuration));
        }

        public IEnumerator FadeOut(float duration)
        {
            return Fade(group != null ? group.alpha : 0f, 1f, duration);
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            if (group == null) yield break;

            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                group.alpha = Mathf.Lerp(from, to, t / duration);
                yield return null;
            }
            group.alpha = to;
        }
    }
}