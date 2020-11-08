using Foundation;
using SudokuApp.Common;
using System;
using System.IO;
using System.Threading.Tasks;
using UIKit;

namespace SudokuApp.iOS.Services
{
    /// <summary>
    /// 画像選択サービス（iOS）
    /// </summary>
    public class PhotoPickerService : IPhotoPickerService
    {
        #region メンバ変数
        /// <summary>
        /// TaskCompletionSource
        /// </summary>
        private TaskCompletionSource<Stream> _taskCompletionSource;
        /// <summary>
        /// UIImagePickerController
        /// </summary>
        private UIImagePickerController _imagePicker;
        #endregion

        /// <summary>
        /// 画像ライブラリから画像を選択する
        /// </summary>
        /// <returns></returns>
        public Task<Stream> GetImageStreamAsync()
        {
            _imagePicker = new UIImagePickerController
            {
                SourceType = UIImagePickerControllerSourceType.PhotoLibrary,
                MediaTypes = UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.PhotoLibrary)
            };

            _imagePicker.FinishedPickingMedia += OnImagePickerFinishedPickingMedia;
            _imagePicker.Canceled += OnImagePickerCancelled;

            var window = UIApplication.SharedApplication.KeyWindow;
            var viewController = window.RootViewController;
            viewController.PresentViewController(_imagePicker, true, null);

            _taskCompletionSource = new TaskCompletionSource<Stream>();
            return _taskCompletionSource.Task;
        }

        /// <summary>
        /// ImagePickerが実行された時のイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void OnImagePickerFinishedPickingMedia(object sender, UIImagePickerMediaPickedEventArgs args)
        {
            var image = args.EditedImage ?? args.OriginalImage;

            if (image != null)
            {
                NSData data;
                if (args.ReferenceUrl.PathExtension.Equals("PNG") || args.ReferenceUrl.PathExtension.Equals("png"))
                {
                    data = image.AsPNG();
                }
                else
                {
                    data = image.AsJPEG(1);
                }
                var stream = data.AsStream();

                UnregisterEventHandlers();

                _taskCompletionSource.SetResult(stream);
            }
            else
            {
                UnregisterEventHandlers();
                _taskCompletionSource.SetResult(null);
            }
            _imagePicker.DismissModalViewController(true);
        }

        /// <summary>
        /// ImagePickerがキャンセルされた時のイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void OnImagePickerCancelled(object sender, EventArgs args)
        {
            UnregisterEventHandlers();
            _taskCompletionSource.SetResult(null);
            _imagePicker.DismissModalViewController(true);
        }

        /// <summary>
        /// イベントハンドラを登録解除する
        /// </summary>
        void UnregisterEventHandlers()
        {
            _imagePicker.FinishedPickingMedia -= OnImagePickerFinishedPickingMedia;
            _imagePicker.Canceled -= OnImagePickerCancelled;
        }
    }
}