// Copyright (c) 2023-2026, winscripter
// Licensed under the MIT License.
// See LICENSE.txt for license details.

using System.Runtime.CompilerServices;

namespace G726Library;

/// <summary>
/// Shared functions used across all G.726 codec bitrates.
/// </summary>
internal static class G726Shared
{
    /// <summary>
    /// Gets a lookup table containing powers of two from 2^0 through 2^14.
    /// </summary>
    /// <value>
    /// A read-only span containing the values 1, 2, 4, 8, and successive powers of two up to 16384.
    /// </value>
    public static ReadOnlySpan<int> Power2Table => [
        1, 2, 4, 8, 0x10, 0x20, 0x40, 0x80,
        0x100, 0x200, 0x400, 0x800, 0x1000, 0x2000, 0x4000];

    /// <summary>
    /// Finds the index of the first table entry greater than the specified value.
    /// </summary>
    /// <param name="value">The value to compare against the table entries.</param>
    /// <param name="table">The table of ascending threshold values.</param>
    /// <param name="size">The number of table entries to examine.</param>
    /// <returns>
    /// The zero-based index of the first entry greater than <paramref name="value"/>,
    /// or <paramref name="size"/> if no such entry exists.
    /// </returns>
    public static int QuantizeWithinRange(int value, ReadOnlySpan<int> table, int size)
    {
        for (int i = 0; i < size; i++)
        {
            if (value < table[i])
            {
                return i;
            }
        }

        return size;
    }

    /// <summary>
    /// Multiplies an integer sample by a fixed-point floating representation.
    /// </summary>
    /// <param name="integer">The signed integer sample.</param>
    /// <param name="fpRepresentation">The encoded multiplier, containing mantissa and exponent fields.</param>
    /// <returns>The signed, scaled product represented as an integer.</returns>
    public static int FloatingMultiply(int integer, int fpRepresentation)
    {
        int anmag = (integer > 0) ? integer : ((-integer) & 0x1FFF);
        int anexp = QuantizeWithinRange(anmag, Power2Table, 15) - 6;
        int anmant = (anmag == 0) ? 32 : (anexp >= 0) ? anmag >> anexp : anmag << -anexp;
        int wanexp = anexp + ((fpRepresentation >> 6) & 0xF) - 13;

        int wanmant = ((anmant * (fpRepresentation & 63)) + 0x30) >> 4;
        int retval = (wanexp >= 0) ? ((wanmant << wanexp) & 0x7FFF) : (wanmant >> -wanexp);

        return ((integer ^ fpRepresentation) < 0) ? -retval : retval;
    }

    /// <summary>
    /// Quantizes a difference signal using an adaptive quantization table.
    /// </summary>
    /// <param name="d">The signed difference signal.</param>
    /// <param name="y">The adaptive quantizer scale factor.</param>
    /// <param name="table">The ordered quantization threshold table.</param>
    /// <param name="size">The number of thresholds to examine.</param>
    /// <returns>The quantized code, including the sign-dependent code mapping.</returns>
    public static int Quantize(int d, int y, ReadOnlySpan<int> table, int size)
    {
        int dqm = Math.Abs(d);
        int exp = QuantizeWithinRange(dqm >> 1, Power2Table, 15);
        int mant = ((dqm << 7) >> exp) & 0x7F;
        int dl = (exp << 7) + mant;

        int dln = dl - (y >> 2);

        int i = QuantizeWithinRange(dln, table, size);
        return d < 0 ? (size << 1) + 1 - i : i == 0 ? (size << 1) + 1 : i;
    }

    /// <summary>
    /// Reconstructs a quantized difference signal using the adaptive scale factor.
    /// </summary>
    /// <param name="sign">A value indicating whether the reconstructed result is negative.</param>
    /// <param name="dqln">The quantized logarithmic difference representation.</param>
    /// <param name="y">The adaptive quantizer scale factor.</param>
    /// <returns>The reconstructed signed difference signal.</returns>
    public static int Reconstruct(bool sign, int dqln, int y)
    {
        int dql = dqln + (y >> 2);

        if (dql < 0)
        {
            return sign ? -0x8000 : 0;
        }
        else
        {
            int dex = (dql >> 7) & 15;
            int dqt = 128 + (dql & 127);
            int dq = (short)((dqt << 7) >> (14 - dex));

            return sign ? (dq - 0x8000) : dq;
        }
    }

    /// <summary>
    /// Adjusts an A-law encoded sample to preserve the specified quantized result
    /// during tandem transcoding.
    /// </summary>
    /// <param name="sr">The reconstructed signal sample.</param>
    /// <param name="se">The estimated signal sample.</param>
    /// <param name="y">The adaptive quantizer scale factor.</param>
    /// <param name="i">The original quantized code.</param>
    /// <param name="sign">The quantizer sign-code offset.</param>
    /// <param name="qtab">The quantization threshold table.</param>
    /// <returns>The adjusted A-law encoded sample.</returns>
    public static byte TandemAdjustAlaw(int sr, int se, int y, int i, int sign, ReadOnlySpan<int> qtab)
    {
        if (sr <= -32768)
        {
            sr = -1;
        }

        int sp = G711Codec.LinearToA((short)((sr >> 1) << 3));
        int dx = (G711Codec.AToLinear((byte)sp) >> 2) - se;
        int id = Quantize(dx, y, qtab, sign - 1);

        if (id == i)
        {
            return (byte)sp;
        }
        else
        {
            int im = i ^ sign;
            int imx = id ^ sign;

            int sd = imx > im
                ? (sp & 0x80) != 0
                    ? (sp == 0xD5) ? 0x55
                    : ((sp ^ 0x55) - 1) ^ 0x55 : (sp == 0x2A) ? 0x2A : ((sp ^ 0x55) + 1) ^ 0x55
                : (sp & 0x80) != 0
                    ? (sp == 0xAA) ? 0xAA : ((sp ^ 0x55) + 1) ^ 0x55
                    : (sp == 0x55) ? 0xD5 : ((sp ^ 0x55) - 1) ^ 0x55;

            return (byte)sd;
        }
    }

    /// <summary>
    /// Adjusts a Mu-law encoded sample to preserve the specified quantized result
    /// during tandem transcoding.
    /// </summary>
    /// <param name="sr">The reconstructed signal sample.</param>
    /// <param name="se">The estimated signal sample.</param>
    /// <param name="y">The adaptive quantizer scale factor.</param>
    /// <param name="i">The original quantized code.</param>
    /// <param name="sign">The quantizer sign-code offset.</param>
    /// <param name="qtab">The quantization threshold table.</param>
    /// <returns>The adjusted Mu-law encoded sample.</returns>
    public static byte TandemAdjustMulaw(int sr, int se, int y, int i, int sign, ReadOnlySpan<int> qtab)
    {
        if (sr <= -32768)
        {
            sr = 0;
        }

        int sp = G711Codec.LinearToMu((short)(sr << 2));
        int dx = (G711Codec.MuToLinear((byte)sp) >> 2) - se;
        int id = Quantize(dx, y, qtab, sign - 1);

        if (id == i)
        {
            return (byte)sp;
        }
        else
        {
            int im = i ^ sign;
            int imx = id ^ sign;

            int sd = imx > im
                ? (sp & 0x80) != 0
                    ? (sp == 0xFF) ? 0x7E : sp + 1
                    : (sp == 0) ? 0 : sp - 1
                : (sp & 0x80) != 0
                    ? (sp == 0x80) ? 0x80 : sp - 1
                    : (sp == 0x7F) ? 0xFE : sp + 1;

            return (byte)sd;
        }
    }

    /// <summary>
    /// Converts an encoded or linear sample into a linear PCM sample.
    /// </summary>
    /// <param name="input">The input sample, represented as a 16-bit integer.</param>
    /// <param name="sampleType">The encoding used by the input sample.</param>
    /// <returns>The corresponding linear PCM sample.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <paramref name="sampleType"/> specifies an unsupported sample encoding.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short ConvertSample(short input, SampleKind sampleType) => sampleType switch
    {
        SampleKind.Pcm => input,
        SampleKind.G711A => G711Codec.AToLinear((byte)input),
        SampleKind.G711U => G711Codec.MuToLinear((byte)input),
        _ => throw new InvalidOperationException("Invalid input sample")
    };
}
