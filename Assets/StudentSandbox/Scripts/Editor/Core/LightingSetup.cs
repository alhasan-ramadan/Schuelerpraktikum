using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace StudentSandbox.EditorTools
{
    /// <summary>
    /// Sonne, Tag-/Nacht-Voreinstellungen, Umgebungslicht, Nebel, Post-Processing und die Manager-Objekte.
    /// Es wird nichts "gebacken" (keine Lightmaps) – alles ist Echtzeit-Licht und sofort in der Scene View sichtbar.
    /// </summary>
    public static class LightingSetup
    {
        public static void Build(SandboxContext ctx)
        {
            ApplyLightingSettingsAsset();
            BuildSun(ctx);
            BuildManagers(ctx);
            BuildPostProcessing(ctx);

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 70f;
            RenderSettings.fogEndDistance = 230f;
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Skybox;
            RenderSettings.reflectionIntensity = 0.5f;

            SB.Note(ctx.WorldRoot.gameObject, "Willkommen in der Mini World!",
                "Diese Szene ist deine Spielwiese. Links in der Hierarchy siehst du alle Objekte, nach Bereichen sortiert:\n" +
                "01 Umgebung (Insel, Wasser, Wege) • 02 zentraler Platz • 03 Baubereich • 04 Physiklabor • 05 Lichtlabor • 06 Interaktionsbereich • 07 Kameras • 08 Manager • 09 Deko.\n\n" +
                "Hier im 00_WORLD_ROOT liegen die Sonne und die Nachbearbeitung (Global Volume).\n" +
                "Tipp: Öffne das Workshop-Fenster über Tools > Student Sandbox > Workshop.");
        }

        static void ApplyLightingSettingsAsset()
        {
            LightingSettings settings = AssetDatabase.LoadAssetAtPath<LightingSettings>(SandboxPaths.LightingSettingsPath);
            if (settings == null)
            {
                settings = new LightingSettings { name = "StudentSandbox_Lighting" };
                AssetDatabase.CreateAsset(settings, SandboxPaths.LightingSettingsPath);
            }
            // Kein Backen: keine Wartezeiten, keine Lightmaps. Alles Echtzeit.
            settings.bakedGI = false;
            settings.realtimeGI = false;
            EditorUtility.SetDirty(settings);
            Lightmapping.lightingSettings = settings;
        }

        static void BuildSun(SandboxContext ctx)
        {
            var sunObject = new GameObject("Sun");
            sunObject.transform.SetParent(ctx.WorldRoot, false);
            sunObject.transform.position = new Vector3(0f, 40f, 0f);
            Light sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.75f;
            sun.intensity = 1.25f;
            sunObject.AddComponent<UniversalAdditionalLightData>();
            RenderSettings.sun = sun;
            ctx.Sun = sun;
            SB.Note(sunObject, "Die Sonne (Directional Light)",
                "Ein Directional Light leuchtet überall gleich stark – wie die Sonne. Wichtig ist nur seine ROTATION:\n" +
                "Drehe die Sonne (Rotation X = Höhe am Himmel, Rotation Y = Himmelsrichtung) und schau, wie die Schatten wandern.\n" +
                "Farbe und Intensity (Helligkeit) findest du in der Light-Komponente.\n" +
                "Tag, Sonnenuntergang und Nacht stellst du am einfachsten im Objekt 08_MANAGERS/DayNightController ein. " +
                "Achtung: Diese Tageszeit-Knöpfe stellen die Sonne wieder auf ihre Werte zurück.");
        }

        static void BuildManagers(SandboxContext ctx)
        {
            GameObject dayNightObject = new GameObject("DayNightController");
            dayNightObject.transform.SetParent(ctx.Managers, false);
            DayNightController dayNight = dayNightObject.AddComponent<DayNightController>();
            dayNight.Setup(ctx.Sun, DayPreset(), SunsetPreset(), NightPreset());
            ctx.DayNight = dayNight;
            SB.Note(dayNightObject, "Tag und Nacht",
                "Dieses Skript steuert die Tageszeit für die ganze Welt.\n" +
                "Klicke unten auf TAG, SONNENUNTERGANG oder NACHT – die Scene View ändert sich sofort.\n" +
                "Was passiert dabei? Die Sonne dreht sich, ihre Farbe und Helligkeit ändern sich, der Himmel wechselt und die Laternen gehen an.\n" +
                "Im Play Mode: Taste T oder die Knöpfe im Lichtlabor.");

            GameObject resetObject = new GameObject("PhysicsResetter");
            resetObject.transform.SetParent(ctx.Managers, false);
            ctx.Resetter = resetObject.AddComponent<PhysicsResetter>();
            SB.Note(resetObject, "Physik zurücksetzen",
                "Setzt im Play Mode alle Kugeln, Dominosteine und Zielzonen zurück (Taste R oder roter RESET-Knopf im Physiklabor).\n" +
                "Deine Änderungen aus dem Editor bleiben dabei erhalten.");

            GameObject hudObject = new GameObject("SandboxHUD");
            hudObject.transform.SetParent(ctx.Managers, false);
            ctx.Hud = hudObject.AddComponent<SandboxHUD>();
            SB.Note(hudObject, "Bildschirm-Anzeige",
                "Zeigt im Play Mode die Steuerung (Taste H), den Text unter dem Mauszeiger und Meldungen wie \"Ziel erreicht!\".");
        }

        public static LightingPreset DayPreset()
        {
            return new LightingPreset
            {
                displayName = "Tag",
                sunRotation = new Vector3(50f, 35f, 0f),
                sunColor = new Color(1f, 0.96f, 0.88f),
                sunIntensity = 1.35f,
                skyColor = new Color(0.62f, 0.72f, 0.88f),
                equatorColor = new Color(0.62f, 0.64f, 0.62f),
                groundColor = new Color(0.38f, 0.36f, 0.32f),
                skybox = SB.Mat(M.SkyDay),
                fogColor = new Color(0.78f, 0.9f, 0.97f),
                lampsOn = false,
            };
        }

        public static LightingPreset SunsetPreset()
        {
            return new LightingPreset
            {
                displayName = "Sonnenuntergang",
                sunRotation = new Vector3(9f, 70f, 0f),
                sunColor = new Color(1f, 0.58f, 0.32f),
                sunIntensity = 1.0f,
                skyColor = new Color(0.58f, 0.46f, 0.56f),
                equatorColor = new Color(0.62f, 0.44f, 0.36f),
                groundColor = new Color(0.26f, 0.2f, 0.2f),
                skybox = SB.Mat(M.SkySunset),
                fogColor = new Color(0.98f, 0.64f, 0.45f),
                lampsOn = true,
            };
        }

        public static LightingPreset NightPreset()
        {
            return new LightingPreset
            {
                displayName = "Nacht",
                sunRotation = new Vector3(55f, 210f, 0f),
                sunColor = new Color(0.55f, 0.65f, 1f),
                sunIntensity = 0.22f,
                skyColor = new Color(0.13f, 0.16f, 0.32f),
                equatorColor = new Color(0.09f, 0.11f, 0.2f),
                groundColor = new Color(0.04f, 0.05f, 0.08f),
                skybox = SB.Mat(M.SkyNight),
                fogColor = new Color(0.1f, 0.13f, 0.26f),
                lampsOn = true,
            };
        }

        static void BuildPostProcessing(SandboxContext ctx)
        {
            VolumeProfile profile = ConfigurePostProcessingProfile();

            var volumeObject = new GameObject("Global Volume (Nachbearbeitung)");
            volumeObject.transform.SetParent(ctx.WorldRoot, false);
            Volume volume = volumeObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 1f;
            volume.sharedProfile = profile;
            SB.Note(volumeObject, "Nachbearbeitung (Post-Processing)",
                "Wie ein Foto-Filter für das ganze Bild: leichtes Leuchten (Bloom) bei hellen Lampen, ein dunklerer Rand (Vignette) und etwas kräftigere Farben.\n" +
                "Probier: Haken bei der Volume-Komponente entfernen und vergleichen.");
        }

        /// <summary>
        /// Legt das Post-Processing-Profil an oder setzt es auf die Ausgangswerte zurück
        /// (wird auch beim Wiederherstellen der Ausgangsszene benutzt).
        /// </summary>
        public static VolumeProfile ConfigurePostProcessingProfile()
        {
            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(SandboxPaths.VolumeProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, SandboxPaths.VolumeProfilePath);
            }

            Tonemapping tonemapping = GetOrAdd<Tonemapping>(profile);
            tonemapping.active = true;
            tonemapping.mode.Override(TonemappingMode.Neutral);

            Bloom bloom = GetOrAdd<Bloom>(profile);
            bloom.active = true;
            bloom.threshold.Override(1.0f);
            bloom.intensity.Override(0.55f);
            bloom.scatter.Override(0.6f);

            Vignette vignette = GetOrAdd<Vignette>(profile);
            vignette.active = true;
            vignette.intensity.Override(0.16f);
            vignette.smoothness.Override(0.4f);

            ColorAdjustments color = GetOrAdd<ColorAdjustments>(profile);
            color.active = true;
            color.saturation.Override(14f);
            color.contrast.Override(9f);

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            return profile;
        }

        static T GetOrAdd<T>(VolumeProfile profile) where T : VolumeComponent
        {
            if (profile.TryGet(out T existing)) return existing;
            T component = profile.Add<T>(true);
            component.name = typeof(T).Name;
            component.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(component, profile);
            return component;
        }
    }
}
