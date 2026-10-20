using UnityEngine;
using UnityEngine.Rendering;

namespace StudentSandbox.EditorTools
{
    /// <summary>
    /// Prefabs für den Baubereich (CREATIVE): Bäume, Büsche, Haus, Bank, Laterne, Zaun, Blume, Stein,
    /// bunte Grundkörper, Plattform und Säule.
    /// Regeln: Pivot unten in der Mitte (y = 0 = Boden), "vorne" = lokal -z, Skalierung der Wurzel = 1.
    /// Diese Datei enthält außerdem kleine gemeinsame Helfer, die auch die anderen Prefab-Dateien benutzen.
    /// </summary>
    public static partial class SandboxPrefabs
    {
        static partial void BuildCreativePrefabs()
        {
            BuildSmallTree();
            BuildLargeTree();
            BuildPineTree();
            BuildBush();
            BuildSmallHouse();
            BuildBench();
            BuildLampPost();
            BuildFence();
            BuildFlower();
            BuildRock();
            BuildColoredCube();
            BuildColoredSphere();
            BuildColoredCylinder();
            BuildPlatform();
            BuildDecorativePillar();
        }

        // ================================================================== Bäume und Büsche

        static void BuildSmallTree()
        {
            GameObject root = NewRoot(P.SmallTree);
            Transform t = root.transform;

            SB.MeshObj(MK.Cylinder6, "Trunk", t, Vector3.zero, new Vector3(0.32f, 1.5f, 0.32f), M.Trunk);
            SB.MeshObj(MK.BlobA, "Crown", t, new Vector3(0f, 2.0f, 0f), new Vector3(1.8f, 1.6f, 1.8f), M.Leaves, new Vector3(0f, 20f, 0f));
            SB.MeshObj(MK.BlobB, "Crown_Top", t, new Vector3(0.15f, 2.68f, -0.05f), new Vector3(1.1f, 1.0f, 1.1f), M.LeavesLight, new Vector3(0f, 65f, 0f));

            AddCapsuleCollider(root, new Vector3(0f, 1.6f, 0f), 0.55f, 3.2f);
            SB.Note(root, "Kleiner Baum",
                "Ein kleiner Laubbaum aus einfachen Formen: Stamm (Trunk) und Krone (Crown, Crown_Top). " +
                "Probier: Mach ihn mit Scale größer oder zieh ein anderes Material auf die Krone.");
            Save(root, P.SmallTree);
        }

        static void BuildLargeTree()
        {
            GameObject root = NewRoot(P.LargeTree);
            Transform t = root.transform;

            SB.MeshObj(MK.Cylinder6, "Trunk", t, Vector3.zero, new Vector3(0.5f, 2.6f, 0.5f), M.Trunk);
            SB.MeshObj(MK.BlobA, "Crown", t, new Vector3(0f, 3.4f, 0f), new Vector3(3.0f, 2.4f, 3.0f), M.Leaves, new Vector3(0f, 10f, 0f));
            SB.MeshObj(MK.BlobC, "Crown_2", t, new Vector3(0.45f, 4.55f, -0.2f), new Vector3(2.0f, 1.7f, 2.0f), M.LeavesLight, new Vector3(0f, 50f, 0f));
            SB.MeshObj(MK.BlobB, "Crown_3", t, new Vector3(-0.75f, 3.95f, 0.5f), new Vector3(1.8f, 1.5f, 1.8f), M.Leaves, new Vector3(0f, -35f, 0f));

            AddCapsuleCollider(root, new Vector3(0f, 2.75f, 0f), 0.8f, 5.5f);
            SB.Note(root, "Großer Baum",
                "Ein großer Baum mit drei Kronen-Teilen (Crown, Crown_2, Crown_3). " +
                "Probier: Färbe die Krone herbstlich (Material Env_LeavesAutumn) oder drehe den Baum mit Rotation Y.");
            Save(root, P.LargeTree);
        }

        static void BuildPineTree()
        {
            GameObject root = NewRoot(P.PineTree);
            Transform t = root.transform;

            SB.MeshObj(MK.Cylinder6, "Trunk", t, Vector3.zero, new Vector3(0.36f, 1.0f, 0.36f), M.Trunk);
            SB.MeshObj(MK.Cone8, "Layer_1", t, new Vector3(0f, 0.7f, 0f), new Vector3(2.2f, 1.6f, 2.2f), M.LeavesPine);
            SB.MeshObj(MK.Cone8, "Layer_2", t, new Vector3(0f, 1.7f, 0f), new Vector3(1.7f, 1.5f, 1.7f), M.LeavesPine, new Vector3(0f, 22.5f, 0f));
            SB.MeshObj(MK.Cone8, "Layer_3", t, new Vector3(0f, 2.8f, 0f), new Vector3(1.15f, 1.4f, 1.15f), M.LeavesPine);

            AddCapsuleCollider(root, new Vector3(0f, 2.1f, 0f), 0.65f, 4.2f);
            SB.Note(root, "Tanne",
                "Eine Tanne aus drei Kegeln (Layer_1 bis Layer_3). " +
                "Probier: Mach sie mit Scale Y schlanker und höher oder verschiebe den obersten Kegel.");
            Save(root, P.PineTree);
        }

        static void BuildBush()
        {
            GameObject root = NewRoot(P.Bush);
            Transform t = root.transform;

            SB.MeshObj(MK.BlobA, "Leaves", t, new Vector3(0f, 0.42f, 0f), new Vector3(1.4f, 0.95f, 1.25f), M.Leaves, new Vector3(0f, 15f, 0f));
            SB.MeshObj(MK.BlobC, "Leaves_2", t, new Vector3(0.3f, 0.68f, 0.12f), new Vector3(0.85f, 0.7f, 0.8f), M.LeavesLight, new Vector3(0f, -40f, 0f));

            // Liegende Kapsel (entlang x) – passt gut zur breiten, flachen Form.
            AddCapsuleCollider(root, new Vector3(0f, 0.45f, 0f), 0.45f, 1.4f, 0);
            SB.Note(root, "Busch",
                "Ein runder Busch aus zwei Blätter-Formen (Leaves, Leaves_2). " +
                "Probier: Mach ihn mit Scale breiter oder stelle mehrere Büsche zu einer Hecke zusammen.");
            Save(root, P.Bush);
        }

        // ================================================================== Haus

        static void BuildSmallHouse()
        {
            GameObject root = NewRoot(P.SmallHouse);
            Transform t = root.transform;

            // Maße (Meter). Wände: x ±1.45, z ±1.37, y 0.18 … 2.33. Tür vorne (-z).
            const float wallHalfX = 1.45f;
            const float wallHalfZ = 1.37f;
            const float floorY = 0.18f;
            const float wallTop = 2.33f;
            const float front = -wallHalfZ;

            SB.Box("Foundation", t, new Vector3(0f, floorY * 0.5f, 0f), new Vector3(3.1f, floorY, 2.94f), M.StoneGray, default, false);
            SB.Box("Step", t, new Vector3(0f, 0.05f, -1.63f), new Vector3(1.2f, 0.1f, 0.32f), M.StoneGray, default, false);
            SB.Box("Walls", t, new Vector3(0f, (floorY + wallTop) * 0.5f, 0f), new Vector3(wallHalfX * 2f, wallTop - floorY, wallHalfZ * 2f), M.Wall, default, false);

            // Satteldach: First entlang z, Giebel vorne und hinten.
            // Das Dach (rot) ist etwas breiter als die Wände. Ein wandfarbiges Giebel-Dreieck mit gleicher Neigung
            // liegt darunter und schaut vorne/hinten 2 cm heraus – so sieht man eine helle Giebelwand mit rotem Dachrand.
            const float roofBaseY = 2.28f;
            const float roofHeight = 1.42f;
            const float roofHalfWidth = 1.7f;
            float slope = roofHeight / roofHalfWidth;
            SB.MeshObj(MK.Prism, "Roof", t, new Vector3(0f, roofBaseY, 0f), new Vector3(roofHalfWidth * 2f, roofHeight, wallHalfZ * 2f + 0.04f), M.RoofRed);
            SB.MeshObj(MK.Prism, "Gable", t, new Vector3(0f, wallTop, 0f), new Vector3(wallHalfX * 2f, wallHalfX * slope, wallHalfZ * 2f + 0.08f), M.Wall);

            // Schornstein: steht hinten rechts im Dach.
            SB.Box("Chimney", t, new Vector3(0.75f, 3.235f, 0.55f), new Vector3(0.36f, 0.77f, 0.36f), M.StoneGray, default, false);
            SB.Box("Chimney_Cap", t, new Vector3(0.75f, 3.66f, 0.55f), new Vector3(0.46f, 0.08f, 0.46f), M.Charcoal, default, false);

            // Tür mit Rahmen und Türknauf.
            SB.Box("Door_Frame", t, new Vector3(0f, floorY + 0.985f, front - 0.015f), new Vector3(1.1f, 1.97f, 0.04f), M.WarmWhite, default, false);
            SB.Box("Door", t, new Vector3(0f, floorY + 0.94f, front - 0.04f), new Vector3(0.92f, 1.88f, 0.05f), M.DarkWood, default, false);
            SB.MeshObj(MK.Sphere, "Door_Knob", t, new Vector3(0.3f, 1.1f, front - 0.08f), Vector3.one * 0.09f, M.SoftYellow);

            // Fenster (die Glas-Teile "Window_*" leuchten abends).
            var windows = new Renderer[4];
            windows[0] = BuildFrontWindow(t, "Front_L", -1.0f, front);
            windows[1] = BuildFrontWindow(t, "Front_R", 1.0f, front);
            windows[2] = BuildSideWindow(t, "Side", wallHalfX);

            // Rundes Fenster im Giebel (Zylinder um 90° gekippt, zeigt nach -z).
            float gableFront = -(wallHalfZ + 0.04f);
            SB.MeshObj(MK.Cylinder12, "Frame_Gable", t, new Vector3(0f, 2.88f, gableFront + 0.005f), new Vector3(0.56f, 0.03f, 0.56f), M.WarmWhite, new Vector3(-90f, 0f, 0f));
            GameObject gableGlass = SB.MeshObj(MK.Cylinder12, "Window_Gable", t, new Vector3(0f, 2.88f, gableFront - 0.015f), new Vector3(0.42f, 0.03f, 0.42f), M.WindowOff, new Vector3(-90f, 0f, 0f));
            windows[3] = RendererOf(gableGlass);

            AddBoxCollider(root, new Vector3(0f, 1.85f, 0f), new Vector3(3.1f, 3.7f, 2.94f));

            LampController lights = root.AddComponent<LampController>();
            lights.Setup(null, windows, SB.Mat(M.WindowOn), SB.Mat(M.WindowOff), false);
            lights.FollowDayNight = true;

            SB.Note(root, "Kleines Haus",
                "Ein Haus aus Wänden (Walls), Dach (Roof), Tür (Door) und Fenstern. " +
                "Abends und nachts leuchten die Fenster von selbst (Lamp Controller). " +
                "Probier: Zieh ein anderes Material auf Walls oder Roof.");
            Save(root, P.SmallHouse);
        }

        /// <summary>Fenster in der Vorderwand (-z): Rahmen, Glas, Fensterkreuz und Fensterbank. Gibt das Glas zurück.</summary>
        static Renderer BuildFrontWindow(Transform parent, string suffix, float x, float wallFront)
        {
            const float y = 1.4f;
            SB.Box("Frame_" + suffix, parent, new Vector3(x, y, wallFront - 0.015f), new Vector3(0.68f, 0.78f, 0.04f), M.WarmWhite, default, false);
            GameObject glass = SB.Box("Window_" + suffix, parent, new Vector3(x, y, wallFront - 0.04f), new Vector3(0.56f, 0.66f, 0.03f), M.WindowOff, default, false);
            SB.Box("Mullion_" + suffix, parent, new Vector3(x, y, wallFront - 0.065f), new Vector3(0.05f, 0.66f, 0.02f), M.WarmWhite, default, false);
            SB.Box("Transom_" + suffix, parent, new Vector3(x, y, wallFront - 0.067f), new Vector3(0.56f, 0.05f, 0.02f), M.WarmWhite, default, false);
            SB.Box("Sill_" + suffix, parent, new Vector3(x, y - 0.42f, wallFront - 0.05f), new Vector3(0.8f, 0.06f, 0.14f), M.WarmWhite, default, false);
            return RendererOf(glass);
        }

        /// <summary>Fenster in der rechten Seitenwand (+x). Gibt das Glas zurück.</summary>
        static Renderer BuildSideWindow(Transform parent, string suffix, float wallSide)
        {
            const float y = 1.4f;
            SB.Box("Frame_" + suffix, parent, new Vector3(wallSide + 0.015f, y, 0f), new Vector3(0.04f, 0.78f, 0.68f), M.WarmWhite, default, false);
            GameObject glass = SB.Box("Window_" + suffix, parent, new Vector3(wallSide + 0.04f, y, 0f), new Vector3(0.03f, 0.66f, 0.56f), M.WindowOff, default, false);
            SB.Box("Mullion_" + suffix, parent, new Vector3(wallSide + 0.065f, y, 0f), new Vector3(0.02f, 0.66f, 0.05f), M.WarmWhite, default, false);
            SB.Box("Transom_" + suffix, parent, new Vector3(wallSide + 0.067f, y, 0f), new Vector3(0.02f, 0.05f, 0.56f), M.WarmWhite, default, false);
            SB.Box("Sill_" + suffix, parent, new Vector3(wallSide + 0.05f, y - 0.42f, 0f), new Vector3(0.14f, 0.06f, 0.8f), M.WarmWhite, default, false);
            return RendererOf(glass);
        }

        // ================================================================== Bank, Laterne, Zaun

        static void BuildBench()
        {
            GameObject root = NewRoot(P.Bench);
            Transform t = root.transform;

            // Sitzfläche (Oberkante 0.45 m) und Lehne hinten (+z) – wer sitzt, schaut nach -z.
            SB.Box("Seat", t, new Vector3(0f, 0.415f, -0.02f), new Vector3(2.0f, 0.07f, 0.5f), M.LightWood, default, false);
            // Zwei dunkle Fugen lassen die Sitzfläche wie einzelne Latten aussehen (knapp über der Oberfläche).
            SB.Box("Seat_Groove_1", t, new Vector3(0f, 0.452f, -0.105f), new Vector3(1.96f, 0.01f, 0.025f), M.DarkWood, default, false);
            SB.Box("Seat_Groove_2", t, new Vector3(0f, 0.452f, 0.065f), new Vector3(1.96f, 0.01f, 0.025f), M.DarkWood, default, false);

            SB.Box("Backrest", t, new Vector3(0f, 0.72f, 0.25f), new Vector3(2.0f, 0.32f, 0.06f), M.LightWood, new Vector3(10f, 0f, 0f), false);
            // Fuge auf der Vorderseite der (gekippten) Lehne.
            Quaternion tilt = Quaternion.Euler(10f, 0f, 0f);
            Vector3 groovePos = new Vector3(0f, 0.72f, 0.25f) + tilt * new Vector3(0f, 0f, -0.033f);
            SB.Box("Backrest_Groove", t, groovePos, new Vector3(1.96f, 0.025f, 0.01f), M.DarkWood, new Vector3(10f, 0f, 0f), false);

            foreach (float side in new[] { -1f, 1f })
            {
                string sideName = side < 0f ? "L" : "R";
                float x = side * 0.85f;
                SB.Box("Leg_Front_" + sideName, t, new Vector3(x, 0.19f, -0.2f), new Vector3(0.07f, 0.38f, 0.07f), M.Charcoal, default, false);
                SB.Box("Leg_Back_" + sideName, t, new Vector3(x, 0.43f, 0.31f), new Vector3(0.07f, 0.86f, 0.07f), M.Charcoal, default, false);
                SB.Box("Leg_Rail_" + sideName, t, new Vector3(x, 0.35f, 0.05f), new Vector3(0.06f, 0.06f, 0.55f), M.Charcoal, default, false);
            }

            AddBoxCollider(root, new Vector3(0f, 0.45f, 0.02f), new Vector3(2.0f, 0.9f, 0.6f));
            SB.Note(root, "Bank",
                "Eine Parkbank mit Sitzfläche (Seat), Lehne (Backrest) und Metall-Beinen. " +
                "Probier: Drehe sie mit Rotation Y, damit man von der Bank einen schönen Ausblick hat.");
            Save(root, P.Bench);
        }

        static void BuildLampPost()
        {
            GameObject root = NewRoot(P.LampPost);
            Transform t = root.transform;

            SB.MeshObj(MK.Cylinder8, "Base", t, Vector3.zero, new Vector3(0.5f, 0.3f, 0.5f), M.Charcoal);
            SB.MeshObj(MK.Cylinder8, "Pole_Collar", t, new Vector3(0f, 0.3f, 0f), new Vector3(0.24f, 0.12f, 0.24f), M.Charcoal);
            SB.MeshObj(MK.Cylinder8, "Pole", t, new Vector3(0f, 0.3f, 0f), new Vector3(0.14f, 2.85f, 0.14f), M.Charcoal);

            // Laternen-Kopf: alles, was das Licht umgibt, wirft keinen Schatten.
            const float bulbY = 3.4f;
            var head = new[]
            {
                SB.MeshObj(MK.Cylinder8, "Lantern_Base", t, new Vector3(0f, 3.12f, 0f), new Vector3(0.42f, 0.08f, 0.42f), M.Charcoal),
                SB.MeshObj(MK.Cylinder8, "Lantern_Top", t, new Vector3(0f, 3.58f, 0f), new Vector3(0.44f, 0.04f, 0.44f), M.Charcoal),
                SB.MeshObj(MK.Pyramid, "Roof_Cap", t, new Vector3(0f, 3.62f, 0f), new Vector3(0.5f, 0.18f, 0.5f), M.Charcoal),
                SB.Box("Lantern_Post_1", t, new Vector3(-0.16f, 3.39f, -0.16f), new Vector3(0.035f, 0.38f, 0.035f), M.Charcoal, default, false),
                SB.Box("Lantern_Post_2", t, new Vector3(0.16f, 3.39f, -0.16f), new Vector3(0.035f, 0.38f, 0.035f), M.Charcoal, default, false),
                SB.Box("Lantern_Post_3", t, new Vector3(0.16f, 3.39f, 0.16f), new Vector3(0.035f, 0.38f, 0.035f), M.Charcoal, default, false),
                SB.Box("Lantern_Post_4", t, new Vector3(-0.16f, 3.39f, 0.16f), new Vector3(0.035f, 0.38f, 0.035f), M.Charcoal, default, false),
            };
            foreach (GameObject part in head) CastNoShadow(part);

            GameObject bulb = SB.MeshObj(MK.Sphere, "Bulb", t, new Vector3(0f, bulbY, 0f), Vector3.one * 0.3f, M.LampOff);
            CastNoShadow(bulb);

            Light lampLight = SB.PointLight("LampLight", t, new Vector3(0f, bulbY, 0f), SB.Hex("#FFD18A"), 4f, 10f, LightShadows.None);
            lampLight.enabled = false;

            LampController controller = root.AddComponent<LampController>();
            controller.Setup(lampLight, new[] { RendererOf(bulb) }, SB.Mat(M.LampOn), SB.Mat(M.LampOff), false);
            controller.FollowDayNight = true;

            AddCapsuleCollider(root, new Vector3(0f, 1.9f, 0f), 0.2f, 3.8f);
            SB.Note(root, "Straßenlaterne",
                "Eine Laterne mit einem Point Light (Kind-Objekt LampLight). Nachts geht sie von selbst an (Lamp Controller → Follow Day Night). " +
                "Probier: Ändere beim LampLight die Farbe (Color) oder die Reichweite (Range).");
            Save(root, P.LampPost);
        }

        static void BuildFence()
        {
            GameObject root = NewRoot(P.Fence);
            Transform t = root.transform;

            // Der Zaun läuft entlang der lokalen x-Achse (3 m). Latten vorne (-z), Querbalken dahinter.
            foreach (float side in new[] { -1f, 1f })
            {
                string sideName = side < 0f ? "L" : "R";
                float x = side * 1.42f;
                SB.Box("Post_" + sideName, t, new Vector3(x, 0.475f, 0.02f), new Vector3(0.14f, 0.95f, 0.14f), M.WarmWhite, default, false);
                SB.MeshObj(MK.Pyramid, "Post_Cap_" + sideName, t, new Vector3(x, 0.95f, 0.02f), new Vector3(0.18f, 0.08f, 0.18f), M.WarmWhite);
            }

            SB.Box("Rail_Top", t, new Vector3(0f, 0.7f, 0.045f), new Vector3(2.84f, 0.08f, 0.05f), M.WarmWhite, default, false);
            SB.Box("Rail_Bottom", t, new Vector3(0f, 0.3f, 0.045f), new Vector3(2.84f, 0.08f, 0.05f), M.WarmWhite, default, false);

            for (int i = 0; i < 6; i++)
            {
                float x = -1.0f + i * 0.4f;
                SB.Box("Picket_" + (i + 1), t, new Vector3(x, 0.45f, 0f), new Vector3(0.14f, 0.8f, 0.04f), M.WarmWhite, default, false);
                SB.MeshObj(MK.Pyramid, "Picket_Tip_" + (i + 1), t, new Vector3(x, 0.85f, 0f), new Vector3(0.14f, 0.12f, 0.04f), M.WarmWhite);
            }

            AddBoxCollider(root, new Vector3(0f, 0.5f, 0.02f), new Vector3(3.0f, 1.0f, 0.15f));
            SB.Note(root, "Zaun",
                "Ein Zaunstück, 3 Meter lang. " +
                "Probier: Kopiere es mit Strg+D und schiebe die Kopie mit dem Move-Werkzeug (W) daneben – so entsteht ein langer Zaun.");
            Save(root, P.Fence);
        }

        // ================================================================== Blume und Stein

        static void BuildFlower()
        {
            GameObject root = NewRoot(P.Flower);
            Transform t = root.transform;

            AddFlowerStem(t, 1, new Vector3(-0.13f, 0f, -0.04f), new Vector3(-6f, 0f, 8f), 0.4f, M.PinkBlossom, 0.18f);
            AddFlowerStem(t, 2, new Vector3(0.12f, 0f, 0.08f), new Vector3(7f, 0f, -9f), 0.34f, M.SoftYellow, 0.16f);
            AddFlowerStem(t, 3, new Vector3(0.02f, 0f, -0.14f), new Vector3(-10f, 0f, -3f), 0.27f, M.Lavender, 0.15f);
            SB.MeshObj(MK.BlobB, "Leaf_1", t, new Vector3(-0.09f, 0.035f, 0.06f), new Vector3(0.24f, 0.06f, 0.12f), M.LeafGreen, new Vector3(0f, 35f, 0f));
            SB.MeshObj(MK.BlobC, "Leaf_2", t, new Vector3(0.1f, 0.035f, -0.07f), new Vector3(0.22f, 0.06f, 0.11f), M.LeafGreen, new Vector3(0f, -40f, 0f));

            AddBoxCollider(root, new Vector3(0f, 0.25f, 0f), new Vector3(0.45f, 0.5f, 0.45f));
            SB.Note(root, "Blume",
                "Drei kleine Blüten (Blossom_1 bis Blossom_3) auf dünnen Stielen. " +
                "Probier: Zieh ein Material aus Materials/Palette auf eine Blüte, um sie umzufärben.");
            Save(root, P.Flower);
        }

        /// <summary>Ein dünner, leicht geneigter Stiel mit einer Blüte an der Spitze (ohne Collider).</summary>
        static void AddFlowerStem(Transform parent, int index, Vector3 basePosition, Vector3 tilt, float length, string blossomMaterial, float blossomSize)
        {
            Vector3 up = Quaternion.Euler(tilt) * Vector3.up;
            // Der Unity-Zylinder ist bei Scale 1 zwei Meter hoch – daher Scale Y = halbe Länge.
            SB.Prim(PrimitiveType.Cylinder, "Stem_" + index, parent, basePosition + up * (length * 0.5f), new Vector3(0.028f, length * 0.5f, 0.028f), M.LeafGreen, tilt, false);
            SB.MeshObj(MK.Sphere, "Blossom_" + index, parent, basePosition + up * length, new Vector3(blossomSize, blossomSize * 0.75f, blossomSize), blossomMaterial, tilt);
        }

        static void BuildRock()
        {
            GameObject root = NewRoot(P.Rock);

            // Das Stein-Mesh ist unregelmäßig – wir messen es aus und setzen es genau auf den Boden (2 cm eingesunken).
            Mesh mesh = SB.Mesh(MK.Rock);
            Bounds bounds = mesh.bounds;
            var scale = new Vector3(1.2f / bounds.size.x, 0.8f / bounds.size.y, 1.0f / bounds.size.z);
            var position = new Vector3(-bounds.center.x * scale.x, -bounds.min.y * scale.y - 0.02f, -bounds.center.z * scale.z);
            SB.MeshObj(MK.Rock, "Stone", root.transform, position, scale, M.Rock, default, ColliderMode.ConvexMesh);

            SB.Note(root, "Stein",
                "Ein kantiger Stein mit Mesh Collider (der Collider hat genau die Form des Steins). " +
                "Probier: Ändere Scale X und Z für einen flachen Felsen oder drehe ihn mit Rotation Y.");
            Save(root, P.Rock);
        }

        // ================================================================== Bunte Grundkörper

        static void BuildColoredCube()
        {
            GameObject root = NewRoot(P.ColoredCube);
            SB.Prim(PrimitiveType.Cube, "Cube", root.transform, new Vector3(0f, 0.5f, 0f), Vector3.one, M.CoralRed);
            SB.Note(root, "Bunter Würfel",
                "Ein Würfel zum Bauen (1 × 1 × 1 Meter). Probier: Ändere Position, Rotation und Scale im Transform " +
                "oder zieh ein Material aus Materials/Palette darauf. Extra-Aufgabe: Add Component → Rigidbody – dann fällt er im Play Mode!");
            Save(root, P.ColoredCube);
        }

        static void BuildColoredSphere()
        {
            GameObject root = NewRoot(P.ColoredSphere);
            SB.Prim(PrimitiveType.Sphere, "Sphere", root.transform, new Vector3(0f, 0.5f, 0f), Vector3.one, M.SkyBlue);
            SB.Note(root, "Bunte Kugel",
                "Eine Kugel zum Bauen (1 Meter dick). Probier: Mach sie mit Scale zu einem Ei (z.B. Y = 1.5) " +
                "oder gib ihr eine neue Farbe. Extra-Aufgabe: Add Component → Rigidbody – dann rollt sie im Play Mode!");
            Save(root, P.ColoredSphere);
        }

        static void BuildColoredCylinder()
        {
            GameObject root = NewRoot(P.ColoredCylinder);
            // Unity-Zylinder: bei Scale 1 zwei Meter hoch → Scale Y 0.5 = 1 Meter.
            GameObject cylinder = SB.Prim(PrimitiveType.Cylinder, "Cylinder", root.transform, new Vector3(0f, 0.5f, 0f), new Vector3(1f, 0.5f, 1f), M.SoftYellow, default, false);
            // Statt der Kapsel (runde Enden, kippt um) ein Collider mit der echten Zylinder-Form.
            AddConvexMeshCollider(cylinder);
            SB.Note(root, "Bunter Zylinder",
                "Ein Zylinder zum Bauen (1 Meter hoch). Probier: Mach ihn mit Scale zu einer Säule oder zu einer flachen Scheibe. " +
                "Extra-Aufgabe: Add Component → Rigidbody und Play drücken.");
            Save(root, P.ColoredCylinder);
        }

        // ================================================================== Plattform und Säule

        static void BuildPlatform()
        {
            GameObject root = NewRoot(P.Platform);
            Transform t = root.transform;

            SB.Box("Top", t, new Vector3(0f, 0.15f, 0f), new Vector3(3f, 0.3f, 3f), M.WarmWhite);
            // Farbiges Band rundherum (ragt 3 cm heraus, liegt unter der Oberfläche).
            SB.Box("Trim", t, new Vector3(0f, 0.15f, 0f), new Vector3(3.06f, 0.1f, 3.06f), M.MintGreen, default, false);

            SB.SetPhysicsMaterial(root, PM.Normal);
            SB.Note(root, "Plattform",
                "Eine flache Bühne (3 × 3 Meter, 0.3 Meter hoch). " +
                "Probier: Stell andere Objekte darauf oder staple mehrere Plattformen zu einer Treppe.");
            Save(root, P.Platform);
        }

        static void BuildDecorativePillar()
        {
            GameObject root = NewRoot(P.DecorativePillar);
            Transform t = root.transform;

            SB.MeshObj(MK.Cylinder8, "Base", t, Vector3.zero, new Vector3(0.9f, 0.3f, 0.9f), M.StoneGray, default, ColliderMode.ConvexMesh);
            SB.MeshObj(MK.Cylinder8, "Column", t, new Vector3(0f, 0.3f, 0f), new Vector3(0.55f, 2.2f, 0.55f), M.WarmWhite, default, ColliderMode.ConvexMesh);
            SB.MeshObj(MK.Cylinder8, "Column_Band", t, new Vector3(0f, 0.62f, 0f), new Vector3(0.6f, 0.1f, 0.6f), M.MintGreen);
            SB.MeshObj(MK.Cylinder8, "Capital", t, new Vector3(0f, 2.5f, 0f), new Vector3(0.8f, 0.22f, 0.8f), M.StoneGray, default, ColliderMode.ConvexMesh);
            SB.MeshObj(MK.Sphere, "Orb", t, new Vector3(0f, 2.96f, 0f), Vector3.one * 0.48f, M.MintGreen);

            SB.Note(root, "Deko-Säule",
                "Eine Säule mit Sockel (Base), Schaft (Column), Kopf (Capital) und Kugel (Orb). " +
                "Probier: Zieh ein anderes Material auf den Orb oder mach die Säule mit Scale Y höher.");
            Save(root, P.DecorativePillar);
        }

        // ================================================================== Gemeinsame Helfer (auch für die anderen Prefab-Dateien)

        static Renderer RendererOf(GameObject go) => go.GetComponent<Renderer>();

        /// <summary>Kein Schattenwurf – für leuchtende Teile, Licht-Gehäuse, Wolken und flache Markierungen.</summary>
        static void CastNoShadow(GameObject go)
        {
            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null) renderer.shadowCastingMode = ShadowCastingMode.Off;
        }

        static BoxCollider AddBoxCollider(GameObject go, Vector3 center, Vector3 size)
        {
            BoxCollider box = go.AddComponent<BoxCollider>();
            box.center = center;
            box.size = size;
            return box;
        }

        /// <summary>Kapsel-Collider. direction: 0 = x, 1 = y (Standard), 2 = z.</summary>
        static CapsuleCollider AddCapsuleCollider(GameObject go, Vector3 center, float radius, float height, int direction = 1)
        {
            CapsuleCollider capsule = go.AddComponent<CapsuleCollider>();
            capsule.center = center;
            capsule.radius = radius;
            capsule.height = height;
            capsule.direction = direction;
            return capsule;
        }

        /// <summary>Entfernt vorhandene Collider und gibt dem Objekt einen konvexen Mesh Collider mit seiner eigenen Form.</summary>
        static MeshCollider AddConvexMeshCollider(GameObject go)
        {
            foreach (Collider existing in go.GetComponents<Collider>()) Object.DestroyImmediate(existing);
            MeshCollider meshCollider = go.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = go.GetComponent<MeshFilter>().sharedMesh;
            meshCollider.convex = true;
            return meshCollider;
        }
    }
}
