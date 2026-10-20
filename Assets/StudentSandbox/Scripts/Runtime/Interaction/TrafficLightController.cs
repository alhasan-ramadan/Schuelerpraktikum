using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace StudentSandbox
{
    /// <summary>Eine Phase der Ampel: welche Lampen leuchten und wie lange.</summary>
    [Serializable]
    public class TrafficLightPhase
    {
        [Tooltip("Name der Phase, z.B. Rot oder Grün.")]
        public string name = "Rot";
        public bool red;
        public bool yellow;
        public bool green;
        [Tooltip("Wie lange die Phase dauert, wenn die Ampel automatisch schaltet (Sekunden).")]
        [Min(0.2f)] public float duration = 3f;

        public TrafficLightPhase() { }

        public TrafficLightPhase(string name, bool red, bool yellow, bool green, float duration)
        {
            this.name = name;
            this.red = red;
            this.yellow = yellow;
            this.green = green;
            this.duration = duration;
        }
    }

    /// <summary>
    /// Eine einfache Zustandsmaschine: Die Ampel ist immer in genau einer Phase.
    /// <see cref="NextPhase"/> schaltet zur nächsten Phase in der Liste weiter.
    /// Die Reihenfolge der Phasen kann im Inspector verändert werden.
    /// </summary>
    [AddComponentMenu("Student Sandbox/Traffic Light Controller")]
    [DisallowMultipleComponent]
    public class TrafficLightController : MonoBehaviour
    {
        [Header("Leuchtkörper (eigene GameObjects)")]
        [SerializeField] Renderer redLamp;
        [SerializeField] Renderer yellowLamp;
        [SerializeField] Renderer greenLamp;

        [Header("Materialien: an / aus")]
        [SerializeField] Material redOn;
        [SerializeField] Material redOff;
        [SerializeField] Material yellowOn;
        [SerializeField] Material yellowOff;
        [SerializeField] Material greenOn;
        [SerializeField] Material greenOff;

        [Header("Ablauf (Reihenfolge darf verändert werden)")]
        [SerializeField] List<TrafficLightPhase> phases = CreateDefaultPhases();
        [Tooltip("Haken = die Ampel schaltet von allein weiter.")]
        [SerializeField] bool autoCycle;
        [Tooltip("In welcher Phase die Ampel gerade ist (0 = erste Phase in der Liste).")]
        [SerializeField, Min(0)] int currentPhase;

        [Header("Event (für eigene Ideen)")]
        [SerializeField] UnityEvent onPhaseChanged = new UnityEvent();

        float timer;
        int appliedPhase = -1;

        public List<TrafficLightPhase> Phases => phases;
        public int CurrentPhaseIndex => currentPhase;
        public TrafficLightPhase CurrentPhase => phases != null && phases.Count > 0 ? phases[Mathf.Clamp(currentPhase, 0, phases.Count - 1)] : null;
        public string CurrentPhaseName => CurrentPhase != null ? CurrentPhase.name : "-";
        public UnityEvent OnPhaseChanged => onPhaseChanged;

        public bool AutoCycle
        {
            get => autoCycle;
            set
            {
                autoCycle = value;
                timer = 0f;
            }
        }

        public bool IsRed => CurrentPhase != null && CurrentPhase.red;
        public bool IsYellow => CurrentPhase != null && CurrentPhase.yellow;
        public bool IsGreen => CurrentPhase != null && CurrentPhase.green;

        /// <summary>Die deutsche Standard-Ampel: Rot, Rot-Gelb, Grün, Gelb.</summary>
        public static List<TrafficLightPhase> CreateDefaultPhases()
        {
            return new List<TrafficLightPhase>
            {
                new TrafficLightPhase("Rot", true, false, false, 4f),
                new TrafficLightPhase("Rot-Gelb", true, true, false, 1.5f),
                new TrafficLightPhase("Grün", false, false, true, 4f),
                new TrafficLightPhase("Gelb", false, true, false, 1.5f),
            };
        }

        /// <summary>Wird vom Generator benutzt, um Lampen und Materialien zu verbinden.</summary>
        public void Setup(Renderer red, Renderer yellow, Renderer green,
            Material redOnMat, Material redOffMat, Material yellowOnMat, Material yellowOffMat, Material greenOnMat, Material greenOffMat)
        {
            redLamp = red;
            yellowLamp = yellow;
            greenLamp = green;
            redOn = redOnMat;
            redOff = redOffMat;
            yellowOn = yellowOnMat;
            yellowOff = yellowOffMat;
            greenOn = greenOnMat;
            greenOff = greenOffMat;
            ApplyPhase();
        }

        void Start()
        {
            timer = 0f;
            ApplyPhase();
        }

        void Update()
        {
            // Wurde "Current Phase" im Inspector geändert? Dann sofort die Lampen anpassen.
            if (appliedPhase != currentPhase) ApplyPhase();
            if (!autoCycle || phases == null || phases.Count == 0) return;
            timer += Time.deltaTime;
            if (timer >= Mathf.Max(0.2f, CurrentPhase.duration)) NextPhase();
        }

        /// <summary>Schaltet zur nächsten Phase. Nach der letzten Phase geht es wieder von vorne los.</summary>
        public void NextPhase()
        {
            if (phases == null || phases.Count == 0) return;
            SetPhase((currentPhase + 1) % phases.Count);
        }

        public void SetPhase(int index)
        {
            if (phases == null || phases.Count == 0) return;
            currentPhase = Mathf.Clamp(index, 0, phases.Count - 1);
            timer = 0f;
            ApplyPhase();
            onPhaseChanged.Invoke();
        }

        /// <summary>Schaltet den Automatik-Betrieb ein oder aus.</summary>
        public void ToggleAutoCycle()
        {
            AutoCycle = !autoCycle;
            SandboxHUD.ShowMessage(autoCycle ? "Ampel: Automatik AN" : "Ampel: Automatik AUS", 1.5f);
        }

        /// <summary>Setzt die Lampen passend zur aktuellen Phase.</summary>
        public void ApplyPhase()
        {
            TrafficLightPhase phase = CurrentPhase;
            if (phase == null) return;
            appliedPhase = currentPhase;
            SetLamp(redLamp, phase.red, redOn, redOff);
            SetLamp(yellowLamp, phase.yellow, yellowOn, yellowOff);
            SetLamp(greenLamp, phase.green, greenOn, greenOff);
        }

        static void SetLamp(Renderer lamp, bool on, Material onMaterial, Material offMaterial)
        {
            if (lamp == null) return;
            Material material = on ? onMaterial : offMaterial;
            if (material != null) lamp.sharedMaterial = material;
        }

        void OnValidate()
        {
            if (phases != null && phases.Count > 0) currentPhase = Mathf.Clamp(currentPhase, 0, phases.Count - 1);
            else currentPhase = 0;
#if UNITY_EDITOR
            // Änderungen im Inspector (Phase, Häkchen) sofort an den Lampen zeigen.
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null) ApplyPhase();
            };
#endif
        }
    }
}
