﻿using System.Text;

namespace Bronya.Xtensions
{
    public static class TextExtensions
    {
        public static string TrueNumbers(this int number, string nominativ, string genetiv, string plural)
        {
            var titles = new[] { nominativ, genetiv, plural };
            var cases = new[] { 2, 0, 1, 1, 1, 2 };
            return number + " " + titles[number % 100 > 4 && number % 100 < 20 ? 2 : cases[number % 10 < 5 ? number % 10 : 5]];
        }

        public static string ToHex(this byte[] bytes, bool upperCase)
        {
            StringBuilder result = new StringBuilder(bytes.Length * 2);
            for (int i = 0; i < bytes.Length; i++)
                result.Append(bytes[i].ToString(upperCase ? "X2" : "x2"));
            return result.ToString();
        }
    }
}
