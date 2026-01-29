using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace Desktop_Crypto_Portfolio_Tracker.ViewModels;

public class PriceColorConverter : IValueConverter
{
    
    public static readonly PriceColorConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        
        if (value is double change)
        {
            
            return change >= 0 ? Brushes.LightGreen : Brushes.PaleVioletRed;
        }
        
        return Brushes.White;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}