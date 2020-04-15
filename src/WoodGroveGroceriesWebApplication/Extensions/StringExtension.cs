namespace WoodGroveGroceriesWebApplication.Extensions
{
    using System;
    using System.Text;

    public static class StringExtension
    {
        public static string ToBase64Encode(this string source)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(source ?? string.Empty);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string ToBase64Decode(this string source)
        {
            var data = Convert.FromBase64String(source ?? string.Empty);
            return Encoding.UTF8.GetString(data);
        }
    }
}