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
using OpenTK.Mathematics;

namespace Gowtu
{
    public enum ButtonCode
     {
        Unknown = -1,
        Left = GLFW.MOUSE_BUTTON_LEFT,
        Button1 = GLFW.MOUSE_BUTTON_1,
        Right = GLFW.MOUSE_BUTTON_RIGHT,
        Button2 = GLFW.MOUSE_BUTTON_2,
        Middle = GLFW.MOUSE_BUTTON_MIDDLE,
        Button3 = GLFW.MOUSE_BUTTON_3,
        Button4 = GLFW.MOUSE_BUTTON_4,
        Button5 = GLFW.MOUSE_BUTTON_5,
        Button6 = GLFW.MOUSE_BUTTON_6,
        Button7 = GLFW.MOUSE_BUTTON_7,
        Button8 = GLFW.MOUSE_BUTTON_8
    }

    public sealed class ButtonState 
    {
        public int down;
        public int up;
        public int pressed;
        public int state;
        public double lastRepeatTime;
        public bool repeat;

        public ButtonState() 
        {
            down = 0;
            up = 0;
            pressed = 0;
            state = 0;
            lastRepeatTime = 0.0;
            repeat = false;
        }
    };

    public delegate void ButtonDownEvent(ButtonCode buttoncode);
    public delegate void ButtonUpEvent(ButtonCode buttoncode);
    public delegate void ButtonPressEvent(ButtonCode buttoncode);
    public delegate void ButtonRepeatEvent(ButtonCode buttoncode);
    public delegate void MouseScrollEvent(float offsetX, float offsetY);

    public sealed class Mouse
    {
        public event ButtonDownEvent ButtonDown;
        public event ButtonUpEvent ButtonUp;
        public event ButtonPressEvent ButtonPress;
        public event ButtonRepeatEvent ButtonRepeat;
        public event MouseScrollEvent Scroll;

        private Dictionary<ButtonCode,ButtonState> states;
        private float positionX;
        private float positionY;
        private float deltaX;
        private float deltaY;
        private float scrollX;
        private float scrollY;
        private float windowPositionX;
        private float windowPositionY;
        private double repeatDelay;
        private double repeatInterval;
        private bool cursorVisible;

        public Mouse()
        {
            repeatDelay = 0.5; // Delay before repeat starts
            repeatInterval = 0.025; // Interval for repeat

            positionX = 0.0f;
            positionY = 0.0f;
            deltaX = 0.0f;
            deltaY = 0.0f;
            scrollX = 0.0f;
            scrollY = 0.0f;
            windowPositionX = 0.0f;
            windowPositionY = 0.0f;

            cursorVisible = true;

            states = new Dictionary<ButtonCode, ButtonState>();

            states.Add(ButtonCode.Left, new ButtonState());
            states.Add(ButtonCode.Right, new ButtonState());
            states.Add(ButtonCode.Middle, new ButtonState());
            states.Add(ButtonCode.Button4, new ButtonState());
            states.Add(ButtonCode.Button5, new ButtonState());
            states.Add(ButtonCode.Button6, new ButtonState());
            states.Add(ButtonCode.Button7, new ButtonState());
            states.Add(ButtonCode.Button8, new ButtonState());
        }

        public void NewFrame() {
            foreach(var k in states) 
            {
                ButtonCode button = k.Key;
                int state = k.Value.state;

                if(state > 0) {
                    if(states[button].down == 0) 
                    {
                        states[button].down = 1;
                        states[button].pressed = 0;
                        states[button].lastRepeatTime = GLFW.GetTime();
                        states[button].repeat = false;
                        ButtonDown?.Invoke(button);
                    } 
                    else 
                    {
                        states[button].down = 1;
                        states[button].pressed = 1;

                        double currentTime = GLFW.GetTime();
                        double elapsed = currentTime - states[button].lastRepeatTime;

                        if (!states[button].repeat) 
                        {
                            if (elapsed >= repeatDelay) 
                            {
                                ButtonRepeat?.Invoke(button);
                                states[button].repeat = true;
                                states[button].lastRepeatTime = currentTime;
                            }
                        } 
                        else 
                        {
                            if (elapsed >= repeatInterval) 
                            {
                                ButtonRepeat?.Invoke(button);
                                states[button].lastRepeatTime = currentTime;
                            }
                        }

                        ButtonPress?.Invoke(button);
                    }

                    states[button].up = 0;
                } 
                else 
                {
                    if(states[button].down == 1 || states[button].pressed == 1) 
                    {
                        states[button].down = 0;
                        states[button].pressed = 0;
                        states[button].up = 1;
                        ButtonUp?.Invoke(button);
                    } 
                    else 
                    {
                        states[button].down = 0;
                        states[button].pressed = 0;
                        states[button].up = 0;
                    }
                }
            }
        }

        public void EndFrame() 
        {
            deltaX = 0.0f;
            deltaY = 0.0f;
            scrollX = 0.0f;
            scrollY = 0.0f;
        }

        public bool IsCursorVisible()
        {
            return cursorVisible;
        }

        public void SetPosition(double x, double y) 
        {
            float prevX = positionX;
            float prevY = positionY;

            positionX = (float)x;
            positionY = (float)y;

            deltaX = (float)x - prevX;
            deltaY = (float)y - prevY;
        }

        public void SetWindowPosition(double x, double y) {
            windowPositionX = (float)x;
            windowPositionY = (float)y;
        }

        public Vector2 GetPosition()
        {
            return new Vector2(positionX, positionY);
        }

        public Vector2 GetAbsolutePosition()
        {
            return new Vector2(windowPositionX + positionX, windowPositionY + positionY);
        }

        public Vector2 GetDelta()
        {
            return new Vector2(deltaX, deltaY);
        }

        public Vector2 GetScroll()
        {
            return new Vector2(scrollX, scrollY);
        }

        public void SetScrollDirection(double x, double y) 
        {
            scrollX = (float)x;
            scrollY = (float)y;
            Scroll?.Invoke(scrollX, scrollY);
        }

        public void SetState(ButtonCode buttoncode, int state) 
        {
            states[buttoncode].state = state;
        }

        public void SetCursor(bool visible)
        {
            GLFW.SetInputMode(Application.NativeWindow, GLFW.CURSOR, visible ? GLFW.CURSOR_NORMAL : GLFW.CURSOR_DISABLED);
            cursorVisible = visible;
        }

        public bool GetState(ButtonCode button)
        {
            return states[button].state > 0;
        }

        public bool GetButton(ButtonCode buttoncode) 
        {
            ButtonState state = states[buttoncode];
            return state.down == 1 && state.pressed == 1;
        }

        public bool GetButtonDown(ButtonCode buttoncode) 
        {
            ButtonState state = states[buttoncode];
            return state.down == 1 && state.pressed == 0;
        }

        public bool GetButtonUp(ButtonCode buttoncode) 
        {
            ButtonState state = states[buttoncode];
            return state.up > 0;
        }

        public bool GetAnyButtonDown(out ButtonCode keycode)
        {
            keycode = ButtonCode.Left;

            foreach(var item in states)
            {
                if(GetButtonDown(item.Key))
                {
                    keycode = item.Key;
                    return true;
                }
            }

            return false;
        }
    }
}