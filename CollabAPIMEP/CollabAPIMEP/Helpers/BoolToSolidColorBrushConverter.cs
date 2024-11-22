using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace CollabAPIMEP.Helpers
{
    public class BoolToSolidColorBrushConverter : IValueConverter
    {
        public SolidColorBrush TrueBrush { get; set; }
        public SolidColorBrush FalseBrush { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? TrueBrush : FalseBrush;
            }
            return FalseBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
