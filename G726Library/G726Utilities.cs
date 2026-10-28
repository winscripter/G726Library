namespace G726Library;

internal static class G726Utilities
{
    public static int DetectBitRateInKbps(G726Codec codec) => codec.g726Codec switch
    {
        Codec16 => 16,
        Codec24 => 24,
        Codec32 => 32,
        Codec40 => 40,
        _ => throw new InvalidOperationException("Custom internal codecs are not supported")
    };

    public static G726Codec CreateCodecFromKbps(int kbits) => kbits switch
    {
        16 => G726Codec.Create16Kbps(),
        24 => G726Codec.Create24Kbps(),
        32 => G726Codec.Create32Kbps(),
        40 => G726Codec.Create40Kbps(),
        _ => throw new InvalidOperationException("Invalid G.726 mode. It must be a valid number of Kbit/s. For example, specify 24 for a G.726 24Kbit/s mode. Correct values are 16, 24, 32, or 40.")
    };
}
