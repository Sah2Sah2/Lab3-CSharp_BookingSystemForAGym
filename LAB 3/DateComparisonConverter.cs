using System;
using System.Globalization;
using System.Windows.Data;

namespace BookingSystem.Gym
{
    public class DateComparisonConverter : IValueConverter // Using IValueConverter interface
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateValue && parameter is string comparisonType)
            {
                DateTime today = DateTime.Today;

                switch (comparisonType)
                {
                    case "Today":
                        return dateValue.Date == today;
                    case "Tomorrow":
                        return dateValue.Date == today.AddDays(1);
                    case "Other":
                        return dateValue.Date < today || dateValue.Date > today.AddDays(1);
                }
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
