using System.IO;
using UnityEditor;

namespace StudentSandbox.EditorTools
{
    /// <summary>Alle Pfade der Spielwiese an einer Stelle.</summary>
    public static class SandboxPaths
    {
        public const string Root = "Assets/StudentSandbox";

        public const string Scenes = Root + "/Scenes";
        public const string ScenePath = Scenes + "/StudentSandbox.unity";
        public const string OriginalFolder = Scenes + "/Original";
        public const string OriginalScenePath = OriginalFolder + "/StudentSandbox_Original.unity";
        public const string Backups = Root + "/Backups";

        public const string Prefabs = Root + "/Prefabs";
        public const string PrefabsEnvironment = Prefabs + "/Environment";
        public const string PrefabsCreative = Prefabs + "/Creative";
        public const string PrefabsPhysics = Prefabs + "/Physics";
        public const string PrefabsInteractions = Prefabs + "/Interactions";

        public const string Materials = Root + "/Materials";
        public const string MaterialsPalette = Materials + "/Palette";
        public const string MaterialsEnvironment = Materials + "/Environment";
        public const string MaterialsEffects = Materials + "/Effects";
        public const string MaterialsSky = Materials + "/Sky";

        public const string PhysicsMaterials = Root + "/PhysicsMaterials";
        public const string Meshes = Root + "/Meshes";
        public const string Settings = Root + "/Settings";
        public const string Documentation = Root + "/Documentation";

        public const string VolumeProfilePath = Settings + "/StudentSandbox_PostProcessing.asset";
        public const string LightingSettingsPath = Settings + "/StudentSandbox_Lighting.lighting";

        static readonly string[] AllFolders =
        {
            Root, Scenes, OriginalFolder, Prefabs, PrefabsEnvironment, PrefabsCreative, PrefabsPhysics, PrefabsInteractions,
            Materials, MaterialsPalette, MaterialsEnvironment, MaterialsEffects, MaterialsSky,
            PhysicsMaterials, Meshes, Settings
        };

        public static void EnsureAllFolders()
        {
            foreach (string folder in AllFolders) EnsureFolder(folder);
        }

        /// <summary>Legt einen Ordner (und alle fehlenden Eltern-Ordner) über die AssetDatabase an.</summary>
        public static void EnsureFolder(string path)
        {
            path = path.Replace('\\', '/').TrimEnd('/');
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string name = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
