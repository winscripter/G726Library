// Copyright (c) 2023-2026, winscripter
// Licensed under the MIT License.
// See LICENSE.txt for license details.

namespace G726Library;

/// <summary>
/// Abstracts an internal G.726 codec that depends on the bit-rate.
/// </summary>
internal interface IG726Codec
{
    /// <summary>
    /// Encodes the input sample, whose type is specified by <paramref name="sampleType"/>, into a
    /// G.726 sample.
    /// </summary>
    /// <param name="inputSample">The input sample.</param>
    /// <param name="sampleType">The type of the input sample.</param>
    /// <returns>The encoded G.726 sample.</returns>
    int EncodeSample(short inputSample, SampleKind sampleType);

    /// <summary>
    /// Decodes the G.726 sample. The kind of the output sample is specified by <paramref name="sampleType"/>.
    /// </summary>
    /// <param name="inputSample">The G.726 sample.</param>
    /// <param name="sampleType">The type of the output sample.</param>
    /// <returns>Decoded sample, which can be PCM, G.711Mu or G.711a.</returns>
    int DecodeSample(int inputSample, SampleKind sampleType);
}
