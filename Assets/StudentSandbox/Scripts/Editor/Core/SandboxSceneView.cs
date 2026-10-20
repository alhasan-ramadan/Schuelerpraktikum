using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StudentSandbox.EditorTools
{
    /// <summary>
    /// Hilfen für die Scene View: auf einen Bereich schauen, Objekte finden und auswählen.
    /// Beim Öffnen der Spielwiesen-Szene wird die Scene View automatisch auf die Übersicht ausgerichtet.
    /// </summary>
    [InitializeOnLoad]
    public static class SandboxSceneView
    {
        static SandboxSceneView()
        {
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneOpened += OnSceneOpened;
        }

        static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            if (Application.isBatchMode) return;
            if (scene.path == SandboxPaths.OriginalScenePath)
            {
                // Die Ausgangskopie soll unberührt bleiben – lieber gleich die richtige Spielwiese öffnen.
                EditorApplication.delayCall += () =>
                {
                    if (EditorUtility.DisplayDialog("Das ist die Ausgangskopie",
                            "Du hast die unberührte Ausgangskopie der Spielwiese geöffnet (Scenes/Original).\n" +
                            "Sie ist nur zum Wiederherstellen da. Bitte arbeite in der richtigen Spielwiese.",
                            "Richtige Spielwiese öffnen", "Trotzdem ansehen"))
                        SandboxGenerator.OpenSandboxScene(true);
                };
                return;
            }
            if (scene.path != SandboxPaths.ScenePath) return;
            // Kurz warten, bis die Scene View bereit ist.
            EditorApplication.delayCall += () => FrameViewpoint(CameraSetup.OverviewName);
        }

        public static bool IsSandboxSceneOpen
        {
            get
            {
                Scene active = SceneManager.GetActiveScene();
                return active.IsValid() && active.path == SandboxPaths.ScenePath;
            }
        }

        /// <summary>Richtet die Scene View auf einen Aussichtspunkt aus (Name aus <see cref="CameraSetup"/>).</summary>
        public static bool FrameViewpoint(string viewpointName)
        {
            GameObject go = FindInScene(viewpointName);
            Viewpoint viewpoint = go != null ? go.GetComponent<Viewpoint>() : null;
            if (viewpoint == null) return false;
            return Frame(viewpoint);
        }

        public static bool Frame(Viewpoint viewpoint)
        {
            SceneView view = SceneView.lastActiveSceneView;
            if (view == null && SceneView.sceneViews.Count > 0) view = (SceneView)SceneView.sceneViews[0];
            if (view == null) return false;

            float fov = view.cameraSettings != null ? view.cameraSettings.fieldOfView : 60f;
            float size = viewpoint.FocusDistance * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
            view.orthographic = false;
            view.LookAt(viewpoint.FocusPoint, viewpoint.transform.rotation, size, false, false);
            view.Repaint();
            return true;
        }

        /// <summary>Wählt ein Objekt nur aus (Hierarchy blinkt kurz), ohne die Scene View zu bewegen.</summary>
        public static bool Select(string objectName)
        {
            GameObject go = FindInScene(objectName);
            if (go == null) return false;
            Selection.activeGameObject = go;
            EditorGUIUtility.PingObject(go);
            return true;
        }

        /// <summary>Wählt ein Objekt aus und zeigt es in der Scene View (wie Doppelklick in der Hierarchy).</summary>
        public static bool SelectAndFrame(string objectName)
        {
            GameObject go = FindInScene(objectName);
            if (go == null) return false;
            Selection.activeGameObject = go;
            EditorGUIUtility.PingObject(go);
            SceneView view = SceneView.lastActiveSceneView;
            if (view != null) view.FrameSelected();
            return true;
        }

        /// <summary>Sucht ein GameObject nach Namen in allen geladenen Szenen (auch inaktive).</summary>
        public static GameObject FindInScene(string objectName)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    Transform found = FindRecursive(root.transform, objectName);
                    if (found != null) return found.gameObject;
                }
            }
            return null;
        }

        static Transform FindRecursive(Transform t, string objectName)
        {
            if (t.name == objectName) return t;
            for (int i = 0; i < t.childCount; i++)
            {
                Transform found = FindRecursive(t.GetChild(i), objectName);
                if (found != null) return found;
            }
            return null;
        }
    }
}
