using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace Desktop_Crypto_Portfolio_Tracker.ViewModels;

public class PriceColorConverter : IValueConverter
{
    // Цей рядок дозволяє звертатися до конвертера через x:Static
    public static readonly PriceColorConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Якщо прийшло число (double)
        if (value is double change)
        {
            // Якщо більше або рівне 0 -> Зелений, інакше -> Червоний
            return change >= 0 ? Brushes.LightGreen : Brushes.PaleVioletRed;
        }
        // Якщо даних немає, колір білий
        return Brushes.White;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}