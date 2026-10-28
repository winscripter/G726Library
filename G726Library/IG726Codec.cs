// Copyright (c) 2023-2026, winscripter
// Licensed under the MIT License.
// See LICENSE.txt for license details.

namespace G726Library;

internal interface IG726Codec
{
    int EncodeSample(short inputSample, SampleKind sampleType);
    int DecodeSample(int inputSample, SampleKind sampleType);
}
