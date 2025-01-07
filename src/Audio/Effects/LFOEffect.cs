using System;
using MiniAudioEx;
using MiniAudioEx.DSP;

namespace Gowtu
{
    public sealed class LFOEffect : IAudioEffect
    {
        private Wavetable wavetable;
        private WaveCalculator calculator;
        private float frequency;

        public float Frequency
        {
            get
            {
                return frequency;
            }
            set
            {
                frequency = value;
            }
        }

        public LFOEffect(WaveType type)
        {
            calculator = new WaveCalculator(type);
            wavetable = new Wavetable(calculator, 1024);
            frequency = 4.0f;
        }

        public void OnDestroy()
        {

        }

        public void OnProcess(MiniAudioEx.AudioBuffer<float> framesOut, ulong frameCount, int channels)
        {
            float sample = 0;

            for(int i = 0; i < framesOut.Length; i += channels)
            {
                sample = Math.Abs(wavetable.GetValue(frequency, AudioSettings.OutputSampleRate));

                for(int j = 0; j < channels; j++)
                {
                    framesOut[i+j] *= sample;
                }
            }
        }
    }
}