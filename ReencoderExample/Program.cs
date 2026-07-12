using G726Library;
using System.Diagnostics;

if (args.Length < 2)
{
    Console.WriteLine("Usage: G726Test input.wav output.wav");
    return;
}

string inputPath = args[0];
string outputPath = args[1];

byte[] wavBytes = File.ReadAllBytes(inputPath);

int headerSize = 44;
byte[] pcmData = new byte[wavBytes.Length - headerSize];
Array.Copy(wavBytes, headerSize, pcmData, 0, pcmData.Length);

short[] samples = new short[pcmData.Length / 2];
for (int i = 0; i < samples.Length; i++)
{
    samples[i] = BitConverter.ToInt16(pcmData, i * 2);
}

G726Codec codec = G726Codec.Create40Kbps();

Stopwatch sw = Stopwatch.StartNew();

int[] encoded = codec.EncodeFromPcm(samples);

short[] decodedSamples = codec.DecodeToPcm(encoded);

sw.Stop();
Console.WriteLine("Round-trip codec test took " + sw.ElapsedMilliseconds + "ms");

byte[] decodedBytes = new byte[decodedSamples.Length * 2];
for (int i = 0; i < decodedSamples.Length; i++)
{
    byte[] b = BitConverter.GetBytes(decodedSamples[i]);
    decodedBytes[i * 2] = b[0];
    decodedBytes[i * 2 + 1] = b[1];
}

using (var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
{
    fs.Write(wavBytes, 0, headerSize);
    fs.Write(decodedBytes, 0, decodedBytes.Length);
}

Console.WriteLine("Round-trip codec test complete.");
