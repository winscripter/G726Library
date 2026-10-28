// Copyright (c) 2023-2026, winscripter
// Licensed under the MIT License.
// See LICENSE.txt for license details.

namespace G726Library;

/// <summary>
/// A G.726 codec
/// </summary>
public class G726Codec
{
    private readonly IG726Codec g726Codec;

    internal G726Codec(IG726Codec codec) => this.g726Codec = codec;

    /// <summary>
    /// Creates a new G.726 (16 Kbit/s) codec.
    /// </summary>
    /// <returns>A new G.726 codec</returns>
    public static G726Codec Create16Kbps() => new(new Codec16());

    /// <summary>
    /// Creates a new G.726 (24 Kbit/s) codec.
    /// </summary>
    /// <returns>A new G.726 codec</returns>
    public static G726Codec Create24Kbps() => new(new Codec24());

    /// <summary>
    /// Creates a new G.726 (32 Kbit/s) codec.
    /// </summary>
    /// <returns>A new G.726 codec</returns>
    public static G726Codec Create32Kbps() => new(new Codec32());

    /// <summary>
    /// Creates a new G.726 (40 Kbit/s) codec.
    /// </summary>
    /// <returns>A new G.726 codec</returns>
    public static G726Codec Create40Kbps() => new(new Codec40());

    /// <summary>
    /// Encodes the input sample into G.726 where the input sample is PCM.
    /// </summary>
    /// <param name="pcm">The PCM sample</param>
    /// <returns>G.726 sample</returns>
    public int EncodeFromPcm(short pcm) => this.g726Codec.EncodeSample(pcm, SampleKind.Pcm);

    /// <summary>
    /// Encodes the input sample into G.726 where the input sample is G.711A.
    /// </summary>
    /// <param name="g711a">The G.711A sample</param>
    /// <returns>G.726 sample</returns>
    public int EncodeFromG711A(byte g711a) => this.g726Codec.EncodeSample(g711a, SampleKind.G711A);

    /// <summary>
    /// Encodes the input sample into G.726 where the input sample is G.711Mu.
    /// </summary>
    /// <param name="g711Mu">The G.711Mu sample</param>
    /// <returns>G.726 sample</returns>
    public int EncodeFromG711Mu(byte g711Mu) => this.g726Codec.EncodeSample(g711Mu, SampleKind.G711U);

    /// <summary>
    /// Encodes all input PCM samples into G.726.
    /// </summary>
    /// <param name="pcm">Input PCM samples</param>
    /// <returns>Output G.726 samples</returns>
    public int[] EncodeFromPcm(ReadOnlySpan<short> pcm)
    {
#if NETCOREAPP2_1_OR_GREATER
        int[] result = GC.AllocateUninitializedArray<int>(pcm.Length);
#else
        int[] result = new int[pcm.Length];
#endif

        for (int i = 0; i < pcm.Length; i++)
        {
            result[i] = this.EncodeFromPcm(pcm[i]);
        }

        return result;
    }

    /// <summary>
    /// Decodes the input G.726 sample into a PCM sample.
    /// </summary>
    /// <param name="input">The input sample</param>
    /// <returns>A PCM sample</returns>
    public short DecodeToPcm(int input) => (short)this.g726Codec.DecodeSample(input, SampleKind.Pcm);

    /// <summary>
    /// Decodes the input G.726 sample into a G.711A sample.
    /// </summary>
    /// <param name="input">The input sample</param>
    /// <returns>A G.711A sample</returns>
    public byte DecodeToG711A(int input) => (byte)this.g726Codec.DecodeSample(input, SampleKind.G711A);

    /// <summary>
    /// Decodes the input G.726 sample into a G.711Mu sample.
    /// </summary>
    /// <param name="input">The input sample</param>
    /// <returns>A G.711Mu sample</returns>
    public byte DecodeToG711Mu(int input) => (byte)this.g726Codec.DecodeSample(input, SampleKind.G711U);

    /// <summary>
    /// Decodes all input G.726 samples into PCM.
    /// </summary>
    /// <param name="g726">Input G.726 samples</param>
    /// <returns>Output PCM samples</returns>
    public short[] DecodeToPcm(ReadOnlySpan<int> g726)
    {
#if NETCOREAPP2_1_OR_GREATER
        short[] result = GC.AllocateUninitializedArray<short>(g726.Length);
#else
        short[] result = new short[g726.Length];
#endif
        for (int i = 0; i < g726.Length; i++)
        {
            result[i] = this.DecodeToPcm(g726[i]);
        }

        return result;
    }
}
