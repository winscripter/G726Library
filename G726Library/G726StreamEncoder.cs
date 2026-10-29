namespace G726Library;

/// <summary>
/// Decodes G.726 streams.
/// </summary>
public class G726StreamEncoder : IDisposable
{
    private readonly G726Codec g726Codec;
    private readonly IBitWriter bitWriter;
    private readonly PackingType packingType;
    private readonly int bitRate;
    private readonly int bitsPerSample;
    private bool disposed;

    private G726StreamEncoder(G726Codec g726Codec, IBitWriter bitWriter, int bitRate, PackingType packingType)
    {
        Guardian.ThrowIfNull(g726Codec, nameof(g726Codec));
        Guardian.ThrowIfNull(bitWriter, nameof(bitWriter));

        this.g726Codec = g726Codec;
        this.bitWriter = bitWriter;
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
    /// Initializes a new instance of the <see cref="G726StreamEncoder"/> class.
    /// </summary>
    /// <param name="g726Codec">Custom G.726 codec.</param>
    /// <param name="stream">The stream to decode from.</param>
    /// <param name="packingType">The packing type. By default, it is Rtp.</param>
    public G726StreamEncoder(G726Codec g726Codec, Stream stream, PackingType packingType = PackingType.Rtp)
        : this(
              g726Codec,
              CreateWriterFromPackingType(packingType, stream),
              G726Utilities.DetectBitRateInKbps(g726Codec),
              packingType)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="G726StreamEncoder"/> class.
    /// </summary>
    /// <param name="kbitPerSecond">The Kbit/s mode of the codec.</param>
    /// <param name="stream">The stream to decode from.</param>
    /// <param name="packingType">The packing type. By default, it is Rtp.</param>
    public G726StreamEncoder(int kbitPerSecond, Stream stream, PackingType packingType = PackingType.Rtp)
        : this(
              G726Utilities.CreateCodecFromKbps(kbitPerSecond),
              CreateWriterFromPackingType(packingType, stream),
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
    /// Encodes the next samples into the output stream.
    /// </summary>
    /// <param name="buffer">Contains the samples to be encoded.</param>
    /// <param name="sampleKind">Represents the kind of each sample in <paramref name="buffer"/>.</param>
    public void Encode(Span<short> buffer, SampleKind sampleKind = SampleKind.Pcm)
    {
        for (int i = 0; i < buffer.Length; i++)
        {
            int g726 = this.Codec.g726Codec.EncodeSample(buffer[i], sampleKind);
            this.bitWriter.WriteBits(g726, this.bitsPerSample);
        }
    }

    private static IBitWriter CreateWriterFromPackingType(PackingType packingType, Stream stream)
    {
        Guardian.ThrowIfNull(stream, nameof(stream));

        return packingType switch
        {
            PackingType.Rtp => new BitWriterRtp(stream),
            PackingType.Aal2 => new BitWriterAal2(stream),
            _ => throw new ArgumentException($"The packing type must be set to Rtp or Aal2")
        };
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Guardian.CheckDisposed(ref this.disposed, nameof(G726StreamEncoder));
        this.bitWriter.Flush();
        GC.SuppressFinalize(this);
    }
}
