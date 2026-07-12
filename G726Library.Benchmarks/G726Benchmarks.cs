using BenchmarkDotNet.Attributes;
using System;

namespace G726Library.Benchmarks
{
    public class G726Benchmarks
    {
        private readonly short[] _randomSamples = new short[80000];
        private readonly int[] _randomSamplesG726 = new int[80000];

        private static readonly Random s_rand = new(1234);

        [GlobalSetup]
        public void Setup()
        {
            for (int i = 0; i < _randomSamples.Length; i++)
                _randomSamples[i] = (short)s_rand.Next(1, 450);

            for (int i = 0; i < _randomSamplesG726.Length; i++)
                _randomSamplesG726[i] = s_rand.Next(1, 32);
        }

        [Benchmark]
        public void Encode10SecondsOfAudio40Kbps()
        {
            G726Codec codec = G726Codec.Create40Kbps();

            _ = codec.EncodeFromPcm(_randomSamples);
        }

        [Benchmark]
        public void Decode10SecondsOfAudio40Kbps()
        {
            G726Codec codec = G726Codec.Create40Kbps();

            _ = codec.DecodeToPcm(_randomSamplesG726);
        }
    }
}
