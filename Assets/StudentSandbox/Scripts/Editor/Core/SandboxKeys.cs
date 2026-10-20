namespace StudentSandbox.EditorTools
{
    /// <summary>
    /// Namen aller generierten Materialien. Wer im Generator ein Material braucht,
    /// benutzt diese Konstanten (Tippfehler fallen so schon beim Kompilieren auf).
    /// </summary>
    public static class M
    {
        // Palette – die Farben, die die Schülerin auf Objekte ziehen soll (Materials/Palette).
        public const string CoralRed = "CoralRed";
        public const string SoftYellow = "SoftYellow";
        public const string MintGreen = "MintGreen";
        public const string SkyBlue = "SkyBlue";
        public const string Lavender = "Lavender";
        public const string WarmOrange = "WarmOrange";
        public const string LightWood = "LightWood";
        public const string DarkWood = "DarkWood";
        public const string GrassGreen = "GrassGreen";
        public const string StoneGray = "StoneGray";
        public const string WarmWhite = "WarmWhite";
        public const string Turquoise = "Turquoise";
        public const string PinkBlossom = "PinkBlossom";
        public const string Charcoal = "Charcoal";
        public const string LeafGreen = "LeafGreen";
        public const string BerryPurple = "BerryPurple";

        // Umgebung (Materials/Environment).
        public const string Grass = "Env_Grass";
        public const string GrassDark = "Env_GrassDark";
        public const string Sand = "Env_Sand";
        public const string Beach = "Env_Beach";
        public const string Path = "Env_Path";
        public const string PlazaLight = "Env_PlazaLight";
        public const string PlazaAccent = "Env_PlazaAccent";
        public const string Water = "Env_Water";
        public const string WaterDeep = "Env_WaterDeep";
        public const string Cliff = "Env_Cliff";
        public const string Leaves = "Env_Leaves";
        public const string LeavesLight = "Env_LeavesLight";
        public const string LeavesPine = "Env_LeavesPine";
        public const string LeavesAutumn = "Env_LeavesAutumn";
        public const string Trunk = "Env_Trunk";
        public const string RoofRed = "Env_RoofRed";
        public const string RoofBlue = "Env_RoofBlue";
        public const string RoofPurple = "Env_RoofPurple";
        public const string Wall = "Env_Wall";
        public const string Window = "Env_Window";
        public const string Metal = "Env_Metal";
        public const string DarkMetal = "Env_DarkMetal";
        public const string Rubber = "Env_Rubber";
        public const string Cloud = "Env_Cloud";
        public const string Rock = "Env_Rock";
        public const string LabFloor = "Env_LabFloor";
        public const string LightFloor = "Env_LightFloor";
        public const string TechFloor = "Env_TechFloor";
        public const string TechPanel = "Env_TechPanel";
        public const string Asphalt = "Env_Asphalt";
        public const string Stripe = "Env_Stripe";
        public const string StageWhite = "Env_StageWhite";

        // Leuchtende Materialien (Materials/Effects).
        public const string LampOn = "Glow_LampOn";
        public const string LampOff = "Glow_LampOff";
        public const string WindowOn = "Glow_WindowOn";
        public const string WindowOff = "Glow_WindowOff";
        public const string TrafficRedOn = "Traffic_RedOn";
        public const string TrafficRedOff = "Traffic_RedOff";
        public const string TrafficYellowOn = "Traffic_YellowOn";
        public const string TrafficYellowOff = "Traffic_YellowOff";
        public const string TrafficGreenOn = "Traffic_GreenOn";
        public const string TrafficGreenOff = "Traffic_GreenOff";
        public const string GoalIdle = "Goal_Idle";
        public const string GoalReached = "Goal_Reached";
        public const string NeonTeal = "Neon_Teal";
        public const string NeonOrange = "Neon_Orange";
        public const string NeonPurple = "Neon_Purple";
        public const string NeonPink = "Neon_Pink";
        public const string Crystal = "Crystal";
        public const string ButtonRed = "Button_Red";
        public const string ButtonGreen = "Button_Green";
        public const string ButtonBlue = "Button_Blue";
        public const string ButtonYellow = "Button_Yellow";
        public const string ButtonOrange = "Button_Orange";
        public const string ButtonPurple = "Button_Purple";
        public const string ButtonTeal = "Button_Teal";
        public const string GlowRed = "Glow_Red";
        public const string GlowGreen = "Glow_Green";
        public const string GlowBlue = "Glow_Blue";
        public const string GlowSun = "Glow_Sun";
        public const string Text3D = "Text3D";

        // Himmel (Materials/Sky).
        public const string SkyDay = "Sky_Day";
        public const string SkySunset = "Sky_Sunset";
        public const string SkyNight = "Sky_Night";
    }

    /// <summary>Namen der Physik-Materialien (PhysicsMaterials).</summary>
    public static class PM
    {
        public const string Slippery = "Slippery";
        public const string Normal = "Normal";
        public const string Bouncy = "Bouncy";
    }

    /// <summary>
    /// Namen der selbst erzeugten Low-Poly-Meshes (Meshes-Ordner).
    /// Konvention: "Sockel"-Meshes stehen auf y = 0 und sind 1 hoch, Grundfläche 1 x 1 (x, z von -0.5 bis 0.5).
    /// "Zentrierte" Meshes liegen um den Ursprung und haben ungefähr 1 Durchmesser.
    /// </summary>
    public static class MK
    {
        // Zentriert (Durchmesser ca. 1).
        public const string Sphere = "LP_Sphere";       // facettierte Kugel (Ikosaeder, 1x unterteilt)
        public const string BlobA = "LP_BlobA";         // leicht unregelmäßige Kugel (Baumkronen, Büsche)
        public const string BlobB = "LP_BlobB";
        public const string BlobC = "LP_BlobC";
        public const string Rock = "LP_Rock";           // kantiger, flacher Stein
        public const string Crystal = "LP_Crystal";     // sechseckige Doppelpyramide, Höhe 1.6 (y -0.8 .. 0.8), Breite 1
        public const string Star = "LP_Star";           // fünfzackiger Stern, flach in der XY-Ebene, Dicke 1 (z -0.5 .. 0.5)
        public const string Torus = "LP_Torus";         // Ring in der XZ-Ebene, Außendurchmesser 1, Rohrdicke 0.2
        public const string ArrowBoard = "LP_ArrowBoard"; // Pfeil-Schild, zeigt nach +x, 1 x 1 x 1 zentriert

        // Sockel (stehen auf y = 0, Höhe 1).
        public const string Cone6 = "LP_Cone6";
        public const string Cone8 = "LP_Cone8";
        public const string Pyramid = "LP_Pyramid";     // 4 Seiten, Grundfläche 1 x 1
        public const string Prism = "LP_Prism";         // Satteldach: Dreieck in der XY-Ebene, Länge entlang z
        public const string Wedge = "LP_Wedge";         // Rampe: hohe Kante bei x = -0.5 (Höhe 1), flaches Ende bei x = +0.5
        public const string Cylinder6 = "LP_Cylinder6";
        public const string Cylinder8 = "LP_Cylinder8";
        public const string Cylinder12 = "LP_Cylinder12";
    }

    /// <summary>
    /// Namen aller Prefabs. Der Ordner ergibt sich aus <see cref="SandboxPrefabs.FolderFor"/>.
    /// Alle Prefabs haben ihren Drehpunkt (Pivot) unten in der Mitte und schauen nach -z ("vorne").
    /// </summary>
    public static class P
    {
        // Creative
        public const string SmallTree = "SmallTree";
        public const string LargeTree = "LargeTree";
        public const string PineTree = "PineTree";
        public const string Bush = "Bush";
        public const string SmallHouse = "SmallHouse";
        public const string Bench = "Bench";
        public const string LampPost = "LampPost";
        public const string Fence = "Fence";
        public const string Flower = "Flower";
        public const string Rock = "Rock";
        public const string ColoredCube = "ColoredCube";
        public const string ColoredSphere = "ColoredSphere";
        public const string ColoredCylinder = "ColoredCylinder";
        public const string Platform = "Platform";
        public const string DecorativePillar = "DecorativePillar";

        // Physics
        public const string PhysicsBall = "PhysicsBall";
        public const string BouncyBall = "BouncyBall";
        public const string WallBlock = "WallBlock";
        public const string SmallRamp = "SmallRamp";
        public const string TallRamp = "TallRamp";
        public const string Bumper = "Bumper";
        public const string ObstacleCube = "ObstacleCube";
        public const string TargetZone = "TargetZone";
        public const string Domino = "Domino";

        // Interactions
        public const string PushButton = "PushButton";
        public const string RotatingCrystal = "RotatingCrystal";
        public const string MovingPlatformSet = "MovingPlatformSet";

        // Environment
        public const string Cloud = "Cloud";
        public const string FlowerBed = "FlowerBed";
        public const string Signpost = "Signpost";
    }
}
