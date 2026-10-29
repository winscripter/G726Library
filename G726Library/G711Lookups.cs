// Copyright (c) 2023-2026, winscripter
// Licensed under the MIT License.
// See LICENSE.txt for license details.

using System.Runtime.CompilerServices;

namespace G726Library;

/// <summary>
/// Contains lookup tables for G.711 decoding and encoding. A sample can be directly indexed into
/// the lookup, allowing for high-performance G.711 conversion.
/// </summary>
internal static class G711Lookups
{
    /// <summary>
    /// Maps A-law samples to linear samples.
    /// </summary>
    public static readonly short[] A2Linear = new short[256];

    /// <summary>
    /// Maps Mu-law samples to linear samples.
    /// </summary>
    public static readonly short[] Mu2Linear = new short[256];

    /// <summary>
    /// Maps Linear samples to A-law samples.
    /// </summary>
    public static readonly byte[] Linear2A = new byte[65536];

    /// <summary>
    /// Maps Linear samples to Mu-law samples.
    /// </summary>
    public static readonly byte[] Linear2Mu = new byte[65536];

    /// <summary>
    /// Populates lookup tables with conversion data.
    /// </summary>
    static G711Lookups()
    {
        // Decoder tables
        for (int i = 0; i < 256; i++)
        {
            A2Linear[i] = ALawToLinear((byte)i);
            Mu2Linear[i] = MuLawToLinear((byte)i);
        }

        // Encoder tables
        for (int i = short.MinValue; i <= short.MaxValue; i++)
        {
            ushort index = (ushort)(i - short.MinValue);
            Linear2A[index] = LinearToALawSample((short)i);
            Linear2Mu[index] = LinearToMuLawSample((short)i);
        }
    }

    /// <summary>
    /// Converts a G.711 A-law sample to a linear sample.
    /// </summary>
    /// <param name="aLaw">Input A-law sample.</param>
    /// <returns>The decoded linear sample.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static short ALawToLinear(byte aLaw)
    {
        aLaw ^= 0x55;

        int sign = aLaw & 0x80;
        int exponent = (aLaw & 0x70) >> 4;
        int data = aLaw & 0x0F;

        data <<= 4;
        data += 8;

        if (exponent != 0)
        {
            data += 0x100;
        }

        if (exponent > 1)
        {
            data <<= exponent - 1;
        }

        return (short)(sign == 0 ? data : -data);
    }

    /// <summary>
    /// Converts a G.711 Mu-law sample to a linear sample.
    /// </summary>
    /// <param name="muLaw">Input Mu-law sample.</param>
    /// <returns>The decoded linear sample.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static short MuLawToLinear(byte muLaw)
    {
        muLaw = (byte)~muLaw;

        int sign = muLaw & 0x80;
        int exponent = (muLaw & 0x70) >> 4;
        int data = muLaw & 0x0F;
        data = (data << 3) + 0x84;
        data <<= exponent;

        return (short)(sign == 0 ? data : -data);
    }

    /// <summary>
    /// Converts a Linear sample to an A-law sample.
    /// </summary>
    /// <param name="pcm">Input linear sample.</param>
    /// <returns>The encoded A-law sample.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte LinearToALawSample(short pcm)
    {
        int compressed;
        int sign = (pcm & 0x8000) >> 8;

        if (sign != 0)
        {
            pcm = (short)-pcm;
        }

        if (pcm > 0x7FFF)
        {
            pcm = 0x7FFF;
        }

        if (pcm >= 256)
        {
            int exponent = (int)Math.Log(pcm / 256.0, 2.0);
            int mantissa = (pcm >> (exponent + 3)) & 0x0F;
            compressed = ((exponent << 4) | mantissa);
        }
        else
        {
            compressed = pcm >> 4;
        }

        return (byte)((compressed ^ 0x55) | sign);
    }

    /// <summary>
    /// Converts a Linear sample to an Mu-law sample.
    /// </summary>
    /// <param name="pcm">Input linear sample.</param>
    /// <returns>The encoded Mu-law sample.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte LinearToMuLawSample(short pcm)
    {
        int sign = (pcm >> 8) & 0x80;

        if (sign != 0)
        {
            pcm = (short)-pcm;
        }

        if (pcm > 0x7FFF)
        {
            pcm = 0x7FFF;
        }

        pcm += 0x84;
        int exponent = 7;

        for (int expMask = 0x4000; (pcm & expMask) == 0 && exponent > 0; expMask >>= 1)
        {
            exponent--;
        }

        int mantissa = (pcm >> (exponent + 3)) & 0x0F;
        return (byte)(~(sign | (exponent << 4) | mantissa));
    }
}
