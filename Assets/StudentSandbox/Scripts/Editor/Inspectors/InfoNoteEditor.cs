using UnityEditor;
using UnityEngine;

namespace StudentSandbox.EditorTools
{
    /// <summary>
    /// Zeigt eine <see cref="InfoNote"/> als gut lesbare, farbige Notiz-Karte im Inspector.
    /// Die Felder zum Bearbeiten stecken in einer zugeklappten Gruppe "Text bearbeiten".
    /// </summary>
    [CustomEditor(typeof(InfoNote))]
    [CanEditMultipleObjects]
    public class InfoNoteEditor : Editor
    {
        static bool showEditFields;
        static GUIStyle titleStyle;
        static GUIStyle textStyle;
        static GUIStyle textAreaStyle;
        static bool stylesForProSkin;

        SerializedProperty titleProperty;
        SerializedProperty textProperty;

        void OnEnable()
        {
            titleProperty = serializedObject.FindProperty("title");
            textProperty = serializedObject.FindProperty("text");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EnsureStyles();
            if (titleProperty == null || textProperty == null)
            {
                DrawDefaultInspector();
                return;
            }

            bool pro = EditorGUIUtility.isProSkin;
            Color background = pro ? WorkshopGUI.Hex("#2C3A50") : WorkshopGUI.Hex("#FFF4CC");
            Color accent = pro ? WorkshopGUI.Sky : WorkshopGUI.Hex("#F2B620");

            string title = titleProperty.hasMultipleDifferentValues ? "Mehrere Notizen ausgewählt" : titleProperty.stringValue;
            string text = textProperty.hasMultipleDifferentValues ? "" : textProperty.stringValue;
            if (string.IsNullOrEmpty(title)) title = "Info";

            GUILayout.Space(4f);
            Rect box = EditorGUILayout.BeginVertical(WorkshopGUI.CardStyle(background, 1f, 16));
            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(new Rect(box.x + 5f, box.y + 7f, 4f, Mathf.Max(0f, box.height - 14f)), accent);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(EditorGUIUtility.IconContent("console.infoicon"), GUILayout.Width(26f), GUILayout.Height(26f));
            GUILayout.Label(title, titleStyle);
            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(text))
            {
                GUILayout.Space(2f);
                GUILayout.Label(text, textStyle);
            }
            EditorGUILayout.EndVertical();

            GUILayout.Space(2f);
            showEditFields = EditorGUILayout.Foldout(showEditFields, "Text bearbeiten", true);
            if (showEditFields)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(titleProperty, new GUIContent("Titel"));
                EditorGUILayout.LabelField("Text");
                EditorGUI.indentLevel--;

                EditorGUI.BeginChangeCheck();
                EditorGUI.showMixedValue = textProperty.hasMultipleDifferentValues;
                string newText = EditorGUILayout.TextArea(textProperty.stringValue, textAreaStyle, GUILayout.MinHeight(90f));
                EditorGUI.showMixedValue = false;
                if (EditorGUI.EndChangeCheck()) textProperty.stringValue = newText;
            }

            serializedObject.ApplyModifiedProperties();
        }

        static void EnsureStyles()
        {
            bool pro = EditorGUIUtility.isProSkin;
            if (titleStyle != null && stylesForProSkin == pro) return;
            stylesForProSkin = pro;

            Color text = pro ? WorkshopGUI.Hex("#E8ECF2") : WorkshopGUI.Hex("#3B4252");
            Color titleColor = pro ? Color.white : WorkshopGUI.Hex("#2E3440");

            titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                wordWrap = true,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(2, 2, 4, 2),
            };
            titleStyle.normal.textColor = titleColor;

            textStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 13,
                wordWrap = true,
                richText = false,
                padding = new RectOffset(4, 4, 2, 4),
            };
            textStyle.normal.textColor = text;

            textAreaStyle = new GUIStyle(EditorStyles.textArea)
            {
                wordWrap = true,
                fontSize = 12,
            };
        }
    }
}
