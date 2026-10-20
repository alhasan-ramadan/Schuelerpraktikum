using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace StudentSandbox.Tests
{
    /// <summary>
    /// Play-Mode-Tests: Vor jedem Test wird die Spielwiese frisch geladen, dann wird geprüft,
    /// ob Physik, Schalter, Tür, Ampel, Kristall, Plattform und Tag/Nacht wirklich funktionieren.
    /// Die ganze Reihe dauert deutlich unter 90 Sekunden.
    /// </summary>
    public class SandboxPlayModeTests
    {
        const string ScenePath = "Assets/StudentSandbox/Scenes/StudentSandbox.unity";
        const float SceneLoadTimeout = 60f;

        // ------------------------------------------------------------------ Szene laden

        [UnitySetUp]
        public IEnumerator LoadSandboxScene()
        {
#if UNITY_EDITOR
            if (!System.IO.File.Exists(ScenePath))
                Assert.Ignore("Die Spielwiese wurde noch nicht erzeugt (" + ScenePath + ").");

            Time.timeScale = 1f;
            AsyncOperation load = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            Assert.IsNotNull(load, "Die Szene konnte nicht geladen werden: " + ScenePath);

            float deadline = Time.realtimeSinceStartup + SceneLoadTimeout;
            while (!load.isDone)
            {
                if (Time.realtimeSinceStartup > deadline) Assert.Fail("Die Szene lädt zu lange: " + ScenePath);
                yield return null;
            }

            // Ein Frame warten, damit alle Start()-Methoden gelaufen sind.
            yield return null;
            Assert.AreEqual(ScenePath, SceneManager.GetActiveScene().path, "Die Spielwiese ist nicht die aktive Szene.");
#else
            Assert.Ignore("Die Play-Mode-Tests der Spielwiese laufen nur im Unity-Editor.");
            yield break;
#endif
        }

        // ------------------------------------------------------------------ D, E, F, G: Murmelbahn

        [UnityTest]
        public IEnumerator BallStartsRolling()
        {
            PhysicsBall ball = MainBall();
            Assert.IsNotNull(ball, "Die rote Kugel 'PhysicsBall' fehlt.");
            Vector3 start = ball.HasStartPose ? ball.StartPosition : ball.transform.position;

            yield return WaitUntilOrTimeout(() => Vector3.Distance(ball.transform.position, start) > 0.5f, 1.5f);

            float moved = Vector3.Distance(ball.transform.position, start);
            Assert.Greater(moved, 0.5f, "Die Kugel hat sich in 1,5 s nur " + moved.ToString("0.00") + " m bewegt – liegt sie auf der Rampe?");
        }

        [UnityTest]
        public IEnumerator BallReachesGoal()
        {
            GoalZone goal = MainGoal();
            Assert.IsNotNull(goal, "Die Zielzone 'GoalZone' fehlt.");
            Assert.IsFalse(goal.IsReached, "Das Ziel ist schon beim Start erreicht.");
            SandboxHUD.ShowMessage(string.Empty, 0.5f); // alte Meldungen aus anderen Tests vergessen

            string messageWhenReached = null;
            float end = Time.time + 12f;
            while (Time.time < end)
            {
                if (goal.IsReached)
                {
                    messageWhenReached = SandboxHUD.LastMessage;
                    break;
                }
                yield return null;
            }

            PhysicsBall ball = MainBall();
            Assert.IsTrue(goal.IsReached,
                "Die Kugel hat das Ziel nicht innerhalb von 12 s erreicht" + (ball != null ? " (sie liegt bei " + ball.transform.position + ")." : "."));
            Assert.IsNotNull(messageWhenReached, "Es wurde keine Meldung angezeigt.");
            StringAssert.Contains("Ziel", messageWhenReached, "Die Erfolgsmeldung sollte das Wort 'Ziel' enthalten.");
        }

        /// <summary>
        /// Die Workshop-Aufgabe "Rampe steiler/flacher machen": Auch mit veränderter Rampe (Rotation Z)
        /// soll die rote Kugel nach einem Reset das Ziel erreichen.
        /// </summary>
        [UnityTest]
        public IEnumerator BallReachesGoalWithSteeperRamp() => BallReachesGoalWithRampAngle(-30f);

        [UnityTest]
        public IEnumerator BallReachesGoalWithFlatterRamp() => BallReachesGoalWithRampAngle(-15f);

        [UnityTest]
        public IEnumerator BallReachesGoalWithVeryFlatRamp() => BallReachesGoalWithRampAngle(-12f);

        IEnumerator BallReachesGoalWithRampAngle(float rampAngleZ)
        {
            GameObject ramp = FindObject("Ramp_Main");
            GoalZone goal = MainGoal();
            PhysicsResetter resetter = Object.FindAnyObjectByType<PhysicsResetter>();
            Assert.IsNotNull(ramp, "Die Rampe 'Ramp_Main' fehlt.");
            Assert.IsNotNull(goal, "Die Zielzone 'GoalZone' fehlt.");
            Assert.IsNotNull(resetter, "Kein PhysicsResetter in der Szene.");

            Vector3 euler = ramp.transform.localEulerAngles;
            ramp.transform.localRotation = Quaternion.Euler(euler.x, euler.y, rampAngleZ);
            yield return new WaitForFixedUpdate();
            resetter.ResetAll();

            PhysicsBall ball = MainBall();
            var path = new System.Text.StringBuilder();
            float end = Time.time + 15f;
            float nextSample = 0f;
            while (Time.time < end && !goal.IsReached)
            {
                if (ball != null && Time.time >= nextSample)
                {
                    Vector3 local = ball.transform.position - ramp.transform.parent.parent.position;
                    path.Append(local.ToString("F2")).Append(' ');
                    nextSample = Time.time + 0.25f;
                }
                yield return null;
            }

            Assert.IsTrue(goal.IsReached,
                "Mit Rampen-Rotation Z = " + rampAngleZ + " erreicht die Kugel das Ziel nicht" +
                (ball != null ? " (sie liegt bei " + ball.transform.position + "). Weg: " + path : "."));
        }

        /// <summary>
        /// Hindernis-Parcours: Liegt die TargetZone im Weg der blauen Kugel, muss sie das Ziel erkennen
        /// (zeigt, dass die Aufgabe lösbar ist und die Zielerkennung jede PhysicsBall akzeptiert).
        /// </summary>
        [UnityTest]
        public IEnumerator BlueBallTriggersTargetZoneInItsPath()
        {
            GoalZone target = FindComponent<GoalZone>("TargetZone");
            PhysicsBall blue = FindComponent<PhysicsBall>("PhysicsBall_Blue");
            PhysicsResetter resetter = Object.FindAnyObjectByType<PhysicsResetter>();
            Assert.IsNotNull(target, "Die Zielzone 'TargetZone' fehlt.");
            Assert.IsNotNull(blue, "Die blaue Kugel 'PhysicsBall_Blue' fehlt.");
            Assert.IsNotNull(resetter, "Kein PhysicsResetter in der Szene.");

            // Ziel genau in die Rollrichtung der blauen Kugel stellen (sie rollt die Rampe hinunter Richtung Süden).
            Vector3 start = blue.StartPosition;
            target.transform.position = new Vector3(start.x, target.transform.position.y, start.z - 6.5f);
            yield return new WaitForFixedUpdate();
            resetter.ResetAll();

            float end = Time.time + 10f;
            while (Time.time < end && !target.IsReached) yield return null;
            Assert.IsTrue(target.IsReached, "Die blaue Kugel hat die TargetZone in ihrem Weg nicht ausgelöst (Kugel bei " + blue.transform.position + ").");
        }

        /// <summary>
        /// Nach Play schwingt der Hammer von allein. Nach einem Reset wartet er – und der DOMINO-Knopf startet ihn wieder.
        /// </summary>
        [UnityTest]
        public IEnumerator DominoButtonRestartsHammerAfterReset()
        {
            DominoStarter hammer = Object.FindAnyObjectByType<DominoStarter>();
            ClickableSwitch dominoButton = FindComponent<ClickableSwitch>("DominoButton");
            PhysicsResetter resetter = Object.FindAnyObjectByType<PhysicsResetter>();
            Assert.IsNotNull(hammer, "Kein DominoStarter (Hammer) in der Szene.");
            Assert.IsNotNull(dominoButton, "Der Knopf 'DominoButton' fehlt.");
            Assert.IsNotNull(resetter, "Kein PhysicsResetter in der Szene.");

            float end = Time.time + 5f;
            while (Time.time < end && !hammer.HasSwung) yield return null;
            Assert.IsTrue(hammer.HasSwung, "Der Hammer ist nach Play nicht von allein losgeschwungen.");
            yield return new WaitForSeconds(1.5f);

            resetter.ResetAll();
            yield return new WaitForSeconds(2.5f);
            Assert.IsFalse(hammer.HasSwung, "Nach dem Reset soll der Hammer auf den DOMINO-Knopf warten.");

            dominoButton.Press();
            end = Time.time + 3f;
            while (Time.time < end && !hammer.HasSwung) yield return null;
            Assert.IsTrue(hammer.HasSwung, "Der DOMINO-Knopf hat den Hammer nicht gestartet.");
        }

        [UnityTest]
        public IEnumerator ResetRestoresBall()
        {
            PhysicsBall ball = MainBall();
            GoalZone goal = MainGoal();
            PhysicsResetter resetter = Object.FindAnyObjectByType<PhysicsResetter>();
            Assert.IsNotNull(ball, "Die rote Kugel 'PhysicsBall' fehlt.");
            Assert.IsNotNull(resetter, "Kein PhysicsResetter in der Szene.");
            var body = ball.GetComponent<Rigidbody>();
            Assert.IsNotNull(body, "Die Kugel hat keinen Rigidbody.");
            Vector3 start = ball.StartPosition;

            yield return WaitUntilOrTimeout(() => Vector3.Distance(body.position, start) > 1f, 3f);
            Assert.Greater(Vector3.Distance(body.position, start), 1f, "Die Kugel ist nicht losgerollt.");

            resetter.ResetAll();

            // Sofort nach dem Reset.
            Assert.LessOrEqual(Vector3.Distance(body.position, start), 0.05f, "Die Kugel ist nach dem Reset nicht am Start.");
            Assert.Less(body.linearVelocity.magnitude, 0.01f, "Die Kugel ist nach dem Reset noch in Bewegung.");
            if (goal != null) Assert.IsFalse(goal.IsReached, "Das Ziel ist nach dem Reset noch als erreicht markiert.");

            // Nach dem nächsten Physik-Schritt: höchstens ein Schritt Schwerkraft ist dazugekommen.
            yield return new WaitForFixedUpdate();
            float oneStepSpeed = Physics.gravity.magnitude * Time.fixedDeltaTime * 1.5f + 0.02f;
            Assert.LessOrEqual(Vector3.Distance(body.position, start), 0.05f, "Die Kugel ist nach dem Reset weggesprungen.");
            Assert.LessOrEqual(body.linearVelocity.magnitude, oneStepSpeed, "Die Kugel hat nach dem Reset noch Schwung.");
            if (goal != null) Assert.IsFalse(goal.IsReached, "Das Ziel ist nach dem Reset noch als erreicht markiert.");
        }

        // ------------------------------------------------------------------ Domino

        [UnityTest]
        public IEnumerator DominoesFall()
        {
            GameObject last = FindObject("Domino_10");
            Assert.IsNotNull(last, "Der Dominostein 'Domino_10' fehlt.");

            DominoStarter starter = Object.FindAnyObjectByType<DominoStarter>();
            Assert.IsNotNull(starter, "Kein Domino-Hammer (DominoStarter) in der Szene.");
            if (!starter.StartAutomatically) starter.StartSwing();

            Transform tile = last.transform;
            yield return WaitUntilOrTimeout(() => Vector3.Angle(tile.up, Vector3.up) > 45f, 10f);

            float tilt = Vector3.Angle(tile.up, Vector3.up);
            Assert.Greater(tilt, 45f, "Der letzte Dominostein ist nicht umgefallen (Neigung " + tilt.ToString("0") + "°).");
        }

        // ------------------------------------------------------------------ H: Tag und Nacht

        [UnityTest]
        public IEnumerator DayNightWorksInPlayMode()
        {
            DayNightController dayNight = Object.FindAnyObjectByType<DayNightController>();
            Assert.IsNotNull(dayNight, "Kein DayNightController in der Szene.");
            List<LampController> lamps = Object.FindObjectsByType<LampController>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(lamp => lamp.FollowDayNight)
                .ToList();
            Assert.IsNotEmpty(lamps, "Keine Lampe folgt der Tageszeit.");

            dayNight.SetNight();
            yield return null;
            LampController litLamp = lamps.FirstOrDefault(lamp => lamp.IsOn);
            Assert.IsNotNull(litLamp, "Nachts ist keine Lampe angegangen.");
            if (litLamp.LampLight != null) Assert.IsTrue(litLamp.LampLight.enabled, "Lampe an, aber Licht aus: " + litLamp.name);
            if (dayNight.Night.skybox != null) Assert.AreSame(dayNight.Night.skybox, RenderSettings.skybox, "Nachts wurde der Himmel nicht gewechselt.");

            dayNight.SetDay();
            yield return null;
            List<string> stillOn = lamps.Where(lamp => lamp.IsOn).Select(lamp => lamp.name).ToList();
            Assert.IsEmpty(stillOn, "Tagsüber leuchten noch Lampen: " + string.Join(", ", stillOn));
            if (litLamp.LampLight != null) Assert.IsFalse(litLamp.LampLight.enabled, "Tagsüber leuchtet noch das Licht von " + litLamp.name);
            if (dayNight.Day.skybox != null) Assert.AreSame(dayNight.Day.skybox, RenderSettings.skybox, "Tagsüber wurde der Himmel nicht zurückgestellt.");
        }

        // ------------------------------------------------------------------ I: Tür

        [UnityTest]
        public IEnumerator DoorSwitchOpensDoor()
        {
            DoorController door = FindComponent<DoorController>("AutomaticDoor");
            ClickableSwitch doorSwitch = FindComponent<ClickableSwitch>("DoorSwitch");
            Assert.IsNotNull(door, "Die Tür 'AutomaticDoor' (DoorController) fehlt.");
            Assert.IsNotNull(doorSwitch, "Der Schalter 'DoorSwitch' fehlt.");

            Transform doorTransform = door.Door;
            Vector3 closedPosition = doorTransform.position;
            Assert.IsFalse(door.IsOpen, "Die Tür ist schon beim Start offen.");
            float wait = Mathf.Max(2f, 1.5f / Mathf.Max(0.05f, door.OpenSpeed));

            doorSwitch.Press();
            yield return new WaitForSeconds(wait);
            Assert.IsTrue(door.IsOpen, "Die Tür hat sich nach dem Klick nicht geöffnet.");
            Assert.GreaterOrEqual(door.OpenAmount, 0.99f, "Die Tür ist nicht ganz aufgegangen.");
            float moved = Vector3.Distance(doorTransform.position, closedPosition);
            Assert.GreaterOrEqual(moved, 2f, "Die Tür hat sich nur " + moved.ToString("0.00") + " m bewegt.");

            doorSwitch.Press();
            yield return new WaitForSeconds(wait);
            Assert.IsFalse(door.IsOpen, "Die Tür hat sich beim zweiten Klick nicht geschlossen.");
            Assert.LessOrEqual(door.OpenAmount, 0.01f, "Die Tür ist nicht ganz zugegangen.");
            Assert.LessOrEqual(Vector3.Distance(doorTransform.position, closedPosition), 0.01f, "Die Tür steht nicht wieder an ihrem Platz.");
        }

        // ------------------------------------------------------------------ J: Ampel

        [UnityTest]
        public IEnumerator TrafficLightAdvances()
        {
            TrafficLightController trafficLight = FindComponent<TrafficLightController>("TrafficLight");
            if (trafficLight == null) trafficLight = Object.FindAnyObjectByType<TrafficLightController>();
            Assert.IsNotNull(trafficLight, "Keine Ampel (TrafficLightController) in der Szene.");
            trafficLight.AutoCycle = false;

            var switches = new List<ClickableSwitch>();
            foreach (string switchName in new[] { "PedestrianButton", "Desk_TrafficButton" })
            {
                ClickableSwitch candidate = FindComponent<ClickableSwitch>(switchName);
                if (candidate != null) switches.Add(candidate);
            }
            Assert.IsNotEmpty(switches, "Weder 'PedestrianButton' noch 'Desk_TrafficButton' gefunden.");

            Renderer red = PrivateField<Renderer>(trafficLight, "redLamp");
            Renderer yellow = PrivateField<Renderer>(trafficLight, "yellowLamp");
            Renderer green = PrivateField<Renderer>(trafficLight, "greenLamp");
            Assert.IsNotNull(red, "Die Ampel hat keine rote Lampe.");
            Assert.IsNotNull(yellow, "Die Ampel hat keine gelbe Lampe.");
            Assert.IsNotNull(green, "Die Ampel hat keine grüne Lampe.");

            int phaseCount = trafficLight.Phases.Count;
            Assert.GreaterOrEqual(phaseCount, 2, "Die Ampel hat zu wenige Phasen.");

            foreach (ClickableSwitch trafficSwitch in switches)
            {
                Material firstRed = red.sharedMaterial;
                bool redChanged = false;
                for (int press = 0; press < phaseCount; press++)
                {
                    int before = trafficLight.CurrentPhaseIndex;
                    trafficSwitch.Press();
                    yield return null;

                    int expected = (before + 1) % phaseCount;
                    Assert.AreEqual(expected, trafficLight.CurrentPhaseIndex,
                        trafficSwitch.name + ": Die Ampel ist nicht genau eine Phase weitergeschaltet.");

                    TrafficLightPhase phase = trafficLight.CurrentPhase;
                    AssertLamp(trafficLight, red, phase.red, "redOn", "redOff", "rote");
                    AssertLamp(trafficLight, yellow, phase.yellow, "yellowOn", "yellowOff", "gelbe");
                    AssertLamp(trafficLight, green, phase.green, "greenOn", "greenOff", "grüne");
                    if (red.sharedMaterial != firstRed) redChanged = true;
                }
                Assert.IsTrue(redChanged, trafficSwitch.name + ": Die rote Lampe hat sich während eines ganzen Umlaufs nie verändert.");
            }
        }

        // ------------------------------------------------------------------ K: Rotator

        [UnityTest]
        public IEnumerator RotatorSpeedControlsRotation()
        {
            Rotator rotator = FindComponent<Rotator>("Crystal");
            Assert.IsNotNull(rotator, "Kein Rotator auf dem Kristall 'Crystal'.");
            rotator.IsRotating = true;
            rotator.Speed = 45f;
            yield return null;

            var rate = new float[1];
            yield return MeasureRotationRate(rotator.transform, 1f, rate);
            float slow = rate[0];

            rotator.Speed = 90f;
            yield return null;
            yield return MeasureRotationRate(rotator.transform, 1f, rate);
            float fast = rate[0];

            Assert.That(slow, Is.InRange(38f, 52f), "Bei Speed 45 dreht sich der Kristall " + slow.ToString("0.0") + "°/s.");
            float ratio = fast / Mathf.Max(0.001f, slow);
            Assert.That(ratio, Is.InRange(1.7f, 2.3f),
                "Doppelte Speed sollte doppelt so schnell drehen (gemessen: " + slow.ToString("0.0") + "°/s → " + fast.ToString("0.0") + "°/s).");
        }

        // ------------------------------------------------------------------ Plattform

        [UnityTest]
        public IEnumerator MovingPlatformMoves()
        {
            MovingPlatform platform = FindComponent<MovingPlatform>("MovingPlatform");
            if (platform == null) platform = Object.FindAnyObjectByType<MovingPlatform>();
            Assert.IsNotNull(platform, "Keine fahrende Plattform (MovingPlatform) in der Szene.");
            platform.IsMoving = true;

            var body = platform.GetComponent<Rigidbody>();
            Func<Vector3> position = () => body != null ? body.position : platform.transform.position;
            Vector3 start = position();

            yield return WaitUntilOrTimeout(() => Vector3.Distance(position(), start) > 0.5f, 2f);

            float moved = Vector3.Distance(position(), start);
            Assert.Greater(moved, 0.5f, "Die Plattform ist in 2 s nur " + moved.ToString("0.00") + " m gefahren.");
        }

        // ------------------------------------------------------------------ Schalter anklickbar

        [UnityTest]
        public IEnumerator SwitchesAreClickableByRaycast()
        {
            yield return new WaitForFixedUpdate();
            Camera camera = Camera.main;
            Assert.IsNotNull(camera, "Keine Kamera mit dem Tag 'MainCamera'.");

            ClickableSwitch[] switches = Object.FindObjectsByType<ClickableSwitch>(FindObjectsSortMode.None);
            Assert.GreaterOrEqual(switches.Length, 5, "Auffällig wenige Schalter in der Szene (" + switches.Length + ").");

            Physics.SyncTransforms();
            var notClickable = new List<string>();
            foreach (ClickableSwitch clickable in switches)
            {
                if (!CanBeClicked(clickable, camera.transform.position)) notClickable.Add(clickable.name);
            }
            Assert.IsEmpty(notClickable, "Diese Schalter kann man nicht anklicken (kein passender Collider): " + string.Join(", ", notClickable));
        }

        // ================================================================== Hilfen

        static GoalZone MainGoal() => FindComponent<GoalZone>("GoalZone");

        /// <summary>Die rote Kugel: am sichersten über das Haupt-Ziel (Required Ball), sonst über den Namen.</summary>
        static PhysicsBall MainBall()
        {
            GoalZone goal = MainGoal();
            if (goal != null && goal.RequiredBall != null) return goal.RequiredBall;
            return FindComponent<PhysicsBall>("PhysicsBall");
        }

        /// <summary>Sucht ein Objekt nach Namen – erst aktiv (GameObject.Find), dann auch inaktive.</summary>
        static GameObject FindObject(string objectName)
        {
            GameObject found = GameObject.Find(objectName);
            if (found != null) return found;
            foreach (Transform t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (t.name == objectName) return t.gameObject;
            }
            return null;
        }

        /// <summary>Sucht zuerst nach Komponenten-Typ (auf einem Objekt mit passendem Namen), dann über den Namen.</summary>
        static T FindComponent<T>(string objectName) where T : Component
        {
            foreach (T component in Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (component.gameObject.name == objectName) return component;
            }
            GameObject go = FindObject(objectName);
            if (go == null) return null;
            T fallback = go.GetComponent<T>();
            return fallback != null ? fallback : null;
        }

        static IEnumerator WaitUntilOrTimeout(Func<bool> condition, float timeoutSeconds)
        {
            float end = Time.time + timeoutSeconds;
            while (!condition() && Time.time < end) yield return null;
        }

        /// <summary>Misst, wie viele Grad pro Sekunde sich ein Objekt (lokal) dreht – Frame für Frame aufsummiert.</summary>
        static IEnumerator MeasureRotationRate(Transform target, float seconds, float[] result)
        {
            float startTime = Time.time;
            Quaternion last = target.localRotation;
            float total = 0f;
            while (Time.time - startTime < seconds)
            {
                yield return null;
                Quaternion current = target.localRotation;
                total += PreciseAngle(last, current);
                last = current;
            }
            float elapsed = Time.time - startTime;
            result[0] = elapsed > 0f ? total / elapsed : 0f;
        }

        /// <summary>Winkel zwischen zwei Drehungen – auch bei sehr kleinen Winkeln genau (Quaternion.Angle rundet die auf 0).</summary>
        static float PreciseAngle(Quaternion a, Quaternion b)
        {
            Quaternion delta = Quaternion.Inverse(a) * b;
            float vectorLength = Mathf.Sqrt(delta.x * delta.x + delta.y * delta.y + delta.z * delta.z);
            return 2f * Mathf.Atan2(vectorLength, Mathf.Abs(delta.w)) * Mathf.Rad2Deg;
        }

        static T PrivateField<T>(object target, string fieldName) where T : class
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            return field != null ? field.GetValue(target) as T : null;
        }

        static void AssertLamp(TrafficLightController trafficLight, Renderer lamp, bool shouldBeOn, string onField, string offField, string colourName)
        {
            Material expected = PrivateField<Material>(trafficLight, shouldBeOn ? onField : offField);
            if (expected == null) return; // kein Material eingetragen – das meldet der Validator
            Assert.AreSame(expected, lamp.sharedMaterial,
                "Phase '" + trafficLight.CurrentPhaseName + "': Die " + colourName + " Lampe sollte " + (shouldBeOn ? "leuchten" : "aus sein") + ".");
        }

        /// <summary>
        /// Prüft mit Strahlen (Raycasts) wie der Click Interactor, ob man den Schalter treffen kann:
        /// von der Kamera aus, von oben und auf jeden eigenen Collider.
        /// </summary>
        static bool CanBeClicked(ClickableSwitch clickable, Vector3 cameraPosition)
        {
            Vector3 target = clickable.ButtonCap != null ? clickable.ButtonCap.position : clickable.transform.position + Vector3.up * 0.9f;
            if (RayHitsSwitch(cameraPosition, target, clickable)) return true;
            if (RayHitsSwitch(target + Vector3.up * 3f, target, clickable)) return true;

            foreach (Collider collider in clickable.GetComponentsInChildren<Collider>())
            {
                if (collider.isTrigger || !collider.enabled) continue;
                Vector3 center = collider.bounds.center;
                if (RayHitsSwitch(cameraPosition, center, clickable)) return true;
                if (RayHitsSwitch(center + Vector3.up * 3f, center, clickable)) return true;
            }
            return false;
        }

        static bool RayHitsSwitch(Vector3 from, Vector3 to, ClickableSwitch clickable)
        {
            Vector3 direction = to - from;
            float distance = direction.magnitude;
            if (distance < 0.001f) return false;
            RaycastHit[] hits = Physics.RaycastAll(from, direction / distance, distance + 1f, ~0, QueryTriggerInteraction.Ignore);
            return hits.Any(hit => hit.collider.GetComponentInParent<IClickable>() is Component owner && owner == clickable);
        }
    }
}
