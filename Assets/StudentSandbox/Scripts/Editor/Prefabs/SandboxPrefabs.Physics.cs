using UnityEngine;

namespace StudentSandbox.EditorTools
{
    /// <summary>
    /// Prefabs für das Physik-Labor: Kugeln, Mauer, Rampen, Bumper, Hindernis-Würfel, Zielfeld und Dominostein.
    /// Regeln: Pivot unten in der Mitte (Ausnahme: die Kugeln, siehe <see cref="BuildBall"/>), "vorne" = lokal -z.
    /// </summary>
    public static partial class SandboxPrefabs
    {
        static partial void BuildPhysicsPrefabs()
        {
            BuildPhysicsBall();
            BuildBouncyBall();
            BuildWallBlock();
            BuildSmallRamp();
            BuildTallRamp();
            BuildBumper();
            BuildObstacleCube();
            BuildTargetZone();
            BuildDomino();
        }

        // ================================================================== Kugeln

        static void BuildPhysicsBall()
        {
            BuildBall(P.PhysicsBall, 0.8f, M.CoralRed, PM.Normal, 1f, "Kugel", "Physik-Kugel",
                "Die Kugel ist ein einzelnes Objekt: Transform, Mesh Filter, Mesh Renderer, Sphere Collider, Rigidbody, Physics Ball. " +
                "Der Rigidbody sorgt dafür, dass sie fällt und rollt. " +
                "Probier: Ändere Mass oder Scale und drücke dann Play. Taste R bringt sie zurück an den Start.");
        }

        static void BuildBouncyBall()
        {
            BuildBall(P.BouncyBall, 0.7f, M.SoftYellow, PM.Bouncy, 0.5f, "Flummi", "Flummi",
                "Ein Flummi: Sein Sphere Collider hat das Physik-Material \"Bouncy\" – deshalb springt er hoch. " +
                "Probier: Zieh ihn im Editor nach oben und drücke Play. " +
                "Tausche danach im Sphere Collider das Material gegen \"Normal\" und vergleiche.");
        }

        /// <summary>
        /// AUSNAHME von der Pivot-Regel: Die Kugel IST das Wurzel-Objekt (ein einzelnes GameObject mit allen Komponenten).
        /// Deshalb liegt ihr Pivot in der Mitte der Kugel und nicht unten. Wer sie auf den Boden legen will,
        /// setzt sie auf Bodenhöhe + Radius (PhysicsBall: 0.4, BouncyBall: 0.35).
        /// </summary>
        static void BuildBall(string prefabName, float diameter, string material, string physicsMaterial, float mass,
            string displayName, string noteTitle, string noteText)
        {
            GameObject root = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            root.name = prefabName;
            root.transform.localScale = Vector3.one * diameter;
            root.GetComponent<Renderer>().sharedMaterial = SB.Mat(material);
            root.GetComponent<SphereCollider>().sharedMaterial = SB.PhysMat(physicsMaterial);

            Rigidbody body = root.AddComponent<Rigidbody>();
            body.mass = mass;
            body.linearDamping = 0.05f;
            body.angularDamping = 0.4f;
            body.useGravity = true;
            body.isKinematic = false;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            PhysicsBall ball = root.AddComponent<PhysicsBall>();
            ball.DisplayName = displayName;

            SB.Note(root, noteTitle, noteText);
            Save(root, prefabName);
        }

        // ================================================================== Mauer und Rampen

        static void BuildWallBlock()
        {
            GameObject root = NewRoot(P.WallBlock);
            Transform t = root.transform;

            SB.Box("Block", t, new Vector3(0f, 0.5f, 0f), new Vector3(3f, 1f, 0.4f), M.StoneGray);
            // Orangefarbener Streifen kurz unter der Oberkante (ragt 1 cm heraus, ohne Collider).
            SB.Box("Stripe", t, new Vector3(0f, 0.84f, 0f), new Vector3(3.02f, 0.1f, 0.42f), M.WarmOrange, default, false);

            SB.SetPhysicsMaterial(root, PM.Normal);
            SB.Note(root, "Mauerstück",
                "Eine feste Mauer ohne Rigidbody: Sie bewegt sich nicht, aber Kugeln prallen von ihr ab. " +
                "Probier: Verschiebe sie (W) oder drehe sie (E), um eine Kugel umzulenken.");
            Save(root, P.WallBlock);
        }

        static void BuildSmallRamp()
        {
            GameObject root = NewRoot(P.SmallRamp);
            Transform t = root.transform;

            const float length = 2f;
            const float height = 0.6f;
            const float depth = 1.6f;
            // Keil: hohe Kante bei lokal -x, die Schräge fällt nach +x ab.
            SB.MeshObj(MK.Wedge, "Surface", t, Vector3.zero, new Vector3(length, height, depth), M.LightWood, default, ColliderMode.ConvexMesh);

            // Orangefarbene Kanten-Streifen an beiden Seiten entlang der Schräge (nur Deko).
            float angle = Mathf.Atan2(height, length) * Mathf.Rad2Deg;
            float slopeLength = Mathf.Sqrt(length * length + height * height);
            var middle = new Vector3(0f, height * 0.5f, 0f);
            SB.Box("Stripe_Front", t, middle + new Vector3(0f, 0f, -(depth * 0.5f + 0.022f)), new Vector3(slopeLength, 0.07f, 0.04f), M.WarmOrange, new Vector3(0f, 0f, -angle), false);
            SB.Box("Stripe_Back", t, middle + new Vector3(0f, 0f, depth * 0.5f + 0.022f), new Vector3(slopeLength, 0.07f, 0.04f), M.WarmOrange, new Vector3(0f, 0f, -angle), false);

            SB.SetPhysicsMaterial(root, PM.Normal);
            SB.Note(root, "Kleine Rampe",
                "Eine flache Rampe. Die hohe Kante liegt gegenüber vom roten Pfeil (x-Achse) – in Pfeilrichtung geht es bergab. " +
                "Probier: Drehe sie mit Rotation Y und lass eine Kugel darüber rollen.");
            Save(root, P.SmallRamp);
        }

        static void BuildTallRamp()
        {
            GameObject root = NewRoot(P.TallRamp);
            Transform t = root.transform;

            const float length = 4.5f;
            const float height = 2f;
            const float depth = 2f;
            SB.MeshObj(MK.Wedge, "Surface", t, Vector3.zero, new Vector3(length, height, depth), M.LightWood, default, ColliderMode.ConvexMesh);

            // Richtung entlang der Schräge (nach unten) und Normale der Schräge (nach oben/außen).
            float angleRad = Mathf.Atan2(height, length);
            float angle = angleRad * Mathf.Rad2Deg;
            float slopeLength = Mathf.Sqrt(length * length + height * height);
            var down = new Vector3(Mathf.Cos(angleRad), -Mathf.Sin(angleRad), 0f);
            var normal = new Vector3(Mathf.Sin(angleRad), Mathf.Cos(angleRad), 0f);
            var middle = new Vector3(0f, height * 0.5f, 0f);
            var tilt = new Vector3(0f, 0f, -angle);

            // Geländer auf beiden Längskanten der Schräge: ragen 25 cm über die Fläche und halten die Kugel auf der Bahn.
            const float railHeight = 0.3f;
            const float railThickness = 0.1f;
            Vector3 railCenter = middle + normal * (railHeight * 0.5f - 0.05f);
            float railZ = depth * 0.5f - railThickness * 0.5f;
            SB.Box("Rail_Front", t, railCenter + new Vector3(0f, 0f, -railZ), new Vector3(slopeLength, railHeight, railThickness), M.WarmOrange, tilt);
            SB.Box("Rail_Back", t, railCenter + new Vector3(0f, 0f, railZ), new Vector3(slopeLength, railHeight, railThickness), M.WarmOrange, tilt);

            // Gelbe Startlinie oben auf der Schräge (knapp über der Fläche, ohne Collider).
            var topEdge = new Vector3(-length * 0.5f, height, 0f);
            Vector3 startLine = topEdge + down * 0.55f + normal * 0.008f;
            SB.Box("Start_Line", t, startLine, new Vector3(0.1f, 0.008f, depth - railThickness * 2f - 0.1f), M.SoftYellow, tilt, false);

            // Orangefarbenes Band an der hohen Rückwand.
            SB.Box("Back_Stripe", t, new Vector3(-length * 0.5f - 0.012f, height - 0.2f, 0f), new Vector3(0.03f, 0.18f, depth + 0.02f), M.WarmOrange, default, false);

            SB.SetPhysicsMaterial(root, PM.Normal);
            SB.Note(root, "Große Rampe",
                "Eine hohe Rampe mit Geländer (Rail_Front, Rail_Back), damit die Kugel nicht herunterfällt. " +
                "Leg eine Kugel oben auf die Startlinie und drücke Play. " +
                "Probier: Mach die Rampe mit Scale X länger – dann wird sie flacher und die Kugel langsamer.");
            Save(root, P.TallRamp);
        }

        // ================================================================== Bumper

        static void BuildBumper()
        {
            GameObject root = NewRoot(P.Bumper);
            Transform t = root.transform;

            SB.MeshObj(MK.Cylinder12, "Base", t, Vector3.zero, new Vector3(1.2f, 0.25f, 1.2f), M.Charcoal, default, ColliderMode.ConvexMesh);

            // Der Kopf pulsiert beim Treffer (Bumper.Visual). Ring und Deckel sind Kinder des Kopfes und pulsieren mit.
            // Ihre lokalen Werte sind durch die Skalierung des Kopfes geteilt, damit sie in Metern die gewünschte Größe haben.
            var headScale = new Vector3(0.95f, 0.5f, 0.95f);
            const float headBottom = 0.25f;
            GameObject head = SB.MeshObj(MK.Cylinder12, "Head", t, new Vector3(0f, headBottom, 0f), headScale, M.CoralRed, default, ColliderMode.ConvexMesh);
            Transform h = head.transform;
            // Ring: außen 1.2 m, 0.2 m hoch, auf halber Kopfhöhe (Welt y 0.5).
            SB.MeshObj(MK.Torus, "Ring", h, new Vector3(0f, (0.5f - headBottom) / headScale.y, 0f),
                new Vector3(1.2f / headScale.x, 1.0f / headScale.y, 1.2f / headScale.z), M.WarmWhite, default, ColliderMode.ConvexMesh);
            // Deckel oben: 0.6 m breit, 0.08 m hoch (Welt y 0.75 … 0.83).
            SB.MeshObj(MK.Cylinder12, "Cap", h, new Vector3(0f, 1f, 0f),
                new Vector3(0.6f / headScale.x, 0.08f / headScale.y, 0.6f / headScale.z), M.WarmWhite);

            SB.SetPhysicsMaterial(root, PM.Bouncy);

            // Kinematischer Rigidbody: Der Bumper bewegt sich nicht, aber alle Collider der Kinder gehören zu ihm.
            // Nur so bekommt das Bumper-Skript auf der Wurzel die Zusammenstöße mit (OnCollisionEnter).
            Rigidbody body = root.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            Bumper bumper = root.AddComponent<Bumper>();
            bumper.Visual = h;
            bumper.BounceStrength = 5f;

            SB.Note(root, "Bumper",
                "Ein Flipper-Pilz: Trifft ihn eine Kugel, schubst er sie kräftig weg. " +
                "Probier: Ändere im Bumper-Skript \"Bounce Strength\" (0 = kein Schubs, 15 = sehr stark). " +
                "Der Rigidbody ist \"Is Kinematic\": Der Bumper bleibt stehen, merkt aber jeden Zusammenstoß.");
            Save(root, P.Bumper);
        }

        // ================================================================== Hindernis-Würfel, Zielfeld, Dominostein

        static void BuildObstacleCube()
        {
            GameObject root = NewRoot(P.ObstacleCube);
            SB.Box("Cube", root.transform, new Vector3(0f, 0.3f, 0f), Vector3.one * 0.6f, M.SkyBlue);
            SB.SetPhysicsMaterial(root, PM.Normal);

            // Rigidbody auf der Wurzel + Collider am Kind = ein zusammengesetzter Physik-Körper.
            Rigidbody body = root.AddComponent<Rigidbody>();
            body.mass = 0.5f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            root.AddComponent<ResettableObject>();

            SB.Note(root, "Hindernis-Würfel",
                "Ein leichter Würfel mit Rigidbody (Mass 0.5): Kugeln können ihn wegschubsen. Taste R stellt ihn zurück. " +
                "Probier: Mach ihn schwer (Mass 20) – was passiert jetzt?");
            Save(root, P.ObstacleCube);
        }

        static void BuildTargetZone()
        {
            GameObject root = NewRoot(P.TargetZone);
            Transform t = root.transform;

            // Unsichtbarer Trigger-Bereich (zuerst der Collider, dann GoalZone – GoalZone braucht einen Collider).
            BoxCollider trigger = AddBoxCollider(root, new Vector3(0f, 0.6f, 0f), new Vector3(2.2f, 1.2f, 2.2f));
            trigger.isTrigger = true;

            // Flaches Zielfeld (Oberkante 0.034 m – Kugeln rollen einfach darüber) mit weißem Ring wie eine Zielscheibe.
            GameObject pad = SB.MeshObj(MK.Cylinder12, "Pad", t, new Vector3(0f, 0.004f, 0f), new Vector3(2.4f, 0.03f, 2.4f), M.GoalIdle);
            CastNoShadow(pad);
            GameObject ring = SB.MeshObj(MK.Torus, "Ring", t, new Vector3(0f, 0.03f, 0f), new Vector3(1.6f, 0.15f, 1.6f), M.WarmWhite);
            CastNoShadow(ring);

            // Kleine Fahne außerhalb des Feldes (hinten rechts). Die Stange hat einen Collider, die Fahne nicht.
            var polePosition = new Vector3(1.3f, 0f, 0.75f);
            SB.Prim(PrimitiveType.Cylinder, "Flag_Pole", t, polePosition + new Vector3(0f, 0.75f, 0f), new Vector3(0.06f, 0.75f, 0.06f), M.WarmWhite);
            SB.MeshObj(MK.Sphere, "Flag_Pole_Top", t, polePosition + new Vector3(0f, 1.53f, 0f), Vector3.one * 0.1f, M.WarmWhite);
            // Dreieckige Fahne: Prisma um -90° gekippt, die Spitze zeigt nach +x.
            SB.MeshObj(MK.Prism, "Flag", t, polePosition + new Vector3(0.03f, 1.28f, 0f), new Vector3(0.4f, 0.6f, 0.03f), M.CoralRed, new Vector3(0f, 0f, -90f));

            Light successLight = SB.PointLight("SuccessLight", t, new Vector3(0f, 1.2f, 0f), SB.Hex("#4CFF8A"), 3f, 5f, LightShadows.None);
            successLight.enabled = false;

            GoalZone goal = root.AddComponent<GoalZone>();
            goal.Setup(RendererOf(pad), SB.Mat(M.GoalIdle), SB.Mat(M.GoalReached), successLight);
            goal.SuccessMessage = "Ziel erreicht!";

            SB.Note(root, "Zielfeld",
                "Rollt eine Kugel auf dieses Feld, wird es grün und ein Licht geht an. " +
                "Der Box Collider ist ein Trigger (Is Trigger): ein unsichtbarer Bereich, durch den man hindurchrollen kann. " +
                "Probier: Verschiebe das Ziel an eine schwierige Stelle.");
            Save(root, P.TargetZone);
        }

        static void BuildDomino()
        {
            GameObject root = NewRoot(P.Domino);
            Transform t = root.transform;

            // Physik auf der Wurzel: ein Box Collider genau so groß wie der Stein.
            BoxCollider box = AddBoxCollider(root, new Vector3(0f, 0.6f, 0f), new Vector3(0.2f, 1.2f, 0.6f));
            box.sharedMaterial = SB.PhysMat(PM.Normal);
            Rigidbody body = root.AddComponent<Rigidbody>();
            body.mass = 0.3f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            root.AddComponent<ResettableObject>();

            // Sichtbarer Stein (ohne eigenen Collider) und zwei weiße Punkte auf der -x-Seite.
            SB.Box("Tile", t, new Vector3(0f, 0.6f, 0f), new Vector3(0.2f, 1.2f, 0.6f), M.CoralRed, default, false);
            SB.Box("Dot_Top", t, new Vector3(-0.104f, 0.88f, 0f), new Vector3(0.01f, 0.12f, 0.12f), M.WarmWhite, default, false);
            SB.Box("Dot_Bottom", t, new Vector3(-0.104f, 0.32f, 0f), new Vector3(0.01f, 0.12f, 0.12f), M.WarmWhite, default, false);

            SB.Note(root, "Dominostein",
                "Ein Dominostein mit Rigidbody. Stehen viele in einer Reihe, stoßen sie sich nacheinander um – eine Kettenreaktion. " +
                "Probier: Ändere den Abstand zwischen den Steinen oder färbe Tile mit einem anderen Material.");
            Save(root, P.Domino);
        }
    }
}
