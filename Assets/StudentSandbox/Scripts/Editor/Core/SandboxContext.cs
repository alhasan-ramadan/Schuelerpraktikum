using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StudentSandbox.EditorTools
{
    /// <summary>
    /// Gemeinsame Daten während der Szenen-Erzeugung: Wurzel-Gruppen der Hierarchy,
    /// zentrale Objekte (Sonne, Manager, Kamera) und die Maße der Welt.
    ///
    /// Weltaufbau (Draufsicht, +z = Norden):
    ///
    ///                 PHYSICS (0, 0, 25)
    ///                        |
    ///   CREATIVE (-25,0,0) — HUB (0,0,0) — LIGHT (25,0,0)
    ///                        |
    ///              INTERACTION (0, 0, -25)
    ///
    /// Jeder Bereich ist 24 x 24 m groß (lokal x und z von -12 bis +12) und NICHT gedreht,
    /// d.h. lokale Achsen = Welt-Achsen. Der Bodenbelag eines Bereichs endet auf y = 0.1.
    /// Der Eingang eines Bereichs liegt immer auf der Seite, die zum zentralen Platz zeigt.
    /// </summary>
    public sealed class SandboxContext
    {
        // ---------- Maße ----------
        public const float IslandRadius = 42f;
        public const float HubRadius = 9f;
        public const float ZoneDistance = 25f;
        public const float ZoneHalfSize = 12f;
        /// <summary>Oberkante des Bodenbelags in den vier Bereichen.</summary>
        public const float ZoneFloorTop = 0.1f;
        /// <summary>Oberkante der Wege zwischen Platz und Bereichen.</summary>
        public const float PathTop = 0.05f;
        /// <summary>Oberkante des zentralen Platzes.</summary>
        public const float HubTop = 0.12f;
        public const float PathWidth = 4f;

        public static readonly Vector3 HubCenter = Vector3.zero;
        public static readonly Vector3 CreativeCenter = new Vector3(-ZoneDistance, 0f, 0f);
        public static readonly Vector3 PhysicsCenter = new Vector3(0f, 0f, ZoneDistance);
        public static readonly Vector3 LightCenter = new Vector3(ZoneDistance, 0f, 0f);
        public static readonly Vector3 InteractionCenter = new Vector3(0f, 0f, -ZoneDistance);

        // Farben (Material-Schlüssel) der Bereiche – für Schilder, Bögen und Randmarkierungen.
        public const string CreativeAccent = M.MintGreen;
        public const string PhysicsAccent = M.WarmOrange;
        public const string LightAccent = M.Lavender;
        public const string InteractionAccent = M.Turquoise;

        // ---------- Szene ----------
        public Scene Scene;

        public Transform WorldRoot;
        public Transform Environment;
        public Transform Hub;
        public Transform CreativeZone;
        public Transform PhysicsZone;
        public Transform LightZone;
        public Transform InteractionZone;
        public Transform Cameras;
        public Transform Managers;
        public Transform Decoration;

        // ---------- Zentrale Objekte (werden nacheinander von den Buildern gesetzt) ----------
        public Light Sun;
        public DayNightController DayNight;
        public PhysicsResetter Resetter;
        public SandboxHUD Hud;
        public Camera MainCamera;
        public readonly List<Viewpoint> Viewpoints = new List<Viewpoint>();

        public SandboxContext(Scene scene)
        {
            Scene = scene;
        }

        /// <summary>Legt die zehn Haupt-Gruppen der Hierarchy an.</summary>
        public void CreateRootGroups()
        {
            WorldRoot = SB.Group("00_WORLD_ROOT", null);
            Environment = SB.Group("01_ENVIRONMENT", null);
            Hub = SB.Group("02_CENTRAL_HUB", null, HubCenter);
            CreativeZone = SB.Group("03_CREATIVE_ZONE", null, CreativeCenter);
            PhysicsZone = SB.Group("04_PHYSICS_ZONE", null, PhysicsCenter);
            LightZone = SB.Group("05_LIGHT_ZONE", null, LightCenter);
            InteractionZone = SB.Group("06_INTERACTION_ZONE", null, InteractionCenter);
            Cameras = SB.Group("07_CAMERAS", null);
            Managers = SB.Group("08_MANAGERS", null);
            Decoration = SB.Group("09_DECORATION", null);
        }
    }
}
