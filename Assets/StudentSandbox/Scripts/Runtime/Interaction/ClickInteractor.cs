using UnityEngine;

namespace StudentSandbox
{
    /// <summary>
    /// Sitzt auf der Kamera. Schaut jeden Frame, welches Objekt unter dem Mauszeiger liegt,
    /// und ruft bei einem Linksklick <see cref="IClickable.OnClick"/> auf.
    /// So wird aus einer Benutzeraktion (Klick) ein Ereignis (Event) im Spiel.
    /// </summary>
    [AddComponentMenu("Student Sandbox/Click Interactor")]
    [RequireComponent(typeof(Camera))]
    [DisallowMultipleComponent]
    public class ClickInteractor : MonoBehaviour
    {
        [Tooltip("Wie weit (in Metern) die Maus Objekte erreichen kann.")]
        [SerializeField, Range(10f, 500f)] float maxDistance = 250f;
        [Tooltip("Welche Layer anklickbar sind. Standard: alle.")]
        [SerializeField] LayerMask clickableLayers = ~0;

        /// <summary>Text des Objekts unter dem Mauszeiger (oder null). Wird vom HUD angezeigt.</summary>
        public static string CurrentHoverText { get; private set; }

        Camera cam;
        IHoverable hovered;

        void Awake()
        {
            cam = GetComponent<Camera>();
        }

        void OnDisable()
        {
            SetHovered(null);
        }

        void Update()
        {
            if (!SandboxInput.HasMouse || cam == null)
            {
                SetHovered(null);
                return;
            }

            // Beim Umschauen (rechte Maustaste) wird nichts angeklickt.
            if (SandboxInput.RightMouseHeld)
            {
                SetHovered(null);
                return;
            }

            IClickable clickable = null;
            IHoverable hoverable = null;
            Ray ray = cam.ScreenPointToRay(SandboxInput.MousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, clickableLayers, QueryTriggerInteraction.Ignore))
            {
                clickable = hit.collider.GetComponentInParent<IClickable>();
                hoverable = hit.collider.GetComponentInParent<IHoverable>();
                // Ausgeschaltete Komponenten (Haken weg) reagieren nicht.
                if (clickable is Behaviour clickBehaviour && !clickBehaviour.isActiveAndEnabled) clickable = null;
                if (hoverable is Behaviour hoverBehaviour && !hoverBehaviour.isActiveAndEnabled) hoverable = null;
            }

            SetHovered(hoverable);

            if (clickable != null && SandboxInput.LeftClickDown)
            {
                clickable.OnClick();
            }
        }

        void SetHovered(IHoverable next)
        {
            if (ReferenceEquals(next, hovered))
            {
                if (hovered != null) CurrentHoverText = hovered.HoverText;
                return;
            }

            // Unity-Objekte können zerstört sein, deshalb vorsichtig prüfen.
            bool oldIsDestroyed = hovered is Object oldObject && oldObject == null;
            if (hovered != null && !oldIsDestroyed) hovered.SetHovered(false);
            hovered = next;
            if (hovered != null)
            {
                hovered.SetHovered(true);
                CurrentHoverText = hovered.HoverText;
            }
            else
            {
                CurrentHoverText = null;
            }
        }
    }
}
