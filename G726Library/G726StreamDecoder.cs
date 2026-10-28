namespace G726Library;

/// <summary>
/// Decodes G.726 streams.
/// </summary>
public class G726StreamDecoder : IDisposable
{
    private readonly G726Codec g726Codec;
    private readonly IBitReader bitReader;
    private readonly PackingType packingType;
    private readonly int bitRate;
    private readonly int bitsPerSample;
    private bool disposed;
    
    private G726StreamDecoder(G726Codec g726Codec, IBitReader bitReader, int bitRate, PackingType packingType)
    {
        Guardian.ThrowIfNull(g726Codec, nameof(g726Codec));
        Guardian.ThrowIfNull(bitReader, nameof(bitReader));

        this.g726Codec = g726Codec;
        this.bitReader = bitReader;
        this.bitRate = bitRate;
        this.packingType = packingType;

        this.bitsPerSample = this.bitRate switch
        {
            40 => 5,
            32 => 4,
            24 => 3,
            16 => 2,
            _ => throw new ArgumentException("Incorrect bit rate.")
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="G726StreamDecoder"/> class.
    /// </summary>
    /// <param name="g726Codec">Custom G.726 codec.</param>
    /// <param name="stream">The stream to decode from.</param>
    /// <param name="packingType">The packing type. By default, it is Rtp.</param>
    public G726StreamDecoder(G726Codec g726Codec, Stream stream, PackingType packingType = PackingType.Rtp)
        : this(
              g726Codec,
              CreateReaderFromPackingType(packingType, stream),
              G726Utilities.DetectBitRateInKbps(g726Codec),
              packingType)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="G726StreamDecoder"/> class.
    /// </summary>
    /// <param name="kbitPerSecond">The Kbit/s mode of the codec.</param>
    /// <param name="stream">The stream to decode from.</param>
    /// <param name="packingType">The packing type. By default, it is Rtp.</param>
    public G726StreamDecoder(int kbitPerSecond, Stream stream, PackingType packingType = PackingType.Rtp)
        : this(
              G726Utilities.CreateCodecFromKbps(kbitPerSecond),
              CreateReaderFromPackingType(packingType, stream),
              kbitPerSecond,
              packingType)
    {
    }

    /// <summary>
    /// Gets the bit-rate in Kbit/s.
    /// </summary>
    public int BitRateInKbps => this.bitRate;

    /// <summary>
    /// Gets the underlying G.726 codec.
    /// </summary>
    public G726Codec Codec => this.g726Codec;

    /// <summary>
    /// Gets the underlying packing type.
    /// </summary>
    public PackingType PackingType => this.packingType;

    /// <summary>
    /// Gets the number of bits for one sample.
    /// </summary>
    public int BitsPerSample => this.bitsPerSample;

    /// <summary>
    /// Decodes the next samples into <paramref name="buffer"/>.
    /// </summary>
    /// <param name="buffer">Contains the next samples to be decoded.</param>
    /// <param name="sampleKind">Represents the kind of each sample in <paramref name="buffer"/>.</param>
    /// <exception cref="EndOfStreamException">Thrown on an end-of-stream.</exception>
    public void Decode(Span<short> buffer, SampleKind sampleKind = SampleKind.Pcm)
    {
        for (int i = 0; i < buffer.Length; i++)
        {
            byte sampleG726 = (byte)this.bitReader.ReadBits(this.bitsPerSample);
            buffer[i] = (short)this.Codec.g726Codec.DecodeSample(sampleG726, sampleKind);
        }
    }

    private static IBitReader CreateReaderFromPackingType(PackingType packingType, Stream stream)
    {
        Guardian.ThrowIfNull(stream, nameof(stream));

        return packingType switch
        {
            PackingType.Rtp => new BitReaderRtp(stream),
            PackingType.Aal2 => new BitReaderAal2(stream),
            _ => throw new ArgumentException($"The packing type must be set to Rtp or Aal2")
        };
    }

    /// <inheritdoc />
    public void Dispose()
    {
        // Nothing to dispose, but this is for consistency with the G726 stream encoder.
        Guardian.CheckDisposed(ref this.disposed, nameof(G726StreamDecoder));
        GC.SuppressFinalize(this);
    }
}
