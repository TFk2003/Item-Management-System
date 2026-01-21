namespace MyAppMVC.Extensions
{
    public static class StringExtensions
    {
        public static string Truncate(this string value, int maxLength, string truncationSuffix = "...")
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength) + truncationSuffix;
        }
    }
}
