using UnityEngine;

namespace StudentSandbox
{
    /// <summary>
    /// Die Bildschirm-Anzeige im Play Mode: Steuerungshilfe, Hinweis unter dem Mauszeiger
    /// und kurze Meldungen wie "Ziel erreicht!". Benutzt bewusst das einfache IMGUI (OnGUI),
    /// damit keine zusätzlichen Pakete oder Schriftarten nötig sind.
    /// </summary>
    [AddComponentMenu("Student Sandbox/Sandbox HUD")]
    [DisallowMultipleComponent]
    public class SandboxHUD : MonoBehaviour
    {
        [Header("Anzeige")]
        [Tooltip("Zeigt die Steuerungshilfe oben links. Im Play Mode mit der Taste H ein- und ausschalten.")]
        [SerializeField] bool showHelp = true;
        [Tooltip("Größe der Schrift auf dem Bildschirm.")]
        [SerializeField, Range(0.5f, 2.5f)] float uiScale = 1f;

        static SandboxHUD instance;

        string message;
        float messageEndTime;
        GUIStyle panelStyle, textStyle, titleStyle, messageStyle, hoverStyle;

        public bool ShowHelp
        {
            get => showHelp;
            set => showHelp = value;
        }

        /// <summary>Der zuletzt angezeigte Meldungstext (für Tests).</summary>
        public static string LastMessage { get; private set; }

        /// <summary>Zeigt für ein paar Sekunden eine große Meldung in der Bildschirmmitte.</summary>
        public static void ShowMessage(string text, float seconds = 3f)
        {
            LastMessage = text;
            if (instance == null) instance = FindAnyObjectByType<SandboxHUD>();
            if (instance == null)
            {
                Debug.Log("[Student Sandbox] " + text);
                return;
            }
            instance.message = text;
            instance.messageEndTime = Time.unscaledTime + Mathf.Max(0.5f, seconds);
        }

        void Awake()
        {
            instance = this;
        }

        void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        void Update()
        {
            if (SandboxInput.GetKeyDown(SandboxKey.H)) showHelp = !showHelp;
        }

        void OnGUI()
        {
            EnsureStyles();
            float scale = Mathf.Max(0.5f, Screen.height / 1080f) * uiScale;
            Matrix4x4 oldMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            float width = Screen.width / scale;
            float height = Screen.height / scale;

            if (showHelp) DrawHelp();
            else GUI.Label(new Rect(16, 12, 360, 30), "H = Hilfe anzeigen", textStyle);

            DrawHover(scale, height);
            DrawMessage(width);

            GUI.matrix = oldMatrix;
        }

        void DrawHelp()
        {
            const string help =
                "<b>W A S D</b>  fliegen\n" +
                "<b>Q / E</b>  runter / hoch\n" +
                "<b>Shift</b>  schneller fliegen\n" +
                "<b>Rechte Maustaste halten</b>  umschauen\n" +
                "<b>Mausrad</b>  Fluggeschwindigkeit\n" +
                "<b>Linksklick</b>  Schalter drücken\n" +
                "<b>1 – 5</b>  zu den Bereichen fliegen\n" +
                "<b>R</b>  Physik zurücksetzen\n" +
                "<b>T</b>  Tageszeit wechseln\n" +
                "<b>Esc</b>  Mauszeiger freigeben\n" +
                "<b>H</b>  Hilfe ausblenden";
            GUILayout.BeginArea(new Rect(16, 12, 340, 340), panelStyle);
            GUILayout.Label("Steuerung", titleStyle);
            GUILayout.Label(help, textStyle);
            GUILayout.EndArea();
        }

        void DrawHover(float scale, float height)
        {
            string hover = ClickInteractor.CurrentHoverText;
            if (string.IsNullOrEmpty(hover)) return;
            Vector2 mouse = SandboxInput.MousePosition / scale;
            // IMGUI zählt von oben, die Maus-Position von unten.
            var rect = new Rect(mouse.x + 18, height - mouse.y + 8, 320, 36);
            GUI.Label(rect, "Klick: " + hover, hoverStyle);
        }

        void DrawMessage(float width)
        {
            if (string.IsNullOrEmpty(message) || Time.unscaledTime > messageEndTime) return;
            var content = new GUIContent(message);
            float boxWidth = Mathf.Min(width - 40f, Mathf.Max(360f, messageStyle.CalcSize(content).x + 10f), 900f);
            float boxHeight = messageStyle.CalcHeight(content, boxWidth);
            GUI.Label(new Rect(width * 0.5f - boxWidth * 0.5f, 90f, boxWidth, boxHeight), content, messageStyle);
        }

        void EnsureStyles()
        {
            if (panelStyle != null) return;
            var dark = new Texture2D(1, 1) { hideFlags = HideFlags.HideAndDontSave };
            dark.SetPixel(0, 0, new Color(0.12f, 0.14f, 0.2f, 0.78f));
            dark.Apply();
            var accent = new Texture2D(1, 1) { hideFlags = HideFlags.HideAndDontSave };
            accent.SetPixel(0, 0, new Color(0.25f, 0.75f, 0.55f, 0.92f));
            accent.Apply();

            panelStyle = new GUIStyle(GUI.skin.box) { padding = new RectOffset(14, 14, 10, 10) };
            panelStyle.normal.background = dark;
            titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold, richText = true };
            titleStyle.normal.textColor = new Color(1f, 0.85f, 0.4f);
            textStyle = new GUIStyle(GUI.skin.label) { fontSize = 16, richText = true, wordWrap = false };
            textStyle.normal.textColor = Color.white;
            hoverStyle = new GUIStyle(GUI.skin.box) { fontSize = 17, alignment = TextAnchor.MiddleLeft, padding = new RectOffset(10, 10, 4, 4) };
            hoverStyle.normal.background = dark;
            hoverStyle.normal.textColor = new Color(1f, 0.92f, 0.6f);
            messageStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 36, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true,
                padding = new RectOffset(24, 24, 14, 14)
            };
            messageStyle.normal.background = accent;
            messageStyle.normal.textColor = Color.white;
        }
    }
}
