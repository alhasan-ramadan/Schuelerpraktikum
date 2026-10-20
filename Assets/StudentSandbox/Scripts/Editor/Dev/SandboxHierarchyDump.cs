using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StudentSandbox.EditorTools
{
    /// <summary>
    /// Entwickler-Werkzeug: schreibt die Hierarchy der Spielwiese als Text (mit Komponenten) in eine Datei.
    /// Menü: Tools > Student Sandbox > Developer > Dump Hierarchy
    /// Kommandozeile: -executeMethod StudentSandbox.EditorTools.SandboxHierarchyDump.DumpBatch -sandboxDump &lt;Datei&gt; [-sandboxDumpDepth 4]
    /// </summary>
    public static class SandboxHierarchyDump
    {
        [MenuItem(SandboxGenerator.MenuRoot + "Developer/Dump Hierarchy", priority = 201)]
        static void DumpFromMenu()
        {
            string file = Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp/StudentSandbox_Hierarchy.txt"));
            File.WriteAllText(file, Dump(SceneManager.GetActiveScene(), 6));
            EditorUtility.RevealInFinder(file);
        }

        public static void DumpBatch()
        {
            string file = GetArg("-sandboxDump") ?? Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp/StudentSandbox_Hierarchy.txt"));
            int depth = int.TryParse(GetArg("-sandboxDumpDepth"), out int d) ? d : 4;
            Scene scene = EditorSceneManager.OpenScene(SandboxPaths.ScenePath, OpenSceneMode.Single);
            File.WriteAllText(file, Dump(scene, depth));
            Debug.Log("[Student Sandbox] Hierarchy geschrieben: " + file);
            if (Application.isBatchMode) EditorApplication.Exit(0);
        }

        public static string Dump(Scene scene, int maxDepth)
        {
            var builder = new StringBuilder();
            int total = 0;
            foreach (GameObject root in scene.GetRootGameObjects()) Append(builder, root.transform, 0, maxDepth, ref total);
            builder.Insert(0, "GameObjects gesamt: " + total + Environment.NewLine);
            return builder.ToString();
        }

        static void Append(StringBuilder builder, Transform t, int depth, int maxDepth, ref int total)
        {
            total++;
            if (depth <= maxDepth)
            {
                builder.Append(new string(' ', depth * 2)).Append(t.name);
                if (PrefabUtility.IsAnyPrefabInstanceRoot(t.gameObject)) builder.Append(" [Prefab]");
                builder.Append("  {");
                foreach (Component component in t.GetComponents<Component>())
                {
                    if (component == null) builder.Append("MISSING ");
                    else if (!(component is Transform)) builder.Append(component.GetType().Name).Append(' ');
                }
                builder.Append('}').Append(Environment.NewLine);
            }
            for (int i = 0; i < t.childCount; i++) Append(builder, t.GetChild(i), depth + 1, maxDepth, ref total);
        }

        static string GetArg(string name)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == name) return args[i + 1];
            return null;
        }
    }
}
