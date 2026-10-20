using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace StudentSandbox.EditorTools
{
    /// <summary>
    /// Entwickler-Werkzeug: rendert Bilder der Spielwiese aus allen Aussichtspunkten (auch im Batchmode),
    /// am Tag sowie Übersicht und Lichtlabor bei Sonnenuntergang und Nacht.
    /// Menü: Tools > Student Sandbox > Developer > Capture Screenshots
    /// Kommandozeile: -executeMethod StudentSandbox.EditorTools.SandboxScreenshots.CaptureBatch -sandboxShotDir &lt;Ordner&gt;
    /// Optional: -sandboxShotPoses "name:x,y,z:tx,ty,tz;name2:..." für zusätzliche Kamerapositionen.
    /// Die Beleuchtung der Szene (Sonne, Himmel, Lampen) wird danach exakt wiederhergestellt.
    /// </summary>
    public static class SandboxScreenshots
    {
        const int Width = 1600;
        const int Height = 900;

        [MenuItem(SandboxGenerator.MenuRoot + "Developer/Capture Screenshots", priority = 200)]
        static void CaptureFromMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Student Sandbox", "Bitte zuerst den Play Mode beenden.", "OK");
                return;
            }
            string folder = Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp/SandboxScreenshots"));
            int count = CaptureAll(folder, null);
            EditorUtility.RevealInFinder(folder);
            Debug.Log("[Student Sandbox] " + count + " Bilder gespeichert in " + folder);
        }

        public static void CaptureBatch()
        {
            string folder = GetArg("-sandboxShotDir") ?? Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp/SandboxScreenshots"));
            int count = 0;
            try
            {
                EditorSceneManager.OpenScene(SandboxPaths.ScenePath, OpenSceneMode.Single);
                count = CaptureAll(folder, GetArg("-sandboxShotPoses"));
                Debug.Log("[Student Sandbox] " + count + " Bilder gespeichert in " + folder);
            }
            catch (Exception exception)
            {
                Debug.LogError("[Student Sandbox] Screenshots fehlgeschlagen: " + exception);
            }
            if (Application.isBatchMode) EditorApplication.Exit(count > 0 ? 0 : 1);
        }

        public static int CaptureAll(string folder, string extraPoses)
        {
            Directory.CreateDirectory(folder);
            Camera source = Camera.main;
            if (source == null) source = UnityEngine.Object.FindAnyObjectByType<Camera>();
            if (source == null) throw new InvalidOperationException("Keine Kamera in der Szene.");
            DayNightController dayNight = UnityEngine.Object.FindAnyObjectByType<DayNightController>();

            Vector3 originalPosition = source.transform.position;
            Quaternion originalRotation = source.transform.rotation;
            LightingSnapshot snapshot = LightingSnapshot.Take(dayNight);
            int count = 0;
            try
            {
                var viewpoints = new List<Viewpoint>(UnityEngine.Object.FindObjectsByType<Viewpoint>(FindObjectsSortMode.None));
                viewpoints.Sort((a, b) => string.CompareOrdinal(a.name, b.name));

                if (dayNight != null) dayNight.Apply(TimeOfDay.Day);
                foreach (Viewpoint viewpoint in viewpoints)
                {
                    Render(source, viewpoint.transform.position, viewpoint.transform.rotation, Path.Combine(folder, viewpoint.name + "_Day.png"));
                    count++;
                }

                foreach (Pose pose in ParsePoses(extraPoses))
                {
                    Render(source, pose.position, pose.rotation, Path.Combine(folder, "Pose_" + pose.name + "_Day.png"));
                    count++;
                }

                if (dayNight != null)
                {
                    foreach (TimeOfDay time in new[] { TimeOfDay.Sunset, TimeOfDay.Night })
                    {
                        dayNight.Apply(time);
                        foreach (Viewpoint viewpoint in viewpoints)
                        {
                            if (!viewpoint.name.Contains("Overview") && !viewpoint.name.Contains("Light")) continue;
                            Render(source, viewpoint.transform.position, viewpoint.transform.rotation, Path.Combine(folder, viewpoint.name + "_" + time + ".png"));
                            count++;
                        }
                    }
                }
            }
            finally
            {
                snapshot.Restore();
                source.transform.SetPositionAndRotation(originalPosition, originalRotation);
            }
            return count;
        }

        /// <summary>Merkt sich Sonne, Umgebungslicht, Himmel, Nebel und alle Lampen – und stellt sie exakt wieder her.</summary>
        sealed class LightingSnapshot
        {
            DayNightController controller;
            SerializedObject controllerState;
            Light sun;
            Quaternion sunRotation;
            Color sunColor;
            float sunIntensity;
            Material skybox;
            AmbientMode ambientMode;
            Color sky, equator, ground, fog;
            readonly List<KeyValuePair<LampController, bool>> lamps = new List<KeyValuePair<LampController, bool>>();

            public static LightingSnapshot Take(DayNightController controller)
            {
                var snapshot = new LightingSnapshot
                {
                    controller = controller,
                    controllerState = controller != null ? new SerializedObject(controller) : null,
                    sun = controller != null ? controller.Sun : RenderSettings.sun,
                    skybox = RenderSettings.skybox,
                    ambientMode = RenderSettings.ambientMode,
                    sky = RenderSettings.ambientSkyColor,
                    equator = RenderSettings.ambientEquatorColor,
                    ground = RenderSettings.ambientGroundColor,
                    fog = RenderSettings.fogColor,
                };
                if (snapshot.sun != null)
                {
                    snapshot.sunRotation = snapshot.sun.transform.rotation;
                    snapshot.sunColor = snapshot.sun.color;
                    snapshot.sunIntensity = snapshot.sun.intensity;
                }
                foreach (LampController lamp in UnityEngine.Object.FindObjectsByType<LampController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                    snapshot.lamps.Add(new KeyValuePair<LampController, bool>(lamp, lamp.IsOn));
                return snapshot;
            }

            public void Restore()
            {
                if (sun != null)
                {
                    sun.transform.rotation = sunRotation;
                    sun.color = sunColor;
                    sun.intensity = sunIntensity;
                }
                RenderSettings.skybox = skybox;
                RenderSettings.ambientMode = ambientMode;
                RenderSettings.ambientSkyColor = sky;
                RenderSettings.ambientEquatorColor = equator;
                RenderSettings.ambientGroundColor = ground;
                RenderSettings.fogColor = fog;
                foreach (KeyValuePair<LampController, bool> lamp in lamps)
                    if (lamp.Key != null) lamp.Key.SetOn(lamp.Value);
                // Die gespeicherte Tageszeit des Controllers ebenfalls zurücksetzen.
                if (controller != null && controllerState != null)
                {
                    var current = new SerializedObject(controller);
                    current.CopyFromSerializedProperty(controllerState.FindProperty("timeOfDay"));
                    current.ApplyModifiedPropertiesWithoutUndo();
                }
                DynamicGI.UpdateEnvironment();
            }
        }

        static void Render(Camera camera, Vector3 position, Quaternion rotation, string file)
        {
            camera.transform.SetPositionAndRotation(position, rotation);
            var target = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32) { antiAliasing = 1 };
            target.Create();
            RenderTexture previousTarget = camera.targetTexture;
            float previousAspect = camera.aspect;
            camera.aspect = (float)Width / Height;

            var request = new RenderPipeline.StandardRequest();
            if (RenderPipeline.SupportsRenderRequest(camera, request))
            {
                request.destination = target;
                RenderPipeline.SubmitRenderRequest(camera, request);
            }
            else
            {
                camera.targetTexture = target;
                camera.Render();
                camera.targetTexture = previousTarget;
            }

            RenderTexture previousActive = RenderTexture.active;
            RenderTexture.active = target;
            var texture = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
            texture.Apply();
            RenderTexture.active = previousActive;
            File.WriteAllBytes(file, texture.EncodeToPNG());

            UnityEngine.Object.DestroyImmediate(texture);
            camera.aspect = previousAspect;
            camera.ResetAspect();
            target.Release();
            UnityEngine.Object.DestroyImmediate(target);
        }

        struct Pose
        {
            public string name;
            public Vector3 position;
            public Quaternion rotation;
        }

        static IEnumerable<Pose> ParsePoses(string text)
        {
            if (string.IsNullOrEmpty(text)) yield break;
            foreach (string entry in text.Split(';'))
            {
                string[] parts = entry.Split(':');
                if (parts.Length != 3) continue;
                Vector3 position = ParseVector(parts[1]);
                Vector3 target = ParseVector(parts[2]);
                yield return new Pose { name = parts[0], position = position, rotation = Quaternion.LookRotation(target - position, Vector3.up) };
            }
        }

        static Vector3 ParseVector(string text)
        {
            string[] values = text.Split(',');
            var culture = System.Globalization.CultureInfo.InvariantCulture;
            return new Vector3(float.Parse(values[0], culture), float.Parse(values[1], culture), float.Parse(values[2], culture));
        }

        static string GetArg(string name)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == name) return args[i + 1];
            return null;
        }
    }
}
