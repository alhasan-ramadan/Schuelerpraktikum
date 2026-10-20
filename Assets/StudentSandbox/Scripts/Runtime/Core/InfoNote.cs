using UnityEngine;

namespace StudentSandbox
{
    /// <summary>
    /// Eine Erklär-Notiz für den Inspector. Sie macht im Spiel nichts,
    /// sondern erklärt, was dieses Objekt ist und was man damit ausprobieren kann.
    /// </summary>
    [AddComponentMenu("Student Sandbox/Info Note")]
    [DisallowMultipleComponent]
    public class InfoNote : MonoBehaviour
    {
        [SerializeField] string title = "Info";
        [SerializeField, TextArea(3, 12)] string text = "";

        public string Title
        {
            get => title;
            set => title = value;
        }

        public string Text
        {
            get => text;
            set => text = value;
        }
    }
}
