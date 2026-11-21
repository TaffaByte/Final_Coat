using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FinalCoat{
    public class MainMenu : MonoBehaviour{
        public string firstLevelScene = "Prototype";
        public EventSystem eventSystem;
        public GameManager gameManager;
        public PrototypeHUD hud;
        public float handOffHeight = 3f;
        public float zoomOutTime = 1.1f;
        [Header("Screens")]
        public GameObject menuRoot;
        public CanvasGroup backdrop;
        public CanvasGroup pickGroup;
        public GameObject homePanel;
        public GameObject pickPanel;
        public GameObject creditsPanel;
        [Header("Home")]
        public Button playButton;
        public Button creditsButton;
        public Button quitButton;
        [Header("Pick your toy")]
        public Button[] previousButtons;
        public Button[] nextButtons;
        public Text[] colourLabels;
        public Image[] toyBodies;
        public LogoJiggle[] toyJiggles;
        public Button startButton;
        public Button skipButton;
        public Button pickBackButton;
        [Header("Credits")]
        public Button creditsBackButton;
        GameObject currentPanel;
        Selectable currentFirst;
        bool playing;
        void Awake(){
            Time.timeScale = 1f;
            EnsureUIActions();
            playButton.onClick.AddListener(() => Show(pickPanel, startButton));
            creditsButton.onClick.AddListener(() => Show(creditsPanel, creditsBackButton));
            quitButton.onClick.AddListener(Quit);
            for (int i = 0; i < GameSettings.PlayerCount; i++){
                int player = i;
                previousButtons[i].onClick.AddListener(() => ChangeColour(player, -1));
                nextButtons[i].onClick.AddListener(() => ChangeColour(player, 1));
            }
            startButton.onClick.AddListener(() => StartCoroutine(JumpDown(false)));
            skipButton.onClick.AddListener(() => StartCoroutine(JumpDown(true)));
            pickBackButton.onClick.AddListener(ShowHome);
            creditsBackButton.onClick.AddListener(ShowHome);
        }

        void EnsureUIActions(){
#if ENABLE_INPUT_SYSTEM
            if (eventSystem == null) return;
            var module = eventSystem.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            if (module != null && module.actionsAsset == null) module.AssignDefaultActions();
#endif
        }

        void Start(){
            if (gameManager != null) gameManager.InMenu = true;
            if (hud != null) hud.enabled = false;
            ShowWorldToys(false);
            Refresh();
            ShowHome();
        }

        void Update(){
            if (playing) return;
            if (currentPanel != homePanel && ToyInput.BackPressed) ShowHome();
            if (currentPanel == pickPanel){
                if (ToyInput.Pressed(ControlScheme.WASD, GameKey.Left)) ChangeColour(0, -1);
                if (ToyInput.Pressed(ControlScheme.WASD, GameKey.Right)) ChangeColour(0, 1);
                if (ToyInput.Pressed(ControlScheme.Arrows, GameKey.Left)) ChangeColour(1, -1);
                if (ToyInput.Pressed(ControlScheme.Arrows, GameKey.Right)) ChangeColour(1, 1);
            }
            var es = EventSystem.current;
            if (es != null && es.currentSelectedGameObject == null && currentFirst != null)
                es.SetSelectedGameObject(currentFirst.gameObject);
        }

        public void ShowHome() => Show(homePanel, playButton);
        void Show(GameObject panel, Selectable first){
            homePanel.SetActive(panel == homePanel);
            pickPanel.SetActive(panel == pickPanel);
            creditsPanel.SetActive(panel == creditsPanel);
            currentPanel = panel;
            currentFirst = first;
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(first.gameObject);
        }

        void ChangeColour(int player, int step){
            GameSettings.CycleColour(player, step);
            Refresh();
            if (toyJiggles != null && player < toyJiggles.Length && toyJiggles[player] != null) toyJiggles[player].Boing();
        }

        void Refresh(){
            for (int i = 0; i < GameSettings.PlayerCount; i++){
                if (i < colourLabels.Length) colourLabels[i].text = GameSettings.GetColourName(i);
                if (i < toyBodies.Length) toyBodies[i].color = GameSettings.GetColour(i);
            }
            if (gameManager == null || gameManager.toys == null) return;
            foreach (var toy in gameManager.toys)
                if (toy != null) toy.UseSavedColour();
        }

        IEnumerator JumpDown(bool toLevel1){
            if (playing) yield break;
            playing = true;
            currentFirst = null;
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
            if (pickGroup != null) pickGroup.interactable = false;
            if (toLevel1 && !Application.CanStreamedLevelBeLoaded(firstLevelScene)){
                Debug.LogWarning($"Final Coat: scene '{firstLevelScene}' is not in the build yet.");
                toLevel1 = false;
            }
            var bodies = new RectTransform[toyJiggles.Length];
            for (int i = 0; i < toyJiggles.Length; i++){
                toyJiggles[i].Boing();
                toyJiggles[i].enabled = false;
                bodies[i] = (RectTransform)toyJiggles[i].transform;
                bodies[i].SetParent(menuRoot.transform, true);
            }
            if (toLevel1){
                yield return FallOffScreen(bodies);
                GameSettings.DropInNextScene = true;
                SceneManager.LoadScene(firstLevelScene);
                yield break;
            }
            yield return HandOff(bodies);
            menuRoot.SetActive(false);
            if (hud != null) hud.enabled = true;
            if (gameManager != null) gameManager.InMenu = false;
        }

        void ShowWorldToys(bool show){
            if (gameManager == null || gameManager.toys == null) return;
            foreach (var toy in gameManager.toys)
                if (toy != null && toy.visual != null) toy.visual.gameObject.SetActive(show);
            if (gameManager.strand != null && gameManager.strand.TryGetComponent(out LineRenderer line)) line.enabled = show;
        }

        IEnumerator HandOff(RectTransform[] bodies){
            var starts = new Vector3[bodies.Length];
            for (int i = 0; i < bodies.Length; i++) starts[i] = bodies[i].position;
            float hop = 0.22f;
            float time = 0f;
            while (time < hop){
                time += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(time / hop);
                for (int i = 0; i < bodies.Length; i++)
                    bodies[i].position = starts[i] + Vector3.up * Screen.height * 0.05f * Mathf.Sin(k * Mathf.PI);
                if (pickGroup != null) pickGroup.alpha = 1f - k;
                yield return null;
            }
            if (pickGroup != null) pickGroup.alpha = 0f;
            var cam = Camera.main;
            if (cam == null || gameManager == null) yield break;
            var follow = cam.GetComponent<CameraFollow>();
            if (follow != null) follow.Hold(30f);
            float endSize = cam.orthographicSize;
            float uiPixels = 140f * bodies[0].lossyScale.y;
            float startSize = 0.98f * Screen.height / (2f * Mathf.Max(1f, uiPixels));
            Vector2 land = Vector2.zero;
            foreach (var toy in gameManager.toys) land += (Vector2)toy.transform.position;
            land /= gameManager.toys.Length;
            cam.orthographicSize = startSize;
            cam.transform.position = new Vector3(follow != null ? follow.ClampX(land.x, startSize) : land.x, land.y + handOffHeight, cam.transform.position.z);
            for (int i = 0; i < bodies.Length && i < gameManager.toys.Length; i++){
                var toy = gameManager.toys[i];
                Vector3 world = cam.ScreenToWorldPoint(bodies[i].position);
                world.z = 0f;
                toy.transform.position = world;
                if (toy.Body != null){
                    toy.Body.position = world;
                    toy.Body.SetVelocity(Vector2.zero);
                }
                var paint = toy.GetComponent<ToyPaint>();
                if (paint != null && paint.body != null) paint.body.color = GameSettings.GetColour(i);
                bodies[i].gameObject.SetActive(false);
            }
            ShowWorldToys(true);
            Vector3 camStart = cam.transform.position;
            time = 0f;
            while (time < zoomOutTime){
                time += Time.unscaledDeltaTime;
                float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(time / zoomOutTime));
                if (backdrop != null) backdrop.alpha = 1f - Mathf.Clamp01(time / 0.25f);
                Vector2 center = Vector2.zero;
                foreach (var toy in gameManager.toys) center += (Vector2)toy.transform.position;
                center /= gameManager.toys.Length;
                float size = Mathf.Lerp(startSize, endSize, k);
                Vector3 target = new Vector3(follow.ClampX(center.x + follow.offset.x, size), center.y + follow.offset.y, camStart.z);
                cam.transform.position = Vector3.Lerp(camStart, target, k);
                cam.orthographicSize = size;
                yield return null;
            }
            if (follow != null) follow.Hold(0f);
        }

        IEnumerator FallOffScreen(RectTransform[] bodies){
            var speeds = new float[bodies.Length];
            for (int i = 0; i < bodies.Length; i++) speeds[i] = Screen.height * (0.9f + i * 0.1f);
            float gravity = Screen.height * 4f;
            float time = 0f;
            while (true){
                float dt = Time.unscaledDeltaTime;
                time += dt;
                bool allGone = true;
                for (int i = 0; i < bodies.Length; i++){
                    if (time < i * 0.12f){
                        allGone = false;
                        continue;
                    }
                    speeds[i] -= gravity * dt;
                    bodies[i].position += Vector3.up * speeds[i] * dt;
                    bodies[i].Rotate(0f, 0f, (i == 0 ? 1f : -1f) * 200f * dt);
                    if (bodies[i].position.y > -Screen.height * 0.3f) allGone = false;
                }
                if (pickGroup != null) pickGroup.alpha = Mathf.Clamp01(1f - time * 3f);
                if (allGone) yield break;
                yield return null;
            }
        }

        void Quit(){
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
