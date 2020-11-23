using OpenCVBridge;
using SudokuApp.Common;
using SudokuApp.UWP.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Xamarin.Forms;

[assembly: Dependency(typeof(FrameDetectService))]
namespace SudokuApp.UWP.Services
{
    /// <summary>
    /// 画像フレーム検出サービス（UWP）
    /// </summary>
    public class FrameDetectService : IFrameDetectService
    {
        /// <summary>
        /// 数独フレームを検出する
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public async Task<object> GetFrameAsync(Stream image)
        {
            SoftwareBitmap inputBitmap;
            using (var raStream = image.AsRandomAccessStream())
            {
                var decoder = await BitmapDecoder.CreateAsync(raStream);
                inputBitmap = await decoder.GetSoftwareBitmapAsync();

                // グレースケール
                if (inputBitmap.BitmapPixelFormat != BitmapPixelFormat.Gray8 || inputBitmap.BitmapAlphaMode != BitmapAlphaMode.Ignore)
                {
                    inputBitmap = SoftwareBitmap.Convert(inputBitmap, BitmapPixelFormat.Gray8, BitmapAlphaMode.Ignore);
                }

                var helper = new OpenCVHelper();
                //helper.Canny(inputBitmap, outputBitmap, 100, 200);
                helper.DetectFrame(inputBitmap);

                inputBitmap = SoftwareBitmap.Convert(inputBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);



                // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                // Test
                var fileSavePicker = new FileSavePicker()
                {
                    SuggestedStartLocation = PickerLocationId.PicturesLibrary

                };
                fileSavePicker.FileTypeChoices.Add("PNG files", new List<string>() { ".png" });
                var storageFile = await fileSavePicker.PickSaveFileAsync();
                if (storageFile != null)
                {
                    SaveSoftwareBitmapToFileAsync(inputBitmap, storageFile);
                }
                // <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            }
            return default(object);
        }

        private async void SaveSoftwareBitmapToFileAsync(SoftwareBitmap softwareBitmap, StorageFile outputFile)
        {
            using (var stream = await outputFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                encoder.SetSoftwareBitmap(softwareBitmap);
                await encoder.FlushAsync();
            }
        }
    }
}
