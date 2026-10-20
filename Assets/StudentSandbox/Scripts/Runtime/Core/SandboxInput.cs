using UnityEngine;
#if ENABLE_INPUT_SYSTEM && SANDBOX_INPUT_SYSTEM_PACKAGE
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

namespace StudentSandbox
{
    /// <summary>Tasten, die die Spielwiese im Play Mode benutzt.</summary>
    public enum SandboxKey
    {
        W, A, S, D, Q, E,
        Shift, Escape, Space,
        R, H, T,
        Digit1, Digit2, Digit3, Digit4, Digit5
    }

    /// <summary>
    /// Kleine Eingabe-Schicht für die ganze Spielwiese.
    /// Das Projekt nutzt das neue Input System ("Active Input Handling = Input System Package").
    /// Falls ein Projekt nur den alten Input Manager nutzt, fällt diese Klasse automatisch darauf zurück.
    /// Alle Sandbox-Skripte fragen Tastatur und Maus nur über diese Klasse ab.
    /// </summary>
    public static class SandboxInput
    {
#if ENABLE_INPUT_SYSTEM && SANDBOX_INPUT_SYSTEM_PACKAGE
        public static bool HasMouse => Mouse.current != null;
        public static bool HasKeyboard => Keyboard.current != null;

        public static bool GetKey(SandboxKey key)
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return false;
            if (key == SandboxKey.Shift) return keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
            var control = ToKeyControl(keyboard, key);
            return control != null && control.isPressed;
        }

        public static bool GetKeyDown(SandboxKey key)
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return false;
            if (key == SandboxKey.Shift) return keyboard.leftShiftKey.wasPressedThisFrame || keyboard.rightShiftKey.wasPressedThisFrame;
            var control = ToKeyControl(keyboard, key);
            if (control != null && control.wasPressedThisFrame) return true;
            var numpad = ToNumpadControl(keyboard, key);
            return numpad != null && numpad.wasPressedThisFrame;
        }

        public static bool LeftClickDown => Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        public static bool RightMouseHeld => Mouse.current != null && Mouse.current.rightButton.isPressed;
        public static Vector2 MousePosition => Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
        /// <summary>Mausbewegung in Pixeln seit dem letzten Frame.</summary>
        public static Vector2 MouseDelta => Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;
        /// <summary>+1 = Mausrad nach oben, -1 = nach unten, 0 = keine Bewegung.</summary>
        public static float ScrollSteps
        {
            get
            {
                if (Mouse.current == null) return 0f;
                float y = Mouse.current.scroll.ReadValue().y;
                return Mathf.Abs(y) < 0.01f ? 0f : Mathf.Sign(y);
            }
        }

        static KeyControl ToKeyControl(Keyboard k, SandboxKey key)
        {
            switch (key)
            {
                case SandboxKey.W: return k.wKey;
                case SandboxKey.A: return k.aKey;
                case SandboxKey.S: return k.sKey;
                case SandboxKey.D: return k.dKey;
                case SandboxKey.Q: return k.qKey;
                case SandboxKey.E: return k.eKey;
                case SandboxKey.Escape: return k.escapeKey;
                case SandboxKey.Space: return k.spaceKey;
                case SandboxKey.R: return k.rKey;
                case SandboxKey.H: return k.hKey;
                case SandboxKey.T: return k.tKey;
                case SandboxKey.Digit1: return k.digit1Key;
                case SandboxKey.Digit2: return k.digit2Key;
                case SandboxKey.Digit3: return k.digit3Key;
                case SandboxKey.Digit4: return k.digit4Key;
                case SandboxKey.Digit5: return k.digit5Key;
                default: return null;
            }
        }

        static KeyControl ToNumpadControl(Keyboard k, SandboxKey key)
        {
            switch (key)
            {
                case SandboxKey.Digit1: return k.numpad1Key;
                case SandboxKey.Digit2: return k.numpad2Key;
                case SandboxKey.Digit3: return k.numpad3Key;
                case SandboxKey.Digit4: return k.numpad4Key;
                case SandboxKey.Digit5: return k.numpad5Key;
                default: return null;
            }
        }
#elif ENABLE_LEGACY_INPUT_MANAGER
        public static bool HasMouse => Input.mousePresent;
        public static bool HasKeyboard => true;

        public static bool GetKey(SandboxKey key)
        {
            if (key == SandboxKey.Shift) return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            return Input.GetKey(ToKeyCode(key));
        }

        public static bool GetKeyDown(SandboxKey key)
        {
            if (key == SandboxKey.Shift) return Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);
            return Input.GetKeyDown(ToKeyCode(key)) || Input.GetKeyDown(ToKeypadCode(key));
        }

        public static bool LeftClickDown => Input.GetMouseButtonDown(0);
        public static bool RightMouseHeld => Input.GetMouseButton(1);
        public static Vector2 MousePosition => Input.mousePosition;
        // Der alte Input Manager liefert skalierte Achsen. Faktor 10 entspricht ungefähr Pixeln.
        public static Vector2 MouseDelta => new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * 10f;
        public static float ScrollSteps
        {
            get
            {
                float y = Input.mouseScrollDelta.y;
                return Mathf.Abs(y) < 0.01f ? 0f : Mathf.Sign(y);
            }
        }

        static KeyCode ToKeyCode(SandboxKey key)
        {
            switch (key)
            {
                case SandboxKey.W: return KeyCode.W;
                case SandboxKey.A: return KeyCode.A;
                case SandboxKey.S: return KeyCode.S;
                case SandboxKey.D: return KeyCode.D;
                case SandboxKey.Q: return KeyCode.Q;
                case SandboxKey.E: return KeyCode.E;
                case SandboxKey.Escape: return KeyCode.Escape;
                case SandboxKey.Space: return KeyCode.Space;
                case SandboxKey.R: return KeyCode.R;
                case SandboxKey.H: return KeyCode.H;
                case SandboxKey.T: return KeyCode.T;
                case SandboxKey.Digit1: return KeyCode.Alpha1;
                case SandboxKey.Digit2: return KeyCode.Alpha2;
                case SandboxKey.Digit3: return KeyCode.Alpha3;
                case SandboxKey.Digit4: return KeyCode.Alpha4;
                case SandboxKey.Digit5: return KeyCode.Alpha5;
                default: return KeyCode.None;
            }
        }

        static KeyCode ToKeypadCode(SandboxKey key)
        {
            switch (key)
            {
                case SandboxKey.Digit1: return KeyCode.Keypad1;
                case SandboxKey.Digit2: return KeyCode.Keypad2;
                case SandboxKey.Digit3: return KeyCode.Keypad3;
                case SandboxKey.Digit4: return KeyCode.Keypad4;
                case SandboxKey.Digit5: return KeyCode.Keypad5;
                default: return KeyCode.None;
            }
        }
#else
        // Kein Eingabesystem aktiv: Die Spielwiese funktioniert im Editor trotzdem, nur ohne Steuerung im Play Mode.
        public static bool HasMouse => false;
        public static bool HasKeyboard => false;
        public static bool GetKey(SandboxKey key) => false;
        public static bool GetKeyDown(SandboxKey key) => false;
        public static bool LeftClickDown => false;
        public static bool RightMouseHeld => false;
        public static Vector2 MousePosition => Vector2.zero;
        public static Vector2 MouseDelta => Vector2.zero;
        public static float ScrollSteps => 0f;
#endif

        /// <summary>Beschreibt, welches Eingabesystem gerade benutzt wird (für Hilfe-Texte und Tests).</summary>
        public static string ActiveBackend
        {
            get
            {
#if ENABLE_INPUT_SYSTEM && SANDBOX_INPUT_SYSTEM_PACKAGE
                return "Input System Package";
#elif ENABLE_LEGACY_INPUT_MANAGER
                return "Input Manager (alt)";
#else
                return "keins";
#endif
            }
        }
    }
}
