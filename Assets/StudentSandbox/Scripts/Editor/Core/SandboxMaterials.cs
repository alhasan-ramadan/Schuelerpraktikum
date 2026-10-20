using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StudentSandbox.EditorTools
{
    /// <summary>
    /// Erzeugt alle Materialien der Spielwiese als echte Material-Assets.
    /// Ist ein Material schon vorhanden, wird es aktualisiert (die GUID bleibt gleich,
    /// damit Szenen und Prefabs ihre Verweise behalten).
    /// </summary>
    public static class SandboxMaterials
    {
        const string UrpLit = "Universal Render Pipeline/Lit";
        const string BuiltinStandard = "Standard";
        const string ProceduralSky = "Skybox/Procedural";
        const string GradientSky = "StudentSandbox/GradientSky";
        const string TextShader = "StudentSandbox/Text3D";

        struct Def
        {
            public string Key;
            public string Folder;
            public string Color;
            public float Smoothness;
            public float Metallic;
            public string Emission;
            public float EmissionIntensity;
        }

        static Def D(string key, string folder, string color, float smoothness = 0.2f, float metallic = 0f, string emission = null, float emissionIntensity = 0f)
        {
            return new Def { Key = key, Folder = folder, Color = color, Smoothness = smoothness, Metallic = metallic, Emission = emission, EmissionIntensity = emissionIntensity };
        }

        const string Pal = SandboxPaths.MaterialsPalette;
        const string Env = SandboxPaths.MaterialsEnvironment;
        const string Fx = SandboxPaths.MaterialsEffects;

        static readonly Def[] Defs =
        {
            // Palette (für die Schülerin)
            D(M.CoralRed, Pal, "#FF6B5B"),
            D(M.SoftYellow, Pal, "#FFD966"),
            D(M.MintGreen, Pal, "#8EE3C0"),
            D(M.SkyBlue, Pal, "#74C0F0"),
            D(M.Lavender, Pal, "#B8A1E8"),
            D(M.WarmOrange, Pal, "#FF9F45"),
            D(M.LightWood, Pal, "#E0B07A", 0.15f),
            D(M.DarkWood, Pal, "#8A5A3B", 0.1f),
            D(M.GrassGreen, Pal, "#7CC657", 0.1f),
            D(M.StoneGray, Pal, "#B5B8BD", 0.15f),
            D(M.WarmWhite, Pal, "#FFF4E0"),
            D(M.Turquoise, Pal, "#36C9C6"),
            D(M.PinkBlossom, Pal, "#FF8FB8"),
            D(M.Charcoal, Pal, "#3B4252", 0.3f),
            D(M.LeafGreen, Pal, "#4FAE5A", 0.1f),
            D(M.BerryPurple, Pal, "#8E5CC8"),

            // Umgebung
            D(M.Grass, Env, "#8ED163", 0.05f),
            D(M.GrassDark, Env, "#6DB24E", 0.05f),
            D(M.Sand, Env, "#F2DFB0", 0.05f),
            D(M.Beach, Env, "#F5E3B5", 0.05f),
            D(M.Path, Env, "#E8D3A8", 0.1f),
            D(M.PlazaLight, Env, "#F3ECDF", 0.15f),
            D(M.PlazaAccent, Env, "#E6C79C", 0.15f),
            D(M.Water, Env, "#4DB6E6", 0.85f),
            D(M.WaterDeep, Env, "#2F8FC9", 0.8f),
            D(M.Cliff, Env, "#C9A27A", 0.05f),
            D(M.Leaves, Env, "#5DBB63", 0.1f),
            D(M.LeavesLight, Env, "#8FD46A", 0.1f),
            D(M.LeavesPine, Env, "#3E9A5F", 0.1f),
            D(M.LeavesAutumn, Env, "#FFB347", 0.1f),
            D(M.Trunk, Env, "#8A5A3B", 0.05f),
            D(M.RoofRed, Env, "#E8604C", 0.2f),
            D(M.RoofBlue, Env, "#5B8FD9", 0.2f),
            D(M.RoofPurple, Env, "#9A7FD1", 0.2f),
            D(M.Wall, Env, "#FFF1DC", 0.1f),
            D(M.Window, Env, "#9ED8F5", 0.7f),
            D(M.Metal, Env, "#AEB6C2", 0.55f, 0.6f),
            D(M.DarkMetal, Env, "#4A5263", 0.45f, 0.5f),
            D(M.Rubber, Env, "#2E333D", 0.1f),
            D(M.Cloud, Env, "#F4F7FB", 0.1f),
            D(M.Rock, Env, "#9EA3AA", 0.1f),
            D(M.LabFloor, Env, "#F7EEDD", 0.2f),
            D(M.LightFloor, Env, "#E9E1F3", 0.2f),
            D(M.TechFloor, Env, "#4B566E", 0.35f),
            D(M.TechPanel, Env, "#64708C", 0.35f),
            D(M.Asphalt, Env, "#5A6070", 0.1f),
            D(M.Stripe, Env, "#FFFFFF", 0.1f),
            D(M.StageWhite, Env, "#F5F5F5", 0.1f),

            // Leuchtende Materialien
            D(M.LampOn, Fx, "#FFF1C9", 0.3f, 0f, "#FFD27A", 2.5f),
            D(M.LampOff, Fx, "#E8E2D0", 0.3f),
            D(M.WindowOn, Fx, "#FFE6A8", 0.3f, 0f, "#FFC861", 1.6f),
            D(M.WindowOff, Fx, "#9ED8F5", 0.7f),
            D(M.TrafficRedOn, Fx, "#FF3B30", 0.5f, 0f, "#FF3B30", 3f),
            D(M.TrafficRedOff, Fx, "#5A1F1F", 0.5f),
            D(M.TrafficYellowOn, Fx, "#FFC400", 0.5f, 0f, "#FFB000", 3f),
            D(M.TrafficYellowOff, Fx, "#5A4A1A", 0.5f),
            D(M.TrafficGreenOn, Fx, "#2EE86A", 0.5f, 0f, "#2EE86A", 3f),
            D(M.TrafficGreenOff, Fx, "#1E4A2A", 0.5f),
            D(M.GoalIdle, Fx, "#FF6B5B", 0.3f, 0f, "#FF4B3B", 0.6f),
            D(M.GoalReached, Fx, "#3EE87A", 0.3f, 0f, "#3EE87A", 2.5f),
            D(M.NeonTeal, Fx, "#36E2D9", 0.4f, 0f, "#36E2D9", 2f),
            D(M.NeonOrange, Fx, "#FF9F45", 0.4f, 0f, "#FF8C2A", 1.5f),
            D(M.NeonPurple, Fx, "#B98CFF", 0.4f, 0f, "#A970FF", 1.8f),
            D(M.NeonPink, Fx, "#FF7EB6", 0.4f, 0f, "#FF5EA0", 1.6f),
            D(M.Crystal, Fx, "#C9A8FF", 0.9f, 0f, "#A57CFF", 1.5f),
            D(M.ButtonRed, Fx, "#FF4B4B", 0.5f, 0f, "#FF2A2A", 0.6f),
            D(M.ButtonGreen, Fx, "#3ED67A", 0.5f, 0f, "#2AD06A", 0.6f),
            D(M.ButtonBlue, Fx, "#4B8BFF", 0.5f, 0f, "#3A7BFF", 0.6f),
            D(M.ButtonYellow, Fx, "#FFD23F", 0.5f, 0f, "#FFC21A", 0.6f),
            D(M.ButtonOrange, Fx, "#FF8C3A", 0.5f, 0f, "#FF7A1A", 0.6f),
            D(M.ButtonPurple, Fx, "#A070FF", 0.5f, 0f, "#8A50FF", 0.6f),
            D(M.ButtonTeal, Fx, "#2EC4B6", 0.5f, 0f, "#1AB8AA", 0.6f),
            D(M.GlowRed, Fx, "#FF5050", 0.4f, 0f, "#FF3030", 2f),
            D(M.GlowGreen, Fx, "#50FF7A", 0.4f, 0f, "#30FF60", 2f),
            D(M.GlowBlue, Fx, "#5A8CFF", 0.4f, 0f, "#3A6CFF", 2f),
            D(M.GlowSun, Fx, "#FFE082", 0.4f, 0f, "#FFC940", 1.5f),
        };

        static readonly Dictionary<string, Material> Cache = new Dictionary<string, Material>();
        static readonly Dictionary<string, PhysicsMaterial> PhysicsCache = new Dictionary<string, PhysicsMaterial>();

        public static IEnumerable<string> PaletteKeys
        {
            get
            {
                foreach (Def def in Defs)
                    if (def.Folder == Pal) yield return def.Key;
            }
        }

        public static int DefinitionCount => Defs.Length + 3;

        /// <summary>Der Shader für alle normalen Materialien – passend zur aktiven Render Pipeline.</summary>
        public static Shader LitShader
        {
            get
            {
                Shader shader = null;
                if (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null) shader = Shader.Find(UrpLit);
                if (shader == null) shader = Shader.Find(BuiltinStandard);
                if (shader == null) shader = Shader.Find(UrpLit);
                return shader;
            }
        }

        public static void CreateAll()
        {
            Cache.Clear();
            Shader lit = LitShader;
            if (lit == null) throw new InvalidOperationException("Kein Lit-Shader gefunden (weder URP Lit noch Standard).");

            foreach (Def def in Defs)
            {
                string path = def.Folder + "/" + def.Key + ".mat";
                var fresh = new Material(lit);
                Configure(fresh, def);
                Cache[def.Key] = Commit(path, fresh);
            }

            // Himmel: oben, Horizont, unten, Sonnenfarbe, Glanz, Sonnengröße, Sterne.
            CreateSky(M.SkyDay, "#4C94D8", "#C8E6F7", "#8CC4E2", "#FFF0C8", 0.55f, 0.035f, 0f);
            CreateSky(M.SkySunset, "#3A4A8C", "#FFA06A", "#7A5560", "#FF9A4A", 1.1f, 0.05f, 0f);
            CreateSky(M.SkyNight, "#060A1C", "#1C2448", "#080B18", "#C8D8FF", 0.25f, 0.02f, 1f);
            CreateTextMaterial();

            CreatePhysicsMaterials();
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Speichert ein frisch konfiguriertes Material. Gibt es das Asset schon, wird sein Inhalt komplett ersetzt
        /// (auch Dinge wie Transparenz oder Texturen, die jemand von Hand geändert hat) – die GUID bleibt gleich,
        /// damit Szenen und Prefabs ihre Verweise behalten.
        /// </summary>
        static Material Commit(string path, Material fresh)
        {
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(fresh, path);
                return fresh;
            }
            string name = existing.name;
            EditorUtility.CopySerialized(fresh, existing);
            existing.name = name;
            UnityEngine.Object.DestroyImmediate(fresh);
            EditorUtility.SetDirty(existing);
            return existing;
        }

        static void Configure(Material material, Def def)
        {
            Color color = Hex(def.Color);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", def.Smoothness);
            if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", def.Smoothness);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", def.Metallic);

            bool emissive = !string.IsNullOrEmpty(def.Emission) && def.EmissionIntensity > 0f;
            if (emissive)
            {
                Color emission = Hex(def.Emission) * def.EmissionIntensity;
                material.SetColor("_EmissionColor", emission);
                material.EnableKeyword("_EMISSION");
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            else
            {
                if (material.HasProperty("_EmissionColor")) material.SetColor("_EmissionColor", Color.black);
                material.DisableKeyword("_EMISSION");
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            }
            material.enableInstancing = true;
        }

        static void CreateSky(string key, string top, string horizon, string bottom, string sunColor, float sunGlow, float sunSize, float stars)
        {
            string path = SandboxPaths.MaterialsSky + "/" + key + ".mat";
            Shader gradient = Shader.Find(GradientSky);
            if (gradient != null)
            {
                var material = new Material(gradient);
                material.SetColor("_TopColor", Hex(top));
                material.SetColor("_HorizonColor", Hex(horizon));
                material.SetColor("_BottomColor", Hex(bottom));
                material.SetColor("_SunColor", Hex(sunColor));
                material.SetFloat("_SunGlow", sunGlow);
                material.SetFloat("_SunSize", sunSize);
                material.SetFloat("_StarDensity", stars);
                Cache[key] = Commit(path, material);
                return;
            }

            // Rückfall: Unitys eingebauter prozeduraler Himmel.
            Shader procedural = Shader.Find(ProceduralSky);
            if (procedural == null)
            {
                Debug.LogWarning("[Student Sandbox] Kein Himmels-Shader gefunden – Himmel bleibt einfarbig.");
                return;
            }
            var fallback = new Material(procedural);
            fallback.SetFloat("_Exposure", stars > 0f ? 0.3f : 1.2f);
            Cache[key] = Commit(path, fallback);
        }

        /// <summary>
        /// Material für 3D-Schrift (TextMesh). Nutzt den eigenen Shader "StudentSandbox/Text3D",
        /// damit Schrift hinter Wänden verschwindet (der eingebaute Schrift-Shader hat unter URP keinen Tiefentest).
        /// </summary>
        static void CreateTextMaterial()
        {
            Shader shader = Shader.Find(TextShader);
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (shader == null || font == null) return;
            string path = SandboxPaths.MaterialsEffects + "/" + M.Text3D + ".mat";
            var material = new Material(shader);
            material.mainTexture = font.material != null ? font.material.mainTexture : null;
            material.SetColor("_Color", Color.white);
            Cache[M.Text3D] = Commit(path, material);
        }

        public static void CreatePhysicsMaterials()
        {
            PhysicsCache.Clear();
            CreatePhysics(PM.Slippery, 0.02f, 0.02f, 0f, PhysicsMaterialCombine.Minimum, PhysicsMaterialCombine.Average);
            CreatePhysics(PM.Normal, 0.5f, 0.6f, 0f, PhysicsMaterialCombine.Average, PhysicsMaterialCombine.Average);
            CreatePhysics(PM.Bouncy, 0.4f, 0.4f, 0.9f, PhysicsMaterialCombine.Average, PhysicsMaterialCombine.Maximum);
        }

        static void CreatePhysics(string key, float dynamicFriction, float staticFriction, float bounciness,
            PhysicsMaterialCombine frictionCombine, PhysicsMaterialCombine bounceCombine)
        {
            string path = SandboxPaths.PhysicsMaterials + "/" + key + ".physicMaterial";
            PhysicsMaterial material = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(path);
            if (material == null)
            {
                material = new PhysicsMaterial(key);
                AssetDatabase.CreateAsset(material, path);
            }
            material.dynamicFriction = dynamicFriction;
            material.staticFriction = staticFriction;
            material.bounciness = bounciness;
            material.frictionCombine = frictionCombine;
            material.bounceCombine = bounceCombine;
            EditorUtility.SetDirty(material);
            PhysicsCache[key] = material;
        }

        /// <summary>Liefert ein generiertes Material. Lädt es bei Bedarf aus dem Projekt.</summary>
        public static Material Get(string key)
        {
            if (Cache.TryGetValue(key, out Material cached) && cached != null) return cached;
            string[] folders = { SandboxPaths.MaterialsPalette, SandboxPaths.MaterialsEnvironment, SandboxPaths.MaterialsEffects, SandboxPaths.MaterialsSky };
            foreach (string folder in folders)
            {
                Material material = AssetDatabase.LoadAssetAtPath<Material>(folder + "/" + key + ".mat");
                if (material != null)
                {
                    Cache[key] = material;
                    return material;
                }
            }
            throw new ArgumentException("[Student Sandbox] Unbekanntes Material: " + key + " (Konstante in M verwenden und CreateAll() vorher aufrufen)");
        }

        public static PhysicsMaterial GetPhysics(string key)
        {
            if (PhysicsCache.TryGetValue(key, out PhysicsMaterial cached) && cached != null) return cached;
            PhysicsMaterial material = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(SandboxPaths.PhysicsMaterials + "/" + key + ".physicMaterial");
            if (material == null) throw new ArgumentException("[Student Sandbox] Unbekanntes Physik-Material: " + key);
            PhysicsCache[key] = material;
            return material;
        }

        public static Color Hex(string hex)
        {
            return ColorUtility.TryParseHtmlString(hex, out Color color) ? color : Color.magenta;
        }
    }
}
