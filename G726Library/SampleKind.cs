// Copyright (c) 2023-2026, winscripter
// Licensed under the MIT License.
// See LICENSE.txt for license details.

namespace G726Library;

/// <summary>
/// Represents the kind of an input or output sample used by the encoder and decoder, respectively.
/// </summary>
public enum SampleKind : byte
{
    /// <summary>
    /// Linear/PCM samples are used for input or output.
    /// </summary>
    Pcm,

    /// <summary>
    /// G.711 (Mu-law) is used for input or output.
    /// </summary>
    G711U,

    /// <summary>
    /// G.711 (A-law) is used for input or output.
    /// </summary>
    G711A
}
