using System;
using System.Text;

namespace BrainFJit
{
    /// <summary>Provides extension methods on the <see cref="string"/> type.</summary>
    static class StringExtensions
    {
        /// <summary>
        ///     Converts the specified string to UTF-8.</summary>
        /// <param name="input">
        ///     String to convert to UTF-8.</param>
        /// <returns>
        ///     The specified string, converted to a byte-array containing the UTF-8 encoding of the string.</returns>
        public static byte[] ToUtf8(this string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            return Encoding.UTF8.GetBytes(input);
        }

        /// <summary>
        ///     Converts the specified string to UTF-16.</summary>
        /// <param name="input">
        ///     String to convert to UTF-16.</param>
        /// <returns>
        ///     The specified string, converted to a byte-array containing the UTF-16 encoding of the string.</returns>
        public static byte[] ToUtf16(this string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            return Encoding.Unicode.GetBytes(input);
        }

        /// <summary>
        ///     Converts the specified string to UTF-16 (Big Endian).</summary>
        /// <param name="input">
        ///     String to convert to UTF-16 (Big Endian).</param>
        /// <returns>
        ///     The specified string, converted to a byte-array containing the UTF-16 (Big Endian) encoding of the string.</returns>
        public static byte[] ToUtf16BE(this string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            return Encoding.BigEndianUnicode.GetBytes(input);
        }

        /// <summary>
        ///     Converts the specified raw UTF-8 data to a string.</summary>
        /// <param name="input">
        ///     Data to interpret as UTF-8 text.</param>
        /// <param name="removeBom">
        ///     <c>true</c> to remove the first character if it is a UTF-8 BOM.</param>
        /// <returns>
        ///     A string containing the characters represented by the UTF-8-encoded input.</returns>
        public static string FromUtf8(this byte[] input, bool removeBom = false)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            var result = Encoding.UTF8.GetString(input);
            if (removeBom && result[0] == '\ufeff')
                return result.Substring(1);
            return result;
        }

        /// <summary>
        ///     Converts the specified raw UTF-16 (little-endian) data to a string.</summary>
        /// <param name="input">
        ///     Data to interpret as UTF-16 text.</param>
        /// <returns>
        ///     A string containing the characters represented by the UTF-16-encoded input.</returns>
        public static string FromUtf16(this byte[] input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            return Encoding.Unicode.GetString(input);
        }

        /// <summary>
        ///     Converts the specified raw UTF-16 (big-endian) data to a string.</summary>
        /// <param name="input">
        ///     Data to interpret as UTF-16BE text.</param>
        /// <returns>
        ///     A string containing the characters represented by the UTF-16BE-encoded input.</returns>
        public static string FromUtf16BE(this byte[] input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            return Encoding.BigEndianUnicode.GetString(input);
        }

        /// <summary>
        ///     Determines the length of the UTF-8 encoding of the specified string.</summary>
        /// <param name="input">
        ///     String to determined UTF-8 length of.</param>
        /// <returns>
        ///     The length of the string in bytes when encoded as UTF-8.</returns>
        public static int Utf8Length(this string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            return Encoding.UTF8.GetByteCount(input);
        }

        /// <summary>
        ///     Determines the length of the UTF-16 encoding of the specified string.</summary>
        /// <param name="input">
        ///     String to determined UTF-16 length of.</param>
        /// <returns>
        ///     The length of the string in bytes when encoded as UTF-16.</returns>
        public static int Utf16Length(this string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            return Encoding.Unicode.GetByteCount(input);
        }
    }
}
