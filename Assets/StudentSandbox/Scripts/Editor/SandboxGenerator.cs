using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StudentSandbox.EditorTools
{
    /// <summary>
    /// Erzeugt die komplette Spielwiese: Ordner, Materialien, Meshes, Prefabs, Szene.
    /// Menü: Tools > Student Sandbox > Generate Sandbox
    ///
    /// Sicherheitsregeln:
    ///  - Es wird ausschließlich unter Assets/StudentSandbox geschrieben.
    ///  - Existiert die Szene schon, wird erst nachgefragt und vor dem Neu-Erzeugen eine Sicherung angelegt.
    ///  - Zusätzlich wird nach jeder Erzeugung eine unberührte Kopie (Scenes/Original) gespeichert.
    /// </summary>
    public static class SandboxGenerator
    {
        public const string MenuRoot = "Tools/Student Sandbox/";

        // ------------------------------------------------------------------ Menü

        [MenuItem(MenuRoot + "Generate Sandbox", priority = 20)]
        public static void GenerateFromMenu()
        {
            GenerateInteractive();
        }

        [MenuItem(MenuRoot + "Open Sandbox Scene", priority = 21)]
        public static void OpenSandboxSceneFromMenu()
        {
            OpenSandboxScene(true);
        }

        [MenuItem(MenuRoot + "Restore Original Scene", priority = 22)]
        public static void RestoreOriginalFromMenu()
        {
            RestoreOriginalScene();
        }

        [MenuItem(MenuRoot + "Create Backup of Scene", priority = 23)]
        public static void BackupFromMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Student Sandbox", "Bitte zuerst den Play Mode beenden.", "OK");
                return;
            }
            if (!File.Exists(SandboxPaths.ScenePath))
            {
                EditorUtility.DisplayDialog("Student Sandbox", "Es gibt noch keine Spielwiesen-Szene. Bitte zuerst 'Generate Sandbox' ausführen.", "OK");
                return;
            }
            if (SandboxSceneView.IsSandboxSceneOpen && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            string backup = BackupScene();
            if (backup != null) EditorUtility.DisplayDialog("Student Sandbox", "Sicherung gespeichert:\n" + backup, "OK");
        }

        // ------------------------------------------------------------------ Öffentliche API

        public static bool SceneExists => File.Exists(SandboxPaths.ScenePath);

        /// <summary>Erzeugen mit Rückfragen (für das Menü und das Workshop-Fenster).</summary>
        public static bool GenerateInteractive()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Student Sandbox", "Bitte zuerst den Play Mode beenden.", "OK");
                return false;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return false;

            bool makeBackup = false;
            if (SceneExists)
            {
                int choice = EditorUtility.DisplayDialogComplex(
                    "Spielwiese existiert bereits",
                    "Die Szene 'StudentSandbox' gibt es schon.\n\n" +
                    "Beim Neu-Erzeugen wird die aktuelle Szene ZUERST als Sicherung im Ordner 'StudentSandbox/Backups' gespeichert.\n" +
                    "Generierte Materialien und Prefabs werden auf den Ausgangszustand zurückgesetzt.\n\n" +
                    "Was möchtest du tun?",
                    "Sicherung anlegen und neu erzeugen",
                    "Abbrechen",
                    "Nur Szene öffnen");
                if (choice == 1) return false;
                if (choice == 2)
                {
                    // Die Speichern-Frage wurde oben schon beantwortet.
                    OpenSandboxScene(false);
                    return false;
                }
                makeBackup = true;
            }

            bool ok = Generate(makeBackup, true);
            if (ok)
            {
                EditorUtility.DisplayDialog("Student Sandbox",
                    "Fertig! Die Spielwiese wurde erzeugt und gespeichert:\n" + SandboxPaths.ScenePath +
                    "\n\nTipp: Öffne jetzt Tools > Student Sandbox > Workshop.", "Los geht's");
            }
            return ok;
        }

        /// <summary>
        /// Für den Batchmode (Kommandozeile):
        /// Unity.exe -batchmode -projectPath . -executeMethod StudentSandbox.EditorTools.SandboxGenerator.GenerateBatch -quit
        /// Im Batchmode ist git die Sicherung – eine Szenen-Sicherung in Backups/ wird nur mit -sandboxBackup angelegt.
        /// </summary>
        public static void GenerateBatch()
        {
            bool backup = SceneExists && Array.IndexOf(Environment.GetCommandLineArgs(), "-sandboxBackup") >= 0;
            bool ok = Generate(backup, false);
            if (Application.isBatchMode) EditorApplication.Exit(ok ? 0 : 1);
        }

        /// <summary>Erzeugt alles neu und speichert die Szene. Gibt true zurück, wenn alles geklappt hat.</summary>
        public static bool Generate(bool makeBackupOfExistingScene, bool showProgress)
        {
            bool newSceneCreated = false;
            try
            {
                Progress(showProgress, "Ordner anlegen", 0.02f);
                SandboxPaths.EnsureAllFolders();

                if (makeBackupOfExistingScene && SceneExists)
                {
                    Progress(showProgress, "Sicherung der alten Szene", 0.04f);
                    if (BackupScene() == null) throw new InvalidOperationException("Sicherung konnte nicht angelegt werden – Abbruch, um nichts zu verlieren.");
                }

                Progress(showProgress, "Materialien", 0.08f);
                SandboxMaterials.CreateAll();
                Progress(showProgress, "Low-Poly-Meshes", 0.14f);
                SandboxMeshes.CreateAll();

                Progress(showProgress, "Neue Szene", 0.18f);
                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                newSceneCreated = true;

                Progress(showProgress, "Prefabs", 0.22f);
                SandboxPrefabs.CreateAll();

                var ctx = new SandboxContext(scene);
                ctx.CreateRootGroups();

                Progress(showProgress, "Licht und Himmel", 0.3f);
                LightingSetup.Build(ctx);
                Progress(showProgress, "Insel, Wasser und Wege", 0.36f);
                WorldBuilder.BuildEnvironment(ctx);
                Progress(showProgress, "Zentraler Platz", 0.42f);
                WorldBuilder.BuildHub(ctx);
                Progress(showProgress, "Baubereich", 0.5f);
                CreativeZoneBuilder.Build(ctx);
                Progress(showProgress, "Physiklabor", 0.58f);
                PhysicsZoneBuilder.Build(ctx);
                Progress(showProgress, "Lichtlabor", 0.66f);
                LightZoneBuilder.Build(ctx);
                Progress(showProgress, "Interaktionsbereich", 0.74f);
                InteractionZoneBuilder.Build(ctx);
                Progress(showProgress, "Dekoration", 0.8f);
                WorldBuilder.BuildDecoration(ctx);
                Progress(showProgress, "Kameras", 0.86f);
                CameraSetup.Build(ctx);

                ctx.DayNight.Apply(TimeOfDay.Day);
                SetFriendlyInspectorRotations(scene);
                RecordAllPrefabInstanceModifications(scene);

                Progress(showProgress, "Szene speichern", 0.92f);
                if (!EditorSceneManager.SaveScene(scene, SandboxPaths.ScenePath))
                    throw new InvalidOperationException("Szene konnte nicht gespeichert werden: " + SandboxPaths.ScenePath);
                SaveOriginalCopy();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Progress(showProgress, "Ansicht ausrichten", 0.98f);
                if (!Application.isBatchMode) SandboxSceneView.FrameViewpoint(CameraSetup.OverviewName);
                Selection.activeObject = null;

                Debug.Log("[Student Sandbox] Spielwiese erfolgreich erzeugt und gespeichert: " + SandboxPaths.ScenePath +
                          " (Ausgangskopie: " + SandboxPaths.OriginalScenePath + ")");
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError("[Student Sandbox] Erzeugen fehlgeschlagen: " + exception);
                if (showProgress) EditorUtility.ClearProgressBar();
                // Halb gebaute, ungespeicherte Szene verwerfen, damit niemand sie aus Versehen speichert.
                if (newSceneCreated && !Application.isBatchMode)
                {
                    if (SceneExists) EditorSceneManager.OpenScene(SandboxPaths.ScenePath, OpenSceneMode.Single);
                    else EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                }
                if (showProgress && !Application.isBatchMode)
                    EditorUtility.DisplayDialog("Student Sandbox", "Beim Erzeugen ist ein Fehler aufgetreten:\n" + exception.Message + "\n\nDetails stehen in der Console.", "OK");
                return false;
            }
            finally
            {
                if (showProgress) EditorUtility.ClearProgressBar();
            }
        }

        // ------------------------------------------------------------------ Szene öffnen, sichern, wiederherstellen

        public static bool OpenSandboxScene(bool askToSave)
        {
            if (!SceneExists)
            {
                if (!Application.isBatchMode)
                    EditorUtility.DisplayDialog("Student Sandbox", "Die Spielwiese wurde noch nicht erzeugt.\nBitte 'Tools > Student Sandbox > Generate Sandbox' ausführen.", "OK");
                return false;
            }
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Student Sandbox", "Bitte zuerst den Play Mode beenden.", "OK");
                return false;
            }
            if (askToSave && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return false;
            EditorSceneManager.OpenScene(SandboxPaths.ScenePath, OpenSceneMode.Single);
            return true;
        }

        /// <summary>Kopiert die gespeicherte Spielwiesen-Szene nach Backups/. Gibt den Pfad der Sicherung zurück.</summary>
        public static string BackupScene()
        {
            if (!SceneExists) return null;
            SandboxPaths.EnsureFolder(SandboxPaths.Backups);
            string stamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string target = SandboxPaths.Backups + "/StudentSandbox_Backup_" + stamp + ".unity";
            int counter = 1;
            while (File.Exists(target)) target = SandboxPaths.Backups + "/StudentSandbox_Backup_" + stamp + "_" + counter++ + ".unity";
            // Datei kopieren (ohne .meta) – Unity vergibt der Sicherung eine neue GUID.
            File.Copy(SandboxPaths.ScenePath, target, false);
            AssetDatabase.ImportAsset(target);
            Debug.Log("[Student Sandbox] Sicherung angelegt: " + target);
            return target;
        }

        static void SaveOriginalCopy()
        {
            SandboxPaths.EnsureFolder(SandboxPaths.OriginalFolder);
            File.Copy(SandboxPaths.ScenePath, SandboxPaths.OriginalScenePath, true);
            AssetDatabase.ImportAsset(SandboxPaths.OriginalScenePath, ImportAssetOptions.ForceUpdate);
        }

        /// <summary>
        /// Stellt die unberührte Ausgangsszene wieder her. Vorher wird gefragt
        /// und die aktuelle Szene als Sicherung gespeichert – es geht nichts verloren.
        /// </summary>
        public static bool RestoreOriginalScene() => RestoreOriginalScene(true);

        /// <param name="askUser">false = ohne Rückfragen (für automatische Tests im Batchmode). Eine Sicherung wird trotzdem angelegt.</param>
        public static bool RestoreOriginalScene(bool askUser)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if (askUser) EditorUtility.DisplayDialog("Student Sandbox", "Bitte zuerst den Play Mode beenden.", "OK");
                return false;
            }
            if (!File.Exists(SandboxPaths.OriginalScenePath))
            {
                if (askUser) EditorUtility.DisplayDialog("Student Sandbox", "Es gibt keine Ausgangsszene. Bitte 'Generate Sandbox' ausführen.", "OK");
                return false;
            }
            if (askUser && !EditorUtility.DisplayDialog("Ausgangsszene wiederherstellen?",
                    "Die Spielwiese wird auf den Zustand direkt nach dem Erzeugen zurückgesetzt – " +
                    "auch die Farben (Materialien), die Prefabs und die Nachbearbeitung.\n\n" +
                    "Deine aktuelle Szene wird vorher als Sicherung im Ordner 'StudentSandbox/Backups' gespeichert, " +
                    "du kannst sie also jederzeit wieder öffnen.\n\nUngespeicherte Änderungen: Du wirst gleich gefragt, ob sie vorher gespeichert werden sollen.",
                    "Ja, wiederherstellen", "Abbrechen"))
                return false;

            Scene active = SceneManager.GetActiveScene();
            if (active.IsValid() && active.path == SandboxPaths.OriginalScenePath)
            {
                // Die Ausgangskopie selbst ist offen: Änderungen daran dürfen NICHT gespeichert werden.
                if (askUser && active.isDirty && !EditorUtility.DisplayDialog("Ausgangskopie ist geöffnet",
                        "Du hast gerade die Ausgangskopie (Scenes/Original) geöffnet und verändert. Diese Änderungen werden verworfen, " +
                        "damit die Ausgangskopie unberührt bleibt.", "OK, verwerfen", "Abbrechen"))
                    return false;
            }
            else if (askUser && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return false;
            }

            string backup = BackupScene();
            // Szene schließen, damit die Datei ersetzt werden kann.
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            ResetGeneratedAssets();
            File.Copy(SandboxPaths.OriginalScenePath, SandboxPaths.ScenePath, true);
            AssetDatabase.ImportAsset(SandboxPaths.ScenePath, ImportAssetOptions.ForceUpdate);
            EditorSceneManager.OpenScene(SandboxPaths.ScenePath, OpenSceneMode.Single);

            if (askUser)
                EditorUtility.DisplayDialog("Student Sandbox",
                "Die Ausgangsszene ist wiederhergestellt." + (backup != null ? "\n\nDeine vorherige Szene liegt hier:\n" + backup : ""), "OK");
            return true;
        }

        /// <summary>
        /// Setzt alle generierten Assets (Materialien, Meshes, Prefabs, Post-Processing-Profil) auf den Ausgangszustand.
        /// Die GUIDs bleiben gleich, deshalb passen Szene und Ausgangskopie weiterhin dazu.
        /// Braucht eine offene (leere) Szene, weil Prefabs dort kurz zusammengebaut werden.
        /// </summary>
        static void ResetGeneratedAssets()
        {
            SandboxPaths.EnsureAllFolders();
            SandboxMaterials.CreateAll();
            SandboxMeshes.CreateAll();
            SandboxPrefabs.CreateAll();
            LightingSetup.ConfigurePostProcessingProfile();
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Änderungen, die per Skript an Prefab-Instanzen gemacht wurden (Name, Position, Material, Texte …),
        /// müssen als "Overrides" registriert werden – sonst gehen sie beim Speichern verloren.
        /// Unveränderte Werte erzeugen dabei keine Overrides.
        /// </summary>
        static void RecordAllPrefabInstanceModifications(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                {
                    if (!PrefabUtility.IsPartOfPrefabInstance(t.gameObject)) continue;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(t.gameObject);
                    foreach (Component component in t.GetComponents<Component>())
                    {
                        if (component != null) PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                    }
                }
            }
        }

        /// <summary>
        /// Im Inspector sollen Drehungen so erscheinen, wie die Anleitungen sie nennen (z.B. Rotation Z = -22 statt 338).
        /// Unity merkt sich dafür einen "Euler-Hinweis" pro Transform – den setzen wir auf Werte zwischen -180 und 180.
        /// </summary>
        static void SetFriendlyInspectorRotations(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                {
                    Vector3 euler = t.localEulerAngles;
                    var friendly = new Vector3(Signed(euler.x), Signed(euler.y), Signed(euler.z));
                    TransformUtils.SetInspectorRotation(t, friendly);
                }
            }
        }

        static float Signed(float angle)
        {
            angle = Mathf.Repeat(angle + 180f, 360f) - 180f;
            float rounded = Mathf.Round(angle * 1000f) / 1000f;
            return Mathf.Approximately(rounded, -180f) ? 180f : rounded;
        }

        static void Progress(bool show, string step, float progress)
        {
            if (show && !Application.isBatchMode) EditorUtility.DisplayProgressBar("Student Sandbox wird erzeugt …", step, progress);
        }
    }
}
