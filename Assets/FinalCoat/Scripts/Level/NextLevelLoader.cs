using UnityEngine;
using UnityEngine.SceneManagement;

namespace FinalCoat{
    // Put on the Game Manager object. After the level is completed, loads nextSceneName.
    // Leave nextSceneName empty on the last level.
    public class NextLevelLoader : MonoBehaviour{
        public string nextSceneName = "Level2";
        public float delay = 4f;
        float loadAt = -1f;
        GUIStyle style;
        void OnEnable() => GameEvents.LevelCompleted += OnLevelCompleted;
        void OnDisable() => GameEvents.LevelCompleted -= OnLevelCompleted;
        void OnLevelCompleted(){
            if (string.IsNullOrEmpty(nextSceneName)) return;
            loadAt = Time.unscaledTime + delay;
        }

        void Update(){
            if (loadAt < 0f || Time.unscaledTime < loadAt) return;
            loadAt = -1f;
            Time.timeScale = 1f;
            SceneManager.LoadScene(nextSceneName);
        }

        void OnGUI(){
            if (loadAt < 0f) return;
            if (style == null){
                style = new GUIStyle(GUI.skin.label){ alignment = TextAnchor.MiddleCenter, fontSize = 18, fontStyle = FontStyle.Bold };
                style.normal.textColor = Color.white;
            }
            int secs = Mathf.CeilToInt(loadAt - Time.unscaledTime);
            GUI.Label(new Rect(0f, Screen.height * 0.5f + 60f, Screen.width, 30f), $"Next level in {secs}...", style);
        }
    }
}
