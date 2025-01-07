using MiniAudioEx;
using MiniAudioEx.DSP;

namespace Gowtu
{
    public sealed class NoiseGenerator : IAudioGenerator
    {
        private Wavetable wavetable;
        private NoiseCalculator calculator;
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

        public NoiseGenerator(NoiseType type)
        {
            calculator = new NoiseCalculator(type);
            wavetable = new Wavetable(calculator, 1024);
            frequency = 0.5f;
        }

        public void OnDestroy()
        {

        }

        public void OnGenerate(MiniAudioEx.AudioBuffer<float> framesOut, ulong frameCount, int channels)
        {
            float sample = 0;

            for(int i = 0; i < framesOut.Length; i += channels)
            {
                sample = wavetable.GetValue(frequency, AudioSettings.OutputSampleRate);

                for(int j = 0; j < channels; j++)
                {
                    framesOut[i+j] = sample;
                }
            }
        }
    }
}