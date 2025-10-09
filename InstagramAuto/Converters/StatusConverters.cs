using System.Globalization;

namespace InstagramAuto.Client.Converters
{
    public class StatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status.ToLowerInvariant() switch
                {
                    "success" => Color.FromArgb("#28a745"),
                    "error" => Color.FromArgb("#dc3545"),
                    "warning" => Color.FromArgb("#ffc107"),
                    "in_progress" => Color.FromArgb("#007bff"),
                    _ => Color.FromArgb("#6c757d")
                };
            }
            return Color.FromArgb("#6c757d");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StatusIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status.ToLowerInvariant() switch
                {
                    "success" => Application.Current.Resources["SuccessIcon"],
                    "error" => Application.Current.Resources["ErrorIcon"],
                    "warning" => Application.Current.Resources["WarningIcon"],
                    "in_progress" => Application.Current.Resources["LoadingIcon"],
                    _ => null
                };
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class UsageColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double usage)
            {
                return usage switch
                {
                    < 0.7 => Color.FromArgb("#28a745"),  // Green
                    < 0.9 => Color.FromArgb("#ffc107"),  // Yellow
                    _ => Color.FromArgb("#dc3545")       // Red
                };
            }
            return Color.FromArgb("#6c757d");  // Gray
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TimeAgoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime time)
            {
                var span = DateTime.Now - time;

                if (span.TotalMinutes < 1)
                    return "?? ?????";
                if (span.TotalMinutes < 60)
                    return $"{(int)span.TotalMinutes} ????? ???";
                if (span.TotalHours < 24)
                    return $"{(int)span.TotalHours} ???? ???";
                if (span.TotalDays < 7)
                    return $"{(int)span.TotalDays} ??? ???";
                if (span.TotalDays < 30)
                    return $"{(int)(span.TotalDays / 7)} ???? ???";
                if (span.TotalDays < 365)
                    return $"{(int)(span.TotalDays / 30)} ??? ???";
                
                return $"{(int)(span.TotalDays / 365)} ??? ???";
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}