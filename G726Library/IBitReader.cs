namespace G726Library;

/// <summary>
/// Abstracts a bit-reader (for RTP and AAL2).
/// </summary>
internal interface IBitReader
{
    /// <summary>
    /// Reads the next <paramref name="n"/> bits.
    /// </summary>
    /// <param name="n">The number of bits to read.</param>
    /// <returns>The next <paramref name="n"/> bits.</returns>
    int ReadBits(int n);
}
