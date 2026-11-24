using UnityEngine;
using UnityEngine.InputSystem;

namespace LWalida
{
    /// <summary>
    /// Thin adapter over the Unity Input System so gameplay code never touches the
    /// legacy Input class (the project is configured for "Input System Package" only).
    /// UnityEngine.UI handles pointer/keyboard menu navigation through the
    /// InputSystemUIInputModule.
    /// </summary>
    public static class LWalidaInput
    {
        private static Keyboard Keyboard => Keyboard.current;

        public static float MoveAxis
        {
            get
            {
                var keyboard = Keyboard;
                if (keyboard == null) return 0f;
                float axis = 0f;
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) axis -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) axis += 1f;
                return Mathf.Clamp(axis, -1f, 1f);
            }
        }

        public static bool InteractPressed
        {
            get { var k = Keyboard; return k != null && k.eKey.wasPressedThisFrame; }
        }

        public static bool CameraPressed
        {
            get { var k = Keyboard; return k != null && k.cKey.wasPressedThisFrame; }
        }

        public static bool RecordPressed
        {
            get { var k = Keyboard; return k != null && k.rKey.wasPressedThisFrame; }
        }

        public static bool JournalPressed
        {
            get { var k = Keyboard; return k != null && k.jKey.wasPressedThisFrame; }
        }

        public static bool BackPressed
        {
            get { var k = Keyboard; return k != null && k.escapeKey.wasPressedThisFrame; }
        }

        public static bool SubmitPressed
        {
            get
            {
                var k = Keyboard;
                if (k == null) return false;
                return k.spaceKey.wasPressedThisFrame || k.enterKey.wasPressedThisFrame ||
                       k.numpadEnterKey.wasPressedThisFrame;
            }
        }

        public static bool AdvancePressed
        {
            get
            {
                var k = Keyboard;
                return k != null && (k.spaceKey.wasPressedThisFrame || k.enterKey.wasPressedThisFrame);
            }
        }

        public static bool SpacePressed
        {
            get { var k = Keyboard; return k != null && k.spaceKey.wasPressedThisFrame; }
        }

        public static bool ConfirmPressed
        {
            get
            {
                var k = Keyboard;
                return k != null && (k.enterKey.wasPressedThisFrame || k.numpadEnterKey.wasPressedThisFrame);
            }
        }

        public static bool MenuUpPressed
        {
            get
            {
                var k = Keyboard;
                return k != null && (k.wKey.wasPressedThisFrame || k.upArrowKey.wasPressedThisFrame);
            }
        }

        public static bool MenuDownPressed
        {
            get
            {
                var k = Keyboard;
                return k != null && (k.sKey.wasPressedThisFrame || k.downArrowKey.wasPressedThisFrame);
            }
        }

        public static int ChoiceNumberPressed
        {
            get
            {
                var k = Keyboard;
                if (k == null) return 0;
                if (k.digit1Key.wasPressedThisFrame || k.numpad1Key.wasPressedThisFrame) return 1;
                if (k.digit2Key.wasPressedThisFrame || k.numpad2Key.wasPressedThisFrame) return 2;
                if (k.digit3Key.wasPressedThisFrame || k.numpad3Key.wasPressedThisFrame) return 3;
                return 0;
            }
        }

        public static bool AnyDirectionPressed
        {
            get
            {
                var k = Keyboard;
                if (k == null) return false;
                return k.wKey.wasPressedThisFrame || k.sKey.wasPressedThisFrame ||
                       k.aKey.wasPressedThisFrame || k.dKey.wasPressedThisFrame ||
                       k.upArrowKey.wasPressedThisFrame || k.downArrowKey.wasPressedThisFrame ||
                       k.leftArrowKey.wasPressedThisFrame || k.rightArrowKey.wasPressedThisFrame;
            }
        }

        public static bool AnyMovementHeld
        {
            get
            {
                var k = Keyboard;
                if (k == null) return false;
                return k.aKey.isPressed || k.dKey.isPressed || k.leftArrowKey.isPressed || k.rightArrowKey.isPressed;
            }
        }

        public static bool SkipHeld
        {
            get { var k = Keyboard; return k != null && k.escapeKey.isPressed; }
        }
    }
}
