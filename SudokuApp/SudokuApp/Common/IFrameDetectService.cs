using System.IO;
using System.Threading.Tasks;

namespace SudokuApp.Common
{
    /// <summary>
    /// 数独フレーム検出インタフェース
    /// </summary>
    public interface IFrameDetectService
    {
        /// <summary>
        /// 数独フレームを検出する
        /// </summary>
        /// <param name="image"></param>
        /// <returns>検出結果</returns>
        Task<object> GetFrameAsync(Stream image);
    }
}
