// Copyright (c) 2023-2026, winscripter
// Licensed under the MIT License.
// See LICENSE.txt for license details.

namespace G726Library;

internal readonly struct Codec16() : IG726Codec
{
    public static ReadOnlySpan<short> DqlnTable => [116, 365, 365, 116];

    public static ReadOnlySpan<short> WiTable => [-704, 14048, 14048, -704];

    public static ReadOnlySpan<short> FiTable => [0, 0xE00, 0xE00, 0];

    public static ReadOnlySpan<int> QTable => [261];

    private readonly G726Context context = new();

    public int EncodeSample(short inputSample, SampleKind sampleType)
    {
        short sl = (short)(G726Shared.ConvertSample(inputSample, sampleType) >> 2);

        int sezi = this.context.GetZeroPredictor();
        int sez = sezi >> 1;
        int sei = sezi + this.context.GetTwoPolePredictor();
        int se = sei >> 1;
        int d = sl - se;
        int y = this.context.GetStepSize();
        int i = G726Shared.Quantize(d, y, QTable, 1);

        if (i == 3 && (d & 0x8000) == 0)
        {
            i = 0;
        }

        int dq = G726Shared.Reconstruct((i & 2) != 0, DqlnTable[i], y);
        int sr = (dq < 0) ? se - (dq & 0x3FFF) : se + dq;
        int dqsez = sr + sez - se;

        this.context.Update(2, y, WiTable[i], FiTable[i], dq, sr, dqsez);

        return i;
    }

    public int DecodeSample(int i, SampleKind sampleType)
    {
        i &= 0x03;
        int sezi = this.context.GetZeroPredictor();
        int sez = sezi >> 1;
        int sei = sezi + this.context.GetTwoPolePredictor();
        int se = sei >> 1;
        int y = this.context.GetStepSize();
        int dq = G726Shared.Reconstruct((i & 0x02) != 0, DqlnTable[i], y);
        int sr = (dq < 0) ? (se - (dq & 0x3FFF)) : se + dq;
        int dqsez = sr - se + sez;
        this.context.Update(2, y, WiTable[i], FiTable[i], dq, sr, dqsez);

        return sampleType switch
        {
            SampleKind.G711A => G726Shared.TandemAdjustAlaw(sr, se, y, i, 2, QTable),
            SampleKind.G711U => G726Shared.TandemAdjustMulaw(sr, se, y, i, 2, QTable),
            _ => sr << 2
        };
    }
}
