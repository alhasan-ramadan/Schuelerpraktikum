using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;

namespace StudentSandbox.EditorTools
{
    public enum ColliderMode
    {
        None,
        Box,
        ConvexMesh,
        Mesh
    }

    /// <summary>
    /// "Sandbox Build" – kleine Bau-Helfer für den Generator.
    /// Alle Positionen sind LOKAL zum übergebenen Eltern-Objekt.
    /// Alle Builder (Zonen, Prefabs, Welt) benutzen diese Funktionen, damit der Stil einheitlich bleibt.
    /// </summary>
    public static class SB
    {
        // ------------------------------------------------------------------ Grundlagen

        /// <summary>Leeres Gruppen-Objekt (zum Ordnen der Hierarchy).</summary>
        public static Transform Group(string name, Transform parent, Vector3 localPosition = default, float yaw = 0f)
        {
            var go = new GameObject(name);
            Transform t = go.transform;
            if (parent != null) t.SetParent(parent, false);
            t.localPosition = localPosition;
            t.localRotation = Quaternion.Euler(0f, yaw, 0f);
            return t;
        }

        /// <summary>Unity-Grundkörper (Cube, Sphere, Cylinder, Capsule, Plane, Quad) mit Material.</summary>
        public static GameObject Prim(PrimitiveType type, string name, Transform parent, Vector3 localPosition, Vector3 localScale,
            string materialKey, Vector3 localEuler = default, bool keepCollider = true)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            Transform t = go.transform;
            if (parent != null) t.SetParent(parent, false);
            t.localPosition = localPosition;
            t.localRotation = Quaternion.Euler(localEuler);
            t.localScale = localScale;
            if (!string.IsNullOrEmpty(materialKey)) go.GetComponent<Renderer>().sharedMaterial = Mat(materialKey);
            if (!keepCollider)
            {
                Collider collider = go.GetComponent<Collider>();
                if (collider != null) Object.DestroyImmediate(collider);
            }
            return go;
        }

        /// <summary>Würfel – die häufigste Form. Kurzform von Prim(PrimitiveType.Cube, ...).</summary>
        public static GameObject Box(string name, Transform parent, Vector3 localPosition, Vector3 size, string materialKey,
            Vector3 localEuler = default, bool keepCollider = true)
        {
            return Prim(PrimitiveType.Cube, name, parent, localPosition, size, materialKey, localEuler, keepCollider);
        }

        /// <summary>Objekt mit einem selbst erzeugten Low-Poly-Mesh (siehe <see cref="MK"/>).</summary>
        public static GameObject MeshObj(string meshKey, string name, Transform parent, Vector3 localPosition, Vector3 localScale,
            string materialKey, Vector3 localEuler = default, ColliderMode collider = ColliderMode.None)
        {
            var go = new GameObject(name);
            Transform t = go.transform;
            if (parent != null) t.SetParent(parent, false);
            t.localPosition = localPosition;
            t.localRotation = Quaternion.Euler(localEuler);
            t.localScale = localScale;
            Mesh mesh = SandboxMeshes.Get(meshKey);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer renderer = go.AddComponent<MeshRenderer>();
            if (!string.IsNullOrEmpty(materialKey)) renderer.sharedMaterial = Mat(materialKey);

            switch (collider)
            {
                case ColliderMode.Box:
                    go.AddComponent<BoxCollider>();
                    break;
                case ColliderMode.ConvexMesh:
                    MeshCollider convex = go.AddComponent<MeshCollider>();
                    convex.sharedMesh = mesh;
                    convex.convex = true;
                    break;
                case ColliderMode.Mesh:
                    go.AddComponent<MeshCollider>().sharedMesh = mesh;
                    break;
            }
            return go;
        }

        // ------------------------------------------------------------------ Assets

        public static Material Mat(string key) => SandboxMaterials.Get(key);
        public static Mesh Mesh(string key) => SandboxMeshes.Get(key);
        public static PhysicsMaterial PhysMat(string key) => SandboxMaterials.GetPhysics(key);

        /// <summary>Lädt ein Prefab-Asset (siehe <see cref="P"/>).</summary>
        public static GameObject Prefab(string prefabName) => SandboxPrefabs.Load(prefabName);

        /// <summary>
        /// Setzt eine Prefab-Instanz in die Szene (bleibt mit dem Prefab verbunden – blaues Würfel-Symbol in der Hierarchy).
        /// </summary>
        public static GameObject Place(string prefabName, Transform parent, Vector3 localPosition, float yaw = 0f,
            float uniformScale = 1f, string instanceName = null)
        {
            GameObject prefab = Prefab(prefabName);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            Transform t = instance.transform;
            t.localPosition = localPosition;
            t.localRotation = Quaternion.Euler(0f, yaw, 0f);
            t.localScale = prefab.transform.localScale * uniformScale;
            if (!string.IsNullOrEmpty(instanceName)) instance.name = instanceName;
            return instance;
        }

        /// <summary>Setzt das Material aller Renderer mit diesem Namen unterhalb von root (z.B. "Crown" eines Baums).</summary>
        public static void Recolor(GameObject root, string childName, string materialKey)
        {
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer.gameObject.name == childName) renderer.sharedMaterial = Mat(materialKey);
            }
        }

        // ------------------------------------------------------------------ Licht

        public static Light PointLight(string name, Transform parent, Vector3 localPosition, Color color, float intensity, float range,
            LightShadows shadows = LightShadows.None)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            Light light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = shadows;
            light.shadowStrength = 0.85f;
            return light;
        }

        public static Light SpotLight(string name, Transform parent, Vector3 localPosition, Vector3 localEuler, Color color, float intensity,
            float range, float spotAngle, LightShadows shadows = LightShadows.Soft)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = Quaternion.Euler(localEuler);
            Light light = go.AddComponent<Light>();
            light.type = LightType.Spot;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.spotAngle = spotAngle;
            light.innerSpotAngle = spotAngle * 0.6f;
            light.shadows = shadows;
            light.shadowStrength = 0.9f;
            return light;
        }

        // ------------------------------------------------------------------ Text und Schilder

        static Font cachedFont;

        public static Font DefaultFont
        {
            get
            {
                if (cachedFont == null) cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                return cachedFont;
            }
        }

        /// <summary>
        /// 3D-Text (TextMesh). Lesbar von der Seite, in die die lokale -z-Achse zeigt
        /// (also wenn man entlang +z auf den Text schaut). letterHeight = Höhe einer Zeile in Metern.
        /// </summary>
        public static TextMesh Text(string name, string text, Transform parent, Vector3 localPosition, Vector3 localEuler,
            float letterHeight, Color color, FontStyle style = FontStyle.Bold)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = Quaternion.Euler(localEuler);
            TextMesh textMesh = go.AddComponent<TextMesh>();
            textMesh.text = text;
            textMesh.font = DefaultFont;
            textMesh.fontSize = 96;
            textMesh.fontStyle = style;
            textMesh.characterSize = letterHeight / 9.6f;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = color;
            textMesh.richText = false;
            MeshRenderer renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = TextMaterial;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return textMesh;
        }

        /// <summary>
        /// Schrift-Material mit Tiefentest (eigener Shader). Fällt auf das Material der Schriftart zurück,
        /// falls der Shader fehlt.
        /// </summary>
        public static Material TextMaterial
        {
            get
            {
                try
                {
                    return SandboxMaterials.Get(M.Text3D);
                }
                catch (System.ArgumentException)
                {
                    return DefaultFont != null ? DefaultFont.material : null;
                }
            }
        }

        /// <summary>Breite eines TextMesh in lokalen Einheiten (über die Zeichen-Maße der Schriftart gemessen).</summary>
        public static float MeasureTextWidth(TextMesh textMesh)
        {
            Font font = textMesh.font;
            if (font == null || string.IsNullOrEmpty(textMesh.text)) return 0f;
            int size = textMesh.fontSize > 0 ? textMesh.fontSize : font.fontSize;
            font.RequestCharactersInTexture(textMesh.text, size, textMesh.fontStyle);
            float widest = 0f;
            float line = 0f;
            foreach (char c in textMesh.text)
            {
                if (c == '\n')
                {
                    widest = Mathf.Max(widest, line);
                    line = 0f;
                    continue;
                }
                if (font.GetCharacterInfo(c, out CharacterInfo info, size, textMesh.fontStyle)) line += info.advance;
            }
            widest = Mathf.Max(widest, line);
            return widest * textMesh.characterSize * 0.1f;
        }

        /// <summary>Verkleinert die Schrift, falls sie breiter als maxWidth ist (damit sie nicht über das Schild hinausragt).</summary>
        public static void FitText(TextMesh textMesh, float maxWidth)
        {
            if (textMesh == null || maxWidth <= 0f) return;
            float width = MeasureTextWidth(textMesh);
            if (width > maxWidth) textMesh.characterSize *= maxWidth / width;
        }

        /// <summary>
        /// Ein Schild auf zwei Pfosten. Die Schrift zeigt in Richtung lokal -z des Schildes
        /// (mit yaw drehen, damit die Schrift zum Betrachter zeigt).
        /// </summary>
        public static GameObject Sign(string name, Transform parent, Vector3 localPosition, float yaw, string title, string subtitle,
            string accentMaterial, float width = 3f, float height = 2.2f)
        {
            Transform root = Group(name, parent, localPosition, yaw);
            float boardHeight = string.IsNullOrEmpty(subtitle) ? 0.8f : 1.15f;
            float boardY = height;
            Box("Post_Left", root, new Vector3(-width * 0.4f, boardY * 0.5f, 0.06f), new Vector3(0.14f, boardY, 0.14f), M.DarkWood);
            Box("Post_Right", root, new Vector3(width * 0.4f, boardY * 0.5f, 0.06f), new Vector3(0.14f, boardY, 0.14f), M.DarkWood);
            Box("Board", root, new Vector3(0f, boardY, 0f), new Vector3(width, boardHeight, 0.12f), M.WarmWhite);
            Box("Board_Trim", root, new Vector3(0f, boardY + boardHeight * 0.5f + 0.06f, 0f), new Vector3(width + 0.1f, 0.12f, 0.16f), accentMaterial, default, false);
            Box("Board_Trim_Bottom", root, new Vector3(0f, boardY - boardHeight * 0.5f - 0.06f, 0f), new Vector3(width + 0.1f, 0.12f, 0.16f), accentMaterial, default, false);
            Color textColor = SandboxMaterials.Hex("#2E3440");
            float textWidth = width - 0.3f;
            if (string.IsNullOrEmpty(subtitle))
            {
                FitText(Text("Title", title, root, new Vector3(0f, boardY, -0.075f), Vector3.zero, 0.5f, textColor), textWidth);
            }
            else
            {
                FitText(Text("Title", title, root, new Vector3(0f, boardY + 0.2f, -0.075f), Vector3.zero, 0.48f, textColor), textWidth);
                FitText(Text("Subtitle", subtitle, root, new Vector3(0f, boardY - 0.27f, -0.075f), Vector3.zero, 0.26f, textColor, FontStyle.Normal), textWidth);
            }
            return root.gameObject;
        }

        /// <summary>
        /// Großer Eingangsbogen eines Bereichs (über dem Weg). Schrift auf der lokalen -z-Seite.
        /// Mit yaw so drehen, dass -z zum zentralen Platz zeigt.
        /// </summary>
        public static GameObject EntranceArch(string name, Transform parent, Vector3 localPosition, float yaw, string title, string subtitle,
            string accentMaterial, string pillarMaterial = M.WarmWhite)
        {
            Transform root = Group(name, parent, localPosition, yaw);
            const float halfWidth = 2.9f;
            const float pillarHeight = 4.2f;
            foreach (float side in new[] { -1f, 1f })
            {
                string sideName = side < 0 ? "Left" : "Right";
                Box("Pillar_Base_" + sideName, root, new Vector3(side * halfWidth, 0.25f, 0f), new Vector3(0.9f, 0.5f, 0.9f), M.StoneGray);
                Box("Pillar_" + sideName, root, new Vector3(side * halfWidth, pillarHeight * 0.5f + 0.25f, 0f), new Vector3(0.6f, pillarHeight, 0.6f), pillarMaterial);
                Box("Pillar_Band_" + sideName, root, new Vector3(side * halfWidth, 1.6f, 0f), new Vector3(0.66f, 0.3f, 0.66f), accentMaterial, default, false);
                MeshObj(MK.Pyramid, "Pillar_Cap_" + sideName, root, new Vector3(side * halfWidth, pillarHeight + 1.05f, 0f), new Vector3(0.8f, 0.55f, 0.8f), accentMaterial);
            }
            Box("Beam", root, new Vector3(0f, pillarHeight + 0.6f, 0f), new Vector3(halfWidth * 2f + 1.2f, 1.3f, 0.5f), accentMaterial);
            Box("Beam_Panel", root, new Vector3(0f, pillarHeight + 0.6f, -0.2f), new Vector3(halfWidth * 2f + 0.4f, 1.0f, 0.15f), M.WarmWhite, default, false);
            Color textColor = SandboxMaterials.Hex("#2E3440");
            float panelWidth = halfWidth * 2f + 0.1f;
            FitText(Text("Title", title, root, new Vector3(0f, pillarHeight + 0.78f, -0.29f), Vector3.zero, 0.62f, textColor), panelWidth);
            if (!string.IsNullOrEmpty(subtitle))
                FitText(Text("Subtitle", subtitle, root, new Vector3(0f, pillarHeight + 0.33f, -0.29f), Vector3.zero, 0.28f, textColor, FontStyle.Normal), panelWidth);
            // Rückseite (vom Bereich aus gesehen) ebenfalls beschriften.
            Box("Beam_Panel_Back", root, new Vector3(0f, pillarHeight + 0.6f, 0.2f), new Vector3(halfWidth * 2f + 0.4f, 1.0f, 0.15f), M.WarmWhite, default, false);
            Text("Title_Back", "ZUM PLATZ", root, new Vector3(0f, pillarHeight + 0.6f, 0.29f), new Vector3(0f, 180f, 0f), 0.45f, textColor);
            return root.gameObject;
        }

        // ------------------------------------------------------------------ Schalter

        /// <summary>
        /// Ein Knopf auf einem Sockel. Der ganze Sockel ist anklickbar.
        /// Die Beschriftung (caption) steht auf einem kleinen Schild vorne (lokal -z).
        /// </summary>
        public static ClickableSwitch PushButton(string name, Transform parent, Vector3 localPosition, float yaw, string hoverLabel,
            string capMaterial, string caption = null, float pedestalHeight = 1.0f)
        {
            Transform root = Group(name, parent, localPosition, yaw);
            MeshObj(MK.Cylinder8, "Pedestal", root, Vector3.zero, new Vector3(0.7f, pedestalHeight, 0.7f), M.Charcoal, default, ColliderMode.ConvexMesh);
            MeshObj(MK.Cylinder8, "Pedestal_Top", root, new Vector3(0f, pedestalHeight, 0f), new Vector3(0.82f, 0.08f, 0.82f), M.StoneGray, default, ColliderMode.ConvexMesh);
            Transform mount = Group("CapMount", root, new Vector3(0f, pedestalHeight + 0.08f, 0f));
            GameObject cap = Prim(PrimitiveType.Cylinder, "ButtonCap", mount, new Vector3(0f, 0.06f, 0f), new Vector3(0.5f, 0.07f, 0.5f), capMaterial);
            if (!string.IsNullOrEmpty(caption))
            {
                Box("Caption_Plate", root, new Vector3(0f, pedestalHeight * 0.62f, -0.37f), new Vector3(0.62f, 0.26f, 0.04f), M.WarmWhite, default, false);
                FitText(Text("Caption", caption, root, new Vector3(0f, pedestalHeight * 0.62f, -0.395f), Vector3.zero, 0.15f, SandboxMaterials.Hex("#2E3440")), 0.56f);
            }
            ClickableSwitch clickable = root.gameObject.AddComponent<ClickableSwitch>();
            clickable.Label = hoverLabel;
            clickable.ButtonCap = cap.transform;
            return clickable;
        }

        /// <summary>
        /// Verbindet ein UnityEvent dauerhaft mit einer Methode (sichtbar im Inspector unter "On Pressed").
        /// Beispiel: SB.Listen(button.OnPressed, door.Toggle);
        /// </summary>
        public static void Listen(UnityEvent unityEvent, UnityAction action)
        {
            UnityEventTools.AddPersistentListener(unityEvent, action);
        }

        // ------------------------------------------------------------------ Erklär-Notizen

        /// <summary>Hängt eine <see cref="InfoNote"/> an und schiebt sie im Inspector ganz nach oben.</summary>
        public static InfoNote Note(GameObject target, string title, string text)
        {
            InfoNote note = target.GetComponent<InfoNote>();
            if (note == null) note = target.AddComponent<InfoNote>();
            note.Title = title;
            note.Text = text;
            // Bei Prefab-Instanzen darf Unity die Reihenfolge der Komponenten nicht ändern.
            // Prefabs bekommen ihre Notiz deshalb schon im Prefab-Asset (dort steht sie oben).
            if (!PrefabUtility.IsPartOfPrefabInstance(target))
            {
                int guard = 0;
                while (UnityEditorInternal.ComponentUtility.MoveComponentUp(note) && guard++ < 32) { }
            }
            return note;
        }

        // ------------------------------------------------------------------ Sonstiges

        /// <summary>Markiert Objekte als statisch (nur für unbewegliche Umgebung wie Boden und Wege).</summary>
        public static void MarkStatic(GameObject root)
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                GameObjectUtility.SetStaticEditorFlags(t.gameObject, StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccludeeStatic | StaticEditorFlags.OccluderStatic);
            }
        }

        /// <summary>Schaltet den Schattenwurf für alle Renderer unterhalb von root aus (z.B. für flache Bodenmarkierungen).</summary>
        public static void NoShadows(GameObject root)
        {
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        public static void SetPhysicsMaterial(GameObject root, string physicsMaterialKey)
        {
            PhysicsMaterial material = PhysMat(physicsMaterialKey);
            foreach (Collider collider in root.GetComponentsInChildren<Collider>(true)) collider.sharedMaterial = material;
        }

        public static Color Hex(string hex) => SandboxMaterials.Hex(hex);
    }
}
