using System;
using System.Linq;

namespace HomeServicePlatform.Application.Common.Helpers
{
    public static class SensitiveMask
    {
        public static string Tail(string? value, int visible = 4)
        {
            var digits = new string((value ?? string.Empty).Where(char.IsLetterOrDigit).ToArray());

            if (digits.Length == 0) return "****";
            if (digits.Length <= visible) return new string('*', 4) + digits;

            return new string('*', 4) + digits[^visible..];
        }
    }
}
