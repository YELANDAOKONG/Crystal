namespace Crystal.Internal;

internal static class CollectionSnapshot
{
    public static IReadOnlyList<T> Create<T>(
        IEnumerable<T> values,
        string parameterName,
        bool allowEmpty = true)
    {
        ArgumentNullException.ThrowIfNull(values, parameterName);

        var snapshot = values.ToArray();

        if (!allowEmpty && snapshot.Length == 0)
        {
            throw new ArgumentException(
                "Collection cannot be empty.",
                parameterName);
        }

        if (snapshot.Any(static value => value is null))
        {
            throw new ArgumentException(
                "Collection cannot contain null values.",
                parameterName);
        }

        return Array.AsReadOnly(snapshot);
    }
}
