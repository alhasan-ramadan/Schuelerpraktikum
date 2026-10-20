using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StudentSandbox.EditorTools
{
    /// <summary>
    /// Erzeugt alle Prefab-Assets. Die eigentlichen Bauanleitungen stehen in den Dateien
    /// SandboxPrefabs.Creative.cs, .Physics.cs, .Interactions.cs und .Environment.cs.
    ///
    /// Regeln für alle Prefabs:
    ///  - Drehpunkt (Pivot) unten in der Mitte (y = 0 ist der Boden).
    ///  - "Vorne" ist lokal -z (z.B. Haustür, Sitzrichtung der Bank, Beschriftungen).
    ///  - Sinnvoll benannte Unterobjekte (Trunk, Crown, Roof, Seat …).
    ///  - Mindestens ein Collider, damit Physik-Kugeln abprallen können.
    /// </summary>
    public static partial class SandboxPrefabs
    {
        static readonly HashSet<string> CreativeNames = new HashSet<string>
        {
            P.SmallTree, P.LargeTree, P.PineTree, P.Bush, P.SmallHouse, P.Bench, P.LampPost, P.Fence, P.Flower, P.Rock,
            P.ColoredCube, P.ColoredSphere, P.ColoredCylinder, P.Platform, P.DecorativePillar,
        };

        static readonly HashSet<string> PhysicsNames = new HashSet<string>
        {
            P.PhysicsBall, P.BouncyBall, P.WallBlock, P.SmallRamp, P.TallRamp, P.Bumper, P.ObstacleCube, P.TargetZone, P.Domino,
        };

        static readonly HashSet<string> InteractionNames = new HashSet<string>
        {
            P.PushButton, P.RotatingCrystal, P.MovingPlatformSet,
        };

        static readonly HashSet<string> EnvironmentNames = new HashSet<string>
        {
            P.Cloud, P.FlowerBed, P.Signpost,
        };

        public static IEnumerable<string> AllNames
        {
            get
            {
                foreach (string name in CreativeNames) yield return name;
                foreach (string name in PhysicsNames) yield return name;
                foreach (string name in InteractionNames) yield return name;
                foreach (string name in EnvironmentNames) yield return name;
            }
        }

        public static string FolderFor(string prefabName)
        {
            if (CreativeNames.Contains(prefabName)) return SandboxPaths.PrefabsCreative;
            if (PhysicsNames.Contains(prefabName)) return SandboxPaths.PrefabsPhysics;
            if (InteractionNames.Contains(prefabName)) return SandboxPaths.PrefabsInteractions;
            if (EnvironmentNames.Contains(prefabName)) return SandboxPaths.PrefabsEnvironment;
            throw new ArgumentException("[Student Sandbox] Unbekanntes Prefab: " + prefabName + " (Konstante in P verwenden)");
        }

        public static string PathFor(string prefabName) => FolderFor(prefabName) + "/" + prefabName + ".prefab";

        public static GameObject Load(string prefabName)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PathFor(prefabName));
            if (prefab == null) throw new InvalidOperationException("[Student Sandbox] Prefab fehlt: " + PathFor(prefabName) + " (wurde CreateAll() aufgerufen?)");
            return prefab;
        }

        /// <summary>Baut alle Prefabs neu. Braucht vorhandene Materialien und Meshes.</summary>
        public static void CreateAll()
        {
            BuildCreativePrefabs();
            BuildPhysicsPrefabs();
            BuildInteractionPrefabs();
            BuildEnvironmentPrefabs();
            AssetDatabase.SaveAssets();
        }

        static partial void BuildCreativePrefabs();
        static partial void BuildPhysicsPrefabs();
        static partial void BuildInteractionPrefabs();
        static partial void BuildEnvironmentPrefabs();

        /// <summary>Legt ein leeres Wurzel-Objekt für ein neues Prefab an (in der aktuell offenen Szene).</summary>
        static GameObject NewRoot(string prefabName)
        {
            return new GameObject(prefabName);
        }

        /// <summary>
        /// Speichert root als Prefab-Asset (überschreibt ein vorhandenes, die GUID bleibt erhalten)
        /// und löscht das temporäre Objekt aus der Szene.
        /// </summary>
        static GameObject Save(GameObject root, string prefabName)
        {
            string path = PathFor(prefabName);
            root.name = prefabName;
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path, out bool success);
            UnityEngine.Object.DestroyImmediate(root);
            if (!success || prefab == null) throw new InvalidOperationException("[Student Sandbox] Prefab konnte nicht gespeichert werden: " + path);
            return prefab;
        }
    }
}
