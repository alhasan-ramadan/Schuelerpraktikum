using UnityEngine;

namespace StudentSandbox
{
    /// <summary>
    /// Ein Hammer, der nach unten schwingt und den ersten Dominostein umstößt.
    /// Der Hammer ist ein kinematischer Rigidbody: Er wird vom Skript bewegt,
    /// schiebt aber trotzdem andere Physik-Objekte an.
    /// </summary>
    [AddComponentMenu("Student Sandbox/Domino Starter")]
    [DisallowMultipleComponent]
    public class DominoStarter : MonoBehaviour, IResettable
    {
        [Header("Hammer")]
        [Tooltip("Der Drehpunkt des Hammers (mit kinematischem Rigidbody). Leer = dieses Objekt.")]
        [SerializeField] Rigidbody hammer;
        [Tooltip("Um welche (lokale) Achse der Hammer schwingt.")]
        [SerializeField] Vector3 swingAxis = Vector3.forward;
        [Tooltip("Wie weit der Hammer schwingt (Grad).")]
        [SerializeField, Range(10f, 170f)] float swingAngle = 100f;
        [Tooltip("Wie schnell der Hammer schwingt (Grad pro Sekunde).")]
        [SerializeField, Range(10f, 360f)] float swingSpeed = 90f;

        [Header("Start")]
        [Tooltip("Haken = der Hammer schwingt kurz nach dem Drücken von Play von allein. Nach einem Reset startet ihn der DOMINO-Knopf.")]
        [SerializeField] bool startAutomatically = true;
        [Tooltip("Wartezeit bis zum automatischen Start (Sekunden).")]
        [SerializeField, Range(0f, 10f)] float startDelay = 1.5f;

        Quaternion restRotation;
        float angle;
        bool swinging;
        bool armed;
        float delayTimer;
        bool initialized;

        public bool HasSwung => angle > 0.01f;
        public bool IsSwinging => swinging;
        public float CurrentAngle => angle;

        public bool StartAutomatically
        {
            get => startAutomatically;
            set => startAutomatically = value;
        }

        public float StartDelay
        {
            get => startDelay;
            set => startDelay = Mathf.Max(0f, value);
        }

        Rigidbody Hammer => hammer != null ? hammer : (hammer = GetComponent<Rigidbody>());

        void Awake()
        {
            Initialize();
        }

        void Initialize()
        {
            if (initialized) return;
            Rigidbody body = Hammer;
            if (body == null)
            {
                Debug.LogWarning("[Student Sandbox] DominoStarter braucht einen Rigidbody für den Hammer.", this);
                return;
            }
            body.isKinematic = true;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            restRotation = body.transform.rotation;
            initialized = true;
        }

        void Start()
        {
            ArmIfAutomatic();
        }

        void ArmIfAutomatic()
        {
            armed = startAutomatically;
            delayTimer = startDelay;
        }

        void FixedUpdate()
        {
            if (!initialized) return;

            if (armed)
            {
                delayTimer -= Time.fixedDeltaTime;
                if (delayTimer <= 0f)
                {
                    armed = false;
                    swinging = true;
                }
            }

            if (!swinging) return;

            angle = Mathf.MoveTowards(angle, swingAngle, swingSpeed * Time.fixedDeltaTime);
            Hammer.MoveRotation(restRotation * Quaternion.AngleAxis(angle, SafeAxis()));
            if (angle >= swingAngle) swinging = false;
        }

        /// <summary>Lässt den Hammer schwingen (für den Knopf "Domino starten").</summary>
        public void StartSwing()
        {
            Initialize();
            if (!initialized) return;
            if (HasSwung && !swinging)
            {
                SandboxHUD.ShowMessage("Erst RESET drücken (Taste R)", 2f);
                return;
            }
            armed = false;
            swinging = true;
        }

        public void ResetToStart()
        {
            Initialize();
            if (!initialized) return;
            angle = 0f;
            swinging = false;
            Rigidbody body = Hammer;
            body.transform.rotation = restRotation;
            body.rotation = restRotation;
            // Nach einem Reset wartet der Hammer auf den DOMINO-Knopf (der Automatik-Start gilt nur direkt nach Play).
            armed = false;
            delayTimer = 0f;
        }

        Vector3 SafeAxis()
        {
            return swingAxis.sqrMagnitude > 0.0001f ? swingAxis.normalized : Vector3.forward;
        }
    }
}
