using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace StudentSandbox.EditorTools
{
    /// <summary>Ergebnis einer einzelnen Prüfung.</summary>
    public enum CheckStatus
    {
        Pass,
        Warning,
        Fail
    }

    /// <summary>Eine Zeile im Prüfbericht.</summary>
    public sealed class CheckResult
    {
        public CheckStatus Status;
        public string Category;
        public string Message;
        /// <summary>Optional: Objekt, das beim Klick auf die Console-Meldung markiert wird.</summary>
        public Object Context;

        public CheckResult()
        {
        }

        public CheckResult(CheckStatus status, string category, string message, Object context = null)
        {
            Status = status;
            Category = category;
            Message = message;
            Context = context;
        }

        public string StatusLabel
        {
            get
            {
                switch (Status)
                {
                    case CheckStatus.Fail: return "FEHLER";
                    case CheckStatus.Warning: return "WARNUNG";
                    default: return "OK";
                }
            }
        }

        public override string ToString() => "[" + StatusLabel + "] " + Category + ": " + Message;
    }

    /// <summary>Alle Ergebnisse einer Prüfung der Spielwiese.</summary>
    public sealed class ValidationReport
    {
        public List<CheckResult> Results = new List<CheckResult>();

        public int Failures => Count(CheckStatus.Fail);
        public int Warnings => Count(CheckStatus.Warning);
        public int Passes => Count(CheckStatus.Pass);
        public bool Success => Failures == 0;

        public int Count(CheckStatus status)
        {
            int count = 0;
            foreach (CheckResult result in Results)
            {
                if (result.Status == status) count++;
            }
            return count;
        }

        public IEnumerable<CheckResult> Problems => Results.Where(r => r.Status != CheckStatus.Pass);

        public CheckResult Add(CheckStatus status, string category, string message, Object context = null)
        {
            var result = new CheckResult(status, category, message, context);
            Results.Add(result);
            return result;
        }

        public CheckResult Pass(string category, string message, Object context = null) => Add(CheckStatus.Pass, category, message, context);
        public CheckResult Warn(string category, string message, Object context = null) => Add(CheckStatus.Warning, category, message, context);
        public CheckResult Fail(string category, string message, Object context = null) => Add(CheckStatus.Fail, category, message, context);

        /// <summary>Trägt "OK" oder (je nach <paramref name="severity"/>) eine Warnung bzw. einen Fehler ein. Gibt <paramref name="ok"/> zurück.</summary>
        public bool Check(bool ok, string category, string passMessage, string problemMessage, Object context = null,
            CheckStatus severity = CheckStatus.Fail)
        {
            if (ok) Pass(category, passMessage, context);
            else Add(severity == CheckStatus.Pass ? CheckStatus.Fail : severity, category, problemMessage, context);
            return ok;
        }

        public void Merge(ValidationReport other)
        {
            if (other == null || ReferenceEquals(other, this)) return;
            Results.AddRange(other.Results);
        }

        /// <summary>Der komplette Bericht als lesbarer Text (für Console, Datei und Tests).</summary>
        public string ToText()
        {
            var text = new StringBuilder();
            text.AppendLine("===== Student Sandbox – Prüfbericht =====");
            text.AppendLine("Zeitpunkt: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            text.AppendLine("Ergebnis:  " + (Success ? "BESTANDEN" : "NICHT BESTANDEN") +
                            "  (" + Failures + " Fehler, " + Warnings + " Warnungen, " + Passes + " OK)");

            List<CheckResult> problems = Problems.OrderBy(r => r.Status == CheckStatus.Fail ? 0 : 1).ToList();
            if (problems.Count > 0)
            {
                text.AppendLine();
                text.AppendLine("--- Zuerst ansehen ---");
                foreach (CheckResult result in problems) text.AppendLine(Line(result));
            }

            var categories = new List<string>();
            foreach (CheckResult result in Results)
            {
                if (!categories.Contains(result.Category)) categories.Add(result.Category);
            }

            foreach (string category in categories)
            {
                text.AppendLine();
                text.AppendLine("--- " + category + " ---");
                foreach (CheckResult result in Results)
                {
                    if (result.Category == category) text.AppendLine(Line(result));
                }
            }
            return text.ToString();
        }

        static string Line(CheckResult result)
        {
            string label = result.StatusLabel;
            return "  " + label.PadRight(8) + result.Category + ": " + result.Message;
        }
    }

    /// <summary>
    /// Prüft, ob die Spielwiese vollständig und funktionsfähig ist: Dateien, Prefabs, Materialien,
    /// die Szene (Hierarchie, Kamera, Licht, Physik, Schalter) und die Benennung.
    /// Menü: Tools > Student Sandbox > Validate Sandbox
    /// Kommandozeile: -executeMethod StudentSandbox.EditorTools.SandboxValidator.ValidateBatch [-sandboxReport &lt;Datei&gt;]
    /// </summary>
    public static class SandboxValidator
    {
        const string CatFiles = "Dateien";
        const string CatPrefabs = "Prefabs";
        const string CatMaterials = "Materialien";
        const string CatMeshes = "Meshes";
        const string CatScripts = "Skripte";
        const string CatScene = "Szene";
        const string CatHierarchy = "Hierarchie";
        const string CatCamera = "Kamera";
        const string CatLighting = "Licht";
        const string CatPhysics = "Physik";
        const string CatInteraction = "Interaktion";
        const string CatSwitches = "Schalter";
        const string CatRendering = "Darstellung";
        const string CatNaming = "Namen";
        const string CatWorkshop = "Workshop";

        const string RuntimeAssemblyName = "StudentSandbox.Runtime";
        const string EditorAssemblyName = "StudentSandbox.Editor";
        const string RuntimeScriptsFolder = SandboxPaths.Root + "/Scripts/Runtime";
        const string EditorScriptsFolder = SandboxPaths.Root + "/Scripts/Editor";
        const int MaxListedNames = 12;

        /// <summary>Die zehn Haupt-Gruppen der Hierarchy (siehe <see cref="SandboxContext.CreateRootGroups"/>).</summary>
        public static readonly string[] RootGroups =
        {
            "00_WORLD_ROOT", "01_ENVIRONMENT", "02_CENTRAL_HUB", "03_CREATIVE_ZONE", "04_PHYSICS_ZONE",
            "05_LIGHT_ZONE", "06_INTERACTION_ZONE", "07_CAMERAS", "08_MANAGERS", "09_DECORATION"
        };

        static readonly string[] ZoneGroups = { "03_CREATIVE_ZONE", "04_PHYSICS_ZONE", "05_LIGHT_ZONE", "06_INTERACTION_ZONE" };

        /// <summary>Objekte, auf die das Workshop-Fenster (Lektionen, "Zeig mir"-Knöpfe) und die Tests per Namen zugreifen.</summary>
        static readonly string[] WorkshopObjects =
        {
            "Tree_Small_1", "House_Red", "Bench_Pond_1", "BuildArea", "ExampleTower", "ColorPalette",
            "PhysicsBall", "Ramp_Main", "GoalZone", "Hammer", "PhysicsBall_Blue", "TargetZone", "ObstacleCourse", "Domino_10",
            "Sun", "DayNightController", "ColorLight_Red", "ColorLight_Blue", "ShadowSpotLight", "ShadowSculpture", "StreetLamp_1",
            "AutomaticDoor", "DoorSwitch", "TrafficLight", "PedestrianButton", "Desk_TrafficButton", "Crystal", "MovingPlatform",
            "PointB", "ControlDesk"
        };

        static readonly Regex GenericNamePattern = new Regex(@"^(Cube|Sphere|Cylinder|GameObject|Capsule|Plane|Quad)( \(\d+\))?$", RegexOptions.Compiled);

        // ================================================================== Menü und Kommandozeile

        [MenuItem(SandboxGenerator.MenuRoot + "Validate Sandbox", priority = 40)]
        public static void ValidateFromMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Student Sandbox – Prüfung", "Bitte zuerst den Play Mode beenden.", "OK");
                return;
            }

            ValidationReport report;
            try
            {
                EditorUtility.DisplayProgressBar("Student Sandbox", "Dateien, Prefabs und Materialien prüfen …", 0.2f);
                report = ValidateAssets();

                if (SandboxGenerator.SceneExists)
                {
                    if (!SandboxSceneView.IsSandboxSceneOpen)
                    {
                        EditorUtility.ClearProgressBar();
                        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
                        EditorSceneManager.OpenScene(SandboxPaths.ScenePath, OpenSceneMode.Single);
                    }
                    EditorUtility.DisplayProgressBar("Student Sandbox", "Szene prüfen …", 0.6f);
                    report.Merge(ValidateOpenScene());
                }
            }
            catch (Exception exception)
            {
                report = new ValidationReport();
                report.Fail(CatScene, "Die Prüfung ist abgebrochen: " + exception.Message);
                Debug.LogException(exception);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            LogReport(report);
            ShowSummaryDialog(report);
        }

        /// <summary>
        /// Für den Batchmode: prüft alles, schreibt den Bericht (optional) in die Datei hinter "-sandboxReport"
        /// und beendet Unity mit Exit-Code 0 (keine Fehler) oder 1 (Fehler gefunden).
        /// </summary>
        public static void ValidateBatch()
        {
            ValidationReport report;
            try
            {
                report = ValidateAll();
            }
            catch (Exception exception)
            {
                report = new ValidationReport();
                report.Fail(CatScene, "Die Prüfung ist mit einer Ausnahme abgebrochen: " + exception);
            }

            string text = report.ToText();
            string reportPath = GetCommandLineArgument("-sandboxReport");
            if (!string.IsNullOrEmpty(reportPath))
            {
                try
                {
                    string fullPath = Path.GetFullPath(reportPath);
                    string directory = Path.GetDirectoryName(fullPath);
                    if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
                    File.WriteAllText(fullPath, text, new UTF8Encoding(false));
                    Debug.Log("[Student Sandbox] Prüfbericht gespeichert: " + fullPath);
                }
                catch (Exception exception)
                {
                    Debug.LogError("[Student Sandbox] Prüfbericht konnte nicht gespeichert werden (" + reportPath + "): " + exception.Message);
                }
            }

            Debug.Log("[Student Sandbox] Prüfung beendet: " + report.Failures + " Fehler, " + report.Warnings + " Warnungen.\n" + text);
            if (Application.isBatchMode) EditorApplication.Exit(report.Failures == 0 ? 0 : 1);
        }

        // ================================================================== Öffentliche Prüfungen

        /// <summary>Prüft alles, was ohne geöffnete Szene geht: Dateien, Ordner, Prefabs, Materialien, Meshes, Skripte.</summary>
        public static ValidationReport ValidateAssets()
        {
            var report = new ValidationReport();
            Run(report, CatFiles, () => CheckFilesAndFolders(report));
            Run(report, CatPrefabs, () => CheckPrefabs(report));
            Run(report, CatMaterials, () => CheckMaterials(report));
            Run(report, CatMeshes, () => CheckMeshes(report));
            Run(report, CatScripts, () => CheckScripts(report));
            return report;
        }

        /// <summary>Prüft die aktive Szene (sollte die Spielwiese sein).</summary>
        public static ValidationReport ValidateOpenScene()
        {
            var report = new ValidationReport();
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                report.Fail(CatScene, "Es ist keine Szene geöffnet.");
                return report;
            }

            if (scene.path == SandboxPaths.ScenePath) report.Pass(CatScene, "Aktive Szene: " + scene.path);
            else report.Warn(CatScene, "Die aktive Szene ist nicht die Spielwiese (" + (string.IsNullOrEmpty(scene.path) ? scene.name : scene.path) + "). Geprüft wird trotzdem.");

            var index = new SceneIndex(scene);
            Run(report, CatHierarchy, () => CheckHierarchy(index, report));
            Run(report, CatCamera, () => CheckCamera(index, report));
            Run(report, CatLighting, () => CheckLighting(index, report));
            Run(report, CatPhysics, () => CheckGround(index, report));
            Run(report, CatPhysics, () => CheckMarbleRun(index, report));
            Run(report, CatPhysics, () => CheckDominoes(index, report));
            Run(report, CatInteraction, () => CheckInteractions(index, report));
            Run(report, CatSwitches, () => CheckSwitches(index, report));
            Run(report, CatRendering, () => CheckRenderers(index, report));
            Run(report, CatNaming, () => CheckNaming(index, report));
            Run(report, CatWorkshop, () => CheckWorkshopObjects(index, report));

            if (scene.isDirty) report.Warn(CatScene, "Die Szene hat ungespeicherte Änderungen (Strg+S speichert).");
            else report.Pass(CatScene, "Die Szene ist gespeichert.");
            return report;
        }

        /// <summary>Prüft die Dateien und öffnet dafür die Spielwiesen-Szene (falls sie nicht schon offen ist).</summary>
        public static ValidationReport ValidateAll()
        {
            ValidationReport report = ValidateAssets();
            if (!File.Exists(SandboxPaths.ScenePath))
            {
                report.Fail(CatScene, "Szenen-Prüfung übersprungen, weil die Szene fehlt.");
                return report;
            }
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                report.Fail(CatScene, "Szenen-Prüfung übersprungen: Bitte zuerst den Play Mode beenden.");
                return report;
            }

            if (!SandboxSceneView.IsSandboxSceneOpen)
            {
                if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    report.Warn(CatScene, "Szenen-Prüfung abgebrochen (die offene Szene wurde nicht gewechselt).");
                    return report;
                }
                try
                {
                    EditorSceneManager.OpenScene(SandboxPaths.ScenePath, OpenSceneMode.Single);
                }
                catch (Exception exception)
                {
                    report.Fail(CatScene, "Die Szene konnte nicht geöffnet werden: " + exception.Message);
                    return report;
                }
            }

            report.Merge(ValidateOpenScene());
            return report;
        }

        // ================================================================== Dateien und Ordner

        static void CheckFilesAndFolders(ValidationReport report)
        {
            report.Check(File.Exists(SandboxPaths.ScenePath), CatFiles,
                "Szene vorhanden: " + SandboxPaths.ScenePath,
                "Die Szene fehlt: " + SandboxPaths.ScenePath + " – bitte 'Tools > Student Sandbox > Generate Sandbox' ausführen.");
            report.Check(File.Exists(SandboxPaths.OriginalScenePath), CatFiles,
                "Ausgangskopie vorhanden: " + SandboxPaths.OriginalScenePath,
                "Die Ausgangskopie fehlt: " + SandboxPaths.OriginalScenePath + " ('Restore Original Scene' funktioniert sonst nicht).");

            string[] folders =
            {
                SandboxPaths.Root, SandboxPaths.Scenes, SandboxPaths.OriginalFolder,
                SandboxPaths.Prefabs, SandboxPaths.PrefabsEnvironment, SandboxPaths.PrefabsCreative, SandboxPaths.PrefabsPhysics, SandboxPaths.PrefabsInteractions,
                SandboxPaths.Materials, SandboxPaths.MaterialsPalette, SandboxPaths.MaterialsEnvironment, SandboxPaths.MaterialsEffects, SandboxPaths.MaterialsSky,
                SandboxPaths.PhysicsMaterials, SandboxPaths.Meshes, SandboxPaths.Settings, RuntimeScriptsFolder, EditorScriptsFolder
            };
            List<string> missing = folders.Where(folder => !AssetDatabase.IsValidFolder(folder)).ToList();
            report.Check(missing.Count == 0, CatFiles,
                "Alle " + folders.Length + " Ordner vorhanden.",
                "Ordner fehlen: " + JoinLimited(missing));

            if (!AssetDatabase.IsValidFolder(SandboxPaths.Documentation))
                report.Warn(CatFiles, "Der Ordner " + SandboxPaths.Documentation + " (Anleitung) fehlt.");

            bool volumeOk = AssetDatabase.LoadAssetAtPath<VolumeProfile>(SandboxPaths.VolumeProfilePath) != null;
            report.Check(volumeOk, CatFiles, "Post-Processing-Profil vorhanden.",
                "Post-Processing-Profil fehlt: " + SandboxPaths.VolumeProfilePath, null, CheckStatus.Warning);
        }

        // ================================================================== Prefabs

        static void CheckPrefabs(ValidationReport report)
        {
            var names = new List<string>(SandboxPrefabs.AllNames);
            foreach (string constant in ConstStrings(typeof(P)))
            {
                if (!names.Contains(constant))
                    report.Fail(CatPrefabs, "Das Prefab '" + constant + "' (Konstante in P) ist nicht in SandboxPrefabs eingetragen.");
            }

            int loaded = 0;
            var withoutCollider = new List<string>();
            foreach (string prefabName in names)
            {
                string path;
                try
                {
                    path = SandboxPrefabs.PathFor(prefabName);
                }
                catch (ArgumentException exception)
                {
                    report.Fail(CatPrefabs, exception.Message);
                    continue;
                }

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    report.Fail(CatPrefabs, "Prefab fehlt: " + path);
                    continue;
                }

                int missingScripts = CountMissingScripts(prefab);
                if (missingScripts > 0)
                {
                    report.Fail(CatPrefabs, "Prefab " + prefabName + ": " + missingScripts + " fehlende Skripte (Missing Script).", prefab);
                    continue;
                }

                if (prefabName != P.Cloud && prefab.GetComponentInChildren<Collider>(true) == null) withoutCollider.Add(prefabName);
                loaded++;
            }

            if (loaded > 0) report.Pass(CatPrefabs, loaded + " von " + names.Count + " Prefabs laden fehlerfrei (keine fehlenden Skripte).");
            if (withoutCollider.Count > 0)
                report.Warn(CatPrefabs, "Prefabs ohne Collider (Kugeln rollen hindurch): " + JoinLimited(withoutCollider));

            // Bausteine, auf die sich die Bereiche verlassen.
            var issues = new List<string>();
            RequireInPrefab<PhysicsBall>(P.PhysicsBall, null, issues);
            RequireInPrefab<Rigidbody>(P.PhysicsBall, null, issues);
            RequireInPrefab<SphereCollider>(P.PhysicsBall, null, issues);
            RequireInPrefab<PhysicsBall>(P.BouncyBall, null, issues);
            RequireInPrefab<GoalZone>(P.TargetZone, null, issues);
            RequireInPrefab<Rigidbody>(P.Domino, null, issues);
            RequireInPrefab<ResettableObject>(P.Domino, null, issues);
            RequireInPrefab<Rigidbody>(P.ObstacleCube, null, issues);
            RequireInPrefab<Bumper>(P.Bumper, null, issues);
            RequireInPrefab<LampController>(P.LampPost, null, issues);
            RequireInPrefab<LampController>(P.SmallHouse, null, issues);
            RequireInPrefab<ClickableSwitch>(P.PushButton, "*", issues);
            RequireInPrefab<Rotator>(P.RotatingCrystal, "Crystal", issues);
            RequireInPrefab<MovingPlatform>(P.MovingPlatformSet, "MovingPlatform", issues);

            GameObject targetZone = LoadPrefab(P.TargetZone);
            if (targetZone != null && !HasEnabledTrigger(targetZone))
                issues.Add("TargetZone: kein Trigger-Collider auf dem Wurzel-Objekt");

            report.Check(issues.Count == 0, CatPrefabs,
                "Wichtige Prefab-Bausteine vollständig (Kugeln, Ziel, Domino, Lampen, Knopf, Kristall, Plattform).",
                "Prefab-Bausteine fehlen: " + JoinLimited(issues));
        }

        static void RequireInPrefab<T>(string prefabName, string childName, List<string> issues) where T : Component
        {
            GameObject prefab = LoadPrefab(prefabName);
            if (prefab == null) return; // wurde oben schon gemeldet

            T component;
            if (childName == "*") component = prefab.GetComponentInChildren<T>(true);
            else if (string.IsNullOrEmpty(childName)) component = prefab.GetComponent<T>();
            else
            {
                Transform child = FindDescendant(prefab.transform, childName);
                component = child != null ? child.GetComponent<T>() : null;
            }

            if (component == null)
            {
                string where = string.IsNullOrEmpty(childName) ? "" : childName == "*" ? " (irgendwo)" : " (Kind '" + childName + "')";
                issues.Add(prefabName + ": " + typeof(T).Name + where);
            }
        }

        static GameObject LoadPrefab(string prefabName)
        {
            try
            {
                return AssetDatabase.LoadAssetAtPath<GameObject>(SandboxPrefabs.PathFor(prefabName));
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        // ================================================================== Materialien

        static void CheckMaterials(ValidationReport report)
        {
            if (!AssetDatabase.IsValidFolder(SandboxPaths.Materials) || !AssetDatabase.IsValidFolder(SandboxPaths.MaterialsPalette))
            {
                report.Fail(CatMaterials, "Der Materialien-Ordner fehlt (" + SandboxPaths.MaterialsPalette + ") – bitte 'Generate Sandbox' ausführen.");
                return;
            }

            string[] paletteGuids = AssetDatabase.FindAssets("t:Material", new[] { SandboxPaths.MaterialsPalette });
            report.Check(paletteGuids.Length >= 10, CatMaterials,
                "Farbpalette: " + paletteGuids.Length + " Materialien in Materials/Palette.",
                "Nur " + paletteGuids.Length + " Materialien in Materials/Palette (mindestens 10 nötig).");

            List<string> missingKeys = ConstStrings(typeof(M)).Where(key => FindMaterialAsset(key) == null).ToList();
            report.Check(missingKeys.Count == 0, CatMaterials,
                "Alle " + ConstStrings(typeof(M)).Count + " Materialien aus der Liste M sind vorhanden.",
                "Materialien fehlen: " + JoinLimited(missingKeys));

            var broken = new List<string>();
            var unsupportedWithoutGpu = new List<string>();
            string[] allGuids = AssetDatabase.FindAssets("t:Material", new[] { SandboxPaths.Materials });
            foreach (string guid in allGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null) continue;
                string problem = ShaderProblem(material.shader, out bool onlyBecauseNoGpu);
                if (problem == null) continue;
                if (onlyBecauseNoGpu) unsupportedWithoutGpu.Add(material.name);
                else broken.Add(material.name + " (" + problem + ")");
            }
            report.Check(broken.Count == 0, CatMaterials,
                "Alle " + allGuids.Length + " Materialien haben einen funktionierenden Shader.",
                "Materialien mit kaputtem Shader (werden pink): " + JoinLimited(broken));
            if (unsupportedWithoutGpu.Count > 0)
                report.Warn(CatMaterials, "Ohne Grafikkarte (-nographics) nicht prüfbar: " + JoinLimited(unsupportedWithoutGpu));

            var skyIssues = new List<string>();
            foreach (string key in new[] { M.SkyDay, M.SkySunset, M.SkyNight })
            {
                var sky = AssetDatabase.LoadAssetAtPath<Material>(SandboxPaths.MaterialsSky + "/" + key + ".mat");
                if (sky == null) skyIssues.Add(key + " fehlt");
                else if (sky.shader == null || (sky.shader.name.IndexOf("Skybox", StringComparison.OrdinalIgnoreCase) < 0 && sky.shader.name.IndexOf("Sky", StringComparison.OrdinalIgnoreCase) < 0))
                    skyIssues.Add(key + " ist kein Himmels-Material (" + (sky.shader != null ? sky.shader.name : "kein Shader") + ")");
            }
            report.Check(skyIssues.Count == 0, CatMaterials,
                "Himmel für Tag, Sonnenuntergang und Nacht vorhanden.",
                "Himmels-Materialien: " + JoinLimited(skyIssues));

            var missingPhysics = new List<string>();
            foreach (string key in ConstStrings(typeof(PM)))
            {
                string path = SandboxPaths.PhysicsMaterials + "/" + key + ".physicMaterial";
                if (AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(path) == null) missingPhysics.Add(key);
            }
            report.Check(missingPhysics.Count == 0, CatMaterials,
                "Physik-Materialien vorhanden: " + string.Join(", ", ConstStrings(typeof(PM))) + ".",
                "Physik-Materialien fehlen: " + JoinLimited(missingPhysics));

            var bouncy = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(SandboxPaths.PhysicsMaterials + "/" + PM.Bouncy + ".physicMaterial");
            if (bouncy != null && bouncy.bounciness < 0.5f)
                report.Warn(CatMaterials, "Das Physik-Material 'Bouncy' springt kaum (Bounciness " + bouncy.bounciness.ToString("0.00") + ").", bouncy);
        }

        static Material FindMaterialAsset(string key)
        {
            string[] folders = { SandboxPaths.MaterialsPalette, SandboxPaths.MaterialsEnvironment, SandboxPaths.MaterialsEffects, SandboxPaths.MaterialsSky };
            foreach (string folder in folders)
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>(folder + "/" + key + ".mat");
                if (material != null) return material;
            }
            return null;
        }

        // ================================================================== Meshes

        static void CheckMeshes(ValidationReport report)
        {
            List<string> keys = ConstStrings(typeof(MK));
            var missing = new List<string>();
            foreach (string key in keys)
            {
                var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(SandboxPaths.Meshes + "/" + key + ".asset");
                if (mesh == null) missing.Add(key);
                else if (mesh.vertexCount == 0) missing.Add(key + " (leer)");
            }
            report.Check(missing.Count == 0, CatMeshes,
                "Alle " + keys.Count + " Low-Poly-Meshes vorhanden.",
                "Meshes fehlen: " + JoinLimited(missing));
        }

        // ================================================================== Skripte und Assemblies

        static void CheckScripts(ValidationReport report)
        {
            string editorName = typeof(SandboxGenerator).Assembly.GetName().Name;
            report.Check(editorName == EditorAssemblyName, CatScripts,
                "Editor-Skripte liegen in der Editor-Assembly '" + EditorAssemblyName + "'.",
                "Editor-Skripte liegen in der Assembly '" + editorName + "' statt '" + EditorAssemblyName + "'.");

            System.Reflection.Assembly runtimeAssembly = typeof(DoorController).Assembly;
            string runtimeName = runtimeAssembly.GetName().Name;
            report.Check(runtimeName == RuntimeAssemblyName, CatScripts,
                "Spiel-Skripte liegen in der Runtime-Assembly '" + RuntimeAssemblyName + "'.",
                "Spiel-Skripte liegen in der Assembly '" + runtimeName + "' statt '" + RuntimeAssemblyName + "'.");

            // Im Editor wird die Runtime-Assembly mit UNITY_EDITOR übersetzt. Ein Verweis auf UnityEditor ist dort nur
            // erlaubt, wenn er in "#if UNITY_EDITOR"-Blöcken steht – sonst bricht ein Build ab.
            List<string> editorReferences = runtimeAssembly.GetReferencedAssemblies()
                .Select(reference => reference.Name)
                .Where(IsEditorAssemblyName)
                .ToList();
            List<string> unguarded = FindUnguardedEditorUsages(RuntimeScriptsFolder);
            if (unguarded.Count > 0)
                report.Fail(CatScripts, "Spiel-Skripte benutzen UnityEditor außerhalb von '#if UNITY_EDITOR' (Build würde scheitern): " + JoinLimited(unguarded));
            else if (editorReferences.Count == 0)
                report.Pass(CatScripts, "Die Runtime-Assembly verweist nicht auf UnityEditor.");
            else
                report.Pass(CatScripts, "Die Runtime-Assembly nutzt UnityEditor nur in '#if UNITY_EDITOR'-Blöcken (" + string.Join(", ", editorReferences) + ") – Builds funktionieren.");

            CheckScriptAssemblies(report, EditorScriptsFolder, EditorAssemblyName);
            CheckScriptAssemblies(report, RuntimeScriptsFolder, RuntimeAssemblyName);

            List<string> playerAssemblies = UnityEditor.Compilation.CompilationPipeline
                .GetAssemblies(UnityEditor.Compilation.AssembliesType.Player)
                .Select(assembly => assembly.name)
                .ToList();
            if (playerAssemblies.Count > 0)
            {
                bool runtimeInPlayer = playerAssemblies.Contains(RuntimeAssemblyName);
                bool editorInPlayer = playerAssemblies.Contains(EditorAssemblyName);
                report.Check(runtimeInPlayer && !editorInPlayer, CatScripts,
                    "Build-Einstellungen der Assemblies stimmen (Runtime im Spiel, Editor nur im Editor).",
                    (runtimeInPlayer ? "" : "'" + RuntimeAssemblyName + "' fehlt im Spiel-Build. ") +
                    (editorInPlayer ? "'" + EditorAssemblyName + "' würde im Spiel-Build landen (Include Platforms = Editor setzen)." : ""));
            }
        }

        static void CheckScriptAssemblies(ValidationReport report, string folder, string expectedAssembly)
        {
            if (!Directory.Exists(folder)) return;
            var wrong = new List<string>();
            string[] files = Directory.GetFiles(folder, "*.cs", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                string assetPath = file.Replace('\\', '/');
                string assembly = UnityEditor.Compilation.CompilationPipeline.GetAssemblyNameFromScriptPath(assetPath);
                if (string.IsNullOrEmpty(assembly)) continue;
                if (assembly.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)) assembly = assembly.Substring(0, assembly.Length - 4);
                if (assembly != expectedAssembly) wrong.Add(Path.GetFileName(assetPath) + " → " + assembly);
            }
            report.Check(wrong.Count == 0, CatScripts,
                files.Length + " Skripte in " + folder + " gehören zu '" + expectedAssembly + "'.",
                "Skripte in " + folder + " gehören zur falschen Assembly: " + JoinLimited(wrong));
        }

        static bool IsEditorAssemblyName(string name)
        {
            return name == "UnityEditor" || name.StartsWith("UnityEditor.", StringComparison.Ordinal);
        }

        /// <summary>Sucht in C#-Dateien nach "UnityEditor", das nicht durch "#if UNITY_EDITOR" geschützt ist.</summary>
        static List<string> FindUnguardedEditorUsages(string folder)
        {
            var result = new List<string>();
            if (!Directory.Exists(folder)) return result;

            foreach (string file in Directory.GetFiles(folder, "*.cs", SearchOption.AllDirectories))
            {
                string[] lines = File.ReadAllLines(file);
                var branches = new Stack<PreprocessorBranch>();
                bool inBlockComment = false;
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();

                    if (line.StartsWith("#if", StringComparison.Ordinal))
                    {
                        branches.Push(new PreprocessorBranch(line.Substring(3)));
                        continue;
                    }
                    if (line.StartsWith("#elif", StringComparison.Ordinal))
                    {
                        if (branches.Count > 0) branches.Pop();
                        branches.Push(new PreprocessorBranch(line.Substring(5)));
                        continue;
                    }
                    if (line.StartsWith("#else", StringComparison.Ordinal))
                    {
                        if (branches.Count > 0)
                        {
                            PreprocessorBranch branch = branches.Pop();
                            branches.Push(branch.Else());
                        }
                        continue;
                    }
                    if (line.StartsWith("#endif", StringComparison.Ordinal))
                    {
                        if (branches.Count > 0) branches.Pop();
                        continue;
                    }

                    string code = StripComments(line, ref inBlockComment);
                    if (code.IndexOf("UnityEditor", StringComparison.Ordinal) < 0) continue;
                    if (branches.Any(branch => branch.EditorOnly)) continue;
                    result.Add(Path.GetFileName(file) + ":" + (i + 1));
                }
            }
            return result;
        }

        readonly struct PreprocessorBranch
        {
            readonly string condition;
            readonly bool isElse;

            public PreprocessorBranch(string condition) : this(condition, false)
            {
            }

            PreprocessorBranch(string condition, bool isElse)
            {
                this.condition = (condition ?? "").Trim();
                this.isElse = isElse;
            }

            public PreprocessorBranch Else() => new PreprocessorBranch(condition, true);

            /// <summary>true, wenn der Code in diesem Zweig nur im Editor übersetzt wird.</summary>
            public bool EditorOnly
            {
                get
                {
                    string c = condition.Replace(" ", "");
                    if (!isElse) return c.Contains("UNITY_EDITOR") && !c.Contains("!UNITY_EDITOR") && !c.Contains("||");
                    return c == "!UNITY_EDITOR";
                }
            }
        }

        static string StripComments(string line, ref bool inBlockComment)
        {
            var builder = new StringBuilder(line.Length);
            int i = 0;
            while (i < line.Length)
            {
                if (inBlockComment)
                {
                    int end = line.IndexOf("*/", i, StringComparison.Ordinal);
                    if (end < 0) return builder.ToString();
                    inBlockComment = false;
                    i = end + 2;
                    continue;
                }
                if (i + 1 < line.Length && line[i] == '/' && line[i + 1] == '/') break;
                if (i + 1 < line.Length && line[i] == '/' && line[i + 1] == '*')
                {
                    inBlockComment = true;
                    i += 2;
                    continue;
                }
                builder.Append(line[i]);
                i++;
            }
            return builder.ToString();
        }

        // ================================================================== Szene: Hierarchie

        static void CheckHierarchy(SceneIndex index, ValidationReport report)
        {
            var missing = new List<string>();
            var empty = new List<string>();
            foreach (string group in RootGroups)
            {
                GameObject root = index.Root(group);
                if (root == null) missing.Add(group);
                else if (root.transform.childCount == 0) empty.Add(group);
            }
            report.Check(missing.Count == 0, CatHierarchy,
                "Alle " + RootGroups.Length + " Hauptgruppen (00_WORLD_ROOT … 09_DECORATION) sind vorhanden.",
                "Hauptgruppen fehlen (oder liegen nicht ganz oben in der Hierarchy): " + JoinLimited(missing));
            report.Check(empty.Count == 0, CatHierarchy,
                "Alle Hauptgruppen enthalten Objekte.",
                "Diese Gruppen sind leer: " + JoinLimited(empty));

            var expectedCenters = new Dictionary<string, Vector3>
            {
                { "03_CREATIVE_ZONE", SandboxContext.CreativeCenter },
                { "04_PHYSICS_ZONE", SandboxContext.PhysicsCenter },
                { "05_LIGHT_ZONE", SandboxContext.LightCenter },
                { "06_INTERACTION_ZONE", SandboxContext.InteractionCenter },
            };
            var moved = new List<string>();
            foreach (KeyValuePair<string, Vector3> pair in expectedCenters)
            {
                GameObject root = index.Root(pair.Key);
                if (root == null) continue;
                Transform t = root.transform;
                if (Vector3.Distance(t.position, pair.Value) > 0.01f || Quaternion.Angle(t.rotation, Quaternion.identity) > 0.1f || (t.lossyScale - Vector3.one).sqrMagnitude > 0.0001f)
                    moved.Add(pair.Key);
            }
            report.Check(moved.Count == 0, CatHierarchy,
                "Die vier Bereiche stehen an ihrem Platz (nicht verschoben, gedreht oder skaliert).",
                "Diese Bereiche wurden verschoben, gedreht oder skaliert: " + JoinLimited(moved), null, CheckStatus.Warning);

            var broken = new List<GameObject>();
            foreach (GameObject go in index.All)
            {
                if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go) > 0) broken.Add(go);
            }
            report.Check(broken.Count == 0, CatHierarchy,
                "Keine fehlenden Skripte (Missing Script) in " + index.All.Count + " Objekten.",
                broken.Count + " Objekte mit fehlenden Skripten: " + JoinLimited(broken.Select(PathOf).ToList()),
                broken.Count > 0 ? broken[0] : null);

            int notes = index.Components<InfoNote>().Count;
            report.Check(notes >= 20, CatHierarchy,
                notes + " Erklär-Notizen (Info Note) in der Szene.",
                "Nur " + notes + " Erklär-Notizen (Info Note) in der Szene – wichtige Objekte sollten eine haben.", null, CheckStatus.Warning);
        }

        // ================================================================== Szene: Kamera

        static void CheckCamera(SceneIndex index, ValidationReport report)
        {
            List<Camera> mainCameras = index.Components<Camera>().Where(c => c.CompareTag("MainCamera")).ToList();
            if (mainCameras.Count == 0)
            {
                report.Fail(CatCamera, "Keine Kamera mit dem Tag 'MainCamera' gefunden.");
            }
            else
            {
                if (mainCameras.Count > 1)
                    report.Warn(CatCamera, mainCameras.Count + " Kameras haben das Tag 'MainCamera' – es sollte nur eine sein.", mainCameras[1]);

                Camera camera = mainCameras[0];
                bool fly = camera.GetComponent<FreeFlyCamera>() != null;
                bool click = camera.GetComponent<ClickInteractor>() != null;
                report.Check(fly && click, CatCamera,
                    "Main Camera hat Free Fly Camera und Click Interactor.",
                    "Der Main Camera fehlt: " + (fly ? "" : "Free Fly Camera ") + (click ? "" : "Click Interactor"), camera);

                FreeFlyCamera flyCamera = camera.GetComponent<FreeFlyCamera>();
                if (flyCamera != null)
                {
                    int assigned = flyCamera.Viewpoints != null ? flyCamera.Viewpoints.Count(v => v != null) : 0;
                    report.Check(assigned >= 5, CatCamera,
                        "Die Flug-Kamera kennt " + assigned + " Aussichtspunkte (Tasten 1–5).",
                        "Die Flug-Kamera kennt nur " + assigned + " Aussichtspunkte (Tasten 1–5 brauchen 5).", flyCamera, CheckStatus.Warning);
                }
            }

            List<Viewpoint> viewpoints = index.Components<Viewpoint>();
            report.Check(viewpoints.Count >= 5, CatCamera,
                viewpoints.Count + " Aussichtspunkte (Viewpoints) vorhanden.",
                "Nur " + viewpoints.Count + " Aussichtspunkte (mindestens 5 nötig).");

            string[] viewpointNames = { CameraSetup.OverviewName, CameraSetup.CreativeName, CameraSetup.PhysicsName, CameraSetup.LightName, CameraSetup.InteractionName };
            List<string> missingViewpoints = viewpointNames.Where(n => index.FindComponent<Viewpoint>(n) == null).ToList();
            report.Check(missingViewpoints.Count == 0, CatCamera,
                "Alle Aussichtspunkte für das Workshop-Fenster gefunden.",
                "Aussichtspunkte fehlen: " + JoinLimited(missingViewpoints));

            List<AudioListener> listeners = index.Components<AudioListener>().Where(l => l.enabled && l.gameObject.activeInHierarchy).ToList();
            report.Check(listeners.Count == 1, CatCamera,
                "Genau ein aktiver Audio Listener.",
                listeners.Count + " aktive Audio Listener gefunden (genau 1 ist richtig).", listeners.Count > 0 ? listeners[0] : null);
        }

        // ================================================================== Szene: Licht

        static void CheckLighting(SceneIndex index, ValidationReport report)
        {
            List<DayNightController> controllers = index.Components<DayNightController>();
            if (controllers.Count == 0)
            {
                report.Fail(CatLighting, "Kein DayNightController in der Szene.");
            }
            else
            {
                if (controllers.Count > 1) report.Warn(CatLighting, controllers.Count + " DayNightController gefunden – einer reicht.", controllers[1]);
                DayNightController dayNight = controllers[0];
                using (var serialized = new SerializedObject(dayNight))
                {
                    var sun = ObjectProperty(serialized, "sun") as Light;
                    if (sun == null) report.Fail(CatLighting, "Der DayNightController hat keine Sonne (Feld 'Sun' ist leer).", dayNight);
                    else if (sun.type != LightType.Directional) report.Fail(CatLighting, "Die Sonne '" + sun.name + "' ist kein Directional Light.", sun);
                    else
                    {
                        report.Pass(CatLighting, "Sonne: Directional Light '" + sun.name + "' ist mit dem DayNightController verbunden.", sun);
                        if (RenderSettings.sun != sun) report.Warn(CatLighting, "Die Lighting-Einstellungen benutzen eine andere Sonne als der DayNightController.", sun);
                        if (sun.shadows == LightShadows.None) report.Warn(CatLighting, "Die Sonne wirft keine Schatten (Shadow Type = No Shadows).", sun);
                    }

                    var missingSky = new List<string>();
                    foreach (string preset in new[] { "day", "sunset", "night" })
                    {
                        var sky = ObjectProperty(serialized, preset + ".skybox") as Material;
                        if (sky == null) missingSky.Add(preset);
                    }
                    report.Check(missingSky.Count == 0, CatLighting,
                        "Tag, Sonnenuntergang und Nacht haben je einen Himmel (Skybox).",
                        "Diesen Tageszeiten fehlt der Himmel: " + JoinLimited(missingSky), dayNight);

                    float dayIntensity = FloatProperty(serialized, "day.sunIntensity");
                    float nightIntensity = FloatProperty(serialized, "night.sunIntensity");
                    bool dayLamps = BoolProperty(serialized, "day.lampsOn");
                    bool nightLamps = BoolProperty(serialized, "night.lampsOn");
                    report.Check(nightIntensity < dayIntensity && nightLamps && !dayLamps, CatLighting,
                        "Nachts ist die Sonne dunkler als tagsüber und die Lampen gehen an.",
                        "Tag/Nacht-Einstellungen ungewöhnlich (Sonne Tag " + dayIntensity.ToString("0.00") + ", Nacht " + nightIntensity.ToString("0.00") +
                        ", Lampen Tag " + (dayLamps ? "an" : "aus") + ", Nacht " + (nightLamps ? "an" : "aus") + ").", dayNight, CheckStatus.Warning);
                }
            }

            if (RenderSettings.skybox == null) report.Warn(CatLighting, "In den Lighting-Einstellungen ist kein Himmel (Skybox) eingetragen.");

            List<SandboxHUD> huds = index.Components<SandboxHUD>();
            report.Check(huds.Count >= 1, CatLighting, "SandboxHUD vorhanden (Meldungen im Play Mode).", "Kein SandboxHUD in der Szene (keine Meldungen im Play Mode).");
            if (huds.Count > 1) report.Warn(CatLighting, huds.Count + " SandboxHUDs gefunden – einer reicht.", huds[1]);

            List<PhysicsResetter> resetters = index.Components<PhysicsResetter>();
            report.Check(resetters.Count >= 1, CatLighting, "PhysicsResetter vorhanden (Taste R).", "Kein PhysicsResetter in der Szene (Taste R funktioniert nicht).");

            List<LampController> lamps = index.Components<LampController>();
            report.Check(lamps.Count >= 6, CatLighting,
                lamps.Count + " Lampen mit Lamp Controller.",
                "Nur " + lamps.Count + " Lampen mit Lamp Controller (mindestens 6 erwartet).");
            var deadLamps = new List<string>();
            foreach (LampController lamp in lamps)
            {
                using (var serialized = new SerializedObject(lamp))
                {
                    bool hasLight = ObjectProperty(serialized, "lampLight") != null;
                    SerializedProperty renderers = serialized.FindProperty("glowRenderers");
                    bool hasGlow = false;
                    if (renderers != null && renderers.isArray)
                    {
                        for (int i = 0; i < renderers.arraySize; i++)
                        {
                            if (renderers.GetArrayElementAtIndex(i).objectReferenceValue != null) hasGlow = true;
                        }
                    }
                    if (!hasLight && !hasGlow) deadLamps.Add(PathOf(lamp.gameObject));
                }
            }
            if (deadLamps.Count > 0) report.Warn(CatLighting, "Lamp Controller ohne Licht und ohne leuchtende Teile: " + JoinLimited(deadLamps));

            List<Light> colorLights = index.Components<Light>()
                .Where(l => l.name.StartsWith("ColorLight_", StringComparison.Ordinal) && l.type == LightType.Point && IsColourful(l.color))
                .ToList();
            report.Check(colorLights.Count >= 3, CatLighting,
                colorLights.Count + " farbige Point Lights im Farben-Labor (" + string.Join(", ", colorLights.Select(l => l.name)) + ").",
                "Nur " + colorLights.Count + " farbige Point Lights mit dem Namen 'ColorLight_…' (mindestens 3 nötig).");

            Light spot = index.FindComponent<Light>("ShadowSpotLight");
            if (spot == null) report.Fail(CatLighting, "'ShadowSpotLight' (Light-Komponente) fehlt.");
            else
                report.Check(spot.type == LightType.Spot && spot.shadows != LightShadows.None, CatLighting,
                    "ShadowSpotLight ist ein Spot Light mit Schatten.",
                    "ShadowSpotLight muss ein Spot Light mit Schatten sein (jetzt: " + spot.type + ", Schatten " + spot.shadows + ").", spot);
        }

        static bool IsColourful(Color color)
        {
            Color.RGBToHSV(color, out _, out float saturation, out float value);
            return saturation > 0.3f && value > 0.2f;
        }

        // ================================================================== Szene: Böden

        static void CheckGround(SceneIndex index, ValidationReport report)
        {
            var issues = new List<string>();
            var heights = new List<string>();
            foreach (string zone in ZoneGroups)
            {
                GameObject root = index.Root(zone);
                if (root == null) continue; // schon als fehlende Gruppe gemeldet
                Transform floor = FindDescendant(root.transform, "Floor");
                if (floor == null)
                {
                    issues.Add(zone + ": kein Objekt 'Floor'");
                    continue;
                }
                if (!HasSolidCollider(floor.gameObject, false)) issues.Add(zone + "/Floor: kein (aktiver, fester) Collider");

                var floorRenderer = floor.GetComponent<Renderer>();
                if (floorRenderer != null && Mathf.Abs(floorRenderer.bounds.max.y - SandboxContext.ZoneFloorTop) > 0.03f)
                    heights.Add(zone + " (Oberkante " + floorRenderer.bounds.max.y.ToString("0.00") + ")");
            }
            report.Check(issues.Count == 0, CatPhysics,
                "Alle vier Bereiche haben einen Boden 'Floor' mit Collider.",
                "Böden: " + JoinLimited(issues));
            if (heights.Count > 0)
                report.Warn(CatPhysics, "Boden liegt nicht auf y = " + SandboxContext.ZoneFloorTop.ToString("0.0") + ": " + JoinLimited(heights));

            GameObject island = index.Find("Island_Grass");
            bool islandOk = island != null && HasSolidCollider(island, false);
            if (!islandOk)
            {
                GameObject environment = index.Root("01_ENVIRONMENT");
                if (environment != null)
                {
                    foreach (Transform t in environment.GetComponentsInChildren<Transform>(true))
                    {
                        if (t.name.StartsWith("Island", StringComparison.Ordinal) && HasSolidCollider(t.gameObject, false))
                        {
                            islandOk = true;
                            island = t.gameObject;
                            break;
                        }
                    }
                }
            }
            report.Check(islandOk, CatPhysics,
                "Der Inselboden hat einen Collider (" + (island != null ? island.name : "?") + ").",
                "Der Inselboden (Island_Grass) hat keinen Collider – Kugeln fallen hindurch.", island);
        }

        // ================================================================== Szene: Murmelbahn und Ziele

        static void CheckMarbleRun(SceneIndex index, ValidationReport report)
        {
            GoalZone mainGoal = index.FindComponent<GoalZone>("GoalZone");
            PhysicsBall ball = mainGoal != null && mainGoal.RequiredBall != null && mainGoal.RequiredBall.name == "PhysicsBall"
                ? mainGoal.RequiredBall
                : index.FindComponent<PhysicsBall>("PhysicsBall");
            GameObject ballObject = ball != null ? ball.gameObject : index.Find("PhysicsBall");
            GameObject ramp = index.Find("Ramp_Main");

            if (ballObject == null)
            {
                report.Fail(CatPhysics, "Das Objekt 'PhysicsBall' fehlt.");
            }
            else
            {
                var issues = new List<string>();
                var body = ballObject.GetComponent<Rigidbody>();
                if (body == null) issues.Add("kein Rigidbody");
                else
                {
                    if (body.isKinematic) issues.Add("Rigidbody ist kinematisch (Is Kinematic)");
                    if (body.mass <= 0f) issues.Add("Masse ist 0");
                    if (!body.useGravity) issues.Add("Schwerkraft (Use Gravity) ist aus");
                }
                var sphere = ballObject.GetComponent<SphereCollider>();
                if (sphere == null) issues.Add("kein Sphere Collider");
                else if (sphere.isTrigger || !sphere.enabled) issues.Add("Sphere Collider ist Trigger oder ausgeschaltet");
                if (ball == null) issues.Add("keine PhysicsBall-Komponente");

                report.Check(issues.Count == 0, CatPhysics,
                    "PhysicsBall: Rigidbody (" + (body != null ? body.mass.ToString("0.##") : "?") + " kg, Schwerkraft an), Sphere Collider und PhysicsBall-Skript.",
                    "PhysicsBall: " + string.Join(", ", issues), ballObject);

                if (index.FindAll("PhysicsBall").Count > 1)
                    report.Warn(CatPhysics, "Es gibt mehrere Objekte mit dem Namen 'PhysicsBall' – Workshop und Tests nehmen das erste.", ballObject);

                if (ramp != null && body != null && sphere != null)
                {
                    bool onRamp = RestsOn(ballObject.transform.position, ramp.transform, 1.2f);
                    report.Check(onRamp, CatPhysics,
                        "Die rote Kugel liegt oben auf der Rampe 'Ramp_Main'.",
                        "Unter der roten Kugel liegt nicht die Rampe 'Ramp_Main' – rollt sie wirklich los?", ballObject, CheckStatus.Warning);
                }
            }

            var ballIssues = new List<string>();
            foreach (PhysicsBall physicsBall in index.Components<PhysicsBall>())
            {
                var body = physicsBall.GetComponent<Rigidbody>();
                if (body == null || body.isKinematic || !body.useGravity) ballIssues.Add(physicsBall.name + " (Rigidbody)");
                else if (!HasSolidCollider(physicsBall.gameObject, true)) ballIssues.Add(physicsBall.name + " (Collider)");
            }
            report.Check(ballIssues.Count == 0, CatPhysics,
                "Alle " + index.Components<PhysicsBall>().Count + " Kugeln können rollen (Rigidbody mit Schwerkraft und Collider).",
                "Diese Kugeln können nicht richtig rollen: " + JoinLimited(ballIssues));

            if (ramp == null) report.Fail(CatPhysics, "Das Objekt 'Ramp_Main' fehlt.");
            else
            {
                int colliders = ramp.GetComponentsInChildren<Collider>(true).Count(c => !c.isTrigger && c.enabled);
                report.Check(colliders > 0, CatPhysics,
                    "Ramp_Main hat " + colliders + " feste Collider.",
                    "Ramp_Main hat keinen festen Collider – die Kugel fällt hindurch.", ramp);
            }

            List<GoalZone> goals = index.Components<GoalZone>();
            if (goals.Count == 0) report.Fail(CatPhysics, "Keine Zielzone (GoalZone) in der Szene.");
            var noTrigger = goals.Where(goal => !HasEnabledTrigger(goal.gameObject)).Select(goal => PathOf(goal.gameObject)).ToList();
            if (goals.Count > 0)
                report.Check(noTrigger.Count == 0, CatPhysics,
                    "Alle " + goals.Count + " Zielzonen haben einen Trigger-Collider.",
                    "Zielzonen ohne Trigger-Collider (Is Trigger): " + JoinLimited(noTrigger));

            if (mainGoal == null)
            {
                report.Fail(CatPhysics, "Die Haupt-Zielzone 'GoalZone' fehlt.");
            }
            else
            {
                if (report.Check(mainGoal.RequiredBall != null, CatPhysics,
                        "GoalZone wartet auf die Kugel '" + (mainGoal.RequiredBall != null ? mainGoal.RequiredBall.name : "") + "' (Required Ball).",
                        "Bei der GoalZone ist keine Kugel eingetragen (Required Ball ist leer).", mainGoal)
                    && ball != null && mainGoal.RequiredBall != ball)
                {
                    report.Warn(CatPhysics, "Die GoalZone wartet auf '" + mainGoal.RequiredBall.name + "', nicht auf die rote Kugel 'PhysicsBall'.", mainGoal);
                }

                using (var serialized = new SerializedObject(mainGoal))
                {
                    bool visuals = ObjectProperty(serialized, "indicator") != null && ObjectProperty(serialized, "idleMaterial") != null &&
                                   ObjectProperty(serialized, "reachedMaterial") != null;
                    report.Check(visuals, CatPhysics,
                        "GoalZone zeigt den Erfolg an (Farbe wechselt).",
                        "Bei der GoalZone fehlen Anzeige oder Materialien (Indicator, Idle/Reached Material).", mainGoal, CheckStatus.Warning);
                }
            }
        }

        // ================================================================== Szene: Dominos

        static void CheckDominoes(SceneIndex index, ValidationReport report)
        {
            List<GameObject> dominoes = index.All
                .Where(go => go.name.StartsWith("Domino_", StringComparison.Ordinal))
                .Where(go =>
                {
                    var body = go.GetComponent<Rigidbody>();
                    return body != null && !body.isKinematic;
                })
                .OrderBy(go => go.name, StringComparer.Ordinal)
                .ToList();
            report.Check(dominoes.Count >= 10, CatPhysics,
                dominoes.Count + " Dominosteine mit Rigidbody.",
                "Nur " + dominoes.Count + " Dominosteine mit (nicht kinematischem) Rigidbody – mindestens 10 nötig.");
            report.Check(index.Find("Domino_10") != null, CatPhysics,
                "Der letzte Dominostein 'Domino_10' ist da.",
                "Der Dominostein 'Domino_10' fehlt.");

            var gaps = new List<string>();
            for (int i = 1; i < dominoes.Count; i++)
            {
                float distance = Vector3.Distance(dominoes[i - 1].transform.position, dominoes[i].transform.position);
                if (distance > 1.0f) gaps.Add(dominoes[i - 1].name + " → " + dominoes[i].name + " (" + distance.ToString("0.00") + " m)");
            }
            if (dominoes.Count >= 2)
                report.Check(gaps.Count == 0, CatPhysics,
                    "Die Dominosteine stehen nah genug beieinander für eine Kettenreaktion.",
                    "Zu große Abstände – die Kette reißt ab: " + JoinLimited(gaps), dominoes[0], CheckStatus.Warning);

            DominoStarter starter = index.FindComponent<DominoStarter>("Hammer");
            GameObject hammer = starter != null ? starter.gameObject : index.Find("Hammer");
            if (hammer == null)
            {
                report.Fail(CatPhysics, "Der Domino-Hammer 'Hammer' fehlt.");
            }
            else
            {
                var body = hammer.GetComponent<Rigidbody>();
                bool ok = body != null && body.isKinematic && starter != null;
                report.Check(ok, CatPhysics,
                    "Hammer: kinematischer Rigidbody + DominoStarter.",
                    "Hammer braucht einen kinematischen Rigidbody und das Skript DominoStarter" +
                    (body == null ? " (Rigidbody fehlt)" : !body.isKinematic ? " (Is Kinematic ist aus)" : "") +
                    (starter == null ? " (DominoStarter fehlt)" : "") + ".", hammer);
            }
        }

        // ================================================================== Szene: Tür, Ampel, Kristall, Plattform

        static void CheckInteractions(SceneIndex index, ValidationReport report)
        {
            // Tür
            DoorController door = index.FindComponent<DoorController>("AutomaticDoor");
            if (door == null)
            {
                GameObject doorObject = index.Find("AutomaticDoor");
                report.Fail(CatInteraction, doorObject == null ? "Die Tür 'AutomaticDoor' fehlt." : "'AutomaticDoor' hat kein DoorController-Skript.", doorObject);
            }
            else
            {
                report.Pass(CatInteraction, "AutomaticDoor hat einen DoorController (" + door.Motion + ").", door);
                var allowed = new HashSet<Type> { typeof(Transform), typeof(MeshFilter), typeof(MeshRenderer), typeof(BoxCollider), typeof(DoorController), typeof(InfoNote) };
                List<string> extra = door.GetComponents<Component>().Where(c => c != null && !allowed.Contains(c.GetType())).Select(c => c.GetType().Name).ToList();
                bool simple = extra.Count == 0 && door.transform.childCount == 0;
                report.Check(simple, CatInteraction,
                    "Die Tür ist ein einzelnes, übersichtliches Objekt (Lern-Beispiel).",
                    "Die Tür sollte ein einzelnes Objekt sein (zusätzlich: " + (extra.Count > 0 ? string.Join(", ", extra) : "") +
                    (door.transform.childCount > 0 ? " " + door.transform.childCount + " Kind-Objekte" : "") + ").", door, CheckStatus.Warning);
            }

            ClickableSwitch doorSwitch = index.FindComponent<ClickableSwitch>("DoorSwitch");
            if (doorSwitch == null)
            {
                report.Fail(CatInteraction, "Der Schalter 'DoorSwitch' (ClickableSwitch) fehlt.");
            }
            else if (door != null)
            {
                bool wired = HasPersistentCall(doorSwitch.OnPressed, door, "Toggle");
                report.Check(wired, CatInteraction,
                    "DoorSwitch → AutomaticDoor.DoorController.Toggle ist verbunden (On Pressed).",
                    "Im Event 'On Pressed' von DoorSwitch fehlt: AutomaticDoor → DoorController.Toggle.", doorSwitch);
            }

            // Ampel
            List<TrafficLightController> trafficLights = index.Components<TrafficLightController>();
            if (trafficLights.Count == 0)
            {
                report.Fail(CatInteraction, "Keine Ampel (TrafficLightController) in der Szene.");
            }
            else
            {
                TrafficLightController trafficLight = index.FindComponent<TrafficLightController>("TrafficLight") ?? trafficLights[0];
                if (trafficLight.name != "TrafficLight")
                    report.Warn(CatInteraction, "Die Ampel heißt '" + trafficLight.name + "' statt 'TrafficLight'.", trafficLight);

                foreach (TrafficLightController controller in trafficLights) CheckTrafficLight(controller, report);

                var trafficSwitches = new[] { "PedestrianButton", "Desk_TrafficButton" }
                    .Select(n => index.FindComponent<ClickableSwitch>(n))
                    .Where(s => s != null && HasPersistentCall(s.OnPressed, trafficLight, "NextPhase"))
                    .Select(s => s.name)
                    .ToList();
                report.Check(trafficSwitches.Count > 0, CatInteraction,
                    "Ampel-Knopf verbunden: " + string.Join(", ", trafficSwitches) + " → NextPhase.",
                    "Weder 'PedestrianButton' noch 'Desk_TrafficButton' ruft TrafficLightController.NextPhase auf.", trafficLight);
            }

            // Kristall
            Rotator crystal = index.FindComponent<Rotator>("Crystal");
            report.Check(crystal != null, CatInteraction,
                "Der Kristall 'Crystal' dreht sich mit einem Rotator (Speed " + (crystal != null ? crystal.Speed.ToString("0") : "") + ").",
                "Kein Rotator auf dem Objekt 'Crystal'.", crystal);

            // Plattform
            List<MovingPlatform> platforms = index.Components<MovingPlatform>();
            if (platforms.Count == 0)
            {
                report.Fail(CatInteraction, "Keine fahrende Plattform (MovingPlatform) in der Szene.");
            }
            foreach (MovingPlatform platform in platforms)
            {
                var issues = new List<string>();
                Transform pointA = platform.PointA;
                Transform pointB = platform.PointB;
                if (pointA == null) issues.Add("Point A fehlt");
                else if (pointA.IsChildOf(platform.transform)) issues.Add("Point A ist ein Kind der Plattform (fährt mit)");
                if (pointB == null) issues.Add("Point B fehlt");
                else if (pointB.IsChildOf(platform.transform)) issues.Add("Point B ist ein Kind der Plattform (fährt mit)");
                report.Check(issues.Count == 0, CatInteraction,
                    "Plattform '" + platform.name + "': Point A und Point B sind eigene Objekte.",
                    "Plattform '" + platform.name + "': " + string.Join(", ", issues), platform);

                if (pointA != null && pointB != null && Vector3.Distance(pointA.position, pointB.position) < 0.5f)
                    report.Warn(CatInteraction, "Plattform '" + platform.name + "': Point A und Point B liegen fast aufeinander.", platform);
                var body = platform.GetComponent<Rigidbody>();
                if (body != null && !body.isKinematic)
                    report.Warn(CatInteraction, "Plattform '" + platform.name + "': Der Rigidbody sollte kinematisch sein.", platform);
            }
        }

        static void CheckTrafficLight(TrafficLightController controller, ValidationReport report)
        {
            using (var serialized = new SerializedObject(controller))
            {
                var red = ObjectProperty(serialized, "redLamp") as Renderer;
                var yellow = ObjectProperty(serialized, "yellowLamp") as Renderer;
                var green = ObjectProperty(serialized, "greenLamp") as Renderer;
                var lampIssues = new List<string>();
                if (red == null) lampIssues.Add("Red Lamp fehlt");
                if (yellow == null) lampIssues.Add("Yellow Lamp fehlt");
                if (green == null) lampIssues.Add("Green Lamp fehlt");
                if (red != null && yellow != null && green != null)
                {
                    var objects = new HashSet<GameObject> { red.gameObject, yellow.gameObject, green.gameObject };
                    if (objects.Count < 3) lampIssues.Add("zwei Lampen sind dasselbe Objekt");
                    if (objects.Contains(controller.gameObject)) lampIssues.Add("eine Lampe ist das Ampel-Objekt selbst");
                }
                report.Check(lampIssues.Count == 0, CatInteraction,
                    "Ampel '" + controller.name + "': drei eigene Leuchtkörper (" + (red != null ? red.name : "") + ", " + (yellow != null ? yellow.name : "") + ", " + (green != null ? green.name : "") + ").",
                    "Ampel '" + controller.name + "': " + string.Join(", ", lampIssues), controller);

                string[] materialFields = { "redOn", "redOff", "yellowOn", "yellowOff", "greenOn", "greenOff" };
                List<string> missingMaterials = materialFields.Where(field => ObjectProperty(serialized, field) == null).ToList();
                report.Check(missingMaterials.Count == 0, CatInteraction,
                    "Ampel '" + controller.name + "': alle 6 An/Aus-Materialien eingetragen.",
                    "Ampel '" + controller.name + "': Materialien fehlen: " + string.Join(", ", missingMaterials), controller);
            }

            int phases = controller.Phases != null ? controller.Phases.Count : 0;
            report.Check(phases >= 2, CatInteraction,
                "Ampel '" + controller.name + "': " + phases + " Phasen (" + string.Join(" → ", (controller.Phases ?? new List<TrafficLightPhase>()).Select(p => p.name)) + ").",
                "Ampel '" + controller.name + "' hat nur " + phases + " Phase(n) – mindestens 2 nötig.", controller);
        }

        // ================================================================== Szene: Schalter

        static void CheckSwitches(SceneIndex index, ValidationReport report)
        {
            List<ClickableSwitch> switches = index.Components<ClickableSwitch>();
            if (switches.Count == 0)
            {
                report.Fail(CatSwitches, "Keine anklickbaren Schalter (ClickableSwitch) in der Szene.");
                return;
            }

            var problems = new List<string>();
            var disabledCalls = new List<string>();
            var notClickable = new List<string>();
            ClickableSwitch firstProblem = null;
            foreach (ClickableSwitch clickable in switches)
            {
                UnityEvent pressed = clickable.OnPressed;
                int count = pressed != null ? pressed.GetPersistentEventCount() : 0;
                if (count == 0)
                {
                    problems.Add(clickable.name + ": keine Aktion in 'On Pressed'");
                    firstProblem = firstProblem ?? clickable;
                }

                for (int i = 0; i < count; i++)
                {
                    Object target = pressed.GetPersistentTarget(i);
                    string method = pressed.GetPersistentMethodName(i);
                    if (target == null) problems.Add(clickable.name + ": Eintrag " + (i + 1) + " hat kein Objekt");
                    else if (string.IsNullOrEmpty(method)) problems.Add(clickable.name + ": Eintrag " + (i + 1) + " hat keine Funktion");
                    else if (!PersistentMethodExists(clickable, i, target, method)) problems.Add(clickable.name + ": Funktion '" + method + "' gibt es nicht an " + target.GetType().Name);
                    else
                    {
                        if (pressed.GetPersistentListenerState(i) == UnityEventCallState.Off) disabledCalls.Add(clickable.name + " → " + method);
                        continue;
                    }
                    firstProblem = firstProblem ?? clickable;
                }

                if (!HasClickableCollider(clickable)) notClickable.Add(clickable.name);
            }

            report.Check(problems.Count == 0, CatSwitches,
                "Alle " + switches.Count + " Schalter haben in 'On Pressed' eine gültige Aktion.",
                "Schalter-Probleme: " + JoinLimited(problems), firstProblem);
            if (disabledCalls.Count > 0)
                report.Warn(CatSwitches, "Ausgeschaltete Einträge in 'On Pressed' (Off): " + JoinLimited(disabledCalls));
            report.Check(notClickable.Count == 0, CatSwitches,
                "Alle Schalter haben einen anklickbaren Collider.",
                "Diese Schalter kann man nicht anklicken (kein fester Collider): " + JoinLimited(notClickable));
        }

        static bool HasClickableCollider(ClickableSwitch clickable)
        {
            foreach (Collider collider in clickable.GetComponentsInChildren<Collider>(true))
            {
                if (collider.isTrigger || !collider.enabled) continue;
                IClickable owner = collider.GetComponentInParent<IClickable>(true);
                if (owner is Component component && component == clickable) return true;
            }
            return false;
        }

        static bool HasPersistentCall(UnityEventBase unityEvent, Object target, string methodName)
        {
            if (unityEvent == null || target == null) return false;
            int count = unityEvent.GetPersistentEventCount();
            for (int i = 0; i < count; i++)
            {
                if (unityEvent.GetPersistentTarget(i) == target && unityEvent.GetPersistentMethodName(i) == methodName) return true;
            }
            return false;
        }

        /// <summary>Prüft, ob die im Event eingetragene Funktion am Ziel-Objekt wirklich existiert (passend zum Parameter-Typ).</summary>
        static bool PersistentMethodExists(ClickableSwitch clickable, int callIndex, Object target, string methodName)
        {
            Type[] argumentTypes = Type.EmptyTypes;
            using (var serialized = new SerializedObject(clickable))
            {
                SerializedProperty calls = serialized.FindProperty("onPressed.m_PersistentCalls.m_Calls");
                if (calls != null && calls.isArray && callIndex < calls.arraySize)
                {
                    SerializedProperty call = calls.GetArrayElementAtIndex(callIndex);
                    SerializedProperty mode = call.FindPropertyRelative("m_Mode");
                    if (mode != null) argumentTypes = ArgumentTypesFor((PersistentListenerMode)mode.intValue, call);
                }
            }

            if (UnityEventBase.GetValidMethodInfo(target, methodName, argumentTypes) != null) return true;
            // Rückfall: irgendeine öffentliche Methode mit diesem Namen und höchstens einem Parameter.
            return target.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Any(m => m.Name == methodName && m.GetParameters().Length <= 1);
        }

        static Type[] ArgumentTypesFor(PersistentListenerMode mode, SerializedProperty call)
        {
            switch (mode)
            {
                case PersistentListenerMode.Int: return new[] { typeof(int) };
                case PersistentListenerMode.Float: return new[] { typeof(float) };
                case PersistentListenerMode.String: return new[] { typeof(string) };
                case PersistentListenerMode.Bool: return new[] { typeof(bool) };
                case PersistentListenerMode.Object:
                {
                    SerializedProperty typeName = call.FindPropertyRelative("m_Arguments.m_ObjectArgumentAssemblyTypeName");
                    Type type = typeName != null && !string.IsNullOrEmpty(typeName.stringValue) ? Type.GetType(typeName.stringValue, false) : null;
                    return new[] { type ?? typeof(Object) };
                }
                default: return Type.EmptyTypes;
            }
        }

        // ================================================================== Szene: Darstellung

        static void CheckRenderers(SceneIndex index, ValidationReport report)
        {
            List<Renderer> renderers = index.Components<Renderer>();
            var broken = new List<string>();
            var unsupportedWithoutGpu = new List<string>();
            Renderer firstBroken = null;
            foreach (Renderer renderer in renderers)
            {
                Material[] materials = renderer.sharedMaterials;
                string problem = null;
                bool onlyBecauseNoGpu = false;
                if (materials == null || materials.Length == 0) problem = "kein Material";
                else
                {
                    foreach (Material material in materials)
                    {
                        if (material == null)
                        {
                            problem = "leerer Material-Platz (Missing)";
                            break;
                        }
                        problem = ShaderProblem(material.shader, out onlyBecauseNoGpu);
                        if (problem != null) break;
                    }
                }
                if (problem == null) continue;
                if (onlyBecauseNoGpu)
                {
                    unsupportedWithoutGpu.Add(renderer.name);
                    continue;
                }
                broken.Add(PathOf(renderer.gameObject) + " (" + problem + ")");
                firstBroken = firstBroken ?? renderer;
            }
            report.Check(broken.Count == 0, CatRendering,
                "Alle " + renderers.Count + " Renderer haben gültige Materialien.",
                broken.Count + " Renderer mit fehlendem oder kaputtem Material (pink): " + JoinLimited(broken), firstBroken);
            if (unsupportedWithoutGpu.Count > 0)
                report.Warn(CatRendering, "Ohne Grafikkarte (-nographics) nicht prüfbar: " + unsupportedWithoutGpu.Count + " Renderer.");
        }

        // ================================================================== Szene: Namen

        static void CheckNaming(SceneIndex index, ValidationReport report)
        {
            List<GameObject> generic = index.All
                .Where(go => GenericNamePattern.IsMatch(go.name) && !NameComesFromPrefab(go))
                .ToList();
            report.Check(generic.Count == 0, CatNaming,
                "Alle Objekte haben sprechende Namen (keine 'Cube (3)').",
                generic.Count + " Objekte mit Standard-Namen – gib ihnen sprechende Namen wie 'Tree_Oak_1': " +
                JoinLimited(generic.Select(PathOf).ToList()),
                generic.Count > 0 ? generic[0] : null, CheckStatus.Warning);
        }

        /// <summary>true, wenn der Name aus einem Prefab stammt (z.B. das Kind "Cube" im Prefab ColoredCube).</summary>
        static bool NameComesFromPrefab(GameObject go)
        {
            if (!PrefabUtility.IsPartOfPrefabInstance(go)) return false;
            if (PrefabUtility.IsAddedGameObjectOverride(go)) return false;
            GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(go);
            return source != null && source.name == go.name;
        }

        static void CheckWorkshopObjects(SceneIndex index, ValidationReport report)
        {
            var missing = new List<string>();
            var duplicates = new List<string>();
            foreach (string objectName in WorkshopObjects)
            {
                int count = index.FindAll(objectName).Count;
                if (count == 0) missing.Add(objectName);
                else if (count > 1) duplicates.Add(objectName + " (" + count + "×)");
            }
            report.Check(missing.Count == 0, CatWorkshop,
                "Alle " + WorkshopObjects.Length + " Objekte, die das Workshop-Fenster zeigt, sind vorhanden.",
                "Diese Objekte findet das Workshop-Fenster nicht: " + JoinLimited(missing), null, CheckStatus.Warning);
            if (duplicates.Count > 0)
                report.Warn(CatWorkshop, "Diese Namen gibt es mehrfach ('Zeig mir' nimmt das erste): " + JoinLimited(duplicates));
        }

        // ================================================================== Hilfen

        static void Run(ValidationReport report, string category, Action check)
        {
            try
            {
                check();
            }
            catch (Exception exception)
            {
                report.Fail(category, "Prüfung abgebrochen (" + exception.GetType().Name + "): " + exception.Message);
            }
        }

        static void LogReport(ValidationReport report)
        {
            Debug.Log("[Student Sandbox] Prüfung: " + report.Failures + " Fehler, " + report.Warnings + " Warnungen, " + report.Passes + " OK.\n" + report.ToText());
            foreach (CheckResult result in report.Results)
            {
                if (result.Status == CheckStatus.Fail) Debug.LogError("[Student Sandbox] FEHLER – " + result.Category + ": " + result.Message, result.Context);
                else if (result.Status == CheckStatus.Warning) Debug.LogWarning("[Student Sandbox] Warnung – " + result.Category + ": " + result.Message, result.Context);
            }
        }

        static void ShowSummaryDialog(ValidationReport report)
        {
            var message = new StringBuilder();
            if (report.Failures == 0 && report.Warnings == 0) message.AppendLine("Alles in Ordnung! Die Spielwiese ist vollständig.");
            else if (report.Failures == 0) message.AppendLine("Keine Fehler – aber ein paar Hinweise.");
            else message.AppendLine("Es wurden Fehler gefunden.");
            message.AppendLine();
            message.AppendLine("OK: " + report.Passes);
            message.AppendLine("Warnungen: " + report.Warnings);
            message.AppendLine("Fehler: " + report.Failures);

            List<CheckResult> problems = report.Problems.OrderBy(r => r.Status == CheckStatus.Fail ? 0 : 1).ToList();
            if (problems.Count > 0)
            {
                message.AppendLine();
                foreach (CheckResult result in problems.Take(8)) message.AppendLine("• " + Shorten(result.ToString(), 160));
                if (problems.Count > 8) message.AppendLine("… und " + (problems.Count - 8) + " weitere.");
            }
            message.AppendLine();
            message.Append("Alle Details stehen in der Console.");

            bool ok = EditorUtility.DisplayDialog("Student Sandbox – Prüfung", message.ToString(), "OK", "Bericht kopieren");
            if (!ok)
            {
                EditorGUIUtility.systemCopyBuffer = report.ToText();
                Debug.Log("[Student Sandbox] Prüfbericht in die Zwischenablage kopiert.");
            }
        }

        static string GetCommandLineArgument(string name)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase)) return args[i + 1];
            }
            return null;
        }

        /// <summary>Alle string-Konstanten einer Klasse wie <see cref="MK"/>, <see cref="P"/> oder <see cref="PM"/>.</summary>
        static List<string> ConstStrings(Type type)
        {
            return type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(field => field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
                .Select(field => (string)field.GetRawConstantValue())
                .ToList();
        }

        static int CountMissingScripts(GameObject root)
        {
            int missing = 0;
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                missing += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
            return missing;
        }

        static bool NoGraphicsDevice => SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;

        /// <summary>Beschreibt, was mit einem Shader nicht stimmt (null = alles gut).</summary>
        static string ShaderProblem(Shader shader, out bool onlyBecauseNoGpu)
        {
            onlyBecauseNoGpu = false;
            if (shader == null) return "kein Shader";
            if (shader.name == "Hidden/InternalErrorShader") return "Fehler-Shader";
            if (!shader.isSupported)
            {
                onlyBecauseNoGpu = NoGraphicsDevice;
                return "Shader '" + shader.name + "' wird nicht unterstützt";
            }
            return null;
        }

        static bool HasEnabledTrigger(GameObject go)
        {
            foreach (Collider collider in go.GetComponents<Collider>())
            {
                if (collider.isTrigger && collider.enabled) return true;
            }
            return false;
        }

        static bool HasSolidCollider(GameObject go, bool includeChildren)
        {
            Collider[] colliders = includeChildren ? go.GetComponentsInChildren<Collider>(true) : go.GetComponents<Collider>();
            return colliders.Any(c => c.enabled && !c.isTrigger);
        }

        /// <summary>Schaut mit einem Strahl nach unten, ob unter dem Punkt ein Collider von <paramref name="support"/> liegt.</summary>
        static bool RestsOn(Vector3 position, Transform support, float maxDistance)
        {
            Physics.SyncTransforms();
            RaycastHit[] hits = Physics.RaycastAll(position, Vector3.down, maxDistance, ~0, QueryTriggerInteraction.Ignore);
            return hits.Any(hit => hit.collider != null && hit.collider.transform.IsChildOf(support));
        }

        static Object ObjectProperty(SerializedObject serialized, string path)
        {
            SerializedProperty property = serialized.FindProperty(path);
            return property != null && property.propertyType == SerializedPropertyType.ObjectReference ? property.objectReferenceValue : null;
        }

        static float FloatProperty(SerializedObject serialized, string path)
        {
            SerializedProperty property = serialized.FindProperty(path);
            return property != null && property.propertyType == SerializedPropertyType.Float ? property.floatValue : 0f;
        }

        static bool BoolProperty(SerializedObject serialized, string path)
        {
            SerializedProperty property = serialized.FindProperty(path);
            return property != null && property.propertyType == SerializedPropertyType.Boolean && property.boolValue;
        }

        static Transform FindDescendant(Transform parent, string objectName)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.name == objectName) return child;
            }
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform found = FindDescendant(parent.GetChild(i), objectName);
                if (found != null) return found;
            }
            return null;
        }

        static string PathOf(GameObject go)
        {
            if (go == null) return "?";
            var parts = new List<string>();
            Transform t = go.transform;
            while (t != null && parts.Count < 4)
            {
                parts.Add(t.name);
                t = t.parent;
            }
            parts.Reverse();
            return (t != null ? "…/" : "") + string.Join("/", parts);
        }

        static string JoinLimited(IList<string> items)
        {
            if (items == null || items.Count == 0) return "-";
            string joined = string.Join(", ", items.Take(MaxListedNames));
            return items.Count > MaxListedNames ? joined + " … (+" + (items.Count - MaxListedNames) + " weitere)" : joined;
        }

        static string Shorten(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength) return text;
            return text.Substring(0, maxLength - 1) + "…";
        }

        // ================================================================== Szenen-Index

        /// <summary>Alle Objekte der Szene einmal eingesammelt (auch inaktive), damit die Prüfungen schnell suchen können.</summary>
        sealed class SceneIndex
        {
            public readonly List<GameObject> Roots = new List<GameObject>();
            public readonly List<GameObject> All = new List<GameObject>();
            readonly Dictionary<string, List<GameObject>> byName = new Dictionary<string, List<GameObject>>();

            public SceneIndex(Scene scene)
            {
                Roots.AddRange(scene.GetRootGameObjects());
                foreach (GameObject root in Roots)
                {
                    foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                    {
                        GameObject go = t.gameObject;
                        All.Add(go);
                        if (!byName.TryGetValue(go.name, out List<GameObject> list))
                        {
                            list = new List<GameObject>();
                            byName.Add(go.name, list);
                        }
                        list.Add(go);
                    }
                }
            }

            public GameObject Root(string objectName) => Roots.FirstOrDefault(root => root.name == objectName);

            public GameObject Find(string objectName)
            {
                return byName.TryGetValue(objectName, out List<GameObject> list) && list.Count > 0 ? list[0] : null;
            }

            public List<GameObject> FindAll(string objectName)
            {
                return byName.TryGetValue(objectName, out List<GameObject> list) ? list : new List<GameObject>();
            }

            /// <summary>Erste Komponente T auf einem Objekt mit diesem Namen (es kann mehrere gleichnamige Objekte geben).</summary>
            public T FindComponent<T>(string objectName) where T : Component
            {
                foreach (GameObject go in FindAll(objectName))
                {
                    var component = go.GetComponent<T>();
                    if (component != null) return component;
                }
                return null;
            }

            public List<T> Components<T>() where T : Component
            {
                var result = new List<T>();
                foreach (GameObject root in Roots) result.AddRange(root.GetComponentsInChildren<T>(true));
                return result;
            }
        }
    }
}
