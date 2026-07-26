using System.Runtime.CompilerServices;

namespace G726Library;

internal static class G726Shared
{
    public static ReadOnlySpan<int> Power2Table => [
            1, 2, 4, 8, 0x10, 0x20, 0x40, 0x80,
            0x100, 0x200, 0x400, 0x800, 0x1000, 0x2000, 0x4000];

    public static int QuantizeWithinRange(int value, ReadOnlySpan<int> table, int size)
    {
        int baseVal = 0;
        for (int i = 0; i < size; i++)
        {
            if (value < table[baseVal++])
                return i;
        }
        return size;
    }

    public static int FloatingMultiply(int integer, int fpRepresentation)
    {
        int anmag = (integer > 0) ? integer : ((-integer) & 0x1FFF);
        int anexp = QuantizeWithinRange(anmag, Power2Table, 15) - 6;
        int anmant = (anmag == 0) ? 32 : (anexp >= 0) ? anmag >> anexp : anmag << -anexp;
        int wanexp = anexp + ((fpRepresentation >> 6) & 0xF) - 13;

        int wanmant = (anmant * (fpRepresentation & 077) + 0x30) >> 4;
        int retval = (wanexp >= 0) ? ((wanmant << wanexp) & 0x7FFF) : (wanmant >> -wanexp);

        return ((integer ^ fpRepresentation) < 0) ? -retval : retval;
    }

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

    public static int Reconstruct(bool sign, int dqln, int y)
    {
        int dql = dqln + (y >> 2);

        if (dql < 0)
        {
            return (sign ? -0x8000 : 0);
        }
        else
        {
            int dex = (dql >> 7) & 15;
            int dqt = 128 + (dql & 127);
            int dq = (short)((dqt << 7) >> (14 - dex));

            return sign ? (dq - 0x8000) : dq;
        }
    }

    public static byte TandemAdjustAlaw(int sr, int se, int y, int i, int sign, ReadOnlySpan<int> qtab)
    {
        if (sr <= -32768)
            sr = -1;

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

    public static byte TandemAdjustMulaw(int sr, int se, int y, int i, int sign, ReadOnlySpan<int> qtab)
    {
        if (sr <= -32768)
            sr = 0;

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short ConvertSample(short input, SampleKind sampleType) => sampleType switch
    {
        SampleKind.Pcm => input,
        SampleKind.G711A => G711Codec.AToLinear((byte)input),
        SampleKind.G711U => G711Codec.MuToLinear((byte)input),
        _ => throw new InvalidOperationException("Invalid input sample")
    };
}
