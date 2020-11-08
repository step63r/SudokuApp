using Android.Content;
using SudokuApp.Common;
using SudokuApp.Droid.Services;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(PhotoPickerService))]
namespace SudokuApp.Droid.Services
{
    /// <summary>
    /// 画像選択サービス（Android）
    /// </summary>
    public class PhotoPickerService : IPhotoPickerService
    {
        /// <summary>
        /// 画像ライブラリから画像を選択する
        /// </summary>
        /// <returns></returns>
        public Task<Stream> GetImageStreamAsync()
        {
            var intent = new Intent();
            intent.SetType("image/*");
            intent.SetAction(Intent.ActionGetContent);

            MainActivity.Instance.StartActivityForResult(Intent.CreateChooser(intent, "Select Picture"), MainActivity._pickImageId);
            MainActivity.Instance.PickImageTaskCompletionSource = new TaskCompletionSource<Stream>();

            return MainActivity.Instance.PickImageTaskCompletionSource.Task;
        }
    }
}