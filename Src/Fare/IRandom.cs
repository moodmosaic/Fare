namespace Fare
{
    using System;
    using System.Diagnostics;
    using System.Security.Cryptography;

    public class HardRandom
        : IRandom
        , IDisposable
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private RNGCryptoServiceProvider _CryptoServiceProvider = new RNGCryptoServiceProvider();

        public void Dispose()
        {
            _CryptoServiceProvider?.Dispose();
            _CryptoServiceProvider = null;
        }

        public void NextBytes(byte[] bytes) =>
            _CryptoServiceProvider.GetBytes(bytes);

        public byte[] NextBytes(long byteCount)
        {
            var bytes = new byte[byteCount];
            NextBytes(bytes);
            return bytes;
        }
    }

    public interface IRandom
    {
        void NextBytes(byte[] bytes);

        byte[] NextBytes(long byteCount);
    }

    public static class RandomExtensions
    {
        public static byte NextByte(this IRandom random) => random.NextBytes(1)[0];

        public static double NextDouble(this IRandom random) => BitConverter.ToDouble(random.NextBytes(8), 0);

        public static short NextInt16(this IRandom random) => BitConverter.ToInt16(random.NextBytes(2), 0);

        public static int NextInt32(this IRandom random) => BitConverter.ToInt32(random.NextBytes(4), 0);

        public static int NextInt32(this IRandom random, int min, int max) => (int)(min + (max - min) * random.NextPercentage());

        public static long NextInt64(this IRandom random) => BitConverter.ToInt64(random.NextBytes(8), 0);

        public static long NextInt64(this IRandom random, long min, long max) => (long)(min + (max - min) * random.NextPercentage());

        public static double NextPercentage(this IRandom random) => random.NextUInt64() / (double)ulong.MaxValue;

        public static sbyte NextSByte(this IRandom random) => (sbyte)random.NextByte();

        public static float NextSingle(this IRandom random) => BitConverter.ToSingle(random.NextBytes(4), 0);

        public static ushort NextUInt16(this IRandom random) => BitConverter.ToUInt16(random.NextBytes(2), 0);

        public static uint NextUInt32(this IRandom random) => BitConverter.ToUInt32(random.NextBytes(4), 0);

        public static ulong NextUInt64(this IRandom random) => BitConverter.ToUInt64(random.NextBytes(8), 0);
    }
}