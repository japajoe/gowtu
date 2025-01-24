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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MiniAudioEx;
using MiniAudioEx.Core;

namespace Gowtu
{
    /// <summary>
    /// This class is responsible for managing the audio context.
    /// </summary>
    public static class Audio
    {
        private static IntPtr audioContext;
        private static ma_device_data_proc deviceDataProc;
        private static List<AudioSource> audioSources = new List<AudioSource>();
        private static List<AudioListener> audioListeners = new List<AudioListener>();
        private static Dictionary<UInt64, IntPtr> audioClipHandles = new Dictionary<UInt64, IntPtr>();

        private static UInt32 sampleRate = 44100;
        private static UInt32 channels = 2;
        private static DateTime lastUpdateTime;

        public static event DeviceDataEvent DataProcess;

        internal static IntPtr NativeContext
        {
            get
            {
                return audioContext;
            }
        }

        /// <summary>
        /// Gets the chosen sample rate.
        /// </summary>
        /// <value></value>
        public static Int32 SampleRate
        {
            get
            {
                return (int)sampleRate;
            }
        }

        public static Int32 Channels
        {
            get
            {
                return (int)channels;
            }            
        }

        /// <summary>
        /// Controls the master volume.
        /// </summary>
        /// <value></value>
        public static float MasterVolume
        {
            get
            {
                return Library.ma_ex_context_get_master_volume(audioContext);
            }
            set
            {
                Library.ma_ex_context_set_master_volume(audioContext, value);
            }
        }

        /// <summary>
        /// Initializes MiniAudioEx. Call this once at the start of your application. The 'deviceInfo' parameter can be left null.
        /// </summary>
        /// <param name="sampleRate"></param>
        /// <param name="channels"></param>
        /// <param name="deviceInfo"></param>
        internal static void Initialize(UInt32 sampleRate, UInt32 channels, DeviceInfo deviceInfo = null)
        {
            if(audioContext != IntPtr.Zero)
                return;

            ma_ex_device_info pDeviceInfo = new ma_ex_device_info();
            pDeviceInfo.index = deviceInfo == null ? 0 : deviceInfo.Index;
            pDeviceInfo.pName = IntPtr.Zero;

            Audio.sampleRate = sampleRate;
            Audio.channels = channels;

            ma_ex_context_config contextConfig = Library.ma_ex_context_config_init(sampleRate, (byte)channels, 0, ref pDeviceInfo);
            
            deviceDataProc = OnDeviceDataProc;
            contextConfig.deviceDataProc = deviceDataProc;
            
            audioContext = Library.ma_ex_context_init(ref contextConfig);

            if(audioContext == IntPtr.Zero)
            {
                Console.WriteLine("Failed to initialize MiniAudioEx");
            }

            lastUpdateTime = DateTime.Now;
        }

        /// <summary>
        /// Deinitializes MiniAudioEx. Call this before closing the application.
        /// </summary>
        internal static void Deinitialize()
        {
            if(audioContext == IntPtr.Zero)
                return;

            for(int i = 0; i < audioSources.Count; i++)
                audioSources[i].Destroy();

            audioSources.Clear();

            foreach(var audioClipHandle in audioClipHandles.Values)
            {
                if(audioClipHandle != IntPtr.Zero)
                    Marshal.FreeHGlobal(audioClipHandle);
            }

            audioClipHandles.Clear();

            for(int i = 0; i < audioListeners.Count; i++)
                audioListeners[i].Destroy();

            audioListeners.Clear();

            Library.ma_ex_context_uninit(audioContext);
            audioContext = IntPtr.Zero;
        }

        /// <summary>
        /// Used to calculate delta time and move messages from the audio thread to the main thread. Call this method from within your main thread loop.
        /// </summary>
        internal static void NewFrame()
        {
            if(audioContext == IntPtr.Zero)
                return;

            for(int i = 0; i < audioSources.Count; i++)
            {
                audioSources[i].Update();
            }

            for(int i = 0; i < audioListeners.Count; i++)
            {
                var handle = audioListeners[i].Handle;

                var position = audioListeners[i].transform.position;
                var direction = audioListeners[i].transform.forward;
                var velocity = audioListeners[i].transform.velocity;

                Library.ma_ex_audio_listener_set_position(handle, position.X, position.Y, position.Z);
                Library.ma_ex_audio_listener_set_direction(handle, direction.X, direction.Y, direction.Z);
                Library.ma_ex_audio_listener_set_velocity(handle, velocity.X, velocity.Y, velocity.Z);
            }

            for(int i = 0; i < audioSources.Count; i++)
            {
                if(!audioSources[i].Spatial)
                    continue;
                
                var handle = audioSources[i].Handle;
                var position = audioSources[i].transform.position;
                var direction = audioSources[i].transform.forward;
                var velocity = audioSources[i].transform.velocity;

                Library.ma_ex_audio_source_set_position(handle, position.X, position.Y, position.Z);
                Library.ma_ex_audio_source_set_direction(handle, direction.X, direction.Y, direction.Z);
                Library.ma_ex_audio_source_set_velocity(handle, velocity.X, velocity.Y, velocity.Z);
            }
        }

        /// <summary>
        /// Gets an array of available playback devices.
        /// </summary>
        /// <returns></returns>
        public static DeviceInfo[] GetDevices()
        {
            IntPtr pDevices = Library.ma_ex_playback_devices_get(out UInt32 count);

            if(pDevices == IntPtr.Zero)
                return null;

            if(count == 0)
            {
                Library.ma_ex_playback_devices_free(pDevices, count);
                return null;
            }
            
            DeviceInfo[] devices = new DeviceInfo[count];

            for (UInt32 i = 0; i < count; i++)
            {
                IntPtr elementPtr = IntPtr.Add(pDevices, (int)i * Marshal.SizeOf<ma_ex_device_info>());
                ma_ex_device_info deviceInfo = Marshal.PtrToStructure<ma_ex_device_info>(elementPtr);
                devices[i] = new DeviceInfo(deviceInfo.pName, deviceInfo.index, deviceInfo.isDefault > 0 ? true : false);
            }

            Library.ma_ex_playback_devices_free(pDevices, count);
            
            return devices;
        }

        internal static void Add(AudioSource source)
        {
            int hashcode = source.GetHashCode();

            for(int i = 0; i < audioSources.Count; i++)
            {
                if(audioSources[i].GetHashCode() == hashcode)
                    return;
            }

            audioSources.Add(source);
        }

        internal static void Add(AudioClip clip)
        {
            if(clip.Hash == 0)
                return;

            if(clip.Handle == IntPtr.Zero)
                return;
            
            if(audioClipHandles.ContainsKey(clip.Hash))
                return;
            
            audioClipHandles.Add(clip.Hash, clip.Handle);
        }

        internal static void Add(AudioListener listener)
        {
            int hashcode = audioListeners.GetHashCode();

            for(int i = 0; i < audioListeners.Count; i++)
            {
                if(audioListeners[i].GetHashCode() == hashcode)
                    return;
            }

            audioListeners.Add(listener);
        }        

        internal static void Remove(AudioSource source)
        {
            int hashcode = source.GetHashCode();
            bool found = false;
            int index = 0;

            for(int i = 0; i < audioSources.Count; i++)
            {
                if(audioSources[i].GetHashCode() == hashcode)
                {
                    index = i;
                    found = true;
                    break;
                }
            }

            if(found)
            {
                audioSources[index].Destroy();
                audioSources.RemoveAt(index);
            }
        }

        internal static void Remove(AudioListener listener)
        {
            int hashcode = listener.GetHashCode();
            bool found = false;
            int index = 0;

            for(int i = 0; i < audioListeners.Count; i++)
            {
                if(audioListeners[i].GetHashCode() == hashcode)
                {
                    index = i;
                    found = true;
                    break;
                }
            }

            if(found)
            {
                audioListeners[index].Destroy();
                audioListeners.RemoveAt(index);
            }
        }

        internal static void Remove(AudioClip clip)
        {
            if(clip.Hash == 0)
                return;
            
            if(audioClipHandles.ContainsKey(clip.Hash))
            {
                IntPtr handle = audioClipHandles[clip.Hash];
                if(handle != IntPtr.Zero)
                    Marshal.FreeHGlobal(handle);
                audioClipHandles.Remove(clip.Hash);
            }
        }

        internal static bool GetAudioClipHandle(UInt64 hashcode, out IntPtr handle)
        {
            handle = IntPtr.Zero;

            if(audioClipHandles.ContainsKey(hashcode))
            {
                handle = audioClipHandles[hashcode];
                return true;
            }

            return false;
        }

        private static void OnDeviceDataProc(IntPtr pDevice, IntPtr pOutput, IntPtr pInput, UInt32 frameCount)
        {
            IntPtr pEngine = Library.ma_ex_device_get_user_data(pDevice);
            Library.ma_engine_read_pcm_frames(pEngine, pOutput, frameCount, out _);

            if(DataProcess != null)
            {
                AudioBuffer<float> buffer = new AudioBuffer<float>(pOutput, (Int32)(frameCount * channels));
                DataProcess.Invoke(buffer, frameCount);
            }
        }
    }
}