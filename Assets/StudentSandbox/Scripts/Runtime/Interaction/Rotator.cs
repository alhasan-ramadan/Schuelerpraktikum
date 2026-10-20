using UnityEngine;

namespace StudentSandbox
{
    /// <summary>
    /// Dreht ein Objekt gleichmäßig. Probier im Inspector: Speed verdoppeln, die Achse ändern
    /// oder den Haken bei "Is Rotating" entfernen.
    /// </summary>
    [AddComponentMenu("Student Sandbox/Rotator")]
    public class Rotator : MonoBehaviour
    {
        [Tooltip("Drehgeschwindigkeit in Grad pro Sekunde. 360 = eine ganze Umdrehung pro Sekunde.")]
        [SerializeField, Range(0f, 720f)] float speed = 45f;
        [Tooltip("Um welche Achse sich das Objekt dreht. (0,1,0) = wie ein Karussell, (1,0,0) = wie ein Rad.")]
        [SerializeField] Vector3 rotationAxis = Vector3.up;
        [Tooltip("Haken weg = die Drehung stoppt.")]
        [SerializeField] bool isRotating = true;

        public float Speed
        {
            get => speed;
            set => speed = Mathf.Max(0f, value);
        }

        public Vector3 RotationAxis
        {
            get => rotationAxis;
            set => rotationAxis = value;
        }

        public bool IsRotating
        {
            get => isRotating;
            set => isRotating = value;
        }

        void Update()
        {
            if (!isRotating || speed <= 0f || rotationAxis.sqrMagnitude < 0.0001f) return;
            transform.Rotate(rotationAxis.normalized, speed * Time.deltaTime, Space.Self);
        }

        /// <summary>Drehung an- oder ausschalten (für Schalter).</summary>
        public void ToggleRotation()
        {
            isRotating = !isRotating;
            SandboxHUD.ShowMessage(isRotating ? "Drehung: AN" : "Drehung: AUS", 1.5f);
        }

        public void StartRotating() => isRotating = true;
        public void StopRotating() => isRotating = false;

        /// <summary>Verdoppelt die Geschwindigkeit (höchstens 720 Grad pro Sekunde).</summary>
        public void DoubleSpeed()
        {
            speed = Mathf.Min(720f, speed * 2f);
        }
    }
}
