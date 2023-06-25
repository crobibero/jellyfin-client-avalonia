#pragma warning disable SA1402 // One type per file

using System.Globalization;

namespace Jellyfin.Avalonia.Converters;

/// <summary>
/// Abstract class used to implement converters that implements the Convert logic.
/// </summary>
/// <typeparam name="TFrom">Type of the input value.</typeparam>
/// <typeparam name="TTo">Type of the output value.</typeparam>
public abstract class BaseConverterOneWay<TFrom, TTo> : BaseConverter<TFrom, TTo>
{
    /// <inheritdoc/>
    protected sealed override TFrom ConvertBackTo(TTo value, CultureInfo? culture) =>
        throw new NotSupportedException("Impossible to revert to original value. Consider setting BindingMode to OneWay.");
}

/// <summary>
/// Abstract class used to implement converters that implements the Convert logic.
/// </summary>
/// <typeparam name="TFrom">Type of the input value.</typeparam>
/// <typeparam name="TTo">Type of the output value.</typeparam>
/// <typeparam name="TParam">Type of parameter.</typeparam>
public abstract class BaseConverterOneWay<TFrom, TTo, TParam> : BaseConverter<TFrom, TTo, TParam>
{
    /// <inheritdoc/>
    protected sealed override TFrom ConvertBackTo(TTo value, TParam? parameter, CultureInfo? culture) =>
        throw new NotSupportedException("Impossible to revert to original value. Consider setting BindingMode to OneWay.");
}
