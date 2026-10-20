using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using StudentSandbox.EditorTools;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace StudentSandbox.Tests
{
    /// <summary>
    /// Edit-Mode-Tests für die generierte Spielwiese.
    /// Die Szene wird geöffnet, aber NIE gespeichert. Am Ende wird sie neu von der Festplatte geladen,
    /// damit keine Test-Änderungen übrig bleiben.
    /// </summary>
    [TestFixture]
    public class SandboxEditModeTests
    {
        static readonly string[] KeyObjects = { "PhysicsBall", "AutomaticDoor", "TrafficLight", "DayNightController" };

        [OneTimeSetUp]
        public void OpenSandboxScene()
        {
            if (!File.Exists(SandboxPaths.ScenePath))
                Assert.Ignore("Die Spielwiese wurde noch nicht erzeugt (" + SandboxPaths.ScenePath + "). Bitte 'Tools > Student Sandbox > Generate Sandbox' ausführen.");

            // Ungespeicherte Arbeit nie stillschweigend verwerfen: vorher fragen (nur interaktiv, nie im Batchmode).
            if (!Application.isBatchMode && HasUnsavedScenes() && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                Assert.Ignore("Abgebrochen: Es gibt ungespeicherte Änderungen in der offenen Szene.");

            EditorSceneManager.OpenScene(SandboxPaths.ScenePath, OpenSceneMode.Single);
        }

        [OneTimeTearDown]
        public void DiscardTestChanges()
        {
            // Neu von der Festplatte laden = alle Änderungen der Tests verwerfen (es wird nichts gespeichert).
            if (File.Exists(SandboxPaths.ScenePath) && !EditorApplication.isPlayingOrWillChangePlaymode)
                EditorSceneManager.OpenScene(SandboxPaths.ScenePath, OpenSceneMode.Single);
        }

        [SetUp]
        public void EnsureSandboxSceneIsActive()
        {
            Scene active = SceneManager.GetActiveScene();
            if (!active.IsValid() || active.path != SandboxPaths.ScenePath)
                EditorSceneManager.OpenScene(SandboxPaths.ScenePath, OpenSceneMode.Single);
        }

        // ------------------------------------------------------------------ A: Szene

        [Test]
        public void SandboxSceneOpens()
        {
            Scene scene = SceneManager.GetActiveScene();
            Assert.IsTrue(scene.IsValid(), "Die Szene ist nicht gültig.");
            Assert.IsTrue(scene.isLoaded, "Die Szene ist nicht geladen.");
            Assert.AreEqual(SandboxPaths.ScenePath, scene.path);

            Dictionary<string, GameObject> roots = scene.GetRootGameObjects()
                .GroupBy(root => root.name)
                .ToDictionary(group => group.Key, group => group.First());
            foreach (string group in SandboxValidator.RootGroups)
            {
                Assert.IsTrue(roots.ContainsKey(group), "Hauptgruppe fehlt: " + group);
                Assert.Greater(roots[group].transform.childCount, 0, "Hauptgruppe ist leer: " + group);
            }
        }

        [Test]
        public void ValidatorReportsNoFailures()
        {
            // Frisch von der Festplatte laden, damit genau der gespeicherte Zustand geprüft wird.
            EditorSceneManager.OpenScene(SandboxPaths.ScenePath, OpenSceneMode.Single);

            ValidationReport report = SandboxValidator.ValidateAssets();
            report.Merge(SandboxValidator.ValidateOpenScene());
            string text = report.ToText();
            TestContext.WriteLine(text);
            Debug.Log("[Student Sandbox] " + report.Failures + " Fehler, " + report.Warnings + " Warnungen.\n" + text);

            List<string> failures = report.Results.Where(r => r.Status == CheckStatus.Fail).Select(r => r.ToString()).ToList();
            Assert.AreEqual(0, report.Failures, "Der Validator hat Fehler gefunden:\n" + string.Join("\n", failures));
            Assert.Greater(report.Passes, 20, "Es wurden auffällig wenige Prüfungen ausgeführt.");
        }

        // ------------------------------------------------------------------ B: Objekt bewegen

        [Test]
        public void TreeCanBeMovedInEditMode()
        {
            GameObject tree = SandboxSceneView.FindInScene("Tree_Small_1");
            Assert.IsNotNull(tree, "Der Baum 'Tree_Small_1' fehlt im Baubereich.");

            Transform t = tree.transform;
            Vector3 originalPosition = t.position;
            Transform child = t.childCount > 0 ? t.GetChild(0) : null;
            Vector3 childOriginal = child != null ? child.position : Vector3.zero;
            var offset = new Vector3(1.5f, 0f, -1f);

            // So wie die Schülerin mit dem Verschieben-Werkzeug (W): eine Änderung mit Undo-Eintrag.
            Undo.RecordObject(t, "Baum verschieben (Test)");
            try
            {
                t.position = originalPosition + offset;
                Assert.Less(Vector3.Distance(t.position, originalPosition + offset), 0.0001f, "Die Position des Baums hat sich nicht geändert.");
                if (child != null)
                    Assert.Less(Vector3.Distance(child.position, childOriginal + offset), 0.0001f, "Die Kind-Objekte (Stamm/Krone) sind nicht mitgewandert.");
            }
            finally
            {
                t.position = originalPosition;
                Undo.ClearUndo(t);
            }

            Assert.Less(Vector3.Distance(t.position, originalPosition), 0.0001f, "Der Baum steht nicht wieder an seinem Platz.");
        }

        // ------------------------------------------------------------------ C: Material ändern

        [Test]
        public void MaterialChangeIsApplied()
        {
            GameObject house = SandboxSceneView.FindInScene("House_Red");
            Assert.IsNotNull(house, "Das Haus 'House_Red' fehlt im Baubereich.");

            Renderer wall = FindRenderer(house, "Walls");
            Assert.IsNotNull(wall, "Das Haus 'House_Red' hat keinen Wand-Renderer ('Walls').");

            Material skyBlue = SandboxMaterials.Get(M.SkyBlue);
            Assert.IsNotNull(skyBlue, "Das Palette-Material SkyBlue fehlt.");

            Material[] original = wall.sharedMaterials;
            try
            {
                wall.sharedMaterial = skyBlue;
                Assert.AreSame(skyBlue, wall.sharedMaterial, "Das neue Material wurde nicht übernommen.");
            }
            finally
            {
                wall.sharedMaterials = original;
            }
            Assert.AreSame(original[0], wall.sharedMaterial, "Das ursprüngliche Material wurde nicht wiederhergestellt.");
        }

        // ------------------------------------------------------------------ H: Tag und Nacht

        [Test]
        public void DayNightSwitchChangesLighting()
        {
            DayNightController dayNight = Object.FindAnyObjectByType<DayNightController>();
            Assert.IsNotNull(dayNight, "Kein DayNightController in der Szene.");
            Light sun = dayNight.Sun;
            Assert.IsNotNull(sun, "Der DayNightController hat keine Sonne.");
            Assert.IsNotNull(dayNight.Night.skybox, "Die Nacht hat keinen Himmel (Skybox).");
            Assert.IsNotNull(dayNight.Day.skybox, "Der Tag hat keinen Himmel (Skybox).");

            List<LampController> lamps = Object.FindObjectsByType<LampController>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(lamp => lamp.FollowDayNight)
                .ToList();
            Assert.IsNotEmpty(lamps, "Keine Lampe folgt der Tageszeit (Follow Day Night).");

            TimeOfDay originalTime = dayNight.CurrentTime;
            try
            {
                dayNight.Apply(TimeOfDay.Night);
                Assert.AreSame(dayNight.Night.skybox, RenderSettings.skybox, "Nachts wurde der Himmel nicht gewechselt.");
                Assert.AreEqual(dayNight.Night.sunIntensity, sun.intensity, 0.0001f, "Nachts hat die Sonne die falsche Helligkeit.");
                LampController litLamp = lamps.FirstOrDefault(lamp => lamp.IsOn);
                Assert.IsNotNull(litLamp, "Nachts ist keine Lampe angegangen.");
                if (litLamp.LampLight != null) Assert.IsTrue(litLamp.LampLight.enabled, "Die Lampe ist an, aber ihr Licht nicht: " + litLamp.name);

                dayNight.Apply(TimeOfDay.Day);
                Assert.AreSame(dayNight.Day.skybox, RenderSettings.skybox, "Tagsüber wurde der Himmel nicht zurückgestellt.");
                Assert.AreEqual(dayNight.Day.sunIntensity, sun.intensity, 0.0001f, "Tagsüber hat die Sonne die falsche Helligkeit.");
                List<string> stillOn = lamps.Where(lamp => lamp.IsOn).Select(lamp => lamp.name).ToList();
                Assert.IsEmpty(stillOn, "Tagsüber leuchten noch Lampen: " + string.Join(", ", stillOn));
            }
            finally
            {
                dayNight.Apply(originalTime);
            }
        }

        // ------------------------------------------------------------------ L: Speichern und Laden

        [Test]
        public void SceneSurvivesReopen()
        {
            int before = CountGameObjects(SceneManager.GetActiveScene());

            Scene reopened = EditorSceneManager.OpenScene(SandboxPaths.ScenePath, OpenSceneMode.Single);
            Assert.IsTrue(reopened.IsValid() && reopened.isLoaded, "Die Szene ließ sich nicht erneut öffnen.");
            int after = CountGameObjects(reopened);

            Assert.AreEqual(before, after, "Nach dem erneuten Öffnen hat die Szene eine andere Anzahl an Objekten.");
            Assert.Greater(after, 300, "Die Szene hat auffällig wenige Objekte (" + after + ").");
            foreach (string key in KeyObjects)
                Assert.IsNotNull(SandboxSceneView.FindInScene(key), "Wichtiges Objekt fehlt nach dem Öffnen: " + key);
        }

        // ------------------------------------------------------------------ Assets

        [Test]
        public void AllPrefabsExist()
        {
            List<string> names = SandboxPrefabs.AllNames.ToList();
            List<string> constants = typeof(P).GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(field => field.IsLiteral && field.FieldType == typeof(string))
                .Select(field => (string)field.GetRawConstantValue())
                .ToList();
            Assert.IsNotEmpty(constants, "Die Liste P enthält keine Prefabs.");
            CollectionAssert.IsSubsetOf(constants, names, "Nicht jedes Prefab aus P ist in SandboxPrefabs eingetragen.");

            var missing = new List<string>();
            var broken = new List<string>();
            foreach (string prefabName in names)
            {
                string path = SandboxPrefabs.PathFor(prefabName);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    missing.Add(path);
                    continue;
                }
                int missingScripts = prefab.GetComponentsInChildren<Transform>(true)
                    .Sum(t => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject));
                if (missingScripts > 0) broken.Add(prefabName + " (" + missingScripts + " fehlende Skripte)");
            }
            Assert.IsEmpty(missing, "Prefabs fehlen:\n" + string.Join("\n", missing));
            Assert.IsEmpty(broken, "Prefabs mit fehlenden Skripten:\n" + string.Join("\n", broken));
        }

        [Test]
        public void PaletteHasAtLeastTenMaterials()
        {
            Assert.IsTrue(AssetDatabase.IsValidFolder(SandboxPaths.MaterialsPalette), "Der Ordner fehlt: " + SandboxPaths.MaterialsPalette);
            string[] guids = AssetDatabase.FindAssets("t:Material", new[] { SandboxPaths.MaterialsPalette });
            Assert.GreaterOrEqual(guids.Length, 10, "Im Ordner Materials/Palette liegen nur " + guids.Length + " Materialien.");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                Assert.IsNotNull(material, "Material lässt sich nicht laden: " + path);
                Assert.IsNotNull(material.shader, "Material ohne Shader: " + path);
                Assert.AreNotEqual("Hidden/InternalErrorShader", material.shader.name, "Material mit Fehler-Shader: " + path);
            }

            foreach (string key in SandboxMaterials.PaletteKeys)
            {
                string path = SandboxPaths.MaterialsPalette + "/" + key + ".mat";
                Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<Material>(path), "Palette-Material fehlt: " + path);
            }
        }

        // ------------------------------------------------------------------ Hilfen

        static bool HasUnsavedScenes()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i).isDirty) return true;
            }
            return false;
        }

        static int CountGameObjects(Scene scene)
        {
            int count = 0;
            foreach (GameObject root in scene.GetRootGameObjects()) count += root.GetComponentsInChildren<Transform>(true).Length;
            return count;
        }

        static Renderer FindRenderer(GameObject root, string childName)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            Renderer exact = renderers.FirstOrDefault(r => r.name == childName);
            if (exact != null) return exact;
            return renderers.FirstOrDefault(r => r.name.IndexOf("Wall", System.StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }
}
