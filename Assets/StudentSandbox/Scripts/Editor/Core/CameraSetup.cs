using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace StudentSandbox.EditorTools
{
    /// <summary>Main Camera (Flug-Kamera + Klicks) und die fünf Aussichtspunkte.</summary>
    public static class CameraSetup
    {
        public const string OverviewName = "Viewpoint_1_Overview";
        public const string CreativeName = "Viewpoint_2_Creative";
        public const string PhysicsName = "Viewpoint_3_Physics";
        public const string LightName = "Viewpoint_4_Light";
        public const string InteractionName = "Viewpoint_5_Interaction";

        public static void Build(SandboxContext ctx)
        {
            Transform viewpoints = SB.Group("Viewpoints", ctx.Cameras);
            ctx.Viewpoints.Clear();
            ctx.Viewpoints.Add(CreateViewpoint(viewpoints, OverviewName, "Übersicht", new Vector3(0f, 56f, -64f), new Vector3(0f, 0f, -1f)));
            ctx.Viewpoints.Add(CreateViewpoint(viewpoints, CreativeName, "Baubereich (Creative)", new Vector3(-7f, 12f, -12f), new Vector3(-26f, 0.5f, 0f)));
            ctx.Viewpoints.Add(CreateViewpoint(viewpoints, PhysicsName, "Physiklabor (Physics)", new Vector3(9f, 13f, 7f), new Vector3(0f, 1.2f, 26f)));
            ctx.Viewpoints.Add(CreateViewpoint(viewpoints, LightName, "Lichtlabor (Light)", new Vector3(8f, 11f, -10f), new Vector3(26f, 1f, 1f)));
            ctx.Viewpoints.Add(CreateViewpoint(viewpoints, InteractionName, "Interaktionsbereich (Interactions)", new Vector3(7f, 14f, -6f), new Vector3(-1f, 0.5f, -26f)));
            SB.Note(viewpoints.gameObject, "Aussichtspunkte",
                "Jeder Aussichtspunkt ist eine Kamera-Position. Im Play Mode fliegst du mit den Tasten 1 bis 5 dorthin.\n" +
                "Das Workshop-Fenster benutzt sie, um die Scene View auf einen Bereich auszurichten.");

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(ctx.Cameras, false);
            Transform overview = ctx.Viewpoints[0].transform;
            cameraObject.transform.SetPositionAndRotation(overview.position, overview.rotation);

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 55f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 600f;
            camera.clearFlags = CameraClearFlags.Skybox;
            UniversalAdditionalCameraData cameraData = camera.GetUniversalAdditionalCameraData();
            cameraData.renderPostProcessing = true;
            cameraData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            cameraObject.AddComponent<AudioListener>();

            FreeFlyCamera fly = cameraObject.AddComponent<FreeFlyCamera>();
            fly.Viewpoints = ctx.Viewpoints.ToArray();
            cameraObject.AddComponent<ClickInteractor>();
            ctx.MainCamera = camera;

            SB.Note(cameraObject, "Die Kamera für den Play Mode",
                "Durch diese Kamera siehst du die Welt, wenn du Play drückst.\n" +
                "W A S D = fliegen • Q / E = runter / hoch • Shift = schneller • rechte Maustaste halten = umschauen\n" +
                "Linksklick = Schalter drücken • 1 bis 5 = zu den Bereichen fliegen • Esc = Maus freigeben.\n" +
                "Free Fly Camera bewegt die Kamera, Click Interactor erkennt Klicks auf Schalter.");
        }

        static Viewpoint CreateViewpoint(Transform parent, string objectName, string displayName, Vector3 position, Vector3 target)
        {
            var go = new GameObject(objectName);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.rotation = Quaternion.LookRotation(target - position, Vector3.up);
            Viewpoint viewpoint = go.AddComponent<Viewpoint>();
            viewpoint.DisplayName = displayName;
            viewpoint.FocusDistance = Vector3.Distance(position, target);
            return viewpoint;
        }
    }
}
