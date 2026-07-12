namespace G726Library;

internal static class G711Lookups
{
    public static readonly short[] A2Linear = new short[256];
    public static readonly short[] Mu2Linear = new short[256];
    public static readonly byte[] Linear2A = new byte[65536];
    public static readonly byte[] Linear2Mu = new byte[65536];

    static G711Lookups()
    {
        for (int i = 0; i < 256; i++)
        {
            A2Linear[i] = ALawToLinear((byte)i);
            Mu2Linear[i] = MuLawToLinear((byte)i);
        }

        for (int i = short.MinValue; i <= short.MaxValue; i++)
        {
            ushort index = (ushort)(i - short.MinValue);
            Linear2A[index] = LinearToALawSample((short)i);
            Linear2Mu[index] = LinearToMuLawSample((short)i);
        }
    }

    private static short ALawToLinear(byte aLaw)
    {
        aLaw ^= 0x55;

        int sign = aLaw & 0x80;
        int exponent = (aLaw & 0x70) >> 4;
        int data = aLaw & 0x0F;

        data <<= 4;
        data += 8;

        if (exponent != 0)
            data += 0x100;
        if (exponent > 1)
            data <<= (exponent - 1);

        return (short)(sign == 0 ? data : -data);
    }

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

    private static byte LinearToALawSample(short pcm)
    {
        int exponent;
        int mantissa;
        int compressed;

        int sign = (pcm & 0x8000) >> 8;
        if (sign != 0)
            pcm = (short)-pcm;
        if (pcm > 0x7FFF) pcm = 0x7FFF;

        if (pcm >= 256)
        {
            exponent = (int)Math.Log(pcm / 256.0, 2.0);
            mantissa = (pcm >> (exponent + 3)) & 0x0F;
            compressed = ((exponent << 4) | mantissa);
        }
        else
        {
            compressed = pcm >> 4;
        }

        return (byte)((compressed ^ 0x55) | sign);
    }

    private static byte LinearToMuLawSample(short pcm)
    {
        int sign = (pcm >> 8) & 0x80;
        if (sign != 0) pcm = (short)-pcm;
        if (pcm > 0x7FFF) pcm = 0x7FFF;

        pcm += 0x84;
        int exponent = 7;
        for (int expMask = 0x4000; (pcm & expMask) == 0 && exponent > 0; expMask >>= 1)
            exponent--;

        int mantissa = (pcm >> (exponent + 3)) & 0x0F;
        return (byte)(~(sign | (exponent << 4) | mantissa));
    }
}
