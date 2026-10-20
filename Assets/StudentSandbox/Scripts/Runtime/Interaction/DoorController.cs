using UnityEngine;
using UnityEngine.Events;

namespace StudentSandbox
{
    public enum DoorMotion
    {
        /// <summary>Die Tür fährt zur Seite (wie eine Schiebetür).</summary>
        Slide,
        /// <summary>Die Tür schwingt um ein Scharnier auf (wie eine Zimmertür).</summary>
        Rotate
    }

    /// <summary>
    /// Öffnet und schließt eine Tür sanft. Ein Schalter ruft <see cref="Toggle"/> auf.
    /// Die geschlossene Position ist die Position, die die Tür beim Start des Play Mode hat.
    /// </summary>
    [AddComponentMenu("Student Sandbox/Door Controller")]
    [DisallowMultipleComponent]
    public class DoorController : MonoBehaviour
    {
        [Header("Welche Tür bewegt sich?")]
        [Tooltip("Das Objekt, das sich bewegt. Leer lassen = dieses Objekt.")]
        [SerializeField] Transform door;

        [Header("Wie öffnet sich die Tür?")]
        [SerializeField] DoorMotion motion = DoorMotion.Slide;
        [Tooltip("Slide: So weit fährt die Tür zur Seite (in Metern, x = seitlich, y = nach oben).")]
        [SerializeField] Vector3 slideOffset = new Vector3(2.6f, 0f, 0f);
        [Tooltip("Rotate: So weit schwingt die Tür auf (in Grad).")]
        [SerializeField, Range(-180f, 180f)] float openAngle = 90f;
        [Tooltip("Rotate: Um welche Achse sich die Tür dreht. (0,1,0) = wie eine normale Tür.")]
        [SerializeField] Vector3 hingeAxis = Vector3.up;

        [Header("Geschwindigkeit")]
        [Tooltip("1 = die Tür braucht 1 Sekunde. 0.2 = 5 Sekunden (sehr langsam). 4 = blitzschnell.")]
        [SerializeField, Range(0.05f, 5f)] float openSpeed = 1f;

        [Header("Status")]
        [Tooltip("Soll die Tür beim Start schon offen sein?")]
        [SerializeField] bool startOpen;
        [Tooltip("Nur zum Ansehen: Ist die Tür gerade offen (oder öffnet sie sich)?")]
        [SerializeField] bool isOpen;

        [Header("Events (für eigene Ideen)")]
        [SerializeField] UnityEvent onOpened = new UnityEvent();
        [SerializeField] UnityEvent onClosed = new UnityEvent();

        Vector3 closedPosition;
        Quaternion closedRotation;
        float progress;
        float target;
        bool initialized;

        public Transform Door
        {
            get => door != null ? door : transform;
            set => door = value;
        }

        public DoorMotion Motion
        {
            get => motion;
            set => motion = value;
        }

        public Vector3 SlideOffset
        {
            get => slideOffset;
            set => slideOffset = value;
        }

        public float OpenAngle
        {
            get => openAngle;
            set => openAngle = value;
        }

        public float OpenSpeed
        {
            get => openSpeed;
            set => openSpeed = Mathf.Max(0.01f, value);
        }

        /// <summary>true, wenn die Tür offen ist oder gerade aufgeht.</summary>
        public bool IsOpen => target > 0.5f;
        /// <summary>0 = ganz zu, 1 = ganz offen.</summary>
        public float OpenAmount => progress;
        public bool IsMoving => !Mathf.Approximately(progress, target);
        public UnityEvent OnOpened => onOpened;
        public UnityEvent OnClosed => onClosed;

        void Awake()
        {
            Initialize();
        }

        void Initialize()
        {
            if (initialized) return;
            Transform t = Door;
            closedPosition = t.localPosition;
            closedRotation = t.localRotation;
            progress = target = startOpen ? 1f : 0f;
            isOpen = startOpen;
            initialized = true;
            ApplyPose();
        }

        public void Toggle()
        {
            if (IsOpen) Close();
            else Open();
        }

        public void Open()
        {
            Initialize();
            target = 1f;
            isOpen = true;
        }

        public void Close()
        {
            Initialize();
            target = 0f;
            isOpen = false;
        }

        void Update()
        {
            if (!initialized || Mathf.Approximately(progress, target)) return;

            progress = Mathf.MoveTowards(progress, target, openSpeed * Time.deltaTime);
            ApplyPose();

            if (Mathf.Approximately(progress, target))
            {
                if (target > 0.5f) onOpened.Invoke();
                else onClosed.Invoke();
            }
        }

        void ApplyPose()
        {
            Transform t = Door;
            float eased = Mathf.SmoothStep(0f, 1f, progress);
            if (motion == DoorMotion.Slide)
            {
                t.localPosition = closedPosition + slideOffset * eased;
                t.localRotation = closedRotation;
            }
            else
            {
                Vector3 axis = hingeAxis.sqrMagnitude > 0.0001f ? hingeAxis.normalized : Vector3.up;
                t.localPosition = closedPosition;
                t.localRotation = closedRotation * Quaternion.AngleAxis(openAngle * eased, axis);
            }
        }
    }
}
