using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace StudentSandbox
{
    public enum TimeOfDay
    {
        Day,
        Sunset,
        Night
    }

    /// <summary>Alle Licht-Einstellungen für eine Tageszeit.</summary>
    [Serializable]
    public class LightingPreset
    {
        public string displayName = "Tag";

        [Header("Sonne")]
        [Tooltip("Drehung der Sonne. X = Höhe am Himmel (90 = Mittag, 5 = kurz vor dem Untergang), Y = Himmelsrichtung.")]
        public Vector3 sunRotation = new Vector3(50f, 30f, 0f);
        public Color sunColor = Color.white;
        [Range(0f, 3f)] public float sunIntensity = 1.2f;

        [Header("Umgebungslicht (von oben, von der Seite, von unten)")]
        public Color skyColor = new Color(0.6f, 0.7f, 0.85f);
        public Color equatorColor = new Color(0.55f, 0.58f, 0.6f);
        public Color groundColor = new Color(0.35f, 0.33f, 0.3f);

        [Header("Himmel und Nebel")]
        public Material skybox;
        public Color fogColor = new Color(0.7f, 0.82f, 0.95f);

        [Header("Lampen")]
        [Tooltip("Haken = Straßenlaternen und Fenster leuchten.")]
        public bool lampsOn;
    }

    /// <summary>
    /// Wechselt zwischen Tag, Sonnenuntergang und Nacht.
    /// Dabei ändern sich: Sonne (Drehung, Farbe, Helligkeit), Umgebungslicht, Himmel, Nebelfarbe und Lampen.
    /// Im Editor gibt es dafür Knöpfe im Inspector und im Workshop-Fenster.
    /// </summary>
    [AddComponentMenu("Student Sandbox/Day Night Controller")]
    [DisallowMultipleComponent]
    public class DayNightController : MonoBehaviour
    {
        [Header("Sonne (Directional Light)")]
        [SerializeField] Light sun;

        [Header("Aktuelle Tageszeit")]
        [SerializeField] TimeOfDay timeOfDay = TimeOfDay.Day;

        [Header("Einstellungen je Tageszeit")]
        [SerializeField] LightingPreset day = new LightingPreset();
        [SerializeField] LightingPreset sunset = new LightingPreset { displayName = "Sonnenuntergang" };
        [SerializeField] LightingPreset night = new LightingPreset { displayName = "Nacht" };

        [Header("Optionen")]
        [Tooltip("Haken = die Nebelfarbe passt sich der Tageszeit an.")]
        [SerializeField] bool controlFog = true;
        [Tooltip("Haken = beim Drücken von Play wird die Tageszeit neu angewendet (überschreibt Änderungen an der Sonne). Ohne Haken bleibt alles so, wie du es im Editor eingestellt hast.")]
        [SerializeField] bool applyOnStart;
        [Tooltip("Haken = Taste T wechselt im Play Mode die Tageszeit.")]
        [SerializeField] bool allowKeyboardSwitch = true;

        [Header("Event (für eigene Ideen)")]
        [SerializeField] UnityEvent onTimeChanged = new UnityEvent();

        public TimeOfDay CurrentTime => timeOfDay;
        public UnityEvent OnTimeChanged => onTimeChanged;

        public Light Sun
        {
            get => sun;
            set => sun = value;
        }

        public LightingPreset Day => day;
        public LightingPreset Sunset => sunset;
        public LightingPreset Night => night;

        /// <summary>Wird vom Generator benutzt.</summary>
        public void Setup(Light sunLight, LightingPreset dayPreset, LightingPreset sunsetPreset, LightingPreset nightPreset)
        {
            sun = sunLight;
            day = dayPreset;
            sunset = sunsetPreset;
            night = nightPreset;
        }

        void Start()
        {
            if (applyOnStart) Apply(timeOfDay);
        }

        void Update()
        {
            if (allowKeyboardSwitch && SandboxInput.GetKeyDown(SandboxKey.T))
            {
                NextTimeOfDay();
                SandboxHUD.ShowMessage(GetPreset(timeOfDay).displayName, 1.5f);
            }
        }

        public void SetDay() => Apply(TimeOfDay.Day);
        public void SetSunset() => Apply(TimeOfDay.Sunset);
        public void SetNight() => Apply(TimeOfDay.Night);

        /// <summary>Tag → Sonnenuntergang → Nacht → Tag …</summary>
        public void NextTimeOfDay()
        {
            Apply((TimeOfDay)(((int)timeOfDay + 1) % 3));
        }

        public LightingPreset GetPreset(TimeOfDay time)
        {
            switch (time)
            {
                case TimeOfDay.Sunset: return sunset;
                case TimeOfDay.Night: return night;
                default: return day;
            }
        }

        /// <summary>Wendet eine Tageszeit auf Sonne, Umgebungslicht, Himmel, Nebel und Lampen an.</summary>
        public void Apply(TimeOfDay time)
        {
            timeOfDay = time;
            LightingPreset preset = GetPreset(time);
            if (preset == null) return;

            if (sun != null)
            {
                sun.transform.rotation = Quaternion.Euler(preset.sunRotation);
                sun.color = preset.sunColor;
                sun.intensity = preset.sunIntensity;
                RenderSettings.sun = sun;
            }

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = preset.skyColor;
            RenderSettings.ambientEquatorColor = preset.equatorColor;
            RenderSettings.ambientGroundColor = preset.groundColor;
            if (preset.skybox != null) RenderSettings.skybox = preset.skybox;
            if (controlFog) RenderSettings.fogColor = preset.fogColor;

            foreach (LampController lamp in FindObjectsByType<LampController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (lamp.FollowDayNight) lamp.SetOn(preset.lampsOn);
            }

            DynamicGI.UpdateEnvironment();
            onTimeChanged.Invoke();
        }
    }
}
