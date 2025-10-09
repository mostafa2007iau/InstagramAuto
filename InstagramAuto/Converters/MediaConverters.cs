using Microsoft.Maui.Controls;
using System;
using System.Globalization;

namespace InstagramAuto.Client.Converters
{
    // Persian: رنگ Badge بر اساس نوع مدیا
    public class MediaTypeToBadgeColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var t = (value as string ?? "").ToUpperInvariant();
            return t switch
            {
                "REEL" => Color.FromArgb("#FF6B6B"),
                "CAROUSEL" => Color.FromArgb("#4D96FF"),
                "POST" => Color.FromArgb("#7B5BF2"),
                _ => Color.FromArgb("#999999"),
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => throw new NotImplementedException();
    }

    // Persian: اختیاری، در این صفحه لازم نیست
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => (value is bool b && b) ? true : false;

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class StringNotNullOrEmptyToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => !string.IsNullOrEmpty(value as string);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
