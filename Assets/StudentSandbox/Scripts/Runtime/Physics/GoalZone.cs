using UnityEngine;
using UnityEngine.Events;

namespace StudentSandbox
{
    /// <summary>
    /// Eine Zielzone mit einem Trigger-Collider (ein unsichtbarer Bereich, durch den man hindurchrollen kann).
    /// Wenn die richtige Kugel hineinrollt, wird das Ziel grün, ein Licht geht an und eine Meldung erscheint.
    /// Nur Objekte mit einer <see cref="PhysicsBall"/>-Komponente zählen.
    /// </summary>
    [AddComponentMenu("Student Sandbox/Goal Zone")]
    [RequireComponent(typeof(Collider))]
    [DisallowMultipleComponent]
    public class GoalZone : MonoBehaviour, IResettable
    {
        [Header("Welche Kugel zählt?")]
        [Tooltip("Leer lassen = jede PhysicsBall zählt. Sonst nur genau diese Kugel.")]
        [SerializeField] PhysicsBall requiredBall;

        [Header("Rückmeldung")]
        [Tooltip("Dieses Objekt wechselt seine Farbe (rot = wartet, grün = geschafft).")]
        [SerializeField] Renderer indicator;
        [SerializeField] Material idleMaterial;
        [SerializeField] Material reachedMaterial;
        [Tooltip("Dieses Licht geht an, wenn das Ziel erreicht ist.")]
        [SerializeField] Light successLight;
        [SerializeField] string successMessage = "Ziel erreicht!";

        [Header("Event (für eigene Ideen, z.B. ein Feuerwerk)")]
        [SerializeField] UnityEvent onGoalReached = new UnityEvent();

        [Header("Status (nur ansehen)")]
        [SerializeField] bool goalReached;

        public bool IsReached => goalReached;
        public UnityEvent OnGoalReached => onGoalReached;

        public PhysicsBall RequiredBall
        {
            get => requiredBall;
            set => requiredBall = value;
        }

        public string SuccessMessage
        {
            get => successMessage;
            set => successMessage = value;
        }

        /// <summary>Wird vom Generator benutzt, um die Rückmeldung zu verbinden.</summary>
        public void Setup(Renderer indicatorRenderer, Material idle, Material reached, Light light)
        {
            indicator = indicatorRenderer;
            idleMaterial = idle;
            reachedMaterial = reached;
            successLight = light;
            ApplyVisuals();
        }

        void Reset()
        {
            var zoneCollider = GetComponent<Collider>();
            if (zoneCollider != null) zoneCollider.isTrigger = true;
        }

        void Start()
        {
            ApplyVisuals();
        }

        void OnTriggerEnter(Collider other)
        {
            if (!isActiveAndEnabled) return;
            PhysicsBall ball = other.attachedRigidbody != null
                ? other.attachedRigidbody.GetComponent<PhysicsBall>()
                : other.GetComponentInParent<PhysicsBall>();
            if (ball == null) return;
            if (requiredBall != null && ball != requiredBall) return;
            if (goalReached) return;

            goalReached = true;
            ApplyVisuals();
            SandboxHUD.ShowMessage(successMessage);
            onGoalReached.Invoke();
        }

        public void ResetToStart()
        {
            goalReached = false;
            ApplyVisuals();
        }

        void ApplyVisuals()
        {
            if (indicator != null)
            {
                Material material = goalReached ? reachedMaterial : idleMaterial;
                if (material != null) indicator.sharedMaterial = material;
            }
            if (successLight != null) successLight.enabled = goalReached;
        }
    }
}
