using System;
using System.ComponentModel;

namespace SudokuApp.Common
{
    public class BindableTwoDArray<T> : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        /// <summary>
        /// PropertyChangedイベントハンドラ
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region プロパティ
        /// <summary>
        /// データオブジェクト（数値型インデクサ）
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public T this[int x, int y]
        {
            get
            {
                return _data[x, y];
            }
            set
            {
                _data[x, y] = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BindableTwoDArray<T>)));
            }
        }
        /// <summary>
        /// データオブジェクト（文字列型インデクサ）
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T this[string index]
        {
            get
            {
                SplitIndex(index, out int x, out int y);
                return _data[x, y];
            }
            set
            {
                SplitIndex(index, out int x, out int y);
                _data[x, y] = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BindableTwoDArray<T>)));
            }
        }
        #endregion

        #region メンバ変数
        /// <summary>
        /// データオブジェクト
        /// </summary>
        private T[,] _data;
        #endregion

        #region コンストラクタ
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="size1">Xの要素数</param>
        /// <param name="size2">Yの要素数</param>
        public BindableTwoDArray(int size1, int size2)
        {
            _data = new T[size1, size2];
        }
        #endregion

        /// <summary>
        /// 数値型の要素番号を文字列に変換する
        /// </summary>
        /// <param name="x">Xインデックス</param>
        /// <param name="y">Yインデックス</param>
        /// <returns></returns>
        public string GetStringIndex(int x, int y)
        {
            return x.ToString() + "-" + y.ToString();
        }

        #region メソッド
        /// <summary>
        /// 文字列の要素番号を数値型に変換する
        /// </summary>
        /// <param name="index">要素番号の文字列</param>
        /// <param name="x">Xインデックス</param>
        /// <param name="y">Yインデックス</param>
        private void SplitIndex(string index, out int x, out int y)
        {
            string[] parts = index.Split('-');
            if (parts.Length != 2)
            {
                throw new ArgumentException("The provided index is not valid");
            }
            x = int.Parse(parts[0]);
            y = int.Parse(parts[1]);
        }
        #endregion

        #region オペレータ
        /// <summary>
        /// T[,]オペレータ
        /// </summary>
        /// <param name="a"></param>
        public static implicit operator T[,](BindableTwoDArray<T> a)
        {
            return a._data;
        }
        #endregion
    }
}
