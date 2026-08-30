namespace Crystal.Media;

/// <summary>Describes exact positive pixel dimensions.</summary>
public sealed record PixelSize
{
    /// <summary>Initializes exact pixel dimensions.</summary>
    /// <param name="width">The positive width in pixels.</param>
    /// <param name="height">The positive height in pixels.</param>
    public PixelSize(int width, int height)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(width),
                width,
                "Pixel width must be positive.");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(height),
                height,
                "Pixel height must be positive.");
        }

        Width = width;
        Height = height;
    }

    /// <summary>Gets the width in pixels.</summary>
    public int Width { get; }

    /// <summary>Gets the height in pixels.</summary>
    public int Height { get; }
}
