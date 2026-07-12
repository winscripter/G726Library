namespace G726Library;

internal sealed class Codec24 : IG726Codec
{
    public static ReadOnlySpan<short> DqlnTable => [-2048, 135, 273, 373, 373, 273, 135, -2048];
    public static ReadOnlySpan<short> WiTable => [-128, 960, 4384, 18624, 18624, 4384, 960, -128];
    public static ReadOnlySpan<short> FiTable => [0, 0x200, 0x400, 0xE00, 0xE00, 0x400, 0x200, 0];
    public static ReadOnlySpan<int> QTable => [8, 218, 331];

    private readonly G726Context context = new();

    public int EncodeSample(short inputSample, SampleKind sampleType)
    {
        int sl = G726Shared.ConvertSample(inputSample, sampleType) >> 2;

        int sezi = context.GetZeroPredictor();
        int sez = sezi >> 1;
        int sei = sezi + context.GetTwoPolePredictor();
        int se = sei >> 1;
        int d = sl - se;
        int y = context.GetStepSize();
        int i = G726Shared.Quantize(d, y, QTable, 3);
        int dq = G726Shared.Reconstruct((i & 4) != 0, DqlnTable[i], y);
        int sr = (dq < 0) ? se - (dq & 0x3FFF) : se + dq;
        int dqsez = sr + sez - se;
        context.Update(3, y, WiTable[i], FiTable[i], dq, sr, dqsez);

        return i;
    }

    public int DecodeSample(int i, SampleKind sampleType)
    {
        i &= 0x07;
        int sezi = context.GetZeroPredictor();
        int sez = sezi >> 1;
        int sei = sezi + context.GetTwoPolePredictor();
        int se = sei >> 1;
        int y = context.GetStepSize();
        int dq = G726Shared.Reconstruct((i & 0x04) != 0, DqlnTable[i], y);
        int sr = (dq < 0) ? (se - (dq & 0x3FFF)) : (se + dq);
        int dqsez = sr - se + sez;
        context.Update(3, y, WiTable[i], FiTable[i], dq, sr, dqsez);

        return sampleType switch
        {
            SampleKind.G711A => G726Shared.TandemAdjustAlaw(sr, se, y, i, 4, QTable),
            SampleKind.G711U => G726Shared.TandemAdjustMulaw(sr, se, y, i, 4, QTable),
            _ => sr << 2
        };
    }
}
