using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SudokuApp.Common
{
    /// <summary>
    /// Computer Visionアクセスクラス
    /// </summary>
    public static class ComputerVisionManager
    {
        #region 定数
        /// <summary>
        /// サブスクリプションキー
        /// </summary>
        private const string SubscriptionKey = "36d6a66a0dc84769a058c5b5a3b2a680";
        /// <summary>
        /// エンドポイント
        /// </summary>
        private const string EndPoint = "https://uchi-sudokuapp-cv.cognitiveservices.azure.com/";
        #endregion

        #region メンバ変数
        /// <summary>
        /// ComputerVisionClientオブジェクト
        /// </summary>
        private static ComputerVisionClient _client;
        #endregion

        #region メソッド
        /// <summary>
        /// ローカルファイルをComputerVisionで読み取る
        /// </summary>
        /// <param name="stream">画像ストリーム</param>
        /// <param name="language">言語（デフォルト：英語）</param>
        /// <returns></returns>
        public static async Task<IList<ReadResult>> ReadFileLocal(Stream stream, string language = "en")
        {
            // 認証が通っていなければ認証する
            if (_client == null)
            {
                Authenticate();
            }

            var textHeaders = await _client.ReadInStreamAsync(stream, language: language);
            string operationLocation = textHeaders.OperationLocation;
            Thread.Sleep(2000);

            const int numberOfCharsInOperationId = 36;
            string operationId = operationLocation.Substring(operationLocation.Length - numberOfCharsInOperationId);

            ReadOperationResult results;
            do
            {
                results = await _client.GetReadResultAsync(Guid.Parse(operationId));
            }
            while (results.Status == OperationStatusCodes.Running || results.Status == OperationStatusCodes.NotStarted);

            return results.AnalyzeResult.ReadResults;
        }

        /// <summary>
        /// Computer Visionのクライアントを作成する
        /// </summary>
        private static void Authenticate()
        {
            _client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(SubscriptionKey))
            {
                Endpoint = EndPoint
            };
        }
        #endregion
    }
}
