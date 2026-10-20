using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace StudentSandbox.EditorTools
{
    // Kleine Test-Knöpfe über den normalen Einstellungen der Spielwiesen-Komponenten.
    // Jeder Inspector ruft am Ende DrawDefaultInspector() auf, damit alle Felder editierbar bleiben.

    /// <summary>Gemeinsame Helfer für die Inspectors in dieser Datei.</summary>
    static class SandboxInspectorUtility
    {
        public static void Heading(string text)
        {
            GUILayout.Space(4f);
            GUILayout.Label(text, WorkshopGUI.SubHeaderTight);
        }

        public static void Info(string text)
        {
            EditorGUILayout.HelpBox(text, MessageType.Info);
        }

        public static void Separator()
        {
            GUILayout.Space(6f);
            Rect line = GUILayoutUtility.GetRect(0f, 1f, GUILayout.ExpandWidth(true));
            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(line, EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.12f) : new Color(0f, 0f, 0f, 0.12f));
            GUILayout.Space(6f);
        }

        /// <summary>Alle ausgewählten Komponenten eines Typs.</summary>
        public static List<T> Targets<T>(Editor editor) where T : Object
        {
            var list = new List<T>();
            foreach (Object item in editor.targets)
            {
                if (item is T typed) list.Add(typed);
            }
            return list;
        }

        /// <summary>Undo vor einer Änderung im Edit Mode (im Play Mode passiert nichts).</summary>
        public static void RecordForUndo(IList<Object> objects, string name)
        {
            if (EditorApplication.isPlaying || objects.Count == 0) return;
            var array = new Object[objects.Count];
            objects.CopyTo(array, 0);
            Undo.RecordObjects(array, name);
        }

        /// <summary>Nach einer Änderung im Edit Mode: Prefab-Overrides merken und die Szene als geändert markieren.</summary>
        public static void FinishEdit(IList<Object> objects)
        {
            if (EditorApplication.isPlaying) return;
            foreach (Object obj in objects)
            {
                if (obj == null) continue;
                if (PrefabUtility.IsPartOfPrefabInstance(obj)) PrefabUtility.RecordPrefabInstancePropertyModifications(obj);
                if (obj is Component component && component.gameObject.scene.IsValid())
                    EditorSceneManager.MarkSceneDirty(component.gameObject.scene);
            }
        }
    }

    // ====================================================================== DoorController

    [CustomEditor(typeof(DoorController))]
    [CanEditMultipleObjects]
    public class DoorControllerEditor : Editor
    {
        public override bool RequiresConstantRepaint() => EditorApplication.isPlaying;

        public override void OnInspectorGUI()
        {
            WorkshopGUI.Ensure();
            var door = (DoorController)target;
            List<DoorController> doors = SandboxInspectorUtility.Targets<DoorController>(this);

            SandboxInspectorUtility.Heading("Tür testen");
            if (EditorApplication.isPlaying)
            {
                Rect[] cells = WorkshopGUI.Row(3, 34f);
                if (WorkshopGUI.Button(cells[0], "Öffnen", WorkshopGUI.Mint))
                    foreach (DoorController d in doors) d.Open();
                if (WorkshopGUI.Button(cells[1], "Schließen", WorkshopGUI.Coral))
                    foreach (DoorController d in doors) d.Close();
                if (WorkshopGUI.Button(cells[2], "Umschalten", WorkshopGUI.Turquoise))
                    foreach (DoorController d in doors) d.Toggle();

                float amount = door.OpenAmount;
                string state = amount <= 0.001f ? "Die Tür ist zu"
                    : amount >= 0.999f ? "Die Tür ist ganz offen"
                    : "Offen: " + Mathf.RoundToInt(amount * 100f) + " %";
                WorkshopGUI.ProgressBar(amount, state, WorkshopGUI.Turquoise);
            }
            else
            {
                SandboxInspectorUtility.Info("Drücke Play und klicke auf den Schalter – oder teste hier im Play Mode.\n" +
                                             "Open Speed: 1 = die Tür braucht 1 Sekunde, 0.2 = ganz langsam (5 Sekunden).");
            }

            SandboxInspectorUtility.Separator();
            DrawDefaultInspector();
        }
    }

    // ====================================================================== TrafficLightController

    [CustomEditor(typeof(TrafficLightController))]
    [CanEditMultipleObjects]
    public class TrafficLightControllerEditor : Editor
    {
        public override bool RequiresConstantRepaint() => EditorApplication.isPlaying;

        public override void OnInspectorGUI()
        {
            WorkshopGUI.Ensure();
            var traffic = (TrafficLightController)target;
            List<TrafficLightController> lights = SandboxInspectorUtility.Targets<TrafficLightController>(this);
            int phaseCount = traffic.Phases != null ? traffic.Phases.Count : 0;

            SandboxInspectorUtility.Heading("Ampel testen");
            if (EditorApplication.isPlaying)
            {
                WorkshopGUI.Pill(traffic.CurrentPhaseName, PhaseColor(traffic), 40f, 20);
                GUILayout.Label("Phase " + (Mathf.Clamp(traffic.CurrentPhaseIndex, 0, Mathf.Max(0, phaseCount - 1)) + 1) + " von " + phaseCount +
                                " · Automatik: " + (traffic.AutoCycle ? "AN" : "AUS"), WorkshopGUI.Small);

                Rect[] cells = WorkshopGUI.Row(2, 34f);
                if (WorkshopGUI.Button(cells[0], "Nächste Phase", WorkshopGUI.Coral))
                    foreach (TrafficLightController t in lights) t.NextPhase();
                if (WorkshopGUI.Button(cells[1], "Automatik an/aus", traffic.AutoCycle ? WorkshopGUI.Yellow : WorkshopGUI.Pale(WorkshopGUI.Yellow)))
                    foreach (TrafficLightController t in lights) t.ToggleAutoCycle();
            }
            else
            {
                SandboxInspectorUtility.Info("Die Ampel ist eine Zustandsmaschine: Sie ist immer in genau einer Phase.\n" +
                                             "Drücke Play – dann kannst du hier weiterschalten oder die Automatik starten.\n" +
                                             "Unter Phases änderst du Reihenfolge und Dauer der Phasen.");
                GUILayout.Label("Startphase: " + traffic.CurrentPhaseName + " (" + phaseCount + " Phasen)", WorkshopGUI.Small);
            }

            SandboxInspectorUtility.Separator();
            DrawDefaultInspector();
        }

        static Color PhaseColor(TrafficLightController traffic)
        {
            if (traffic.IsRed && traffic.IsYellow) return WorkshopGUI.Orange;
            if (traffic.IsRed) return WorkshopGUI.Hex("#F0524A");
            if (traffic.IsYellow) return WorkshopGUI.Yellow;
            if (traffic.IsGreen) return WorkshopGUI.Hex("#5CCB5F");
            return WorkshopGUI.Neutral;
        }
    }

    // ====================================================================== ClickableSwitch

    [CustomEditor(typeof(ClickableSwitch))]
    [CanEditMultipleObjects]
    public class ClickableSwitchEditor : Editor
    {
        public override bool RequiresConstantRepaint() => EditorApplication.isPlaying;

        public override void OnInspectorGUI()
        {
            WorkshopGUI.Ensure();
            var clickable = (ClickableSwitch)target;

            SandboxInspectorUtility.Heading("Schalter");
            if (EditorApplication.isPlaying)
            {
                if (WorkshopGUI.Button("Jetzt drücken (Test)", WorkshopGUI.Coral, 36f, true, "Löst das Event On Pressed aus – wie ein Klick im Game-Fenster."))
                {
                    foreach (ClickableSwitch item in SandboxInspectorUtility.Targets<ClickableSwitch>(this)) item.Press();
                }
                GUILayout.Label("Schon " + clickable.PressCount + "-mal gedrückt.", WorkshopGUI.Small);
            }
            else
            {
                SandboxInspectorUtility.Info("Unten bei „On Pressed ()“ steht, was beim Klicken passiert:\n" +
                                             "links das Objekt (welches Objekt reagiert?), rechts die Funktion (was soll es tun?).\n" +
                                             "Mit + fügst du eine weitere Aktion hinzu. Teste den Schalter im Play Mode.");
            }

            DrawListenerSummary(clickable);

            SandboxInspectorUtility.Separator();
            DrawDefaultInspector();
        }

        static void DrawListenerSummary(ClickableSwitch clickable)
        {
            if (clickable.OnPressed == null) return;
            int count = clickable.OnPressed.GetPersistentEventCount();

            WorkshopGUI.BeginCard(WorkshopGUI.Sky, EditorGUIUtility.isProSkin ? 0.15f : 0.25f);
            GUILayout.Label("Beim Klicken passiert:", WorkshopGUI.SmallBold);
            if (count == 0)
            {
                GUILayout.Label("Noch nichts. Füge unten bei On Pressed mit + eine Aktion hinzu.", WorkshopGUI.Body);
            }
            for (int i = 0; i < count; i++)
            {
                Object listenerTarget = clickable.OnPressed.GetPersistentTarget(i);
                string method = clickable.OnPressed.GetPersistentMethodName(i);
                string targetText;
                if (listenerTarget == null) targetText = "(kein Objekt)";
                else if (listenerTarget is Component component) targetText = component.gameObject.name + " → " + component.GetType().Name;
                else targetText = listenerTarget.name;
                string methodText = string.IsNullOrEmpty(method) ? "(keine Funktion)" : method;
                string off = clickable.OnPressed.GetPersistentListenerState(i) == UnityEngine.Events.UnityEventCallState.Off ? "  (ausgeschaltet)" : "";
                GUILayout.Label("•  " + targetText + "." + methodText + off, WorkshopGUI.BodyBold);
            }
            WorkshopGUI.EndCard();
        }
    }

    // ====================================================================== Rotator

    [CustomEditor(typeof(Rotator))]
    [CanEditMultipleObjects]
    public class RotatorEditor : Editor
    {
        const float MaxSpeed = 720f;
        const float DefaultSpeed = 45f;

        public override void OnInspectorGUI()
        {
            WorkshopGUI.Ensure();
            var rotator = (Rotator)target;
            List<Rotator> rotators = SandboxInspectorUtility.Targets<Rotator>(this);

            SandboxInspectorUtility.Heading("Drehung ändern");
            Rect[] cells = WorkshopGUI.RowWeighted(34f, 1.3f, 1.3f, 1f);
            if (WorkshopGUI.Button(cells[0], "Doppelt so schnell", WorkshopGUI.Lavender, true, "Speed × 2 (höchstens 720)", 12))
                ChangeSpeed(rotators, 2f, "Doppelt so schnell");
            if (WorkshopGUI.Button(cells[1], "Halb so schnell", WorkshopGUI.Pale(WorkshopGUI.Lavender), true, "Speed ÷ 2", 12))
                ChangeSpeed(rotators, 0.5f, "Halb so schnell");
            if (WorkshopGUI.Button(cells[2], rotator.IsRotating ? "Stopp" : "Start", rotator.IsRotating ? WorkshopGUI.Coral : WorkshopGUI.Green, true,
                    "Schaltet die Drehung an oder aus (Haken Is Rotating).", 12))
                ToggleRotation(rotators, !rotator.IsRotating);

            GUILayout.Label(Describe(rotator), WorkshopGUI.Small);

            SandboxInspectorUtility.Separator();
            DrawDefaultInspector();
        }

        static string Describe(Rotator rotator)
        {
            if (!rotator.IsRotating) return "Steht gerade still. Speed: " + rotator.Speed.ToString("0.#") + " Grad pro Sekunde.";
            if (rotator.Speed <= 0.01f) return "Speed ist 0 – es dreht sich nichts.";
            float seconds = 360f / rotator.Speed;
            return "Speed: " + rotator.Speed.ToString("0.#") + " Grad pro Sekunde = eine ganze Umdrehung in " + seconds.ToString("0.#") + " Sekunden.";
        }

        static void ChangeSpeed(List<Rotator> rotators, float factor, string undoName)
        {
            var objects = new List<Object>(rotators.ToArray());
            SandboxInspectorUtility.RecordForUndo(objects, undoName);
            foreach (Rotator rotator in rotators)
            {
                float speed = rotator.Speed <= 0.01f && factor > 1f ? DefaultSpeed : rotator.Speed * factor;
                rotator.Speed = Mathf.Clamp(speed, 0f, MaxSpeed);
            }
            SandboxInspectorUtility.FinishEdit(objects);
        }

        static void ToggleRotation(List<Rotator> rotators, bool rotate)
        {
            if (EditorApplication.isPlaying)
            {
                // Im Play Mode mit Meldung auf dem Bildschirm.
                foreach (Rotator rotator in rotators)
                {
                    if (rotator.IsRotating != rotate) rotator.ToggleRotation();
                }
                return;
            }
            var objects = new List<Object>(rotators.ToArray());
            SandboxInspectorUtility.RecordForUndo(objects, rotate ? "Drehung starten" : "Drehung stoppen");
            foreach (Rotator rotator in rotators) rotator.IsRotating = rotate;
            SandboxInspectorUtility.FinishEdit(objects);
        }
    }

    // ====================================================================== PhysicsResetter

    [CustomEditor(typeof(PhysicsResetter))]
    [CanEditMultipleObjects]
    public class PhysicsResetterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            WorkshopGUI.Ensure();
            var resetter = (PhysicsResetter)target;

            SandboxInspectorUtility.Heading("Physik zurücksetzen");
            if (EditorApplication.isPlaying)
            {
                if (WorkshopGUI.Button("Alles zurücksetzen", WorkshopGUI.Coral, 38f, true, "Kugeln, Dominosteine, Hammer und Ziele zurück an den Start (wie Taste R)."))
                    resetter.ResetAll();
                GUILayout.Label("Beim letzten Mal zurückgesetzt: " + resetter.LastResetCount + " Objekte.", WorkshopGUI.Small);
            }
            else
            {
                SandboxInspectorUtility.Info("Im Play Mode setzt dieser Knopf (oder Taste R) alle Kugeln, Dominosteine und Ziele an den Start zurück.\n" +
                                             "Deine Änderungen aus dem Edit Mode bleiben dabei erhalten.");
            }

            SandboxInspectorUtility.Separator();
            DrawDefaultInspector();
        }
    }

    // ====================================================================== MovingPlatform

    [CustomEditor(typeof(MovingPlatform))]
    [CanEditMultipleObjects]
    public class MovingPlatformEditor : Editor
    {
        public override bool RequiresConstantRepaint() => EditorApplication.isPlaying;

        public override void OnInspectorGUI()
        {
            WorkshopGUI.Ensure();
            var platform = (MovingPlatform)target;

            SandboxInspectorUtility.Heading("Plattform");
            if (EditorApplication.isPlaying)
            {
                if (WorkshopGUI.Button(platform.IsMoving ? "Stopp" : "Start", platform.IsMoving ? WorkshopGUI.Coral : WorkshopGUI.Green, 36f, true,
                        "Plattform anhalten oder losfahren lassen."))
                {
                    foreach (MovingPlatform item in SandboxInspectorUtility.Targets<MovingPlatform>(this)) item.ToggleMoving();
                }
                WorkshopGUI.ProgressBar(platform.PathPosition, "PointA  ←  Position  →  PointB", WorkshopGUI.Turquoise);
            }
            else
            {
                Rect[] cells = WorkshopGUI.Row(2, 32f);
                if (WorkshopGUI.Button(cells[0], "PointA auswählen", WorkshopGUI.Turquoise, platform.PointA != null, "Wählt den Startpunkt aus."))
                    SelectLater(platform.PointA);
                if (WorkshopGUI.Button(cells[1], "PointB auswählen", WorkshopGUI.Turquoise, platform.PointB != null, "Wählt den Endpunkt aus."))
                    SelectLater(platform.PointB);
                SandboxInspectorUtility.Info("Verschiebe PointA oder PointB mit dem Verschieben-Werkzeug (W), um die Strecke zu ändern. " +
                                             "Speed = Meter pro Sekunde, Wait Time = Pause an jedem Ende. Im Play Mode kannst du hier starten und stoppen.");
            }

            SandboxInspectorUtility.Separator();
            DrawDefaultInspector();
        }

        /// <summary>Auswahl erst nach dem Zeichnen ändern, sonst baut sich der Inspector mitten im Zeichnen um.</summary>
        static void SelectLater(Transform point)
        {
            if (point == null) return;
            EditorApplication.delayCall += () =>
            {
                if (point == null) return;
                Selection.activeTransform = point;
                EditorGUIUtility.PingObject(point.gameObject);
                SceneView view = SceneView.lastActiveSceneView;
                if (view != null) view.FrameSelected();
            };
        }
    }

    // ====================================================================== LampController

    [CustomEditor(typeof(LampController))]
    [CanEditMultipleObjects]
    public class LampControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            WorkshopGUI.Ensure();
            var lamp = (LampController)target;
            List<LampController> lamps = SandboxInspectorUtility.Targets<LampController>(this);

            SandboxInspectorUtility.Heading("Lampe schalten");
            WorkshopGUI.Pill(lamp.IsOn ? "Die Lampe ist AN" : "Die Lampe ist AUS", lamp.IsOn ? WorkshopGUI.Yellow : WorkshopGUI.Charcoal, 28f, 14);

            Rect[] cells = WorkshopGUI.Row(2, 34f);
            if (WorkshopGUI.Button(cells[0], "An", lamp.IsOn ? WorkshopGUI.Yellow : WorkshopGUI.Pale(WorkshopGUI.Yellow), true, "Licht und leuchtende Teile einschalten."))
                DayNightEditorUtility.SetLamps(lamps, true);
            if (WorkshopGUI.Button(cells[1], "Aus", lamp.IsOn ? WorkshopGUI.Pale(WorkshopGUI.Charcoal) : WorkshopGUI.Charcoal, true, "Licht und leuchtende Teile ausschalten."))
                DayNightEditorUtility.SetLamps(lamps, false);

            GUILayout.Label(lamp.FollowDayNight
                    ? "Follow Day Night ist an: Bei Sonnenuntergang und Nacht geht die Lampe automatisch an, am Tag aus."
                    : "Follow Day Night ist aus: Die Lampe bleibt so, wie du sie schaltest.",
                WorkshopGUI.Small);
            if (lamp.LampLight != null)
                GUILayout.Label("Farbe und Helligkeit stellst du am Licht „" + lamp.LampLight.gameObject.name + "“ ein (Light → Color, Intensity).", WorkshopGUI.Small);

            SandboxInspectorUtility.Separator();
            DrawDefaultInspector();
        }
    }
}
