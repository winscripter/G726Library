namespace G726Library;

internal sealed class Codec32 : IG726Codec
{
    public static ReadOnlySpan<short> DqlnTable => [-2048, 4, 135, 213, 273, 323, 373, 425,
                425, 373, 323, 273, 213, 135, 4, -2048];
    public static ReadOnlySpan<short> WiTable => [-12, 18, 41, 64, 112, 198, 355, 1122,
                1122, 355, 198, 112, 64, 41, 18, -12];
    public static ReadOnlySpan<short> FiTable => [0, 0, 0, 0x200, 0x200, 0x200, 0x600, 0xE00,
                0xE00, 0x600, 0x200, 0x200, 0x200, 0, 0, 0];
    public static ReadOnlySpan<int> QTable => [-124, 80, 178, 246, 300, 349, 400];

    private readonly G726Context context = new();

    public int EncodeSample(short inputSample, SampleKind sampleType)
    {
        int sl = G726Shared.ConvertSample(inputSample, sampleType) >> 2;

        int sezi = context.GetZeroPredictor();
        int sez = sezi >> 1;
        int se = (sezi + context.GetTwoPolePredictor()) >> 1;
        int d = sl - se;
        int y = context.GetStepSize();
        int i = G726Shared.Quantize(d, y, QTable, 7);
        int dq = G726Shared.Reconstruct((i & 8) != 0, DqlnTable[i], y);
        int sr = (dq < 0) ? se - (dq & 0x3FFF) : se + dq;
        int dqsez = sr + sez - se;

        context.Update(4, y, WiTable[i] << 5, FiTable[i], dq, sr, dqsez);

        return i;
    }

    public int DecodeSample(int i, SampleKind sampleType)
    {
        i &= 0x0f;
        int sezi = context.GetZeroPredictor();
        int sez = sezi >> 1;
        int sei = sezi + context.GetTwoPolePredictor();
        int se = sei >> 1;
        int y = context.GetStepSize();
        int dq = G726Shared.Reconstruct((i & 0x08) != 0, DqlnTable[i], y);
        int sr = (dq < 0) ? (se - (dq & 0x3FFF)) : se + dq;
        int dqsez = sr - se + sez;
        context.Update(4, y, WiTable[i] << 5, FiTable[i], dq, sr, dqsez);

        switch (sampleType)
        {
            case SampleKind.G711A:
                return G726Shared.TandemAdjustAlaw(sr, se, y, i, 8, QTable);
            case SampleKind.G711U:
                return G726Shared.TandemAdjustMulaw(sr, se, y, i, 8, QTable);
            default:
                int lino = sr << 2;
                lino = lino > 32767 ? 32767 : lino;
                lino = lino < -32768 ? -32768 : lino;
                return lino;
        }
    }
}
