using SudokuApp.Common;
using SudokuApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SudokuApp.ViewModels
{
    /// <summary>
    /// SudokuControlのViewModelクラス
    /// </summary>
    public class SudokuControlViewModel : ViewModelBase
    {
        #region プロパティ
        /// <summary>
        /// 数独オブジェクト
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
            }
        }
        #endregion

        #region メンバ変数
        /// <summary>
        /// 数独オブジェクト
        /// </summary>
        private Sudoku _sudoku;
        #endregion

        #region コンストラクタ
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SudokuControlViewModel()
        {
            Sudoku = new Sudoku();
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
                        Sudoku.Put(x, y, int.Parse(line.Substring(y, 1)));
                    }
                }
            }

            // メッセージの購読
            MessagingCenter.Subscribe<MainViewModel>(this, "ExecuteAsync", (sender) =>
            {
                Execute();
                //ExecuteAsync();
            });
        }
        #endregion

        #region デストラクタ
        /// <summary>
        /// デストラクタ
        /// </summary>
        ~SudokuControlViewModel()
        {
            // メッセージの購読解除
            MessagingCenter.Unsubscribe<MainViewModel>(this, "ExecuteAsync");
        }
        #endregion

        #region メソッド
        /// <summary>
        /// 数独の解答を非同期で実行する
        /// </summary>
        /// <returns></returns>
        public async Task ExecuteAsync()
        {
            await Task.Run(() =>
            {
                var sudoku = Sudoku.DeepCopy();
                var results = new List<int[,]>();
                DepthFirstSearch(ref sudoku, ref results);
                Sudoku = sudoku.DeepCopy();
            });
        }

        public void Execute()
        {
            var sudoku = Sudoku.DeepCopy();
            var results = new List<int[,]>();
            DepthFirstSearch(ref sudoku, ref results);
            Sudoku = sudoku.DeepCopy();
        }

        /// <summary>
        /// 深さ優先探索で数独を解答する
        /// </summary>
        /// <param name="board">数独オブジェクト</param>
        /// <param name="results">解答リスト</param>
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
                results.Add(board.Field);

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
    }
}
