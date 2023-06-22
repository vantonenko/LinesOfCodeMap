namespace ConsoleApp5.Extensions;

internal static class UnitsExtensions
{
    private static readonly List<(int capacity, string unit)> TimeUnits = new()
    {
        (1000, "ms"),
        (60, "sec"),
        (60, "min"),
        (60, "hours")
    };

    private static readonly List<(int capacity, string unit)> CountUnits = new()
    {
        (1000, ""),
        (1000, "K"),
        (1000, "M"),
        (1000, "B")
    };

    private static readonly List<(int capacity, string unit)> SizeUnits = new()
    {
        (1000, "b"),
        (1000, "Kb"),
        (1000, "Mb"),
        (1000, "Tb")
    };

    internal static string AsTime(this TimeSpan timeSpan) => timeSpan.TotalMilliseconds.AsTime();
    internal static string AsTime(this double milliseconds) => AutoAdjustUnits(milliseconds, TimeUnits);
    internal static string AsCount(this int count) => AutoAdjustUnits(count, CountUnits, false);
    internal static string AsSize(this long count) => AutoAdjustUnits(count, SizeUnits);
    internal static string AsSize(this int count) => AutoAdjustUnits(count, SizeUnits);

    private static string AutoAdjustUnits(double value, List<(int capacity, string unit)> units, bool spacePrefix = true)
    {
        foreach ((int capacity, string unit) in units)
        {
            if (value > capacity)
            {
                value /= capacity;
            }
            else
            {
                return string.IsNullOrEmpty(unit)
                    ? $"{(int)value}"
                    : $"{value:F1}{(spacePrefix ? " ": "")}{unit}";
            }
        }

        return $"{value:F1} {units.FirstOrDefault().unit}";
    }
}