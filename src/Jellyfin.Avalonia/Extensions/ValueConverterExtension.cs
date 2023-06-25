namespace Jellyfin.Avalonia.Extensions;

/// <summary>
/// Extensions for value converters.
/// </summary>
public abstract class ValueConverterExtension
{
    private static bool IsNullable<T>()
    {
        var type = typeof(T);

        if (!type.IsValueType)
        {
            return true; // ref-type
        }

        return Nullable.GetUnderlyingType(type) is not null;
    }

#pragma warning disable CS8603 // Possible null reference return. If TParam is null (e.g. `string?`), a null return value is expected
    /// <summary>
    /// Convert the value to the target type.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <typeparam name="TValue">The target type.</typeparam>
    /// <returns>The converted value.</returns>
    public static TValue ConvertValue<TValue>(object? value) => value switch
    {
        null when IsNullable<TValue>() => default,
        null when !IsNullable<TValue>() => throw new ArgumentNullException(nameof(value), $"Value cannot be null because {nameof(TValue)} is not nullable"),
        TValue convertedValue => convertedValue,
        _ => throw new ArgumentException($"Value needs to be of type {typeof(TValue)}", nameof(value))
    };
#pragma warning restore CS8603 // Possible null reference return.
}
