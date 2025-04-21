public class UserTypeColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string userType)
        {
            switch (userType)
            {
                case "Student":
                    return new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Green
                case "Employee":
                    return new SolidColorBrush(Color.FromRgb(33, 150, 243)); // Blue
                case "Visitor":
                    return new SolidColorBrush(Color.FromRgb(255, 152, 0)); // Orange
                default:
                    return new SolidColorBrush(Color.FromRgb(158, 158, 158)); // Gray
            }
        }

        return new SolidColorBrush(Color.FromRgb(158, 158, 158));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class EventTypeColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string eventType)
        {
            switch (eventType)
            {
                case "ENTRY":
                    return new SolidColorBrush(Color.FromRgb(255, 235, 59)); // Yellow
                case "EXIT":
                    return new SolidColorBrush(Color.FromRgb(244, 67, 54)); // Red
                default:
                    return new SolidColorBrush(Color.FromRgb(158, 158, 158)); // Gray
            }
        }

        return new SolidColorBrush(Color.FromRgb(158, 158, 158));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}