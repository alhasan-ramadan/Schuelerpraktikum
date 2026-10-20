using UnityEngine;

namespace StudentSandbox.EditorTools
{
    /// <summary>
    /// Prefabs für den Bereich "Schalter &amp; Verhalten": Knopf, drehender Kristall und fahrende Plattform.
    /// Regeln: Pivot unten in der Mitte, "vorne" = lokal -z.
    /// </summary>
    public static partial class SandboxPrefabs
    {
        static partial void BuildInteractionPrefabs()
        {
            BuildPushButton();
            BuildRotatingCrystal();
            BuildMovingPlatformSet();
        }

        // ================================================================== Knopf

        static void BuildPushButton()
        {
            // Der Knopf selbst ist die Wurzel: ClickableSwitch sitzt direkt auf dem Prefab-Objekt.
            ClickableSwitch button = SB.PushButton(P.PushButton, null, Vector3.zero, 0f, "Knopf", M.ButtonRed, "KNOPF");
            GameObject root = button.gameObject;

            SB.Note(root, "Knopf",
                "Ein Knopf, den du im Play Mode anklicken kannst. Was beim Klicken passiert, steht im Clickable Switch unter \"On Pressed\". " +
                "Probier: Klick dort auf +, zieh ein Objekt in das leere Feld und wähle eine Funktion aus (z.B. Rotator → ToggleRotation).");
            Save(root, P.PushButton);
        }

        // ================================================================== Drehender Kristall

        static void BuildRotatingCrystal()
        {
            GameObject root = NewRoot(P.RotatingCrystal);
            Transform t = root.transform;

            // Sockel (mit Collidern).
            SB.MeshObj(MK.Cylinder8, "Pedestal_Base", t, Vector3.zero, new Vector3(1.4f, 0.15f, 1.4f), M.Charcoal, default, ColliderMode.ConvexMesh);
            SB.MeshObj(MK.Cylinder8, "Pedestal", t, new Vector3(0f, 0.15f, 0f), new Vector3(1.1f, 0.8f, 1.1f), M.TechPanel, default, ColliderMode.ConvexMesh);
            SB.MeshObj(MK.Cylinder8, "Neon_Ring", t, new Vector3(0f, 0.7f, 0f), new Vector3(1.16f, 0.08f, 1.16f), M.NeonTeal);
            SB.MeshObj(MK.Cylinder8, "Pedestal_Top", t, new Vector3(0f, 0.95f, 0f), new Vector3(1.24f, 0.08f, 1.24f), M.Charcoal, default, ColliderMode.ConvexMesh);
            GameObject glowPlate = SB.MeshObj(MK.Cylinder8, "Pedestal_Glow", t, new Vector3(0f, 1.03f, 0f), new Vector3(0.8f, 0.012f, 0.8f), M.NeonTeal);
            CastNoShadow(glowPlate);

            // Schwebender Kristall: 1.12 m hoch (y 1.54 … 2.66). Collider, damit man ihn anklicken kann.
            // Kristall und Ring umgeben das Licht – sie werfen deshalb keinen Schatten.
            const float crystalY = 2.1f;
            GameObject crystal = SB.MeshObj(MK.Crystal, "Crystal", t, new Vector3(0f, crystalY, 0f), Vector3.one * 0.7f, M.Crystal, default, ColliderMode.ConvexMesh);
            CastNoShadow(crystal);
            Rotator crystalRotator = crystal.AddComponent<Rotator>();
            crystalRotator.Speed = 45f;
            crystalRotator.RotationAxis = Vector3.up;
            crystalRotator.IsRotating = true;

            // Ring: außen 2 m, innen ca. 1.2 m – groß genug, dass er sich um die Kristall-Spitzen herum überschlagen kann
            // (tiefster Punkt 1.1 m, also über dem Sockel).
            GameObject ring = SB.MeshObj(MK.Torus, "Orbit_Ring", t, new Vector3(0f, crystalY, 0f), new Vector3(2.0f, 1.2f, 2.0f), M.NeonPurple, new Vector3(20f, 0f, 0f));
            CastNoShadow(ring);
            Rotator ringRotator = ring.AddComponent<Rotator>();
            ringRotator.Speed = 30f;
            ringRotator.RotationAxis = new Vector3(1f, 0f, 0f);
            ringRotator.IsRotating = true;

            SB.PointLight("CrystalGlow", t, new Vector3(0f, crystalY, 0f), SB.Hex("#B98CFF"), 2f, 4f, LightShadows.None);

            SB.Note(root, "Drehender Kristall",
                "Der Kristall (Crystal) und der Ring (Orbit_Ring) drehen sich mit dem Rotator-Skript. " +
                "Probier: Wähle Crystal aus und ändere Speed (z.B. 90) oder Rotation Axis (z.B. 1, 0, 0).");
            Save(root, P.RotatingCrystal);
        }

        // ================================================================== Fahrende Plattform

        static void BuildMovingPlatformSet()
        {
            GameObject root = NewRoot(P.MovingPlatformSet);
            Transform t = root.transform;

            var pointAPosition = new Vector3(-3f, 0.6f, 0f);
            var pointBPosition = new Vector3(3f, 0.6f, 0f);

            // Dunkle Fahrspur auf dem Boden (y 0.005 … 0.015, unter den Markierungen bei y 0.02 … 0.04).
            GameObject track = SB.Box("Track", t, new Vector3(0f, 0.01f, 0f), new Vector3(6f, 0.01f, 0.7f), M.Charcoal, default, false);
            CastNoShadow(track);

            // Start- und Endpunkt: eigene Objekte NEBEN der Plattform (nicht ihre Kinder), auf Höhe der Plattform-Mitte.
            Transform pointA = BuildPlatformPoint(t, "PointA", pointAPosition, "A", -1f);
            Transform pointB = BuildPlatformPoint(t, "PointB", pointBPosition, "B", 1f);

            // Die Plattform: Wurzel ohne Skalierung (Rigidbody, Collider, Skript), das Aussehen steckt in den Kindern.
            Transform platform = SB.Group("MovingPlatform", t, pointAPosition);
            GameObject platformObject = platform.gameObject;
            SB.Box("Deck", platform, Vector3.zero, new Vector3(2.5f, 0.3f, 2.5f), M.TechPanel, default, false);
            SB.Box("Trim", platform, Vector3.zero, new Vector3(2.56f, 0.08f, 2.56f), M.NeonTeal, default, false);
            SB.Box("Stripe_Front", platform, new Vector3(0f, 0.155f, -1.05f), new Vector3(2.3f, 0.01f, 0.12f), M.NeonTeal, default, false);
            SB.Box("Stripe_Back", platform, new Vector3(0f, 0.155f, 1.05f), new Vector3(2.3f, 0.01f, 0.12f), M.NeonTeal, default, false);

            BoxCollider deckCollider = AddBoxCollider(platformObject, Vector3.zero, new Vector3(2.5f, 0.3f, 2.5f));
            deckCollider.sharedMaterial = SB.PhysMat(PM.Normal);

            Rigidbody body = platformObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            body.interpolation = RigidbodyInterpolation.Interpolate;

            MovingPlatform mover = platformObject.AddComponent<MovingPlatform>();
            mover.PointA = pointA;
            mover.PointB = pointB;
            mover.Speed = 2f;
            mover.WaitTime = 1f;
            mover.IsMoving = true;

            SB.Note(root, "Fahrende Plattform",
                "Die Plattform (MovingPlatform) fährt im Play Mode zwischen PointA und PointB hin und her. " +
                "Probier: Verschiebe PointB, um die Strecke zu ändern, oder ändere bei MovingPlatform Speed und Wait Time.");
            Save(root, P.MovingPlatformSet);
        }

        /// <summary>
        /// Ein Wegpunkt der Plattform: leeres Objekt auf Höhe der Plattform-Mitte mit einer leuchtenden Bodenscheibe
        /// und einem kleinen Buchstaben-Schild außen am Ende der Strecke (Schrift auf beiden Seiten).
        /// </summary>
        static Transform BuildPlatformPoint(Transform parent, string name, Vector3 position, string letter, float outwardSide)
        {
            Transform point = SB.Group(name, parent, position);
            float ground = -position.y;

            GameObject marker = SB.MeshObj(MK.Cylinder12, "Marker", point, new Vector3(0f, ground + 0.02f, 0f), new Vector3(1.3f, 0.02f, 1.3f), M.NeonTeal);
            CastNoShadow(marker);
            SB.Note(marker, "Nur die Bodenmarkierung",
                "Ich bin nur die leuchtende Scheibe am Boden. Der eigentliche Punkt ist mein Eltern-Objekt " + name + ". " +
                "Wähle " + name + " in der Hierarchy aus und verschiebe ihn – dann fährt die Plattform eine neue Strecke.");

            float labelX = outwardSide * 1.6f;
            SB.Box("Label_Post", point, new Vector3(labelX, ground + 0.475f, 0f), new Vector3(0.08f, 0.95f, 0.08f), M.Charcoal, default, false);
            SB.Box("Label_Plate", point, new Vector3(labelX, ground + 1.12f, 0f), new Vector3(0.44f, 0.44f, 0.06f), M.WarmWhite, default, false);
            Color textColor = SB.Hex("#2E3440");
            SB.Text("Label_Front", letter, point, new Vector3(labelX, ground + 1.12f, -0.036f), Vector3.zero, 0.34f, textColor);
            SB.Text("Label_Back", letter, point, new Vector3(labelX, ground + 1.12f, 0.036f), new Vector3(0f, 180f, 0f), 0.34f, textColor);
            return point;
        }
    }
}
