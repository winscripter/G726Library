// Copyright (c) 2023-2026, winscripter
// Licensed under the MIT License.
// See LICENSE.txt for license details.

namespace G726Library;

internal readonly struct Codec40() : IG726Codec
{
    public static ReadOnlySpan<short> DqlnTable => [
        -2048, -66, 28, 104, 169, 224, 274, 318,
        358, 395, 429, 459, 488, 514, 539, 566,
        566, 539, 514, 488, 459, 429, 395, 358,
        318, 274, 224, 169, 104, 28, -66, -2048];

    public static ReadOnlySpan<short> WiTable => [
        448, 448, 768, 1248, 1280, 1312, 1856, 3200,
        4512, 5728, 7008, 8960, 11456, 14080, 16928, 22272,
        22272, 16928, 14080, 11456, 8960, 7008, 5728, 4512,
        3200, 1856, 1312, 1280, 1248, 768, 448, 448];

    public static ReadOnlySpan<short> FiTable => [
        0, 0, 0, 0, 0, 0x200, 0x200, 0x200,
        0x200, 0x200, 0x400, 0x600, 0x800, 0xA00, 0xC00, 0xC00,
        0xC00, 0xC00, 0xA00, 0x800, 0x600, 0x400, 0x200, 0x200,
        0x200, 0x200, 0x200, 0, 0, 0, 0, 0];

    public static ReadOnlySpan<int> QTable => [
        -122, -16, 68, 139, 198, 250, 298, 339,
        378, 413, 445, 475, 502, 528, 553];

    private readonly G726Context context = new();

    public int EncodeSample(short inputSample, SampleKind sampleType)
    {
        int sl = G726Shared.ConvertSample(inputSample, sampleType) >> 2;

        int sezi = this.context.GetZeroPredictor();
        int sez = sezi >> 1;
        int sei = sezi + this.context.GetTwoPolePredictor();
        int se = sei >> 1;
        int d = sl - se;
        int y = this.context.GetStepSize();
        int i = G726Shared.Quantize(d, y, QTable, 15);
        int dq = G726Shared.Reconstruct((i & 0x10) != 0, DqlnTable[i], y);
        int sr = (dq < 0) ? se - (dq & 0x7FFF) : se + dq;
        int dqsez = sr + sez - se;

        this.context.Update(5, y, WiTable[i], FiTable[i], dq, sr, dqsez);

        return i;
    }

    public int DecodeSample(int i, SampleKind sampleType)
    {
        i &= 0x1f;
        int sezi = this.context.GetZeroPredictor();
        int sez = sezi >> 1;
        int sei = sezi + this.context.GetTwoPolePredictor();
        int se = sei >> 1;
        int y = this.context.GetStepSize();
        int dq = G726Shared.Reconstruct((i & 0x10) != 0, DqlnTable[i], y);
        int sr = (dq < 0) ? (se - (dq & 0x7FFF)) : (se + dq);
        int dqsez = sr - se + sez;

        this.context.Update(5, y, WiTable[i], FiTable[i], dq, sr, dqsez);

        return sampleType switch
        {
            SampleKind.G711A => G726Shared.TandemAdjustAlaw(sr, se, y, i, 16, QTable),
            SampleKind.G711U => G726Shared.TandemAdjustMulaw(sr, se, y, i, 16, QTable),
            _ => sr << 2,
        };
    }
}
