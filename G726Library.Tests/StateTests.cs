namespace G726Library.Tests;

public class StateTests
{
    [Fact]
    public void TestStreamDecoder()
    {
        G726StreamDecoder sd = new(32, new MemoryStream(), PackingType.Aal2);

        Assert.Equal(32, sd.BitRateInKbps);
        Assert.Equal(4, sd.BitsPerSample);
        Assert.Equal(PackingType.Aal2, sd.PackingType);
    }
}
