using SudokuApp.Common;
using SudokuApp.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Xamarin.Forms;

namespace SudokuApp.ViewModels
{
    /// <summary>
    /// MainPageのViewModelクラス
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        #region プロパティ
        private Sudoku _sudoku = new Sudoku();
        /// <summary>
        /// 数独クラス
        /// </summary>
        public Sudoku Sudoku
        {
            get
            {
                return _sudoku;
            }
            set
            {
                _sudoku = value;
                OnPropertyChanged(nameof(_sudoku));
                ExecuteSolveCommand.ChangeCanExecute();
            }
        }
        /// <summary>
        /// カメラ起動コマンド
        /// </summary>
        public Command ExecuteCameraCommand { get; private set; }
        /// <summary>
        /// 数独解答コマンド
        /// </summary>
        public Command ExecuteSolveCommand { get; private set; }
        #endregion

        #region コンストラクタ
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainViewModel()
        {
            ExecuteCameraCommand = new Command(ExecuteCamera, CanExecuteCamera);
            ExecuteSolveCommand = new Command(ExecuteSolve, CanExecuteSolve);
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
        #endregion
    }
}
