// Copyright (c) 2023-2026, winscripter
// Licensed under the MIT License.
// See LICENSE.txt for license details.

using System.Runtime.CompilerServices;

namespace G726Library;

/// <summary>
/// G.711 codec, which is preferred if the input or output sample is G.711.
/// </summary>
internal static class G711Codec
{
    /// <summary>
    /// Converts the input linear sample into G.711 Mu-law.
    /// </summary>
    /// <param name="linear">The input linear sample.</param>
    /// <returns>The converted G.711 Mu-law sample.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte LinearToMu(short linear) => G711Lookups.Linear2Mu[(ushort)(linear - short.MinValue)];

    /// <summary>
    /// Converts the input linear sample into G.711 A-law.
    /// </summary>
    /// <param name="linear">The input linear sample.</param>
    /// <returns>The converted G.711 A-law sample.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte LinearToA(short linear) => G711Lookups.Linear2A[(ushort)(linear - short.MinValue)];

    /// <summary>
    /// Converts the input G.711 Mu-law sample into a linear sample.
    /// </summary>
    /// <param name="mu">The input G.711 Mu-law sample.</param>
    /// <returns>The converted linear sample.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short MuToLinear(byte mu) => G711Lookups.Mu2Linear[mu];

    /// <summary>
    /// Converts the input G.711 A-law sample into a linear sample.
    /// </summary>
    /// <param name="a">The input G.711 A-law sample.</param>
    /// <returns>The converted linear sample.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short AToLinear(byte a) => G711Lookups.A2Linear[a];
}
