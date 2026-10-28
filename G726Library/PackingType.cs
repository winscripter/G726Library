namespace G726Library;

/// <summary>
/// Specifies the bit-packing convention used when decoding or encoding G.726 data.
/// </summary>
public enum PackingType : byte
{
    /// <summary>
    /// Uses RTP-style packing, with codewords packed least-significant bit first.
    /// </summary>
    Rtp = 0,

    /// <summary>
    /// Uses AAL2-style packing, with codewords packed most-significant bit first.
    /// </summary>
    Aal2 = 1,
}
