// InstagramAuto.Client.Converters/InverseBoolConverter.cs
using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace InstagramAuto.Client.Converters
{
    public class InverseBoolConverter : IValueConverter
    {
        // مقدار ورودی (bool) را معکوس می‌کند
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
                return !b;

            return false;
        }

        // برای دوطرفه‌سازی اگر لازم باشد مقدار را برگردانده و معکوس می‌کند
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
                return !b;

            return false;
        }
    }
}
