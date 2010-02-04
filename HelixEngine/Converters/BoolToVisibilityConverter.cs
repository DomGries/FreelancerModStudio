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
        public BoolToVisibilityConverter()
        {
            Inverted = false;
        }

        public bool Inverted { get; set; }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var b = (bool) value;
            if (targetType == typeof (string))
                return b.ToString();
            if (targetType == typeof (Visibility))
            {
                if (b != Inverted)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility)
            {
                var v = (Visibility) value;
                if (v == Visibility.Visible)
                    return !Inverted;
                else
                    return Inverted;
            }
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}