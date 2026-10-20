using UnityEditor;
using UnityEngine;

namespace StudentSandbox.EditorTools
{
    /// <summary>
    /// Inspector für den <see cref="DayNightController"/>: drei große Knöpfe TAG / SONNENUNTERGANG / NACHT,
    /// die sofort in der Scene View wirken (im Edit Mode mit Strg+Z rückgängig zu machen).
    /// Darunter stehen wie gewohnt alle Einstellungen.
    /// </summary>
    [CustomEditor(typeof(DayNightController))]
    [CanEditMultipleObjects]
    public class DayNightControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            WorkshopGUI.Ensure();
            var controller = (DayNightController)target;
            TimeOfDay current = controller.CurrentTime;

            GUILayout.Space(4f);
            GUILayout.Label("Tageszeit wählen", WorkshopGUI.SubHeaderTight);

            if (EditorGUIUtility.currentViewWidth >= 360f)
            {
                Rect[] cells = WorkshopGUI.RowWeighted(42f, 1f, 2f, 1f);
                TimeButton(cells[0], "TAG", TimeOfDay.Day, current);
                TimeButton(cells[1], "SONNENUNTERGANG", TimeOfDay.Sunset, current);
                TimeButton(cells[2], "NACHT", TimeOfDay.Night, current);
            }
            else
            {
                // Schmaler Inspector: untereinander, damit SONNENUNTERGANG nicht abgeschnitten wird.
                TimeButton(WorkshopGUI.Row(1, 34f)[0], "TAG", TimeOfDay.Day, current);
                TimeButton(WorkshopGUI.Row(1, 34f)[0], "SONNENUNTERGANG", TimeOfDay.Sunset, current);
                TimeButton(WorkshopGUI.Row(1, 34f)[0], "NACHT", TimeOfDay.Night, current);
            }

            WorkshopGUI.Pill("Jetzt: " + DayNightEditorUtility.DisplayName(current), ColorFor(current), 28f, 14);

            GUILayout.Label(EditorApplication.isPlaying
                    ? "Im Play Mode kannst du auch die Taste T drücken."
                    : "Die Scene View ändert sich sofort. Mit Strg+Z machst du es rückgängig.\n" +
                      "Tipp: Ändere unten bei " + PresetFieldLabel(current) + " die Sun Intensity oder Sun Color – du siehst es sofort.",
                WorkshopGUI.Small);

            GUILayout.Space(8f);

            // Ändert jemand unten eine Einstellung (z.B. Day → Sun Intensity), wird die aktuelle Tageszeit
            // sofort neu angewendet – so sieht man die Wirkung direkt. Undo fasst beides zu einem Schritt zusammen.
            int undoGroup = Undo.GetCurrentGroup();
            int dirtyBefore = DirtyCount();
            // Nur bei echten Wert-Änderungen neu anwenden (nicht beim Auf- und Zuklappen einer Gruppe).
            if (DrawDefaultInspector() && DirtyCount() != dirtyBefore)
            {
                foreach (Object item in targets)
                {
                    if (item is DayNightController changed) DayNightEditorUtility.Apply(changed, changed.CurrentTime);
                }
                if (!EditorApplication.isPlaying) Undo.CollapseUndoOperations(undoGroup);
            }
        }

        int DirtyCount()
        {
            int count = 0;
            foreach (Object item in targets)
            {
                if (item != null) count += EditorUtility.GetDirtyCount(item);
            }
            return count;
        }

        void TimeButton(Rect rect, string label, TimeOfDay time, TimeOfDay current)
        {
            Color color = ColorFor(time);
            bool active = time == current;
            string tooltip = "Stellt die Tageszeit auf " + DayNightEditorUtility.DisplayName(time) + ".";
            if (!WorkshopGUI.Button(rect, label, active ? color : WorkshopGUI.Pale(color), true, tooltip, 12)) return;

            foreach (Object item in targets)
            {
                if (item is DayNightController controller) DayNightEditorUtility.Apply(controller, time);
            }
        }

        /// <summary>So heißt das Feld der aktuellen Tageszeit im Inspector.</summary>
        static string PresetFieldLabel(TimeOfDay time)
        {
            switch (time)
            {
                case TimeOfDay.Sunset: return "Sunset";
                case TimeOfDay.Night: return "Night";
                default: return "Day";
            }
        }

        static Color ColorFor(TimeOfDay time)
        {
            switch (time)
            {
                case TimeOfDay.Sunset: return WorkshopGUI.Orange;
                case TimeOfDay.Night: return WorkshopGUI.NightBlue;
                default: return WorkshopGUI.Yellow;
            }
        }
    }
}
