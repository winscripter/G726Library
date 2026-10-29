namespace G726Library;

#pragma warning disable CA2208

/// <summary>
/// Simple guard. Can't use ArgumentOutOfRangeException.ThrowIfGreaterThan for example as we have
/// to target .NET Framework which doesn't support this...
/// </summary>
internal static class Guardian
{
    /// <summary>
    /// Throws an exception if n is negative.
    /// </summary>
    /// <param name="n">Value to ensure not to be negative.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if n is negative.</exception>
    public static void ThrowIfNegative(int n, string parameterName)
    {
        if (n < 0)
        {
            throw new ArgumentOutOfRangeException("The parameter cannot be negative", parameterName);
        }
    }

    /// <summary>
    /// Throws an exception if <paramref name="value"/> is greater than <paramref name="max"/>.
    /// </summary>
    /// <param name="value">The value that must not be greater than <paramref name="max"/>.</param>
    /// <param name="max">Maximum value.</param>
    /// <param name="parameterName">The parameter name.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if n is negative.</exception>
    public static void ThrowIfGreaterThan(int value, int max, string parameterName)
    {
        if (value > max)
        {
            throw new ArgumentOutOfRangeException("The parameter is too large", parameterName);
        }
    }

    /// <summary>
    /// Throws an exception if <paramref name="value"/> is null.
    /// </summary>
    /// <typeparam name="T">The type of <paramref name="value"/>.</typeparam>
    /// <param name="value">The value that should not be null.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static void ThrowIfNull<T>(T? value, string parameterName)
    {
        if (value is null)
        {
            throw new ArgumentNullException(parameterName);
        }
    }

    public static void CheckDisposed(ref bool disposed, string name)
    {
        if (disposed)
        {
            throw new ObjectDisposedException(name);
        }

        disposed = true;
    }
}
