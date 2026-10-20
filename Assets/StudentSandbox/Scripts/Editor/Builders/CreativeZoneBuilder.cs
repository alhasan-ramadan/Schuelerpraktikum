using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StudentSandbox.EditorTools
{
    /// <summary>
    /// Bereich 03 CREATIVE – "Park &amp; Bauplatz".
    /// Hier verschiebt, dreht, skaliert und dupliziert die Schülerin Objekte und zieht Prefabs und Materialien in die Szene.
    /// Deshalb: wenige, gut getrennte, leicht anklickbare Objekte, viel freier Platz, fast alles als Prefab-Instanz.
    ///
    /// Grundriss (lokal, +z = Norden, Eingang im Osten bei x = +12, z = 0):
    ///
    ///   z = +12  Zaun · Großer Baum · kleiner Baum · Büsche
    ///            Teich (-5.5, 5) mit Bänken     Säule (5.5, 6)   Willkommens-Schild
    ///   z =   0  Kiefer · Treffpunkt (1.5, 0) ======== Hauptweg ======== Eingangsbogen (x = 11.3)
    ///            Rotes Haus (-8, -3)            Bauplatz x -0.3 … 8.7, z -10.8 … -3.4
    ///            Blaues Haus (-9, -7.7)         (Holzpodest mit Bausteinen, Beispiel-Turm)   Bauplatz-Schild
    ///   z = -12  Farbkasten (x -9 … -1)
    ///
    /// Höhen (keine gleich hohen Flächen übereinander → kein Flimmern):
    ///   Rasen 0.100 · Wege 0.110 · Sand 0.115 · Treffpunkt 0.115 / 0.121 / 0.127 / Stern 0.133 · Trittsteine 0.128
    ///   Teich-Rand 0.300 · Wasser 0.305 · tiefes Wasser 0.311 · Seerosenblätter 0.325
    /// </summary>
    public static class CreativeZoneBuilder
    {
        // ------------------------------------------------------------------ Maße

        const float FloorTop = SandboxContext.ZoneFloorTop;   // 0.100
        const float PathTop = 0.11f;
        const float PathThickness = 0.025f;
        const float SandTop = 0.115f;

        // Bauplatz (Sandfläche).
        const float BuildMinX = -0.3f;
        const float BuildMaxX = 8.7f;
        const float BuildMinZ = -10.8f;
        const float BuildMaxZ = -3.4f;
        // Lücke in der Bretter-Umrandung, durch die der Weg auf den Bauplatz führt.
        const float BuildGateMinX = 0.45f;
        const float BuildGateMaxX = 2.55f;

        // Holzpodest mit den Bausteinen (Mitte, Größe, Oberkante).
        const float PalletCenterX = 1.3f;
        const float PalletCenterZ = -7.55f;
        const float PalletWidth = 2.6f;
        const float PalletLength = 5.2f;
        const float PalletTop = 0.265f;

        // Farbkasten (Regal am Südrand).
        const float PaletteCenterX = -4.95f;
        const float PaletteZ = -11.1f;
        const float SwatchSpacing = 0.74f;
        const float ShelfTop = 0.57f;

        /// <summary>Die Palette-Farben in genau dieser Reihenfolge (die Kugeln heißen wie die Materialien).</summary>
        static readonly string[] PaletteOrder =
        {
            M.CoralRed, M.SoftYellow, M.MintGreen, M.SkyBlue, M.Lavender, M.WarmOrange,
            M.LightWood, M.DarkWood, M.GrassGreen, M.StoneGray, M.WarmWhite,
        };

        static readonly Color TextColor = SandboxMaterials.Hex("#2E3440");

        /// <summary>Alle hier gesetzten Prefab-Instanzen – ihre Änderungen werden am Ende als Overrides gespeichert.</summary>
        static readonly List<GameObject> PlacedInstances = new List<GameObject>();

        // ------------------------------------------------------------------ Einstieg

        public static void Build(SandboxContext ctx)
        {
            PlacedInstances.Clear();
            Transform zone = ctx.CreativeZone;

            Transform environment = SB.Group("StaticEnvironment", zone);
            Transform editable = SB.Group("EditableObjects", zone);
            Transform buildArea = SB.Group("BuildArea", zone);
            Transform palette = SB.Group("ColorPalette", zone);
            Transform signs = SB.Group("Signs", zone);

            BuildGround(environment);
            BuildPaths(environment);
            BuildPond(environment);
            BuildEditableObjects(editable);
            BuildBuildArea(buildArea);
            BuildColorPalette(palette);
            BuildSigns(signs);

            GameObject arch = SB.EntranceArch("EntranceArch", zone, new Vector3(11.3f, FloorTop, 0f), -90f,
                "CREATIVE", "Bauen & Gestalten", M.MintGreen);

            SB.Note(environment.gameObject, "Hintergrund",
                "Boden, Rand, Wege und Teich bilden die Bühne für deinen Park. " +
                "Sie sind als „Static“ markiert, weil sie sich nie bewegen. Deine eigenen Objekte findest du in EditableObjects.");

            // Nur der Hintergrund ist statisch – alles, was die Schülerin bewegen soll, bleibt beweglich.
            SB.MarkStatic(environment.gameObject);
            SB.MarkStatic(arch);
            SB.MarkStatic(signs.gameObject);
            SB.MarkStatic(palette.gameObject);

            SB.Note(zone.gameObject, "CREATIVE – Bauen & Gestalten",
                "Willkommen im Park mit Bauplatz! Hier gestaltest du deine eigene Welt.\n" +
                "Aufgabe A1: Setz neue Bäume in den Park, verschieb ein Haus und mach einen Baum größer. " +
                "Dreh eine Bank und dupliziere sie mit Strg+D. Gib einem Objekt eine neue Farbe und bau eigene Deko.\n" +
                "Aufgabe A2: Bau auf dem Bauplatz einen eigenen, ungewöhnlichen Turm.\n" +
                "Alles im Ordner EditableObjects und auf dem Bauplatz darfst du verändern.");

            RecordPrefabOverrides();
        }

        // ------------------------------------------------------------------ Boden und Rand

        static void BuildGround(Transform environment)
        {
            SB.Box("Floor", environment, new Vector3(0f, FloorTop * 0.5f, 0f), new Vector3(24f, FloorTop, 24f), M.Grass);

            // Niedrige mintgrüne Umrandung mit 5 m Lücke am Eingang (Osten, z -2.5 … 2.5).
            Transform trim = SB.Group("Trim", environment);
            const float height = 0.25f;
            const float thickness = 0.3f;
            const float gapHalf = 2.5f;
            float y = FloorTop + height * 0.5f;
            float edge = SandboxContext.ZoneHalfSize - thickness * 0.5f;   // 11.85
            float inner = SandboxContext.ZoneHalfSize - thickness;         // 11.7
            float full = SandboxContext.ZoneHalfSize * 2f;
            float eastLength = inner - gapHalf;
            float eastCenter = (inner + gapHalf) * 0.5f;

            SB.Box("Trim_North", trim, new Vector3(0f, y, edge), new Vector3(full, height, thickness), M.MintGreen);
            SB.Box("Trim_South", trim, new Vector3(0f, y, -edge), new Vector3(full, height, thickness), M.MintGreen);
            SB.Box("Trim_West", trim, new Vector3(-edge, y, 0f), new Vector3(thickness, height, inner * 2f), M.MintGreen);
            SB.Box("Trim_East_North", trim, new Vector3(edge, y, eastCenter), new Vector3(thickness, height, eastLength), M.MintGreen);
            SB.Box("Trim_East_South", trim, new Vector3(edge, y, -eastCenter), new Vector3(thickness, height, eastLength), M.MintGreen);
        }

        // ------------------------------------------------------------------ Wege und Treffpunkt

        static void BuildPaths(Transform environment)
        {
            Transform paths = SB.Group("Paths", environment);

            // Alle Wege beginnen unter dem Treffpunkt (der liegt höher und verdeckt die Enden).
            PathStrip("Path_Main", paths, new Vector2(2.8f, 0f), new Vector2(11.995f, 0f), 2.8f);
            PathStrip("Path_Pond", paths, new Vector2(1.5f, 0f), new Vector2(-3.7f, 3.74f), 1.8f);          // endet unter dem Teich-Rand
            PathStrip("Path_BuildArea", paths, new Vector2(1.5f, 0f), new Vector2(1.5f, BuildMaxZ - 0.15f), 1.8f); // endet unter dem Sand

            // Runder Treffpunkt mit Ring und Stern (wie eine kleine Kompassrose).
            Transform plaza = SB.Group("MeetingPoint", paths, new Vector3(1.5f, 0f, 0f));
            Disc("Plaza_Disc", plaza, Vector2.zero, 2.2f, 2.2f, 0.115f, 0.03f, M.PlazaLight);
            Disc("Plaza_Ring", plaza, Vector2.zero, 1.45f, 1.45f, 0.121f, 0.03f, M.PlazaAccent);
            Disc("Plaza_Inner", plaza, Vector2.zero, 1.2f, 1.2f, 0.127f, 0.03f, M.PlazaLight);
            // Stern liegt flach (90° um X gekippt), Spitze zeigt nach Norden, Oberseite bei 0.133.
            SB.MeshObj(MK.Star, "Plaza_Star", plaza, new Vector3(0f, 0.128f, 0f), new Vector3(1.5f, 1.5f, 0.01f), M.MintGreen, new Vector3(90f, 0f, 0f));

            // Trittsteine vom Treffpunkt zur Tür des roten Hauses.
            Transform stones = SB.Group("SteppingStones_HouseRed", paths);
            Vector2 from = new Vector2(-1.25f, -0.8f);
            Vector2 to = new Vector2(-5.65f, -2.95f);
            float walkYaw = Mathf.Atan2(to.x - from.x, to.y - from.y) * Mathf.Rad2Deg;
            float[] yawJitter = { 8f, -12f, 5f, -7f, 13f };
            float[] sizeJitter = { 1.0f, 0.9f, 1.05f, 0.95f, 1.0f };
            for (int i = 0; i < yawJitter.Length; i++)
            {
                Vector2 p = Vector2.Lerp(from, to, i / (yawJitter.Length - 1f));
                float s = sizeJitter[i];
                SB.MeshObj(MK.Cylinder8, "Stone_" + (i + 1), stones, new Vector3(p.x, 0.088f, p.y), new Vector3(0.72f * s, 0.04f, 0.52f * s),
                    M.StoneGray, new Vector3(0f, walkYaw + yawJitter[i], 0f));
            }

            // Flache Bodenbeläge werfen keine Schatten.
            SB.NoShadows(paths.gameObject);
        }

        // ------------------------------------------------------------------ Teich

        static void BuildPond(Transform environment)
        {
            Transform pond = SB.Group("Pond", environment, new Vector3(-5.5f, 0f, 5.0f));

            // Rand: ein flacher grauer Zylinder (Oberkante 0.3). Das Wasser liegt 5 mm darüber und füllt ihn bis zum Rand.
            SB.MeshObj(MK.Cylinder12, "Pond_Rim", pond, Vector3.zero, new Vector3(7f, 0.3f, 5f), M.StoneGray, default, ColliderMode.ConvexMesh);
            GameObject water = Disc("Water", pond, Vector2.zero, 3.15f, 2.15f, 0.305f, 0.255f, M.Water);
            GameObject deep = Disc("Water_Deep", pond, new Vector2(0.25f, 0.15f), 1.8f, 1.15f, 0.311f, 0.2f, M.WaterDeep);
            SB.NoShadows(water);
            SB.NoShadows(deep);

            Transform pads = SB.Group("LilyPads", pond);
            Disc("LilyPad_1", pads, new Vector2(-1.9f, 0.5f), 0.4f, 0.4f, 0.325f, 0.012f, M.LeafGreen, MK.Cylinder8, 20f);
            Disc("LilyPad_2", pads, new Vector2(1.5f, -0.85f), 0.32f, 0.32f, 0.325f, 0.012f, M.LeafGreen, MK.Cylinder8, 65f);
            Disc("LilyPad_3", pads, new Vector2(0.5f, 1.15f), 0.36f, 0.36f, 0.325f, 0.012f, M.LeafGreen, MK.Cylinder8, 10f);
            SB.MeshObj(MK.Sphere, "Blossom", pads, new Vector3(0.5f, 0.39f, 1.15f), new Vector3(0.28f, 0.2f, 0.28f), M.PinkBlossom);
            SB.MeshObj(MK.Sphere, "Blossom_Center", pads, new Vector3(0.5f, 0.475f, 1.15f), Vector3.one * 0.09f, M.SoftYellow);
            SB.NoShadows(pads.gameObject);

            SB.Note(pond.gameObject, "Teich",
                "Ein Teich aus zwei flachen Zylindern: ein grauer Rand und eine blaue Wasserfläche. " +
                "Die dunkle Mitte und die Seerosenblätter sind auch nur flache Zylinder. " +
                "Probier: Zieh ein anderes Material auf „Water“.");
        }

        // ------------------------------------------------------------------ Bearbeitbare Objekte (Prefab-Instanzen)

        static void BuildEditableObjects(Transform editable)
        {
            SB.Note(editable.gameObject, "Deine Objekte",
                "Alles in diesem Ordner darfst du verschieben (W), drehen (E), skalieren (R), duplizieren (Strg+D) und löschen (Entf). " +
                "Keine Angst – es gibt eine Sicherung!");

            Transform houses = SB.Group("Houses", editable);
            Transform trees = SB.Group("Trees", editable);
            Transform benches = SB.Group("Benches", editable);
            Transform decoration = SB.Group("Decoration", editable);

            // Häuser – Prefab-Vorderseite ist -z: yaw -90 dreht die Tür nach +x (zum Treffpunkt),
            // yaw -45 lässt das blaue Haus schräg nach Südosten (zur Kamera und zum Bauplatz) schauen.
            GameObject houseRed = Place(P.SmallHouse, houses, OnFloor(-8.0f, -3.0f), -90f, 1f, "House_Red");
            SB.Note(houseRed, "Rotes Haus",
                "Ein Prefab (blaues Symbol). Zieh ein Material aus Materials/Palette auf das Dach oder die Wände.");

            GameObject houseBlue = Place(P.SmallHouse, houses, OnFloor(-9.0f, -7.7f), -45f, 1f, "House_Blue");
            SB.Recolor(houseBlue, "Roof", M.RoofBlue);
            SB.Note(houseBlue, "Blaues Haus",
                "Das gleiche Prefab wie das rote Haus – nur mit blauem Dach (Roof). " +
                "Probier: Dreh es mit Rotation Y, bis die Tür in eine andere Richtung zeigt.");

            // Bäume (verschiedene Größen und Drehungen, damit sie nicht wie Kopien aussehen).
            GameObject treeSmall1 = Place(P.SmallTree, trees, OnFloor(-2.5f, 9.5f), 15f, 1.0f, "Tree_Small_1");
            SB.Note(treeSmall1, "Kleiner Baum",
                "Ein Baum aus einem Zylinder (Stamm) und zwei facettierten Kugeln (Krone). Probier: Scale auf 2 – doppelt so groß!");
            Place(P.SmallTree, trees, OnFloor(4.5f, 9.0f), 140f, 1.2f, "Tree_Small_2");
            Place(P.LargeTree, trees, OnFloor(-10.0f, 9.5f), 60f, 1.0f, "Tree_Large_1");
            Place(P.PineTree, trees, OnFloor(-10.3f, 2.0f), 0f, 1.05f, "Tree_Pine_1");
            Place(P.PineTree, trees, OnFloor(10.2f, -10.0f), 35f, 0.95f, "Tree_Pine_2");
            Place(P.SmallTree, trees, OnFloor(-2.4f, -8.6f), 250f, 0.9f, "Tree_Small_3");

            // Bänke am Teich (Sitzrichtung = Prefab -z): yaw 180 schaut nach Norden, yaw 90 nach Westen – beide zum Wasser.
            GameObject bench1 = Place(P.Bench, benches, OnFloor(-5.5f, 1.2f), 180f, 1f, "Bench_Pond_1");
            SB.Note(bench1, "Bank am Teich",
                "Eine Bank, die zum Teich schaut. Probier: Dreh sie mit E (Rotation Y). " +
                "Mit Strg+D machst du eine Kopie, die du daneben stellen kannst.");
            Place(P.Bench, benches, OnFloor(-1.6f, 5.0f), 90f, 1f, "Bench_Pond_2");

            // Deko.
            Place(P.LampPost, decoration, OnFloor(3.5f, 1.8f), 0f, 1f, "LampPost_1");
            Place(P.LampPost, decoration, OnFloor(8.0f, -1.8f), 180f, 1f, "LampPost_2");
            for (int i = 0; i < 3; i++)
                Place(P.Fence, decoration, OnFloor(-6f + 3f * i, 10.8f), 0f, 1f, "Fence_" + (i + 1));

            Place(P.Flower, decoration, OnFloor(-3.0f, 7.6f), 20f, 1.1f, "Flower_1");    // am Teich (Nordost)
            Place(P.Flower, decoration, OnFloor(-8.4f, 2.6f), 110f, 1.0f, "Flower_2");   // am Teich (Südwest)
            Place(P.Flower, decoration, OnFloor(-5.3f, -1.7f), 200f, 1.15f, "Flower_3"); // vor dem roten Haus
            Place(P.Flower, decoration, OnFloor(-6.6f, -9.2f), 300f, 1.05f, "Flower_4"); // vor dem blauen Haus

            Place(P.Rock, decoration, OnFloor(-9.1f, 6.1f), 25f, 1.0f, "Rock_1");        // am Teich-Rand (West)
            Place(P.Rock, decoration, OnFloor(-7.0f, 7.75f), -40f, 0.75f, "Rock_2");     // am Teich-Rand (Nord)

            Place(P.FlowerBed, decoration, OnFloor(-5.3f, -4.3f), 0f, 1f, "FlowerBed_1"); // Vorgarten des roten Hauses
            Place(P.DecorativePillar, decoration, OnFloor(5.5f, 6.0f), 0f, 1f, "DecorativePillar_1");

            Place(P.Bush, decoration, OnFloor(10.4f, 8.0f), 30f, 1.0f, "Bush_1");
            Place(P.Bush, decoration, OnFloor(8.2f, 10.4f), 160f, 0.85f, "Bush_2");
            Place(P.Bush, decoration, OnFloor(-10.6f, -0.6f), 75f, 0.9f, "Bush_3");
        }

        // ------------------------------------------------------------------ Bauplatz

        static void BuildBuildArea(Transform buildArea)
        {
            SB.Note(buildArea.gameObject, "Bauplatz",
                "Zieh Prefabs aus dem Project-Fenster (StudentSandbox/Prefabs/Creative) hierher. Oder: GameObject > 3D Object > Cube.");

            float centerX = (BuildMinX + BuildMaxX) * 0.5f;
            float centerZ = (BuildMinZ + BuildMaxZ) * 0.5f;
            GameObject ground = SB.Box("BuildArea_Ground", buildArea, new Vector3(centerX, SandTop - 0.015f, centerZ),
                new Vector3(BuildMaxX - BuildMinX, 0.03f, BuildMaxZ - BuildMinZ), M.Sand);
            SB.MarkStatic(ground);

            BuildBuildAreaBorder(buildArea);
            BuildPallet(buildArea);
            BuildBuildingBlocks(buildArea);
            BuildExampleTower(buildArea);
        }

        /// <summary>Niedrige Bretter-Umrandung (ohne Collider) mit Lücke für den Weg und vier Eckpfosten mit gelben Fähnchen.</summary>
        static void BuildBuildAreaBorder(Transform buildArea)
        {
            Transform border = SB.Group("Border", buildArea);
            const float thickness = 0.18f;
            const float outside = 0.01f; // Bretter stehen 1 cm über den Sandrand hinaus (keine gleichen Seitenflächen).
            float north = BuildMaxZ + outside - thickness * 0.5f;
            float south = BuildMinZ - outside + thickness * 0.5f;
            float west = BuildMinX - outside + thickness * 0.5f;
            float east = BuildMaxX + outside - thickness * 0.5f;
            float x0 = BuildMinX - outside;
            float x1 = BuildMaxX + outside;

            PlankAlongX("Plank_North_West", border, x0, BuildGateMinX, north);
            PlankAlongX("Plank_North_East", border, BuildGateMaxX, x1, north);
            PlankAlongX("Plank_South", border, x0, x1, south);
            float innerZ0 = south + thickness * 0.5f;
            float innerZ1 = north - thickness * 0.5f;
            PlankAlongZ("Plank_West", border, innerZ0, innerZ1, west);
            PlankAlongZ("Plank_East", border, innerZ0, innerZ1, east);

            CornerFlag("Flag_NorthWest", border, BuildMinX, BuildMaxZ);
            CornerFlag("Flag_NorthEast", border, BuildMaxX, BuildMaxZ);
            CornerFlag("Flag_SouthWest", border, BuildMinX, BuildMinZ);
            CornerFlag("Flag_SouthEast", border, BuildMaxX, BuildMinZ);

            SB.MarkStatic(border.gameObject);
        }

        static void PlankAlongX(string name, Transform parent, float fromX, float toX, float z)
        {
            const float height = 0.14f;
            SB.Box(name, parent, new Vector3((fromX + toX) * 0.5f, 0.095f + height * 0.5f, z), new Vector3(toX - fromX, height, 0.18f),
                M.LightWood, default, false);
        }

        static void PlankAlongZ(string name, Transform parent, float fromZ, float toZ, float x)
        {
            const float height = 0.14f;
            SB.Box(name, parent, new Vector3(x, 0.095f + height * 0.5f, (fromZ + toZ) * 0.5f), new Vector3(0.18f, height, toZ - fromZ),
                M.LightWood, default, false);
        }

        static void CornerFlag(string name, Transform parent, float x, float z)
        {
            Transform flag = SB.Group(name, parent, new Vector3(x, 0f, z));
            const float postHeight = 1.35f;
            SB.Box("Post", flag, new Vector3(0f, FloorTop + postHeight * 0.5f, 0f), new Vector3(0.12f, postHeight, 0.12f), M.DarkWood);
            SB.MeshObj(MK.Sphere, "Knob", flag, new Vector3(0f, FloorTop + postHeight + 0.05f, 0f), Vector3.one * 0.16f, M.SoftYellow);
            // Wimpel: Dreiecks-Prisma, um Z gekippt (Spitze nach +x) und um Y gedreht (Spitze nach -z).
            SB.MeshObj(MK.Prism, "Pennant", flag, new Vector3(0f, 1.2f, -0.06f), new Vector3(0.42f, 0.6f, 0.03f), M.SoftYellow,
                new Vector3(0f, 90f, -90f));
        }

        /// <summary>Niedriges Holzpodest (Palette) am Westrand des Bauplatzes.</summary>
        static void BuildPallet(Transform buildArea)
        {
            Transform pallet = SB.Group("Pallet", buildArea, new Vector3(PalletCenterX, 0f, PalletCenterZ));

            // Drei dunkle Kufen (stecken 1 cm im Sand) und sechs helle Bretter darüber.
            float[] runnerX = { -1.1f, 0f, 1.1f };
            for (int i = 0; i < runnerX.Length; i++)
                SB.Box("Runner_" + (i + 1), pallet, new Vector3(runnerX[i], 0.16f, 0f), new Vector3(0.16f, 0.12f, PalletLength), M.DarkWood, default, false);

            const int slatCount = 6;
            const float slatWidth = 0.72f;
            const float slatThickness = 0.05f;
            float gap = (PalletLength - slatCount * slatWidth) / (slatCount - 1);
            for (int i = 0; i < slatCount; i++)
            {
                float z = -PalletLength * 0.5f + slatWidth * 0.5f + i * (slatWidth + gap);
                SB.Box("Slat_" + (i + 1), pallet, new Vector3(0f, PalletTop - slatThickness * 0.5f, z),
                    new Vector3(PalletWidth, slatThickness, slatWidth), M.LightWood);
            }

            SB.MarkStatic(pallet.gameObject);
        }

        /// <summary>Bausteine (Prefab-Instanzen) auf dem Podest – Raster 1.3 m, damit jedes Teil leicht anklickbar ist.</summary>
        static void BuildBuildingBlocks(Transform buildArea)
        {
            Transform blocks = SB.Group("BuildingBlocks", buildArea);
            SB.Note(blocks.gameObject, "Bausteine",
                "Würfel, Kugeln, Zylinder, eine Säule und eine Plattform – alles Prefabs. " +
                "Nimm sie vom Holzpodest und stapel sie im Sand zu einem Turm. " +
                "Tipp: Schau beim Stapeln auch von der Seite, ob alles genau aufeinander liegt.");

            const float spacing = 1.3f;
            float colA = PalletCenterX - spacing * 0.5f;   // 0.65
            float colB = PalletCenterX + spacing * 0.5f;   // 1.95
            float row1 = PalletCenterZ + spacing * 1.5f;   // -5.6
            float row2 = PalletCenterZ + spacing * 0.5f;   // -6.9
            float row3 = PalletCenterZ - spacing * 0.5f;   // -8.2
            float row4 = PalletCenterZ - spacing * 1.5f;   // -9.5

            Place(P.ColoredCube, blocks, OnPallet(colA, row1), 0f, 1f, "Block_Cube_Red");
            SB.Recolor(Place(P.ColoredCube, blocks, OnPallet(colB, row1), 0f, 1f, "Block_Cube_Yellow"), "Cube", M.SoftYellow);
            SB.Recolor(Place(P.ColoredCube, blocks, OnPallet(colA, row2), 0f, 1f, "Block_Cube_Mint"), "Cube", M.MintGreen);
            Place(P.ColoredSphere, blocks, OnPallet(colB, row2), 0f, 1f, "Block_Sphere_Blue");
            SB.Recolor(Place(P.ColoredSphere, blocks, OnPallet(colA, row3), 0f, 1f, "Block_Sphere_Lavender"), "Sphere", M.Lavender);
            Place(P.ColoredCylinder, blocks, OnPallet(colB, row3), 0f, 1f, "Block_Cylinder_Yellow");
            SB.Recolor(Place(P.ColoredCylinder, blocks, OnPallet(colA, row4), 0f, 1f, "Block_Cylinder_Orange"), "Cylinder", M.WarmOrange);
            Place(P.DecorativePillar, blocks, OnPallet(colB, row4), 0f, 0.8f, "Block_Pillar");

            // Die Plattform (3 × 3 m) ist zu groß fürs Podest – sie liegt daneben im Sand und kann als Fundament dienen.
            Place(P.Platform, blocks, new Vector3(4.6f, SandTop, -5.3f), 0f, 1f, "Block_Platform");
        }

        /// <summary>Beispiel-Turm (Aufgabe A2) – genau gestapelt, weil alle Prefabs ihren Drehpunkt unten haben.</summary>
        static void BuildExampleTower(Transform buildArea)
        {
            Transform tower = SB.Group("ExampleTower", buildArea, new Vector3(7.3f, SandTop, -8.4f));
            SB.Note(tower.gameObject, "Beispiel-Turm",
                "Ein Beispiel-Turm aus einfachen Formen. Baue daneben deinen eigenen ungewöhnlichen Turm!");

            const float baseScale = 1.2f;   // Würfel 1.2 m hoch
            const float cylinderHeight = 1.0f;
            const float twistedScale = 0.8f; // Würfel 0.8 m hoch
            float cylinderY = baseScale;
            float twistedY = cylinderY + cylinderHeight;
            float sphereY = twistedY + twistedScale;

            Place(P.ColoredCube, tower, Vector3.zero, 0f, baseScale, "Tower_Cube_Base");
            Place(P.ColoredCylinder, tower, new Vector3(0f, cylinderY, 0f), 0f, 1f, "Tower_Cylinder");
            SB.Recolor(Place(P.ColoredCube, tower, new Vector3(0f, twistedY, 0f), 45f, twistedScale, "Tower_Cube_Twisted"), "Cube", M.Lavender);
            SB.Recolor(Place(P.ColoredSphere, tower, new Vector3(0f, sphereY, 0f), 0f, 0.7f, "Tower_Sphere_Top"), "Sphere", M.MintGreen);
        }

        // ------------------------------------------------------------------ Farbkasten

        static void BuildColorPalette(Transform palette)
        {
            SB.Note(palette.gameObject, "Farbkasten",
                "Jede Kugel zeigt ein Material aus dem Ordner Materials/Palette. " +
                "Zieh ein Material aus dem Project-Fenster auf ein Objekt in der Scene View.");

            Vector3 origin = new Vector3(PaletteCenterX, 0f, PaletteZ);

            // Langes, niedriges Holzregal. Vorderseite = -z (Süden): so sind die Namen aus der Kamera-Richtung (Südosten) lesbar.
            Transform shelf = SB.Group("Shelf", palette, origin);
            SB.Box("Shelf_Body", shelf, new Vector3(0f, (FloorTop + ShelfTop - 0.05f) * 0.5f, 0f), new Vector3(8.0f, ShelfTop - 0.05f - FloorTop, 0.6f), M.LightWood);
            SB.Box("Shelf_Top", shelf, new Vector3(0f, ShelfTop - 0.025f, 0f), new Vector3(8.2f, 0.05f, 0.7f), M.DarkWood);
            // Schräge Namensleiste vorne (45° nach oben gekippt – von vorne und von oben lesbar).
            Vector3 stripCenter = new Vector3(0f, 0.47f, -0.4f);
            SB.Box("Label_Strip", shelf, stripCenter, new Vector3(8.0f, 0.18f, 0.025f), M.WarmWhite, new Vector3(45f, 0f, 0f), false);

            Transform stands = SB.Group("Stands", palette, origin);
            Transform swatches = SB.Group("Swatches", palette, origin);
            Transform labels = SB.Group("Labels", palette, origin);

            const float sphereDiameter = 0.55f;
            const float standTop = ShelfTop + 0.08f;
            Vector3 stripNormal = new Vector3(0f, Mathf.Sqrt(0.5f), -Mathf.Sqrt(0.5f));
            Vector3 labelOffset = stripNormal * 0.016f;
            int middle = PaletteOrder.Length / 2;

            for (int i = 0; i < PaletteOrder.Length; i++)
            {
                string key = PaletteOrder[i];
                float x = (i - middle) * SwatchSpacing;
                Disc("Stand_" + key, stands, new Vector2(x, 0.05f), 0.14f, 0.14f, standTop, 0.085f, M.WarmWhite);
                SB.Prim(PrimitiveType.Sphere, key, swatches, new Vector3(x, standTop + sphereDiameter * 0.5f - 0.03f, 0.05f),
                    Vector3.one * sphereDiameter, key);
                SB.Text("Label_" + key, key, labels, new Vector3(x, stripCenter.y, stripCenter.z) + labelOffset, new Vector3(45f, 0f, 0f),
                    0.12f, TextColor, FontStyle.Normal);
            }

            BuildPaletteHeader(palette, origin);
        }

        /// <summary>Kleines Schild "FARBEN" hinten auf dem Regal, über den Kugeln (blockiert nichts).</summary>
        static void BuildPaletteHeader(Transform palette, Vector3 origin)
        {
            Transform header = SB.Group("Sign_Colors", palette, origin + new Vector3(0f, 0f, 0.27f));
            const float boardY = 1.55f;
            const float boardZ = -0.02f;
            const float boardDepth = 0.06f;
            foreach (float side in new[] { -1f, 1f })
            {
                string sideName = side < 0f ? "Left" : "Right";
                SB.Box("Post_" + sideName, header, new Vector3(side * 1.1f, (ShelfTop - 0.02f + 1.85f) * 0.5f, 0f),
                    new Vector3(0.08f, 1.85f - (ShelfTop - 0.02f), 0.08f), M.DarkWood);
            }
            SB.Box("Board", header, new Vector3(0f, boardY, boardZ), new Vector3(2.5f, 0.55f, boardDepth), M.WarmWhite);
            SB.Box("Board_Trim", header, new Vector3(0f, boardY + 0.315f, boardZ), new Vector3(2.6f, 0.08f, 0.1f), M.MintGreen, default, false);

            float front = boardZ - boardDepth * 0.5f - 0.005f;
            float back = boardZ + boardDepth * 0.5f + 0.005f;
            SB.Text("Title", "FARBEN", header, new Vector3(0f, boardY + 0.09f, front), Vector3.zero, 0.28f, TextColor);
            SB.Text("Subtitle", "Zieh eine Farbe auf ein Objekt", header, new Vector3(0f, boardY - 0.15f, front), Vector3.zero, 0.12f, TextColor, FontStyle.Normal);
            SB.Text("Title_Back", "FARBEN", header, new Vector3(0f, boardY, back), new Vector3(0f, 180f, 0f), 0.3f, TextColor);
        }

        // ------------------------------------------------------------------ Schilder

        static void BuildSigns(Transform signs)
        {
            // Beide Schilder schauen nach Ost-Südost: zum Eingang/zentralen Platz und zum Kamera-Aussichtspunkt.
            GameObject welcome = SB.Sign("Sign_Welcome", signs, new Vector3(8.6f, FloorTop, 4.2f), -70f,
                "PARK & BAUPLATZ", "Gestalte deine eigene Welt", M.MintGreen, 4.4f, 2.1f);
            SetLetterHeight(welcome, "Title", 0.4f);

            GameObject buildSign = SB.Sign("Sign_BuildArea", signs, new Vector3(9.9f, FloorTop, -6.6f), -80f,
                "BAUPLATZ", "Hier ist Platz für deine Ideen!", M.MintGreen, 3.4f, 1.8f);
            SetLetterHeight(buildSign, "Subtitle", 0.22f);
        }

        // ------------------------------------------------------------------ Helfer

        static Vector3 OnFloor(float x, float z) => new Vector3(x, FloorTop, z);

        static Vector3 OnPallet(float x, float z) => new Vector3(x, PalletTop, z);

        /// <summary>SB.Place + merken, damit die Änderungen am Ende als Prefab-Overrides gespeichert werden.</summary>
        static GameObject Place(string prefabName, Transform parent, Vector3 localPosition, float yaw, float uniformScale, string instanceName)
        {
            GameObject instance = SB.Place(prefabName, parent, localPosition, yaw, uniformScale, instanceName);
            PlacedInstances.Add(instance);
            return instance;
        }

        /// <summary>
        /// Ein flacher Weg-Streifen von "from" nach "to" (x/z-Koordinaten), Oberkante <see cref="PathTop"/>, ohne Collider.
        /// </summary>
        static GameObject PathStrip(string name, Transform parent, Vector2 from, Vector2 to, float width)
        {
            Vector2 delta = to - from;
            Vector2 middle = (from + to) * 0.5f;
            float yaw = Mathf.Atan2(delta.x, delta.y) * Mathf.Rad2Deg;
            return SB.Box(name, parent, new Vector3(middle.x, PathTop - PathThickness * 0.5f, middle.y),
                new Vector3(width, PathThickness, delta.magnitude), M.Path, new Vector3(0f, yaw, 0f), false);
        }

        /// <summary>Flache (ovale) Scheibe aus einem Sockel-Mesh mit genau festgelegter Oberkante, ohne Collider.</summary>
        static GameObject Disc(string name, Transform parent, Vector2 center, float radiusX, float radiusZ, float topY, float thickness,
            string material, string meshKey = MK.Cylinder12, float yaw = 0f)
        {
            return SB.MeshObj(meshKey, name, parent, new Vector3(center.x, topY - thickness, center.y),
                new Vector3(radiusX * 2f, thickness, radiusZ * 2f), material, new Vector3(0f, yaw, 0f));
        }

        /// <summary>Ändert die Schrifthöhe eines Schild-Textes (gleiche Umrechnung wie SB.Text: fontSize 96).</summary>
        static void SetLetterHeight(GameObject root, string childName, float letterHeight)
        {
            foreach (TextMesh text in root.GetComponentsInChildren<TextMesh>(true))
            {
                if (text.gameObject.name == childName) text.characterSize = letterHeight / 9.6f;
            }
        }

        /// <summary>
        /// Speichert alle per Skript geänderten Werte der Prefab-Instanzen (Name, Transform, Materialien, Info-Notizen)
        /// als Overrides – sonst könnten sie beim nächsten Laden des Prefabs verloren gehen.
        /// </summary>
        static void RecordPrefabOverrides()
        {
            foreach (GameObject instance in PlacedInstances)
            {
                if (instance == null || !PrefabUtility.IsPartOfPrefabInstance(instance)) continue;
                PrefabUtility.RecordPrefabInstancePropertyModifications(instance);
                foreach (Component component in instance.GetComponentsInChildren<Component>(true))
                {
                    if (component == null || !PrefabUtility.IsPartOfPrefabInstance(component)) continue;
                    if (PrefabUtility.IsAddedComponentOverride(component)) continue;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                }
            }
            PlacedInstances.Clear();
        }
    }
}
