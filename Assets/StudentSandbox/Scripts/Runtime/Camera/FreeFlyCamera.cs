using UnityEngine;

namespace StudentSandbox
{
    /// <summary>
    /// Eine einfache Flug-Kamera für den Play Mode.
    /// WASD = fliegen, Q/E = runter/hoch, Shift = schneller,
    /// rechte Maustaste halten = umschauen, Mausrad = Tempo, 1–5 = Aussichtspunkte, Esc = Maus freigeben.
    /// </summary>
    [AddComponentMenu("Student Sandbox/Free Fly Camera")]
    [DisallowMultipleComponent]
    public class FreeFlyCamera : MonoBehaviour
    {
        [Header("Bewegung")]
        [Tooltip("Fluggeschwindigkeit in Metern pro Sekunde.")]
        [SerializeField, Range(1f, 40f)] float moveSpeed = 10f;
        [Tooltip("Mit gedrückter Shift-Taste fliegt die Kamera so viel schneller.")]
        [SerializeField, Range(1f, 5f)] float fastMultiplier = 3f;
        [Tooltip("Die Kamera fliegt nie tiefer als diese Höhe (damit sie nicht im Boden verschwindet).")]
        [SerializeField] float minHeight = 0.5f;

        [Header("Umschauen (rechte Maustaste gedrückt halten)")]
        [SerializeField, Range(0.02f, 1f)] float lookSensitivity = 0.15f;
        [SerializeField] bool invertY;

        [Header("Start")]
        [Tooltip("Nur im Unity-Editor: Haken = beim Drücken von Play startet die Kamera dort, wohin die Scene View gerade schaut.")]
        [SerializeField] bool startAtSceneView = true;

        [Header("Aussichtspunkte (Tasten 1 bis 5)")]
        [SerializeField] Viewpoint[] viewpoints = new Viewpoint[0];
        [Tooltip("Wie lange der Flug zu einem Aussichtspunkt dauert (Sekunden).")]
        [SerializeField, Range(0.1f, 3f)] float flyDuration = 0.8f;

        float yaw;
        float pitch;
        bool looking;
        bool flying;
        float flyTime;
        Vector3 flyFromPosition;
        Quaternion flyFromRotation;
        Vector3 flyToPosition;
        Quaternion flyToRotation;

        public Viewpoint[] Viewpoints
        {
            get => viewpoints;
            set => viewpoints = value ?? new Viewpoint[0];
        }

        public float MoveSpeed
        {
            get => moveSpeed;
            set => moveSpeed = Mathf.Clamp(value, 1f, 40f);
        }

        public bool IsFlying => flying;

        void Start()
        {
#if UNITY_EDITOR
            if (startAtSceneView)
            {
                UnityEditor.SceneView view = UnityEditor.SceneView.lastActiveSceneView;
                if (view != null && view.camera != null && !view.in2DMode)
                {
                    Transform viewTransform = view.camera.transform;
                    Vector3 position = viewTransform.position;
                    position.y = Mathf.Max(minHeight, position.y);
                    transform.SetPositionAndRotation(position, viewTransform.rotation);
                }
            }
#endif
            SyncAnglesFromTransform();
        }

        void OnDisable()
        {
            ReleaseCursor();
        }

        void Update()
        {
            float deltaTime = Time.unscaledDeltaTime;

            if (SandboxInput.GetKeyDown(SandboxKey.Escape)) ReleaseCursor();

            HandleViewpointKeys();
            if (flying)
            {
                UpdateFlight(deltaTime);
                return;
            }

            HandleLook();
            HandleMove(deltaTime);
        }

        void HandleViewpointKeys()
        {
            if (SandboxInput.GetKeyDown(SandboxKey.Digit1)) FlyTo(0);
            else if (SandboxInput.GetKeyDown(SandboxKey.Digit2)) FlyTo(1);
            else if (SandboxInput.GetKeyDown(SandboxKey.Digit3)) FlyTo(2);
            else if (SandboxInput.GetKeyDown(SandboxKey.Digit4)) FlyTo(3);
            else if (SandboxInput.GetKeyDown(SandboxKey.Digit5)) FlyTo(4);
        }

        void HandleLook()
        {
            if (SandboxInput.RightMouseHeld)
            {
                if (!looking)
                {
                    looking = true;
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }

                Vector2 delta = SandboxInput.MouseDelta * lookSensitivity;
                yaw += delta.x;
                pitch += invertY ? delta.y : -delta.y;
                pitch = Mathf.Clamp(pitch, -89f, 89f);
                transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
            }
            else if (looking)
            {
                ReleaseCursor();
            }
        }

        void HandleMove(float deltaTime)
        {
            float scroll = SandboxInput.ScrollSteps;
            if (scroll != 0f) MoveSpeed = moveSpeed * (scroll > 0f ? 1.15f : 1f / 1.15f);

            Vector3 move = Vector3.zero;
            if (SandboxInput.GetKey(SandboxKey.W)) move += transform.forward;
            if (SandboxInput.GetKey(SandboxKey.S)) move -= transform.forward;
            if (SandboxInput.GetKey(SandboxKey.D)) move += transform.right;
            if (SandboxInput.GetKey(SandboxKey.A)) move -= transform.right;
            if (SandboxInput.GetKey(SandboxKey.E)) move += Vector3.up;
            if (SandboxInput.GetKey(SandboxKey.Q)) move -= Vector3.up;
            if (move.sqrMagnitude < 0.0001f) return;

            float speed = moveSpeed * (SandboxInput.GetKey(SandboxKey.Shift) ? fastMultiplier : 1f);
            Vector3 position = transform.position + move.normalized * speed * deltaTime;
            position.y = Mathf.Max(minHeight, position.y);
            transform.position = position;
        }

        /// <summary>Fliegt sanft zum Aussichtspunkt mit dieser Nummer (0 = erster).</summary>
        public void FlyTo(int index)
        {
            if (viewpoints == null || index < 0 || index >= viewpoints.Length || viewpoints[index] == null) return;
            FlyTo(viewpoints[index]);
        }

        public void FlyTo(Viewpoint viewpoint)
        {
            if (viewpoint == null) return;
            flyFromPosition = transform.position;
            flyFromRotation = transform.rotation;
            flyToPosition = viewpoint.transform.position;
            flyToRotation = viewpoint.transform.rotation;
            flyTime = 0f;
            flying = true;
            SandboxHUD.ShowMessage(viewpoint.DisplayName, 1.5f);
        }

        void UpdateFlight(float deltaTime)
        {
            flyTime += deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(flyTime / flyDuration));
            transform.SetPositionAndRotation(
                Vector3.Lerp(flyFromPosition, flyToPosition, t),
                Quaternion.Slerp(flyFromRotation, flyToRotation, t));
            if (t >= 1f)
            {
                flying = false;
                SyncAnglesFromTransform();
            }
        }

        void SyncAnglesFromTransform()
        {
            Vector3 euler = transform.rotation.eulerAngles;
            yaw = euler.y;
            pitch = euler.x > 180f ? euler.x - 360f : euler.x;
        }

        void ReleaseCursor()
        {
            looking = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
