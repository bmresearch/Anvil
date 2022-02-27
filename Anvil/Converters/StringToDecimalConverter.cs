using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Anvil.Converters
{
    /// <summary>
    /// Converts an input value from string to decimal. Specially useful for TextBox controls.
    /// </summary>
    public class StringToDecimalConverter : IValueConverter
    {
        /// <inheritdoc cref="Convert(object, Type, object, CultureInfo)"/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not decimal amount) return "";

            return amount.ToString();
        }

        /// <inheritdoc cref="ConvertBack(object, Type, object, CultureInfo)"/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string amount) return -1m;
            if (string.IsNullOrWhiteSpace(amount)) return -1m;

            var success = decimal.TryParse(amount, NumberStyles.Float, CultureInfo.InvariantCulture, out var decimalValue);
            if (success)
            {
                return decimalValue;
            }
            return -1m;
        }
    }
}
