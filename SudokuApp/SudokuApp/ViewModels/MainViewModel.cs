using SudokuApp.Common;
using Xamarin.Forms;

namespace SudokuApp.ViewModels
{
    /// <summary>
    /// MainPageのViewModelクラス
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        #region プロパティ
        /// <summary>
        /// カメラ起動コマンド
        /// </summary>
        public Command ExecuteCameraCommand { get; private set; }
        /// <summary>
        /// 数独解答コマンド
        /// </summary>
        public Command ExecuteSolveCommand { get; private set; }
        /// <summary>
        /// 消去コマンド
        /// </summary>
        public Command ExecuteClearCommand { get; private set; }
        /// <summary>
        /// 画像選択コマンド
        /// </summary>
        public Command ExecutePickPhotoCommand { get; private set; }
        #endregion

        #region コンストラクタ
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainViewModel()
        {
            ExecuteCameraCommand = new Command(ExecuteCamera, CanExecuteCamera);
            ExecuteSolveCommand = new Command(ExecuteSolve, CanExecuteSolve);
            ExecuteClearCommand = new Command(ExecuteClear, CanExecuteClear);
            ExecutePickPhotoCommand = new Command(ExecutePickPhoto, CanExecutePickPhoto);
        }
        #endregion

        #region メソッド
        /// <summary>
        /// カメラ起動
        /// </summary>
        public void ExecuteCamera()
        {

        }
        /// <summary>
        /// カメラ起動が実行可能か
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteCamera()
        {
            return false;
        }

        /// <summary>
        /// 数独の解答を実行する
        /// </summary>
        public void ExecuteSolve()
        {
            MessagingCenter.Send(this, "ExecuteAsync");
        }
        /// <summary>
        /// 数独の解答が実行可能かどうか
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteSolve()
        {
            return true;
        }

        /// <summary>
        /// 消去を実行する
        /// </summary>
        public void ExecuteClear()
        {
            MessagingCenter.Send(this, "ExecuteClear");
        }
        /// <summary>
        /// 消去が実行可能かどうか
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteClear()
        {
            return true;
        }

        /// <summary>
        /// 画像選択を実行する
        /// </summary>
        public async void ExecutePickPhoto()
        {
            var stream = await DependencyService.Get<IPhotoPickerService>().GetImageStreamAsync();
            if (stream != null)
            {
                var frameDetectResult = await DependencyService.Get<IFrameDetectService>().GetFrameAsync(stream);

                //var result = await ComputerVisionManager.ReadFileLocal(stream);
                //// TODO: 読み取った画像をAzure Cognitive Servicesで認識させ、読み取った配列を格納する
                //var array = new BindableTwoDArray<int?>(9, 9);
                //MessagingCenter.Send(this, "ExecutePickPhoto", array);
            }
        }
        /// <summary>
        /// 画像選択が実行可能かどうか
        /// </summary>
        /// <returns></returns>
        private bool CanExecutePickPhoto()
        {
            return true;
        }
        #endregion
    }
}
