# Overview
G726Library is an ITU-T G.726/G.723/G.721 codec written in C#. It can take any input PCM, G.711A or G.711Mu sample and encode it into
G.726 and vice versa. It also supports 16, 24, 32 and 40 Kbps modes.

# Usage
Start by installing the G726Library NuGet package. Go to the Visual Studio NuGet Package Manager, search G726Library
and install it. If you're using the .NET CLI, use `dotnet add package G726Library`.

Here's an example of decoding a G.726 (32 Kbit/s) stream into PCM.

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

For more information, please see the [documentation](docs/index.md).

# Strengths
- High compatibility. Any framework since .NET Framework 4.7.2 and .NET Core 2.1 is supported.
- High performance. See Benchmarks below.
- Permissive license (MIT), see LICENSE.txt. Free to use and open-source.
- Simplicity. It is very easy to use.
- Memory management. Unless we're talking overloads that return arrays of decoded/encoded samples, the codec performs no allocations at all.
- Contributions are very welcome. You can ask questions, report bugs, or submit PRs.
- Super lightweight. Only weighs around ~18KB.

# Benchmarks
The following benchmarks demonstrate the codec performance of encoding 10 seconds of 40 Kbps audio into G.726, as well as decoding it back.
This maps to about 80,000 unique samples.

```
BenchmarkDotNet v0.15.2, Windows 11 (10.0.22631.2861/23H2/2023Update/SunValley3)
13th Gen Intel Core i5-13420H 2.10GHz, 1 CPU, 12 logical and 8 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]     : .NET 10.0.9 (10.0.926.27113), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.9 (10.0.926.27113), X64 RyuJIT AVX2
```

| Method                       | Mean     | Error     | StdDev    |
|----------------------------- |---------:|----------:|----------:|
| Encode10SecondsOfAudio40Kbps | 3.799 ms | 0.0129 ms | 0.0114 ms |
| Decode10SecondsOfAudio40Kbps | 3.167 ms | 0.0066 ms | 0.0058 ms |

Doing some math, in 40 Kbps mode alone, you can:
- Encode 43 minutes of audio per second
- Decode 52 minutes of audio per second

This is well beyond the performance you'll have to hit for real-time communication.

Realistically, hitting those speeds may take some time due to the .NET JIT compiler having to jit all methods,
but you can always compile against ReadyToRun (R2R) to make things several times faster.

# Compatibility
The library supports:
- .NET Framework 4.7.2 and later
- .NET Core 2.1 and later

# Licensing
The library is licensed under the MIT License, you can find details in the LICENSE.txt file.
It is completely free to use.

# Contributions
You are free to ask questions, submit bug reports, propose new features, or submit PRs to this repo and I will make sure to look into it.

# Credits
I used some code from Sun Microsystems' implementation (which is in C) to write this library.
