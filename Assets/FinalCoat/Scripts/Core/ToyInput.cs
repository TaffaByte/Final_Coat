using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif
namespace FinalCoat{
    public enum ControlScheme { WASD, Arrows }
    public enum GameKey { Left, Right, Jump, Tug, Hold }
    public static class ToyInput{
        public const float StickDeadZone = 0.25f;
        enum RawKey { A, D, W, S, LeftShift, LeftArrow, RightArrow, UpArrow, DownArrow, RightShift, Escape, R, F1 }
        public static float Horizontal(ControlScheme scheme){
            float x = 0f;
            if (Held(scheme, GameKey.Left)) x -= 1f;
            if (Held(scheme, GameKey.Right)) x += 1f;
            if (x == 0f) x = PadHorizontal(PadIndex(scheme));
            return x;
        }

        public static bool Held(ControlScheme scheme, GameKey key){
            if (key == GameKey.Left || key == GameKey.Right) return KeyHeld(Map(scheme, key));
            return KeyHeld(Map(scheme, key)) || PadHeld(PadIndex(scheme), key);
        }

        public static bool Pressed(ControlScheme scheme, GameKey key) => KeyPressed(Map(scheme, key)) || PadPressed(PadIndex(scheme), key);
        public static bool Released(ControlScheme scheme, GameKey key) => KeyReleased(Map(scheme, key)) || PadReleased(PadIndex(scheme), key);
        public static bool PausePressed => KeyPressed(RawKey.Escape) || AnyPad(true);
        public static bool RestartPressed => KeyPressed(RawKey.R) || AnyPad(false);
        static int PadIndex(ControlScheme scheme) => scheme == ControlScheme.Arrows ? 1 : 0;
        public static bool HelpPressed => KeyPressed(RawKey.F1);
        public static bool BackPressed => KeyPressed(RawKey.Escape) || AnyPadBack();
        static RawKey Map(ControlScheme scheme, GameKey key){
            if (scheme == ControlScheme.WASD){
                switch (key){
                    case GameKey.Left: return RawKey.A;
                    case GameKey.Right: return RawKey.D;
                    case GameKey.Jump: return RawKey.W;
                    case GameKey.Tug: return RawKey.LeftShift;
                    default: return RawKey.S;
                }
            }
            switch (key){
                case GameKey.Left: return RawKey.LeftArrow;
                case GameKey.Right: return RawKey.RightArrow;
                case GameKey.Jump: return RawKey.UpArrow;
                case GameKey.Tug: return RawKey.RightShift;
                default: return RawKey.DownArrow;
            }
        }

#if ENABLE_INPUT_SYSTEM
        static Gamepad Pad(int index){
            var all = Gamepad.all;
            return index < all.Count ? all[index] : null;
        }

        static float PadHorizontal(int index){
            var pad = Pad(index);
            if (pad == null) return 0f;
            float x = pad.leftStick.x.ReadValue();
            if (Mathf.Abs(x) < StickDeadZone) x = 0f;
            if (pad.dpad.left.isPressed) x = -1f;
            if (pad.dpad.right.isPressed) x = 1f;
            return Mathf.Clamp(x, -1f, 1f);
        }

        static ButtonControl PadKey(Gamepad pad, GameKey key){
            switch (key){
                case GameKey.Jump: return pad.buttonSouth;
                case GameKey.Hold: return pad.buttonEast;
                case GameKey.Tug: return pad.buttonWest;
                case GameKey.Left: return pad.dpad.left;
                default: return pad.dpad.right;
            }
        }

        static bool PadHeld(int index, GameKey key){
            var pad = Pad(index);
            return pad != null && PadKey(pad, key).isPressed;
        }

        static bool PadPressed(int index, GameKey key){
            var pad = Pad(index);
            return pad != null && PadKey(pad, key).wasPressedThisFrame;
        }

        static bool PadReleased(int index, GameKey key){
            var pad = Pad(index);
            return pad != null && PadKey(pad, key).wasReleasedThisFrame;
        }

        static bool AnyPadBack(){
            foreach (var pad in Gamepad.all)
                if (pad.buttonEast.wasPressedThisFrame) return true;
            return false;
        }

        static bool AnyPad(bool start){
            foreach (var pad in Gamepad.all){
                var button = start ? pad.startButton : pad.buttonNorth;
                if (button.wasPressedThisFrame) return true;
            }
            return false;
        }

        static Key ToKey(RawKey key){
            switch (key){
                case RawKey.A: return Key.A;
                case RawKey.D: return Key.D;
                case RawKey.W: return Key.W;
                case RawKey.S: return Key.S;
                case RawKey.LeftShift: return Key.LeftShift;
                case RawKey.LeftArrow: return Key.LeftArrow;
                case RawKey.RightArrow: return Key.RightArrow;
                case RawKey.UpArrow: return Key.UpArrow;
                case RawKey.DownArrow: return Key.DownArrow;
                case RawKey.RightShift: return Key.RightShift;
                case RawKey.Escape: return Key.Escape;
                case RawKey.R: return Key.R;
                default: return Key.F1;
            }
        }

        static bool KeyHeld(RawKey key){
            var keyboard = Keyboard.current;
            return keyboard != null && keyboard[ToKey(key)].isPressed;
        }

        static bool KeyPressed(RawKey key){
            var keyboard = Keyboard.current;
            return keyboard != null && keyboard[ToKey(key)].wasPressedThisFrame;
        }

        static bool KeyReleased(RawKey key){
            var keyboard = Keyboard.current;
            return keyboard != null && keyboard[ToKey(key)].wasReleasedThisFrame;
        }

#else
        static float PadHorizontal(int index) => 0f;
        static bool PadHeld(int index, GameKey key) => false;
        static bool PadPressed(int index, GameKey key) => false;
        static bool PadReleased(int index, GameKey key) => false;
        static bool AnyPad(bool start) => false;
        static bool AnyPadBack() => false;
        static KeyCode ToKeyCode(RawKey key){
            switch (key){
                case RawKey.A: return KeyCode.A;
                case RawKey.D: return KeyCode.D;
                case RawKey.W: return KeyCode.W;
                case RawKey.S: return KeyCode.S;
                case RawKey.LeftShift: return KeyCode.LeftShift;
                case RawKey.LeftArrow: return KeyCode.LeftArrow;
                case RawKey.RightArrow: return KeyCode.RightArrow;
                case RawKey.UpArrow: return KeyCode.UpArrow;
                case RawKey.DownArrow: return KeyCode.DownArrow;
                case RawKey.RightShift: return KeyCode.RightShift;
                case RawKey.Escape: return KeyCode.Escape;
                case RawKey.R: return KeyCode.R;
                default: return KeyCode.F1;
            }
        }

        static bool KeyHeld(RawKey key) => Input.GetKey(ToKeyCode(key));
        static bool KeyPressed(RawKey key) => Input.GetKeyDown(ToKeyCode(key));
        static bool KeyReleased(RawKey key) => Input.GetKeyUp(ToKeyCode(key));
#endif
    }
}
