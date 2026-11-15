using UnityEditor;
using UnityEngine;

namespace Assets._Herman.Scripts.UI.Editor
{
    public static class IntroReset
    {
        [MenuItem("Tools/Herman/Reset Intro (show again)")]
        public static void Reset()
        {
            PlayerPrefs.DeleteKey(MainMenu.IntroSeenKey);
            PlayerPrefs.Save();
            Debug.Log("Заставка будет показана снова");
        }
    }
}