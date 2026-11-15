using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

namespace Assets._Herman.Scripts.UI
{
    // Проигрывает заставку один раз и загружает игру. Пропуск — клавиша skipKey.
    public class IntroPlayer : MonoBehaviour
    {
        [SerializeField] private VideoPlayer video;
        [SerializeField] private string nextScene = "Main";
        [SerializeField] private bool allowSkip = true;
        [SerializeField] private Key skipKey = Key.F;

        private bool loading;

        private void Awake()
        {
            if (video == null) video = GetComponent<VideoPlayer>();
        }

        private void OnEnable() => video.loopPointReached += OnFinished;
        private void OnDisable() => video.loopPointReached -= OnFinished;

        private void Start() => video.Play();

        private void Update()
        {
            if (!allowSkip || loading || Keyboard.current == null) return;

            if (Keyboard.current[skipKey].wasPressedThisFrame)
                LoadNext();
        }

        private void OnFinished(VideoPlayer vp) => LoadNext();

        private void LoadNext()
        {
            if (loading) return;
            loading = true;

            // запоминаем: заставку больше не показываем
            PlayerPrefs.SetInt(MainMenu.IntroSeenKey, 1);
            PlayerPrefs.Save();

            SceneManager.LoadScene(nextScene);
        }
    }
}