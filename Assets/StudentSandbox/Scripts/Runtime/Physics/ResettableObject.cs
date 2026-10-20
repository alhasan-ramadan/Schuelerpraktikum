using UnityEngine;

namespace StudentSandbox
{
    /// <summary>
    /// Merkt sich beim Start des Play Mode, wo das Objekt steht.
    /// Der Reset-Knopf (Taste R) bringt es genau dorthin zurück und stoppt jede Bewegung.
    /// Weil die Startposition erst beim Drücken von Play gespeichert wird,
    /// bleiben deine Änderungen aus dem Editor erhalten.
    /// </summary>
    [AddComponentMenu("Student Sandbox/Resettable Object")]
    public class ResettableObject : MonoBehaviour, IResettable
    {
        Vector3 startPosition;
        Quaternion startRotation;
        bool hasStartPose;
        Rigidbody body;

        public bool HasStartPose => hasStartPose;
        public Vector3 StartPosition => startPosition;
        public Quaternion StartRotation => startRotation;

        protected virtual void Awake()
        {
            body = GetComponent<Rigidbody>();
            CaptureStartPose();
        }

        /// <summary>Speichert die aktuelle Position als neue Startposition.</summary>
        public void CaptureStartPose()
        {
            startPosition = transform.position;
            startRotation = transform.rotation;
            hasStartPose = true;
        }

        /// <summary>Zurück an den Start und alle Geschwindigkeiten auf null.</summary>
        public virtual void ResetToStart()
        {
            if (!hasStartPose) return;
            if (body == null) body = GetComponent<Rigidbody>();

            if (body != null && !body.isKinematic)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }

            transform.SetPositionAndRotation(startPosition, startRotation);

            if (body != null)
            {
                body.position = startPosition;
                body.rotation = startRotation;
                if (!body.isKinematic)
                {
                    body.linearVelocity = Vector3.zero;
                    body.angularVelocity = Vector3.zero;
                    body.WakeUp();
                }
            }
        }
    }
}
