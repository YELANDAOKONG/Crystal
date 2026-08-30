namespace Crystal.Media;

/// <summary>Describes a normalized positive width-to-height ratio.</summary>
public sealed record AspectRatio
{
    /// <summary>Initializes and normalizes a width-to-height ratio.</summary>
    /// <param name="width">The positive relative width.</param>
    /// <param name="height">The positive relative height.</param>
    public AspectRatio(int width, int height)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(width),
                width,
                "Aspect-ratio width must be positive.");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(height),
                height,
                "Aspect-ratio height must be positive.");
        }

        var divisor = GreatestCommonDivisor(width, height);
        Width = width / divisor;
        Height = height / divisor;
    }

    /// <summary>Gets the normalized relative width.</summary>
    public int Width { get; }

    /// <summary>Gets the normalized relative height.</summary>
    public int Height { get; }

    private static int GreatestCommonDivisor(int left, int right)
    {
        while (right != 0)
        {
            var remainder = left % right;
            left = right;
            right = remainder;
        }

        return left;
    }
}
