using SudokuApp.Common;
using SudokuApp.UWP.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Xamarin.Forms;

[assembly: Dependency(typeof(PhotoPickerService))]
namespace SudokuApp.UWP.Services
{
    /// <summary>
    /// 画像選択サービス（UWP）
    /// </summary>
    public class PhotoPickerService : IPhotoPickerService
    {
        /// <summary>
        /// 画像ライブラリから画像を選択する
        /// </summary>
        /// <returns></returns>
        public async Task<Stream> GetImageStreamAsync()
        {
            var openPicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };

            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");

            var storageFile = await openPicker.PickSingleFileAsync();

            if (storageFile == null)
            {
                return null;
            }

            var raStream = await storageFile.OpenReadAsync();
            return raStream.AsStreamForRead();
        }
    }
}
