using System;
using System.Globalization;
using System.Windows.Data;

namespace SCE24_BioMedSW_Blood_Establishment_WPF
{
    public class UserRoleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int roleNumber)
            {
                return Util.GetUserRole(roleNumber);
            }
            return "None";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string roleTitle)
            {
                return Util.GetUserRole(roleTitle);
            }
            return -1;
        }
    }
}