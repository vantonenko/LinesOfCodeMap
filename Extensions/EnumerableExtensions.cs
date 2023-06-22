#pragma warning disable CS8321
using ConsoleApp5.Extensions;

namespace ConsoleApp5.Extensions;

internal static class EnumerableExtensions
{
    internal static IEnumerable<T> Concat<T>(this IEnumerable<T> items, T item) => items.Concat(new[] { item });
}