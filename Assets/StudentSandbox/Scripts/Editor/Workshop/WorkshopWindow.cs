using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StudentSandbox.EditorTools
{
    /// <summary>
    /// Das Workshop-Fenster: Startpunkt für die Schülerin.
    /// Menü: Tools > Student Sandbox > Workshop
    ///
    /// Aufbau (von oben nach unten): Willkommen · Lektionen · Bereich auswählen · Aufgaben der Lektion · Werkzeuge.
    /// Alles, was Dialoge öffnet oder Szenen lädt, läuft über EditorApplication.delayCall (außerhalb von OnGUI),
    /// damit das IMGUI-Layout nie durcheinanderkommt.
    /// </summary>
    public sealed class WorkshopWindow : EditorWindow
    {
        const string WindowTitle = "Student Sandbox – Workshop";
        const string SelectedLessonKey = "StudentSandbox.Workshop.SelectedLesson";
        const string ShowHelpKey = "StudentSandbox.Workshop.ShowHelp";
        const string ManualPath = SandboxPaths.Documentation + "/WORKSHOP_DE.md";

        [SerializeField] Vector2 scroll;
        [SerializeField] string backupInfo;
        int selectedLesson;
        bool showHelp;
        bool[][] ticks;
        float lessonSectionY = -1f;
        readonly List<System.Action> afterGui = new List<System.Action>();

        // Zustand – wird nur beim Layout-Event aktualisiert, damit Layout und Zeichnen immer zusammenpassen.
        bool isPlaying;
        bool sceneExists;
        bool sceneOpen;
        bool inPrefabMode;
        bool manualExists;
        bool hasDayNight;
        TimeOfDay currentTime;
        string selectedName;
        float contentWidth = 360f;

        // ------------------------------------------------------------------ Menü

        [MenuItem("Tools/Student Sandbox/Workshop", priority = 0)]
        public static void ShowWindow()
        {
            WorkshopWindow window = GetWindow<WorkshopWindow>(WindowTitle);
            window.minSize = new Vector2(380f, 520f);
            window.Show();
        }

        [MenuItem("Tools/Student Sandbox/Frame World Overview", priority = 30)]
        public static void FrameWorldOverview()
        {
            if (!SandboxSceneView.FrameViewpoint(CameraSetup.OverviewName))
            {
                EditorUtility.DisplayDialog("Student Sandbox",
                    "Die Weltübersicht wurde nicht gefunden.\n\nÖffne zuerst die Spielwiese (Tools > Student Sandbox > Open Sandbox Scene) " +
                    "und achte darauf, dass eine Scene View offen ist.", "OK");
            }
        }

        // ------------------------------------------------------------------ Lebenszyklus

        void OnEnable()
        {
            titleContent = new GUIContent(WindowTitle);
            minSize = new Vector2(380f, 520f);
            wantsMouseMove = true;

            selectedLesson = Mathf.Clamp(EditorPrefs.GetInt(SelectedLessonKey, 0), 0, WorkshopLessons.All.Length - 1);
            showHelp = EditorPrefs.GetBool(ShowHelpKey, false);
            LoadTicks();
            RefreshState();

            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorApplication.hierarchyChanged += Repaint;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorSceneManager.activeSceneChangedInEditMode += OnActiveSceneChanged;
            Selection.selectionChanged += Repaint;
        }

        void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorApplication.hierarchyChanged -= Repaint;
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.activeSceneChangedInEditMode -= OnActiveSceneChanged;
            Selection.selectionChanged -= Repaint;
        }

        void OnFocus()
        {
            LoadTicks();
            Repaint();
        }

        void OnPlayModeChanged(PlayModeStateChange change) => Repaint();
        void OnSceneOpened(Scene scene, OpenSceneMode mode) => Repaint();
        void OnActiveSceneChanged(Scene previous, Scene next) => Repaint();

        /// <summary>Etwa 10-mal pro Sekunde: neu zeichnen, wenn sich draußen etwas geändert hat (z.B. Taste T im Play Mode).</summary>
        void OnInspectorUpdate()
        {
            DayNightController dayNight = DayNightEditorUtility.Find();
            bool changed = EditorApplication.isPlaying != isPlaying
                           || (dayNight != null) != hasDayNight
                           || (dayNight != null && dayNight.CurrentTime != currentTime);
            if (changed) Repaint();
        }

        void RefreshState()
        {
            isPlaying = EditorApplication.isPlaying;
            sceneExists = SandboxGenerator.SceneExists;
            // Im Prefab-Modus zeigt die Scene View nur das Prefab – die Bereichs-Knöpfe hätten dann keine sichtbare Wirkung.
            inPrefabMode = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null;
            sceneOpen = SandboxSceneView.IsSandboxSceneOpen && !inPrefabMode;
            manualExists = File.Exists(ManualPath);
            DayNightController dayNight = DayNightEditorUtility.Find();
            hasDayNight = dayNight != null;
            currentTime = dayNight != null ? dayNight.CurrentTime : TimeOfDay.Day;
            GameObject selected = Selection.activeGameObject;
            selectedName = selected != null
                ? selected.name + (EditorUtility.IsPersistent(selected) ? " (im Project-Fenster)" : "")
                : null;
            // Fensterbreite minus Rand und Scrollbalken.
            contentWidth = Mathf.Max(200f, position.width - 2f * Page - 16f);
        }

        const float Page = 12f;

        // ------------------------------------------------------------------ OnGUI

        void OnGUI()
        {
            Event current = Event.current;
            if (current.type == EventType.Layout) RefreshState();
            if (current.type == EventType.MouseMove) Repaint();

            WorkshopGUI.Ensure();

            scroll = EditorGUILayout.BeginScrollView(scroll, GUIStyle.none, GUI.skin.verticalScrollbar);
            EditorGUILayout.BeginVertical(WorkshopGUI.PageStyle);

            DrawWelcome();
            DrawLessonPicker();
            DrawAreas();
            DrawLessonTasks();
            DrawTools();

            GUILayout.Space(16f);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();

            RunAfterGui();
        }

        /// <summary>Kleine Zustandsänderungen (z.B. andere Lektion) erst nach dem Zeichnen ausführen.</summary>
        void AfterGui(System.Action action) => afterGui.Add(action);

        /// <summary>Größere Aktionen (Dialoge, Szenen laden, Play Mode) außerhalb von OnGUI ausführen.</summary>
        void RunLater(System.Action action)
        {
            EditorApplication.delayCall += () =>
            {
                action();
                if (this != null) Repaint();
            };
        }

        void RunAfterGui()
        {
            if (afterGui.Count == 0) return;
            System.Action[] actions = afterGui.ToArray();
            afterGui.Clear();
            foreach (System.Action action in actions) action();
            Repaint();
        }

        // ------------------------------------------------------------------ 1. Willkommen

        void DrawWelcome()
        {
            GUILayout.Space(8f);
            WorkshopGUI.RainbowBar(6f);
            GUILayout.Label("Willkommen in deiner Unity-Spielwiese!", WorkshopGUI.BigTitle);
            GUILayout.Label("Heute lernst du, wie man eigene 3D-Welten baut, Physik simuliert, Licht verändert und Objekte zum Leben erweckt.",
                WorkshopGUI.Body);
            GUILayout.Space(8f);

            if (!sceneExists)
            {
                WorkshopGUI.Callout("Die Spielwiese gibt es noch nicht. Klicke auf den Knopf – Unity baut dann die ganze Welt für dich. Das dauert ein paar Sekunden.",
                    WorkshopGUI.Sky, "console.infoicon");
                if (WorkshopGUI.Button("Spielwiese erzeugen", WorkshopGUI.Mint, 40f, !isPlaying, "Baut die komplette Spielwiese und speichert sie."))
                    RunLater(() => SandboxGenerator.GenerateInteractive());
            }
            else if (inPrefabMode)
            {
                WorkshopGUI.Callout("Du bearbeitest gerade ein Prefab (Prefab-Modus). Die Scene View zeigt nur dieses eine Objekt.",
                    WorkshopGUI.Sky, "console.infoicon");
                if (WorkshopGUI.Button("Zurück zur Spielwiese", WorkshopGUI.Mint, 40f, true, "Verlässt den Prefab-Modus (wie der Pfeil oben links in der Hierarchy)."))
                    RunLater(UnityEditor.SceneManagement.StageUtility.GoToMainStage);
            }
            else if (!sceneOpen)
            {
                WorkshopGUI.Callout("Die Spielwiese ist gerade nicht geöffnet.", WorkshopGUI.Sky, "console.infoicon");
                if (WorkshopGUI.Button("Spielwiese öffnen", WorkshopGUI.Mint, 40f, !isPlaying, "Öffnet die Szene StudentSandbox."))
                    RunLater(() => SandboxGenerator.OpenSandboxScene(true));
            }

            DrawStatusLine();

            string playLabel = isPlaying ? "■  Play beenden" : "▶  Play starten";
            string playTip = isPlaying ? "Beendet den Play Mode. Änderungen aus dem Play Mode gehen verloren." : "Startet das Spiel – wie der Play-Knopf oben in Unity.";
            if (WorkshopGUI.Button(playLabel, isPlaying ? WorkshopGUI.Coral : WorkshopGUI.Green, 38f, true, playTip, 14))
                RunLater(() => EditorApplication.isPlaying = !EditorApplication.isPlaying);

            if (isPlaying)
                WorkshopGUI.Callout("Achtung: Änderungen im Play Mode werden beim Beenden zurückgesetzt!", WorkshopGUI.Orange, "console.warnicon");
        }

        void DrawStatusLine()
        {
            Color dot = isPlaying ? WorkshopGUI.Coral : WorkshopGUI.Green;
            string text = isPlaying
                ? "Play Mode – das Spiel läuft. Klicke im Game-Fenster auf die Knöpfe. Taste H zeigt die Steuerung."
                : "Edit Mode – hier baust und veränderst du die Welt.";

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(2f);
            Rect dotRect = GUILayoutUtility.GetRect(14f, 14f, GUILayout.Width(14f), GUILayout.Height(18f));
            if (Event.current.type == EventType.Repaint)
                GUI.DrawTexture(new Rect(dotRect.x, dotRect.y + 3f, 12f, 12f), WorkshopGUI.Circle(dot));
            GUILayout.Label("<b>Status:</b> " + text, WorkshopGUI.BodyRich);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(4f);
        }

        // ------------------------------------------------------------------ 2. Lektionen

        void DrawLessonPicker()
        {
            WorkshopGUI.SectionHeader("Lektionen", WorkshopGUI.Sky);
            GUILayout.Label("Wähle eine Lektion. Die Aufgaben stehen weiter unten.", WorkshopGUI.Small);
            GUILayout.Space(2f);

            WorkshopLesson[] lessons = WorkshopLessons.All;
            for (int row = 0; row < (lessons.Length + 1) / 2; row++)
            {
                Rect[] cells = WorkshopGUI.Row(2, 56f);
                for (int column = 0; column < 2; column++)
                {
                    int index = row * 2 + column;
                    if (index >= lessons.Length) continue;
                    DrawLessonButton(cells[column], index);
                }
            }
        }

        void DrawLessonButton(Rect rect, int index)
        {
            WorkshopLesson lesson = WorkshopLessons.All[index];
            int done = CountDone(index);
            int total = lesson.Steps.Length;
            bool selected = index == selectedLesson;
            Color color = LessonColor(index);
            string progress = done >= total ? "Fertig – super!" : done + " von " + total + " erledigt";
            string text = lesson.Title + "\n<size=10>" + progress + "</size>";

            if (WorkshopGUI.Button(rect, text, selected ? color : WorkshopGUI.Pale(color), true, "Zeigt die Aufgaben dieser Lektion.", 12))
            {
                AfterGui(() =>
                {
                    selectedLesson = index;
                    EditorPrefs.SetInt(SelectedLessonKey, index);
                    if (lessonSectionY > 0f) scroll.y = lessonSectionY - 6f;
                });
            }
        }

        // ------------------------------------------------------------------ 3. Bereich auswählen

        void DrawAreas()
        {
            WorkshopGUI.SectionHeader("Bereich auswählen", WorkshopGUI.Turquoise);

            if (!sceneOpen)
            {
                WorkshopGUI.Callout("Du bist gerade nicht in der Spielwiese. Öffne sie zuerst, dann kannst du zu den Bereichen springen.",
                    WorkshopGUI.Sky, "console.infoicon");
                if (sceneExists)
                {
                    if (WorkshopGUI.Button("Spielwiese öffnen", WorkshopGUI.Mint, 34f, !isPlaying))
                        RunLater(() => SandboxGenerator.OpenSandboxScene(true));
                }
                else if (WorkshopGUI.Button("Spielwiese erzeugen", WorkshopGUI.Mint, 34f, !isPlaying))
                {
                    RunLater(() => SandboxGenerator.GenerateInteractive());
                }
            }

            Rect[] row1 = WorkshopGUI.Row(2, 42f);
            AreaButton(row1[0], "Zum Baubereich", CameraSetup.CreativeName);
            AreaButton(row1[1], "Zum Physiklabor", CameraSetup.PhysicsName);
            Rect[] row2 = WorkshopGUI.Row(2, 42f);
            AreaButton(row2[0], "Zum Lichtlabor", CameraSetup.LightName);
            AreaButton(row2[1], "Zum Interaktionsbereich", CameraSetup.InteractionName);
            Rect[] row3 = WorkshopGUI.Row(1, 36f);
            AreaButton(row3[0], "Zur Weltübersicht", CameraSetup.OverviewName);

            if (isPlaying)
                GUILayout.Label("Im Play Mode bewegt das nur die Scene View. Im Game-Fenster fliegst du mit den Tasten 1 bis 5 zu den Bereichen.", WorkshopGUI.Small);
        }

        void AreaButton(Rect rect, string label, string viewpointName)
        {
            if (WorkshopGUI.Button(rect, label, AreaColor(viewpointName), sceneOpen, "Richtet die Scene View auf diesen Bereich aus."))
                GoToArea(viewpointName);
        }

        void GoToArea(string viewpointName)
        {
            if (!SandboxSceneView.FrameViewpoint(viewpointName))
            {
                ShowNotification(new GUIContent("Bereich nicht gefunden.\nIst die Spielwiese offen und gibt es eine Scene View?"));
                return;
            }
            SceneView view = SceneView.lastActiveSceneView;
            if (view == null) return;
            view.ShowNotification(new GUIContent(AreaLabel(viewpointName)), 1.2);
            if (!EditorApplication.isPlaying) view.Focus();
        }

        void ShowObject(string objectName, string label, bool frame = true)
        {
            bool found = frame ? SandboxSceneView.SelectAndFrame(objectName) : SandboxSceneView.Select(objectName);
            if (!found)
            {
                ShowNotification(new GUIContent("„" + objectName + "“ wurde nicht gefunden.\nVielleicht umbenannt oder gelöscht?"));
                return;
            }
            SceneView view = SceneView.lastActiveSceneView;
            if (view != null) view.ShowNotification(new GUIContent(label + ": " + objectName), 1.2);
        }

        // ------------------------------------------------------------------ 4. Aufgaben der Lektion

        void DrawLessonTasks()
        {
            WorkshopLesson lesson = WorkshopLessons.All[selectedLesson];
            Color color = LessonColor(selectedLesson);

            Rect header = WorkshopGUI.SectionHeader("Aufgaben der Lektion", color);
            if (Event.current.type == EventType.Repaint) lessonSectionY = header.y;

            GUILayout.Label(lesson.Title, WorkshopGUI.Title);
            GUILayout.Space(4f);

            // Hilfs-Knöpfe: Zum Bereich + Zeig mir
            Rect[] areaRow = WorkshopGUI.Row(1, 34f);
            if (WorkshopGUI.Button(areaRow[0], "Zum Bereich: " + AreaLabel(lesson.Area), color, sceneOpen, "Richtet die Scene View auf den Bereich dieser Lektion aus."))
                GoToArea(lesson.Area);

            ShowMeTarget[] targets = lesson.ShowMe ?? new ShowMeTarget[0];
            if (targets.Length > 0)
            {
                int columns = contentWidth >= 520f ? 3 : 2;
                for (int start = 0; start < targets.Length; start += columns)
                {
                    Rect[] cells = WorkshopGUI.Row(columns, 38f);
                    for (int c = 0; c < columns && start + c < targets.Length; c++)
                    {
                        ShowMeTarget showMe = targets[start + c];
                        if (WorkshopGUI.Button(cells[c], "Zeig mir: " + showMe.Label, WorkshopGUI.Pale(color), sceneOpen,
                                "Wählt „" + showMe.ObjectName + "“ aus und zeigt es in der Scene View.", 12))
                            ShowObject(showMe.ObjectName, showMe.Label, showMe.Frame);
                    }
                }
            }
            if (!sceneOpen) GUILayout.Label("Die Knöpfe funktionieren, sobald die Spielwiese geöffnet ist.", WorkshopGUI.Small);

            // Erklärung und Lernziel
            GUILayout.Label("Erklärung", WorkshopGUI.SubHeader);
            GUILayout.Label(lesson.Explanation, WorkshopGUI.Body);

            GUILayout.Label("Lernziel", WorkshopGUI.SubHeader);
            WorkshopGUI.BeginCard(color, 0.22f);
            GUILayout.Label(lesson.Goal, WorkshopGUI.BodyBold);
            WorkshopGUI.EndCard();

            // Schritte
            GUILayout.Label("Schritte – hake ab, was du geschafft hast", WorkshopGUI.SubHeader);
            int lessonNumber = selectedLesson + 1;
            bool[] lessonTicks = ticks[selectedLesson];
            for (int i = 0; i < lesson.Steps.Length; i++)
            {
                bool value = WorkshopGUI.StepToggle(lessonTicks[i], (i + 1) + ". " + lesson.Steps[i]);
                if (value != lessonTicks[i])
                {
                    lessonTicks[i] = value;
                    EditorPrefs.SetBool(WorkshopLessons.StepKey(lessonNumber, i + 1), value);
                }
            }

            GUILayout.Space(4f);
            int done = CountDone(selectedLesson);
            int total = lesson.Steps.Length;
            WorkshopGUI.ProgressBar(total > 0 ? (float)done / total : 0f, done + " von " + total + " Schritten erledigt", color);

            Rect[] resetRow = WorkshopGUI.Row(2, 26f);
            if (WorkshopGUI.Button(resetRow[1], "Häkchen zurücksetzen", WorkshopGUI.Neutral, done > 0, "Entfernt alle Häkchen dieser Lektion.", 11))
            {
                int lessonIndex = selectedLesson;
                RunLater(() =>
                {
                    if (EditorUtility.DisplayDialog("Häkchen zurücksetzen?",
                            "Alle Häkchen von Lektion " + WorkshopLessons.All[lessonIndex].Title + " werden entfernt.", "Ja, zurücksetzen", "Abbrechen"))
                        ResetTicks(lessonIndex);
                });
            }

            // Herausforderung
            GUILayout.Space(4f);
            WorkshopGUI.BeginCard(WorkshopGUI.Yellow, 0.35f);
            GUILayout.Label("Herausforderung", WorkshopGUI.SubHeaderTight);
            GUILayout.Label(lesson.Challenge, WorkshopGUI.Body);
            WorkshopGUI.EndCard();

            // Tipp für Erwachsene
            GUILayout.Space(2f);
            WorkshopGUI.BeginCard(WorkshopGUI.Neutral, EditorGUIUtility.isProSkin ? 0.12f : 0.35f);
            GUILayout.Label("Tipp für Erwachsene", WorkshopGUI.SmallBold);
            GUILayout.Label(lesson.AdultTip, WorkshopGUI.AdultText);
            WorkshopGUI.EndCard();
        }

        // ------------------------------------------------------------------ 5. Werkzeuge

        void DrawTools()
        {
            WorkshopGUI.SectionHeader("Werkzeuge", WorkshopGUI.Pink);
            if (isPlaying)
                WorkshopGUI.Callout("Achtung: Änderungen im Play Mode werden beim Beenden zurückgesetzt!", WorkshopGUI.Orange, "console.warnicon");

            // Auswahl zeigen
            if (WorkshopGUI.Button("Ausgewähltes Objekt zeigen", WorkshopGUI.Sky, 34f, true, "Wie Taste F: zoomt die Scene View auf das ausgewählte Objekt."))
                FrameSelection();
            GUILayout.Label(selectedName != null ? "Ausgewählt: " + selectedName : "Nichts ausgewählt – klicke ein Objekt in der Hierarchy oder Scene View an.",
                WorkshopGUI.Small);

            // Tageszeit
            GUILayout.Label("Tageszeit", WorkshopGUI.SubHeader);
            Rect[] times = WorkshopGUI.RowWeighted(36f, 1f, 1.7f, 1f);
            TimeButton(times[0], "Tag", TimeOfDay.Day, WorkshopGUI.Yellow);
            TimeButton(times[1], "Sonnenuntergang", TimeOfDay.Sunset, WorkshopGUI.Orange);
            TimeButton(times[2], "Nacht", TimeOfDay.Night, WorkshopGUI.NightBlue);
            if (!hasDayNight) GUILayout.Label("Kein DayNightController gefunden – öffne zuerst die Spielwiese.", WorkshopGUI.Small);

            // Weitere Werkzeuge
            GUILayout.Label("Szene und Hilfe", WorkshopGUI.SubHeader);
            Rect[] rowA = WorkshopGUI.Row(2, 40f);
            if (WorkshopGUI.Button(rowA[0], "Physik zurücksetzen", WorkshopGUI.Orange, isPlaying,
                    isPlaying ? "Setzt Kugeln, Dominosteine und Ziele zurück (wie Taste R)." : "Geht nur im Play Mode."))
                ResetPhysics();
            if (WorkshopGUI.Button(rowA[1], showHelp ? "Hilfe ausblenden" : "Hilfe", WorkshopGUI.Lavender, true, "Zeigt, wie du dich in der Scene View bewegst."))
            {
                AfterGui(() =>
                {
                    showHelp = !showHelp;
                    EditorPrefs.SetBool(ShowHelpKey, showHelp);
                });
            }

            Rect[] rowB = WorkshopGUI.Row(2, 40f);
            if (WorkshopGUI.Button(rowB[0], "Sicherung anlegen", WorkshopGUI.Mint, sceneExists && !isPlaying,
                    "Speichert eine Kopie deiner Szene im Ordner StudentSandbox/Backups."))
                RunLater(CreateBackup);
            if (WorkshopGUI.Button(rowB[1], "Ausgangsszene wiederherstellen", WorkshopGUI.Pale(WorkshopGUI.Coral), !isPlaying,
                    "Setzt die Spielwiese auf den Anfang zurück. Vorher wird automatisch eine Sicherung angelegt.", 12))
                RunLater(() => SandboxGenerator.RestoreOriginalScene());

            Rect[] rowC = WorkshopGUI.Row(2, 40f);
            if (WorkshopGUI.Button(rowC[0], "Spielwiese neu erzeugen", WorkshopGUI.Pale(WorkshopGUI.Turquoise), !isPlaying,
                    "Baut die ganze Spielwiese neu. Vorher wird gefragt und eine Sicherung angelegt.", 12))
                RunLater(() => SandboxGenerator.GenerateInteractive());
            if (WorkshopGUI.Button(rowC[1], "Anleitung öffnen", WorkshopGUI.Pale(WorkshopGUI.Sky), manualExists,
                    manualExists ? "Öffnet die Workshop-Anleitung (" + ManualPath + ")." : "Die Anleitung " + ManualPath + " gibt es noch nicht.", 12))
                OpenManual();

            if (!string.IsNullOrEmpty(backupInfo)) GUILayout.Label(backupInfo, WorkshopGUI.Small);

            if (showHelp) DrawHelp();
        }

        void TimeButton(Rect rect, string label, TimeOfDay time, Color color)
        {
            bool active = hasDayNight && currentTime == time;
            if (WorkshopGUI.Button(rect, label, active ? color : WorkshopGUI.Pale(color), hasDayNight, "Stellt die Tageszeit auf " + label + ".", 12))
            {
                if (!DayNightEditorUtility.Apply(time))
                    ShowNotification(new GUIContent("Kein DayNightController gefunden."));
            }
        }

        void DrawHelp()
        {
            GUILayout.Space(6f);
            WorkshopGUI.BeginCard(WorkshopGUI.Lavender, 0.25f);
            GUILayout.Label("So bewegst du dich in der Scene View", WorkshopGUI.SubHeaderTight);
            GUILayout.Label(
                "•  Rechte Maustaste gedrückt halten + W A S D = fliegen (Q / E = runter / hoch, Shift = schneller)\n" +
                "•  Mausrad = zoomen\n" +
                "•  Mittlere Maustaste gedrückt halten = Ansicht verschieben\n" +
                "•  Alt + linke Maustaste = um das Objekt herum drehen\n" +
                "•  F = ausgewähltes Objekt fokussieren (oder Doppelklick in der Hierarchy)",
                WorkshopGUI.Body);
            GUILayout.Space(4f);
            GUILayout.Label("Werkzeuge und Tastenkürzel", WorkshopGUI.SubHeaderTight);
            GUILayout.Label(
                "•  W = Verschieben, E = Drehen, R = Skalieren (Q = Hand)\n" +
                "•  Strg+D = duplizieren (Kopie machen)\n" +
                "•  Strg+Z = rückgängig, Strg+Y = wiederholen\n" +
                "•  Strg+S = Szene speichern",
                WorkshopGUI.Body);
            GUILayout.Space(4f);
            GUILayout.Label("Wichtig: Änderungen im Play Mode gehen beim Beenden verloren! Bau und verändere die Welt deshalb immer im Edit Mode.",
                WorkshopGUI.BodyBold);
            WorkshopGUI.EndCard();
        }

        // ------------------------------------------------------------------ Aktionen

        void FrameSelection()
        {
            if (Selection.activeGameObject == null)
            {
                ShowNotification(new GUIContent("Wähle zuerst ein Objekt aus\n(in der Hierarchy oder Scene View)."));
                return;
            }
            if (EditorUtility.IsPersistent(Selection.activeGameObject))
            {
                ShowNotification(new GUIContent("Das ist ein Prefab im Project-Fenster.\nZieh es zuerst in die Scene View."));
                return;
            }
            SceneView view = SceneView.lastActiveSceneView;
            if (view == null && SceneView.sceneViews.Count > 0) view = (SceneView)SceneView.sceneViews[0];
            if (view == null)
            {
                ShowNotification(new GUIContent("Es ist keine Scene View offen."));
                return;
            }
            view.FrameSelected();
        }

        void ResetPhysics()
        {
            PhysicsResetter resetter = Object.FindAnyObjectByType<PhysicsResetter>();
            if (resetter == null)
            {
                ShowNotification(new GUIContent("Kein PhysicsResetter in der Szene gefunden."));
                return;
            }
            resetter.ResetAll();
            ShowNotification(new GUIContent("Zurückgesetzt: " + resetter.LastResetCount + " Objekte"));
        }

        void CreateBackup()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Sicherung anlegen", "Bitte zuerst den Play Mode beenden.", "OK");
                return;
            }
            if (!SandboxGenerator.SceneExists)
            {
                EditorUtility.DisplayDialog("Sicherung anlegen", "Es gibt noch keine Spielwiese. Bitte zuerst „Spielwiese erzeugen“ klicken.", "OK");
                return;
            }

            Scene active = SceneManager.GetActiveScene();
            if (SandboxSceneView.IsSandboxSceneOpen && active.isDirty)
            {
                int choice = EditorUtility.DisplayDialogComplex("Sicherung anlegen",
                    "Deine Szene hat Änderungen, die noch nicht gespeichert sind.\n\n" +
                    "Soll sie zuerst gespeichert werden? Sonst enthält die Sicherung nur den zuletzt gespeicherten Stand.",
                    "Speichern und sichern", "Abbrechen", "Ohne Speichern sichern");
                if (choice == 1) return;
                if (choice == 0 && !EditorSceneManager.SaveScene(active))
                {
                    EditorUtility.DisplayDialog("Sicherung anlegen", "Die Szene konnte nicht gespeichert werden. Es wurde keine Sicherung angelegt.", "OK");
                    return;
                }
            }

            string path = SandboxGenerator.BackupScene();
            if (string.IsNullOrEmpty(path))
            {
                backupInfo = "Die Sicherung hat nicht geklappt. Details stehen in der Console.";
                EditorUtility.DisplayDialog("Sicherung anlegen", backupInfo, "OK");
                return;
            }

            backupInfo = "Letzte Sicherung: " + path;
            SceneAsset asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            if (asset != null) EditorGUIUtility.PingObject(asset);
            EditorUtility.DisplayDialog("Sicherung anlegen",
                "Fertig! Deine Sicherung liegt hier:\n" + path + "\n\nDu findest sie im Project-Fenster im Ordner StudentSandbox/Backups.", "OK");
        }

        void OpenManual()
        {
            TextAsset manual = AssetDatabase.LoadAssetAtPath<TextAsset>(ManualPath);
            if (manual == null)
            {
                ShowNotification(new GUIContent("Die Anleitung gibt es noch nicht."));
                return;
            }
            AssetDatabase.OpenAsset(manual);
        }

        // ------------------------------------------------------------------ Häkchen

        void LoadTicks()
        {
            WorkshopLesson[] lessons = WorkshopLessons.All;
            ticks = new bool[lessons.Length][];
            for (int l = 0; l < lessons.Length; l++)
            {
                int count = lessons[l].Steps != null ? lessons[l].Steps.Length : 0;
                ticks[l] = new bool[count];
                for (int s = 0; s < count; s++)
                    ticks[l][s] = EditorPrefs.GetBool(WorkshopLessons.StepKey(l + 1, s + 1), false);
            }
        }

        void ResetTicks(int lessonIndex)
        {
            if (ticks == null || lessonIndex < 0 || lessonIndex >= ticks.Length) return;
            for (int s = 0; s < ticks[lessonIndex].Length; s++)
            {
                ticks[lessonIndex][s] = false;
                EditorPrefs.DeleteKey(WorkshopLessons.StepKey(lessonIndex + 1, s + 1));
            }
        }

        int CountDone(int lessonIndex)
        {
            if (ticks == null || lessonIndex < 0 || lessonIndex >= ticks.Length) return 0;
            int done = 0;
            foreach (bool tick in ticks[lessonIndex])
                if (tick) done++;
            return done;
        }

        // ------------------------------------------------------------------ Farben und Namen der Bereiche

        static Color AreaColor(string viewpointName)
        {
            switch (viewpointName)
            {
                case CameraSetup.CreativeName: return WorkshopGUI.Mint;
                case CameraSetup.PhysicsName: return WorkshopGUI.Orange;
                case CameraSetup.LightName: return WorkshopGUI.Lavender;
                case CameraSetup.InteractionName: return WorkshopGUI.Turquoise;
                default: return WorkshopGUI.Neutral;
            }
        }

        static Color LessonColor(int lessonIndex)
        {
            string area = WorkshopLessons.All[lessonIndex].Area;
            return area == CameraSetup.OverviewName ? WorkshopGUI.Pink : AreaColor(area);
        }

        static string AreaLabel(string viewpointName)
        {
            switch (viewpointName)
            {
                case CameraSetup.CreativeName: return "Baubereich";
                case CameraSetup.PhysicsName: return "Physiklabor";
                case CameraSetup.LightName: return "Lichtlabor";
                case CameraSetup.InteractionName: return "Interaktionsbereich";
                default: return "Weltübersicht";
            }
        }
    }

    /// <summary>
    /// Gemeinsame Farben, Stile und Zeichen-Helfer für das Workshop-Fenster und die Inspectors der Spielwiese.
    /// Alle Texturen werden zur Laufzeit erzeugt (kleine abgerundete Rechtecke) und nie gespeichert.
    /// </summary>
    internal static class WorkshopGUI
    {
        // Farben passend zur Material-Palette der Spielwiese (SandboxMaterials).
        public static readonly Color Mint = Hex("#8EE3C0");
        public static readonly Color Orange = Hex("#FF9F45");
        public static readonly Color Lavender = Hex("#B8A1E8");
        public static readonly Color Turquoise = Hex("#36C9C6");
        public static readonly Color Yellow = Hex("#FFD966");
        public static readonly Color Coral = Hex("#FF6B5B");
        public static readonly Color Sky = Hex("#74C0F0");
        public static readonly Color Pink = Hex("#FF8FB8");
        public static readonly Color Green = Hex("#7CC657");
        public static readonly Color Neutral = Hex("#CDD3DC");
        public static readonly Color NightBlue = Hex("#3D4F8C");
        public static readonly Color Charcoal = Hex("#3B4252");
        public static readonly Color DarkText = Hex("#2E3440");

        const int TextureSize = 16;
        const int Corner = 6;
        public const float Gap = 6f;

        static readonly Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
        static readonly Dictionary<string, GUIStyle> styles = new Dictionary<string, GUIStyle>();
        static bool stylesForProSkin;

        public static GUIStyle PageStyle;
        public static GUIStyle BigTitle;
        public static GUIStyle Title;
        public static GUIStyle SectionTitle;
        public static GUIStyle SubHeader;
        public static GUIStyle SubHeaderTight;
        public static GUIStyle Body;
        public static GUIStyle BodyRich;
        public static GUIStyle BodyBold;
        public static GUIStyle Small;
        public static GUIStyle SmallBold;
        public static GUIStyle AdultText;
        public static GUIStyle StepText;
        public static GUIStyle StepTextDone;
        public static GUIStyle CenteredBold;

        static WorkshopGUI()
        {
            AssemblyReloadEvents.beforeAssemblyReload += DestroyTextures;
        }

        public static Color Hex(string hex)
        {
            return ColorUtility.TryParseHtmlString(hex, out Color color) ? color : Color.magenta;
        }

        /// <summary>Blasse Variante einer Farbe (für nicht gewählte Knöpfe).</summary>
        public static Color Pale(Color color)
        {
            return EditorGUIUtility.isProSkin
                ? Color.Lerp(color, new Color(0.26f, 0.27f, 0.29f, 1f), 0.5f)
                : Color.Lerp(color, Color.white, 0.55f);
        }

        public static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        /// <summary>Dunkle oder weiße Schrift – je nachdem, was auf der Hintergrundfarbe besser lesbar ist.</summary>
        public static Color TextOn(Color background)
        {
            float luminance = 0.299f * background.r + 0.587f * background.g + 0.114f * background.b;
            return luminance > 0.6f ? DarkText : Color.white;
        }

        // ------------------------------------------------------------------ Stile

        /// <summary>Muss am Anfang jedes OnGUI / OnInspectorGUI aufgerufen werden.</summary>
        public static void Ensure()
        {
            bool pro = EditorGUIUtility.isProSkin;
            if (BigTitle != null && stylesForProSkin == pro) return;
            stylesForProSkin = pro;
            styles.Clear();

            Color text = pro ? new Color(0.9f, 0.91f, 0.93f) : DarkText;
            Color muted = pro ? new Color(0.68f, 0.7f, 0.74f) : new Color(0.36f, 0.39f, 0.45f);

            PageStyle = new GUIStyle { padding = new RectOffset(12, 12, 6, 6) };

            BigTitle = Label(20, FontStyle.Bold, text);
            Title = Label(16, FontStyle.Bold, text);
            SectionTitle = Label(15, FontStyle.Bold, text);
            SectionTitle.wordWrap = false;
            SectionTitle.alignment = TextAnchor.MiddleLeft;

            SubHeader = Label(13, FontStyle.Bold, text);
            SubHeader.margin = new RectOffset(4, 4, 10, 2);
            SubHeaderTight = Label(13, FontStyle.Bold, text);
            SubHeaderTight.margin = new RectOffset(4, 4, 0, 2);

            Body = Label(12, FontStyle.Normal, text);
            BodyRich = Label(12, FontStyle.Normal, text);
            BodyRich.richText = true;
            BodyBold = Label(12, FontStyle.Bold, text);
            Small = Label(11, FontStyle.Normal, muted);
            SmallBold = Label(11, FontStyle.Bold, muted);
            AdultText = Label(11, FontStyle.Italic, muted);

            StepText = Label(12, FontStyle.Normal, text);
            StepText.padding = new RectOffset(2, 2, 1, 1);
            StepTextDone = Label(12, FontStyle.Normal, muted);
            StepTextDone.padding = new RectOffset(2, 2, 1, 1);

            CenteredBold = Label(13, FontStyle.Bold, DarkText);
            CenteredBold.wordWrap = false;
            CenteredBold.alignment = TextAnchor.MiddleCenter;
            CenteredBold.clipping = TextClipping.Clip;
        }

        static GUIStyle Label(int fontSize, FontStyle fontStyle, Color color)
        {
            var style = new GUIStyle(EditorStyles.label)
            {
                fontSize = fontSize,
                fontStyle = fontStyle,
                wordWrap = true,
                richText = false,
            };
            style.normal.textColor = color;
            style.hover.textColor = color;
            style.active.textColor = color;
            style.focused.textColor = color;
            return style;
        }

        /// <summary>Großer, farbiger Knopf mit abgerundeten Ecken.</summary>
        public static GUIStyle ButtonStyle(Color color, int fontSize = 13)
        {
            string key = "button_" + ColorUtility.ToHtmlStringRGBA(color) + "_" + fontSize;
            if (styles.TryGetValue(key, out GUIStyle cached) && cached.normal.background != null) return cached;

            var style = new GUIStyle
            {
                border = new RectOffset(Corner, Corner, Corner, Corner),
                padding = new RectOffset(8, 8, 4, 6),
                margin = new RectOffset(0, 0, 0, 0),
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = fontSize,
                wordWrap = true,
                richText = true,
                clipping = TextClipping.Clip,
            };
            Color text = TextOn(color);
            style.normal.background = Rounded(color, Corner, true);
            style.hover.background = Rounded(Color.Lerp(color, Color.white, 0.18f), Corner, true);
            style.active.background = Rounded(Color.Lerp(color, Color.black, 0.12f), Corner, false);
            style.normal.textColor = text;
            style.hover.textColor = text;
            style.active.textColor = text;
            styles[key] = style;
            return style;
        }

        /// <summary>Abgerundete, getönte Fläche (Karte) mit Innenabstand.</summary>
        public static GUIStyle CardStyle(Color color, float alpha, int paddingLeft = 10)
        {
            string key = "card_" + ColorUtility.ToHtmlStringRGBA(color) + "_" + alpha.ToString("0.00") + "_" + paddingLeft;
            if (styles.TryGetValue(key, out GUIStyle cached) && cached.normal.background != null) return cached;

            var style = new GUIStyle
            {
                border = new RectOffset(Corner, Corner, Corner, Corner),
                padding = new RectOffset(paddingLeft, 10, 8, 8),
                margin = new RectOffset(0, 0, 3, 3),
            };
            style.normal.background = Rounded(WithAlpha(color, alpha), Corner, false);
            styles[key] = style;
            return style;
        }

        static GUIStyle SolidStyle(Color color)
        {
            string key = "solid_" + ColorUtility.ToHtmlStringRGBA(color);
            if (styles.TryGetValue(key, out GUIStyle cached) && cached.normal.background != null) return cached;
            var style = new GUIStyle { border = new RectOffset(Corner, Corner, Corner, Corner) };
            style.normal.background = Rounded(color, Corner, false);
            styles[key] = style;
            return style;
        }

        // ------------------------------------------------------------------ Texturen

        public static Texture2D Circle(Color color) => Rounded(color, TextureSize / 2, false);

        /// <summary>Kleine Textur (16 x 16) mit abgerundeten Ecken. shade = dunklere Unterkante (sieht "drückbar" aus).</summary>
        public static Texture2D Rounded(Color color, int radius, bool shade)
        {
            string key = ColorUtility.ToHtmlStringRGBA(color) + "_" + radius + "_" + shade;
            if (textures.TryGetValue(key, out Texture2D cached) && cached != null) return cached;

            var texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                name = "WorkshopGUI_" + key,
            };
            var pixels = new Color[TextureSize * TextureSize];
            Color shadeColor = Color.Lerp(color, Color.black, 0.18f);
            shadeColor.a = color.a;
            for (int y = 0; y < TextureSize; y++)
            {
                for (int x = 0; x < TextureSize; x++)
                {
                    float px = x + 0.5f;
                    float py = y + 0.5f;
                    float dx = Mathf.Max(radius - px, px - (TextureSize - radius), 0f);
                    float dy = Mathf.Max(radius - py, py - (TextureSize - radius), 0f);
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    float coverage = Mathf.Clamp01(radius - distance + 0.5f);
                    // y = 0 ist die unterste Zeile der Textur.
                    Color pixel = shade && y < 2 ? shadeColor : color;
                    pixel.a *= coverage;
                    pixels[y * TextureSize + x] = pixel;
                }
            }
            texture.SetPixels(pixels);
            texture.Apply(false, false);
            textures[key] = texture;
            return texture;
        }

        static void DestroyTextures()
        {
            foreach (Texture2D texture in textures.Values)
            {
                if (texture != null) Object.DestroyImmediate(texture);
            }
            textures.Clear();
            styles.Clear();
        }

        // ------------------------------------------------------------------ Zeichen-Helfer

        /// <summary>Reserviert eine Zeile und teilt sie in gleich breite Zellen.</summary>
        public static Rect[] Row(int count, float height)
        {
            var weights = new float[Mathf.Max(1, count)];
            for (int i = 0; i < weights.Length; i++) weights[i] = 1f;
            return RowWeighted(height, weights);
        }

        /// <summary>Reserviert eine Zeile und teilt sie nach Gewichten (z.B. 1, 1.7, 1).</summary>
        public static Rect[] RowWeighted(float height, params float[] weights)
        {
            Rect row = GUILayoutUtility.GetRect(0f, height, GUILayout.ExpandWidth(true));
            GUILayout.Space(Gap);
            float total = 0f;
            foreach (float weight in weights) total += Mathf.Max(0.01f, weight);
            float available = row.width - Gap * (weights.Length - 1);
            var cells = new Rect[weights.Length];
            float x = row.x;
            for (int i = 0; i < weights.Length; i++)
            {
                float width = available * Mathf.Max(0.01f, weights[i]) / total;
                cells[i] = new Rect(x, row.y, width, row.height);
                x += width + Gap;
            }
            return cells;
        }

        /// <summary>Farbiger Knopf an einer festen Stelle.</summary>
        public static bool Button(Rect rect, string text, Color color, bool enabled = true, string tooltip = null, int fontSize = 13)
        {
            using (new EditorGUI.DisabledScope(!enabled))
            {
                if (enabled) EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
                return GUI.Button(rect, new GUIContent(text, tooltip), ButtonStyle(color, fontSize));
            }
        }

        /// <summary>Farbiger Knopf über die ganze Breite.</summary>
        public static bool Button(string text, Color color, float height, bool enabled = true, string tooltip = null, int fontSize = 13)
        {
            Rect rect = GUILayoutUtility.GetRect(0f, height, GUILayout.ExpandWidth(true));
            GUILayout.Space(Gap);
            return Button(rect, text, color, enabled, tooltip, fontSize);
        }

        /// <summary>Überschrift eines Abschnitts: getönter Balken mit farbigem Streifen links.</summary>
        public static Rect SectionHeader(string title, Color color)
        {
            GUILayout.Space(16f);
            Rect rect = GUILayoutUtility.GetRect(0f, 32f, GUILayout.ExpandWidth(true));
            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(rect, WithAlpha(color, EditorGUIUtility.isProSkin ? 0.2f : 0.3f));
                EditorGUI.DrawRect(new Rect(rect.x, rect.y, 6f, rect.height), color);
                GUI.DrawTexture(new Rect(rect.x + 16f, rect.y + 10f, 12f, 12f), Circle(color));
                GUI.Label(new Rect(rect.x + 38f, rect.y, rect.width - 42f, rect.height), title, SectionTitle);
            }
            GUILayout.Space(6f);
            return rect;
        }

        /// <summary>Bunter Streifen in den Farben der vier Bereiche (oben im Fenster).</summary>
        public static void RainbowBar(float height)
        {
            Rect rect = GUILayoutUtility.GetRect(0f, height, GUILayout.ExpandWidth(true));
            GUILayout.Space(8f);
            if (Event.current.type != EventType.Repaint) return;
            Color[] colors = { Mint, Orange, Lavender, Turquoise, Pink };
            float width = rect.width / colors.Length;
            for (int i = 0; i < colors.Length; i++)
                EditorGUI.DrawRect(new Rect(rect.x + i * width, rect.y, Mathf.Ceil(width), rect.height), colors[i]);
        }

        /// <summary>Hinweis-Karte mit Symbol und fettem Text.</summary>
        public static void Callout(string text, Color color, string iconName)
        {
            BeginCard(color, EditorGUIUtility.isProSkin ? 0.25f : 0.35f);
            EditorGUILayout.BeginHorizontal();
            if (!string.IsNullOrEmpty(iconName))
                GUILayout.Label(EditorGUIUtility.IconContent(iconName), GUILayout.Width(24f), GUILayout.Height(24f));
            GUILayout.Label(text, BodyBold);
            EditorGUILayout.EndHorizontal();
            EndCard();
            GUILayout.Space(2f);
        }

        public static void BeginCard(Color color, float alpha)
        {
            EditorGUILayout.BeginVertical(CardStyle(color, alpha));
        }

        public static void EndCard()
        {
            EditorGUILayout.EndVertical();
        }

        /// <summary>Farbiges Schild mit großem, zentriertem Text (z.B. die aktuelle Ampelphase).</summary>
        public static void Pill(string text, Color color, float height = 30f, int fontSize = 15)
        {
            Rect rect = GUILayoutUtility.GetRect(0f, height, GUILayout.ExpandWidth(true));
            GUILayout.Space(Gap);
            if (Event.current.type != EventType.Repaint) return;
            SolidStyle(color).Draw(rect, GUIContent.none, false, false, false, false);
            CenteredBold.fontSize = fontSize;
            CenteredBold.normal.textColor = TextOn(color);
            GUI.Label(rect, text, CenteredBold);
        }

        /// <summary>Fortschrittsbalken in einer Farbe, mit Text in der Mitte.</summary>
        public static void ProgressBar(float fraction, string label, Color color, float height = 22f)
        {
            Rect rect = GUILayoutUtility.GetRect(0f, height, GUILayout.ExpandWidth(true));
            GUILayout.Space(Gap);
            if (Event.current.type != EventType.Repaint) return;

            SolidStyle(Color.Lerp(color, Color.white, 0.72f)).Draw(rect, GUIContent.none, false, false, false, false);
            fraction = Mathf.Clamp01(fraction);
            if (fraction > 0f)
            {
                var fill = new Rect(rect.x, rect.y, Mathf.Max(2f * Corner, rect.width * fraction), rect.height);
                SolidStyle(color).Draw(fill, GUIContent.none, false, false, false, false);
            }
            CenteredBold.fontSize = 12;
            CenteredBold.normal.textColor = DarkText;
            GUI.Label(rect, label, CenteredBold);
        }

        /// <summary>Kästchen zum Abhaken mit umbrechendem Text. Klick auf den Text schaltet ebenfalls um.</summary>
        public static bool StepToggle(bool value, string text)
        {
            EditorGUILayout.BeginHorizontal();
            bool result = EditorGUILayout.Toggle(value, GUILayout.Width(18f));
            var content = new GUIContent(text);
            GUIStyle style = value ? StepTextDone : StepText;
            Rect labelRect = GUILayoutUtility.GetRect(content, style);
            EditorGUIUtility.AddCursorRect(labelRect, MouseCursor.Link);
            if (GUI.Button(labelRect, content, style)) result = !value;
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(2f);
            return result;
        }
    }
}
