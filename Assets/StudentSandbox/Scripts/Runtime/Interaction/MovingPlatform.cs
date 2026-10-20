using UnityEngine;

namespace StudentSandbox
{
    /// <summary>
    /// Fährt zwischen Punkt A und Punkt B hin und her.
    /// Die beiden Punkte sind eigene GameObjects – verschiebe sie, um die Strecke zu ändern.
    /// Die Plattform startet im Play Mode immer bei Punkt A.
    /// </summary>
    [AddComponentMenu("Student Sandbox/Moving Platform")]
    [DisallowMultipleComponent]
    public class MovingPlatform : MonoBehaviour
    {
        [Header("Start- und Endpunkt")]
        [SerializeField] Transform pointA;
        [SerializeField] Transform pointB;

        [Header("Bewegung")]
        [Tooltip("Geschwindigkeit in Metern pro Sekunde.")]
        [SerializeField, Range(0.1f, 10f)] float speed = 2f;
        [Tooltip("Wie lange die Plattform an jedem Endpunkt wartet (Sekunden).")]
        [SerializeField, Range(0f, 5f)] float waitTime = 1f;
        [Tooltip("Haken weg = die Plattform bleibt stehen.")]
        [SerializeField] bool isMoving = true;

        Rigidbody body;
        float t;
        int direction = 1;
        float waitTimer;

        public Transform PointA
        {
            get => pointA;
            set => pointA = value;
        }

        public Transform PointB
        {
            get => pointB;
            set => pointB = value;
        }

        public float Speed
        {
            get => speed;
            set => speed = Mathf.Max(0.01f, value);
        }

        public float WaitTime
        {
            get => waitTime;
            set => waitTime = Mathf.Max(0f, value);
        }

        public bool IsMoving
        {
            get => isMoving;
            set => isMoving = value;
        }

        /// <summary>0 = bei Punkt A, 1 = bei Punkt B.</summary>
        public float PathPosition => t;

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            if (body != null)
            {
                // Eine kinematische Plattform wird vom Skript bewegt, nicht von der Schwerkraft.
                body.isKinematic = true;
                body.interpolation = RigidbodyInterpolation.Interpolate;
            }
        }

        void Start()
        {
            t = 0f;
            direction = 1;
            if (pointA != null) MoveTo(pointA.position, true);
        }

        void Update()
        {
            if (body == null) Step(Time.deltaTime);
        }

        void FixedUpdate()
        {
            if (body != null) Step(Time.fixedDeltaTime);
        }

        void Step(float deltaTime)
        {
            if (!isMoving || pointA == null || pointB == null) return;
            if (waitTimer > 0f)
            {
                waitTimer -= deltaTime;
                return;
            }

            float distance = Vector3.Distance(pointA.position, pointB.position);
            if (distance < 0.001f) return;

            t += direction * speed * deltaTime / distance;
            if (t >= 1f)
            {
                t = 1f;
                direction = -1;
                waitTimer = waitTime;
            }
            else if (t <= 0f)
            {
                t = 0f;
                direction = 1;
                waitTimer = waitTime;
            }

            float eased = Mathf.SmoothStep(0f, 1f, t);
            MoveTo(Vector3.Lerp(pointA.position, pointB.position, eased), false);
        }

        void MoveTo(Vector3 position, bool teleport)
        {
            if (body != null && !teleport) body.MovePosition(position);
            else
            {
                transform.position = position;
                if (body != null) body.position = position;
            }
        }

        /// <summary>Bewegung an- oder ausschalten (für Schalter).</summary>
        public void ToggleMoving()
        {
            isMoving = !isMoving;
            SandboxHUD.ShowMessage(isMoving ? "Plattform fährt" : "Plattform stoppt", 1.5f);
        }

        void OnDrawGizmos()
        {
            if (pointA == null || pointB == null) return;
            Gizmos.color = new Color(0.2f, 0.9f, 0.9f, 0.9f);
            Gizmos.DrawLine(pointA.position, pointB.position);
            Gizmos.DrawWireSphere(pointA.position, 0.3f);
            Gizmos.DrawWireSphere(pointB.position, 0.3f);
        }
    }
}
