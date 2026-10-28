using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Flarial.Launcher.Converters;

public sealed class PercentageToFraction : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
            return 0d;

        return double.TryParse(value.ToString(), out var number)
            ? Math.Clamp(number / 100d, 0d, 1d)
            : 0d;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
            return 0d;

        return double.TryParse(value.ToString(), out var number) ? number * 100d : 0d;
    }
}
