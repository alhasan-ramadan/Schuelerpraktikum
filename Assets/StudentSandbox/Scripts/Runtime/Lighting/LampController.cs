using UnityEngine;

namespace StudentSandbox
{
    /// <summary>
    /// Schaltet eine Lampe an oder aus: das Licht (Light-Komponente) und die leuchtenden Teile.
    /// Mit "Follow Day Night" geht die Lampe bei Sonnenuntergang und Nacht automatisch an.
    /// </summary>
    [AddComponentMenu("Student Sandbox/Lamp Controller")]
    [DisallowMultipleComponent]
    public class LampController : MonoBehaviour
    {
        [Header("Licht")]
        [Tooltip("Die Lichtquelle dieser Lampe. Farbe und Helligkeit stellst du direkt an der Light-Komponente ein.")]
        [SerializeField] Light lampLight;

        [Header("Leuchtende Teile")]
        [Tooltip("Diese Teile bekommen das 'An'- oder 'Aus'-Material.")]
        [SerializeField] Renderer[] glowRenderers = new Renderer[0];
        [SerializeField] Material onMaterial;
        [SerializeField] Material offMaterial;

        [Header("Schalten")]
        [Tooltip("Haken = die Lampe leuchtet.")]
        [SerializeField] bool isOn;
        [Tooltip("Haken = die Lampe geht bei Sonnenuntergang und Nacht automatisch an.")]
        [SerializeField] bool followDayNight = true;

        public bool IsOn => isOn;
        public Light LampLight => lampLight;

        public bool FollowDayNight
        {
            get => followDayNight;
            set => followDayNight = value;
        }

        /// <summary>Wird vom Generator benutzt, um Licht und Materialien zu verbinden.</summary>
        public void Setup(Light light, Renderer[] renderers, Material on, Material off, bool startOn)
        {
            lampLight = light;
            glowRenderers = renderers ?? new Renderer[0];
            onMaterial = on;
            offMaterial = off;
            isOn = startOn;
            Apply();
        }

        void Start()
        {
            Apply();
        }

        public void SetOn(bool on)
        {
            isOn = on;
            Apply();
        }

        public void TurnOn() => SetOn(true);
        public void TurnOff() => SetOn(false);
        public void Toggle() => SetOn(!isOn);

        /// <summary>Überträgt den Zustand "an/aus" auf Licht und Materialien.</summary>
        public void Apply()
        {
            if (lampLight != null) lampLight.enabled = isOn;
            if (glowRenderers == null) return;
            Material material = isOn ? onMaterial : offMaterial;
            if (material == null) return;
            foreach (Renderer glow in glowRenderers)
            {
                if (glow != null) glow.sharedMaterial = material;
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            // Im Editor sofort zeigen, wenn jemand den Haken "Is On" setzt.
            // delayCall, weil Unity in OnValidate keine anderen Komponenten verändern möchte.
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null) Apply();
            };
        }
#endif
    }
}
