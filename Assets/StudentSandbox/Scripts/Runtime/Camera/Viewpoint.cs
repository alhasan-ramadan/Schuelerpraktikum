using UnityEngine;

namespace StudentSandbox
{
    /// <summary>
    /// Ein Aussichtspunkt: Position und Blickrichtung einer Kamera.
    /// Die Play-Mode-Kamera fliegt mit den Tasten 1–5 dorthin,
    /// und das Workshop-Fenster richtet die Scene View danach aus.
    /// </summary>
    [AddComponentMenu("Student Sandbox/Viewpoint")]
    public class Viewpoint : MonoBehaviour
    {
        [SerializeField] string displayName = "Übersicht";
        [Tooltip("Abstand von der Kamera zum Punkt, auf den sie schaut (in Metern).")]
        [SerializeField, Min(1f)] float focusDistance = 25f;

        public string DisplayName
        {
            get => displayName;
            set => displayName = value;
        }

        public float FocusDistance
        {
            get => focusDistance;
            set => focusDistance = Mathf.Max(1f, value);
        }

        /// <summary>Der Punkt, auf den der Aussichtspunkt schaut.</summary>
        public Vector3 FocusPoint => transform.position + transform.forward * focusDistance;

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, 0.4f);
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2f);
        }
    }
}
