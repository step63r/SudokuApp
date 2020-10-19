using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SudokuApp.Common
{
    /// <summary>
    /// オブジェクト操作系拡張メソッド
    /// </summary>
    public static class ObjectHelper
    {
        /// <summary>
        /// オブジェクトをdeep copyする
        /// </summary>
        /// <typeparam name="T">型</typeparam>
        /// <param name="src">コピー元オブジェクト</param>
        /// <returns>コピー先オブジェクト</returns>
        public static T DeepCopy<T>(this T src)
        {
            using (var memoryStream = new MemoryStream())
            {
                var binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(memoryStream, src);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return (T)binaryFormatter.Deserialize(memoryStream);
            }
        }
    }
}
