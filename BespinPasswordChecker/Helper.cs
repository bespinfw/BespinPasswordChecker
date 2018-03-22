using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace BespinPasswordChecker
{
    public class Helper
    {
        public static string HashFromConsole()
        {
            ConsoleKeyInfo s = new ConsoleKeyInfo();
            string secure = "";

            s = Console.ReadKey(true);
            while (s.Key != ConsoleKey.Enter)
            {
                secure = secure + s.KeyChar;
                Console.Write("*");
                s = Console.ReadKey(true);
            }
            Console.WriteLine();
            return Hash(secure);
        }

        private static string Hash(string input)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    // can be "x2" if you want lowercase
                    sb.Append(b.ToString("X2"));
                }

                return sb.ToString();
            }
        }
    }
}
