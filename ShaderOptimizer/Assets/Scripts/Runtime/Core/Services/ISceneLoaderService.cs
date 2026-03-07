#nullable enable

using System;
using Cysharp.Threading.Tasks;

namespace ShaderOp.Core.Services
{
    /// <summary>
    /// シーン読み込みサービスインターフェース
    /// </summary>
    /// <remarks>
    /// シーン遷移を管理します。
    /// 現在のSceneLoader.csをサービスパターンに移行します。
    /// </remarks>
    public interface ISceneLoaderService
    {
        /// <summary>現在ロード中か</summary>
        bool IsLoading { get; }

        /// <summary>ロード進捗（0.0 ~ 1.0）</summary>
        float LoadProgress { get; }

        /// <summary>シーンロード開始イベント</summary>
        event Action<string>? OnSceneLoadStarted;

        /// <summary>シーンロード完了イベント</summary>
        event Action<string>? OnSceneLoadCompleted;

        /// <summary>
        /// シーンを非同期でロード
        /// </summary>
        /// <param name="sceneName">シーン名</param>
        UniTask LoadSceneAsync(string sceneName);

        /// <summary>
        /// メインメニューをロード
        /// </summary>
        UniTask LoadMainMenuAsync();

        /// <summary>
        /// キャラクターカスタマイズをロード
        /// </summary>
        UniTask LoadCharacterCustomizationAsync();

        /// <summary>
        /// 部屋デコレーションをロード
        /// </summary>
        UniTask LoadRoomDecorationAsync();

        /// <summary>
        /// ミニゲームをロード
        /// </summary>
        /// <param name="gameName">ゲーム名</param>
        UniTask LoadMinigameAsync(string gameName);
    }
}
