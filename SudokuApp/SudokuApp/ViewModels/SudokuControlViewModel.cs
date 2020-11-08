using Plugin.Toasts;
using SudokuApp.Common;
using SudokuApp.Debug;
using SudokuApp.Models;
using System.Collections.Generic;
using System.IO;
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
        /// 画面に表示する盤面
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
        /// 画面に表示する盤面
        /// </summary>
        private BindableTwoDArray<int?> _displayBoard = new BindableTwoDArray<int?>(9, 9);
        #endregion

        #region サンプル
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
                // 盤面を数独オブジェクトにコピーする
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

                int[,] ret = await ExecuteAsync();
                if (ret == null)
                {
                    var notificator = DependencyService.Get<IToastNotificator>();
                    if (notificator != null)
                    {
                        var androidOptions = new AndroidOptions
                        {
                            DismissText = ""
                        };
                        var options = new NotificationOptions()
                        {
                            Title = "答えが見つかりませんでした…",
                            AndroidOptions = androidOptions,
                        };
                        await notificator.Notify(options);
                    }
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
