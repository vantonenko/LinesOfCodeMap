namespace ConsoleApp5.Extensions;

internal static class StringExtensions
{
    internal static string JoinAsString<T>(this IEnumerable<T> items, string separator = "") => string.Join(separator, items);

    internal static string WithPrefix(this string str, string prefix) => $"{prefix}{str}";
}