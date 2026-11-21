#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace FinalCoat.EditorTools{
    public static class MainMenuBuilder{
        const string ScenePath = "Assets/FinalCoat/Scenes/MainMenu.unity";
        const string LogoPath = "Assets/FinalCoat/Art/UI/logo.png";
        static readonly Rect BucketArea = Rect.MinMaxRect(0.267f, 0.04f, 0.521f, 0.446f);
        static readonly Rect BrushArea = Rect.MinMaxRect(0.775f, 0.038f, 0.988f, 0.701f);
        static readonly Color Background = new Color(0.95f, 0.91f, 0.84f);
        static readonly Color Ink = new Color(0.03f, 0.09f, 0.2f);
        static readonly Color ButtonNormal = new Color(0.03f, 0.09f, 0.2f);
        static readonly Color ButtonActive = new Color(0.35f, 0f, 0f);
        static readonly Color ButtonPressed = new Color(0.22f, 0f, 0f);
        static readonly Color Gold = new Color(1f, 0.8f, 0.1f);
        static Font font;
        static Sprite roundedSprite;
        static Sprite circleSprite;
        [MenuItem("Final Coat/Build Main Menu Scene")]
        public static void Build(){
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (File.Exists(ScenePath) && !EditorUtility.DisplayDialog(
                    "Rebuild the main menu?",
                    "This replaces " + ScenePath + ". Anything changed by hand in that scene will be lost.",
                    "Rebuild", "Cancel"))
                return;
            EnsureFolder("Assets/FinalCoat/Scenes");
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            roundedSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            circleSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            var logo = AssetDatabase.LoadAssetAtPath<Sprite>(LogoPath);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var gm = TutorialBuilder.BuildWorld();
            if (gm == null) return;
            var eventSystem = MakeEventSystem();
            var canvas = MakeCanvas();
            var menuGo = new GameObject("Main Menu");
            var menu = menuGo.AddComponent<MainMenu>();
            menu.eventSystem = eventSystem;
            menu.gameManager = gm;
            menu.hud = gm.GetComponent<PrototypeHUD>();
            menu.menuRoot = canvas.gameObject;
            menu.backdrop = MakeBackdrop(canvas);
            var home = MakePanel(canvas, "Home", new Vector2(900f, 980f));
            if (logo != null) MakeLogo(home, logo);
            else MakeText(home, "Title", "FINAL COAT", 120, Ink, FontStyle.Bold, 140f);
            MakeText(home, "Tagline", "Two blank toys. One wet paint strand. One open window.", 28, Ink, FontStyle.Italic, 50f);
            menu.playButton = MakeButton(home, "Play");
            menu.creditsButton = MakeButton(home, "Credits");
            menu.quitButton = MakeButton(home, "Quit");
            MakeText(home, "Hint", "Mouse, keyboard (arrows + Enter) or controller (stick + A)", 22, Ink, FontStyle.Normal, 40f);
            var pick = MakePanel(canvas, "Pick Your Toy", new Vector2(1200f, 900f));
            menu.pickGroup = pick.gameObject.AddComponent<CanvasGroup>();
            MakeText(pick, "Heading", "Pick your toy", 64, Ink, FontStyle.Bold, 90f);
            MakeText(pick, "Hint", "You start blank and fill up with your colour as you grab paint.", 24, Ink, FontStyle.Italic, 40f);
            var cards = UI("Cards", pick);
            var row = cards.gameObject.AddComponent<HorizontalLayoutGroup>();
            row.spacing = 60f;
            row.childAlignment = TextAnchor.MiddleCenter;
            row.childControlWidth = true;
            row.childControlHeight = true;
            row.childForceExpandWidth = false;
            row.childForceExpandHeight = false;
            var cardsLe = cards.gameObject.AddComponent<LayoutElement>();
            cardsLe.preferredHeight = 400f;
            cardsLe.preferredWidth = 1060f;
            menu.previousButtons = new Button[2];
            menu.nextButtons = new Button[2];
            menu.colourLabels = new Text[2];
            menu.toyBodies = new Image[2];
            menu.toyJiggles = new LogoJiggle[2];
            MakePlayerCard(cards, menu, 0, "Player 1", "A / D  or  controller 1 d-pad");
            MakePlayerCard(cards, menu, 1, "Player 2", "Left / Right arrows  or  controller 2 d-pad");
            MakeSpacer(pick, 10f);
            menu.startButton = MakeButton(pick, "Start tutorial");
            menu.skipButton = MakeButton(pick, "Skip to Level 1", 520f, 64f, 28);
            menu.pickBackButton = MakeButton(pick, "Back", 520f, 64f, 28);
            LinkVertical(menu.startButton, menu.skipButton, menu.pickBackButton);
            var credits = MakePanel(canvas, "Credits", new Vector2(1000f, 860f));
            MakeText(credits, "Heading", "Credits", 64, Ink, FontStyle.Bold, 90f);
            MakeText(credits, "Team",
                "CS 319, Group 7\n\nAreesha Ashfaq\nMustafa Farhan Qureshi\nSyed Farooq Ahmed",
                30, Ink, FontStyle.Normal, 220f);
            MakeText(credits, "Assets",
                "Made with Unity.\nLogo, wallpaper and art: Areesha Ashfaq\nFurniture: 2D Side-Scroller Furniture Spritesheet by Monkeystein Studios (itch.io)",
                24, Ink, FontStyle.Normal, 200f, 900f);
            menu.creditsBackButton = MakeButton(credits, "Back");
            menu.homePanel = home.gameObject;
            menu.pickPanel = pick.gameObject;
            menu.creditsPanel = credits.gameObject;
            pick.gameObject.SetActive(false);
            credits.gameObject.SetActive(false);
            EditorSceneManager.SaveScene(scene, ScenePath);
            PutFirstInBuild(ScenePath);
            Selection.activeGameObject = menuGo;
            Debug.Log("Final Coat: main menu built at " + ScenePath + ". Press Play.");
        }

        static void MakeLogo(Transform parent, Sprite logo){
            var rt = UI("Logo", parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = logo;
            img.preserveAspect = true;
            img.raycastTarget = true;
            var le = rt.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = 600f;
            le.preferredHeight = 600f * logo.rect.height / logo.rect.width;
            var jiggle = rt.gameObject.AddComponent<LogoJiggle>();
            MakeSplashSpot(rt, "Bucket Splash", BucketArea, jiggle);
            MakeSplashSpot(rt, "Brush Splash", BrushArea, jiggle);
        }

        static void MakeSplashSpot(RectTransform logo, string name, Rect area, LogoJiggle jiggle){
            var rt = UI(name, logo);
            rt.anchorMin = area.min;
            rt.anchorMax = area.max;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var img = rt.gameObject.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0f);
            var splash = rt.gameObject.AddComponent<PaintSplash>();
            splash.dropSprite = circleSprite;
            splash.jiggle = jiggle;
        }

        static void MakePlayerCard(Transform parent, MainMenu menu, int index, string title, string keys){
            var card = UI(title + " Card", parent);
            var bg = card.gameObject.AddComponent<Image>();
            bg.sprite = roundedSprite;
            bg.type = Image.Type.Sliced;
            bg.color = new Color(1f, 1f, 1f, 0.6f);
            bg.raycastTarget = false;
            var layout = card.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 8f;
            layout.padding = new RectOffset(20, 20, 20, 20);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            var le = card.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = 480f;
            le.preferredHeight = 390f;
            MakeText(card, "Title", title, 36, Ink, FontStyle.Bold, 46f, 440f);
            MakeText(card, "Keys", keys, 20, Ink, FontStyle.Normal, 28f, 440f);
            var holder = UI("Toy", card);
            var holderLe = holder.gameObject.AddComponent<LayoutElement>();
            holderLe.preferredWidth = 200f;
            holderLe.preferredHeight = 200f;
            int facing = index == 0 ? 1 : -1;
            Shape(holder, "Outline", Vector2.zero, new Vector2(128f, 140f), Ink, null);
            menu.toyBodies[index] = Shape(holder, "Body", Vector2.zero, new Vector2(116f, 128f), Color.white, null);
            Shape(holder, "Eye", new Vector2(10f * facing, 22f), new Vector2(18f, 26f), Ink, circleSprite);
            Shape(holder, "Eye", new Vector2(38f * facing, 22f), new Vector2(18f, 26f), Ink, circleSprite);
            var jiggle = holder.gameObject.AddComponent<LogoJiggle>();
            jiggle.autoBoingEvery = 0f;
            jiggle.wobbleAngle = 4f;
            jiggle.boingStrength = 0.25f;
            jiggle.phase = index * 1.3f;
            menu.toyJiggles[index] = jiggle;
            var pickerRow = UI("Colour Row", card);
            var rowLayout = pickerRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            rowLayout.childAlignment = TextAnchor.MiddleCenter;
            rowLayout.spacing = 16f;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = false;
            var rowLe = pickerRow.gameObject.AddComponent<LayoutElement>();
            rowLe.preferredWidth = 440f;
            rowLe.preferredHeight = 70f;
            menu.previousButtons[index] = MakeButton(pickerRow, "<", 70f, 70f, 36);
            menu.colourLabels[index] = MakeText(pickerRow, "Colour", "Red", 34, Ink, FontStyle.Bold, 70f, 220f);
            menu.nextButtons[index] = MakeButton(pickerRow, ">", 70f, 70f, 36);
            NoNavigation(menu.previousButtons[index]);
            NoNavigation(menu.nextButtons[index]);
        }

        static void LinkVertical(params Button[] buttons){
            for (int i = 0; i < buttons.Length; i++){
                var nav = new Navigation{ mode = Navigation.Mode.Explicit };
                nav.selectOnUp = i > 0 ? buttons[i - 1] : null;
                nav.selectOnDown = i < buttons.Length - 1 ? buttons[i + 1] : null;
                buttons[i].navigation = nav;
            }
        }

        static void NoNavigation(Button button) => button.navigation = new Navigation{ mode = Navigation.Mode.None };
        static EventSystem MakeEventSystem(){
            var go = new GameObject("EventSystem");
            var es = go.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            var module = go.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
#else
            go.AddComponent<StandaloneInputModule>();
#endif
            return es;
        }

        static Transform MakeCanvas(){
            var go = new GameObject("Canvas");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            return go.transform;
        }

        static CanvasGroup MakeBackdrop(Transform canvas){
            var root = UI("Backdrop", canvas);
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;
            var group = root.gameObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            var bg = UI("Background", root);
            bg.anchorMin = Vector2.zero;
            bg.anchorMax = Vector2.one;
            bg.offsetMin = Vector2.zero;
            bg.offsetMax = Vector2.zero;
            var img = bg.gameObject.AddComponent<Image>();
            img.color = Background;
            img.raycastTarget = false;
            var stripe = UI("Paint Stripe", root);
            stripe.anchorMin = new Vector2(0f, 0f);
            stripe.anchorMax = new Vector2(1f, 0f);
            stripe.pivot = new Vector2(0.5f, 0f);
            stripe.sizeDelta = new Vector2(0f, 24f);
            var stripeImg = stripe.gameObject.AddComponent<Image>();
            stripeImg.color = ButtonActive;
            stripeImg.raycastTarget = false;
            return group;
        }

        static RectTransform MakePanel(Transform parent, string name, Vector2 size){
            var rt = UI(name, parent);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = Vector2.zero;
            var layout = rt.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 14f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            return rt;
        }

        static Image Shape(Transform parent, string name, Vector2 position, Vector2 size, Color color, Sprite sprite){
            var rt = UI(name, parent);
            rt.sizeDelta = size;
            rt.anchoredPosition = position;
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        static Text MakeText(Transform parent, string name, string content, int size, Color color, FontStyle style, float height, float width = 820f){
            var rt = UI(name, parent);
            var text = rt.gameObject.AddComponent<Text>();
            text.font = font;
            text.text = content;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            var le = rt.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = height;
            le.preferredWidth = width;
            return text;
        }

        static Button MakeButton(Transform parent, string label, float width = 520f, float height = 76f, int fontSize = 34){
            var rt = UI(label + " Button", parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = roundedSprite;
            img.type = Image.Type.Sliced;
            img.color = Color.white;
            var button = rt.gameObject.AddComponent<Button>();
            button.targetGraphic = img;
            var colors = ColorBlock.defaultColorBlock;
            colors.normalColor = ButtonNormal;
            colors.highlightedColor = ButtonActive;
            colors.selectedColor = ButtonActive;
            colors.pressedColor = ButtonPressed;
            colors.disabledColor = new Color(0.6f, 0.6f, 0.6f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.08f;
            button.colors = colors;
            var le = rt.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = width;
            le.preferredHeight = height;
            var text = MakeText(rt, "Label", label, fontSize, Color.white, FontStyle.Bold, height, width);
            var trt = text.rectTransform;
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;
            var underline = UI("Gold Edge", rt);
            underline.anchorMin = new Vector2(0f, 0f);
            underline.anchorMax = new Vector2(1f, 0f);
            underline.pivot = new Vector2(0.5f, 0f);
            underline.sizeDelta = new Vector2(-24f, 5f);
            underline.anchoredPosition = new Vector2(0f, 6f);
            var edge = underline.gameObject.AddComponent<Image>();
            edge.color = Gold;
            edge.raycastTarget = false;
            return button;
        }

        static void MakeSpacer(Transform parent, float height){
            var rt = UI("Spacer", parent);
            rt.gameObject.AddComponent<LayoutElement>().preferredHeight = height;
        }

        static RectTransform UI(string name, Transform parent){
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        static void EnsureFolder(string path){
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        static void PutFirstInBuild(string path){
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            scenes.RemoveAll(s => s.path == path || s.path == "Assets/FinalCoat/Scenes/Tutorial.unity");
            scenes.Insert(0, new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
#endif
