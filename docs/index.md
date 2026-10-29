# G726Library documentation
G726Library is a G.726, G.723 and G.721 encoder/decoder written in fully managed C#. Here's how to use it.

# Decoding from a Stream
Usually you'd receive G.726 data from a network or file stream. In such a case, each sample is 2, 3, 4, or 5 bit.
To decode data from a stream, use the `G726StreamDecoder` class.

```cs
using G726Library;

using FileStream g726 = File.OpenRead("speech.g726");
using G726StreamDecoder decoder = new(32, g726);
Span<short> samples = stackalloc short[160];

bool success = true;
while (success)
{
	success = decoder.TryDecode(samples, out int samplesDecoded, SampleKind.Pcm);
	Span<short> decoded = samples[..samplesDecoded];
	ProcessPCMSamples(decoded);
}
```

Here:
- First we create a variable g726 which lets us read a file named speech.g726. That file is assumed to have G.726 audio data.
- Then we instantiate the stream decoder. The first parameter, `32`, configures the G.726 codec to be 32Kbit/s, and each sample is 4-bit. (Correct values are 16, 24, 32, 40). The second parameter is the stream where we will be decoding from.
- 'samples' is our sample buffer. We read 160 samples at a time.
- There is then a loop that lets us decode samples until the end of the stream. When there's an end of stream, `decoder.TryDecode` returns false. `decoder.TryDecode` decodes the next G.726 samples and puts them into the `samples` parameter, then `samplesDecoded` specifies how many samples were correctly decoded, and finally the sample kind - which is the expected sample type in `samples`. Here it's PCM, but you can also configure it to be G.711a or G.711mu.

You can also pass a third parameter to the G726StreamDecoder constructor: `PackingType`. This can be `PackingType.Aal2` or `PackingType.Rtp` (default is `PackingType.Rtp`). It specifies the bit-packing convention, which is
important if you're using AAL2 or RTP.

# Encoding to a Stream
Very similar to the decoder API.

```cs
using G726Library;

using FileStream g726 = File.Open("speech.g726", FileMode.Create);
using G726StreamEncoder encoder = new(32, g726);
Span<short> samples = stackalloc short[8000];
GetSamples(samples); // ...

encoder.Encode(samples, SampleKind.Pcm);
```

# Direct sample conversion
You can also convert samples directly, without encoding or decoding to/from a stream.

### Encode G.726 samples from PCM (24Kbps)

```cs
using G726Library;

static void PcmToG726(short[] pcm, int[] g726)
{
	G726Codec codec = G726Codec.Create24Kbps();

	for (int i = 0; i < pcm.Length; i++)
	{
		g726[i] = codec.EncodeFromPcm(pcm[i]);
	}
}
```

### Decode PCM samples from G.726 (24Kbps)

```cs
using G726Library;

static void G726toPcm(int[] g726, short[] pcm)
{
	G726Codec codec = G726Codec.Create24Kbps();

	for (int i = 0; i < g726.Length; i++)
	{
		pcm[i] = codec.DecodeToPcm(g726[i]);
	}
}
```

### Decode G.711A samples from G.726 (24Kbps)

```cs
using G726Library;

static void G726toG711A(int[] g726, byte[] g711a)
{
	G726Codec codec = G726Codec.Create24Kbps();

	for (int i = 0; i < g726.Length; i++)
	{
		g711a[i] = (byte)codec.DecodeToG711A(g726[i]);
	}
}
```

### Encode G.726 samples from G.711Mu (40Kbps)

```cs
using G726Library;

static void G711MuToG726(short[] g711, int[] g726)
{
	G726Codec codec = G726Codec.Create40Kbps();

	for (int i = 0; i < g711.Length; i++)
	{
		g726[i] = codec.EncodeFromG711Mu(g711[i]);
	}
}
```
