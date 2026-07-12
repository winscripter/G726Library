namespace G726Library
{
    internal static class G711Codec
    {
        public static byte LinearToMu(short linear) => G711Lookups.Linear2Mu[(ushort)(linear - short.MinValue)];

        public static byte LinearToA(short linear) => G711Lookups.Linear2A[(ushort)(linear - short.MinValue)];

        public static short MuToLinear(byte mu) => G711Lookups.Mu2Linear[mu];

        public static short AToLinear(byte a) => G711Lookups.A2Linear[a];
    }
}
