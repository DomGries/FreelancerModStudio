using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace HelixEngine
{
    /// <summary>
    /// BooltoVisibility converter with MarkupExtension
    /// Usage: 
    ///   Visibility="{Binding Myproperty, Converter={helix:BoolToVisibilityConverter}}"
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityConverter : SelfProvider, IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var b = (bool)value;
            if (targetType == typeof(string))
                return b.ToString();
            if (targetType == typeof(Visibility))
                return b ? Visibility.Visible : Visibility.Collapsed;

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((Visibility)value) == Visibility.Visible;
        }

        #endregion
    }
}