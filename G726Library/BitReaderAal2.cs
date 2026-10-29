namespace G726Library;

/// <summary>
/// Reads G.726 codewords using AAL2 bit packing, with the most-significant
/// bit of each codeword transmitted first.
/// </summary>
internal sealed class BitReaderAal2 : IBitReader
{
    private readonly Stream stream;
    private byte cache;
    private int bitPosition = 8;

    /// <summary>
    /// Initializes a new instance of the <see cref="BitReaderAal2"/> class.
    /// </summary>
    /// <param name="stream">The stream containing the packed codewords.</param>
    public BitReaderAal2(Stream stream)
    {
        Guardian.ThrowIfNull(stream, nameof(stream));
        this.stream = stream;
    }

    /// <summary>
    /// Reads the specified number of bits from the stream.
    /// </summary>
    /// <param name="n">The number of bits to read, from 0 to 32.</param>
    /// <returns>The decoded bits, with the first bit read in the most-significant position.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="n"/> is outside the range 0–32.</exception>
    /// <exception cref="EndOfStreamException">The stream ends before all requested bits are read.</exception>
    public int ReadBits(int n)
    {
        Guardian.ThrowIfNegative(n, nameof(n));
        Guardian.ThrowIfGreaterThan(n, 32, nameof(n));

        int result = 0;

        for (int i = 0; i < n; i++)
        {
            if (this.bitPosition == 8)
            {
                int value = this.stream.ReadByte();

                if (value < 0)
                {
                    throw new EndOfStreamException();
                }

                this.cache = (byte)value;
                this.bitPosition = 0;
            }

            result = (result << 1) | ((this.cache >> (7 - this.bitPosition)) & 1);
            this.bitPosition++;
        }

        return result;
    }
}
