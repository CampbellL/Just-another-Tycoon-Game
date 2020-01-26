using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using System.Globalization;

using UnityEngine;
using UnityEngine.UI;

namespace UI.Xml
{
    public static class StringExtensions
    {
        /// <summary>
        /// Remove the specified characters from the string.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="chars"></param>
        /// <returns></returns>
        public static string StripChars(this string s, params char[] chars)
        {
            if (string.IsNullOrEmpty(s) || chars.Length == 0) return s;

            return string.Join(string.Empty, s.Split(chars));

            // Replaced this with the above (should be more GC friendly)
            /*
            var pattern = "[" + String.Join("", chars.Select(c => c.ToString()).ToArray()) + "]";
            var regex = new Regex(pattern);

            return regex.Replace(s, "");
            */
        }

        /// <summary>
        /// Returns the string with spaced in front of each capital letter.
        /// Existing whitespace is left as is.
        /// If the string starts with a capital letter, no space is added for that letter.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string SplitByCapitals(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                s = string.Empty;
            }

            var regex = new Regex(@"
                (?<=[A-Z])(?=[A-Z][a-z]) |
                 (?<=[^A-Z])(?=[A-Z]) |
                 (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);

            return regex.Replace(s, " ");
        }

        public static string ToTitleCase(this string s)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
        }

        public static bool EndsWithAny(this string name, string[] endings)
        {
            return endings.Any(x => name.EndsWith(x));
        }

        /// <summary>
        /// https://stackoverflow.com/questions/1615559/convert-a-unicode-string-to-an-escaped-ascii-string
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string DecodeEncodedNonAsciiCharacters(string value)
        {
            if (value == null) return null;

            return Regex.Replace(
                value,
                @"\\[uU](?<Value>[a-zA-Z0-9]{4})",
                m =>
                {
                    return ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString();
                });
        }

        public static bool IsUrl(this string path)
        {
            Uri uriResult;
            bool result = Uri.TryCreate(path, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            return result;
        }
    }
}
