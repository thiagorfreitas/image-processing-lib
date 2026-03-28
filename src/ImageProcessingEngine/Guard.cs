namespace ImageProcessingEngine;

/// <summary>
/// Centralised guard / defensive-validation helpers.
/// Keeps validation logic DRY across all public methods.
/// </summary>
internal static class Guard
{
    /// <summary>Throws if <paramref name="value"/> is null or a zero-length array.</summary>
    public static void NotNullOrEmpty(byte[]? value, string paramName)
    {
        if (value is null)
            throw new ArgumentNullException(paramName, $"'{paramName}' must not be null.");

        if (value.Length == 0)
            throw new ArgumentException($"'{paramName}' must not be empty.", paramName);
    }

    /// <summary>Throws if <paramref name="value"/> is less than or equal to 0.</summary>
    public static void Positive(int value, string paramName, string message)
    {
        if (value <= 0)
            throw new ArgumentException(message, paramName);
    }

    /// <summary>Throws if <paramref name="value"/> is negative.</summary>
    public static void NotNegative(int value, string paramName, string message)
    {
        if (value < 0)
            throw new ArgumentException(message, paramName);
    }
}
