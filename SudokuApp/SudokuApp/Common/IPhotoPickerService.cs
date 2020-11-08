using System.IO;
using System.Threading.Tasks;

namespace SudokuApp.Common
{
    /// <summary>
    /// 写真選択インタフェース
    /// </summary>
    public interface IPhotoPickerService
    {
        /// <summary>
        /// 画像ライブラリから画像を選択する
        /// </summary>
        /// <returns></returns>
        Task<Stream> GetImageStreamAsync();
    }
}
