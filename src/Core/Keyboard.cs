// MIT License

// Copyright (c) 2025 W.M.R Jap-A-Joe

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections.Generic;
using GLFWNet;

namespace Gowtu
{
    public enum KeyCode 
    {
        Unknown = GLFW.KEY_UNKNOWN,
        Space = GLFW.KEY_SPACE,
        Apostrophe = GLFW.KEY_APOSTROPHE,
        Comma = GLFW.KEY_COMMA,
        Minus = GLFW.KEY_MINUS,
        Period = GLFW.KEY_PERIOD,
        Slash = GLFW.KEY_SLASH,
        Alpha0 = GLFW.KEY_0,
        Alpha1 = GLFW.KEY_1,
        Alpha2 = GLFW.KEY_2,
        Alpha3 = GLFW.KEY_3,
        Alpha4 = GLFW.KEY_4,
        Alpha5 = GLFW.KEY_5,
        Alpha6 = GLFW.KEY_6,
        Alpha7 = GLFW.KEY_7,
        Alpha8 = GLFW.KEY_8,
        Alpha9 = GLFW.KEY_9,
        SemiColon = GLFW.KEY_SEMICOLON,
        Equal = GLFW.KEY_EQUAL,
        A = GLFW.KEY_A,
        B = GLFW.KEY_B,
        C = GLFW.KEY_C,
        D = GLFW.KEY_D,
        E = GLFW.KEY_E,
        F = GLFW.KEY_F,
        G = GLFW.KEY_G,
        H = GLFW.KEY_H,
        I = GLFW.KEY_I,
        J = GLFW.KEY_J,
        K = GLFW.KEY_K,
        L = GLFW.KEY_L,
        M = GLFW.KEY_M,
        N = GLFW.KEY_N,
        O = GLFW.KEY_O,
        P = GLFW.KEY_P,
        Q = GLFW.KEY_Q,
        R = GLFW.KEY_R,
        S = GLFW.KEY_S,
        T = GLFW.KEY_T,
        U = GLFW.KEY_U,
        V = GLFW.KEY_V,
        W = GLFW.KEY_W,
        X = GLFW.KEY_X,
        Y = GLFW.KEY_Y,
        Z = GLFW.KEY_Z,
        LeftBracket = GLFW.KEY_LEFT_BRACKET,
        BackSlash = GLFW.KEY_BACKSLASH,
        RightBracket = GLFW.KEY_RIGHT_BRACKET,
        GraveAccent = GLFW.KEY_GRAVE_ACCENT,    
        World1 = GLFW.KEY_UNKNOWN,
        World2 = GLFW.KEY_UNKNOWN,
        Escape = GLFW.KEY_ESCAPE,
        Enter = GLFW.KEY_ENTER,
        Tab = GLFW.KEY_TAB,
        Backspace = GLFW.KEY_BACKSPACE,
        Insert = GLFW.KEY_INSERT,
        Delete = GLFW.KEY_DELETE,
        Right = GLFW.KEY_RIGHT,
        Left = GLFW.KEY_LEFT,
        Down = GLFW.KEY_DOWN,
        Up = GLFW.KEY_UP,
        PageUp = GLFW.KEY_PAGE_UP,
        PageDown = GLFW.KEY_PAGE_DOWN,
        Home = GLFW.KEY_HOME,
        End = GLFW.KEY_END,
        CapsLock = GLFW.KEY_CAPS_LOCK,
        ScrollLock = GLFW.KEY_SCROLL_LOCK,
        NumLock = GLFW.KEY_NUM_LOCK,
        PrintScreen = GLFW.KEY_PRINT_SCREEN,
        Pause = GLFW.KEY_PAUSE,
        F1 = GLFW.KEY_F1,
        F2 = GLFW.KEY_F2,
        F3 = GLFW.KEY_F3,
        F4 = GLFW.KEY_F4,
        F5 = GLFW.KEY_F5,
        F6 = GLFW.KEY_F6,
        F7 = GLFW.KEY_F7,
        F8 = GLFW.KEY_F8,
        F9 = GLFW.KEY_F9,
        F10 = GLFW.KEY_F10,
        F11 = GLFW.KEY_F11,
        F12 = GLFW.KEY_F12,
        F13 = GLFW.KEY_F13,
        F14 = GLFW.KEY_F14,
        F15 = GLFW.KEY_F15,
        F16 = GLFW.KEY_F16,
        F17 = GLFW.KEY_F17,
        F18 = GLFW.KEY_F18,
        F19 = GLFW.KEY_F19,
        F20 = GLFW.KEY_F20,
        F21 = GLFW.KEY_F21,
        F22 = GLFW.KEY_F22,
        F23 = GLFW.KEY_F23,
        F24 = GLFW.KEY_F24,
        F25 = GLFW.KEY_UNKNOWN,
        Keypad0 = GLFW.KEY_KP_0,
        Keypad1 = GLFW.KEY_KP_1,
        Keypad2 = GLFW.KEY_KP_2,
        Keypad3 = GLFW.KEY_KP_3,
        Keypad4 = GLFW.KEY_KP_4,
        Keypad5 = GLFW.KEY_KP_5,
        Keypad6 = GLFW.KEY_KP_6,
        Keypad7 = GLFW.KEY_KP_7,
        Keypad8 = GLFW.KEY_KP_8,
        Keypad9 = GLFW.KEY_KP_9,
        Decimal = GLFW.KEY_KP_DECIMAL,
        Divide = GLFW.KEY_KP_DIVIDE,
        Multiply = GLFW.KEY_KP_MULTIPLY,
        Subtract = GLFW.KEY_KP_SUBTRACT,
        Add = GLFW.KEY_KP_ADD,
        KeypadEnter = GLFW.KEY_KP_ENTER,
        KeypadEqual = GLFW.KEY_KP_EQUAL,
        LeftShift = GLFW.KEY_LEFT_SHIFT,
        LeftControl = GLFW.KEY_LEFT_CONTROL,
        LeftAlt = GLFW.KEY_LEFT_ALT,
        LeftSuper = GLFW.KEY_LEFT_SUPER,
        RightShift = GLFW.KEY_RIGHT_SHIFT,
        RightControl = GLFW.KEY_RIGHT_CONTROL,
        RightAlt = GLFW.KEY_RIGHT_ALT,
        RightSuper = GLFW.KEY_RIGHT_SUPER,
        Menu = GLFW.KEY_MENU
    }

    public sealed class KeyState 
    {
        public int down;
        public int up;
        public int pressed;
        public int state;
        public double lastRepeatTime;
        public bool repeat;

        public KeyState() 
        {
            down = 0;
            up = 0;
            pressed = 0;
            state = 0;
            lastRepeatTime = 0.0;
            repeat = false;
        }
    }

    public delegate void CharPressEvent(uint codepoint);
    public delegate void KeyDownEvent(KeyCode keycode);
    public delegate void KeyUpEvent(KeyCode keycode);
    public delegate void KeyPressEvent(KeyCode keycode);
    public delegate void KeyRepeatEvent(KeyCode keycode);

    public sealed class Keyboard
    {
        public event CharPressEvent CharPress;
        public event KeyDownEvent KeyDown;
        public event KeyUpEvent KeyUp;
        public event KeyPressEvent KeyPress;
        public event KeyRepeatEvent KeyRepeat;

        private Dictionary<KeyCode,KeyState> states;
        private double repeatDelay;
        private double repeatInterval;

        public Keyboard() 
        {
            repeatDelay = 0.5; // Delay before repeat starts
            repeatInterval = 0.025; // Interval for repeat

            states = new Dictionary<KeyCode, KeyState>();

            states.Add(KeyCode.Unknown, new KeyState());
            states.Add(KeyCode.Space, new KeyState());
            states.Add(KeyCode.Apostrophe, new KeyState());
            states.Add(KeyCode.Comma, new KeyState());
            states.Add(KeyCode.Minus, new KeyState());
            states.Add(KeyCode.Period, new KeyState());
            states.Add(KeyCode.Slash, new KeyState());
            states.Add(KeyCode.Alpha0, new KeyState());
            states.Add(KeyCode.Alpha1, new KeyState());
            states.Add(KeyCode.Alpha2, new KeyState());
            states.Add(KeyCode.Alpha3, new KeyState());
            states.Add(KeyCode.Alpha4, new KeyState());
            states.Add(KeyCode.Alpha5, new KeyState());
            states.Add(KeyCode.Alpha6, new KeyState());
            states.Add(KeyCode.Alpha7, new KeyState());
            states.Add(KeyCode.Alpha8, new KeyState());
            states.Add(KeyCode.Alpha9, new KeyState());
            states.Add(KeyCode.SemiColon, new KeyState());
            states.Add(KeyCode.Equal, new KeyState());
            states.Add(KeyCode.A, new KeyState());
            states.Add(KeyCode.B, new KeyState());
            states.Add(KeyCode.C, new KeyState());
            states.Add(KeyCode.D, new KeyState());
            states.Add(KeyCode.E, new KeyState());
            states.Add(KeyCode.F, new KeyState());
            states.Add(KeyCode.G, new KeyState());
            states.Add(KeyCode.H, new KeyState());
            states.Add(KeyCode.I, new KeyState());
            states.Add(KeyCode.J, new KeyState());
            states.Add(KeyCode.K, new KeyState());
            states.Add(KeyCode.L, new KeyState());
            states.Add(KeyCode.M, new KeyState());
            states.Add(KeyCode.N, new KeyState());
            states.Add(KeyCode.O, new KeyState());
            states.Add(KeyCode.P, new KeyState());
            states.Add(KeyCode.Q, new KeyState());
            states.Add(KeyCode.R, new KeyState());
            states.Add(KeyCode.S, new KeyState());
            states.Add(KeyCode.T, new KeyState());
            states.Add(KeyCode.U, new KeyState());
            states.Add(KeyCode.V, new KeyState());
            states.Add(KeyCode.W, new KeyState());
            states.Add(KeyCode.X, new KeyState());
            states.Add(KeyCode.Y, new KeyState());
            states.Add(KeyCode.Z, new KeyState());
            states.Add(KeyCode.LeftBracket, new KeyState());
            states.Add(KeyCode.BackSlash, new KeyState());
            states.Add(KeyCode.RightBracket, new KeyState());
            states.Add(KeyCode.GraveAccent, new KeyState());
            states.Add(KeyCode.Escape, new KeyState());
            states.Add(KeyCode.Enter, new KeyState());
            states.Add(KeyCode.Tab, new KeyState());
            states.Add(KeyCode.Backspace, new KeyState());
            states.Add(KeyCode.Insert, new KeyState());
            states.Add(KeyCode.Delete, new KeyState());
            states.Add(KeyCode.Right, new KeyState());
            states.Add(KeyCode.Left, new KeyState());
            states.Add(KeyCode.Down, new KeyState());
            states.Add(KeyCode.Up, new KeyState());
            states.Add(KeyCode.PageUp, new KeyState());
            states.Add(KeyCode.PageDown, new KeyState());
            states.Add(KeyCode.Home, new KeyState());
            states.Add(KeyCode.End, new KeyState());
            states.Add(KeyCode.CapsLock, new KeyState());
            states.Add(KeyCode.ScrollLock, new KeyState());
            states.Add(KeyCode.NumLock, new KeyState());
            states.Add(KeyCode.PrintScreen, new KeyState());
            states.Add(KeyCode.Pause, new KeyState());
            states.Add(KeyCode.F1, new KeyState());
            states.Add(KeyCode.F2, new KeyState());
            states.Add(KeyCode.F3, new KeyState());
            states.Add(KeyCode.F4, new KeyState());
            states.Add(KeyCode.F5, new KeyState());
            states.Add(KeyCode.F6, new KeyState());
            states.Add(KeyCode.F7, new KeyState());
            states.Add(KeyCode.F8, new KeyState());
            states.Add(KeyCode.F9, new KeyState());
            states.Add(KeyCode.F10, new KeyState());
            states.Add(KeyCode.F11, new KeyState());
            states.Add(KeyCode.F12, new KeyState());
            states.Add(KeyCode.F13, new KeyState());
            states.Add(KeyCode.F14, new KeyState());
            states.Add(KeyCode.F15, new KeyState());
            states.Add(KeyCode.F16, new KeyState());
            states.Add(KeyCode.F17, new KeyState());
            states.Add(KeyCode.F18, new KeyState());
            states.Add(KeyCode.F19, new KeyState());
            states.Add(KeyCode.F20, new KeyState());
            states.Add(KeyCode.F21, new KeyState());
            states.Add(KeyCode.F22, new KeyState());
            states.Add(KeyCode.F23, new KeyState());
            states.Add(KeyCode.F24, new KeyState());
            states.Add(KeyCode.Keypad0, new KeyState());
            states.Add(KeyCode.Keypad1, new KeyState());
            states.Add(KeyCode.Keypad2, new KeyState());
            states.Add(KeyCode.Keypad3, new KeyState());
            states.Add(KeyCode.Keypad4, new KeyState());
            states.Add(KeyCode.Keypad5, new KeyState());
            states.Add(KeyCode.Keypad6, new KeyState());
            states.Add(KeyCode.Keypad7, new KeyState());
            states.Add(KeyCode.Keypad8, new KeyState());
            states.Add(KeyCode.Keypad9, new KeyState());
            states.Add(KeyCode.Decimal, new KeyState());
            states.Add(KeyCode.Divide, new KeyState());
            states.Add(KeyCode.Multiply, new KeyState());
            states.Add(KeyCode.Subtract, new KeyState());
            states.Add(KeyCode.Add, new KeyState());
            states.Add(KeyCode.KeypadEnter, new KeyState());
            states.Add(KeyCode.KeypadEqual, new KeyState());
            states.Add(KeyCode.LeftShift, new KeyState());
            states.Add(KeyCode.LeftControl, new KeyState());
            states.Add(KeyCode.LeftAlt, new KeyState());
            states.Add(KeyCode.LeftSuper, new KeyState());
            states.Add(KeyCode.RightShift, new KeyState());
            states.Add(KeyCode.RightControl, new KeyState());
            states.Add(KeyCode.RightAlt, new KeyState());
            states.Add(KeyCode.RightSuper, new KeyState());
            states.Add(KeyCode.Menu, new KeyState());
        }

        public void NewFrame() 
        {
            foreach(var k in states) 
            {
                KeyCode key = k.Key;
                int state = k.Value.state;

                if(state > 0) 
                {
                    if(states[key].down == 0) 
                    {
                        states[key].down = 1;
                        states[key].pressed = 0;
                        states[key].lastRepeatTime = GLFW.GetTime();
                        states[key].repeat = false;
                        KeyDown?.Invoke(key);
                    } 
                    else 
                    {
                        states[key].down = 1;
                        states[key].pressed = 1;
                        
                        double currentTime = GLFW.GetTime();
                        double elapsed = currentTime - states[key].lastRepeatTime;

                        if (!states[key].repeat) 
                        {
                            if (elapsed >= repeatDelay) 
                            {
                                KeyRepeat?.Invoke(key);
                                states[key].repeat = true;
                                states[key].lastRepeatTime = currentTime;
                            }
                        } 
                        else 
                        {
                            if (elapsed >= repeatInterval) 
                            {
                                KeyRepeat?.Invoke(key);
                                states[key].lastRepeatTime = currentTime;
                            }
                        }
                        
                        KeyPress?.Invoke(key);
                    }

                    states[key].up = 0;
                } 
                else 
                {
                    if(states[key].down == 1 || states[key].pressed == 1) 
                    {
                        states[key].down = 0;
                        states[key].pressed = 0;
                        states[key].up = 1;
                        KeyUp?.Invoke(key);
                    } 
                    else 
                    {
                        states[key].down = 0;
                        states[key].pressed = 0;
                        states[key].up = 0;
                    }
                }
            }
        }

        public bool GetState(KeyCode keycode)
        {
            return states[keycode].state > 0;
        }

        public void SetState(KeyCode keycode, int state) 
        {
            states[keycode].state = state;
        }

        public void AddInputCharacter(uint codepoint) 
        {
            if(codepoint > 0) 
            {
                CharPress?.Invoke(codepoint);
            }
        }

        public bool GetKey(KeyCode keycode) 
        {
            KeyState state = states[keycode];
            return state.down == 1 && state.pressed == 1;
        }

        public bool GetKeyDown(KeyCode keycode) 
        {
            KeyState state = states[keycode];
            return state.down == 1 && state.pressed == 0;
        }

        public bool GetKeyUp(KeyCode keycode) 
        {
            KeyState state = states[keycode];
            return state.up > 0;
        }

        public bool IsAnyKeyPressed() 
        {
            foreach(var k in states) 
            {
                if(k.Value.pressed > 0)
                    return true;
            }

            return false;
        }

        public bool GetAnyKeyDown(out KeyCode keycode)
        {
            keycode = KeyCode.Unknown;

            foreach(var item in states)
            {
                if(GetKeyDown(item.Key))
                {
                    keycode = item.Key;
                    return true;
                }
            }

            return false;
        }
    }
}