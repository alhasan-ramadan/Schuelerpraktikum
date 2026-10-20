using System.Collections.Generic;
using UnityEngine;

namespace StudentSandbox.EditorTools
{
    /// <summary>
    /// Baut den Bereich 05_LIGHT_ZONE ("Licht &amp; Schatten"):
    /// Dorfplatz mit Sonnenuhr, zwei Häuser, sechs Straßenlaternen, das Farben-Labor (Bühne mit drei farbigen Point Lights),
    /// das Schatten-Labor (Spot Light + Skulptur) und das Tageszeit-Pult mit drei Knöpfen.
    ///
    /// Alle Koordinaten sind LOKAL zur Zonen-Wurzel (Welt (25, 0, 0), nicht gedreht).
    /// Der Eingang liegt auf der -x-Seite: Der Besucher kommt vom zentralen Platz (Westen) und schaut nach Osten (+x).
    ///
    /// Höhen der flachen Beläge (nie zwei Flächen auf gleicher Höhe – kein Flimmern):
    ///   Boden 0.10 · Weg 0.11 · Weg-Kante 0.115 · Dorfplatz 0.12 · Platz-Ringe 0.125 · Platten-Rand 0.13 · Schatten-Platte 0.14 · Bühne 0.25
    /// </summary>
    public static class LightZoneBuilder
    {
        const float FloorTop = SandboxContext.ZoneFloorTop; // 0.1
        const float WalkwayTop = 0.11f;
        const float SquareTop = 0.12f;
        const float SquareRingTop = 0.125f;
        const float PlateTop = 0.14f;
        const float StageTop = 0.25f;

        /// <summary>Halber Abstand zwischen zwei gegenüberliegenden geraden Kanten des achteckigen Dorfplatzes.</summary>
        const float SquareApothem = 5f;

        static readonly Vector3 SquareCenter = new Vector3(0.5f, 0f, 0f);
        static readonly Vector3 PlateCenter = new Vector3(2.0f, 0f, -7.0f);

        // ------------------------------------------------------------------ Einstieg

        public static void Build(SandboxContext ctx)
        {
            Transform zone = ctx.LightZone;
            if (zone == null)
            {
                Debug.LogError("[Student Sandbox] LightZoneBuilder: ctx.LightZone fehlt – CreateRootGroups() wurde nicht aufgerufen.");
                return;
            }

            Transform environment = SB.Group("StaticEnvironment", zone);
            BuildEnvironment(environment);

            SB.EntranceArch("EntranceArch", zone, new Vector3(-11.3f, FloorTop, 0f), 90f, "LIGHT", "Licht & Schatten", M.Lavender);

            Transform village = SB.Group("Village", zone);
            BuildVillage(village);

            Transform streetLamps = SB.Group("StreetLamps", zone);
            BuildStreetLamps(streetLamps);

            Transform colorLab = SB.Group("ColorLab", zone);
            BuildColorLab(colorLab);

            Transform shadowLab = SB.Group("ShadowLab", zone);
            BuildShadowLab(shadowLab);

            Transform controls = SB.Group("Controls", zone);
            BuildControls(ctx, controls);

            Transform signs = SB.Group("Signs", zone);
            BuildSigns(signs);

            SB.Note(zone.gameObject, "Lichtlabor – Licht & Schatten",
                "Hier dreht sich alles um Licht:\n" +
                "• Dorfplatz: 6 Straßenlaternen (StreetLamp_1 bis 6) und 2 Häuser. Ihre Lichter gehen bei Sonnenuntergang und Nacht an.\n" +
                "• Farben-Labor (Bühne im Norden): 3 farbige Point Lights – Rot, Grün und Blau.\n" +
                "• Schatten-Labor (im Süden): ein Spot Light und eine Skulptur, die einen Schatten wirft.\n" +
                "• Tageszeit-Pult am Eingang: Knöpfe TAG, ABEND und NACHT.\n" +
                "Im Play Mode wechselt die Taste T die Tageszeit.");
        }

        // ------------------------------------------------------------------ StaticEnvironment

        static void BuildEnvironment(Transform env)
        {
            // Boden des Bereichs (mit Collider, damit Dinge darauf liegen bleiben).
            GameObject floor = SB.Box("Floor", env, new Vector3(0f, 0.05f, 0f), new Vector3(24f, 0.1f, 24f), M.LightFloor);
            SB.MarkStatic(floor);

            BuildTrim(env);
            BuildVillageSquare(env);
            BuildWalkway(env);
            BuildSundial(env);
        }

        /// <summary>Niedrige Randleiste in Lavendel mit 5 m Lücke am Eingang (x = -12, z von -2.5 bis 2.5).</summary>
        static void BuildTrim(Transform env)
        {
            Transform trim = SB.Group("Trim", env);
            const float height = 0.25f;
            const float thickness = 0.3f;
            const float gapHalf = 2.5f;
            float y = FloorTop + height * 0.5f;
            float edge = SandboxContext.ZoneHalfSize - thickness * 0.5f;   // 11.85
            float inner = SandboxContext.ZoneHalfSize - thickness;         // 11.7

            SB.Box("Trim_North", trim, new Vector3(0f, y, edge), new Vector3(24f, height, thickness), M.Lavender);
            SB.Box("Trim_South", trim, new Vector3(0f, y, -edge), new Vector3(24f, height, thickness), M.Lavender);
            SB.Box("Trim_East", trim, new Vector3(edge, y, 0f), new Vector3(thickness, height, inner * 2f), M.Lavender);

            float westLength = inner - gapHalf;
            float westCenter = gapHalf + westLength * 0.5f;
            SB.Box("Trim_West_North", trim, new Vector3(-edge, y, westCenter), new Vector3(thickness, height, westLength), M.Lavender);
            SB.Box("Trim_West_South", trim, new Vector3(-edge, y, -westCenter), new Vector3(thickness, height, westLength), M.Lavender);
            SB.MarkStatic(trim.gameObject);
        }

        /// <summary>Achteckiger Dorfplatz (Pflaster) mit Lavendel-Ring am Rand und hellem Ring um die Sonnenuhr.</summary>
        static void BuildVillageSquare(Transform env)
        {
            Transform square = SB.Group("VillageSquare", env);
            // Cylinder8: Ecken liegen auf Radius 0.5 * scale – so skalieren, dass die geraden Kanten 5 m vom Mittelpunkt entfernt sind.
            float scale = 2f * SquareApothem / Mathf.Cos(22.5f * Mathf.Deg2Rad);
            const float thickness = 0.04f;
            GameObject cobbles = SB.MeshObj(MK.Cylinder8, "Square_Cobblestones", square,
                new Vector3(SquareCenter.x, SquareTop - thickness, SquareCenter.z), new Vector3(scale, thickness, scale),
                M.PlazaLight, default, ColliderMode.ConvexMesh);
            SB.NoShadows(cobbles);

            OctagonRing("Square_Ring", square, SquareCenter, SquareApothem - 0.4f, 0.3f, SquareRingTop, M.Lavender);
            OctagonRing("Square_InnerRing", square, SquareCenter, 2.05f, 0.18f, SquareRingTop, M.PlazaAccent);
            SB.MarkStatic(square.gameObject);
        }

        /// <summary>Weg vom Eingangsbogen bis zum Dorfplatz (er schiebt sich ein Stück unter das Pflaster, damit keine Lücke bleibt).</summary>
        static void BuildWalkway(Transform env)
        {
            Transform walkway = SB.Group("Walkway", env);
            const float startX = -SandboxContext.ZoneHalfSize;
            float endX = SquareCenter.x - SquareApothem + 0.2f;   // -4.3: 20 cm unter dem Platz
            const float width = 3f;
            const float thickness = 0.02f;
            SB.Box("Walkway_Path", walkway, new Vector3((startX + endX) * 0.5f, WalkwayTop - thickness * 0.5f, 0f),
                new Vector3(endX - startX, thickness, width), M.Path, default, false);

            // Schmale Kanten links und rechts (enden kurz vor dem Platz).
            const float edgeWidth = 0.15f;
            const float edgeTop = 0.115f;
            float edgeEndX = SquareCenter.x - SquareApothem - 0.05f;
            float edgeZ = width * 0.5f + edgeWidth * 0.5f;
            foreach (float side in new[] { 1f, -1f })
            {
                SB.Box(side > 0f ? "Walkway_Edge_North" : "Walkway_Edge_South", walkway,
                    new Vector3((startX + edgeEndX) * 0.5f, edgeTop - thickness * 0.5f, side * edgeZ),
                    new Vector3(edgeEndX - startX, thickness, edgeWidth), M.Lavender, default, false);
            }
            SB.NoShadows(walkway.gameObject);
            SB.MarkStatic(walkway.gameObject);
        }

        /// <summary>Sonnenuhr in der Mitte des Dorfplatzes: Stufe, Sockel, Zifferblatt mit 12 Strichen und dreieckiger Zeiger (Gnomon).</summary>
        static void BuildSundial(Transform env)
        {
            Transform dial = SB.Group("Sundial", env, new Vector3(SquareCenter.x, SquareTop, SquareCenter.z));
            SB.MeshObj(MK.Cylinder8, "Sundial_Step", dial, new Vector3(0f, -0.02f, 0f), new Vector3(2.9f, 0.12f, 2.9f), M.StoneGray, default, ColliderMode.ConvexMesh);
            SB.MeshObj(MK.Cylinder12, "Sundial_Base", dial, new Vector3(0f, 0.1f, 0f), new Vector3(2.5f, 0.1f, 2.5f), M.Lavender, default, ColliderMode.ConvexMesh);
            SB.MeshObj(MK.Cylinder12, "Sundial_Face", dial, new Vector3(0f, 0.2f, 0f), new Vector3(2.2f, 0.08f, 2.2f), M.WarmWhite, default, ColliderMode.ConvexMesh);
            const float faceTop = 0.28f;

            // 12 Stundenstriche, jeder dritte etwas größer (wie bei einer Uhr).
            Transform marks = SB.Group("Hour_Marks", dial);
            for (int k = 0; k < 12; k++)
            {
                float angle = k * 30f;
                bool major = k % 3 == 0;
                float radius = major ? 0.86f : 0.9f;
                Vector3 direction = EdgeDirection(angle);
                Vector3 size = major ? new Vector3(0.07f, 0.012f, 0.2f) : new Vector3(0.035f, 0.012f, 0.12f);
                SB.Box("Hour_Mark_" + (k + 1), marks, direction * radius + new Vector3(0f, faceTop + 0.006f, 0f), size, M.Charcoal,
                    new Vector3(0f, -angle, 0f), false);
            }
            SB.NoShadows(marks.gameObject);

            // Der Zeiger: hohe Kante im Norden (+z), die Schräge fällt nach Süden ab – wie bei einer echten Sonnenuhr.
            SB.MeshObj(MK.Wedge, "Gnomon", dial, new Vector3(0f, faceTop, 0f), new Vector3(0.95f, 0.62f, 0.06f), M.Charcoal,
                new Vector3(0f, 90f, 0f), ColliderMode.ConvexMesh);

            SB.Note(dial.gameObject, "Sonnenuhr",
                "Eine Sonnenuhr: Dreh die Sonne (00_WORLD_ROOT/Sun) und beobachte den Schatten.\n" +
                "Der schwarze Zeiger (Gnomon) wirft seinen Schatten auf das Zifferblatt. Je nach Sonnenstand zeigt er auf einen anderen Strich.");
        }

        // ------------------------------------------------------------------ Village

        static void BuildVillage(Transform village)
        {
            Transform houses = SB.Group("Houses", village);
            GameObject lavenderHouse = SB.Place(P.SmallHouse, houses, new Vector3(8.5f, FloorTop, 3.4f), 90f, 1.15f, "Village_House_Lavender");
            SB.Recolor(lavenderHouse, "Roof", M.RoofPurple);
            SB.Recolor(lavenderHouse, "Door", M.BerryPurple);
            SB.Note(lavenderHouse, "Haus mit lila Dach",
                "Die Fenster leuchten bei Sonnenuntergang und Nacht. Das macht der Lamp Controller.\n" +
                "Probier: Zieh ein anderes Material aus Materials/Palette auf das Dach (Roof).");

            GameObject blueHouse = SB.Place(P.SmallHouse, houses, new Vector3(8.5f, FloorTop, -3.4f), 90f, 1.15f, "Village_House_Blue");
            SB.Recolor(blueHouse, "Roof", M.RoofBlue);
            SB.Recolor(blueHouse, "Door", M.SkyBlue);
            SB.Note(blueHouse, "Haus mit blauem Dach",
                "Schalte die Tageszeit auf Nacht: Dann leuchten die Fenster warm-gelb.\n" +
                "Im Lamp Controller kannst du den Haken bei Is On auch selbst setzen.");

            // Bänke auf dem Platz – alle schauen zur Sonnenuhr.
            Transform benches = SB.Group("Benches", village);
            PlaceFacing(P.Bench, benches, new Vector3(4.3f, SquareTop, 0f), SquareCenter, "Bench_Square_East");
            PlaceFacing(P.Bench, benches, new Vector3(-2.05f, SquareTop, 2.55f), SquareCenter, "Bench_Square_NorthWest");
            PlaceFacing(P.Bench, benches, new Vector3(-2.05f, SquareTop, -2.55f), SquareCenter, "Bench_Square_SouthWest");

            Transform plants = SB.Group("Plants", village);
            // Blumenbeete am Weg (Begrüßung) und zwischen den Häusern.
            SB.Place(P.FlowerBed, plants, new Vector3(-6.0f, FloorTop, 2.55f), 0f, 1f, "FlowerBed_Walkway_North");
            SB.Place(P.FlowerBed, plants, new Vector3(-6.0f, FloorTop, -2.55f), 180f, 1f, "FlowerBed_Walkway_South");
            SB.Place(P.FlowerBed, plants, new Vector3(9.0f, FloorTop, 0f), 90f, 1f, "FlowerBed_Alley");

            // Ein Blütenbaum zwischen den Häusern (hinter dem Beet) und ein paar Büsche in den Ecken.
            GameObject tree = SB.Place(P.SmallTree, plants, new Vector3(10.9f, FloorTop, 0f), 20f, 1.1f, "Tree_Blossom");
            SB.Recolor(tree, "Crown", M.PinkBlossom);
            SB.Recolor(tree, "Crown_Top", M.PinkBlossom);

            SB.Place(P.Bush, plants, new Vector3(11.0f, FloorTop, 4.7f), 15f, 0.8f, "Bush_1");
            SB.Place(P.Bush, plants, new Vector3(11.0f, FloorTop, -4.7f), 70f, 0.8f, "Bush_2");
            GameObject bush3 = SB.Place(P.Bush, plants, new Vector3(9.6f, FloorTop, 7.4f), 130f, 1f, "Bush_3");
            SB.Recolor(bush3, "Leaves", M.LeavesLight);
            SB.Place(P.Bush, plants, new Vector3(9.8f, FloorTop, -7.8f), 200f, 1f, "Bush_4");
            SB.Place(P.Bush, plants, new Vector3(-10.4f, FloorTop, 10.3f), 45f, 0.9f, "Bush_5");
            GameObject bush6 = SB.Place(P.Bush, plants, new Vector3(-10.4f, FloorTop, -10.3f), 300f, 0.9f, "Bush_6");
            SB.Recolor(bush6, "Leaves", M.LeavesLight);
        }

        // ------------------------------------------------------------------ StreetLamps

        static void BuildStreetLamps(Transform lamps)
        {
            Vector3[] positions =
            {
                new Vector3(-3.5f, FloorTop, 4.0f),
                new Vector3(4.5f, FloorTop, 4.0f),
                new Vector3(-3.5f, FloorTop, -4.0f),
                new Vector3(4.5f, FloorTop, -4.0f),
                new Vector3(-8.0f, FloorTop, 2.3f),
                new Vector3(-8.0f, FloorTop, -2.3f),
            };

            for (int i = 0; i < positions.Length; i++)
            {
                Vector3 position = positions[i];
                // Die vier Laternen am Platz schauen zur Sonnenuhr, die zwei am Weg zum Weg.
                Vector3 target = i < 4 ? SquareCenter : new Vector3(position.x, 0f, 0f);
                GameObject lamp = SB.Place(P.LampPost, lamps, position, YawTowards(position, target), 1f, "StreetLamp_" + (i + 1));
                if (i == 0)
                {
                    SB.Note(lamp, "Straßenlaterne",
                        "Eine Straßenlaterne: Sie hat ein Point Light (Kind-Objekt LampLight). Ändere dort Color und Intensity. " +
                        "Nachts geht sie automatisch an (Lamp Controller → Follow Day Night).");
                }
            }
        }

        // ------------------------------------------------------------------ ColorLab (Farben-Labor)

        static void BuildColorLab(Transform lab)
        {
            BuildStage(lab);

            // Weiße Gegenstände – auf Weiß sieht man farbiges Licht und bunte Schatten am besten.
            SB.MeshObj(MK.Cylinder8, "White_Sphere_Pedestal", lab, new Vector3(0f, StageTop, 8.6f), new Vector3(0.8f, 0.5f, 0.8f),
                M.StageWhite, default, ColliderMode.ConvexMesh);
            GameObject sphere = SB.Prim(PrimitiveType.Sphere, "White_Sphere", lab, new Vector3(0f, StageTop + 0.5f + 0.6f, 8.6f),
                Vector3.one * 1.2f, M.StageWhite);
            SB.Note(sphere, "Weiße Kugel",
                "Weiß wirft jede Lichtfarbe zurück. Darum siehst du hier rote, grüne und blaue Flecken.\n" +
                "Schau auch auf die Rückwand: Dort sind die Schatten bunt!");

            SB.Box("White_Cube", lab, new Vector3(-2.6f, StageTop + 0.5f, 8.2f), Vector3.one, M.StageWhite, new Vector3(0f, 25f, 0f));

            // Zylinder: Unitys CapsuleCollider ist an den Enden rund – ein konvexer MeshCollider passt genau zur Form.
            GameObject cylinder = SB.Prim(PrimitiveType.Cylinder, "White_Cylinder", lab, new Vector3(2.6f, StageTop + 0.8f, 8.2f),
                new Vector3(0.9f, 0.8f, 0.9f), M.StageWhite, default, false);
            MeshCollider cylinderCollider = cylinder.AddComponent<MeshCollider>();
            cylinderCollider.sharedMesh = cylinder.GetComponent<MeshFilter>().sharedMesh;
            cylinderCollider.convex = true;

            SB.Box("White_Pillar", lab, new Vector3(0f, StageTop + 1.1f, 9.8f), new Vector3(0.4f, 2.2f, 0.4f), M.StageWhite);

            // Die drei farbigen Point Lights (Rot, Grün, Blau) – ihre Ständer liegen in einer eigenen Gruppe.
            Transform stands = SB.Group("LightStands", lab);
            SB.Note(stands.gameObject, "Ständer für die Lichter",
                "Die Ständer gehören NICHT zu den Lichtern. So kannst du die farbigen Lichter (ColorLight_...) frei verschieben – " +
                "der Ständer bleibt einfach stehen.");
            BuildColorLight(lab, stands, "ColorLight_Red", "Red", new Vector3(-2.4f, 2.3f, 6.0f), new Color(1f, 0.15f, 0.15f), M.GlowRed);
            BuildColorLight(lab, stands, "ColorLight_Green", "Green", new Vector3(0f, 2.6f, 5.7f), new Color(0.15f, 1f, 0.25f), M.GlowGreen);
            BuildColorLight(lab, stands, "ColorLight_Blue", "Blue", new Vector3(2.4f, 2.3f, 6.0f), new Color(0.2f, 0.35f, 1f), M.GlowBlue);
        }

        /// <summary>Offene Bühne: weißer Boden, weiße Rückwand, Seitenwände (außen Lavendel, innen weiß), Dach mit Lämpchen-Leiste.</summary>
        static void BuildStage(Transform lab)
        {
            Transform stage = SB.Group("Stage", lab);

            // Bühnenboden: x von -5 bis 5, z von 5 bis 11, Oberkante 0.25.
            SB.Box("Stage_Floor", stage, new Vector3(0f, 0.15f, 8f), new Vector3(10f, 0.2f, 6f), M.StageWhite);
            SB.Box("Stage_Edge", stage, new Vector3(0f, 0.1675f, 4.98f), new Vector3(10f, 0.135f, 0.04f), M.Lavender, default, false);

            // Rückwand: 10 breit, 4 hoch (ab Bühnenboden), 0.3 dick. Oben steckt sie 2 cm im Dach.
            SB.Box("Backdrop", stage, new Vector3(0f, 2.225f, 10.75f), new Vector3(10f, 4.05f, 0.3f), M.StageWhite);

            const float roofBottom = 4.23f;
            foreach (float side in new[] { -1f, 1f })
            {
                string sideName = side < 0f ? "West" : "East";
                // Seitenwand (Lavendel) von z 7.5 bis 11, 3.5 hoch über der Bühne, mit weißer Innenverkleidung.
                SB.Box("SideWall_" + sideName, stage, new Vector3(side * 5f, 1.925f, 9.25f), new Vector3(0.3f, 3.65f, 3.5f), M.Lavender);
                SB.Box("SideWall_" + sideName + "_Inner", stage, new Vector3(side * 4.83f, 1.995f, 9.05f), new Vector3(0.04f, 3.49f, 3.1f),
                    M.StageWhite, default, false);
                // Vorderer Pfosten, der das Dach trägt.
                SB.Box("Stage_Post_" + sideName, stage, new Vector3(side * 5f, (FloorTop + roofBottom + 0.02f) * 0.5f, 7.25f),
                    new Vector3(0.3f, roofBottom + 0.02f - FloorTop, 0.5f), M.Lavender);
            }

            SB.Box("Stage_Roof", stage, new Vector3(0f, roofBottom + 0.15f, 9.05f), new Vector3(10.6f, 0.3f, 4.1f), M.Lavender);
            SB.Box("Stage_Roof_Fascia", stage, new Vector3(0f, roofBottom - 0.15f, 7.03f), new Vector3(10.6f, 0.3f, 0.12f), M.WarmWhite, default, false);

            // Lämpchen-Leiste am Dach: leuchtet bei Sonnenuntergang und Nacht (Lamp Controller ohne eigenes Licht).
            Transform marquee = SB.Group("Stage_Marquee", stage);
            var bulbs = new List<Renderer>();
            for (int i = 0; i < 7; i++)
            {
                float x = -4.5f + i * 1.5f;
                GameObject bulb = SB.MeshObj(MK.Sphere, "Bulb_" + (i + 1), marquee, new Vector3(x, roofBottom - 0.15f, 6.92f),
                    Vector3.one * 0.14f, M.LampOff);
                bulbs.Add(bulb.GetComponent<Renderer>());
            }
            SB.NoShadows(marquee.gameObject);
            LampController marqueeLamp = marquee.gameObject.AddComponent<LampController>();
            marqueeLamp.Setup(null, bulbs.ToArray(), SB.Mat(M.LampOn), SB.Mat(M.LampOff), false);
            marqueeLamp.FollowDayNight = true;
            SB.Note(marquee.gameObject, "Bühnen-Lämpchen",
                "Diese kleinen Glühbirnen gehen bei Sonnenuntergang und Nacht an. Das macht der Lamp Controller (Follow Day Night).\n" +
                "Probier: Setz den Haken bei Is On – dann leuchten sie sofort.");

            SB.Note(stage.gameObject, "Die Bühne",
                "Die Bühne ist weiß, damit du die Farben der Lichter gut siehst. Das Dach macht Schatten – so leuchten die Farben stärker.\n" +
                "Am schönsten ist es bei Sonnenuntergang oder Nacht.");
        }

        /// <summary>Ein farbiges Point Light mit leuchtender Glas-Kugel ("Lamp") und einem getrennten Ständer.</summary>
        static void BuildColorLight(Transform lab, Transform stands, string lightName, string colorName, Vector3 position, Color color,
            string glowMaterial)
        {
            Light light = SB.PointLight(lightName, lab, position, color, 5f, 8f, LightShadows.Soft);
            const float bulbDiameter = 0.35f;
            GameObject bulb = SB.MeshObj(MK.Sphere, "Lamp", light.transform, Vector3.zero, Vector3.one * bulbDiameter, glowMaterial);
            SB.Note(bulb, "Nur die Glas-Kugel",
                "Ich bin nur die leuchtende Glas-Kugel. Das Licht selbst ist mein Eltern-Objekt " + lightName + ". " +
                "Wähle " + lightName + " in der Hierarchy aus (eine Zeile über mir), um das Licht zu verschieben oder seine Farbe zu ändern.");
            SB.MeshObj(MK.Torus, "Lamp_Ring", light.transform, Vector3.zero, Vector3.one * 0.44f, M.Charcoal);
            // Die Glas-Kugel umschließt das Licht: Sie darf keinen Schatten werfen, sonst bliebe alles dunkel.
            SB.NoShadows(light.gameObject);

            SB.Note(light.gameObject, "Farbiges Licht (Point Light)",
                "Ein Point Light leuchtet in alle Richtungen wie eine Glühbirne. Ändere Color, Intensity oder Range – oder verschiebe mich. " +
                "Wo sich Rot, Grün und Blau mischen, entstehen neue Farben (Rot + Grün = Gelb).");

            // Ständer vom Bühnenboden bis knapp unter die Kugel.
            Transform stand = SB.Group("Stand_" + colorName, stands, new Vector3(position.x, StageTop, position.z));
            SB.MeshObj(MK.Cylinder8, "Stand_Base", stand, Vector3.zero, new Vector3(0.36f, 0.05f, 0.36f), M.Charcoal);
            float poleTop = position.y - StageTop - bulbDiameter * 0.5f + 0.015f;
            const float poleBottom = 0.05f;
            float poleHeight = poleTop - poleBottom;
            SB.Prim(PrimitiveType.Cylinder, "Stand_Pole", stand, new Vector3(0f, poleBottom + poleHeight * 0.5f, 0f),
                new Vector3(0.06f, poleHeight * 0.5f, 0.06f), M.Charcoal, default, false);
            SB.NoShadows(stand.gameObject);
        }

        // ------------------------------------------------------------------ ShadowLab (Schatten-Labor)

        static void BuildShadowLab(Transform lab)
        {
            // Weiße runde Platte mit Lavendel-Rand.
            GameObject rim = SB.MeshObj(MK.Cylinder12, "Shadow_Plate_Rim", lab, new Vector3(PlateCenter.x, 0.09f, PlateCenter.z),
                new Vector3(7.3f, PlateTop - 0.01f - 0.09f, 7.3f), M.Lavender);
            SB.NoShadows(rim);
            GameObject plate = SB.MeshObj(MK.Cylinder12, "Shadow_Plate", lab, new Vector3(PlateCenter.x, 0.09f, PlateCenter.z),
                new Vector3(7f, PlateTop - 0.09f, 7f), M.StageWhite, default, ColliderMode.ConvexMesh);
            SB.NoShadows(plate);
            SB.Note(plate, "Schatten-Platte",
                "Eine weiße Platte – darauf siehst du Schatten besonders gut. Stell eigene Objekte darauf und schau, was für Schatten sie werfen.");

            BuildShadowSculpture(lab);
            BuildShadowSpotLight(lab);
        }

        /// <summary>Skulptur mit spannender Silhouette: Würfel, schräge Säule, Ring und Stern (ca. 2.6 m hoch).</summary>
        static void BuildShadowSculpture(Transform lab)
        {
            Transform sculpture = SB.Group("ShadowSculpture", lab, new Vector3(PlateCenter.x, PlateTop, PlateCenter.z));

            const float baseSize = 0.55f;
            SB.Box("Sculpture_Base", sculpture, new Vector3(0f, baseSize * 0.5f, 0f), Vector3.one * baseSize, M.WarmWhite, new Vector3(0f, 15f, 0f));

            // Schräge Säule: steht 5 cm tief im Würfel und neigt sich 14° nach Norden (+z).
            const float tilt = 14f;
            const float columnLength = 0.9f;
            Vector3 columnFoot = new Vector3(0f, baseSize - 0.05f, 0f);
            Vector3 columnAxis = Quaternion.Euler(tilt, 0f, 0f) * Vector3.up;
            SB.Box("Sculpture_Column", sculpture, columnFoot + columnAxis * (columnLength * 0.5f), new Vector3(0.22f, columnLength, 0.22f),
                M.SoftYellow, new Vector3(tilt, 0f, 0f));
            Vector3 columnTop = columnFoot + columnAxis * columnLength;

            // Ring (steht senkrecht, schaut zur Lampe im Westen): sitzt auf der Säule.
            const float ringScale = 0.76f;
            float ringOuterRadius = 0.5f * ringScale;
            Vector3 ringCenter = columnTop + new Vector3(0f, ringOuterRadius - 0.05f, 0f);
            SB.MeshObj(MK.Torus, "Sculpture_Ring", sculpture, ringCenter, Vector3.one * ringScale, M.WarmWhite,
                new Vector3(0f, 0f, 90f), ColliderMode.ConvexMesh);

            // Stern oben auf dem Ring (in derselben Ebene wie der Ring).
            const float starScale = 0.55f;
            float starFootOffset = 0.162f;                                   // seitlicher Abstand der unteren Zacken
            float ringTopAtFeet = ringCenter.y + Mathf.Sqrt(ringOuterRadius * ringOuterRadius - starFootOffset * starFootOffset);
            float starCenterY = ringTopAtFeet - 0.02f + 0.405f * starScale;  // untere Zacken liegen bei -0.405 * scale
            SB.MeshObj(MK.Star, "Sculpture_Star", sculpture, new Vector3(0f, starCenterY, ringCenter.z),
                new Vector3(starScale, starScale, 0.11f), M.SoftYellow, new Vector3(0f, 90f, 0f), ColliderMode.ConvexMesh);

            SB.Note(sculpture.gameObject, "Schatten-Skulptur",
                "Ich werfe einen Schatten. Verschiebe oder drehe die ShadowSpotLight-Lampe (oder die Sonne) und beobachte, wie sich mein Schatten verändert.");
        }

        /// <summary>Spot Light auf einem Ständer, schräg auf die Skulptur gerichtet. Der Ständer ist ein getrenntes Objekt.</summary>
        static void BuildShadowSpotLight(Transform lab)
        {
            Vector3 lightPosition = new Vector3(-2.5f, 4.6f, -7.0f);
            Vector3 aimPoint = new Vector3(PlateCenter.x, 0.5f, PlateCenter.z);
            Vector3 euler = Quaternion.LookRotation(aimPoint - lightPosition, Vector3.up).eulerAngles;
            Light spot = SB.SpotLight("ShadowSpotLight", lab, lightPosition, euler, new Color(1f, 0.92f, 0.75f), 18f, 14f, 60f, LightShadows.Soft);

            // Lampenkopf: Das Licht sitzt im Ursprung, +z ist die Leuchtrichtung.
            Transform head = spot.transform;
            Vector3 alongZ = new Vector3(90f, 0f, 0f); // dreht die y-Achse der Sockel-Meshes auf +z
            SB.MeshObj(MK.Cylinder8, "Lamp_Body", head, new Vector3(0f, 0f, -0.6f), new Vector3(0.44f, 0.62f, 0.44f), M.Charcoal, alongZ);
            SB.MeshObj(MK.Cylinder8, "Lamp_Back", head, new Vector3(0f, 0f, -0.68f), new Vector3(0.3f, 0.08f, 0.3f), M.Lavender, alongZ);
            SB.MeshObj(MK.Cylinder12, "Lamp_Lens", head, new Vector3(0f, 0f, 0.02f), new Vector3(0.36f, 0.03f, 0.36f), M.LampOn, alongZ);
            SB.MeshObj(MK.Torus, "Lamp_Rim", head, new Vector3(0f, 0f, 0.035f), Vector3.one * 0.46f, M.Charcoal, alongZ);
            SB.NoShadows(spot.gameObject);

            SB.Note(spot.gameObject, "Spot Light (Scheinwerfer)",
                "Ein Spot Light leuchtet wie eine Taschenlampe in eine Richtung. Ändere Rotation (Drehen-Werkzeug E) und den Lichtkegel (im Inspector: Inner / Outer Spot Angle). " +
                "Tipp: Nachts siehst du den Schatten am besten.");

            // Ständer mit Gabel (Bügel) – getrennt vom Licht, damit man das Licht frei drehen und verschieben kann.
            Transform pole = SB.Group("ShadowSpotLight_Pole", lab, new Vector3(lightPosition.x, FloorTop, lightPosition.z));
            float pivotY = lightPosition.y - FloorTop;   // Höhe des Lichts über dem Boden (4.5)
            const float poleBottom = 0.08f;
            float poleTop = pivotY - 0.3f;
            SB.MeshObj(MK.Cylinder8, "Pole_Base", pole, Vector3.zero, new Vector3(0.56f, poleBottom, 0.56f), M.Charcoal);
            SB.Prim(PrimitiveType.Cylinder, "Pole", pole, new Vector3(0f, (poleBottom + poleTop) * 0.5f, 0f),
                new Vector3(0.1f, (poleTop - poleBottom) * 0.5f, 0.1f), M.Charcoal, default, false);
            SB.Box("Yoke_Bottom", pole, new Vector3(0f, poleTop + 0.03f, 0f), new Vector3(0.1f, 0.06f, 0.66f), M.Charcoal, default, false);
            float armBottom = poleTop + 0.06f;
            float armTop = pivotY + 0.1f;
            foreach (float side in new[] { -1f, 1f })
            {
                string sideName = side < 0f ? "South" : "North";
                SB.Box("Yoke_Arm_" + sideName, pole, new Vector3(0f, (armBottom + armTop) * 0.5f, side * 0.29f),
                    new Vector3(0.08f, armTop - armBottom, 0.05f), M.Charcoal, default, false);
                // Kleine Schraube am Drehpunkt (außen an der Gabel).
                float boltStart = side > 0f ? 0.315f : -0.355f;
                SB.MeshObj(MK.Cylinder8, "Yoke_Bolt_" + sideName, pole, new Vector3(0f, pivotY, boltStart), new Vector3(0.1f, 0.04f, 0.1f),
                    M.Lavender, alongZ);
            }
            SB.NoShadows(pole.gameObject);
        }

        // ------------------------------------------------------------------ Controls (Tageszeit-Pult)

        static void BuildControls(SandboxContext ctx, Transform controls)
        {
            BuildTimeConsole(controls);

            ClickableSwitch day = SB.PushButton("DayButton", controls, new Vector3(-8.5f, FloorTop, -4.8f), 90f, "Tag", M.ButtonYellow, "TAG");
            ClickableSwitch sunset = SB.PushButton("SunsetButton", controls, new Vector3(-8.5f, FloorTop, -6.0f), 90f, "Sonnenuntergang", M.ButtonOrange, "ABEND");
            ClickableSwitch night = SB.PushButton("NightButton", controls, new Vector3(-8.5f, FloorTop, -7.2f), 90f, "Nacht", M.ButtonBlue, "NACHT");

            if (ctx.DayNight != null)
            {
                SB.Listen(day.OnPressed, ctx.DayNight.SetDay);
                SB.Listen(sunset.OnPressed, ctx.DayNight.SetSunset);
                SB.Listen(night.OnPressed, ctx.DayNight.SetNight);
            }
            else
            {
                Debug.LogWarning("[Student Sandbox] LightZoneBuilder: Kein DayNightController vorhanden – die Tageszeit-Knöpfe bleiben ohne Funktion.");
            }

            SB.Note(day.gameObject, "Knopf: Tag",
                "Klicke im Play Mode auf diese Knöpfe. Im Editor geht es über 08_MANAGERS/DayNightController.\n" +
                "Unter On Pressed siehst du, was beim Klicken passiert: DayNightController → SetDay.");
            SB.Note(sunset.gameObject, "Knopf: Sonnenuntergang",
                "Klick im Play Mode: Die Sonne steht tief, der Himmel wird orange und die Laternen gehen an.\n" +
                "Unter On Pressed steht: DayNightController → SetSunset.");
            SB.Note(night.gameObject, "Knopf: Nacht",
                "Klick im Play Mode: Es wird Nacht. Jetzt leuchten Laternen, Fenster und die farbigen Lichter auf der Bühne besonders schön.\n" +
                "Unter On Pressed steht: DayNightController → SetNight.");
        }

        /// <summary>Niedriges Pult unter den drei Knöpfen, Titel "TAGESZEIT" vorne (Richtung -x) und Symbole über den Knöpfen.</summary>
        static void BuildTimeConsole(Transform controls)
        {
            Vector3 origin = new Vector3(-8.6f, FloorTop, -6.0f);
            Transform console = SB.Group("TimeConsole", controls, origin);

            // Pult-Körper: x von -9.45 bis -7.75, z von -7.95 bis -4.05, Oberkante 0.53 (die Knopf-Säulen ragen hindurch).
            const float deskHeight = 0.4f;
            const float panelHeight = 0.03f;
            SB.Box("Console_Desk", console, new Vector3(0f, deskHeight * 0.5f, 0f), new Vector3(1.7f, deskHeight, 3.9f), M.Lavender);
            SB.Box("Console_Top", console, new Vector3(0f, deskHeight + panelHeight * 0.5f, 0f), new Vector3(1.6f, panelHeight, 3.8f),
                M.TechPanel, default, false);
            float deskTop = deskHeight + panelHeight;

            // Titel-Schild vorne am Pult (zeigt nach -x, zum Eingang).
            const float plateThickness = 0.04f;
            float plateX = -0.85f - plateThickness * 0.5f;
            SB.Box("Console_TitlePlate", console, new Vector3(plateX, 0.21f, 0f), new Vector3(plateThickness, 0.28f, 2.2f), M.WarmWhite, default, false);
            SB.Text("Console_Title", "TAGESZEIT", console, new Vector3(plateX - plateThickness * 0.5f - 0.006f, 0.21f, 0f),
                new Vector3(0f, 90f, 0f), 0.2f, SB.Hex("#2E3440"));

            // Symbole auf kurzen Stangen hinter den Knöpfen: Sonne, halbe Sonne, Mond.
            const float iconX = 0.63f;
            BuildSunIcon(console, new Vector3(iconX, deskTop, 1.2f));
            BuildSunsetIcon(console, new Vector3(iconX, deskTop, 0f));
            BuildMoonIcon(console, new Vector3(iconX, deskTop, -1.2f));

            SB.Note(console.gameObject, "Tageszeit-Pult",
                "Drei Knöpfe für die Tageszeit: TAG, ABEND und NACHT.\n" +
                "Jeder Knopf hat ein Event (On Pressed), das den DayNightController aufruft. Die Symbole zeigen, was passiert.");
        }

        static Transform IconPost(string name, Transform parent, Vector3 basePosition, float postHeight)
        {
            Transform icon = SB.Group(name, parent, basePosition);
            SB.Prim(PrimitiveType.Cylinder, "Post", icon, new Vector3(0f, postHeight * 0.5f, 0f), new Vector3(0.06f, postHeight * 0.5f, 0.06f),
                M.Charcoal, default, false);
            return icon;
        }

        /// <summary>Strahlen in der y-z-Ebene (das Symbol schaut nach -x). angle 0 = nach oben.</summary>
        static void Ray(Transform icon, string name, Vector3 center, float angle, float radius, Vector3 size, string material)
        {
            float rad = angle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(0f, Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
            SB.Box(name, icon, center + offset, size, material, new Vector3(angle, 0f, 0f), false);
        }

        static void BuildSunIcon(Transform console, Vector3 basePosition)
        {
            const float postHeight = 0.99f;
            Transform icon = IconPost("Icon_Sun", console, basePosition, postHeight);
            Vector3 center = new Vector3(0f, postHeight + 0.16f, 0f);
            SB.MeshObj(MK.Sphere, "Sun_Symbol", icon, center, Vector3.one * 0.32f, M.GlowSun);
            for (int i = 0; i < 8; i++)
                Ray(icon, "Ray_" + (i + 1), center, 22.5f + i * 45f, 0.25f, new Vector3(0.04f, 0.09f, 0.04f), M.GlowSun);
        }

        static void BuildSunsetIcon(Transform console, Vector3 basePosition)
        {
            const float postHeight = 0.93f;
            Transform icon = IconPost("Icon_Sunset", console, basePosition, postHeight);
            const float horizonHeight = 0.17f;
            Vector3 center = new Vector3(0f, postHeight + horizonHeight, 0f);
            // Die untere Hälfte der Sonne steckt im "Horizont" – sie geht gerade unter.
            SB.Box("Horizon", icon, new Vector3(0f, postHeight + horizonHeight * 0.5f, 0f), new Vector3(0.38f, horizonHeight, 0.52f),
                M.BerryPurple, default, false);
            SB.MeshObj(MK.Sphere, "HalfSun_Symbol", icon, center, Vector3.one * 0.34f, M.NeonOrange);
            float[] angles = { -55f, 0f, 55f };
            for (int i = 0; i < angles.Length; i++)
                Ray(icon, "Ray_" + (i + 1), center, angles[i], 0.27f, new Vector3(0.04f, 0.09f, 0.04f), M.NeonOrange);
        }

        static void BuildMoonIcon(Transform console, Vector3 basePosition)
        {
            const float postHeight = 0.99f;
            Transform icon = IconPost("Icon_Moon", console, basePosition, postHeight);
            Vector3 center = new Vector3(0f, postHeight + 0.15f, 0f);
            SB.MeshObj(MK.Sphere, "Moon_Symbol", icon, center, Vector3.one * 0.3f, M.SkyBlue);
            // Sterne schauen nach -x (yaw 90), damit man sie vom Eingang aus sieht.
            SB.MeshObj(MK.Star, "Star_1", icon, center + new Vector3(0f, 0.22f, -0.2f), new Vector3(0.15f, 0.15f, 0.05f), M.GlowSun,
                new Vector3(0f, 90f, 0f));
            SB.MeshObj(MK.Star, "Star_2", icon, center + new Vector3(0f, -0.12f, -0.26f), new Vector3(0.1f, 0.1f, 0.04f), M.GlowSun,
                new Vector3(0f, 90f, 0f));
        }

        // ------------------------------------------------------------------ Signs

        static void BuildSigns(Transform signs)
        {
            SB.Sign("Sign_ColorLab", signs, new Vector3(-6.8f, FloorTop, 6.5f), 90f, "FARBEN-LABOR", "Rot + Grün + Blau = ?", M.Lavender, 3.2f);
            SB.Sign("Sign_ShadowLab", signs, new Vector3(-6.8f, FloorTop, -6.5f), 90f, "SCHATTEN-LABOR", "Licht + Objekt = Schatten", M.Lavender, 3.2f);
        }

        // ------------------------------------------------------------------ Hilfsfunktionen

        /// <summary>
        /// Richtung (x-z-Ebene) zur Mitte der k-ten geraden Kante eines Cylinder8/12-Meshes.
        /// angle 0 = -z (dort liegt bei diesen Meshes eine gerade Kante), 90 = +x.
        /// </summary>
        static Vector3 EdgeDirection(float angle)
        {
            float rad = angle * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(rad), 0f, -Mathf.Cos(rad));
        }

        /// <summary>
        /// Flacher achteckiger Ring aus 8 dünnen Leisten (passend zur Ausrichtung von MK.Cylinder8).
        /// apothem = Abstand der Leisten-Mitte vom Zentrum, topY = Oberkante. Ohne Collider und ohne Schattenwurf.
        /// </summary>
        static void OctagonRing(string name, Transform parent, Vector3 center, float apothem, float width, float topY, string material)
        {
            Transform ring = SB.Group(name, parent);
            const float thickness = 0.01f;
            float outer = apothem + width * 0.5f;
            float length = 2f * outer * Mathf.Tan(22.5f * Mathf.Deg2Rad);   // äußere Ecken treffen sich genau
            for (int k = 0; k < 8; k++)
            {
                float angle = k * 45f;
                Vector3 position = center + EdgeDirection(angle) * apothem;
                position.y = topY - thickness * 0.5f;
                SB.Box(name + "_" + (k + 1), ring, position, new Vector3(length, thickness, width), material, new Vector3(0f, -angle, 0f), false);
            }
            SB.NoShadows(ring.gameObject);
        }

        /// <summary>yaw so, dass die Vorderseite (lokal -z) von "from" nach "to" zeigt (siehe Yaw-Regel in der SPEC).</summary>
        static float YawTowards(Vector3 from, Vector3 to)
        {
            Vector3 d = to - from;
            return Mathf.Atan2(-d.x, -d.z) * Mathf.Rad2Deg;
        }

        static GameObject PlaceFacing(string prefabName, Transform parent, Vector3 position, Vector3 lookAt, string instanceName)
        {
            return SB.Place(prefabName, parent, position, YawTowards(position, lookAt), 1f, instanceName);
        }
    }
}
