namespace G726Library;

/// <summary>
/// Abstracts a bit-reader (for RTP and AAL2).
/// </summary>
internal interface IBitWriter
{
    /// <summary>
    /// Writes the next <paramref name="n"/> bits.
    /// </summary>
    /// <param name="value">The code that will be written.</param>
    /// <param name="n">The number of bits to be written.</param>
    public void WriteBits(int value, int n);

    /// <summary>
    /// Flushes the remaining cache into the output stream.
    /// </summary>
    public void Flush();
}
