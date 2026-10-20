using UnityEngine;
using UnityEngine.Rendering;

namespace StudentSandbox.EditorTools
{
    /// <summary>
    /// Prefabs für die Umgebung: Wolke, Blumenbeet und Wegweiser.
    /// Regeln: Pivot unten in der Mitte, "vorne" = lokal -z.
    /// </summary>
    public static partial class SandboxPrefabs
    {
        static partial void BuildEnvironmentPrefabs()
        {
            BuildCloud();
            BuildFlowerBed();
            BuildSignpost();
        }

        // ================================================================== Wolke

        static void BuildCloud()
        {
            GameObject root = NewRoot(P.Cloud);
            Transform t = root.transform;

            // Fünf weiche, flach gedrückte Formen. Keine Collider, keine Schatten (die Wolke schwebt hoch über der Insel).
            GameObject[] puffs =
            {
                SB.MeshObj(MK.BlobA, "Puff_1", t, new Vector3(-0.3f, 1.0f, 0f), new Vector3(3.0f, 1.8f, 2.6f), M.Cloud, new Vector3(0f, 10f, 0f)),
                SB.MeshObj(MK.BlobB, "Puff_2", t, new Vector3(-2.1f, 0.78f, 0.3f), new Vector3(2.2f, 1.4f, 2.0f), M.Cloud, new Vector3(0f, -25f, 0f)),
                SB.MeshObj(MK.BlobC, "Puff_3", t, new Vector3(1.45f, 0.85f, -0.2f), new Vector3(2.4f, 1.5f, 2.2f), M.Cloud, new Vector3(0f, 40f, 0f)),
                SB.MeshObj(MK.BlobB, "Puff_4", t, new Vector3(-0.8f, 1.5f, 0.45f), new Vector3(1.9f, 1.25f, 1.7f), M.Cloud, new Vector3(0f, 70f, 0f)),
                SB.MeshObj(MK.BlobA, "Puff_5", t, new Vector3(2.45f, 0.6f, 0.5f), new Vector3(1.5f, 1.0f, 1.4f), M.Cloud, new Vector3(0f, -60f, 0f)),
            };
            foreach (GameObject puff in puffs)
            {
                Renderer renderer = RendererOf(puff);
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }

            SB.Note(root, "Wolke",
                "Eine Wolke aus fünf weichen Formen (Puff_1 bis Puff_5) – ohne Collider und ohne Schatten. " +
                "Probier: Verschiebe sie am Himmel oder mach sie mit Scale größer.");
            Save(root, P.Cloud);
        }

        // ================================================================== Blumenbeet

        static void BuildFlowerBed()
        {
            GameObject root = NewRoot(P.FlowerBed);
            Transform t = root.transform;

            // Holzkasten 2.2 × 1.1 m, Bretter 0.35 m hoch, Erde bis 0.3 m.
            SB.Box("Frame_Front", t, new Vector3(0f, 0.175f, -0.5f), new Vector3(2.2f, 0.35f, 0.1f), M.LightWood, default, false);
            SB.Box("Frame_Back", t, new Vector3(0f, 0.175f, 0.5f), new Vector3(2.2f, 0.35f, 0.1f), M.LightWood, default, false);
            SB.Box("Frame_Left", t, new Vector3(-1.05f, 0.175f, 0f), new Vector3(0.1f, 0.35f, 0.9f), M.LightWood, default, false);
            SB.Box("Frame_Right", t, new Vector3(1.05f, 0.175f, 0f), new Vector3(0.1f, 0.35f, 0.9f), M.LightWood, default, false);
            int corner = 1;
            foreach (float x in new[] { -1.05f, 1.05f })
            {
                foreach (float z in new[] { -0.5f, 0.5f })
                {
                    SB.Box("Corner_Post_" + corner, t, new Vector3(x, 0.2f, z), new Vector3(0.14f, 0.4f, 0.14f), M.DarkWood, default, false);
                    corner++;
                }
            }
            SB.Box("Soil", t, new Vector3(0f, 0.15f, 0f), new Vector3(2.0f, 0.3f, 0.9f), M.DarkWood, default, false);

            // Grüne Blätter-Polster auf der Erde.
            SB.MeshObj(MK.BlobA, "Foliage_1", t, new Vector3(-0.62f, 0.33f, 0.02f), new Vector3(0.55f, 0.2f, 0.4f), M.LeafGreen, new Vector3(0f, 20f, 0f));
            SB.MeshObj(MK.BlobB, "Foliage_2", t, new Vector3(0.05f, 0.33f, -0.05f), new Vector3(0.6f, 0.2f, 0.42f), M.LeafGreen, new Vector3(0f, -15f, 0f));
            SB.MeshObj(MK.BlobC, "Foliage_3", t, new Vector3(0.66f, 0.33f, 0.04f), new Vector3(0.5f, 0.18f, 0.38f), M.LeafGreen, new Vector3(0f, 45f, 0f));

            // Sieben Blüten in gemischten Farben (Stiele starten auf der Erde bei y 0.3).
            AddFlowerStem(t, 1, new Vector3(-0.82f, 0.3f, -0.17f), new Vector3(-5f, 0f, 6f), 0.2f, M.PinkBlossom, 0.17f);
            AddFlowerStem(t, 2, new Vector3(-0.52f, 0.3f, 0.2f), new Vector3(6f, 0f, -4f), 0.24f, M.SoftYellow, 0.17f);
            AddFlowerStem(t, 3, new Vector3(-0.22f, 0.3f, -0.18f), new Vector3(-7f, 0f, -5f), 0.21f, M.Lavender, 0.16f);
            AddFlowerStem(t, 4, new Vector3(0.1f, 0.3f, 0.17f), new Vector3(5f, 0f, 7f), 0.24f, M.CoralRed, 0.17f);
            AddFlowerStem(t, 5, new Vector3(0.38f, 0.3f, -0.14f), new Vector3(-4f, 0f, -7f), 0.2f, M.PinkBlossom, 0.16f);
            AddFlowerStem(t, 6, new Vector3(0.66f, 0.3f, 0.21f), new Vector3(7f, 0f, 3f), 0.23f, M.WarmOrange, 0.17f);
            AddFlowerStem(t, 7, new Vector3(0.86f, 0.3f, -0.2f), new Vector3(-6f, 0f, -6f), 0.19f, M.SoftYellow, 0.16f);

            AddBoxCollider(root, new Vector3(0f, 0.3f, 0f), new Vector3(2.2f, 0.6f, 1.1f));
            SB.Note(root, "Blumenbeet",
                "Ein Holzkasten mit Erde (Soil) und bunten Blüten (Blossom_1 bis Blossom_7). " +
                "Probier: Färbe die Blüten mit Materialien aus Materials/Palette um oder stelle mehrere Beete an einen Weg.");
            Save(root, P.FlowerBed);
        }

        // ================================================================== Wegweiser

        static void BuildSignpost()
        {
            GameObject root = NewRoot(P.Signpost);
            Transform t = root.transform;

            SB.MeshObj(MK.Cylinder8, "Base", t, Vector3.zero, new Vector3(0.45f, 0.16f, 0.45f), M.StoneGray, default, ColliderMode.ConvexMesh);
            SB.MeshObj(MK.Cylinder8, "Pole", t, Vector3.zero, new Vector3(0.18f, 3.05f, 0.18f), M.LightWood, default, ColliderMode.ConvexMesh);
            SB.MeshObj(MK.Cylinder8, "Pole_Cap", t, new Vector3(0f, 3.05f, 0f), new Vector3(0.26f, 0.06f, 0.26f), M.DarkWood);
            // Stern oben drauf: steht mit seinen zwei unteren Zacken auf der Kappe (y 3.11).
            SB.MeshObj(MK.Star, "Top_Star", t, new Vector3(0f, 3.29f, 0f), new Vector3(0.44f, 0.44f, 0.1f), M.SoftYellow);

            // Vier Pfeile in die vier Himmelsrichtungen (wie die Bereiche auf der Insel liegen).
            // Das Pfeil-Mesh zeigt nach +x; yaw 180 → -x, yaw -90 → +z, yaw 90 → -z.
            BuildSignBoard(t, "Board_Light", "LIGHT", M.Lavender, 0f, 2.6f, 2.3f);
            BuildSignBoard(t, "Board_Creative", "CREATIVE", M.MintGreen, 180f, 2.1f, 2.3f);
            BuildSignBoard(t, "Board_Physics", "PHYSICS", M.WarmOrange, -90f, 1.6f, 2.3f);
            BuildSignBoard(t, "Board_Interactions", "INTERACTIONS", M.Turquoise, 90f, 1.1f, 2.6f);

            SB.Note(root, "Wegweiser",
                "Die vier Pfeile zeigen zu den vier Bereichen der Insel. " +
                "Probier: Wähle ein Schild aus (z.B. Board_Light → Text_Front) und ändere in der Text Mesh-Komponente den Text. " +
                "Ändere auch Text_Front_Shadow (der dunkle Schatten dahinter), sonst schimmert das alte Wort durch.");
            Save(root, P.Signpost);
        }

        /// <summary>
        /// Ein Pfeil-Schild am Pfahl. Gruppe mit yaw (unskaliert), darin der skalierte Pfeil und die Schrift auf beiden Seiten.
        /// Der Pfeil beginnt im Pfahl und zeigt in lokaler +x-Richtung nach außen.
        /// </summary>
        static void BuildSignBoard(Transform parent, string name, string label, string material, float yaw, float height, float length)
        {
            const float boardHeight = 0.45f;
            const float thickness = 0.08f;
            const float tailX = -0.05f;

            Transform board = SB.Group(name, parent, new Vector3(0f, height, 0f), yaw);
            SB.MeshObj(MK.ArrowBoard, "Arrow", board, new Vector3(tailX + length * 0.5f, 0f, 0f), new Vector3(length, boardHeight, thickness), material);

            // Schrift im rechteckigen Teil des Pfeils (vom Pfahl bis zum Beginn der Spitze bei 72 % der Länge).
            const float textStart = 0.12f;
            float textEnd = tailX + length * 0.72f;
            float textX = (textStart + textEnd) * 0.5f;
            float available = textEnd - textStart - 0.1f;
            // Grobe Schätzung: ein Großbuchstabe ist ca. 0.68 × so breit wie hoch.
            float letterHeight = Mathf.Min(0.28f, available / (label.Length * 0.68f));
            float faceZ = thickness * 0.5f + 0.006f;
            AddShadowedText(board, "Text_Front", label, new Vector3(textX, 0f, -faceZ), 0f, letterHeight);
            AddShadowedText(board, "Text_Back", label, new Vector3(textX, 0f, faceZ), 180f, letterHeight);
        }

        /// <summary>Weiße Schrift mit einem dunklen, leicht versetzten Schatten dahinter – gut lesbar auch auf hellen Farben.</summary>
        static void AddShadowedText(Transform parent, string name, string text, Vector3 position, float yaw, float letterHeight)
        {
            var euler = new Vector3(0f, yaw, 0f);
            // Schatten etwas nach rechts unten und 3 mm näher am Brett (aus Sicht des Lesers).
            Vector3 shadowOffset = Quaternion.Euler(euler) * new Vector3(0.012f, -0.012f, 0.003f);
            SB.Text(name + "_Shadow", text, parent, position + shadowOffset, euler, letterHeight, new Color(0.18f, 0.2f, 0.25f, 0.6f));
            SB.Text(name, text, parent, position, euler, letterHeight, Color.white);
        }
    }
}
