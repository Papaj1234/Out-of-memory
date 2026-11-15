using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets._Herman.Scripts.UI
{
    public class MainMenu : MonoBehaviour
    {
        public const string IntroSeenKey = "IntroSeen";

        [SerializeField] private string introScene = "Intro";
        [SerializeField] private string gameScene = "Main";

        // Кнопка "Начать игру"
        public void StartGame()
        {
            bool introSeen = PlayerPrefs.GetInt(IntroSeenKey, 0) == 1;
            SceneManager.LoadScene(introSeen ? gameScene : introScene);
        }

        // Кнопка "Выход"
        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}