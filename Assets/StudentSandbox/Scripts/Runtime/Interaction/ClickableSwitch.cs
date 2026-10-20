using UnityEngine;
using UnityEngine.Events;

namespace StudentSandbox
{
    /// <summary>
    /// Ein Knopf, den man im Play Mode anklicken kann.
    /// Was beim Klicken passiert, steht im Inspector unter "On Pressed" (ein UnityEvent).
    /// Dort kann man Objekte hineinziehen und eine Funktion auswählen, z.B. DoorController.Toggle.
    /// </summary>
    [AddComponentMenu("Student Sandbox/Clickable Switch")]
    [DisallowMultipleComponent]
    public class ClickableSwitch : MonoBehaviour, IClickable, IHoverable
    {
        [Header("Beschriftung")]
        [Tooltip("Text, der neben dem Mauszeiger erscheint, wenn man über den Knopf fährt.")]
        [SerializeField] string label = "Schalter";

        [Header("Was passiert beim Klicken? (Event)")]
        [Tooltip("Hier stehen die Aktionen, die beim Klicken ausgeführt werden.")]
        [SerializeField] UnityEvent onPressed = new UnityEvent();

        [Header("Aussehen")]
        [Tooltip("Der Teil des Knopfes, der beim Drücken nach unten fährt.")]
        [SerializeField] Transform buttonCap;
        [Tooltip("Wie tief der Knopf beim Drücken einsinkt (in Metern).")]
        [SerializeField, Range(0f, 0.3f)] float pressDepth = 0.06f;
        [Tooltip("Wie lange die Drück-Animation dauert (in Sekunden).")]
        [SerializeField, Range(0.05f, 1f)] float pressDuration = 0.25f;
        [Tooltip("Wie stark der Knopf wächst, wenn die Maus darüber ist.")]
        [SerializeField, Range(1f, 1.5f)] float hoverScale = 1.12f;

        [Header("Status (nur ansehen)")]
        [SerializeField] int pressCount;

        Vector3 capRestPosition;
        Vector3 capRestScale = Vector3.one;
        float pressTimer;
        bool hovered;
        bool capInitialized;

        public string Label
        {
            get => label;
            set => label = value;
        }

        public UnityEvent OnPressed => onPressed;
        public Transform ButtonCap
        {
            get => buttonCap;
            set
            {
                buttonCap = value;
                capInitialized = false;
            }
        }

        public int PressCount => pressCount;
        public string HoverText => label;

        void Awake()
        {
            InitCap();
        }

        void InitCap()
        {
            if (buttonCap == null || capInitialized) return;
            capRestPosition = buttonCap.localPosition;
            capRestScale = buttonCap.localScale;
            capInitialized = true;
        }

        /// <summary>Drückt den Knopf: Animation starten und das Event auslösen.</summary>
        public void Press()
        {
            if (!isActiveAndEnabled) return;
            pressCount++;
            pressTimer = pressDuration;
            onPressed.Invoke();
        }

        public void OnClick() => Press();

        public void SetHovered(bool isHovered)
        {
            hovered = isHovered;
        }

        void Update()
        {
            if (buttonCap == null) return;
            InitCap();

            bool animating = pressTimer > 0f;
            Vector3 targetScale = hovered ? capRestScale * hoverScale : capRestScale;
            if (!animating && buttonCap.localScale == targetScale && buttonCap.localPosition == capRestPosition) return;

            float offset = 0f;
            if (animating)
            {
                pressTimer -= Time.deltaTime;
                float t = 1f - Mathf.Clamp01(pressTimer / pressDuration);
                offset = Mathf.Sin(t * Mathf.PI) * pressDepth;
            }

            buttonCap.localPosition = capRestPosition + Vector3.down * offset;
            buttonCap.localScale = Vector3.MoveTowards(buttonCap.localScale, targetScale, Time.deltaTime * 2f);
        }
    }
}
