using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace StudentSandbox.EditorTools
{
    /// <summary>
    /// Baut das Physik-Labor (04_PHYSICS_ZONE) mit drei Experimenten:
    ///  - Murmelbahn: Die rote Kugel rollt vom Startturm über eine Rampe, stößt drei Kegel um und rollt ins Ziel.
    ///  - Domino: Ein kinematischer Hammer stößt eine Reihe aus zehn Dominosteinen an (Kettenreaktion).
    ///  - Hindernis-Parcours: Rätsel – die blaue Kugel soll mit Mauern und Bumper ins Ziel gelenkt werden.
    ///
    /// Alle Positionen sind LOKAL zur Zonen-Wurzel (Welt (0, 0, 25), nicht gedreht). Bodenoberkante y = 0.1.
    /// Zuverlässigkeit geht vor: Die Ausgangs-Konfiguration muss jedes Mal funktionieren. Deshalb werden alle
    /// abgeleiteten Maße (Rampen-Enden, Kugel-Start, Stützenhöhe …) hier aus wenigen Konstanten berechnet.
    /// </summary>
    public static class PhysicsZoneBuilder
    {
        // ---------------------------------------------------------------- Maße: Bereich

        const float FloorTop = SandboxContext.ZoneFloorTop;     // 0.1
        const float ZoneHalf = SandboxContext.ZoneHalfSize;     // 12
        const float WallHeight = 0.5f;
        const float WallThickness = 0.3f;
        const float WallCapHeight = 0.06f;
        const float WallCapOverhang = 0.03f;
        const float ArchZ = -11.3f;
        /// <summary>Außenkante der Pfeiler-Sockel des Eingangsbogens (SB.EntranceArch: halfWidth 2.9 + halber Sockel 0.45).</summary>
        const float ArchPillarOuterX = 2.9f + 0.45f;

        // ---------------------------------------------------------------- Maße: Murmelbahn (B1)

        const float TrackZ = 7.0f;
        const float TrackTop = 0.12f;
        const float RampLength = 8.0f;
        const float RampAngle = 22f;
        const float RampBottomX = -1.0f;
        const float RampThickness = 0.2f;
        const float TrackWidth = 2.4f;
        const float RailHeight = 0.5f;
        const float RailThickness = 0.15f;
        const float TrackEndX = 7.0f;
        const float GoalX = 8.2f;
        const float GoalEndX = 9.4f;
        const float SupportOffset = 3.7f;
        const float BallRadius = 0.4f;
        const float BallStartDistance = 0.9f;
        const float BallStartGap = 0.05f;
        const float PlatformWidth = 2.2f;
        const float PlatformDepth = 2.6f;
        const float PlatformThickness = 0.3f;

        // ---------------------------------------------------------------- Maße: Domino (B2)

        const float DominoRowZ = -3.0f;
        const float DominoFirstX = 3.0f;
        const float DominoSpacing = 0.65f;
        const int DominoCount = 10;
        const float HammerX = 2.6f;
        const float HammerAxleY = 2.6f;
        const float HammerRestAngle = -75f;
        const float HammerFramePostTop = 2.75f;
        const float HammerFrameHalfWidth = 0.9f;
        const float CatchBoardX = 9.8f;

        // ---------------------------------------------------------------- Farben für Text

        static readonly Color TextDark = SandboxMaterials.Hex("#2E3440");
        static readonly Color TextLight = SandboxMaterials.Hex("#FFFFFF");
        static readonly Color ChalkWhite = SandboxMaterials.Hex("#F4F1E8");

        static readonly string[] DominoColors = { M.CoralRed, M.SoftYellow, M.MintGreen, M.SkyBlue, M.Lavender, M.WarmOrange };

        // ================================================================ Einstieg

        public static void Build(SandboxContext ctx)
        {
            Transform zone = ctx != null ? ctx.PhysicsZone : null;
            if (zone == null)
            {
                Debug.LogError("[Student Sandbox] PhysicsZoneBuilder: ctx.PhysicsZone fehlt (wurde CreateRootGroups() aufgerufen?).");
                return;
            }

            Transform staticEnvironment = SB.Group("StaticEnvironment", zone);
            Transform marbleRun = SB.Group("MarbleRun", zone);
            Transform dominoExperiment = SB.Group("DominoExperiment", zone);
            Transform obstacleCourse = SB.Group("ObstacleCourse", zone);
            Transform controls = SB.Group("Controls", zone);
            Transform signs = SB.Group("Signs", zone);
            Transform decoration = SB.Group("Decoration", zone);

            BuildStaticEnvironment(staticEnvironment);
            BuildMarbleRun(marbleRun, signs);
            DominoStarter starter = BuildDominoExperiment(dominoExperiment);
            BuildObstacleCourse(obstacleCourse);
            BuildControls(ctx, controls, starter);
            BuildSigns(signs);
            BuildDecoration(decoration);

            SB.Note(zone.gameObject, "Physik-Labor",
                "Hier warten drei Experimente auf dich: die Murmelbahn mit der roten Kugel, die Domino-Kettenreaktion " +
                "und der Hindernis-Parcours mit der blauen Kugel. Drück Play und schau zu, was passiert! " +
                "Mit der Taste R oder dem RESET-Knopf kommt alles zurück an den Start.");
        }

        // ================================================================ Boden, Rand, Eingang

        static void BuildStaticEnvironment(Transform parent)
        {
            GameObject floor = SB.Box("Floor", parent, new Vector3(0f, FloorTop * 0.5f, 0f), new Vector3(ZoneHalf * 2f, FloorTop, ZoneHalf * 2f), M.LabFloor);
            SB.SetPhysicsMaterial(floor, PM.Normal);

            // Dezente Fliesenfugen alle 4 m (nur Optik, knapp über dem Boden, ohne Collider).
            Transform lines = SB.Group("Floor_Lines", parent);
            const float lineHeight = 0.003f;
            const float lineWidth = 0.05f;
            float lineY = FloorTop + 0.005f - lineHeight * 0.5f;   // Oberkante 0.105
            float inner = ZoneHalf - WallThickness;                 // 11.7
            int index = 1;
            for (int i = -2; i <= 2; i++)
            {
                float p = i * 4f;
                SB.Box("Floor_Line_X_" + index, lines, new Vector3(p, lineY, 0f), new Vector3(lineWidth, lineHeight, inner * 2f), M.PlazaAccent, default, false);
                SB.Box("Floor_Line_Z_" + index, lines, new Vector3(0f, lineY, p), new Vector3(inner * 2f, lineHeight, lineWidth), M.PlazaAccent, default, false);
                index++;
            }
            SB.NoShadows(lines.gameObject);

            // Niedrige Randmauer (hält die Kugeln im Labor). Ecken ohne Überlappung, damit nichts flimmert.
            // Im Süden endet die Mauer an den Sockeln des Eingangsbogens – die Öffnung ist so breit wie der Bogen.
            Transform border = SB.Group("Border", parent);
            float edge = ZoneHalf - WallThickness;   // Innenkante 11.7
            Wall("Border_North", border, -ZoneHalf, ZoneHalf, edge, ZoneHalf);
            Wall("Border_East", border, edge, ZoneHalf, -edge, edge);
            Wall("Border_West", border, -ZoneHalf, -edge, -edge, edge);
            Wall("Border_South_L", border, -ZoneHalf, -ArchPillarOuterX, -ZoneHalf, -edge);
            Wall("Border_South_R", border, ArchPillarOuterX, ZoneHalf, -ZoneHalf, -edge);

            float o = WallCapOverhang;
            WallCap("Border_North_Cap", border, -ZoneHalf - o, ZoneHalf + o, edge - o, ZoneHalf + o);
            WallCap("Border_East_Cap", border, edge - o, ZoneHalf + o, -edge + o, edge - o);
            WallCap("Border_West_Cap", border, -ZoneHalf - o, -edge + o, -edge + o, edge - o);
            WallCap("Border_South_L_Cap", border, -ZoneHalf - o, -ArchPillarOuterX, -ZoneHalf - o, -edge + o);
            WallCap("Border_South_R_Cap", border, ArchPillarOuterX, ZoneHalf + o, -ZoneHalf - o, -edge + o);
            SB.SetPhysicsMaterial(border.gameObject, PM.Normal);

            SB.EntranceArch("EntranceArch", parent, new Vector3(0f, FloorTop, ArchZ), 0f, "PHYSICS", "Physik-Labor", M.WarmOrange);

            SB.MarkStatic(parent.gameObject);
        }

        static GameObject Wall(string name, Transform parent, float xMin, float xMax, float zMin, float zMax)
        {
            return SB.Box(name, parent,
                new Vector3((xMin + xMax) * 0.5f, FloorTop + WallHeight * 0.5f, (zMin + zMax) * 0.5f),
                new Vector3(xMax - xMin, WallHeight, zMax - zMin), M.WarmOrange);
        }

        /// <summary>Heller Abschluss oben auf der Mauer (leicht breiter, liegt 1 cm tiefer als die Mauerkante → kein Flimmern).</summary>
        static void WallCap(string name, Transform parent, float xMin, float xMax, float zMin, float zMax)
        {
            float y = FloorTop + WallHeight - 0.01f + WallCapHeight * 0.5f;
            SB.Box(name, parent, new Vector3((xMin + xMax) * 0.5f, y, (zMin + zMax) * 0.5f),
                new Vector3(xMax - xMin, WallCapHeight, zMax - zMin), M.WarmWhite, default, false);
        }

        // ================================================================ B1 Murmelbahn

        static void BuildMarbleRun(Transform parent, Transform signs)
        {
            float rad = RampAngle * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            float tan = Mathf.Tan(rad);
            float rampTopX = RampBottomX - RampLength * cos;    // ≈ -8.417
            float rampTopY = TrackTop + RampLength * sin;       // ≈ 3.117
            float railZ = TrackWidth * 0.5f + RailThickness * 0.5f;

            // ---------- Rampe: Drehpunkt oben am Turm, fällt nach +x ab.
            Transform ramp = SB.Group("Ramp_Main", parent, new Vector3(rampTopX, rampTopY, TrackZ));
            ramp.localRotation = Quaternion.Euler(0f, 0f, -RampAngle);
            SB.Box("Ramp_Surface", ramp, new Vector3(RampLength * 0.5f, -RampThickness * 0.5f, 0f),
                new Vector3(RampLength, RampThickness, TrackWidth), M.LightWood);
            SB.Box("Ramp_Rail_Left", ramp, new Vector3(RampLength * 0.5f, RailHeight * 0.5f, railZ),
                new Vector3(RampLength, RailHeight, RailThickness), M.WarmOrange);
            SB.Box("Ramp_Rail_Right", ramp, new Vector3(RampLength * 0.5f, RailHeight * 0.5f, -railZ),
                new Vector3(RampLength, RailHeight, RailThickness), M.WarmOrange);
            // Sicherheitswand oben (Oberkante 0.6 über der Fahrbahn). Sie reicht bis in die Turm-Plattform hinein,
            // damit zwischen Wand und Plattform kein Spalt zu sehen ist.
            SB.Box("Ramp_Backstop", ramp, new Vector3(-0.075f, 0.2f, 0f),
                new Vector3(0.15f, 0.8f, TrackWidth + 2f * RailThickness), M.WarmOrange);
            // Querstreifen auf der Fahrbahn (nur Optik, 4 mm über der Oberfläche) – zeigen, wie schnell die Kugel wird.
            Transform stripes = SB.Group("Ramp_Stripes", ramp);
            int stripeCount = Mathf.FloorToInt(RampLength) - 1;
            for (int i = 1; i <= stripeCount; i++)
            {
                SB.Box("Ramp_Stripe_" + i, stripes, new Vector3(i, 0.004f, 0f), new Vector3(0.12f, 0.004f, TrackWidth - 0.2f), M.WarmWhite, default, false);
            }
            SB.NoShadows(stripes.gameObject);

            SB.Note(ramp.gameObject, "Die Rampe",
                "Dreh die Rampe an Rotation Z: -30 = steiler (die Kugel wird schneller), -12 = flacher. " +
                "Der Drehpunkt liegt oben am Turm, deshalb bleibt die Rampe am Turm hängen. Danach Play drücken!");

            SB.Note(ramp.Find("Ramp_Surface").gameObject, "Fahrbahn der Rampe",
                "Ich bin nur die Fahrbahn. Zum Drehen wähl in der Hierarchy das Eltern-Objekt 'Ramp_Main' (eine Ebene höher) – " +
                "dann dreht sich die ganze Rampe um ihren Drehpunkt oben am Turm.");

            // ---------- Stütze unter der Rampe.
            // Sie hängt als Kind an Ramp_Main, steht aber bei -22° senkrecht. Würde sie fest stehen bleiben, ragte sie bei
            // Rotation Z = -30 einen halben Meter durch die Fahrbahn und hielte die Kugel auf. Die Pfosten reichen deshalb
            // tief unter den Boden: Bei steiler oder flacher gestellter Rampe (etwa -40° … 0°) sieht man nie ein freies Ende.
            float supportX = rampTopX + SupportOffset;
            float undersideAtSupport = rampTopY - SupportOffset * tan - RampThickness / cos;   // ≈ 1.406
            const float beamWidth = 0.3f;
            const float beamHeight = 0.16f;
            const float postBottomY = FloorTop - 2.1f;
            // Oberkante = Unterseite der Rampe an der höheren (bergseitigen) Kante des Balkens → keine sichtbare Lücke.
            float beamTop = undersideAtSupport + beamWidth * 0.5f * tan;
            Transform support = SB.Group("Ramp_Support", ramp);
            Quaternion inverseRamp = Quaternion.Inverse(ramp.localRotation);
            support.localPosition = inverseRamp * (new Vector3(supportX, 0f, TrackZ) - ramp.localPosition);
            support.localRotation = inverseRamp;   // in der Ausgangslage achsenparallel zur Welt
            SB.Box("Support_Beam", support, new Vector3(0f, beamTop - beamHeight * 0.5f, 0f), new Vector3(beamWidth, beamHeight, TrackWidth), M.Charcoal);
            float postTop = beamTop - beamHeight + 0.02f;
            float postHeight = postTop - postBottomY;
            foreach (float side in new[] { -1f, 1f })
            {
                string sideName = side > 0f ? "Left" : "Right";
                SB.Box("Support_Post_" + sideName, support, new Vector3(0f, postBottomY + postHeight * 0.5f, side * 0.85f),
                    new Vector3(0.25f, postHeight, 0.25f), M.Charcoal);
            }
            SB.SetPhysicsMaterial(ramp.gameObject, PM.Normal);

            // ---------- Startturm
            BuildStartTower(parent, rampTopX, rampTopY);

            // ---------- Die rote Kugel: 0.9 m unterhalb der Rampen-Oberkante, 5 cm über der Fahrbahn.
            Vector3 surfacePoint = new Vector3(rampTopX + BallStartDistance * cos, rampTopY - BallStartDistance * sin, TrackZ);
            Vector3 surfaceNormal = new Vector3(sin, cos, 0f);
            Vector3 ballPosition = surfacePoint + surfaceNormal * (BallRadius + BallStartGap);
            GameObject ball = Place(P.PhysicsBall, parent, ballPosition, 0f, 1f, "PhysicsBall");
            PhysicsBall redBall = ball.GetComponent<PhysicsBall>();
            if (redBall != null)
            {
                redBall.DisplayName = "Rote Kugel";
                Record(redBall);
            }
            else
            {
                Debug.LogWarning("[Student Sandbox] PhysicsBall-Prefab hat keine PhysicsBall-Komponente.", ball);
            }
            Note(ball, "Die rote Kugel",
                "Ich bin die Kugel! Ich habe einen Rigidbody (Physik) und einen Sphere Collider (meine Form für Zusammenstöße). " +
                "Probier: Scale größer, Mass im Rigidbody ändern, Startposition verschieben – dann Play.");

            // ---------- Flache Bahn bis zum Ziel (Oberkante 0.12 = Ende der Rampe).
            Transform track = SB.Group("Track", parent);
            float trackStart = RampBottomX - 0.1f;
            float trackLength = TrackEndX - trackStart;
            SB.Box("Track_Floor", track, new Vector3(trackStart + trackLength * 0.5f, TrackTop - 0.01f, TrackZ),
                new Vector3(trackLength, 0.02f, TrackWidth), M.LightWood);
            // Die Bande steht auf dem Laborboden und endet 0.5 m über der Bahn.
            float railLength = GoalEndX - RampBottomX;
            float railTotalHeight = TrackTop + RailHeight - FloorTop;
            foreach (float side in new[] { 1f, -1f })
            {
                string sideName = side > 0f ? "Left" : "Right";
                SB.Box("Track_Rail_" + sideName, track,
                    new Vector3(RampBottomX + railLength * 0.5f, FloorTop + railTotalHeight * 0.5f, TrackZ + side * railZ),
                    new Vector3(railLength, railTotalHeight, RailThickness), M.WarmOrange);
            }
            SB.SetPhysicsMaterial(track.gameObject, PM.Normal);

            // ---------- Kegel (Kollisionen zeigen)
            // Sie stehen IM Zielbereich: Die Kugel löst das Ziel immer zuerst aus und räumt dann die Kegel ab.
            // (Auf der Bahn davor könnte ein umgefallener Kegel die Kugel zurückwerfen.)
            Transform pins = SB.Group("Pins", parent);
            Vector3[] pinPositions =
            {
                new Vector3(8.2f, TrackTop, TrackZ),
                new Vector3(8.65f, TrackTop, TrackZ - 0.35f),
                new Vector3(8.65f, TrackTop, TrackZ + 0.35f),
            };
            for (int i = 0; i < pinPositions.Length; i++)
            {
                GameObject pin = BuildPin("Pin_" + (i + 1), pins, pinPositions[i]);
                if (i == 0)
                {
                    SB.Note(pin, "Die Kegel",
                        "Drei Kegel mit Rigidbody im Ziel: Die Kugel stößt sie um – das ist eine Kollision. " +
                        "Probier: Stell bei einem Kegel die Mass im Rigidbody auf 5. Fällt er dann immer noch um?");
                }
            }

            // ---------- Ziel
            BuildGoal(parent, redBall);

            // Prallwand hinter dem Ziel (steht auf dem Laborboden, Oberkante 0.9 über der Bahn).
            float backstopTop = TrackTop + 0.9f;
            GameObject backstop = SB.Box("Goal_Backstop", parent,
                new Vector3(GoalEndX + 0.15f, (FloorTop + backstopTop) * 0.5f, TrackZ),
                new Vector3(0.3f, backstopTop - FloorTop, TrackWidth + 2f * RailThickness), M.CoralRed);
            SB.SetPhysicsMaterial(backstop, PM.Normal);

            // Kleines Schild "ZIEL", zur Kamera (Süden) gedreht.
            MakeSign("Sign_Goal", signs, new Vector3(GoalX, FloorTop, 4.7f), "ZIEL", null, 1.6f, 1.1f);
        }

        static void BuildStartTower(Transform parent, float rampTopX, float rampTopY)
        {
            Transform tower = SB.Group("StartTower", parent);
            float platformX = rampTopX - PlatformWidth * 0.5f;
            float backX = rampTopX - PlatformWidth;   // Rückseite (-x)

            GameObject platform = SB.Box("Tower_Platform", tower, new Vector3(platformX, rampTopY - PlatformThickness * 0.5f, TrackZ),
                new Vector3(PlatformWidth, PlatformThickness, PlatformDepth), M.WarmOrange);
            SB.SetPhysicsMaterial(platform, PM.Normal);

            // Vier Beine an den Ecken (stehen auf dem Boden, stecken oben in der Plattform).
            const float leg = 0.25f;
            float legHeight = rampTopY - 0.25f;
            float legY = FloorTop + legHeight * 0.5f;
            float legOffsetX = PlatformWidth * 0.5f - leg * 0.5f;
            float legOffsetZ = PlatformDepth * 0.5f - leg * 0.5f;
            int legIndex = 1;
            foreach (float sx in new[] { -1f, 1f })
            {
                foreach (float sz in new[] { -1f, 1f })
                {
                    SB.Box("Tower_Leg_" + legIndex++, tower, new Vector3(platformX + sx * legOffsetX, legY, TrackZ + sz * legOffsetZ),
                        new Vector3(leg, legHeight, leg), M.Charcoal);
                }
            }

            // Querstreben zwischen den Beinen (zwei Höhen, alle vier Seiten).
            const float brace = 0.1f;
            float braceLengthX = PlatformWidth - 2f * leg + 0.04f;
            float braceLengthZ = PlatformDepth - 2f * leg + 0.04f;
            int braceIndex = 1;
            foreach (float heightFactor in new[] { 0.33f, 0.66f })
            {
                float y = FloorTop + (rampTopY - FloorTop) * heightFactor;
                foreach (float sz in new[] { -1f, 1f })
                {
                    SB.Box("Tower_Brace_" + braceIndex++, tower, new Vector3(platformX, y, TrackZ + sz * legOffsetZ),
                        new Vector3(braceLengthX, brace, brace), M.Charcoal, default, false);
                }
                foreach (float sx in new[] { -1f, 1f })
                {
                    SB.Box("Tower_Brace_" + braceIndex++, tower, new Vector3(platformX + sx * legOffsetX, y, TrackZ),
                        new Vector3(brace, brace, braceLengthZ), M.Charcoal, default, false);
                }
            }

            // Leiter auf der Rückseite (-x): zwei Holme + sechs Sprossen, ohne Collider.
            float ladderX = backX - 0.1f;
            float ladderTop = rampTopY + 0.9f;
            float ladderLength = ladderTop - FloorTop;
            const float ladderHalfWidth = 0.3f;
            foreach (float side in new[] { -1f, 1f })
            {
                SB.Box(side < 0f ? "Ladder_Rail_Right" : "Ladder_Rail_Left", tower,
                    new Vector3(ladderX, FloorTop + ladderLength * 0.5f, TrackZ + side * ladderHalfWidth),
                    new Vector3(0.08f, ladderLength, 0.08f), M.LightWood, default, false);
            }
            const int rungs = 6;
            for (int i = 1; i <= rungs; i++)
            {
                float y = FloorTop + i * (rampTopY - FloorTop) / (rungs + 1);
                SB.Box("Ladder_Rung_" + i, tower, new Vector3(ladderX, y, TrackZ), new Vector3(0.06f, 0.06f, ladderHalfWidth * 2f), M.LightWood, default, false);
            }

            // Geländer hinten und an den Seiten – NICHT auf der Rampenseite. Hinten mit Lücke für die Leiter.
            Transform railing = SB.Group("Tower_Railing", tower);
            float railBackX = backX + 0.05f;
            float railFrontX = rampTopX - 0.3f;
            float railNorthZ = TrackZ + PlatformDepth * 0.5f - 0.05f;
            float railSouthZ = TrackZ - PlatformDepth * 0.5f + 0.05f;
            float gapHalf = ladderHalfWidth + 0.1f;
            Railing("Railing_North", railing, new Vector3(railBackX, rampTopY, railNorthZ), new Vector3(railFrontX, rampTopY, railNorthZ), true, true);
            Railing("Railing_South", railing, new Vector3(railBackX, rampTopY, railSouthZ), new Vector3(railFrontX, rampTopY, railSouthZ), true, true);
            Railing("Railing_Back_South", railing, new Vector3(railBackX, rampTopY, railSouthZ), new Vector3(railBackX, rampTopY, TrackZ - gapHalf), false, true);
            Railing("Railing_Back_North", railing, new Vector3(railBackX, rampTopY, TrackZ + gapHalf), new Vector3(railBackX, rampTopY, railNorthZ), true, false);

            // Fahne an der hinteren Ecke.
            Vector3 flagBase = new Vector3(backX + 0.25f, rampTopY, railNorthZ - 0.2f);
            const float poleHeight = 1.8f;
            SB.Prim(PrimitiveType.Cylinder, "Flag_Pole", tower, flagBase + new Vector3(0f, poleHeight * 0.5f, 0f),
                new Vector3(0.05f, poleHeight * 0.5f, 0.05f), M.WarmWhite, default, false);
            SB.MeshObj(MK.Sphere, "Flag_Pole_Top", tower, flagBase + new Vector3(0f, poleHeight + 0.04f, 0f), new Vector3(0.1f, 0.1f, 0.1f), M.SoftYellow);
            SB.Box("Flag", tower, flagBase + new Vector3(0.38f, poleHeight - 0.24f, 0f), new Vector3(0.7f, 0.42f, 0.03f), M.CoralRed, default, false);

            // "START" auf der Südseite der Plattform (zur Kamera).
            SB.Text("Start_Label", "START", tower, new Vector3(platformX, rampTopY - PlatformThickness * 0.5f, TrackZ - PlatformDepth * 0.5f - 0.012f),
                Vector3.zero, 0.22f, TextLight);

            SB.Note(tower.gameObject, "Der Startturm",
                "Oben auf dem Turm beginnt die Rampe. Je höher die Kugel startet, desto mehr Schwung bekommt sie beim Hinunterrollen. " +
                "Die Leiter und das Geländer sind nur Dekoration.");
        }

        /// <summary>Ein Geländer-Stück von a nach b (a, b liegen auf der Standfläche). Ohne Collider.</summary>
        static void Railing(string name, Transform parent, Vector3 a, Vector3 b, bool postAtA, bool postAtB)
        {
            const float height = 0.9f;
            const float post = 0.07f;
            const float rail = 0.06f;
            Vector3 direction = b - a;
            float length = direction.magnitude;
            if (length < 0.01f) return;
            // Yaw so wählen, dass die lokale x-Achse von a nach b zeigt (lokal x → (cos θ, 0, -sin θ)).
            float yaw = Mathf.Atan2(-direction.z, direction.x) * Mathf.Rad2Deg;
            Transform root = SB.Group(name, parent, (a + b) * 0.5f, yaw);
            SB.Box("Rail_Top", root, new Vector3(0f, height, 0f), new Vector3(length + post, rail, rail), M.WarmWhite, default, false);
            SB.Box("Rail_Middle", root, new Vector3(0f, height * 0.5f, 0f), new Vector3(length, rail * 0.8f, rail * 0.8f), M.WarmWhite, default, false);
            if (postAtA) SB.Box("Post_A", root, new Vector3(-length * 0.5f, height * 0.5f, 0f), new Vector3(post, height, post), M.WarmWhite, default, false);
            if (postAtB) SB.Box("Post_B", root, new Vector3(length * 0.5f, height * 0.5f, 0f), new Vector3(post, height, post), M.WarmWhite, default, false);
            if (length > 1.5f) SB.Box("Post_Middle", root, new Vector3(0f, height * 0.5f, 0f), new Vector3(post, height, post), M.WarmWhite, default, false);
        }

        /// <summary>
        /// Ein Kegel: Wurzel mit Rigidbody + ResettableObject, sichtbarer Zylinder mit konvexem Mesh Collider
        /// (ein Capsule Collider wäre unten rund und würde umkippen).
        /// </summary>
        static GameObject BuildPin(string name, Transform parent, Vector3 position)
        {
            Transform root = SB.Group(name, parent, position);
            GameObject body = SB.Prim(PrimitiveType.Cylinder, "Pin_Body", root, new Vector3(0f, 0.35f, 0f), new Vector3(0.24f, 0.35f, 0.24f), M.WarmWhite, default, false);
            MeshCollider meshCollider = body.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = body.GetComponent<MeshFilter>().sharedMesh;
            meshCollider.convex = true;
            SB.Prim(PrimitiveType.Cylinder, "Pin_Band", root, new Vector3(0f, 0.52f, 0f), new Vector3(0.25f, 0.03f, 0.25f), M.CoralRed, default, false);
            SB.MeshObj(MK.Sphere, "Pin_Top", root, new Vector3(0f, 0.7f, 0f), new Vector3(0.235f, 0.2f, 0.235f), M.WarmWhite);

            Rigidbody body3D = root.gameObject.AddComponent<Rigidbody>();
            body3D.mass = 0.2f;
            body3D.interpolation = RigidbodyInterpolation.Interpolate;
            body3D.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            root.gameObject.AddComponent<ResettableObject>();
            return root.gameObject;
        }

        static void BuildGoal(Transform parent, PhysicsBall requiredBall)
        {
            var goalObject = new GameObject("GoalZone");
            Transform goalRoot = goalObject.transform;
            goalRoot.SetParent(parent, false);
            goalRoot.localPosition = new Vector3(GoalX, TrackTop, TrackZ);

            BoxCollider trigger = goalObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(2.2f, 1.2f, 2.3f);
            trigger.center = new Vector3(0f, 0.6f, 0f);

            // Leuchtende Bodenplatte (bündig mit der Bahn, Oberkante 0.12).
            GameObject pad = SB.Box("Goal_Pad", goalRoot, new Vector3((TrackEndX + GoalEndX) * 0.5f - GoalX, -0.01f, 0f),
                new Vector3(GoalEndX - TrackEndX, 0.02f, TrackWidth), M.GoalIdle);
            SB.SetPhysicsMaterial(pad, PM.Normal);

            Light successLight = SB.PointLight("SuccessLight", goalRoot, new Vector3(0f, 2.0f, 0f), SB.Hex("#4CFF8A"), 4f, 6f);
            successLight.enabled = false;

            GoalZone goal = goalObject.AddComponent<GoalZone>();
            goal.Setup(pad.GetComponent<Renderer>(), SB.Mat(M.GoalIdle), SB.Mat(M.GoalReached), successLight);
            goal.RequiredBall = requiredBall;
            goal.SuccessMessage = "Ziel erreicht! Die rote Kugel ist angekommen.";

            BuildGoalArch(goalRoot);

            SB.Note(goalObject, "Das Ziel (Trigger)",
                "Mein Box Collider hat einen Haken bei 'Is Trigger': Das ist ein unsichtbarer Bereich, durch den die Kugel hindurchrollt, statt abzuprallen. " +
                "Rollt die rote Kugel hinein, leuchtet das Ziel grün. Bei 'Required Ball' steht nur die rote Kugel – " +
                "lass das Feld leer, dann zählt jede Kugel.");
        }

        /// <summary>Zielbogen über der Bahn (nur Dekoration, keine Collider). Kind von GoalZone → wandert beim Verschieben mit.</summary>
        static void BuildGoalArch(Transform goalRoot)
        {
            Transform arch = SB.Group("Goal_Arch", goalRoot);
            float bottom = FloorTop - TrackTop;      // lokal: Laborboden
            const float postHeight = 2.3f;
            const float postZ = 1.55f;
            const float postSize = 0.2f;
            foreach (float side in new[] { -1f, 1f })
            {
                SB.Box(side > 0f ? "Arch_Post_Left" : "Arch_Post_Right", arch, new Vector3(0f, bottom + postHeight * 0.5f, side * postZ),
                    new Vector3(postSize, postHeight, postSize), M.Charcoal, default, false);
            }

            const float bannerHeight = 0.5f;
            const float bannerLength = 3.5f;
            const float bannerDepth = 0.2f;
            float bannerBottom = bottom + postHeight - 0.1f;
            float bannerY = bannerBottom + bannerHeight * 0.5f;
            SB.Box("Arch_Banner", arch, new Vector3(0f, bannerY, 0f), new Vector3(bannerDepth, bannerHeight, bannerLength), M.WarmOrange, default, false);
            // Schrift auf beiden Seiten: zur ankommenden Kugel (-x) und zur Rückseite (+x).
            SB.Text("Arch_Text_Front", "ZIEL", arch, new Vector3(-bannerDepth * 0.5f - 0.012f, bannerY, 0f), new Vector3(0f, 90f, 0f), 0.36f, TextDark);
            SB.Text("Arch_Text_Back", "ZIEL", arch, new Vector3(bannerDepth * 0.5f + 0.012f, bannerY, 0f), new Vector3(0f, -90f, 0f), 0.36f, TextDark);

            // Zielflaggen-Muster: weißer Streifen mit dunklen Karos (Enden weiß, damit keine Flächen zusammenfallen).
            const int columns = 11;
            float square = bannerLength / columns;
            float stripY = bannerBottom + bannerHeight + square * 0.5f;
            SB.Box("Arch_Checker", arch, new Vector3(0f, stripY, 0f), new Vector3(bannerDepth, square, bannerLength), M.WarmWhite, default, false);
            for (int i = 1; i < columns; i += 2)
            {
                float z = -bannerLength * 0.5f + (i + 0.5f) * square;
                SB.Box("Arch_Checker_Dark_" + ((i + 1) / 2), arch, new Vector3(0f, stripY, z),
                    new Vector3(bannerDepth + 0.02f, square + 0.01f, square), M.Charcoal, default, false);
            }
        }

        // ================================================================ B2 Domino

        static DominoStarter BuildDominoExperiment(Transform parent)
        {
            // Dunkle Matte unter der Reihe (nur Optik, 4–10 mm über dem Boden).
            float matStart = HammerX - 0.6f;
            float matEnd = CatchBoardX + 0.3f;
            GameObject mat = SB.Box("Domino_Mat", parent, new Vector3((matStart + matEnd) * 0.5f, FloorTop + 0.007f, DominoRowZ),
                new Vector3(matEnd - matStart, 0.006f, 1.5f), M.Charcoal, default, false);
            mat.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;

            // Gestell des Hammers (keine Collider – es steht außerhalb der Dominoreihe).
            Transform frame = SB.Group("Hammer_Frame", parent);
            float postHeight = HammerFramePostTop - FloorTop;
            foreach (float side in new[] { -1f, 1f })
            {
                string sideName = side > 0f ? "North" : "South";
                float z = DominoRowZ + side * HammerFrameHalfWidth;
                SB.Box("Frame_Post_" + sideName, frame, new Vector3(HammerX, FloorTop + postHeight * 0.5f, z), new Vector3(0.16f, postHeight, 0.16f), M.Charcoal, default, false);
                SB.Box("Frame_Foot_" + sideName, frame, new Vector3(HammerX, FloorTop + 0.03f, z), new Vector3(0.45f, 0.06f, 0.45f), M.StoneGray, default, false);
                SB.MeshObj(MK.Pyramid, "Frame_Cap_" + sideName, frame, new Vector3(HammerX, HammerFramePostTop, z), new Vector3(0.22f, 0.15f, 0.22f), M.WarmOrange);
            }
            SB.Prim(PrimitiveType.Cylinder, "Frame_Axle", frame, new Vector3(HammerX, HammerAxleY, DominoRowZ),
                new Vector3(0.1f, HammerFrameHalfWidth + 0.1f, 0.1f), M.Metal, new Vector3(90f, 0f, 0f), false);

            // Hammer: leerer Drehpunkt mit kinematischem Rigidbody, ausgeholt nach -x.
            Transform hammer = SB.Group("Hammer", parent, new Vector3(HammerX, HammerAxleY, DominoRowZ));
            hammer.localRotation = Quaternion.Euler(0f, 0f, HammerRestAngle);
            SB.Box("Hammer_Arm", hammer, new Vector3(0f, -0.8f, 0f), new Vector3(0.12f, 1.6f, 0.12f), M.DarkWood);
            GameObject hammerHead = SB.Box("Hammer_Head", hammer, new Vector3(0f, -1.6f, 0f), new Vector3(0.45f, 0.45f, 0.6f), M.CoralRed);
            SB.Note(hammerHead, "Hammerkopf",
                "Ich bin nur der Kopf des Hammers. Die Einstellungen (Rigidbody und Domino Starter) findest du beim Eltern-Objekt 'Hammer' – " +
                "klick in der Hierarchy eine Ebene höher.");
            SB.Prim(PrimitiveType.Cylinder, "Hammer_Hub", hammer, Vector3.zero, new Vector3(0.22f, 0.09f, 0.22f), M.Metal, new Vector3(90f, 0f, 0f), false);

            Rigidbody hammerBody = hammer.gameObject.AddComponent<Rigidbody>();
            hammerBody.isKinematic = true;
            hammerBody.useGravity = false;
            hammerBody.interpolation = RigidbodyInterpolation.Interpolate;
            DominoStarter starter = hammer.gameObject.AddComponent<DominoStarter>();
            starter.StartAutomatically = true;
            starter.StartDelay = 1.5f;
            SB.Note(hammer.gameObject, "Der Hammer",
                "Der Hammer ist ein kinematischer Rigidbody: Ein Skript bewegt ihn, aber er schubst trotzdem andere Physik-Objekte. " +
                "Kurz nach Play schwingt er von allein los. Probier: Im 'Domino Starter' die 'Swing Speed' ändern.");

            // Zehn Dominosteine in einer Reihe nach +x.
            Transform row = SB.Group("Dominoes", parent);
            for (int i = 0; i < DominoCount; i++)
            {
                GameObject domino = Place(P.Domino, row, new Vector3(DominoFirstX + i * DominoSpacing, FloorTop, DominoRowZ), 0f, 1f, $"Domino_{i + 1:00}");
                Recolor(domino, "Tile", DominoColors[i % DominoColors.Length]);
                if (i == 0)
                {
                    Note(domino, "Dominosteine",
                        "Jeder Stein hat einen Rigidbody und einen Box Collider. Fällt einer um, stößt er den nächsten an – eine Kettenreaktion! " +
                        "Probier: Schieb einen Stein weiter weg (Position X) oder dreh ihn (Rotation Y). Klappt es dann noch?");
                }
            }

            // Niedriges Fangbrett hinter dem letzten Stein.
            GameObject catchBoard = SB.Box("Domino_CatchBoard", parent, new Vector3(CatchBoardX, FloorTop + 0.15f, DominoRowZ),
                new Vector3(0.15f, 0.3f, 1.2f), M.WarmOrange);
            SB.SetPhysicsMaterial(catchBoard, PM.Normal);

            return starter;
        }

        // ================================================================ B3 Hindernis-Parcours

        static void BuildObstacleCourse(Transform parent)
        {
            // Große Rampe: yaw 90 → hohe Kante bei z = 1.75, fällt nach -z ab (Ende bei z = -2.75).
            Place(P.TallRamp, parent, new Vector3(-9.0f, FloorTop, -0.5f), 90f, 1f, "Course_Ramp");

            GameObject blue = Place(P.PhysicsBall, parent, new Vector3(-9.0f, 2.19f, 0.84f), 0f, 1f, "PhysicsBall_Blue");
            Recolor(blue, "PhysicsBall_Blue", M.SkyBlue);
            PhysicsBall blueBall = blue.GetComponent<PhysicsBall>();
            if (blueBall != null)
            {
                blueBall.DisplayName = "Blaue Kugel";
                Record(blueBall);
            }
            Note(blue, "Die blaue Kugel",
                "Ich rolle die große Rampe hinunter und lande an der Mauer. Hilf mir ins Ziel: Bau mir mit den Mauern einen Weg! " +
                "Tipp: Eine schräg gestellte Mauer lenkt mich um.");

            GameObject target = Place(P.TargetZone, parent, new Vector3(-4.0f, FloorTop, -8.0f), 0f, 1f, "TargetZone");
            GoalZone targetGoal = target.GetComponent<GoalZone>();
            if (targetGoal != null)
            {
                targetGoal.SuccessMessage = "Geschafft! Eine Kugel ist im Ziel.";
                Record(targetGoal);
            }
            else
            {
                Debug.LogWarning("[Student Sandbox] TargetZone-Prefab hat keine GoalZone-Komponente.", target);
            }
            Note(target, "Ziel des Parcours",
                "Ein Trigger-Bereich wie bei der Murmelbahn. Hier ist 'Required Ball' leer – deshalb zählt jede Kugel, auch der gelbe Flummi.");

            // Bauteile zum Umbauen (liegen NICHT im geraden Weg der blauen Kugel, x zwischen -10 und -8).
            // WallBlock_2 liegt versetzt (z = 0.3 statt 1.2), weil sich zwei 3 m lange Mauern bei x = -6 und -3.5 überschneiden würden.
            Place(P.WallBlock, parent, new Vector3(-6.0f, FloorTop, 1.2f), 0f, 1f, "WallBlock_1");
            Place(P.WallBlock, parent, new Vector3(-3.2f, FloorTop, 0.3f), 0f, 1f, "WallBlock_2");
            Place(P.Bumper, parent, new Vector3(-5.5f, FloorTop, -3.5f), 0f, 1f, "Bumper_1");
            Place(P.SmallRamp, parent, new Vector3(-3.0f, FloorTop, -2.5f), 180f, 1f, "SmallRamp_1");
            Place(P.ObstacleCube, parent, new Vector3(-2.2f, FloorTop, -5.0f), 0f, 1f, "ObstacleCube_1");
            Place(P.ObstacleCube, parent, new Vector3(-2.2f, FloorTop, -5.8f), 0f, 1f, "ObstacleCube_2");
            Place(P.ObstacleCube, parent, new Vector3(-2.2f, 0.7f, -5.4f), 0f, 1f, "ObstacleCube_3");
            Place(P.BouncyBall, parent, new Vector3(-6.5f, 0.45f, -1.2f), 0f, 1f, "BouncyBall");

            // Aufgemalter Rahmen um den Parcours (gelbe Linien, 6–11 mm über dem Boden, ohne Collider).
            Transform marking = SB.Group("Course_Marking", parent);
            const float x0 = -11f, x1 = -1.5f, z0 = -11f, z1 = 2f, width = 0.12f, height = 0.005f;
            float y = FloorTop + 0.006f + height * 0.5f;
            SB.Box("Marking_North", marking, new Vector3((x0 + x1) * 0.5f, y, z1), new Vector3(x1 - x0 + width, height, width), M.SoftYellow, default, false);
            SB.Box("Marking_South", marking, new Vector3((x0 + x1) * 0.5f, y, z0), new Vector3(x1 - x0 + width, height, width), M.SoftYellow, default, false);
            SB.Box("Marking_West", marking, new Vector3(x0, y, (z0 + z1) * 0.5f), new Vector3(width, height, z1 - z0 - width), M.SoftYellow, default, false);
            SB.Box("Marking_East", marking, new Vector3(x1, y, (z0 + z1) * 0.5f), new Vector3(width, height, z1 - z0 - width), M.SoftYellow, default, false);
            SB.NoShadows(marking.gameObject);

            SB.Note(parent.gameObject, "Hindernis-Parcours (Rätsel)",
                "Die blaue Kugel rollt die große Rampe hinunter – aber am Ziel vorbei. Stell die Mauern (WallBlock) oder den Bumper so hin, " +
                "dass sie die Kugel ins Ziel (TargetZone) lenken. Verschieben mit dem W-Werkzeug, drehen mit E. Dann Play drücken und testen!");
        }

        // ================================================================ Knöpfe

        static void BuildControls(SandboxContext ctx, Transform parent, DominoStarter starter)
        {
            ClickableSwitch reset = SB.PushButton("ResetButton", parent, new Vector3(-4.0f, FloorTop, -9.6f), 0f,
                "Alles zurücksetzen (Taste R)", M.ButtonRed, "RESET");
            FitCaption(reset);
            if (ctx.Resetter != null)
            {
                SB.Listen(reset.OnPressed, ctx.Resetter.ResetAll);
            }
            else
            {
                Debug.LogWarning("[Student Sandbox] PhysicsZoneBuilder: ctx.Resetter fehlt – der RESET-Knopf hat keine Aktion.", reset);
            }
            SB.Note(reset.gameObject, "RESET-Knopf",
                "Klick im Play Mode auf mich: Alle Kugeln, Kegel, Dominosteine und der Hammer springen zurück an den Start. " +
                "Unten im Inspector bei 'On Pressed' siehst du, was beim Klicken passiert: PhysicsResetter.ResetAll. Die Taste R macht dasselbe.");

            ClickableSwitch dominoButton = SB.PushButton("DominoButton", parent, new Vector3(4.0f, FloorTop, -9.6f), 0f,
                "Domino-Hammer starten", M.ButtonOrange, "DOMINO");
            FitCaption(dominoButton);
            if (starter != null) SB.Listen(dominoButton.OnPressed, starter.StartSwing);
            SB.Note(dominoButton.gameObject, "DOMINO-Knopf",
                "Klick im Play Mode auf mich, dann schwingt der Hammer los. Bei 'On Pressed' ist der Hammer eingetragen " +
                "mit der Funktion DominoStarter.StartSwing. Mit dem + kannst du weitere Aktionen hinzufügen.");
        }

        static void FitCaption(ClickableSwitch button)
        {
            Transform caption = button.transform.Find("Caption");
            if (caption != null) FitText(caption.GetComponent<TextMesh>(), 0.56f);
        }

        // ================================================================ Schilder

        static void BuildSigns(Transform parent)
        {
            // Hinter der Murmelbahn (z > 8.7), Schrift zur Kamera (-z). Etwas breiter als 3.6 m, damit der Untertitel passt.
            MakeSign("Sign_MarbleRun", parent, new Vector3(-4.5f, FloorTop, 10.4f), "MURMELBAHN", "Schwerkraft + Rampe = Tempo!", 4.2f);
            MakeSign("Sign_Domino", parent, new Vector3(6.0f, FloorTop, -0.9f), "DOMINO", "Eine Kettenreaktion", 2.8f);
            MakeSign("Sign_ObstacleCourse", parent, new Vector3(-6.5f, FloorTop, -10.6f), "HINDERNIS-PARCOURS", "Bring die blaue Kugel ins Ziel!", 3.9f);
        }

        /// <summary>SB.Sign mit Akzentfarbe des Labors; zu lange Schrift wird auf die Schildbreite verkleinert.</summary>
        static GameObject MakeSign(string name, Transform parent, Vector3 position, string title, string subtitle, float width, float height = 2.2f)
        {
            GameObject sign = SB.Sign(name, parent, position, 0f, title, subtitle, M.WarmOrange, width, height);
            foreach (TextMesh text in sign.GetComponentsInChildren<TextMesh>(true)) FitText(text, width - 0.3f);
            return sign;
        }

        // ================================================================ Labor-Dekoration

        static void BuildDecoration(Transform parent)
        {
            // Nordwest-Ecke (hinter dem Startturm).
            Crate("Crate_1", parent, new Vector3(-10.5f, FloorTop, 9.7f), 0.9f, 8f);
            Crate("Crate_2", parent, new Vector3(-9.35f, FloorTop, 10.25f), 0.75f, -12f);
            Crate("Crate_3", parent, new Vector3(-10.5f, FloorTop + 0.9f, 9.7f), 0.6f, 25f);
            Place(P.LampPost, parent, new Vector3(-11.0f, FloorTop, 11.0f), 0f, 1f, "LampPost_Lab_2");

            // Südost-Ecke (neben dem Domino-Experiment).
            BuildBlackboard(parent, new Vector3(10.0f, FloorTop, -8.6f), 30f);
            Crate("Crate_4", parent, new Vector3(10.9f, FloorTop, -10.9f), 0.9f, -6f);
            Crate("Crate_5", parent, new Vector3(9.85f, FloorTop, -11.05f), 0.7f, 14f);
            Place(P.LampPost, parent, new Vector3(11.0f, FloorTop, -5.6f), 0f, 1f, "LampPost_Lab_1");
        }

        static void Crate(string name, Transform parent, Vector3 position, float size, float yaw)
        {
            Transform root = SB.Group(name, parent, position, yaw);
            SB.Box("Crate_Body", root, new Vector3(0f, size * 0.5f, 0f), Vector3.one * size, M.LightWood);
            float band = size * 0.12f;
            Vector3 bandSize = new Vector3(size + 0.02f, band, size + 0.02f);
            SB.Box("Crate_Band_Bottom", root, new Vector3(0f, size * 0.2f, 0f), bandSize, M.DarkWood, default, false);
            SB.Box("Crate_Band_Top", root, new Vector3(0f, size * 0.8f, 0f), bandSize, M.DarkWood, default, false);
        }

        static void BuildBlackboard(Transform parent, Vector3 position, float yaw)
        {
            Transform root = SB.Group("Blackboard", parent, position, yaw);
            const float boardWidth = 2.2f;
            const float boardHeight = 1.3f;
            const float boardY = 1.65f;
            foreach (float side in new[] { -1f, 1f })
            {
                SB.Box(side < 0f ? "Leg_Left" : "Leg_Right", root, new Vector3(side * (boardWidth * 0.5f - 0.05f), 1.2f, 0.06f),
                    new Vector3(0.1f, 2.4f, 0.1f), M.DarkWood);
            }
            SB.Box("Board_Frame", root, new Vector3(0f, boardY, 0f), new Vector3(boardWidth, boardHeight, 0.08f), M.DarkWood);
            SB.Box("Board_Surface", root, new Vector3(0f, boardY, -0.03f), new Vector3(boardWidth - 0.16f, boardHeight - 0.16f, 0.04f), M.Charcoal, default, false);
            SB.Box("Chalk_Tray", root, new Vector3(0f, boardY - boardHeight * 0.5f + 0.02f, -0.09f), new Vector3(1.6f, 0.04f, 0.12f), M.LightWood, default, false);

            float maxWidth = boardWidth - 0.4f;
            const float textZ = -0.062f;
            FitText(SB.Text("Text_Title", "Schwerkraft", root, new Vector3(0f, boardY + 0.36f, textZ), Vector3.zero, 0.2f, ChalkWhite), maxWidth);
            FitText(SB.Text("Text_Formula", "g = 9,81 m/s²", root, new Vector3(0f, boardY + 0.02f, textZ), Vector3.zero, 0.3f, ChalkWhite), maxWidth);
            FitText(SB.Text("Text_Hint", "Alles fällt nach unten!", root, new Vector3(0f, boardY - 0.32f, textZ), Vector3.zero, 0.14f, ChalkWhite, FontStyle.Normal), maxWidth);

            SB.Note(root.gameObject, "Die Tafel",
                "g = 9,81 m/s² ist die Schwerkraft der Erde: Sie zieht alles nach unten. Jeder Rigidbody mit Haken bei 'Use Gravity' fällt. " +
                "Probier: Nimm bei der roten Kugel den Haken weg – was passiert nach Play?");
        }

        // ================================================================ Hilfen

        /// <summary>
        /// Änderungen an Prefab-Instanzen (per Skript) als Override speichern – sonst gehen sie beim Neuladen der Szene verloren.
        /// </summary>
        static void Record(Object target)
        {
            if (target != null && PrefabUtility.IsPartOfPrefabInstance(target))
                PrefabUtility.RecordPrefabInstancePropertyModifications(target);
        }

        /// <summary>SB.Place + Name/Position/Drehung als Override der Prefab-Instanz speichern.</summary>
        static GameObject Place(string prefabName, Transform parent, Vector3 localPosition, float yaw, float uniformScale, string instanceName)
        {
            GameObject instance = SB.Place(prefabName, parent, localPosition, yaw, uniformScale, instanceName);
            Record(instance);
            Record(instance.transform);
            return instance;
        }

        /// <summary>SB.Recolor + Override speichern (für Prefab-Instanzen).</summary>
        static void Recolor(GameObject root, string childName, string materialKey)
        {
            SB.Recolor(root, childName, materialKey);
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer.gameObject.name == childName) Record(renderer);
            }
        }

        /// <summary>
        /// Erklär-Notiz. Bei Prefab-Instanzen wird die vorhandene Notiz des Prefabs überschrieben (bzw. eine neue angehängt)
        /// und als Override gespeichert – ohne Komponenten umzusortieren (das ist bei Prefab-Instanzen nicht erlaubt).
        /// </summary>
        static void Note(GameObject target, string title, string text)
        {
            if (!PrefabUtility.IsPartOfPrefabInstance(target))
            {
                SB.Note(target, title, text);
                return;
            }
            InfoNote note = target.GetComponent<InfoNote>();
            if (note == null) note = target.AddComponent<InfoNote>();
            note.Title = title;
            note.Text = text;
            Record(note);
        }

        /// <summary>Verkleinert einen TextMesh, falls er breiter als maxWidth (Meter) wäre.</summary>
        static void FitText(TextMesh text, float maxWidth)
        {
            if (text == null || string.IsNullOrEmpty(text.text) || maxWidth <= 0f) return;
            float width = EstimateTextWidth(text);
            if (width > maxWidth) text.characterSize *= maxWidth / width;
        }

        /// <summary>
        /// Geschätzte Breite in Metern. TextMesh: 1 em = fontSize · characterSize · 0.1 Meter.
        /// Zeichenbreiten sind grob an Arial angelehnt (eher etwas zu breit geschätzt).
        /// </summary>
        static float EstimateTextWidth(TextMesh text)
        {
            float line = 0f;
            float longest = 0f;
            foreach (char c in text.text)
            {
                if (c == '\n')
                {
                    longest = Mathf.Max(longest, line);
                    line = 0f;
                    continue;
                }
                line += CharWidth(c);
            }
            longest = Mathf.Max(longest, line);
            bool bold = text.fontStyle == FontStyle.Bold || text.fontStyle == FontStyle.BoldAndItalic;
            if (bold) longest *= 1.06f;
            float fontSize = text.fontSize > 0 ? text.fontSize : 16f;
            return longest * fontSize * text.characterSize * 0.1f;
        }

        static float CharWidth(char c)
        {
            if (c == ' ') return 0.28f;
            if ("iljI.,:;!'|".IndexOf(c) >= 0) return 0.26f;
            if ("ftr-()/[]".IndexOf(c) >= 0) return 0.34f;
            if ("mwMW".IndexOf(c) >= 0) return 0.85f;
            if (char.IsDigit(c)) return 0.56f;
            if (char.IsUpper(c)) return 0.70f;
            if (char.IsLower(c)) return 0.54f;
            return 0.6f;
        }
    }
}
