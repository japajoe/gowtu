using System;
using MiniAudioEx;
using MiniAudioEx.DSP;

namespace Gowtu
{
    public sealed class FadeInEffect : IAudioEffect
    {
        private float fadeDuration; // Duration of the fade-in in seconds
        private ulong totalFrames; // Total frames to process
        private ulong processedFrames; // Frames processed so far

        public float FadeDuration
        {
            get { return fadeDuration; }
            set { fadeDuration = value; }
        }

        public FadeInEffect(float duration)
        {
            fadeDuration = duration;
            processedFrames = 0;
        }

        public void OnDestroy()
        {

        }

        public void OnProcess(MiniAudioEx.AudioBuffer<float> framesOut, ulong frameCount, int channels)
        {
            totalFrames = frameCount;
            float fadeStep = 1.0f / (fadeDuration * AudioSettings.OutputSampleRate); // Calculate fade step

            for (int i = 0; i < framesOut.Length; i++)
            {
                // Calculate the fade factor based on processed frames
                float fadeFactor = Math.Min(1.0f, processedFrames * fadeStep);

                fadeFactor = EaseOutCubic(fadeFactor);

                for (int j = 0; j < channels; j++)
                {
                    framesOut[i+j] *= fadeFactor; // Apply fade factor
                }

                processedFrames++;
            }
        }

        public void Reset()
        {
            processedFrames = 0;
        }

        private float EaseInCubic(float x)
        {
            x = Math.Max(0, Math.Min(1.0f, x));
            return x * x * x;
        }

        private float EaseOutCubic(float x)
        {
            x = Math.Max(0, Math.Min(1.0f, x));
            return 1.0f - (float)Math.Pow(1 - x, 3);
        }
    }
}