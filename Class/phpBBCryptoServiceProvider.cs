#region

#region

using System;
using System.Security.Cryptography;
using System.Text;

#endregion

/*
    Copyright (C) 2014 LeagueSharp

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

namespace LeagueSharp.Loader.Class
{
    /// <summary>
    /// Computes the phpBB/SubMD5 hash value for the input data using the implementation provided by http://openwall.com/phpass/ modified by http://www.phpbb.com/ adjusted for L# by pqmailer.
    /// </summary>
    /// <remarks>
    /// Ported by Ryan Irecki
    /// Website: http://www.digilitepc.net/
    /// E-mail: razchek@gmail.com
    /// </remarks>
    internal class phpBBCryptoServiceProvider
    {
        /// <summary>
        /// The encryption string base.
        /// </summary>
        private const string Itoa64 = "./0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        /// <summary>
        /// Compares the password string given with the hash retrieved from your database.
        /// </summary>
        /// <param name="password">Plaintext password.</param>
        /// <param name="hash">Hash from a SQL database</param>
        /// <returns>True if the password is correct, False otherwise.</returns>
        public bool CheckHash(string password, string hash)
        {
            if (hash.Length == 34)
            {
                return (HashCryptPrivate(Encoding.ASCII.GetBytes(password), hash, Itoa64) == hash);
            }
            return false;
        }

        /// <summary>
        /// This function will return the resulting hash from the password string you specify.
        /// </summary>
        /// <param name="password">String to hash.</param>
        /// <returns>Encrypted hash.</returns>
        /// <remarks>
        /// Although this will return the md5 for an older password, I have not added
        /// support for older passwords, so they will not work with this class unless
        /// I or someone else updates it.
        /// </remarks>
        public string Hash(string password)
        {
            // Generate a random string from a random number with the length of 6.
            // You could use a static string instead, doesn't matter. E.g.
            // byte[] random = ASCIIEncoding.ASCII.GetBytes("abc123");
            var random = Encoding.ASCII.GetBytes(new Random().Next(100000, 999999).ToString());

            var hash = HashCryptPrivate(Encoding.ASCII.GetBytes(password), HashGensaltPrivate(random, Itoa64), Itoa64);

            if (hash.Length == 34)
            {
                return hash;
            }

            return sMD5(password);
        }

        /// <summary>
        /// The workhorse that encrypts your hash.
        /// </summary>
        /// <param name="password">String to be encrypted. Use: ASCIIEncoding.ASCII.GetBytes();</param>
        /// <param name="genSalt">Generated salt.</param>
        /// <param name="itoa64">The itoa64 string.</param>
        /// <returns>The encrypted hash ready to be compared.</returns>
        /// <remarks>
        /// password:  Saves conversion inside the function, lazy coding really.
        /// genSalt:   Returns from hashGensaltPrivate(random, itoa64);
        /// return:    Compare with phpbbCheckHash(password, hash)
        /// </remarks>
        private static string HashCryptPrivate(byte[] password, string genSalt, string itoa64)
        {
            var output = "*";
            var md5 = new MD5CryptoServiceProvider();
            if (!genSalt.StartsWith("$H$"))
            {
                return output;
            }
            //   $count_log2 = strpos($itoa64, $setting[3]);
            var countLog2 = itoa64.IndexOf(genSalt[3]);
            if (countLog2 < 7 || countLog2 > 30)
            {
                return output;
            }

            var count = 1 << countLog2;
            var salt = Encoding.ASCII.GetBytes(genSalt.Substring(4, 8));

            if (salt.Length != 8)
            {
                return output;
            }

            var hash = md5.ComputeHash(Combine(salt, password));

            do
            {
                hash = md5.ComputeHash(Combine(hash, password));
            } while (count-- > 1);

            output = genSalt.Substring(0, 12);
            output += HashEncode64(hash, 16, itoa64);

            return output;
        }

        /// <summary>
        /// Private function to concat byte arrays.
        /// </summary>
        /// <param name="b1">Source array.</param>
        /// <param name="b2">Array to add to the source array.</param>
        /// <returns>Combined byte array.</returns>
        private static byte[] Combine(byte[] b1, byte[] b2)
        {
            var retVal = new byte[b1.Length + b2.Length];
            Array.Copy(b1, 0, retVal, 0, b1.Length);
            Array.Copy(b2, 0, retVal, b1.Length, b2.Length);
            return retVal;
        }

        /// <summary>
        /// Encode the hash.
        /// </summary>
        /// <param name="input">The hash to encode.</param>
        /// <param name="count">[This parameter needs documentation].</param>
        /// <param name="itoa64">The itoa64 string.</param>
        /// <returns>Encoded hash.</returns>
        private static string HashEncode64(byte[] input, int count, string itoa64)
        {
            var output = "";
            var i = 0;

            do
            {
                int value = input[i++];
                output += itoa64[value & 0x3f];

                if (i < count)
                {
                    value |= input[i] << 8;
                }
                output += itoa64[(value >> 6) & 0x3f];
                if (i++ >= count)
                {
                    break;
                }

                if (i < count)
                {
                    value |= input[i] << 16;
                }
                output += itoa64[(value >> 12) & 0x3f];
                if (i++ >= count)
                {
                    break;
                }

                output += itoa64[(value >> 18) & 0x3f];
            } while (i < count);

            return output;
        }

        /// <summary>
        /// Generate salt for hash generation.
        /// </summary>
        /// <param name="input">Any random information.</param>
        /// <param name="itoa64">The itoa64 string.</param>
        /// <returns>Generated salt string</returns>
        private static string HashGensaltPrivate(byte[] input, string itoa64)
        {
            const int iterationCountLog2 = 6;

            var output = "$H$";
            output += itoa64[Math.Min(iterationCountLog2 + 5, 30)];
            output += HashEncode64(input, 6, itoa64);

            return output;
        }

        /// <summary>
        /// Returns a hexadecimal string representation for the encrypted MD5 parameter.
        /// </summary>
        /// <param name="password">String to be encrypted.</param>
        /// <returns>String</returns>
        private string sMD5(string password)
        {
            return sMD5(password, false);
        }

        /// <summary>
        /// Returns a hexadecimal string representation for the encrypted MD5 parameter.
        /// </summary>
        /// <param name="password">String to be encrypted.</param>
        /// <param name="raw">Whether or not to produce a raw string.</param>
        /// <returns>String</returns>
        private string sMD5(string password, bool raw)
        {
            var md5 = new MD5CryptoServiceProvider();

            return raw
                ? Encoding.ASCII.GetString(md5.ComputeHash(Encoding.ASCII.GetBytes(password)))
                : BitConverter.ToString(md5.ComputeHash(Encoding.ASCII.GetBytes(password))).Replace("-", "");
        }
    }
}