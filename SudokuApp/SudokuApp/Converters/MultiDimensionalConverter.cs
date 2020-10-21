using System;
using System.Globalization;
using Xamarin.Forms;

namespace SudokuApp.Converters
{
    /// <summary>
    /// 多次元配列コンバータクラス
    /// </summary>
    public class MultiDimensionalConverter : IMultiValueConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="values"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == null)
            {
                return null;
            }
            return (values[0] as string[,])[(int)values[1], (int)values[2]];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetTypes"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
            //return new[] { value };
        }
    }
}
