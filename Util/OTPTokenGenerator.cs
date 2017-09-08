using System;
using System.Linq;
using System.Security.Cryptography;

namespace HDLauncher
{
    internal static class OTPTokenGenerator
    {
        public static string GenerateToken(int oid, byte[] seed)
        {
            var now = GetNow();

            var time = now / 10;

            var accSeed = new byte[31];
            var timeSeed = new byte[11];

            for (var i = 10; i >= 0; i--)
            {
                accSeed[i] = (byte)(oid & 255);
                timeSeed[i] = (byte)(time & 255);

                oid >>= 8;
                time >>= 8;
            }

            Array.Copy(seed, 0, accSeed, 11, 20);

            var accSeed0 = new byte[64];
            Array.Copy(GetSHA1Hash(accSeed), 0, accSeed0, 0, 20);

            var accSeed1 = accSeed0.ToArray();
            var accSeed2 = accSeed0.ToArray();

            for (var i = 0; i < 64; i++)
            {
                accSeed1[i] ^= 54;
                accSeed2[i] ^= 92;
            }

            var digest = GetSHA1Hash(accSeed1.Concat(timeSeed).ToArray());
            digest = GetSHA1Hash(accSeed2.Concat(digest).ToArray());

            var digit = digest.Last() & 0xf;
            var token = 0;

            for (var i = 0; i < 4; i++)
            {
                token <<= 8;
                token |= digest[digit + i];
            }

            token &= 0xffffdb;

            var rem = (now % 30) / 10;
            if (rem == 1)
                token |= 4;
            else if (rem == 2)
                token |= 32;

            token %= 10000000;

            return token.ToString().PadLeft(7, '0');
        }

        private static byte[] GetSHA1Hash(byte[] value)
        {
            using (var sha1 = new SHA1Managed())
            {
                return sha1.ComputeHash(value);
            }
        }

        private static int GetNow()
        {
            var now = DateTime.Now;
            return (now.Year - 2000) * 31536000
                   + (now.Month - 1) * 2592000
                   + (now.Day - 1) * 86400
                   + now.Hour * 3600
                   + now.Minute * 60
                   + now.Second;
        }
    }
}
