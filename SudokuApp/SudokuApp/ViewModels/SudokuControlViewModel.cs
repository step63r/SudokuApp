using SudokuApp.Common;
using SudokuApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
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
        /// <summary>
        /// 
        /// </summary>
        public BindableTwoDArray<int?> DisplayBoard
        {
            get
            {
                return _displayBoard;
            }
            set
            {
                _displayBoard = value;
                OnPropertyChanged(nameof(_displayBoard));
            }
        }
        #endregion

        #region メンバ変数
        /// <summary>
        /// 数独オブジェクト
        /// </summary>
        private Sudoku _sudoku = new Sudoku();
        /// <summary>
        /// 
        /// </summary>
        private BindableTwoDArray<int?> _displayBoard = new BindableTwoDArray<int?>(9, 9);
        #endregion

        #region サンプル
        /// <summary>
        /// サンプル問題（初級）
        /// </summary>
        private readonly string _easyString = $@"3***4****
**82*6*9*
****1***5
*5*674***
49*38*25*
**1**2*87
1*973*6*4
2*34*8519
56*1293*8";
        /// <summary>
        /// サンプル問題（中級）
        /// </summary>
        private readonly string _nornalString = $@"***4****9
*34*8****
7163****4
*73****4*
*****31**
642**73**
5********
****1*96*
***56**8*";
        /// <summary>
        /// サンプル問題（上級）
        /// </summary>
        private readonly string _hardString = $@"3*5*1**4*
*****7***
**8*****3
*4*6*17*2
6**4**1**
*3*******
9**2*6**7
*56*74**8
****3****";
        /// <summary>
        /// サンプル問題（最高級）
        /// </summary>
        private readonly string _extremeString = $@"*****6***
**3*59***
******2**
4********
27*4*****
*******69
**9*****5
***2**7**
31*******";

        /// <summary>
        /// Viewにバインドされているオブジェクトにサンプルの問題をセットする
        /// </summary>
        /// <param name="value">問題の文字列</param>
        private void SetAllValuesToDisplay(string value)
        {
            using (var reader = new StringReader(value))
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
                        if (line.Substring(y, 1) == "*")
                        {
                            continue;
                        }
                        DisplayBoard[x, y] = int.Parse(line.Substring(y, 1));
                    }
                }
            }
        }

        /// <summary>
        /// 数独オブジェクトの盤面を生成する
        /// </summary>
        /// <param name="values"></param>
        private void PutAllValuesFromString(BindableTwoDArray<int?> values)
        {
            for (int x = 0; x < 9; ++x)
            {
                
                for (int y = 0; y < 9; ++y)
                {
                    // 空マスの場合は何もしない
                    if (DisplayBoard[x, y] == null)
                    {
                        continue;
                    }
                    Sudoku.Put(x, y, (int)DisplayBoard[x, y]);
                }
            }
        }

        /// <summary>
        /// デバッグに結果をフォーマットして出力する
        /// </summary>
        /// <param name="src"></param>
        private void DebugWriteSudokuFormat(List<int[,]> src)
        {
            int count = 1;
            foreach (int[,] one_ret in src)
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
        #endregion

        #region コンストラクタ
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SudokuControlViewModel()
        {
            // メッセージの購読
            MessagingCenter.Subscribe<MainViewModel>(this, "ExecuteAsync", async (sender) =>
            {
                PutAllValuesFromString(DisplayBoard);
                int[,] ret = await ExecuteAsync();
                if (ret == null)
                {
                    Debug.WriteLine("答えが見つかりませんでした…");
                }
                else
                {
                    var board = new BindableTwoDArray<int?>(9, 9);
                    for (int x = 0; x < 9; ++x)
                    {
                        for (int y = 0; y < 9; ++y)
                        {
                            board[x, y] = ret[x, y];
                        }
                    }
                    DisplayBoard = board;
                    OnPropertyChanged(nameof(DisplayBoard));
                }
            });

            MessagingCenter.Subscribe<MainViewModel>(this, "ExecuteClear", (sender) =>
            {
                Sudoku = new Sudoku();
                DisplayBoard = new BindableTwoDArray<int?>(9, 9);
                OnPropertyChanged(nameof(DisplayBoard));
            });

            MessagingCenter.Subscribe<MainViewModel, BindableTwoDArray<int?>>(this, "ExecutePickPhoto", (sender, array) =>
            {
                Sudoku = new Sudoku();
                DisplayBoard = array;
                OnPropertyChanged(nameof(DisplayBoard));
            });

            //SetAllValuesToDisplay(_extremeString);
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
        public async Task<int[,]> ExecuteAsync()
        {
            var sudoku = Sudoku.DeepCopy();
            var results = new List<int[,]>();
            await Task.Run(() =>
            {   
                DepthFirstSearch(ref _sudoku, ref results);
            }).ConfigureAwait(false);
            if (results.Count == 0)
            {
                return default;
            }
            return results[0];
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
