using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021
{
    public enum MouseButton
    {
        Left,
        Right,
        Middle,
        X1,
        X2,
    }

    public struct InputState
    {
        public InputState(KeyboardState keyboard, MouseState mouse, GamePadState gamepad, string inputText)
        {
            Keyboard = keyboard;
            Mouse = mouse;
            GamePad = gamepad;
            InputText = inputText;
        }

        public KeyboardState Keyboard;
        public MouseState Mouse;
        public GamePadState GamePad;
        public string InputText;

        public bool IsMouseDown(MouseButton button)
        {
            switch (button)
            {
                case MouseButton.Left:
                    return Mouse.LeftButton == ButtonState.Pressed;
                case MouseButton.Right:
                    return Mouse.RightButton == ButtonState.Pressed;
                case MouseButton.Middle:
                    return Mouse.MiddleButton == ButtonState.Pressed;
                case MouseButton.X1:
                    return Mouse.XButton1 == ButtonState.Pressed;
                case MouseButton.X2:
                    return Mouse.XButton2 == ButtonState.Pressed;
            }
            return false;
        }

        public bool IsMouseUp(MouseButton button)
        {
            return !IsMouseDown(button);
        }
    }

    public class InputTwinState
    {
        class KeyRepeat
        {
            public int TimePressed;
            public int RepeatCount;
        }

        public InputTwinState(InputState previous, InputState next, InputTwinState old)
        {
            Previous = previous;
            Next = next;
            if (old != null)
            {
                KeyRepeats = old.KeyRepeats;
                ButtonRepeats = old.ButtonRepeats;
            }
            else
            {
                KeyRepeats = new Dictionary<Keys, KeyRepeat>();
                ButtonRepeats = new Dictionary<Buttons, KeyRepeat>();
            }

        }

        public InputState Previous;
        public InputState Next;
        Dictionary<Keys, KeyRepeat> KeyRepeats;
        Dictionary<Buttons, KeyRepeat> ButtonRepeats;

        public int MouseX => Next.Mouse.X;
        public int MouseY => Next.Mouse.Y;

        public void HandleRepeats()
        {
            foreach (var keyRepeat in KeyRepeats)
            {
                if (IsKeyDown(keyRepeat.Key))
                    keyRepeat.Value.TimePressed++;
                else
                    keyRepeat.Value.TimePressed = 0;
            }

            foreach (var buttonRepeat in ButtonRepeats)
            {
                if (IsButtonDown(buttonRepeat.Key))
                    buttonRepeat.Value.TimePressed++;
                else
                    buttonRepeat.Value.TimePressed = 0;
            }
        }

        public bool IsMouseWheelUp()
        {
            int previous = Previous.Mouse.ScrollWheelValue;
            int next = Next.Mouse.ScrollWheelValue;
            return next > previous;
        }

        public bool IsMouseWheelDown()
        {
            int previous = Previous.Mouse.ScrollWheelValue;
            int next = Next.Mouse.ScrollWheelValue;
            return next < previous;
        }

        public bool IsMouseDown(MouseButton button)
        {
            return Next.IsMouseDown(button);
        }

        public bool IsMouseUp(MouseButton button)
        {
            return Next.IsMouseUp(button);
        }

        public bool IsMousePressed(MouseButton button)
        {
            return Previous.IsMouseUp(button) && Next.IsMouseDown(button);
        }

        public bool IsMouseReleased(MouseButton button)
        {
            return Previous.IsMouseDown(button) && Next.IsMouseUp(button);
        }

        public bool IsKeyDown(Keys key)
        {
            return Next.Keyboard.IsKeyDown(key);
        }

        public bool IsKeyUp(Keys key)
        {
            return Next.Keyboard.IsKeyUp(key);
        }

        public bool IsKeyPressed(Keys key)
        {
            return Next.Keyboard.IsKeyDown(key) && Previous.Keyboard.IsKeyUp(key);
        }

        public bool IsKeyPressed(Keys key, int repeatStart, int repeatStep)
        {
            KeyRepeat repeat = new KeyRepeat();
            if (!KeyRepeats.ContainsKey(key))
                KeyRepeats.Add(key, new KeyRepeat());
            else
                repeat = KeyRepeats[key];
            return IsKeyPressed(key) || (repeat.TimePressed > repeatStart && (repeat.TimePressed - repeatStart) % repeatStep == 0);
        }

        public bool IsKeyPressed(Keys key, int delay)
        {
            KeyRepeat repeat = new KeyRepeat();
            if (!KeyRepeats.ContainsKey(key))
                KeyRepeats.Add(key, new KeyRepeat());
            else
                repeat = KeyRepeats[key];
            return repeat.TimePressed == delay;
        }

        public bool IsKeyReleased(Keys key)
        {
            return Next.Keyboard.IsKeyUp(key) && Previous.Keyboard.IsKeyDown(key);
        }

        public bool IsButtonDown(Buttons button)
        {
            return Next.GamePad.IsButtonDown(button);
        }

        public bool IsButtonUp(Buttons button)
        {
            return Next.GamePad.IsButtonUp(button);
        }

        public bool IsButtonPressed(Buttons button)
        {
            return Next.GamePad.IsButtonDown(button) && Previous.GamePad.IsButtonUp(button);
        }

        public bool IsButtonPressed(Buttons button, int repeatStart, int repeatStep)
        {
            KeyRepeat repeat = new KeyRepeat();
            if (!ButtonRepeats.ContainsKey(button))
                ButtonRepeats.Add(button, new KeyRepeat());
            else
                repeat = ButtonRepeats[button];
            return IsButtonPressed(button) || (repeat.TimePressed > repeatStart && (repeat.TimePressed - repeatStart) % repeatStep == 0);
        }

        public bool IsButtonPressed(Buttons button, int delay)
        {
            KeyRepeat repeat = new KeyRepeat();
            if (!ButtonRepeats.ContainsKey(button))
                ButtonRepeats.Add(button, new KeyRepeat());
            else
                repeat = ButtonRepeats[button];
            return repeat.TimePressed == delay;
        }

        public bool IsButtonReleased(Buttons button)
        {
            return Next.GamePad.IsButtonUp(button) && Previous.GamePad.IsButtonDown(button);
        }

        public void AddText(ref string text)
        {
            StringBuilder builder = new StringBuilder(text);
            foreach (var chr in Next.InputText)
            {
                switch (chr)
                {
                    case ('\b'): //Backspace
                        if (builder.Length > 0)
                        {
                            builder.Remove(builder.Length - 1, 1);
                        }
                        break;
                    case ('\t'): //Tab
                    case ((char)27): //Escape
                        break;
                    default:
                        builder.Append(chr);
                        break;
                }
            }
            text = builder.ToString();
        }
    }
}
