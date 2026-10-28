namespace G726Library;

/// <summary>
/// Writes G.726 codewords using RTP bit packing, with the least-significant
/// bit of each codeword transmitted first.
/// </summary>
internal sealed class BitWriterRtp : IBitWriter
{
    private readonly Stream stream;
    private byte cache;
    private int bitPosition;

    /// <summary>
    /// Initializes a new instance of the <see cref="BitWriterRtp"/> class.
    /// </summary>
    /// <param name="stream">The destination stream for the packed codewords.</param>
    public BitWriterRtp(Stream stream)
    {
        Guardian.ThrowIfNull(stream, nameof(stream));
        this.stream = stream;
    }

    /// <summary>
    /// Writes the specified number of least-significant bits to the stream.
    /// </summary>
    /// <param name="value">The value whose bits are written.</param>
    /// <param name="n">The number of bits to write, from 0 to 32.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="n"/> is outside the range 0–32.</exception>
    public void WriteBits(int value, int n)
    {
        Guardian.ThrowIfNegative(n, nameof(n));
        Guardian.ThrowIfGreaterThan(n, 32, nameof(n));

        for (int i = 0; i < n; i++)
        {
            this.cache |= (byte)(((value >> i) & 1) << this.bitPosition);

            if (++this.bitPosition == 8)
            {
                this.stream.WriteByte(this.cache);
                this.cache = 0;
                this.bitPosition = 0;
            }
        }
    }

    /// <summary>
    /// Writes any remaining bits as a final byte, padding unused bits with zeros,
    /// and flushes the underlying stream.
    /// </summary>
    public void Flush()
    {
        if (this.bitPosition != 0)
        {
            this.stream.WriteByte(this.cache);
            this.cache = 0;
            this.bitPosition = 0;
        }

        this.stream.Flush();
    }
}
