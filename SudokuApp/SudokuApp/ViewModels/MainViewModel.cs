using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Newtonsoft.Json;
using Plugin.Media;
using Plugin.Media.Abstractions;
using SudokuApp.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
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
        /// <summary>
        /// 非同期処理が実行中
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return _isRunning;
            }
            set
            {
                if (_isRunning != value)
                {
                    _isRunning = value;
                    OnPropertyChanged(nameof(IsRunning));
                }
            }
        }
        /// <summary>
        /// 実行中ステータス
        /// </summary>
        public string RunningStatus
        {
            get
            {
                return _runningStatus;
            }
            set
            {
                if (_runningStatus != value)
                {
                    _runningStatus = value;
                    OnPropertyChanged(nameof(RunningStatus));
                }
            }
        }
        #endregion

        #region メンバ変数
        /// <summary>
        /// 非同期処理が実行中
        /// </summary>
        private bool _isRunning = false;
        /// <summary>
        /// 実行中ステータス
        /// </summary>
        private string _runningStatus = "";
        /// <summary>
        /// Azure FunctionsのURL
        /// </summary>
        private static readonly string _functionsUrl = "https://uchi-sudokuapp-functions.azurewebsites.net/api/DetectSudokuFrame?code=nZy9VUADNqLCzR/zxzxKfQQHrego94baN0fzbH6U82Uxp4pqyfGkzQ==";
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
        public async void ExecuteCamera()
        {
            var mediaOptions = new StoreCameraMediaOptions
            {
                AllowCropping = false,
                DefaultCamera = CameraDevice.Rear,
                Directory = "SudokuPicture",
                Name = $"{DateTime.UtcNow.ToLongDateString()}.jpg"
            };
            var file = await CrossMedia.Current.TakePhotoAsync(mediaOptions);
            if (file != null)
            {
                await AnalyzePicture(file.GetStream());
            }
        }
        /// <summary>
        /// カメラ起動が実行可能か
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteCamera()
        {
            return CrossMedia.Current.IsTakePhotoSupported;
        }

        /// <summary>
        /// 数独の解答を実行する
        /// </summary>
        public void ExecuteSolve()
        {
            // TODO: 
            // MessagingCenterに戻り値っぽいものが実現すれば
            // ここでIsRunningを使える
            // 今は、全マスが空のまま実行すると戻ってこれない…
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
                await AnalyzePicture(stream);
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

        /// <summary>
        /// 画像ストリームを読み込んで数字と枠の検出を行う
        /// </summary>
        /// <param name="stream">ファイルストリーム</param>
        /// <returns></returns>
        private async Task AnalyzePicture(Stream stream)
        {
            if (stream != null)
            {
                IsRunning = true;
                RunningStatus = "初期化中";

                // SudokuControlViewModelに渡すオブジェクト
                var sudokuArray = new BindableTwoDArray<int?>(9, 9);

                // 画像をバイト配列に変換
                // 注意：ComputerVisionManager.ReadFileLocalを呼び出すとStreamが閉じてしまうため、それより先に行う
                byte[] byteStreamArray;
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    byteStreamArray = memoryStream.ToArray();
                }

                // ストリームの位置を戻す
                stream.Seek(0, SeekOrigin.Begin);

                RunningStatus = "文字列認識中（Custom Vision）";
                // Computer Visionによる文字認識結果
                var cvResult = await ComputerVisionManager.ReadFileLocal(stream);
                if (cvResult == null || cvResult.Count == 0)
                {
                    await Application.Current.MainPage.DisplayAlert("ERROR", "文字列の認識に失敗しました", "OK");
                    IsRunning = false;
                    return;
                }

                var detectedNumbers = GetDetectedNumbers(cvResult[0]);

                RunningStatus = "フレーム認識中（Azure Functions）";
                // Azure Functionsによるフレーム認識結果
                if (byteStreamArray != null)
                {
                    var requestContent = new ByteArrayContent(byteStreamArray);
                    HttpResponseMessage response;
                    using (var client = new HttpClient())
                    {
                        response = await client.PostAsync(_functionsUrl, requestContent);
                    }

                    if (response != null)
                    {
                        string strResponseBody = await response.Content.ReadAsStringAsync();
                        var detectedFrames = JsonConvert.DeserializeObject<List<List<List<int>>>>(strResponseBody);

                        if (detectedFrames.Count != 81)
                        {
                            await Application.Current.MainPage.DisplayAlert("ERROR", "フレームが正しく検出できませんでした", "OK");
                            IsRunning = false;
                            return;
                        }

                        // 検出された数値がフレームに収まっているかを確認
                        foreach (var detectedNumber in detectedNumbers)
                        {
                            // 各フレームでループ
                            int frameIndex = 0;
                            foreach (var frame in detectedFrames)
                            {
                                var convertedFrame = new List<Point>();
                                foreach (var vertex in frame)
                                {
                                    convertedFrame.Add(new Point(vertex[0], vertex[1]));
                                }

                                // そのフレームに対し検出された数値を囲む多角形の
                                // 頂点が全て内側と判定されればそのフレームの数値とみなす
                                int numPointInPolygon = 0;
                                foreach (var vertex in detectedNumber.Points)
                                {
                                    if (PointInPolygon(vertex, convertedFrame))
                                    {
                                        numPointInPolygon++;
                                    }
                                }
                                if (numPointInPolygon >= detectedNumber.Points.Count / 2)
                                {
                                    var (row, col) = GetRowColIndex(frameIndex);
                                    sudokuArray[row, col] = detectedNumber.Number;
                                }
                                frameIndex++;
                            }
                        }
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("ERROR", "フレーム検出のリクエストに失敗しました", "OK");
                        IsRunning = false;
                        return;
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("ERROR", "画像の読み込みに失敗しました", "OK");
                    IsRunning = false;
                    return;
                }

                MessagingCenter.Send(this, "ExecutePickPhoto", sudokuArray);
                IsRunning = false;
            }
        }

        /// <summary>
        /// 検出された数字とその位置を取得する
        /// </summary>
        /// <param name="src">Computer Visionの検出結果</param>
        /// <returns></returns>
        private List<DetectedNumber> GetDetectedNumbers(ReadResult src)
        {
            var detectedNumbers = new List<DetectedNumber>();
            foreach (var line in src.Lines)
            {
                var points = new List<Point>();
                if (int.TryParse(line.Text, out int num))
                {
                    if (num < 1 || num > 9)
                    {
                        // 数独で使う数字として認識できなかったのですっ飛ばす
                        continue;
                    }
                    for (int i = 0; i < line.BoundingBox.Count; i += 2)
                    {
                        points.Add(new Point((double)line.BoundingBox[i], (double)line.BoundingBox[i + 1]));
                    }
                    detectedNumbers.Add(new DetectedNumber
                    {
                        Number = num,
                        Points = points
                    });
                }
                else
                {
                    // 数字として認識できなかったのですっ飛ばす
                    continue;
                }
            }
            return detectedNumbers;
        }

        /// <summary>
        /// 多角形に点が含まれるかどうか判定する
        /// </summary>
        /// <param name="p">点</param>
        /// <param name="poly">多角形を構成する点</param>
        /// <returns>含まれる場合 true</returns>
        /// <remarks>(参考) http://home.a00.itscom.net/hatada/c01/algorithm/polygon01.html</remarks>
        private static bool PointInPolygon(Point p, List<Point> poly)
        {
            Point p1, p2;
            bool inside = false;
            Point oldPoint = poly[poly.Count - 1];
            for (int i = 0; i < poly.Count; i++)
            {
                Point newPoint = poly[i];
                if (newPoint.X > oldPoint.X)
                {
                    p1 = oldPoint; p2 = newPoint;
                }
                else
                {
                    p1 = newPoint; p2 = oldPoint;
                }
                if ((p1.X < p.X) == (p.X <= p2.X) && (p.Y - p1.Y) * (p2.X - p1.X) < (p2.Y - p1.Y) * (p.X - p1.X))
                {
                    inside = !inside;
                }
                oldPoint = newPoint;
            }
            return inside;
        }

        /// <summary>
        /// リストの要素番号から数独盤面の何行目、何列目を取得する
        /// </summary>
        /// <param name="index">要素番号</param>
        /// <param name="colCount">列数（既定値：9）</param>
        /// <returns>(行番号, 列番号) のタプル</returns>
        private (int Row, int Column) GetRowColIndex(int index, int colCount = 9)
        {
            return (index / colCount, index % colCount);
        }
        #endregion
    }
}
