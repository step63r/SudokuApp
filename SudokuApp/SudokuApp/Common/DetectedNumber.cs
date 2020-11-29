using System.Collections.Generic;
using Xamarin.Forms;

namespace SudokuApp.Common
{
    /// <summary>
    /// Computer Visionにより検出された数字とそれを囲む多角形の頂点
    /// </summary>
    public class DetectedNumber
    {
        /// <summary>
        /// 検出された数字
        /// </summary>
        public int Number;
        /// <summary>
        /// 多角形の頂点
        /// </summary>
        public List<Point> Points;
    }
}
