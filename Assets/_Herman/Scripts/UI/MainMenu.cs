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
            SceneManager.LoadScene(introScene);
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