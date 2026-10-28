namespace G726Library.Tests;

public class BitIOTests
{
    [Fact]
    public void TestBitIO()
    {
        using MemoryStream ms = new();
        BitWriterAal2 aal2 = new(ms);

        aal2.WriteBits(15, 5);
        aal2.WriteBits(20, 5);
        aal2.WriteBits(25, 5);
        aal2.WriteBits(5, 5);
        aal2.Flush();

        ms.Position = 0;
        BitReaderAal2 aal2reader = new(ms);

        Assert.Equal(15, aal2reader.ReadBits(5));
        Assert.Equal(20, aal2reader.ReadBits(5));
        Assert.Equal(25, aal2reader.ReadBits(5));
        Assert.Equal(5, aal2reader.ReadBits(5));
    }
}
