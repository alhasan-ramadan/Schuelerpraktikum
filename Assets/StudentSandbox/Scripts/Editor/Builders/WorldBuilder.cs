using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace StudentSandbox.EditorTools
{
    /// <summary>
    /// Baut alles außerhalb der vier Bereiche:
    ///  - 01_ENVIRONMENT: Insel (Gras, Rand, Erde, Strand), Wasser (Lagune, Meer, Brandung) und die vier Wege.
    ///  - 02_CENTRAL_HUB: der zentrale Platz mit Springbrunnen, Bänken, Laternen, Hecke, Blumenbeeten und Schildern.
    ///  - 09_DECORATION: Bäume, Steine, Blumen, Wolken, Strand-Deko und vier Sehenswürdigkeiten
    ///    (Windmühle NO, Leuchtturm SW, Steg mit Boot SO, Picknickplatz NW).
    ///
    /// Richtungsangaben: "bearing" = Himmelsrichtung in Grad, 0 = Norden (+z), 90 = Osten (+x), 180 = Süden, 270 = Westen.
    /// Alle Höhen sind so gewählt, dass keine zwei sichtbaren Flächen auf derselben Höhe liegen (kein Z-Fighting).
    /// </summary>
    public static class WorldBuilder
    {
        // ------------------------------------------------------------------ Maße der Insel

        const float BeachTop = -0.3f;
        const float WaterLevel = -0.45f;
        const float PathStart = 8.5f;
        const float PathEnd = 13.3f;
        const float PathThickness = 0.1f;

        // ------------------------------------------------------------------ Maße des Platzes

        const float HubTop = SandboxContext.HubTop;          // 0.12
        const float InnerDiscTop = 0.14f;                    // helle Innenfläche um den Brunnen
        const float BenchRadius = 7.1f;
        const float LampRadius = 8.2f;
        const float HedgeRadius = 9.8f;
        const float FlowerBedRadius = 7.6f;

        /// <summary>Feste Startzahl für den Zufall: Bei jedem Erzeugen sieht die Welt gleich aus.</summary>
        const int Seed = 20261020;

        static readonly float[] Diagonals = { 45f, 135f, 225f, 315f };
        static readonly string[] DiagonalNames = { "NE", "SE", "SW", "NW" };

        enum CrownStyle
        {
            Default,
            Autumn,
            Light,
            Blossom
        }

        readonly struct Spot
        {
            public readonly string Name;
            public readonly string Prefab;
            public readonly float X;
            public readonly float Z;
            public readonly CrownStyle Crown;

            public Spot(string name, string prefab, float x, float z, CrownStyle crown = CrownStyle.Default)
            {
                Name = name;
                Prefab = prefab;
                X = x;
                Z = z;
                Crown = crown;
            }
        }

        // ================================================================== 01_ENVIRONMENT

        public static void BuildEnvironment(SandboxContext ctx)
        {
            Transform environment = ctx.Environment;
            BuildIsland(SB.Group("Island", environment));
            BuildWater(SB.Group("Water", environment));
            BuildPaths(SB.Group("Paths", environment), new System.Random(Seed + 1));
        }

        static void BuildIsland(Transform island)
        {
            // Grasfläche: 12-eckiger Zylinder, Oberkante genau bei y = 0.
            GameObject grass = SB.MeshObj(MK.Cylinder12, "Island_Grass", island, new Vector3(0f, -0.6f, 0f), new Vector3(84f, 0.6f, 84f),
                M.Grass, default, ColliderMode.Mesh);

            // Dunklerer Grasrand (eine Stufe tiefer) – gibt der Kante Tiefe.
            SB.MeshObj(MK.Cylinder12, "Island_GrassEdge", island, new Vector3(0f, -0.6f, 0f), new Vector3(85.6f, 0.48f, 85.6f), M.GrassDark);

            // Erdsockel unter dem Gras (liegt unter Strand und Wasser).
            SB.MeshObj(MK.Cylinder12, "Island_Cliff", island, new Vector3(0f, -3.0f, 0f), new Vector3(81f, 2.35f, 81f), M.Cliff);

            // Sandstrand: um 15° gedreht, damit seine Ecken zwischen den Ecken der Grasfläche liegen (abwechselnd breiter und schmaler Strand).
            SB.MeshObj(MK.Cylinder12, "Island_Beach", island, new Vector3(0f, -1.2f, 0f), new Vector3(91f, 0.9f, 91f), M.Beach,
                new Vector3(0f, 15f, 0f));

            SB.MarkStatic(island.gameObject);
            SB.Note(grass, "Der Boden der Insel",
                "Der Boden der Insel ist ein flacher Zylinder mit 12 Ecken – so entsteht der Low-Poly-Look.\n" +
                "Er hat einen Mesh Collider, deshalb fallen Kugeln und Objekte nicht hindurch.\n" +
                "Darunter liegen der dunkle Grasrand, die Erde (Island_Cliff) und der Sandstrand (Island_Beach).\n" +
                "Probier: Zieh ein anderes Material aus Materials/Palette auf den Boden.");
        }

        static void BuildWater(Transform water)
        {
            // Dunkles, tiefes Meer – 600 x 600 m, damit man nirgends den Rand sieht.
            SB.Prim(PrimitiveType.Plane, "Ocean", water, new Vector3(0f, WaterLevel - 0.02f, 0f), new Vector3(60f, 1f, 60f), M.WaterDeep,
                default, false);
            // Helles Wasser rund um die Insel (Oberkante y = -0.45). Der Rand liegt so weit draußen, dass ihn der Nebel weich macht.
            SB.MeshObj(MK.Cylinder12, "Lagoon", water, new Vector3(0f, WaterLevel - 0.02f, 0f), new Vector3(150f, 0.02f, 150f), M.Water);
            // Weißer Schaum direkt am Strand (wie der Strand um 15° gedreht).
            SB.MeshObj(MK.Cylinder12, "Surf", water, new Vector3(0f, WaterLevel - 0.01f, 0f), new Vector3(92.6f, 0.02f, 92.6f), M.WarmWhite,
                new Vector3(0f, 15f, 0f));

            SB.NoShadows(water.gameObject);
            SB.MarkStatic(water.gameObject);
            SB.Note(water.gameObject, "Das Meer",
                "Das Wasser besteht nur aus flachen Flächen ohne Collider:\n" +
                "'Lagoon' ist das helle Wasser nahe der Insel, 'Ocean' das dunkle Meer weiter draußen, 'Surf' der weiße Schaum am Strand.\n" +
                "Fällt eine Kugel ins Meer, kommt sie automatisch zu ihrem Start zurück.\n" +
                "Probier: Gib der Lagune ein anderes Material – zum Beispiel Turquoise.");
        }

        static void BuildPaths(Transform paths, System.Random random)
        {
            BuildPath(paths, "Path_Creative", Vector3.left, random);
            BuildPath(paths, "Path_Physics", Vector3.forward, random);
            BuildPath(paths, "Path_Light", Vector3.right, random);
            BuildPath(paths, "Path_Interaction", Vector3.back, random);

            SB.MarkStatic(paths.gameObject);
            SB.Note(paths.gameObject, "Die Wege",
                "Vier Wege führen vom Platz in der Mitte zu den vier Bereichen.\n" +
                "Jeder Weg ('Surface') ist ein Würfel, der ganz flach und lang gezogen wurde – schau dir seine Scale im Inspector an!\n" +
                "Die kleinen grauen Trittsteine am Rand sind nur Deko.");
        }

        /// <summary>Ein Weg vom Platz (Radius 8.5) bis unter den Boden des Bereichs (Radius 13.3), mit Trittsteinen an beiden Rändern.</summary>
        static void BuildPath(Transform parent, string name, Vector3 direction, System.Random random)
        {
            Transform path = SB.Group(name, parent);
            bool alongX = Mathf.Abs(direction.x) > 0.5f;
            float length = PathEnd - PathStart;
            Vector3 center = direction * (PathStart + length * 0.5f) + Vector3.up * (SandboxContext.PathTop - PathThickness * 0.5f);
            Vector3 size = alongX
                ? new Vector3(length, PathThickness, SandboxContext.PathWidth)
                : new Vector3(SandboxContext.PathWidth, PathThickness, length);
            SB.Box("Surface", path, center, size, M.Path);

            // Flache Trittsteine knapp neben dem Weg (auf dem Gras, ohne Collider).
            Vector3 side = new Vector3(direction.z, 0f, -direction.x);
            float baseYaw = alongX ? 0f : 90f;
            float[] distances = { 9.7f, 10.6f, 11.5f, 12.4f };
            int index = 1;
            foreach (float sideSign in new[] { -1f, 1f })
            {
                foreach (float distance in distances)
                {
                    float lateral = SandboxContext.PathWidth * 0.5f + 0.32f + Range(random, -0.06f, 0.06f);
                    Vector3 position = direction * (distance + Range(random, -0.12f, 0.12f)) + side * (sideSign * lateral);
                    position.y = 0.005f;
                    GameObject stone = SB.MeshObj(MK.Cylinder8, "Stone_" + index, path, position,
                        new Vector3(Range(random, 0.5f, 0.68f), 0.05f, Range(random, 0.34f, 0.44f)), M.StoneGray,
                        new Vector3(0f, baseYaw + Range(random, -14f, 14f), 0f));
                    SB.NoShadows(stone);
                    index++;
                }
            }
        }

        // ================================================================== 02_CENTRAL_HUB

        public static void BuildHub(SandboxContext ctx)
        {
            Transform hub = ctx.Hub;
            BuildPlaza(SB.Group("Plaza", hub));
            BuildFountain(SB.Group("Fountain", hub));
            BuildBenches(SB.Group("Benches", hub));
            BuildHubPlants(SB.Group("Plants", hub), new System.Random(Seed + 2));
            BuildHubLamps(SB.Group("Lamps", hub));
            BuildHubSigns(SB.Group("Signs", hub));
        }

        static void BuildPlaza(Transform plaza)
        {
            // Hauptfläche: Radius 9, Oberkante 0.12 (liegt über den Wegen, die darunter enden).
            GameObject ground = SB.MeshObj(MK.Cylinder12, "Plaza_Ground", plaza, new Vector3(0f, -0.1f, 0f),
                new Vector3(18f, HubTop + 0.1f, 18f), M.PlazaLight, default, ColliderMode.Mesh);
            // Bordstein rundherum (eine Stufe tiefer, etwas größer).
            SB.MeshObj(MK.Cylinder12, "Plaza_Curb", plaza, new Vector3(0f, -0.1f, 0f), new Vector3(18.6f, 0.2f, 18.6f), M.PlazaAccent,
                default, ColliderMode.Mesh);

            // Dekorative Ringe: jede Schicht 1 cm höher als die darunter.
            GameObject ring = SB.MeshObj(MK.Cylinder12, "Plaza_Ring", plaza, new Vector3(0f, 0.11f, 0f), new Vector3(13f, 0.02f, 13f), M.PlazaAccent);
            GameObject inner = SB.MeshObj(MK.Cylinder12, "Plaza_Inner", plaza, new Vector3(0f, 0.12f, 0f), new Vector3(11.8f, 0.02f, 11.8f), M.PlazaLight);
            GameObject fountainRing = SB.MeshObj(MK.Cylinder12, "Plaza_FountainRing", plaza, new Vector3(0f, 0.13f, 0f),
                new Vector3(6.6f, 0.02f, 6.6f), M.PlazaAccent);
            SB.NoShadows(ring);
            SB.NoShadows(inner);
            SB.NoShadows(fountainRing);

            // Rauten-Kacheln an den vier Eingängen (wie eine Kompassrose).
            string[] axisNames = { "N", "E", "S", "W" };
            for (int i = 0; i < 4; i++)
            {
                Vector3 position = Polar(i * 90f, 7.6f, 0.12f);
                GameObject tile = SB.Box("Plaza_Tile_" + axisNames[i], plaza, position, new Vector3(1.3f, 0.02f, 1.3f), M.PlazaAccent,
                    new Vector3(0f, 45f, 0f), false);
                SB.NoShadows(tile);
            }

            SB.MarkStatic(plaza.gameObject);
            SB.Note(ground, "Der zentrale Platz",
                "Hier treffen sich alle Wege. Von hier kommst du in die vier Bereiche:\n" +
                "Bauen im Westen, Physik im Norden, Licht im Osten und Schalter im Süden.\n" +
                "Der Wegweiser neben dem Brunnen zeigt dir die Richtung.");
        }

        static void BuildFountain(Transform fountain)
        {
            // --- Unteres Becken
            SB.MeshObj(MK.Cylinder12, "Basin", fountain, new Vector3(0f, HubTop, 0f), new Vector3(5.2f, 0.5f - HubTop, 5.2f), M.StoneGray,
                default, ColliderMode.ConvexMesh);
            PolygonRing("Basin_Rim", fountain, new Vector3(0f, 0.5f, 0f), 12, 2.6f * Mathf.Cos(15f * Mathf.Deg2Rad), 0.32f, 0.22f,
                M.StoneGray, true);
            GameObject basinWater = SB.MeshObj(MK.Cylinder12, "Basin_Water", fountain, new Vector3(0f, 0.51f, 0f),
                new Vector3(4.6f, 0.04f, 4.6f), M.Water);

            // --- Säule mit Ring
            SB.MeshObj(MK.Cylinder8, "Column", fountain, new Vector3(0f, 0.5f, 0f), new Vector3(0.6f, 1.3f, 0.6f), M.WarmWhite,
                default, ColliderMode.ConvexMesh);
            SB.MeshObj(MK.Torus, "Column_Ring", fountain, new Vector3(0f, 1.05f, 0f), new Vector3(0.85f, 0.8f, 0.85f), M.Turquoise);

            // --- Obere Schale: umgedrehter Kegel, darauf Wasser und ein runder Rand
            SB.MeshObj(MK.Cone8, "Bowl", fountain, new Vector3(0f, 1.85f, 0f), new Vector3(2.3f, 0.5f, 2.3f), M.StoneGray,
                new Vector3(180f, 0f, 0f));
            GameObject bowlWater = SB.MeshObj(MK.Cylinder12, "Bowl_Water", fountain, new Vector3(0f, 1.84f, 0f),
                new Vector3(1.9f, 0.03f, 1.9f), M.Water);
            SB.MeshObj(MK.Torus, "Bowl_Lip", fountain, new Vector3(0f, 1.87f, 0f), new Vector3(2.25f, 1.2f, 2.25f), M.WarmWhite);

            // --- Vier Wasserstrahlen, die aus der Schale ins Becken fallen
            for (int i = 0; i < 4; i++)
            {
                GameObject stream = SB.MeshObj(MK.Cylinder8, "Water_Stream_" + (i + 1), fountain, Polar(45f + i * 90f, 1.22f, 0.55f),
                    new Vector3(0.1f, 1.27f, 0.1f), M.Water);
                SB.NoShadows(stream);
            }

            // --- Spitze mit drehendem Kristall
            SB.MeshObj(MK.Cylinder8, "Top_Column", fountain, new Vector3(0f, 1.8f, 0f), new Vector3(0.28f, 0.7f, 0.28f), M.WarmWhite);
            SB.MeshObj(MK.Cylinder8, "Top_Cap", fountain, new Vector3(0f, 2.5f, 0f), new Vector3(0.5f, 0.08f, 0.5f), M.StoneGray);
            GameObject ornament = SB.MeshObj(MK.Crystal, "Ornament", fountain, new Vector3(0f, 3.12f, 0f), Vector3.one * 0.55f, M.Turquoise);
            Rotator spin = ornament.AddComponent<Rotator>();
            spin.Speed = 30f;
            spin.RotationAxis = Vector3.up;
            spin.IsRotating = true;

            SB.NoShadows(basinWater);
            SB.NoShadows(bowlWater);

            SB.Note(fountain.gameObject, "Der Springbrunnen",
                "Der Brunnen ist nur aus einfachen Formen gebaut: Zylinder, ein umgedrehter Kegel (die Schale), Ringe und ganz oben ein Kristall.\n" +
                "Das Wasser ist eine flache, blaue Scheibe.\n" +
                "Probier: Gib dem Wasser ('Basin_Water') ein anderes Material.");
            SB.Note(ornament, "Der drehende Kristall",
                "Das Rotator-Skript dreht den Kristall langsam um die Y-Achse: 30 Grad pro Sekunde.\n" +
                "Drück Play, dann siehst du die Drehung.\n" +
                "Probier: Stell Speed auf 180 oder Rotation Axis auf (1, 0, 0).");
        }

        static void BuildBenches(Transform benches)
        {
            for (int i = 0; i < Diagonals.Length; i++)
            {
                Vector3 position = Polar(Diagonals[i], BenchRadius, HubTop);
                SB.Place(P.Bench, benches, position, YawToward(position, Vector3.zero), 1f, "Bench_Hub_" + DiagonalNames[i]);
            }
            SB.Note(benches.gameObject, "Bänke",
                "Vier Bänke aus dem Prefab 'Bench'. Jede Bank ist so gedreht (Rotation Y), dass man auf den Brunnen schaut.\n" +
                "Probier: Dreh eine Bank mit dem Rotate-Werkzeug (Taste E).");
        }

        static void BuildHubLamps(Transform lamps)
        {
            for (int i = 0; i < Diagonals.Length; i++)
            {
                Vector3 position = Polar(Diagonals[i], LampRadius, HubTop);
                SB.Place(P.LampPost, lamps, position, YawToward(position, Vector3.zero), 1f, "LampPost_Hub_" + DiagonalNames[i]);
            }
            SB.Note(lamps.gameObject, "Laternen",
                "Die Laternen gehen bei Sonnenuntergang und in der Nacht automatisch an. Das macht der Lamp Controller.\n" +
                "Probier: Stell im Objekt 08_MANAGERS/DayNightController die Nacht ein.");
        }

        static void BuildHubPlants(Transform plants, System.Random random)
        {
            // Hecke am Rand des Platzes – mit Lücken für die vier Wege (kein Busch näher als 3 m an einer Achse).
            Transform hedge = SB.Group("Hedge", plants);
            const int bushesPerQuarter = 8;
            const float firstBearing = 22f;
            const float lastBearing = 68f;
            for (int quarter = 0; quarter < 4; quarter++)
            {
                for (int k = 0; k < bushesPerQuarter; k++)
                {
                    float bearing = quarter * 90f + Mathf.Lerp(firstBearing, lastBearing, k / (bushesPerQuarter - 1f));
                    Vector3 position = Polar(bearing, HedgeRadius, 0f);
                    float scale = 0.8f * Range(random, 0.94f, 1.06f);
                    SB.Place(P.Bush, hedge, position, Range(random, 0f, 360f), scale, "Hedge_" + DiagonalNames[quarter] + "_" + (k + 1));
                }
            }

            // Vier Blumenbeete: links und rechts der Eingänge im Norden und Süden (das südliche Paar rahmt das Willkommensschild ein).
            float[] bedBearings = { 25.5f, 334.5f, 154.5f, 205.5f };
            for (int i = 0; i < bedBearings.Length; i++)
            {
                Vector3 position = Polar(bedBearings[i], FlowerBedRadius, HubTop);
                SB.Place(P.FlowerBed, plants, position, YawToward(position, Vector3.zero), 1f, "FlowerBed_Hub_" + (i + 1));
            }

            SB.Note(plants.gameObject, "Hecke und Blumenbeete",
                "Die Hecke besteht aus vielen kleinen Büschen (Prefab 'Bush'). An den vier Wegen ist eine Lücke, damit man durchgehen kann.\n" +
                "Probier: Kopiere einen Busch mit Strg+D und schieb ihn an eine neue Stelle.");
        }

        static void BuildHubSigns(Transform signs)
        {
            GameObject welcome = SB.Sign("WelcomeSign", signs, new Vector3(0f, HubTop, -7.3f), 0f, "MINI WORLD", "Deine Unity-Spielwiese",
                M.SoftYellow, 4.2f, 2.4f);
            // Rückseite (vom Brunnen aus lesbar).
            SB.Text("Title_Back", "Viel Spaß!", welcome.transform, new Vector3(0f, 2.4f, 0.075f), new Vector3(0f, 180f, 0f), 0.42f,
                SB.Hex("#2E3440"));
            SB.Note(welcome, "Das Willkommensschild",
                "Die Schrift ist ein 3D-Text (Text Mesh).\n" +
                "Probier: Klick auf das Kind-Objekt 'Title' und schreib bei Text deinen Namen hinein.\n" +
                "Auf der Rückseite ('Title_Back') steht noch ein Gruß.");

            // Wegweiser neben dem Brunnen (yaw 0 – die Pfeile zeigen schon in die richtigen Himmelsrichtungen).
            SB.Place(P.Signpost, signs, new Vector3(3.0f, InnerDiscTop, -3.0f), 0f, 1f, "Signpost_Hub");
        }

        // ================================================================== 09_DECORATION

        public static void BuildDecoration(SandboxContext ctx)
        {
            Transform decoration = ctx.Decoration;
            Transform trees = SB.Group("Trees", decoration);
            Transform rocks = SB.Group("Rocks", decoration);
            Transform flowers = SB.Group("Flowers", decoration);
            Transform landmarks = SB.Group("Landmarks", decoration);
            Transform clouds = SB.Group("Clouds", decoration);
            Transform shore = SB.Group("Shore", decoration);

            BuildTrees(trees, new System.Random(Seed + 10));
            BuildRocks(rocks, new System.Random(Seed + 11));
            BuildFlowers(flowers, new System.Random(Seed + 12));
            BuildWindmill(landmarks);
            BuildLighthouse(landmarks, new System.Random(Seed + 13));
            BuildPier(landmarks);
            BuildPicnic(landmarks, new System.Random(Seed + 14));
            BuildClouds(clouds, new System.Random(Seed + 15));
            BuildShore(shore, new System.Random(Seed + 16));

            SB.Note(decoration.gameObject, "Deko rund um die Bereiche",
                "Hier liegt alles, was die Insel schön macht: Bäume, Steine, Blumen, Wolken und vier besondere Orte:\n" +
                "die Windmühle (Nordosten), den Leuchtturm (Südwesten), den Steg mit Boot (Südosten) und den Picknickplatz (Nordwesten).");
        }

        // ------------------------------------------------------------------ Bäume, Steine, Blumen

        static readonly Spot[] TreeSpots =
        {
            // Nordosten (rund um die Windmühle – nicht vor ihren Flügeln)
            new Spot("Tree_NE_1", P.LargeTree, 17f, 30.5f),
            new Spot("Tree_NE_2", P.PineTree, 15.5f, 34.5f),
            new Spot("Tree_NE_3", P.SmallTree, 16f, 22.5f, CrownStyle.Autumn),
            new Spot("Tree_NE_4", P.LargeTree, 30.5f, 17f, CrownStyle.Light),
            new Spot("Tree_NE_5", P.PineTree, 34.5f, 15.5f),
            new Spot("Bush_NE_1", P.Bush, 21.5f, 17f),
            // Nordwesten (rund um den Picknickplatz)
            new Spot("Tree_NW_1", P.PineTree, -16f, 33.5f),
            new Spot("Tree_NW_2", P.LargeTree, -17f, 21f, CrownStyle.Autumn),
            new Spot("Tree_NW_3", P.PineTree, -33.5f, 16f),
            new Spot("Tree_NW_4", P.SmallTree, -30f, 17.5f, CrownStyle.Light),
            new Spot("Tree_NW_5", P.SmallTree, -15.5f, 27f),
            new Spot("Bush_NW_1", P.Bush, -21f, 16.5f),
            // Südwesten (rund um den Leuchtturm)
            new Spot("Tree_SW_1", P.PineTree, -16f, -33.5f),
            new Spot("Tree_SW_2", P.PineTree, -33.5f, -16f),
            new Spot("Tree_SW_3", P.LargeTree, -17.5f, -22f, CrownStyle.Light),
            new Spot("Tree_SW_4", P.SmallTree, -21f, -16f, CrownStyle.Autumn),
            new Spot("Bush_SW_1", P.Bush, -31f, -20.5f),
            new Spot("Bush_SW_2", P.Bush, -20.5f, -31f),
            // Südosten (rund um den Steg)
            new Spot("Tree_SE_1", P.LargeTree, 17.5f, -22f, CrownStyle.Autumn),
            new Spot("Tree_SE_2", P.PineTree, 15.5f, -33.5f),
            new Spot("Tree_SE_3", P.SmallTree, 16f, -27f, CrownStyle.Light),
            new Spot("Tree_SE_4", P.PineTree, 33.5f, -16f),
            new Spot("Tree_SE_5", P.SmallTree, 25f, -17.5f),
            new Spot("Bush_SE_1", P.Bush, 21f, -16f),
            // Kleine Dreiecke zwischen den Wegen nahe am Platz
            new Spot("Bush_Hub_NE", P.Bush, 10.5f, 10.5f),
            new Spot("Bush_Hub_NW", P.Bush, -10.5f, 10.5f),
        };

        static readonly Spot[] RockSpots =
        {
            new Spot("Rock_NE_1", P.Rock, 14.5f, 28.5f),
            new Spot("Rock_NE_2", P.Rock, 22.5f, 30.5f),
            new Spot("Rock_NE_3", P.Rock, 29.5f, 22.5f),
            new Spot("Rock_NW_1", P.Rock, -17.5f, 30f),
            new Spot("Rock_NW_2", P.Rock, -27.5f, 14.5f),
            new Spot("Rock_NW_3", P.Rock, -10.5f, 6.5f),
            new Spot("Rock_SW_1", P.Rock, -15f, -24f),
            new Spot("Rock_SW_2", P.Rock, -23f, -29.5f),
            new Spot("Rock_SE_1", P.Rock, 30f, -15.5f),
            new Spot("Rock_SE_2", P.Rock, 14.5f, -30f),
        };

        static readonly Spot[] FlowerSpots =
        {
            new Spot("Flowers_Hub_NE", P.Flower, 9f, 7f),
            new Spot("Flowers_Hub_NW", P.Flower, -7f, 9f),
            new Spot("Flowers_Hub_SE", P.Flower, 7f, -9f),
            new Spot("Flowers_Hub_SW", P.Flower, -9f, -7f),
            new Spot("Flowers_NE_1", P.Flower, 18.5f, 26.5f),
            new Spot("Flowers_NE_2", P.Flower, 28.5f, 20.5f),
            new Spot("Flowers_NW_1", P.Flower, -20.5f, 20.5f),
            new Spot("Flowers_SW_1", P.Flower, -19f, -27.5f),
            new Spot("Flowers_SW_2", P.Flower, -24f, -19f),
            new Spot("Flowers_SE_1", P.Flower, 20.5f, -30.5f),
        };

        static void BuildTrees(Transform trees, System.Random random)
        {
            foreach (Spot spot in TreeSpots)
            {
                // Große Bäume etwas kleiner halten (höchstens ca. 6 m).
                float scale = spot.Prefab == P.LargeTree ? Range(random, 0.8f, 1.1f) : Range(random, 0.8f, 1.3f);
                GameObject tree = SB.Place(spot.Prefab, trees, new Vector3(spot.X, 0f, spot.Z), Range(random, 0f, 360f), scale, spot.Name);
                TintCrowns(tree, spot.Crown);
            }
            SB.MarkStatic(trees.gameObject);
            SB.Note(trees.gameObject, "Bäume",
                "Lauter Baum-Prefabs: SmallTree, LargeTree, PineTree und Bush.\n" +
                "Ein paar Kronen haben ein anderes Material bekommen (Herbst, Hellgrün).\n" +
                "Probier: Zieh ein Material aus Materials/Palette auf eine Baumkrone.");
        }

        static void BuildRocks(Transform rocks, System.Random random)
        {
            foreach (Spot spot in RockSpots)
                SB.Place(spot.Prefab, rocks, new Vector3(spot.X, 0f, spot.Z), Range(random, 0f, 360f), Range(random, 0.8f, 1.6f), spot.Name);
            SB.MarkStatic(rocks.gameObject);
            SB.Note(rocks.gameObject, "Steine",
                "Steine aus dem Prefab 'Rock'. Jeder Stein ist etwas anders gedreht und skaliert – so sieht es natürlicher aus.");
        }

        static void BuildFlowers(Transform flowers, System.Random random)
        {
            foreach (Spot spot in FlowerSpots)
                SB.Place(spot.Prefab, flowers, new Vector3(spot.X, 0f, spot.Z), Range(random, 0f, 360f), Range(random, 0.9f, 1.3f), spot.Name);
            SB.MarkStatic(flowers.gameObject);
            SB.Note(flowers.gameObject, "Blumen",
                "Kleine Blumen aus dem Prefab 'Flower'.\n" +
                "Probier: Mach eine Blume riesig – setz ihre Scale zum Beispiel auf 5, 5, 5!");
        }

        /// <summary>Färbt die Kronen eines Baum- oder Busch-Prefabs um (die Namen stammen aus den Prefabs).</summary>
        static void TintCrowns(GameObject tree, CrownStyle style)
        {
            string main;
            string accent;
            switch (style)
            {
                case CrownStyle.Autumn:
                    main = M.LeavesAutumn;
                    accent = M.SoftYellow;
                    break;
                case CrownStyle.Light:
                    main = M.LeavesLight;
                    accent = M.Leaves;
                    break;
                case CrownStyle.Blossom:
                    main = M.PinkBlossom;
                    accent = M.PinkBlossom;
                    break;
                default:
                    return;
            }
            SB.Recolor(tree, "Crown", main);
            SB.Recolor(tree, "Crown_3", main);
            SB.Recolor(tree, "Crown_Top", accent);
            SB.Recolor(tree, "Crown_2", accent);
            SB.Recolor(tree, "Leaves", main);
            SB.Recolor(tree, "Leaves_2", accent);
        }

        // ------------------------------------------------------------------ Windmühle (Nordosten)

        static void BuildWindmill(Transform parent)
        {
            Vector3 position = new Vector3(26f, 0f, 26f);
            // Vorderseite (lokal -z, mit Tür und Flügeln) schaut zum Platz.
            Transform mill = SB.Group("Windmill", parent, position, YawToward(position, Vector3.zero));

            SB.MeshObj(MK.Cylinder8, "Base", mill, Vector3.zero, new Vector3(4.2f, 0.35f, 4.2f), M.StoneGray, default, ColliderMode.ConvexMesh);
            // Der Turm wird nach oben schmaler: drei Stufen mit Holzbändern dazwischen.
            SB.MeshObj(MK.Cylinder8, "Tower_Lower", mill, new Vector3(0f, 0.35f, 0f), new Vector3(3.4f, 2.4f, 3.4f), M.WarmWhite, default,
                ColliderMode.ConvexMesh);
            SB.MeshObj(MK.Cylinder8, "Band_Lower", mill, new Vector3(0f, 2.75f, 0f), new Vector3(3.6f, 0.2f, 3.6f), M.LightWood);
            SB.MeshObj(MK.Cylinder8, "Tower_Middle", mill, new Vector3(0f, 2.95f, 0f), new Vector3(3.0f, 2.2f, 3.0f), M.WarmWhite, default,
                ColliderMode.ConvexMesh);
            SB.MeshObj(MK.Cylinder8, "Band_Upper", mill, new Vector3(0f, 5.15f, 0f), new Vector3(3.2f, 0.2f, 3.2f), M.LightWood);
            SB.MeshObj(MK.Cylinder8, "Tower_Upper", mill, new Vector3(0f, 5.35f, 0f), new Vector3(2.6f, 1.8f, 2.6f), M.WarmWhite, default,
                ColliderMode.ConvexMesh);
            SB.MeshObj(MK.Cone8, "Roof", mill, new Vector3(0f, 7.15f, 0f), new Vector3(3.3f, 2.5f, 3.3f), M.RoofRed);
            SB.MeshObj(MK.Sphere, "Roof_Tip", mill, new Vector3(0f, 9.7f, 0f), Vector3.one * 0.35f, M.SoftYellow);

            float lowerFace = Apothem8(3.4f);
            float middleFace = Apothem8(3.0f);
            float upperFace = Apothem8(2.6f);
            SB.Box("Door", mill, new Vector3(0f, 1.1f, -(lowerFace + 0.03f)), new Vector3(0.9f, 1.5f, 0.1f), M.DarkWood, default, false);

            // Fenster – sie leuchten nachts (Lamp Controller ohne eigenes Licht, wie beim Haus-Prefab).
            Renderer[] windows =
            {
                SB.Box("Window_Front", mill, new Vector3(0f, 4.05f, -(middleFace + 0.02f)), new Vector3(0.6f, 0.75f, 0.08f), M.WindowOff, default, false)
                    .GetComponent<Renderer>(),
                SB.Box("Window_Left", mill, new Vector3(-(lowerFace + 0.02f), 1.9f, 0f), new Vector3(0.08f, 0.7f, 0.55f), M.WindowOff, default, false)
                    .GetComponent<Renderer>(),
                SB.Box("Window_Right", mill, new Vector3(middleFace + 0.02f, 4.05f, 0f), new Vector3(0.08f, 0.7f, 0.55f), M.WindowOff, default, false)
                    .GetComponent<Renderer>(),
                SB.Box("Window_Back", mill, new Vector3(0f, 6.2f, upperFace + 0.02f), new Vector3(0.5f, 0.6f, 0.08f), M.WindowOff, default, false)
                    .GetComponent<Renderer>(),
            };
            LampController windowLights = mill.gameObject.AddComponent<LampController>();
            windowLights.Setup(null, windows, SB.Mat(M.WindowOn), SB.Mat(M.WindowOff), false);
            windowLights.FollowDayNight = true;

            // Achse nach vorne (-z) und die drehenden Flügel davor.
            const float hubHeight = 6.4f;
            SB.MeshObj(MK.Cylinder8, "Axle", mill, new Vector3(0f, hubHeight, -0.6f), new Vector3(0.4f, 1.6f, 0.4f), M.DarkWood,
                new Vector3(-90f, 0f, 0f));
            Transform blades = SB.Group("Blades", mill, new Vector3(0f, hubHeight, -2.2f));
            SB.MeshObj(MK.Sphere, "Blade_Hub", blades, Vector3.zero, Vector3.one * 0.75f, M.RoofRed);
            for (int i = 0; i < 4; i++)
            {
                float angle = 45f + i * 90f;
                Quaternion rotation = Quaternion.Euler(0f, 0f, angle);
                var euler = new Vector3(0f, 0f, angle);
                string prefix = "Blade_" + (i + 1);
                SB.Box(prefix + "_Arm", blades, rotation * new Vector3(0f, 2.35f, 0f), new Vector3(0.16f, 4.3f, 0.12f), M.DarkWood, euler, false);
                SB.Box(prefix + "_Sail", blades, rotation * new Vector3(0.5f, 2.7f, 0.04f), new Vector3(0.75f, 3.3f, 0.05f), M.WarmWhite, euler, false);
                SB.Box(prefix + "_Bar_1", blades, rotation * new Vector3(0.46f, 2.0f, -0.03f), new Vector3(0.84f, 0.07f, 0.06f), M.LightWood, euler, false);
                SB.Box(prefix + "_Bar_2", blades, rotation * new Vector3(0.46f, 3.4f, -0.03f), new Vector3(0.84f, 0.07f, 0.06f), M.LightWood, euler, false);
            }
            Rotator rotor = blades.gameObject.AddComponent<Rotator>();
            rotor.Speed = 40f;
            rotor.RotationAxis = Vector3.forward;
            rotor.IsRotating = true;

            SB.Note(mill.gameObject, "Die Windmühle",
                "Die Windmühle dreht sich mit einem Rotator-Skript – genau wie der Kristall auf dem Springbrunnen.\n" +
                "Das Skript sitzt auf dem Objekt 'Blades' (die Flügel). Probier: Ändere dort Speed oder Rotation Axis.\n" +
                "Nachts leuchten die Fenster.");
            SB.Note(blades.gameObject, "Die Flügel",
                "Der Rotator dreht die Flügel um ihre eigene Z-Achse – deshalb ist Rotation Axis (0, 0, 1).\n" +
                "Probier: Stell Speed auf 120 oder entferne den Haken bei 'Is Rotating'.");
        }

        // ------------------------------------------------------------------ Leuchtturm (Südwesten)

        static void BuildLighthouse(Transform parent, System.Random random)
        {
            Vector3 position = new Vector3(-27f, 0f, -27f);
            Transform tower = SB.Group("Lighthouse", parent, position, YawToward(position, Vector3.zero));

            SB.MeshObj(MK.Cylinder8, "Base", tower, Vector3.zero, new Vector3(4.4f, 0.5f, 4.4f), M.StoneGray, default, ColliderMode.ConvexMesh);

            // Gestreifter Turm: sechs Ringe, abwechselnd weiß und rot, nach oben schmaler (Oberkante 8.9 m).
            const int stripes = 6;
            const float stripeHeight = 1.4f;
            float y = 0.5f;
            for (int i = 0; i < stripes; i++)
            {
                float diameter = StripeDiameter(i);
                SB.MeshObj(MK.Cylinder8, "Tower_Stripe_" + (i + 1), tower, new Vector3(0f, y, 0f), new Vector3(diameter, stripeHeight, diameter),
                    i % 2 == 0 ? M.WarmWhite : M.CoralRed, default, ColliderMode.ConvexMesh);
                y += stripeHeight;
            }

            SB.Box("Door", tower, new Vector3(0f, 0.5f + 0.675f, -(Apothem8(StripeDiameter(0)) + 0.03f)), new Vector3(0.8f, 1.35f, 0.1f),
                M.DarkWood, default, false);
            SB.Box("Window_1", tower, new Vector3(0f, 0.5f + 2.5f * stripeHeight, -(Apothem8(StripeDiameter(2)) + 0.02f)),
                new Vector3(0.4f, 0.55f, 0.08f), M.Window, default, false);
            SB.Box("Window_2", tower, new Vector3(0f, 0.5f + 4.5f * stripeHeight, -(Apothem8(StripeDiameter(4)) + 0.02f)),
                new Vector3(0.4f, 0.55f, 0.08f), M.Window, default, false);

            // Umgang mit Geländer.
            float galleryBottom = y; // 8.9
            SB.MeshObj(MK.Cylinder8, "Gallery", tower, new Vector3(0f, galleryBottom, 0f), new Vector3(3.1f, 0.2f, 3.1f), M.Charcoal, default,
                ColliderMode.ConvexMesh);
            PolygonRing("Gallery_Rail", tower, new Vector3(0f, galleryBottom + 0.2f, 0f), 8, Apothem8(3.1f), 0.07f, 0.42f, M.Charcoal, false);

            // Laternenraum: vier dünne Pfosten, in der Mitte die Lampe (Bulb) mit dem Punktlicht.
            float lanternBottom = galleryBottom + 0.2f;          // 9.1
            float lanternFloor = lanternBottom + 0.2f;           // 9.3
            const float lanternHeight = 1.1f;
            float lanternTop = lanternFloor + lanternHeight;     // 10.4
            float bulbHeight = lanternFloor + lanternHeight * 0.5f;
            SB.MeshObj(MK.Cylinder8, "Lantern_Base", tower, new Vector3(0f, lanternBottom, 0f), new Vector3(1.9f, 0.2f, 1.9f), M.Charcoal);
            for (int i = 0; i < 4; i++)
            {
                Vector3 postPosition = Polar(45f + i * 90f, 0.75f, lanternFloor + lanternHeight * 0.5f);
                GameObject post = SB.Box("Lantern_Post_" + (i + 1), tower, postPosition, new Vector3(0.09f, lanternHeight, 0.09f), M.Charcoal,
                    new Vector3(0f, 45f, 0f), false);
                SB.NoShadows(post);
            }
            GameObject bulb = SB.MeshObj(MK.Sphere, "Bulb", tower, new Vector3(0f, bulbHeight, 0f), Vector3.one * 0.8f, M.LampOff);
            Renderer bulbRenderer = bulb.GetComponent<Renderer>();
            bulbRenderer.shadowCastingMode = ShadowCastingMode.Off;
            Light beacon = SB.PointLight("BeaconLight", tower, new Vector3(0f, bulbHeight, 0f), SB.Hex("#FFD18A"), 3f, 14f);
            beacon.enabled = false;

            GameObject lanternRoof = SB.MeshObj(MK.Cylinder8, "Lantern_Top", tower, new Vector3(0f, lanternTop, 0f), new Vector3(2.1f, 0.15f, 2.1f),
                M.Charcoal);
            lanternRoof.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
            SB.MeshObj(MK.Cone8, "Roof", tower, new Vector3(0f, lanternTop + 0.15f, 0f), new Vector3(2.0f, 1.3f, 2.0f), M.RoofRed);
            SB.MeshObj(MK.Sphere, "Roof_Tip", tower, new Vector3(0f, lanternTop + 1.5f, 0f), Vector3.one * 0.28f, M.Charcoal);

            LampController lamp = tower.gameObject.AddComponent<LampController>();
            lamp.Setup(beacon, new[] { bulbRenderer }, SB.Mat(M.LampOn), SB.Mat(M.LampOff), false);
            lamp.FollowDayNight = true;

            // Ein paar Felsen am Fuß (seitlich und hinten, nicht vor der Tür).
            SB.Place(P.Rock, tower, new Vector3(2.7f, 0f, 1.0f), Range(random, 0f, 360f), 1.4f, "Rock_Lighthouse_1");
            SB.Place(P.Rock, tower, new Vector3(-2.5f, 0f, 1.6f), Range(random, 0f, 360f), 1.1f, "Rock_Lighthouse_2");
            SB.Place(P.Rock, tower, new Vector3(1.0f, 0f, 2.8f), Range(random, 0f, 360f), 0.9f, "Rock_Lighthouse_3");

            SB.Note(tower.gameObject, "Der Leuchtturm",
                "Oben im Leuchtturm sitzt ein Point Light (Punktlicht) namens 'BeaconLight'.\n" +
                "Der Lamp Controller schaltet es bei Sonnenuntergang und in der Nacht automatisch an – genau wie bei den Laternen.\n" +
                "Probier: Stell die Nacht ein und ändere dann Color oder Range des Lichts.");
        }

        static float StripeDiameter(int index) => 3.3f - 0.2f * index;

        // ------------------------------------------------------------------ Steg mit Boot (Südosten)

        static void BuildPier(Transform parent)
        {
            // Beginnt bei ca. (24, 0, -30) auf dem Gras und führt 12 m hinaus übers Wasser.
            const float bearing = 141f;
            Vector3 start = Polar(bearing, 38.5f, 0f);
            // Lokal -z zeigt zum Platz, lokal +z hinaus aufs Meer.
            Transform pier = SB.Group("Pier", parent, start, YawToward(start, Vector3.zero));

            const float length = 12f;
            const float width = 2f;
            const float deckTop = 0.15f;
            const float deckThickness = 0.15f;
            SB.Box("Deck", pier, new Vector3(0f, deckTop - deckThickness * 0.5f, length * 0.5f), new Vector3(width, deckThickness, length), M.LightWood);
            const float edgeX = width * 0.5f + 0.07f;
            SB.Box("Edge_Left", pier, new Vector3(-edgeX, 0.06f, length * 0.5f), new Vector3(0.14f, 0.22f, length), M.DarkWood, default, false);
            SB.Box("Edge_Right", pier, new Vector3(edgeX, 0.06f, length * 0.5f), new Vector3(0.14f, 0.22f, length), M.DarkWood, default, false);

            // Pfosten stehen im Sand und im Wasser (auf dem Gras liegt der Steg direkt auf).
            float[] postDistances = { 4.2f, 6.8f, 9.4f, 11.8f };
            for (int i = 0; i < postDistances.Length; i++)
            {
                SB.MeshObj(MK.Cylinder8, "Post_L_" + (i + 1), pier, new Vector3(-edgeX, -1.3f, postDistances[i]), new Vector3(0.24f, 1.6f, 0.24f),
                    M.DarkWood);
                SB.MeshObj(MK.Cylinder8, "Post_R_" + (i + 1), pier, new Vector3(edgeX, -1.3f, postDistances[i]), new Vector3(0.24f, 1.6f, 0.24f),
                    M.DarkWood);
            }

            GameObject lamp = SB.Place(P.LampPost, pier, new Vector3(-0.6f, deckTop, 11.3f), 0f, 1f, "Lamp_Pier");
            BuildBoat(pier, new Vector3(2.5f, WaterLevel, 9.0f), 4f);

            SB.MarkStatic(pier.gameObject);
            ClearStatic(lamp); // Lampe wechselt nachts ihr Material – nicht statisch.

            SB.Note(pier.gameObject, "Der Steg",
                "Ein Holzsteg aus langen, flachen Würfeln. Die Pfosten stehen im Wasser.\n" +
                "Am Ende steht eine Laterne, die nachts leuchtet.");
        }

        /// <summary>Kleines Segelboot. Der Ursprung liegt auf der Wasserlinie.</summary>
        static void BuildBoat(Transform parent, Vector3 localPosition, float yaw)
        {
            Transform boat = SB.Group("Boat", parent, localPosition, yaw);
            // Rumpf von y -0.3 (unter Wasser) bis 0.35, vorne (+z) ein spitzer Bug.
            SB.Box("Hull", boat, new Vector3(0f, 0.025f, -0.5f), new Vector3(1.6f, 0.65f, 3.0f), M.CoralRed, default, false);
            SB.MeshObj(MK.Prism, "Hull_Bow", boat, new Vector3(0f, 0.025f, 1.0f), new Vector3(1.6f, 1.1f, 0.65f), M.CoralRed,
                new Vector3(90f, 0f, 0f));
            SB.Box("Hull_Stripe", boat, new Vector3(0f, 0.24f, -0.5f), new Vector3(1.64f, 0.1f, 3.02f), M.WarmWhite, default, false);
            SB.Box("Boat_Deck", boat, new Vector3(0f, 0.36f, -0.55f), new Vector3(1.4f, 0.04f, 2.7f), M.LightWood, default, false);
            SB.Box("Seat", boat, new Vector3(0f, 0.47f, -1.45f), new Vector3(1.4f, 0.18f, 0.4f), M.WarmWhite, default, false);

            // Mast, Baum und ein dreieckiges Segel (Keil-Mesh, sehr dünn).
            SB.MeshObj(MK.Cylinder8, "Mast", boat, new Vector3(0f, 0.38f, 0.25f), new Vector3(0.1f, 3.0f, 0.1f), M.WarmWhite);
            SB.Box("Boom", boat, new Vector3(0f, 0.95f, -0.5f), new Vector3(0.06f, 0.06f, 1.5f), M.LightWood, default, false);
            SB.MeshObj(MK.Wedge, "Sail", boat, new Vector3(0f, 1.0f, -0.51f), new Vector3(1.4f, 2.2f, 0.04f), M.WarmWhite, new Vector3(0f, 90f, 0f));
            SB.Box("Flag", boat, new Vector3(0f, 3.28f, 0.05f), new Vector3(0.02f, 0.18f, 0.32f), M.SoftYellow, default, false);

            SB.Note(boat.gameObject, "Das Boot",
                "Das Boot ist aus Würfeln, einem Prisma (der Bug) und einem Keil (das Segel) gebaut.\n" +
                "Es schwimmt nicht wirklich: Es steht einfach genau auf der Höhe des Wassers.\n" +
                "Probier: Färbe den Rumpf ('Hull' und 'Hull_Bow') um.");
        }

        // ------------------------------------------------------------------ Picknickplatz (Nordwesten)

        static void BuildPicnic(Transform parent, System.Random random)
        {
            Vector3 position = new Vector3(-23.5f, 0f, 23.5f);
            Transform picnic = SB.Group("Picnic", parent, position, YawToward(position, Vector3.zero));

            // Karierte Decke aus 4 flachen Würfeln (knapp über dem Gras).
            const float tile = 0.85f;
            const float tileHeight = 0.02f;
            const float tileTop = 0.025f;
            int tileNumber = 1;
            for (int ix = 0; ix < 2; ix++)
            {
                for (int iz = 0; iz < 2; iz++)
                {
                    bool red = (ix + iz) % 2 == 0;
                    GameObject blanket = SB.Box("Blanket_Tile_" + tileNumber++, picnic,
                        new Vector3((ix - 0.5f) * tile, tileTop - tileHeight * 0.5f, (iz - 0.5f) * tile),
                        new Vector3(tile, tileHeight, tile), red ? M.CoralRed : M.WarmWhite, default, false);
                    SB.NoShadows(blanket);
                }
            }

            // Korb mit Henkel (ein senkrecht gestellter Ring).
            SB.Box("Basket", picnic, new Vector3(0.42f, tileTop + 0.15f, 0.4f), new Vector3(0.5f, 0.3f, 0.36f), M.LightWood);
            SB.MeshObj(MK.Torus, "Basket_Handle", picnic, new Vector3(0.42f, tileTop + 0.3f, 0.4f), new Vector3(0.42f, 0.25f, 0.42f), M.DarkWood,
                new Vector3(90f, 0f, 0f));

            // Teller mit Apfel, noch ein Apfel und eine Flasche Limonade.
            GameObject plate = SB.MeshObj(MK.Cylinder12, "Plate", picnic, new Vector3(-0.4f, tileTop, -0.35f), new Vector3(0.34f, 0.02f, 0.34f),
                M.WarmWhite);
            SB.NoShadows(plate);
            SB.MeshObj(MK.Sphere, "Apple_Green", picnic, new Vector3(-0.4f, tileTop + 0.02f + 0.065f, -0.35f), Vector3.one * 0.13f, M.LeafGreen);
            SB.MeshObj(MK.Sphere, "Apple_Red", picnic, new Vector3(0.3f, tileTop + 0.065f, -0.35f), Vector3.one * 0.13f, M.CoralRed);
            SB.MeshObj(MK.Cylinder8, "Lemonade", picnic, new Vector3(-0.3f, tileTop, 0.35f), new Vector3(0.12f, 0.3f, 0.12f), M.SoftYellow);

            // Zwei Bäume als Schattenspender – einer davon blüht rosa.
            GameObject shadeTree = SB.Place(P.LargeTree, picnic, new Vector3(-0.5f, 0f, 3.2f), Range(random, 0f, 360f), 1f, "Tree_Picnic_1");
            TintCrowns(shadeTree, CrownStyle.Blossom);
            SB.Place(P.SmallTree, picnic, new Vector3(3.0f, 0f, 1.6f), Range(random, 0f, 360f), 1.1f, "Tree_Picnic_2");

            SB.MarkStatic(picnic.gameObject);
            SB.Note(picnic.gameObject, "Der Picknickplatz",
                "Eine Decke aus vier flachen Würfeln, ein Korb mit Henkel (ein Ring), Äpfel und Limonade.\n" +
                "Zwei Bäume spenden Schatten – einer blüht rosa.\n" +
                "Probier: Zieh ein Material aus Materials/Palette auf ein Feld der Decke.");
        }

        // ------------------------------------------------------------------ Wolken

        /// <summary>x, y (Höhe), z und Skalierung. Über dem Meer und hoch über der Insel – sie verdecken die Übersicht nicht.</summary>
        static readonly Vector4[] CloudSpots =
        {
            new Vector4(-50f, 24f, 22f, 1.4f),
            new Vector4(52f, 26f, 30f, 1.6f),
            new Vector4(-14f, 22f, 58f, 1.3f),
            new Vector4(20f, 23f, 66f, 1.5f),
            new Vector4(24f, 31f, 18f, 1.1f),
            new Vector4(-26f, 32f, 30f, 1.2f),
            new Vector4(-62f, 28f, -24f, 1.0f),
            new Vector4(60f, 25f, -30f, 0.9f),
            new Vector4(2f, 30f, 48f, 1.3f),
            new Vector4(-40f, 27f, 52f, 0.8f),
        };

        static void BuildClouds(Transform clouds, System.Random random)
        {
            for (int i = 0; i < CloudSpots.Length; i++)
            {
                Vector4 spot = CloudSpots[i];
                SB.Place(P.Cloud, clouds, new Vector3(spot.x, spot.y, spot.z), Range(random, 0f, 360f), spot.w, "Cloud_" + (i + 1));
            }
            SB.Note(clouds.gameObject, "Wolken",
                "Die Wolken schweben hoch über der Insel und dem Meer. Sie haben keinen Collider und werfen keinen Schatten.\n" +
                "Probier: Zieh eine Wolke tiefer (Position Y) oder mach sie größer (Scale).");
        }

        // ------------------------------------------------------------------ Strand

        static void BuildShore(Transform shore, System.Random random)
        {
            // Steine auf dem Sand und einer in der Brandung.
            PlaceShoreRock(shore, "Rock_Shore_1", 200f, 43.2f, BeachTop - 0.08f, random);
            PlaceShoreRock(shore, "Rock_Shore_2", 85f, 43.3f, BeachTop - 0.08f, random);
            PlaceShoreRock(shore, "Rock_Shore_3", 320f, 44.6f, WaterLevel - 0.05f, random);
            PlaceShoreRock(shore, "Rock_Shore_4", 268f, 43.4f, BeachTop - 0.08f, random);

            // Drei Palmen dort, wo der Strand am breitesten ist.
            BuildPalm(shore, "Palm_1", 27f, 43.2f, random);
            BuildPalm(shore, "Palm_2", 117f, 43.2f, random);
            BuildPalm(shore, "Palm_3", 297f, 43.2f, random);

            SB.MarkStatic(shore.gameObject);
            SB.Note(shore.gameObject, "Strand",
                "Palmen und Steine am Sandstrand.\n" +
                "Die Palmen sind aus schräg gestellten Zylindern (Stamm) und flach gedrückten Kugeln (Blätter) gebaut.");
        }

        static void PlaceShoreRock(Transform parent, string name, float bearing, float radius, float height, System.Random random)
        {
            SB.Place(P.Rock, parent, Polar(bearing, radius, height), Range(random, 0f, 360f), Range(random, 1.1f, 1.7f), name);
        }

        /// <summary>Eine Palme aus drei schrägen Stamm-Stücken, fünf Blättern und zwei Kokosnüssen. Sie neigt sich zum Meer.</summary>
        static void BuildPalm(Transform parent, string name, float bearing, float radius, System.Random random)
        {
            Vector3 basePosition = Polar(bearing, radius, BeachTop);
            // Lokal +z zeigt aufs Meer hinaus – dorthin neigt sich der Stamm.
            Transform palm = SB.Group(name, parent, basePosition, YawToward(basePosition, Vector3.zero) + Range(random, -20f, 20f));

            float[] tilts = { 4f, 11f, 19f };
            float[] widths = { 0.34f, 0.29f, 0.24f };
            const float segment = 1.15f;
            Vector3 point = Vector3.zero;
            for (int i = 0; i < tilts.Length; i++)
            {
                SB.MeshObj(MK.Cylinder8, "Trunk_" + (i + 1), palm, point, new Vector3(widths[i], segment + 0.08f, widths[i]), M.Trunk,
                    new Vector3(tilts[i], 0f, 0f));
                point += Quaternion.Euler(tilts[i], 0f, 0f) * new Vector3(0f, segment, 0f);
            }

            float leafOffset = Range(random, 0f, 72f);
            for (int k = 0; k < 5; k++)
            {
                Quaternion rotation = Quaternion.Euler(28f, leafOffset + k * 72f, 0f);
                SB.MeshObj(MK.BlobB, "Leaf_" + (k + 1), palm, point + rotation * new Vector3(0f, 0f, 0.95f), new Vector3(0.55f, 0.1f, 2.0f),
                    k % 2 == 0 ? M.LeafGreen : M.Leaves, rotation.eulerAngles);
            }
            SB.MeshObj(MK.Sphere, "Coconut_1", palm, point + new Vector3(0.13f, -0.14f, 0.05f), Vector3.one * 0.22f, M.DarkWood);
            SB.MeshObj(MK.Sphere, "Coconut_2", palm, point + new Vector3(-0.12f, -0.16f, -0.04f), Vector3.one * 0.22f, M.DarkWood);

            palm.localScale = Vector3.one * Range(random, 0.95f, 1.15f);
        }

        // ================================================================== Hilfsfunktionen

        /// <summary>Punkt auf einem Kreis um den Ursprung. bearing: 0 = Norden (+z), 90 = Osten (+x).</summary>
        static Vector3 Polar(float bearingDegrees, float radius, float y)
        {
            float radians = bearingDegrees * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(radians) * radius, y, Mathf.Cos(radians) * radius);
        }

        /// <summary>
        /// Drehung um Y (Grad), mit der die Vorderseite eines Objekts (lokal -z) von "from" aus auf "target" zeigt.
        /// (yaw θ bildet (0,0,-1) auf (-sin θ, 0, -cos θ) ab.)
        /// </summary>
        static float YawToward(Vector3 from, Vector3 target)
        {
            Vector3 direction = target - from;
            return Mathf.Atan2(-direction.x, -direction.z) * Mathf.Rad2Deg;
        }

        /// <summary>Abstand von der Mitte bis zur Mitte einer Seitenfläche bei einem 8-eckigen Zylinder (MK.Cylinder8).</summary>
        static float Apothem8(float diameter) => diameter * 0.5f * Mathf.Cos(22.5f * Mathf.Deg2Rad);

        static float Range(System.Random random, float min, float max) => min + (float)random.NextDouble() * (max - min);

        /// <summary>
        /// Ring aus Quadern entlang eines regelmäßigen Vielecks (passt genau auf MK.Cylinder8 / MK.Cylinder12 mit gleichem Außenmaß).
        /// center = Mitte der Unterkante (lokal zum parent).
        /// </summary>
        static Transform PolygonRing(string name, Transform parent, Vector3 center, int sides, float outerApothem, float thickness, float height,
            string material, bool keepCollider)
        {
            Transform ring = SB.Group(name, parent, center);
            float length = 2f * outerApothem * Mathf.Tan(Mathf.PI / sides);
            float middle = outerApothem - thickness * 0.5f;
            for (int i = 0; i < sides; i++)
            {
                // Seitenflächen der Low-Poly-Zylinder liegen bei 0°, 360°/sides, … gemessen von -z Richtung +x.
                float angle = i * 360f / sides;
                float radians = angle * Mathf.Deg2Rad;
                var position = new Vector3(Mathf.Sin(radians) * middle, height * 0.5f, -Mathf.Cos(radians) * middle);
                SB.Box(name + "_" + (i + 1).ToString("00"), ring, position, new Vector3(length, height, thickness), material,
                    new Vector3(0f, -angle, 0f), keepCollider);
            }
            return ring;
        }

        /// <summary>Nimmt einem Objekt (und seinen Kindern) die Static-Markierung wieder weg.</summary>
        static void ClearStatic(GameObject root)
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                GameObjectUtility.SetStaticEditorFlags(t.gameObject, (StaticEditorFlags)0);
        }
    }
}
