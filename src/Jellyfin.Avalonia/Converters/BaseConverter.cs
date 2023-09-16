#pragma warning disable SA1402 // One type per file

using System.Globalization;
using Avalonia.Data.Converters;
using Jellyfin.Avalonia.Extensions;

namespace Jellyfin.Avalonia.Converters;

/// <summary>
/// Abstract class used to implement converters that implements the ConvertBack logic.
/// </summary>
/// <typeparam name="TFrom">Type of the input value.</typeparam>
/// <typeparam name="TTo">Type of the output value.</typeparam>
/// <typeparam name="TParam">Type of parameter.</typeparam>
public abstract class BaseConverter<TFrom, TTo, TParam> : IValueConverter
{
    /// <summary>
    /// Method that will be called by <see cref="IValueConverter.Convert(object?, Type, object?, CultureInfo?)"/>.
    /// </summary>
    /// <param name="value">The object to convert <typeparamref name="TFrom"/> to <typeparamref name="TTo"/>.</param>
    /// <param name="parameter">Optional Parameters.</param>
    /// <param name="culture">Culture Info.</param>
    /// <returns>An object of type <typeparamref name="TTo"/>.</returns>
    protected abstract TTo ConvertFrom(TFrom value, TParam parameter, CultureInfo? culture);

    /// <summary>
    /// Method that will be called by <see cref="IValueConverter.ConvertBack(object?, Type, object?, CultureInfo?)"/>.
    /// </summary>
    /// <param name="value">Value to be converted from <typeparamref name="TTo"/> to <typeparamref name="TFrom"/>.</param>
    /// <param name="parameter">Optional Parameters.</param>
    /// <param name="culture">Culture Info.</param>
    /// <returns>An object of type <typeparamref name="TFrom"/>.</returns>
    protected abstract TFrom ConvertBackTo(TTo value, TParam parameter, CultureInfo? culture);

    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var converterParameter = ValueConverterExtension.ConvertValue<TParam>(parameter);
        var converterValue = ValueConverterExtension.ConvertValue<TFrom>(value);

        return ConvertFrom(converterValue, converterParameter, culture);
    }

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var converterParameter = ValueConverterExtension.ConvertValue<TParam>(parameter);
        var converterValue = ValueConverterExtension.ConvertValue<TTo>(value);

        return ConvertBackTo(converterValue, converterParameter, culture);
    }
}

/// <summary>
/// Abstract class used to implement converters that implements the ConvertBack logic.
/// </summary>
/// <typeparam name="TFrom">Type of the input value.</typeparam>
/// <typeparam name="TTo">Type of the output value.</typeparam>
public abstract class BaseConverter<TFrom, TTo> : IValueConverter
{
    /// <summary>
    /// Method that will be called by <see cref="IValueConverter.Convert(object?, Type, object?, CultureInfo?)"/>.
    /// </summary>
    /// <param name="value">The object to convert <typeparamref name="TFrom"/> to <typeparamref name="TTo"/>.</param>
    /// <param name="culture">Culture Info.</param>
    /// <returns>An object of type <typeparamref name="TTo"/>.</returns>
    protected abstract TTo ConvertFrom(TFrom value, CultureInfo? culture);

    /// <summary>
    /// Method that will be called by <see cref="IValueConverter.ConvertBack(object?, Type, object?, CultureInfo?)"/>.
    /// </summary>
    /// <param name="value">Value to be converted from <typeparamref name="TTo"/> to <typeparamref name="TFrom"/>.</param>
    /// <param name="culture">Culture Info.</param>
    /// <returns>An object of type <typeparamref name="TFrom"/>.</returns>
    protected abstract TFrom ConvertBackTo(TTo value, CultureInfo? culture);

    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var converterValue = ValueConverterExtension.ConvertValue<TFrom>(value);

        return ConvertFrom(converterValue, culture);
    }

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var converterValue = ValueConverterExtension.ConvertValue<TTo>(value);

        return ConvertBackTo(converterValue, culture);
    }
}
