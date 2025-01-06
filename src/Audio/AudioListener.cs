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

using System;
using MiniAudioEx.Core;

namespace Gowtu
{
    /// <summary>
    /// This class represents a point in the 3D space where audio is perceived or heard.
    /// </summary>
    public sealed class AudioListener : Component
    {
        private IntPtr handle;

        /// <summary>
        /// A handle to the native ma_audio_listener instance.
        /// </summary>
        /// <value></value>
        internal IntPtr Handle
        {
            get
            {
                return handle;
            }
        }

        /// <summary>
        /// If true, then spatialization is enabled for this listener.
        /// </summary>
        /// <value></value>
        public bool Enabled
        {
            get
            {
                return Library.ma_ex_audio_listener_get_spatialization(handle) > 0;
            }
            set
            {
                Library.ma_ex_audio_listener_set_spatialization(handle, value ? (uint)1 : 0);
            }
        }


        public AudioListener() : base()
        {

        }

        internal override void OnInitializeComponent()
        {
            handle = Library.ma_ex_audio_listener_init(Audio.NativeContext);

            if(handle != IntPtr.Zero)
            {
                Audio.Add(this);
            }
        }

        internal override void OnDestroyComponent()
        {
            Audio.Remove(this);
        }

        internal void Destroy()
        {
            if(handle != IntPtr.Zero)
            {
                Library.ma_ex_audio_listener_uninit(handle);
                handle = IntPtr.Zero;
            }
        }
    }
}