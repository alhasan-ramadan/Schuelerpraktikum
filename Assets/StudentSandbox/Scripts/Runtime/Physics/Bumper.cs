using UnityEngine;

namespace StudentSandbox
{
    /// <summary>
    /// Ein Flipper-Pilz: Stößt Kugeln, die ihn berühren, kräftig weg.
    /// Je größer "Bounce Strength", desto stärker der Stoß.
    /// </summary>
    [AddComponentMenu("Student Sandbox/Bumper")]
    public class Bumper : MonoBehaviour
    {
        [Tooltip("Wie stark der Bumper wegstößt (Meter pro Sekunde, die dazukommen).")]
        [SerializeField, Range(0f, 20f)] float bounceStrength = 5f;
        [Tooltip("Dieser Teil wird beim Treffer kurz größer.")]
        [SerializeField] Transform visual;
        [SerializeField, Range(1f, 1.6f)] float pulseScale = 1.25f;

        Vector3 visualBaseScale;
        float pulse;

        public float BounceStrength
        {
            get => bounceStrength;
            set => bounceStrength = Mathf.Max(0f, value);
        }

        public Transform Visual
        {
            get => visual;
            set => visual = value;
        }

        void Awake()
        {
            if (visual != null) visualBaseScale = visual.localScale;
        }

        void OnCollisionEnter(Collision collision)
        {
            // Haken bei der Komponente weg = Bumper aus (Unity ruft Kollisionen sonst trotzdem auf).
            if (!isActiveAndEnabled) return;
            Rigidbody other = collision.rigidbody;
            if (other == null || other.isKinematic) return;

            Vector3 direction = other.worldCenterOfMass - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f) direction = -collision.relativeVelocity;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f) return;

            other.AddForce(direction.normalized * bounceStrength, ForceMode.VelocityChange);
            pulse = 1f;
        }

        void Update()
        {
            if (visual == null || pulse <= 0f) return;
            pulse = Mathf.Max(0f, pulse - Time.deltaTime * 4f);
            visual.localScale = visualBaseScale * Mathf.Lerp(1f, pulseScale, pulse);
        }
    }
}
