using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;

namespace StudentSandbox.EditorTools
{
    /// <summary>
    /// Schaltet die Tageszeit aus dem Editor heraus um – im Edit Mode mit Rückgängig (Strg+Z),
    /// im Play Mode direkt. Wird vom Inspector des DayNightControllers und vom Workshop-Fenster benutzt.
    /// </summary>
    public static class DayNightEditorUtility
    {
        /// <summary>Sucht den DayNightController in der offenen Szene (auch wenn er deaktiviert ist).</summary>
        public static DayNightController Find()
        {
            return Object.FindAnyObjectByType<DayNightController>(FindObjectsInactive.Include);
        }

        /// <summary>Deutscher Name einer Tageszeit (für Knöpfe und Anzeigen).</summary>
        public static string DisplayName(TimeOfDay time)
        {
            switch (time)
            {
                case TimeOfDay.Sunset: return "Sonnenuntergang";
                case TimeOfDay.Night: return "Nacht";
                default: return "Tag";
            }
        }

        /// <summary>Sucht den Controller und wendet die Tageszeit an. Gibt false zurück, wenn es keinen gibt.</summary>
        public static bool Apply(TimeOfDay time)
        {
            DayNightController controller = Find();
            if (controller == null) return false;
            Apply(controller, time);
            return true;
        }

        /// <summary>
        /// Wendet eine Tageszeit an. Im Edit Mode wird vorher alles für Undo gemerkt
        /// (Controller, Sonne, alle Lampen mit Licht und leuchtenden Teilen) und die Szene als geändert markiert.
        /// </summary>
        public static void Apply(DayNightController controller, TimeOfDay time)
        {
            if (controller == null) return;

            if (EditorApplication.isPlaying)
            {
                controller.Apply(time);
                return;
            }

            List<Object> recorded = CollectUndoObjects(controller);
            Undo.RecordObjects(recorded.ToArray(), "Tageszeit: " + DisplayName(time));

            controller.Apply(time);

            FinishEditModeChange(recorded);
        }

        /// <summary>
        /// Schaltet Lampen an oder aus – im Edit Mode mit Undo (Komponente, Licht und leuchtende Teile)
        /// und mit markierter Szene, damit der Zustand gespeichert wird.
        /// </summary>
        public static void SetLamps(IEnumerable<LampController> lamps, bool on)
        {
            var list = new List<LampController>();
            foreach (LampController lamp in lamps)
            {
                if (lamp != null) list.Add(lamp);
            }
            if (list.Count == 0) return;

            if (EditorApplication.isPlaying)
            {
                foreach (LampController lamp in list) lamp.SetOn(on);
                return;
            }

            var recorded = new List<Object>();
            var seen = new HashSet<Object>();
            foreach (LampController lamp in list) AddLampObjects(lamp, recorded, seen);
            Undo.RecordObjects(recorded.ToArray(), on ? "Lampe an" : "Lampe aus");

            foreach (LampController lamp in list) lamp.SetOn(on);

            FinishEditModeChange(recorded);
        }

        static List<Object> CollectUndoObjects(DayNightController controller)
        {
            var objects = new List<Object>();
            var seen = new HashSet<Object>();

            Add(objects, seen, controller);
            // Himmel, Umgebungslicht und Nebel gehören zu den RenderSettings der Szene – auch sie sollen mit Strg+Z zurückgehen.
            Add(objects, seen, Unsupported.GetRenderSettings());

            Light sun = controller.Sun;
            if (sun != null)
            {
                Add(objects, seen, sun.transform);
                Add(objects, seen, sun);
            }

            LampController[] lamps = Object.FindObjectsByType<LampController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (LampController lamp in lamps) AddLampObjects(lamp, objects, seen);
            return objects;
        }

        /// <summary>Lampe, ihr Licht und alle leuchtenden Teile (Kinder und die Liste "glowRenderers").</summary>
        static void AddLampObjects(LampController lamp, List<Object> objects, HashSet<Object> seen)
        {
            if (lamp == null) return;
            Add(objects, seen, lamp);
            Add(objects, seen, lamp.LampLight);
            foreach (Renderer renderer in lamp.GetComponentsInChildren<Renderer>(true)) Add(objects, seen, renderer);

            // Leuchtende Teile, die nicht unterhalb der Lampe liegen, stehen nur in der Liste "glowRenderers".
            using (var serialized = new SerializedObject(lamp))
            {
                SerializedProperty glow = serialized.FindProperty("glowRenderers");
                if (glow != null && glow.isArray)
                {
                    for (int i = 0; i < glow.arraySize; i++)
                        Add(objects, seen, glow.GetArrayElementAtIndex(i).objectReferenceValue);
                }
            }
        }

        static void Add(List<Object> objects, HashSet<Object> seen, Object obj)
        {
            if (obj != null && seen.Add(obj)) objects.Add(obj);
        }

        /// <summary>Nach einer Änderung im Edit Mode: Prefab-Overrides merken, Szenen markieren, Ansichten neu zeichnen.</summary>
        static void FinishEditModeChange(List<Object> changed)
        {
            var scenes = new HashSet<UnityEngine.SceneManagement.Scene>();
            foreach (Object obj in changed)
            {
                if (obj == null) continue;
                // Änderungen an Prefab-Instanzen (Laternen, Häuser) als Overrides speichern.
                if (PrefabUtility.IsPartOfPrefabInstance(obj)) PrefabUtility.RecordPrefabInstancePropertyModifications(obj);

                GameObject go = obj as GameObject;
                if (go == null && obj is Component component) go = component.gameObject;
                if (go != null && go.scene.IsValid()) scenes.Add(go.scene);
            }
            foreach (UnityEngine.SceneManagement.Scene scene in scenes) EditorSceneManager.MarkSceneDirty(scene);

            SceneView.RepaintAll();
            InternalEditorUtility.RepaintAllViews();
        }
    }
}
