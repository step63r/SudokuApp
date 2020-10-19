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
            var mySudoku = new Sudoku();
            string sample = $@"*****3***
****654**
*8****7**
*94******
*******5*
*1*9*7*3*
5*6******
***8**9**
*********";
            using (var reader = new StringReader(sample))
            {
                string line = "";
                var list = new List<string>();

                while ((line = reader.ReadLine()) != null)
                {
                    list.Add(line);
                }

                for (int x = 0; x < 9; ++x)
                {
                    line = list[x];
                    for (int y = 0; y < 9; ++y)
                    {
                        // 空マスの場合は何もしない
                        if (line.Substring(y, 1) == "*")
                        {
                            continue;
                        }
                        mySudoku.Put(x, y, int.Parse(line.Substring(y, 1)));
                    }
                }

                // 数独を解く
                var results = new List<int[,]>();
                DepthFirstSearch(ref mySudoku, ref results);

                // デバッグ出力
                if (results.Count > 0)
                {
                    DebugWriteSudokuFormat(results);
                }
            }
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
        /// 深さ優先探索で数独を解答する
        /// </summary>
        /// <param name="board"></param>
        /// <param name="results"></param>
        private void DepthFirstSearch(ref Sudoku board, ref List<int[,]> results)
        {
            // 数独の盤面状態を保持しておく
            var boardPrev = board.DeepCopy();

            // 一意に自動的に決まるマスを埋める
            board.Process();

            // 終端条件を処理し、同時に空きマスを探す
            if (!board.FindEmpty(out int x, out int y))
            {
                // 解に追加
                results.Add(board.Get());

                // リターンする前に一回元に戻す
                board = boardPrev.DeepCopy();
                return;
            }

            // マス (x, y) に入れられる数値の集合を求める
            var canUse = board.FindChoices(x, y);

            // バックトラッキング
            foreach (int val in canUse)
            {
                board.Put(x, y, val);
                DepthFirstSearch(ref board, ref results);
                board.Reset(x, y);
                board = boardPrev.DeepCopy();
            }

            // 元に戻す
            board = boardPrev.DeepCopy();
        }
        #endregion

        /// <summary>
        /// [テスト用] デバッグに結果をフォーマットして出力する
        /// </summary>
        /// <param name="src"></param>
        private void DebugWriteSudokuFormat(List<int[,]> src)
        {
            int count = 1;
            foreach (var one_ret in src)
            {
                Debug.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>");
                Debug.WriteLine($"Result: No. {count}");
                Debug.WriteLine("    0 1 2   3 4 5   6 7 8  ");
                Debug.WriteLine("  -------------------------");
                for (int x = 0; x < 9; ++x)
                {
                    Debug.Write($"{x} | ");
                    for (int y = 0; y < 9; ++y)
                    {
                        if (one_ret[x, y] == -1)
                        {
                            Debug.Write("* ");
                        }
                        else
                        {
                            Debug.Write($"{one_ret[x, y]} ");
                        }
                        if (y % 3 == 2)
                        {
                            Debug.Write("| ");
                        }
                    }
                    Debug.WriteLine("");
                    if (x % 3 == 2)
                    {
                        Debug.WriteLine("  -------------------------");
                    }
                }
                Debug.WriteLine("<<<<<<<<<<<<<<<<<<<<<<<<<<<");
                count++;
            }
        }
    }
}
