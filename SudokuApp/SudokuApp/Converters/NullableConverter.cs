using System;
using System.Globalization;
using Xamarin.Forms;

namespace SudokuApp.Converters
{
    /// <summary>
    /// Null許容型コンバータクラス
    /// </summary>
    public class NullableConverter : IValueConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType.GetGenericTypeDefinition() != typeof(Nullable<>))
            {
                return value;
            }
            if (!(value is string))
            {
                return value;
            }
            if (!string.IsNullOrEmpty((string)value))
            {
                return value;
            }
            return null;
        }
    }
}
