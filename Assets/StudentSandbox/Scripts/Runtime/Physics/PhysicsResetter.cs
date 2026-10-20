using UnityEngine;

namespace StudentSandbox
{
    /// <summary>
    /// Setzt alle Physik-Experimente zurück: Kugeln, Dominosteine, Hammer und Zielzonen.
    /// Funktioniert im Play Mode über den RESET-Knopf oder die Taste R.
    /// Es werden nur Dinge zurückgesetzt, die im Play Mode passiert sind –
    /// Änderungen aus dem Editor bleiben erhalten.
    /// </summary>
    [AddComponentMenu("Student Sandbox/Physics Resetter")]
    [DisallowMultipleComponent]
    public class PhysicsResetter : MonoBehaviour
    {
        [Tooltip("Taste R setzt alles zurück.")]
        [SerializeField] bool allowKeyboardReset = true;
        [SerializeField] string resetMessage = "Zurückgesetzt!";

        /// <summary>Wie viele Objekte beim letzten Reset zurückgesetzt wurden (für Tests).</summary>
        public int LastResetCount { get; private set; }

        void Update()
        {
            if (allowKeyboardReset && SandboxInput.GetKeyDown(SandboxKey.R)) ResetAll();
        }

        /// <summary>Findet alle Komponenten mit <see cref="IResettable"/> und setzt sie zurück.</summary>
        public void ResetAll()
        {
            int count = 0;
            MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is IResettable resettable)
                {
                    resettable.ResetToStart();
                    count++;
                }
            }
            LastResetCount = count;
            SandboxHUD.ShowMessage(resetMessage, 1.5f);
        }
    }
}
