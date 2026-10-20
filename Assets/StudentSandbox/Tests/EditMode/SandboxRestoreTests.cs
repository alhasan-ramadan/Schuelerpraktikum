using System.IO;
using NUnit.Framework;
using StudentSandbox.EditorTools;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StudentSandbox.Tests
{
    /// <summary>
    /// Prüft "Ausgangsszene wiederherstellen" wirklich: verändert Szene und Material, stellt wieder her und kontrolliert das Ergebnis.
    /// Läuft nur auf ausdrücklichen Wunsch (Explicit), weil dabei Dateien geschrieben werden
    /// (die dabei entstandene Sicherung wird am Ende wieder gelöscht).
    /// </summary>
    [Explicit("Schreibt Dateien (Szene, Material, Sicherung) – nur gezielt ausführen.")]
    public class SandboxRestoreTests
    {
        const string RoofMaterialPath = SandboxPaths.MaterialsEnvironment + "/" + M.RoofRed + ".mat";

        [Test]
        public void RestoreOriginalSceneResetsSceneAndAssets()
        {
            if (!File.Exists(SandboxPaths.ScenePath) || !File.Exists(SandboxPaths.OriginalScenePath))
                Assert.Ignore("Spielwiese ist nicht erzeugt.");

            string[] backupsBefore = Directory.Exists(SandboxPaths.Backups) ? Directory.GetFiles(SandboxPaths.Backups, "*.unity") : new string[0];

            // 1. Material verändern (Farbe + Transparenz) – so wie ein Kind es im Inspector tun könnte.
            Material roof = AssetDatabase.LoadAssetAtPath<Material>(RoofMaterialPath);
            Color originalColor = roof.GetColor("_BaseColor");
            roof.SetColor("_BaseColor", Color.black);
            roof.SetFloat("_Surface", 1f);
            EditorUtility.SetDirty(roof);
            AssetDatabase.SaveAssets();

            // 2. Szene verändern und speichern.
            Scene scene = EditorSceneManager.OpenScene(SandboxPaths.ScenePath, OpenSceneMode.Single);
            GameObject tree = GameObject.Find("Tree_Small_1");
            Assert.IsNotNull(tree, "Tree_Small_1 fehlt.");
            Vector3 originalTreePosition = tree.transform.position;
            tree.transform.position += new Vector3(5f, 0f, 0f);
            Assert.IsTrue(EditorSceneManager.SaveScene(scene));

            // 3. Wiederherstellen – ohne Rückfragen (im Batchmode gibt es keine Dialoge).
            Assert.IsTrue(SandboxGenerator.RestoreOriginalScene(false), "Wiederherstellen hat false zurückgegeben.");

            // 4. Ergebnis prüfen.
            Assert.AreEqual(SandboxPaths.ScenePath, SceneManager.GetActiveScene().path, "Nach dem Wiederherstellen ist nicht die Spielwiese offen.");
            GameObject restoredTree = GameObject.Find("Tree_Small_1");
            Assert.IsNotNull(restoredTree);
            Assert.Less(Vector3.Distance(originalTreePosition, restoredTree.transform.position), 0.01f, "Der Baum steht nicht wieder an seinem Platz.");

            Material restoredRoof = AssetDatabase.LoadAssetAtPath<Material>(RoofMaterialPath);
            Assert.AreEqual(originalColor, restoredRoof.GetColor("_BaseColor"), "Die Dachfarbe wurde nicht zurückgesetzt.");
            Assert.AreEqual(0f, restoredRoof.GetFloat("_Surface"), "Die Transparenz-Einstellung wurde nicht zurückgesetzt.");

            string[] backupsAfter = Directory.GetFiles(SandboxPaths.Backups, "*.unity");
            Assert.AreEqual(backupsBefore.Length + 1, backupsAfter.Length, "Es wurde keine Sicherung angelegt.");

            // Aufräumen: die Sicherung aus diesem Test wieder löschen.
            foreach (string file in backupsAfter)
            {
                if (System.Array.IndexOf(backupsBefore, file) >= 0) continue;
                AssetDatabase.DeleteAsset(file.Replace('\\', '/'));
            }
            if (backupsBefore.Length == 0 && Directory.GetFiles(SandboxPaths.Backups).Length == 0)
                AssetDatabase.DeleteAsset(SandboxPaths.Backups);
        }
    }
}
