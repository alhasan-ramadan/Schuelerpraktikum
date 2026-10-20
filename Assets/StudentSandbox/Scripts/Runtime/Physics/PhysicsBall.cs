using UnityEngine;

namespace StudentSandbox
{
    /// <summary>
    /// Markiert eine Kugel für die Physik-Experimente.
    /// Die eigentliche Physik macht der Rigidbody: Er sorgt dafür, dass die Kugel fällt und rollt.
    /// Dieses Skript kann die Kugel zurücksetzen und holt sie zurück, wenn sie ins Wasser fällt.
    /// </summary>
    [AddComponentMenu("Student Sandbox/Physics Ball")]
    [RequireComponent(typeof(Rigidbody))]
    [DisallowMultipleComponent]
    public class PhysicsBall : ResettableObject
    {
        [Header("Kugel")]
        [Tooltip("Name der Kugel für Meldungen, z.B. \"Rote Kugel\".")]
        [SerializeField] string displayName = "Kugel";

        [Header("Sicherheitsnetz")]
        [Tooltip("Fällt die Kugel tiefer als die Grenze unten (z.B. ins Wasser), kommt sie automatisch zurück an den Start.")]
        [SerializeField] bool autoResetWhenFallen = true;
        [SerializeField] float fallLimitY = -5f;

        public string DisplayName
        {
            get => displayName;
            set => displayName = value;
        }

        void FixedUpdate()
        {
            if (autoResetWhenFallen && transform.position.y < fallLimitY)
            {
                ResetToStart();
                SandboxHUD.ShowMessage(displayName + " ist zurück am Start", 2f);
            }
        }
    }
}
