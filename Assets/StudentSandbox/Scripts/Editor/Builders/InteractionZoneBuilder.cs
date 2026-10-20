using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace StudentSandbox.EditorTools
{
    /// <summary>
    /// Baut den Interaktionsbereich (06_INTERACTION_ZONE, Welt (0, 0, -25)):
    /// eine kleine futuristische Technik-Plattform mit vier Experimenten zu Komponenten, Skripten und Events.
    ///
    /// Draufsicht (lokal, +z = Norden = Eingang vom zentralen Platz, der Besucher schaut nach Süden):
    ///
    ///   z = +12   ─────────── Eingang (Bogen bei z = 11.3) ───────────
    ///   z ≈ 6     Laterne · 5 Knöpfe · Laterne   (Kontrollpult, Bildschirm dahinter)
    ///   z ≈ 4.5                                         Kristall (x = 7)
    ///   z = -2    Wand mit automatischer Tür (x = -7), Schatzkammer dahinter
    ///   z ≈ -6    Ampel (x = 0) an einer kurzen Straße       fahrende Plattform (x = 7)
    ///   z = -12   ─────────────────────────────────────────────────────
    ///
    /// Alle Koordinaten sind lokal zum Bereich (der Bereich ist nicht gedreht). Bodenoberkante y = 0.1.
    /// Flache Markierungen haben eigene Höhen (kein Z-Fighting):
    /// Boden 0.1 · Straße/Platten 0.11 · Glow-Ring 0.112 · Linien/Zebrastreifen 0.115 · Teppich 0.116 · Knoten 0.12.
    /// </summary>
    public static class InteractionZoneBuilder
    {
        // ------------------------------------------------------------------ Höhen und Maße

        const float FloorTop = SandboxContext.ZoneFloorTop; // 0.1
        const float TileTop = 0.11f;
        const float LineTop = 0.115f;
        const float PadTop = 0.12f;
        const float LineWidth = 0.04f;

        const float WallZ = -2.0f;
        const float WallThickness = 0.5f;
        const float WallHeight = 3.4f;
        const float DoorX = -7.0f;
        const float DoorWidth = 2.4f;
        const float DoorHeight = 3.0f;

        // Kontrollpult: Die Knöpfe stehen so weit vom Eingangsbogen weg, dass der Bogen-Balken
        // ihre Beschriftung vom Aussichtspunkt 5 aus nicht verdeckt.
        const float DeskZ = 4.8f;          // Mitte des Pults (Tiefe 1 m → z 4.3 … 5.3)
        const float DeskButtonZ = 6.0f;    // Knöpfe VOR dem Pult (vom Eingang aus gesehen)
        const float DeskLampZ = 6.4f;
        const float MaxCaptionPlateWidth = 0.86f;

        static readonly Color TextDark = new Color(0.18f, 0.2f, 0.25f);

        public static void Build(SandboxContext ctx)
        {
            Transform zone = ctx.InteractionZone;

            Transform environment = SB.Group("StaticEnvironment", zone);
            Transform doorGroup = SB.Group("DoorExperiment", zone);
            Transform trafficGroup = SB.Group("TrafficLightExperiment", zone);
            Transform crystalGroup = SB.Group("CrystalExperiment", zone);
            Transform platformGroup = SB.Group("PlatformExperiment", zone);
            Transform deskGroup = SB.Group("ControlDesk", zone);
            Transform signs = SB.Group("Signs", zone);

            BuildEnvironment(zone, environment);
            DoorController door = BuildDoorExperiment(doorGroup, signs);
            TrafficLightController traffic = BuildTrafficLightExperiment(trafficGroup, signs);
            Rotator crystalRotator = BuildCrystalExperiment(crystalGroup, signs);
            MovingPlatform platform = BuildPlatformExperiment(platformGroup, signs);
            BuildControlDesk(deskGroup, door, traffic, crystalRotator, platform);

            SB.Note(zone.gameObject, "Interaktionsbereich – Schalter & Verhalten",
                "Hier reagieren Objekte auf deine Klicks. Es gibt vier Experimente (vom Eingang aus gesehen): " +
                "die Tür mit Schatzkammer (rechts), die Ampel (hinten in der Mitte), den Kristall (links) und die fahrende Plattform (hinten links).\n" +
                "Drücke Play und klicke mit der LINKEN Maustaste auf die Knöpfe – direkt bei den Experimenten oder am Kontrollpult.\n" +
                "Jeder Knopf hat ein Event \"On Pressed\": Dort steht, welches Objekt reagiert und welche Funktion aufgerufen wird.");
        }

        // ------------------------------------------------------------------ Boden, Rand, Bodenplatten, Leuchtlinien

        static void BuildEnvironment(Transform zone, Transform environment)
        {
            SB.Box("Floor", environment, new Vector3(0f, 0.05f, 0f), new Vector3(24f, 0.1f, 24f), M.TechFloor);

            // Leuchtender Rand (0.25 hoch, 0.3 dick) mit 5 m Lücke am Eingang (z = +12, x ∈ [-2.5, 2.5]).
            Transform trim = SB.Group("Trim", environment);
            const float trimY = FloorTop + 0.125f;
            TrimPiece("Trim_South", trim, new Vector3(0f, trimY, -11.85f), new Vector3(24f, 0.25f, 0.3f));
            TrimPiece("Trim_West", trim, new Vector3(-11.85f, trimY, 0.15f), new Vector3(0.3f, 0.25f, 23.7f));
            TrimPiece("Trim_East", trim, new Vector3(11.85f, trimY, 0.15f), new Vector3(0.3f, 0.25f, 23.7f));
            TrimPiece("Trim_North_West", trim, new Vector3(-7.1f, trimY, 11.85f), new Vector3(9.2f, 0.25f, 0.3f));
            TrimPiece("Trim_North_East", trim, new Vector3(7.1f, trimY, 11.85f), new Vector3(9.2f, 0.25f, 0.3f));

            // Leicht erhöhte Bodenplatten – nur an zwei Stellen: Vorplatz am Eingang und vor der Tür.
            Transform tiles = SB.Group("FloorTiles", environment);
            float[] entranceColumns = { -1.65f, 0f, 1.65f };
            float[] entranceRows = { 7.5f, 9.15f, 10.8f };
            int tileIndex = 1;
            foreach (float row in entranceRows)
                foreach (float column in entranceColumns)
                    FlatBox("Tile_Entrance_" + tileIndex++, tiles, column, row, 1.55f, 1.55f, TileTop, 0.02f, M.TechPanel);

            float[] doorColumns = { -9.0f, -7.0f, -5.0f };
            float[] doorRows = { -0.75f, 1.15f };
            tileIndex = 1;
            foreach (float row in doorRows)
                foreach (float column in doorColumns)
                    FlatBox("Tile_Door_" + tileIndex++, tiles, column, row, 1.9f, 1.85f, TileTop, 0.02f, M.TechPanel);

            BuildFloorCircuit(environment);

            SB.MarkStatic(environment.gameObject);

            SB.EntranceArch("EntranceArch", zone, new Vector3(0f, 0.1f, 11.3f), 180, "INTERACTIONS", "Schalter & Verhalten", M.Turquoise);
        }

        static void TrimPiece(string name, Transform parent, Vector3 position, Vector3 size)
        {
            GameObject piece = SB.Box(name, parent, position, size, M.NeonTeal);
            SB.NoShadows(piece);
        }

        /// <summary>
        /// Dünne Leuchtlinien wie Kabel: vom Kontrollpult zu Tür, Ampel, Kristall und Plattform,
        /// außerdem vom Tür-Schalter zur Tür. Nur Deko – die echte Verbindung ist das Event "On Pressed".
        /// </summary>
        static void BuildFloorCircuit(Transform environment)
        {
            Transform circuit = SB.Group("FloorCircuit", environment);

            // Hauptleitung: hinter dem Pult nach Süden bis zur Ampel.
            Line("Line_Main", circuit, 0f, DeskZ - 0.52f, 0f, -5.8f);
            // Zur Tür.
            Line("Line_Door_1", circuit, 0f, 2.6f, DoorX, 2.6f);
            Line("Line_Door_2", circuit, DoorX, 2.6f, DoorX, -1.6f);
            // Vom Schalter neben der Tür zur Tür.
            Line("Line_DoorSwitch", circuit, -9.1f, 0.2f, DoorX, 0.2f);
            // Zum Kristall.
            Line("Line_Crystal", circuit, 0f, 3.6f, 5.4f, 3.6f);
            // Zur Plattform.
            Line("Line_Platform_1", circuit, 0f, -5.3f, 7f, -5.3f);
            Line("Line_Platform_2", circuit, 7f, -5.3f, 7f, -6.0f);
            // Leuchtlinie vor den Knöpfen des Kontrollpults.
            Line("Line_DeskFront", circuit, -4.4f, DeskButtonZ + 0.52f, 4.4f, DeskButtonZ + 0.52f);

            // Knotenpunkte und Endpunkte (etwas höher als die Linien).
            Pad("Node_Crystal", circuit, 0f, 3.6f, 0.14f);
            Pad("Node_Door", circuit, 0f, 2.6f, 0.14f);
            Pad("Node_Door_Corner", circuit, DoorX, 2.6f, 0.14f);
            Pad("Node_DoorSwitch", circuit, DoorX, 0.2f, 0.14f);
            Pad("Node_Platform", circuit, 0f, -5.3f, 0.14f);
            Pad("Node_Platform_Corner", circuit, 7f, -5.3f, 0.14f);
            Pad("End_TrafficLight", circuit, 0f, -5.68f, 0.24f);
            Pad("End_Door", circuit, DoorX, -1.6f, 0.24f);
            Pad("End_DoorSwitch", circuit, -9.1f, 0.2f, 0.24f);
            Pad("End_Crystal", circuit, 5.4f, 3.6f, 0.24f);
            Pad("End_Platform", circuit, 7f, -6.0f, 0.24f);

            SB.Note(circuit.gameObject, "Leuchtlinien (nur Deko)",
                "Die leuchtenden Linien sehen aus wie Kabel vom Kontrollpult zu den Experimenten.\n" +
                "In Unity braucht man keine Kabel: Die echte Verbindung macht das Event \"On Pressed\" an jedem Knopf.");
        }

        static void Line(string name, Transform parent, float x1, float z1, float x2, float z2)
        {
            float sizeX = Mathf.Abs(x2 - x1) + LineWidth;
            float sizeZ = Mathf.Abs(z2 - z1) + LineWidth;
            FlatBox(name, parent, (x1 + x2) * 0.5f, (z1 + z2) * 0.5f, sizeX, sizeZ, LineTop, 0.02f, M.NeonTeal);
        }

        static void Pad(string name, Transform parent, float x, float z, float size)
        {
            FlatBox(name, parent, x, z, size, size, PadTop, 0.02f, M.NeonTeal);
        }

        // ------------------------------------------------------------------ D1: Tür + Schatzkammer (Westen)

        static DoorController BuildDoorExperiment(Transform doorGroup, Transform signs)
        {
            float wallY = FloorTop + WallHeight * 0.5f;
            float wallTop = FloorTop + WallHeight;                         // 3.5
            float openingLeft = DoorX - DoorWidth * 0.5f;                  // -8.2
            float openingRight = DoorX + DoorWidth * 0.5f;                 // -5.8
            float doorTop = FloorTop + DoorHeight;                         // 3.1
            float wallFront = WallZ + WallThickness * 0.5f;                // -1.75 (Seite zum Besucher)

            // Wand mit Öffnung (links, rechts, Sturz darüber) und türkiser Abdeckung.
            Transform wall = SB.Group("Door_Wall", doorGroup);
            SB.Box("Wall_Left", wall, new Vector3((-11.5f + openingLeft) * 0.5f, wallY, WallZ),
                new Vector3(openingLeft + 11.5f, WallHeight, WallThickness), M.WarmWhite);
            SB.Box("Wall_Right", wall, new Vector3((openingRight - 2.5f) * 0.5f, wallY, WallZ),
                new Vector3(-2.5f - openingRight, WallHeight, WallThickness), M.WarmWhite);
            SB.Box("Lintel", wall, new Vector3(DoorX, (doorTop + wallTop) * 0.5f, WallZ),
                new Vector3(DoorWidth, wallTop - doorTop, WallThickness), M.WarmWhite);
            SB.Box("Wall_Cap", wall, new Vector3(-7.0f, wallTop + 0.06f, WallZ), new Vector3(9.1f, 0.12f, 0.6f), M.Turquoise, default, false);

            // Leuchtender Türrahmen auf der Vorderseite der Wand.
            Transform frame = SB.Group("Door_Frame", doorGroup);
            const float frameWidth = 0.12f;
            float frameZ = wallFront + 0.02f;
            float frameHeight = DoorHeight + frameWidth;
            GameObject frameLeft = SB.Box("Frame_Left", frame, new Vector3(openingLeft - frameWidth * 0.5f, FloorTop + frameHeight * 0.5f, frameZ),
                new Vector3(frameWidth, frameHeight, 0.08f), M.NeonTeal, default, false);
            GameObject frameRight = SB.Box("Frame_Right", frame, new Vector3(openingRight + frameWidth * 0.5f, FloorTop + frameHeight * 0.5f, frameZ),
                new Vector3(frameWidth, frameHeight, 0.08f), M.NeonTeal, default, false);
            GameObject frameTop = SB.Box("Frame_Top", frame, new Vector3(DoorX, doorTop + frameWidth * 0.5f, frameZ),
                new Vector3(DoorWidth, frameWidth, 0.08f), M.NeonTeal, default, false);
            SB.NoShadows(frameLeft);
            SB.NoShadows(frameRight);
            SB.NoShadows(frameTop);

            // Die Tür: genau EIN Objekt (Transform, MeshFilter, MeshRenderer, BoxCollider, DoorController, InfoNote).
            GameObject doorObject = SB.Box("AutomaticDoor", doorGroup, new Vector3(DoorX, FloorTop + DoorHeight * 0.5f, WallZ),
                new Vector3(DoorWidth, DoorHeight, 0.25f), M.Turquoise);
            DoorController door = doorObject.AddComponent<DoorController>();
            door.Motion = DoorMotion.Slide;
            door.SlideOffset = new Vector3(2.6f, 0f, 0f); // fährt in die rechte Wand hinein (Schiebetür)
            door.OpenSpeed = 1f;
            SB.Note(doorObject, "Automatische Tür",
                "Ich bin die Tür. Mein Verhalten kommt vom Skript Door Controller.\n" +
                "Probier: Open Speed (0.2 = ganz langsam), Slide Offset (0, 3, 0) = Tür fährt nach oben.\n" +
                "Danach Play und auf den Schalter klicken.");

            BuildTreasureRoom(doorGroup);

            // Schalter neben der Tür.
            ClickableSwitch doorSwitch = SB.PushButton("DoorSwitch", doorGroup, new Vector3(-9.6f, 0.1f, 0.2f), 180,
                "Tür öffnen / schließen", M.ButtonTeal, "TÜR");
            SB.Listen(doorSwitch.OnPressed, door.Toggle);
            SB.Note(doorSwitch.gameObject, "Schalter für die Tür",
                "Schau unten bei Clickable Switch in die Liste \"On Pressed ()\".\n" +
                "Hier steht: AutomaticDoor → DoorController.Toggle. Das ist ein Event: Klick → Tür bewegt sich.\n" +
                "Toggle heißt umschalten: Ist die Tür zu, geht sie auf. Ist sie offen, geht sie zu.");

            // Schild oben auf der Wand über der Tür – verdeckt so nichts.
            SB.Sign("Sign_Door", signs, new Vector3(DoorX, wallTop + 0.12f, WallZ), 180, "TÜR", "Klick auf den Knopf!", M.Turquoise, 2.6f, 0.75f);

            return door;
        }

        static void BuildTreasureRoom(Transform doorGroup)
        {
            Transform room = SB.Group("TreasureRoom", doorGroup);
            const float roomWallHeight = 2.5f;
            const float roomWallY = FloorTop + roomWallHeight * 0.5f;
            const float roomTop = FloorTop + roomWallHeight;
            const float backZ = -7.0f;
            const float innerBack = backZ + 0.15f;            // -6.85
            float wallBack = WallZ - WallThickness * 0.5f;    // -2.25
            float sideLength = wallBack - innerBack;          // 4.6
            float sideCenterZ = (wallBack + innerBack) * 0.5f;

            // Wände ohne Dach (von oben kann man hineinschauen). Ecken stoßen nur aneinander (keine doppelten Flächen).
            SB.Box("Room_Wall_West", room, new Vector3(-10.5f, roomWallY, sideCenterZ), new Vector3(0.3f, roomWallHeight, sideLength), M.WarmWhite);
            SB.Box("Room_Wall_East", room, new Vector3(-3.5f, roomWallY, sideCenterZ), new Vector3(0.3f, roomWallHeight, sideLength), M.WarmWhite);
            SB.Box("Room_Wall_Back", room, new Vector3(-7.0f, roomWallY, backZ), new Vector3(7.3f, roomWallHeight, 0.3f), M.WarmWhite);
            // Abdeckungen: die Seiten enden genau dort, wo die hintere Abdeckung beginnt (keine überlappenden Flächen).
            const float backCapInner = backZ + 0.2f;          // -6.8
            float sideCapLength = wallBack - backCapInner;    // 4.55
            float sideCapCenterZ = (wallBack + backCapInner) * 0.5f;
            SB.Box("Room_Cap_West", room, new Vector3(-10.5f, roomTop + 0.05f, sideCapCenterZ), new Vector3(0.4f, 0.1f, sideCapLength), M.Turquoise, default, false);
            SB.Box("Room_Cap_East", room, new Vector3(-3.5f, roomTop + 0.05f, sideCapCenterZ), new Vector3(0.4f, 0.1f, sideCapLength), M.Turquoise, default, false);
            SB.Box("Room_Cap_Back", room, new Vector3(-7.0f, roomTop + 0.05f, backZ), new Vector3(7.4f, 0.1f, 0.4f), M.Turquoise, default, false);

            // Boden: Technik-Platte, lila Teppich zur Mitte, goldener Leuchtring um den Sockel.
            const float pedestalZ = -4.4f;
            FlatBox("Room_Floor", room, -7.0f, sideCenterZ, 6.7f, sideLength, TileTop, 0.02f, M.TechPanel);
            float carpetEnd = pedestalZ + 0.8f;
            FlatBox("Room_Carpet", room, DoorX, (wallBack + carpetEnd) * 0.5f, 1.4f, wallBack - carpetEnd, 0.116f, 0.02f, M.BerryPurple);
            GameObject glowRing = SB.MeshObj(MK.Cylinder12, "Pedestal_Glow", room, new Vector3(DoorX, PadTop - 0.03f, pedestalZ),
                new Vector3(1.7f, 0.03f, 1.7f), M.GlowSun);
            SB.NoShadows(glowRing);

            // Sockel mit goldenem Stern.
            SB.MeshObj(MK.Cylinder8, "Pedestal", room, new Vector3(DoorX, FloorTop, pedestalZ), new Vector3(0.9f, 0.8f, 0.9f),
                M.WarmWhite, default, ColliderMode.ConvexMesh);
            SB.MeshObj(MK.Cylinder8, "Pedestal_Top", room, new Vector3(DoorX, FloorTop + 0.8f, pedestalZ), new Vector3(1.05f, 0.1f, 1.05f),
                M.Turquoise, default, ColliderMode.ConvexMesh);

            GameObject star = SB.MeshObj(MK.Star, "TreasureStar", room, new Vector3(DoorX, 1.65f, pedestalZ), new Vector3(0.95f, 0.95f, 0.22f), M.GlowSun);
            SB.NoShadows(star);
            Rotator starRotator = star.AddComponent<Rotator>();
            starRotator.Speed = 60f;
            starRotator.RotationAxis = Vector3.up;
            SB.Note(star, "Der Schatz",
                "Ein goldener Stern. Er dreht sich mit dem Rotator-Skript (Speed 60).\n" +
                "Du siehst ihn am besten, wenn die Tür offen ist.\n" +
                "Probier: Ändere Color am Licht \"TreasureLight\".");

            SB.PointLight("TreasureLight", room, new Vector3(DoorX, 2.4f, pedestalZ), new Color(1f, 0.8f, 0.45f), 4f, 5f);

            SB.Note(room.gameObject, "Schatzkammer",
                "Der kleine Raum hinter der Tür. Er hat kein Dach – von oben kannst du hineinschauen.\n" +
                "Öffne im Play Mode die Tür, dann siehst du den Schatz.");
        }

        // ------------------------------------------------------------------ D2: Ampel an einer kurzen Straße (Mitte, Süden)

        static TrafficLightController BuildTrafficLightExperiment(Transform trafficGroup, Transform signs)
        {
            // Straße: x ∈ [-3.0, 2.4] (endet vor der fahrenden Plattform), z ∈ [-10, -7], Oberkante 0.11.
            const float roadWest = -3.0f;
            const float roadEast = 2.4f;
            const float roadZ = -8.5f;
            const float roadWidth = 3.0f;
            const float roadLength = roadEast - roadWest;
            const float roadCenterX = (roadWest + roadEast) * 0.5f;
            const float sidewalkTop = 0.18f;
            const float zebraX = 1.0f;

            Transform street = SB.Group("Street", trafficGroup);
            SB.Box("Road", street, new Vector3(roadCenterX, TileTop - 0.02f, roadZ), new Vector3(roadLength, 0.04f, roadWidth), M.Asphalt);
            SB.Box("Sidewalk_North", street, new Vector3(roadCenterX, FloorTop + 0.04f, -6.4f), new Vector3(roadLength, 0.08f, 1.2f), M.StoneGray);
            SB.Box("Sidewalk_South", street, new Vector3(roadCenterX, FloorTop + 0.04f, -10.5f), new Vector3(roadLength, 0.08f, 1.0f), M.StoneGray);

            // Fahrbahnmarkierungen (weiß, flach, ohne Collider).
            Transform markings = SB.Group("RoadMarkings", street);
            FlatBox("Line_Edge_North", markings, roadCenterX, roadZ + 1.36f, roadLength - 0.2f, 0.06f, LineTop, 0.01f, M.Stripe);
            FlatBox("Line_Edge_South", markings, roadCenterX, roadZ - 1.36f, roadLength - 0.2f, 0.06f, LineTop, 0.01f, M.Stripe);
            float[] dashes = { -2.5f, -1.5f, -0.5f };
            for (int i = 0; i < dashes.Length; i++)
                FlatBox("Line_Center_" + (i + 1), markings, dashes[i], roadZ, 0.5f, 0.08f, LineTop, 0.01f, M.Stripe);
            float[] zebraOffsets = { -1.1f, -0.66f, -0.22f, 0.22f, 0.66f, 1.1f };
            for (int i = 0; i < zebraOffsets.Length; i++)
                FlatBox("Zebra_" + (i + 1), markings, zebraX, roadZ + zebraOffsets[i], 1.5f, 0.28f, LineTop, 0.01f, M.Stripe);

            // Zwei Poller am Zebrastreifen (Deko).
            BuildBollard("Bollard_West", street, new Vector3(zebraX - 0.95f, sidewalkTop, -10.45f));
            BuildBollard("Bollard_East", street, new Vector3(zebraX + 0.95f, sidewalkTop, -10.45f));

            TrafficLightController controller = BuildTrafficLight(trafficGroup);

            // Kleine Figur, die am Zebrastreifen wartet (Deko, ohne Collider).
            Transform pedestrian = SB.Group("Pedestrian", trafficGroup, new Vector3(zebraX, sidewalkTop, -6.45f));
            SB.Prim(PrimitiveType.Capsule, "Body", pedestrian, new Vector3(0f, 0.5f, 0f), new Vector3(0.38f, 0.5f, 0.32f), M.CoralRed, default, false);
            SB.Prim(PrimitiveType.Sphere, "Head", pedestrian, new Vector3(0f, 1.2f, 0f), Vector3.one * 0.36f, M.SoftYellow, default, false);
            SB.MeshObj(MK.Cylinder8, "Hat", pedestrian, new Vector3(0f, 1.33f, 0f), new Vector3(0.3f, 0.08f, 0.3f), M.Turquoise);

            // Schild hinter der Straße: verdeckt so weder Ampel noch Straße.
            SB.Sign("Sign_Traffic", signs, new Vector3(-0.4f, 0.1f, -11.15f), 180, "AMPEL", "Eine Zustandsmaschine", M.Turquoise, 3.0f);

            return controller;
        }

        static void BuildBollard(string name, Transform parent, Vector3 position)
        {
            Transform bollard = SB.Group(name, parent, position);
            SB.MeshObj(MK.Cylinder8, "Post", bollard, Vector3.zero, new Vector3(0.2f, 0.7f, 0.2f), M.Charcoal);
            SB.MeshObj(MK.Cylinder8, "Reflector", bollard, new Vector3(0f, 0.48f, 0f), new Vector3(0.215f, 0.08f, 0.215f), M.Stripe);
        }

        /// <summary>
        /// Die Ampel. Wurzel bei (0, 0.1, -6.3) mit yaw 180: lokal -z ("vorne") zeigt nach Welt +z, also zum Besucher.
        /// </summary>
        static TrafficLightController BuildTrafficLight(Transform trafficGroup)
        {
            Transform root = SB.Group("TrafficLight", trafficGroup, new Vector3(0f, 0.1f, -6.3f), 180f);

            SB.MeshObj(MK.Cylinder8, "Foundation", root, Vector3.zero, new Vector3(0.42f, 0.14f, 0.42f), M.Charcoal);
            SB.MeshObj(MK.Cylinder8, "Pole", root, Vector3.zero, new Vector3(0.16f, 2.5f, 0.16f), M.Charcoal, default, ColliderMode.ConvexMesh);

            // Gehäuse: 0.6 x 1.75 x 0.45, Höhe 2.4 bis 4.15. Dahinter eine Kontrastblende mit weißem Rand.
            const float housingY = 3.275f;
            const float housingFront = -0.225f;
            SB.Box("Housing", root, new Vector3(0f, housingY, 0f), new Vector3(0.6f, 1.75f, 0.45f), M.Charcoal);
            SB.Box("Housing_Top", root, new Vector3(0f, 4.175f, 0f), new Vector3(0.66f, 0.05f, 0.5f), M.Charcoal, default, false);
            SB.Box("BackPlate", root, new Vector3(0f, housingY, 0.235f), new Vector3(0.74f, 1.88f, 0.02f), M.Charcoal, default, false);
            SB.Box("BackPlate_Border", root, new Vector3(0f, housingY, 0.26f), new Vector3(0.84f, 1.98f, 0.03f), M.WarmWhite, default, false);

            // Drei Lampen als eigene GameObjects (mit Collider), leicht vor dem Gehäuse.
            Renderer red = Lamp("Lamp_Red", root, housingY + 0.55f, M.TrafficRedOff);
            Renderer yellow = Lamp("Lamp_Yellow", root, housingY, M.TrafficYellowOff);
            Renderer green = Lamp("Lamp_Green", root, housingY - 0.55f, M.TrafficGreenOff);
            Visor("Visor_Red", root, housingY + 0.55f, housingFront);
            Visor("Visor_Yellow", root, housingY, housingFront);
            Visor("Visor_Green", root, housingY - 0.55f, housingFront);

            TrafficLightController controller = root.gameObject.AddComponent<TrafficLightController>();
            controller.Setup(red, yellow, green,
                SB.Mat(M.TrafficRedOn), SB.Mat(M.TrafficRedOff),
                SB.Mat(M.TrafficYellowOn), SB.Mat(M.TrafficYellowOff),
                SB.Mat(M.TrafficGreenOn), SB.Mat(M.TrafficGreenOff));
            SB.Note(root.gameObject, "Ampel (Zustandsmaschine)",
                "Die Ampel ist eine Zustandsmaschine: Sie ist immer in genau einer Phase.\n" +
                "Im Inspector unter Phases kannst du die Reihenfolge ändern oder neue Phasen hinzufügen.\n" +
                "Auto Cycle = schaltet von allein.");

            BuildPedestrianButton(root, controller);
            return controller;
        }

        static Renderer Lamp(string name, Transform root, float y, string offMaterial)
        {
            GameObject lamp = SB.Prim(PrimitiveType.Sphere, name, root, new Vector3(0f, y, -0.12f), Vector3.one * 0.42f, offMaterial);
            Renderer renderer = lamp.GetComponent<Renderer>();
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            return renderer;
        }

        static void Visor(string name, Transform root, float lampY, float housingFront)
        {
            GameObject visor = SB.Box(name, root, new Vector3(0f, lampY + 0.235f, housingFront - 0.1f), new Vector3(0.46f, 0.03f, 0.2f),
                M.Charcoal, new Vector3(-8f, 0f, 0f), false);
            visor.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
        }

        /// <summary>
        /// Gelber Fußgänger-Kasten ("Signal kommt") am Mast. Der rote Knopf sitzt auf einem "CapMount",
        /// dessen lokale +y-Achse nach vorne zeigt – so wird er beim Klicken in den Kasten gedrückt.
        /// </summary>
        static void BuildPedestrianButton(Transform trafficLightRoot, TrafficLightController controller)
        {
            Transform button = SB.Group("PedestrianButton", trafficLightRoot, new Vector3(0f, 1.1f, -0.155f));
            SB.Box("Button_Box", button, Vector3.zero, new Vector3(0.25f, 0.35f, 0.15f), M.SoftYellow);
            SB.Text("Label", "DRÜCKEN", button, new Vector3(0f, 0.11f, -0.078f), Vector3.zero, 0.044f, TextDark);
            SB.Box("Signal_Panel", button, new Vector3(0f, 0.045f, -0.076f), new Vector3(0.18f, 0.05f, 0.004f), M.Charcoal, default, false);

            Transform mount = SB.Group("CapMount", button, new Vector3(0f, -0.07f, -0.075f));
            mount.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            GameObject cap = SB.Prim(PrimitiveType.Cylinder, "ButtonCap", mount, new Vector3(0f, 0.02f, 0f), new Vector3(0.1f, 0.02f, 0.1f), M.ButtonRed);

            ClickableSwitch clickable = button.gameObject.AddComponent<ClickableSwitch>();
            clickable.Label = "Ampel weiterschalten";
            clickable.ButtonCap = cap.transform;
            SetPressDepth(clickable, 0.025f);
            SB.Listen(clickable.OnPressed, controller.NextPhase);

            SB.Note(button.gameObject, "Fußgänger-Knopf",
                "Klick im Play Mode auf den gelben Kasten: Die Ampel schaltet eine Phase weiter.\n" +
                "In \"On Pressed ()\" steht: TrafficLight → TrafficLightController.NextPhase.");
        }

        // ------------------------------------------------------------------ D3: Rotierender Kristall (Nordosten)

        static Rotator BuildCrystalExperiment(Transform crystalGroup, Transform signs)
        {
            Vector3 center = new Vector3(7.0f, 0f, 4.5f);

            // Runde Technik-Platte mit leuchtendem Rand (zwei gestapelte Scheiben → sauberer Ring).
            GameObject padGlow = SB.MeshObj(MK.Cylinder12, "Crystal_Pad_Glow", crystalGroup, new Vector3(center.x, 0.095f, center.z),
                new Vector3(3.5f, 0.017f, 3.5f), M.NeonTeal);
            SB.NoShadows(padGlow);
            GameObject pad = SB.MeshObj(MK.Cylinder12, "Crystal_Pad", crystalGroup, new Vector3(center.x, 0.095f, center.z),
                new Vector3(3.2f, PadTop - 0.095f, 3.2f), M.TechPanel);
            SB.NoShadows(pad);

            GameObject crystalSet = SB.Place(P.RotatingCrystal, crystalGroup, new Vector3(center.x, PadTop, center.z), 0, 1.2f, "RotatingCrystal");

            Transform crystal = FindDeep(crystalSet.transform, "Crystal");
            Rotator rotator = crystal != null ? crystal.GetComponent<Rotator>() : null;
            if (rotator == null)
            {
                Debug.LogWarning("[Student Sandbox] RotatingCrystal: Kind \"Crystal\" mit Rotator nicht gefunden – nehme den ersten Rotator.");
                rotator = crystalSet.GetComponentInChildren<Rotator>(true);
            }

            GameObject noteTarget = rotator != null ? rotator.gameObject : crystalSet;
            SB.Note(noteTarget, "Kristall (Rotator)",
                "Ich drehe mich mit dem Rotator-Skript.\n" +
                "Aufgabe: Mach mich doppelt so schnell – ändere Speed von 45 auf 90.\n" +
                "Probier auch Rotation Axis (1,0,0).");

            // Schild etwas östlich vor dem Kristall, damit es den Kristall vom Platz aus nicht verdeckt.
            SB.Sign("Sign_Crystal", signs, new Vector3(8.3f, 0.1f, 7.6f), 180, "KRISTALL", "Rotator: Speed ändern", M.Turquoise, 2.9f);

            return rotator;
        }

        // ------------------------------------------------------------------ D4: Fahrende Plattform (Südosten)

        static MovingPlatform BuildPlatformExperiment(Transform platformGroup, Transform signs)
        {
            // yaw 180: "vorne" (lokal -z) zeigt zum Besucher. So stehen die Buchstaben A und B richtig herum
            // und vom Eingang aus gesehen liegt A links, B rechts (Lesereihenfolge A → B).
            GameObject set = SB.Place(P.MovingPlatformSet, platformGroup, new Vector3(7.0f, 0.1f, -7.5f), 180f, 1f, "MovingPlatformSet");
            MovingPlatform platform = set.GetComponentInChildren<MovingPlatform>(true);
            if (platform == null)
            {
                Debug.LogWarning("[Student Sandbox] MovingPlatformSet: keine MovingPlatform-Komponente gefunden.");
            }
            else
            {
                SB.Note(platform.gameObject, "Fahrende Plattform",
                    "Ich fahre zwischen PointA und PointB hin und her.\n" +
                    "Verschiebe PointA oder PointB, ändere Speed oder Wait Time.\n" +
                    "Im Play Mode starte ich immer bei PointA.");
            }

            // Schild hinter der Plattform (Süden) – so bleibt die Plattform vom Eingang aus frei sichtbar.
            SB.Sign("Sign_Platform", signs, new Vector3(7.0f, 0.1f, -10.6f), 180, "PLATTFORM", "Fährt von A nach B", M.Turquoise, 3.3f);
            return platform;
        }

        // ------------------------------------------------------------------ Kontrollpult (Norden, nahe Eingang)

        static void BuildControlDesk(Transform desk, DoorController door, TrafficLightController traffic, Rotator crystalRotator, MovingPlatform platform)
        {
            // Pult: weißer Körper, schräge Technik-Platte (hohe Kante hinten), türkise Seitenteile, Leuchtkante vorne.
            SB.Box("Desk_Base", desk, new Vector3(0f, FloorTop + 0.45f, DeskZ), new Vector3(9.2f, 0.9f, 1.0f), M.WarmWhite);
            SB.MeshObj(MK.Wedge, "Desk_Slope", desk, new Vector3(0f, FloorTop + 0.9f, DeskZ), new Vector3(1.0f, 0.28f, 9.2f), M.TechPanel,
                new Vector3(0f, -90f, 0f));
            SB.Box("Desk_Side_West", desk, new Vector3(-4.66f, FloorTop + 0.66f, DeskZ), new Vector3(0.12f, 1.32f, 1.08f), M.Turquoise);
            SB.Box("Desk_Side_East", desk, new Vector3(4.66f, FloorTop + 0.66f, DeskZ), new Vector3(0.12f, 1.32f, 1.08f), M.Turquoise);
            GameObject deskTrim = SB.Box("Desk_Trim", desk, new Vector3(0f, FloorTop + 0.9f, DeskZ + 0.52f), new Vector3(9.2f, 0.04f, 0.04f),
                M.NeonTeal, default, false);
            SB.NoShadows(deskTrim);

            // Flacher Bildschirm direkt hinter dem Pult (Oberkante 2.33 m): So bleibt die Ampel dahinter
            // auch vom Weg aus (Augenhöhe) sichtbar.
            const float screenZ = DeskZ - 0.62f;
            const float screenY = 1.88f;
            const float screenWidth = 6.4f;
            const float screenHeight = 0.9f;
            float frameZ = screenZ + 0.08f;
            SB.Box("Screen_Post_West", desk, new Vector3(-2.7f, FloorTop + 0.9f, screenZ - 0.14f), new Vector3(0.16f, 1.8f, 0.16f), M.Charcoal);
            SB.Box("Screen_Post_East", desk, new Vector3(2.7f, FloorTop + 0.9f, screenZ - 0.14f), new Vector3(0.16f, 1.8f, 0.16f), M.Charcoal);
            SB.Box("Screen", desk, new Vector3(0f, screenY, screenZ), new Vector3(screenWidth, screenHeight, 0.12f), M.TechPanel);
            Transform screenFrame = SB.Group("Screen_Frame", desk);
            float halfWidth = screenWidth * 0.5f;
            float halfHeight = screenHeight * 0.5f;
            SB.Box("Frame_Top", screenFrame, new Vector3(0f, screenY + halfHeight + 0.03f, frameZ), new Vector3(screenWidth + 0.06f, 0.06f, 0.04f), M.NeonTeal, default, false);
            SB.Box("Frame_Bottom", screenFrame, new Vector3(0f, screenY - halfHeight - 0.03f, frameZ), new Vector3(screenWidth + 0.06f, 0.06f, 0.04f), M.NeonTeal, default, false);
            SB.Box("Frame_West", screenFrame, new Vector3(-halfWidth, screenY, frameZ), new Vector3(0.06f, screenHeight, 0.04f), M.NeonTeal, default, false);
            SB.Box("Frame_East", screenFrame, new Vector3(halfWidth, screenY, frameZ), new Vector3(0.06f, screenHeight, 0.04f), M.NeonTeal, default, false);
            SB.NoShadows(screenFrame.gameObject);
            SB.Text("Screen_Title", "KONTROLLPULT", desk, new Vector3(0f, screenY + 0.14f, screenZ + 0.07f), new Vector3(0f, 180f, 0f), 0.44f, Color.white);
            SB.Text("Screen_Subtitle", "Ein Knopf – eine Aktion – eine Reaktion", desk, new Vector3(0f, screenY - 0.26f, screenZ + 0.07f),
                new Vector3(0f, 180f, 0f), 0.19f, SB.Hex("#9FF3EE"), FontStyle.Normal);

            // Fünf Knöpfe vor dem Pult (vom Eingang aus gut sichtbar, nicht vom Eingangsbogen verdeckt).
            ClickableSwitch doorButton = DeskButton(desk, "Desk_DoorButton", -3.6f, "Tür", M.ButtonTeal, "TÜR");
            ClickableSwitch trafficButton = DeskButton(desk, "Desk_TrafficButton", -1.8f, "Ampel weiterschalten", M.ButtonRed, "AMPEL");
            ClickableSwitch autoButton = DeskButton(desk, "Desk_TrafficAutoButton", 0f, "Ampel-Automatik an/aus", M.ButtonYellow, "AUTO");
            ClickableSwitch crystalButton = DeskButton(desk, "Desk_CrystalButton", 1.8f, "Kristall drehen an/aus", M.ButtonPurple, "KRISTALL");
            ClickableSwitch platformButton = DeskButton(desk, "Desk_PlatformButton", 3.6f, "Plattform an/aus", M.ButtonBlue, "PLATTFORM");

            if (door != null)
            {
                SB.Listen(doorButton.OnPressed, door.Toggle);
                SB.Note(doorButton.gameObject, "Knopf: Tür",
                    "On Pressed ruft AutomaticDoor → DoorController.Toggle auf.\n" +
                    "Das ist derselbe Befehl wie beim Schalter direkt neben der Tür.");
            }
            if (traffic != null)
            {
                SB.Listen(trafficButton.OnPressed, traffic.NextPhase);
                SB.Note(trafficButton.gameObject, "Knopf: Ampel",
                    "On Pressed ruft TrafficLight → TrafficLightController.NextPhase auf.\n" +
                    "Die Ampel geht eine Phase weiter: Rot → Rot-Gelb → Grün → Gelb → Rot …");
                SB.Listen(autoButton.OnPressed, traffic.ToggleAutoCycle);
                SB.Note(autoButton.gameObject, "Knopf: Ampel-Automatik",
                    "On Pressed ruft TrafficLight → TrafficLightController.ToggleAutoCycle auf.\n" +
                    "Danach schaltet die Ampel von allein. Noch ein Klick – sie wartet wieder auf dich.");
            }
            if (crystalRotator != null)
            {
                SB.Listen(crystalButton.OnPressed, crystalRotator.ToggleRotation);
                SB.Note(crystalButton.gameObject, "Knopf: Kristall",
                    "On Pressed ruft Crystal → Rotator.ToggleRotation auf.\n" +
                    "Der Kristall hört auf, sich zu drehen – oder fängt wieder an.");
            }
            if (platform != null)
            {
                SB.Listen(platformButton.OnPressed, platform.ToggleMoving);
                SB.Note(platformButton.gameObject, "Knopf: Plattform",
                    "On Pressed ruft MovingPlatform → MovingPlatform.ToggleMoving auf.\n" +
                    "Die Plattform bleibt stehen oder fährt weiter.");
            }

            // Kleine farbige Leuchten auf der Schräge – in der Farbe des Knopfs davor.
            DeskLight(desk, "Desk_Light_Door", -3.6f, M.ButtonTeal);
            DeskLight(desk, "Desk_Light_Traffic", -1.8f, M.ButtonRed);
            DeskLight(desk, "Desk_Light_TrafficAuto", 0f, M.ButtonYellow);
            DeskLight(desk, "Desk_Light_Crystal", 1.8f, M.ButtonPurple);
            DeskLight(desk, "Desk_Light_Platform", 3.6f, M.ButtonBlue);

            // Zwei Laternen links und rechts (gehen bei Sonnenuntergang und Nacht automatisch an).
            SB.Place(P.LampPost, desk, new Vector3(-5.25f, 0.1f, DeskLampZ), 180f, 1f, "LampPost_West");
            SB.Place(P.LampPost, desk, new Vector3(5.25f, 0.1f, DeskLampZ), 180f, 1f, "LampPost_East");

            SB.Note(desk.gameObject, "Kontrollpult",
                "Jeder Knopf hat ein Event (On Pressed). Dort steht, welches Objekt reagiert und welche Funktion aufgerufen wird.\n" +
                "Ein Knopf – eine Aktion – eine sichtbare Reaktion.\n" +
                "Wähle einen Knopf aus (z.B. Desk_DoorButton) und schau dir im Inspector die Liste \"On Pressed ()\" an.");
        }

        static ClickableSwitch DeskButton(Transform desk, string name, float x, string hoverLabel, string capMaterial, string caption)
        {
            ClickableSwitch button = SB.PushButton(name, desk, new Vector3(x, 0.1f, DeskButtonZ), 180, hoverLabel, capMaterial, caption, 0.9f);
            FitCaption(button);
            return button;
        }

        /// <summary>Flache Leuchte, die genau auf der Schräge des Pults liegt (Schräge: 0.28 hoch auf 1 m Tiefe).</summary>
        static void DeskLight(Transform desk, string name, float x, string material)
        {
            const float slopeHeight = 0.28f;
            const float z = DeskZ + 0.05f;
            float slopeAngle = Mathf.Atan2(slopeHeight, 1.0f) * Mathf.Rad2Deg;
            float surfaceY = FloorTop + 0.9f + slopeHeight * (0.5f - (z - DeskZ));
            Vector3 normal = new Vector3(0f, Mathf.Cos(slopeAngle * Mathf.Deg2Rad), Mathf.Sin(slopeAngle * Mathf.Deg2Rad));
            Vector3 position = new Vector3(x, surfaceY, z) + normal * 0.004f;
            GameObject led = SB.Box(name, desk, position, new Vector3(0.34f, 0.02f, 0.14f), material, new Vector3(slopeAngle, 0f, 0f), false);
            led.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
        }

        // ------------------------------------------------------------------ Kleine Helfer (nur für diesen Bereich)

        /// <summary>Flache Platte/Markierung ohne Collider und ohne Schatten. top = Oberkante (lokal).</summary>
        static GameObject FlatBox(string name, Transform parent, float x, float z, float sizeX, float sizeZ, float top, float thickness, string material)
        {
            GameObject go = SB.Box(name, parent, new Vector3(x, top - thickness * 0.5f, z), new Vector3(sizeX, thickness, sizeZ), material, default, false);
            go.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
            return go;
        }

        /// <summary>Sucht ein Kind-Objekt mit diesem Namen (auch tiefer verschachtelt).</summary>
        static Transform FindDeep(Transform root, string childName)
        {
            if (root == null) return null;
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t != root && t.name == childName) return t;
            }
            return null;
        }

        /// <summary>
        /// Lange Beschriftungen (z.B. "PLATTFORM") passen nicht auf das Standard-Schildchen des Knopfs:
        /// erst das Schildchen verbreitern (höchstens 0.86 m), dann die Schrift verkleinern.
        /// </summary>
        static void FitCaption(ClickableSwitch button)
        {
            Transform root = button.transform;
            Transform captionTransform = root.Find("Caption");
            Transform plate = root.Find("Caption_Plate");
            if (captionTransform == null || plate == null) return;
            TextMesh caption = captionTransform.GetComponent<TextMesh>();
            if (caption == null || string.IsNullOrEmpty(caption.text)) return;

            const float margin = 0.1f;
            float letterHeight = caption.characterSize * 9.6f; // Umkehrung von SB.Text
            float widthEm = EstimateWidthEm(caption.text);
            float width = widthEm * letterHeight;
            Vector3 plateScale = plate.localScale;
            if (width + margin <= plateScale.x) return;

            plateScale.x = Mathf.Min(MaxCaptionPlateWidth, width + margin);
            plate.localScale = plateScale;
            if (width + margin > plateScale.x)
            {
                float fittedLetterHeight = (plateScale.x - margin) / widthEm;
                caption.characterSize = fittedLetterHeight / 9.6f;
            }
        }

        /// <summary>Grobe Breite eines Textes in "em" (Schrifthöhen) für die fette Standard-Schrift.</summary>
        static float EstimateWidthEm(string text)
        {
            float sum = 0f;
            foreach (char c in text)
            {
                if (c == ' ') sum += 0.28f;
                else if ("IJijl.,:;!|'".IndexOf(c) >= 0) sum += 0.3f;
                else if (c == 'M' || c == 'W') sum += 0.85f;
                else if (char.IsUpper(c) || char.IsDigit(c)) sum += 0.7f;
                else sum += 0.56f;
            }
            return sum;
        }

        /// <summary>Wie tief der Knopf beim Drücken einsinkt (privates Feld von ClickableSwitch).</summary>
        static void SetPressDepth(ClickableSwitch clickable, float depth)
        {
            var serialized = new SerializedObject(clickable);
            SerializedProperty property = serialized.FindProperty("pressDepth");
            if (property == null) return;
            property.floatValue = depth;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
