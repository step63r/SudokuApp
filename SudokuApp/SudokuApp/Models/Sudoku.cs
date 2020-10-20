using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace SudokuApp.Models
{
    /// <summary>
    /// 数独の盤面情報クラス
    /// </summary>
    public class Sudoku : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        /// <summary>
        /// PropertyChangedイベントハンドラ
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region プロパティ
        /// <summary>
        /// 盤面
        /// </summary>
        public int[,] Field
        {
            get
            {
                return _field;
            }
            set
            {
                _field = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Field)));
            }
        }
        #endregion

        #region メンバ変数
        /// <summary>
        /// 盤面
        /// </summary>
        private int[,] _field = new int[9, 9];
        /// <summary>
        /// マス (x, y) の行・列・ブロックが持つ v+1 の数
        /// </summary>
        private int[,,] _nums = new int[9, 9, 9];
        /// <summary>
        /// マス (x, y) を埋めることができる数字
        /// </summary>
        private SortedSet<int>[,] _choices = new SortedSet<int>[9, 9];
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Sudoku()
        {
            // 初期化
            // もう少し賢い方法があると思うが、速度を重視するためゴリ押し
            for (int x = 0; x < 9; ++x)
            {
                for (int y = 0; y < 9; ++y)
                {
                    Field[x, y] = -1;
                    for (int i = 0; i < 9; ++i)
                    {
                        _nums[x, y, i] = 0;
                    }
                    _choices[x, y] = new SortedSet<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Sudoku DeepCopy()
        {
            var obj = (Sudoku)MemberwiseClone();
            obj.Field = new int[9, 9];
            obj._nums = new int[9, 9, 9];
            obj._choices = new SortedSet<int>[9, 9];
            for (int x = 0; x < 9; ++x)
            {
                for (int y = 0; y < 9; ++y)
                {
                    obj.Field[x, y] = Field[x, y];
                    for (int i = 0; i < 9; ++i)
                    {
                        obj._nums[x, y, i] = _nums[x, y, i];
                    }
                    obj._choices[x, y] = _choices[x, y];
                }
            }
            return obj;
        }

        ///// <summary>
        ///// 盤面を返す
        ///// </summary>
        ///// <returns></returns>
        //public int[,] Get()
        //{
        //    return _field;
        //}

        /// <summary>
        /// 空きマスのうち、選択肢が最も少ないマスを探す
        /// </summary>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        /// <returns>空きマスが見つかった場合true、それ以外の場合false</returns>
        public bool FindEmpty(out int x, out int y)
        {
            x = y = -1;
            int minNumChoices = 10;
            for (int i = 0; i < 9; ++i)
            {
                for (int j = 0; j < 9; ++j)
                {
                    if (Field[i, j] != -1)
                    {
                        continue;
                    }
                    if (minNumChoices > _choices[i, j].Count)
                    {
                        minNumChoices = _choices[i, j].Count;
                        x = i;
                        y = j;
                    }
                }
            }
            // 存在しない場合はfalse
            if (minNumChoices == 10)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// マス (x, y) に入れられる選択肢を探す
        /// </summary>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        /// <returns></returns>
        public SortedSet<int> FindChoices(int x, int y)
        {
            return _choices[x, y];
        }

        /// <summary>
        /// マス (x, y) に数値 val を入れることによるマス (x2, y2) への影響
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="val"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        private void PutDetail(int x, int y, int val, int x2, int y2)
        {
            if (x == x2 && y == y2)
            {
                return;
            }

            // (x2, y2) にすでに値が入っている場合は何もしない
            if (Field[x2, y2] != -1)
            {
                return;
            }

            // それまで (x2, y2) の影響範囲に値 val がなかった場合は選択肢から除く
            if (_nums[x2, y2, val - 1] == 0)
            {
                _choices[x2, y2].Remove(val);
            }

            // numsを更新
            ++_nums[x2, y2, val - 1];
        }

        /// <summary>
        /// マス (x, y) から数値 val を除去したことによるマス (x2, y2) への影響
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="val"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        private void ResetDetail(int x, int y, int val, int x2, int y2)
        {
            if (x == x2 && y == y2)
            {
                return;
            }

            // (x2, y2) にすでに値が入っている場合は何もしない
            if (Field[x2, y2] != -1)
            {
                return;
            }

            // numsを更新
            --_nums[x2, y2, val - 1];

            // numsが0になる場合は選択肢に復活する
            if (_nums[x2, y2, val - 1] == 0)
            {
                _choices[x2, y2].Add(val);
            }
        }

        /// <summary>
        /// マス (x, y) に数値 val を入れる
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="val"></param>
        public void Put(int x, int y, int val)
        {
            // 数値を入れる
            Field[x, y] = val;

            // マス (x, y) を含む行・列・ブロックへの影響を更新する
            for (int i = 0; i < 9; ++i)
            {
                PutDetail(x, y, val, x, i);
            }
            for (int i = 0; i < 9; ++i)
            {
                PutDetail(x, y, val, i, y);
            }
            int cx = x / 3 * 3 + 1;
            int cy = y / 3 * 3 + 1;
            for (int i = cx - 1; i <= cx + 1; ++i)
            {
                for (int j = cy - 1; j <= cy + 1; ++j)
                {
                    PutDetail(x, y, val, i, j);
                }
            }
        }

        /// <summary>
        /// マス (x, y) の数値を削除する
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void Reset(int x, int y)
        {
            // マス (x, y) を含む行・列・ブロックへの影響を更新する
            int val = Field[x, y];
            for (int i = 0; i < 9; ++i)
            {
                ResetDetail(x, y, val, x, i);
            }
            for (int i = 0; i < 9; ++i)
            {
                ResetDetail(x, y, val, i, y);
            }
            int cx = x / 3 * 3 + 1;
            int cy = y / 3 * 3 + 1;
            for (int i = cx - 1; i <= cx + 1; ++i)
            {
                for (int j = cy - 1; j <= cy + 1; ++j)
                {
                    ResetDetail(x, y, val, i, j);
                }
            }

            // 数値を除去する
            Field[x, y] = -1;
        }

        /// <summary>
        /// 一意に決まるマスを埋めていく
        /// </summary>
        public void Process()
        {
            // 数値 1, 2, ..., 9 について順に処理する
            for (int val = 1; val <= 9; ++val)
            {
                // x行目について
                for (int x = 0; x < 9; ++x)
                {
                    bool exist = false;
                    List<int> canEnter = new List<int>();
                    for (int y = 0; y < 9; ++y)
                    {
                        if (Field[x, y] == val)
                        {
                            exist = true;
                        }
                        if (Field[x, y] == -1 && _choices[x, y].Contains(val))
                        {
                            canEnter.Add(y);
                        }
                    }
                    // valを入れられるマス目がただ一つならば入れる
                    if (!exist && canEnter.Count == 1)
                    {
                        int y = canEnter[0];
                        Put(x, y, val);
                    }
                }

                // y列目について
                for (int y = 0; y < 9; ++y)
                {
                    bool exist = false;
                    List<int> canEnter = new List<int>();
                    for (int x = 0; x < 9; ++x)
                    {
                        if (Field[x, y] == val)
                        {
                            exist = true;
                        }
                        if (Field[x, y] == -1 && _choices[x, y].Contains(val))
                        {
                            canEnter.Add(x);
                        }
                    }
                    // valを入れられるマス目がただ一つならば入れる
                    if (!exist && canEnter.Count == 1)
                    {
                        int x = canEnter[0];
                        Put(x, y, val);
                    }
                }

                // 各ブロックについて
                for (int bx = 0; bx < 3; ++bx)
                {
                    for (int by = 0; by < 3; ++by)
                    {
                        bool exist = false;
                        List<(int, int)> canEnter = new List<(int, int)>();
                        for (int x = bx * 3; x < (bx + 1) * 3; ++x)
                        {
                            for (int y = by * 3; y < (by + 1) * 3; ++y)
                            {
                                if (Field[x, y] == val)
                                {
                                    exist = true;
                                }
                                if (Field[x, y] == -1 && _choices[x, y].Contains(val))
                                {
                                    canEnter.Add((x, y));
                                }
                            }
                        }
                        // valを入れられるマス目がただ一つならば入れる
                        if (!exist && canEnter.Count == 1)
                        {
                            int x = canEnter[0].Item1;
                            int y = canEnter[0].Item2;
                            Put(x, y, val);
                        }
                    }
                }
            }
        }
    }
}
