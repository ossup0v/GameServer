public static class IComparableExtensions
{
    public static bool IsValueEquals<T>(this IComparable<T> source, T other)
        => source.CompareTo(other) == 0;
}
