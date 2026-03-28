using Cysharp.Threading.Tasks;
using System;

#nullable enable

namespace ShaderOp.Core.Services
{
    /// <summary>
    /// ネットワーク接続管理サービスインターフェース
    /// Photon PUN 2を使用したリアルタイムマルチプレイヤー接続を提供
    /// </summary>
    /// <remarks>
    /// Phase 5 Week 1で実装。
    /// UniTaskを使用した非同期接続管理により、UI blocking無しで
    /// サーバー接続・ルーム作成・参加を実現。
    /// </remarks>
    public interface INetworkService
    {
        /// <summary>Photonマスターサーバーに接続済みか</summary>
        bool IsConnected { get; }

        /// <summary>現在ルームに参加しているか</summary>
        bool IsInRoom { get; }

        /// <summary>現在のルーム名（未参加時はnull）</summary>
        string? CurrentRoomName { get; }

        /// <summary>現在のルーム内プレイヤー数</summary>
        int PlayerCount { get; }

        /// <summary>ローカルプレイヤーのPhoton Actor ID</summary>
        int LocalPlayerId { get; }

        /// <summary>ローカルプレイヤーがルームマスターか</summary>
        bool IsMasterClient { get; }

        /// <summary>
        /// Photonマスターサーバーに接続
        /// </summary>
        /// <returns>接続成功でtrue、失敗でfalse</returns>
        UniTask<bool> ConnectToServerAsync();

        /// <summary>
        /// 新規ルームを作成して参加
        /// </summary>
        /// <param name="roomName">ルーム名（ユニークである必要あり）</param>
        /// <param name="maxPlayers">最大プレイヤー数（デフォルト2）</param>
        /// <returns>作成成功でtrue、失敗でfalse</returns>
        UniTask<bool> CreateRoomAsync(string roomName, int maxPlayers = 2);

        /// <summary>
        /// 指定コードのルームに参加（Unity Multiplayer Servicesでは6桁Join Code使用）
        /// </summary>
        /// <param name="joinCode">参加するルームのJoin Code（6桁数字、例: "123456"）</param>
        /// <returns>参加成功でtrue、失敗でfalse</returns>
        /// <remarks>
        /// Unity Multiplayer Servicesでは、ルーム名ではなく6桁のJoin Codeで参加します。
        /// CreateRoomWithCodeAsync()で生成されたJoin Codeを使用してください。
        /// </remarks>
        UniTask<bool> JoinRoomAsync(string joinCode);

        /// <summary>
        /// ランダムなルームに参加（マッチメイキング）
        /// </summary>
        /// <returns>参加成功でtrue、失敗でfalse</returns>
        UniTask<bool> JoinRandomRoomAsync();

        /// <summary>
        /// 現在のルームから退出
        /// </summary>
        UniTask LeaveRoomAsync();

        /// <summary>
        /// Photonサーバーから切断
        /// </summary>
        UniTask DisconnectAsync();

        /// <summary>接続状態変更時イベント（true=接続、false=切断）</summary>
        event Action<bool>? OnConnectedChanged;

        /// <summary>ルーム参加成功時イベント（ルーム名を通知）</summary>
        event Action<string>? OnRoomJoined;

        /// <summary>ルーム退出時イベント</summary>
        event Action? OnRoomLeft;

        /// <summary>他プレイヤーがルームに参加した時（Actor IDを通知）</summary>
        event Action<int>? OnPlayerJoined;

        /// <summary>他プレイヤーがルームから退出した時（Actor IDを通知）</summary>
        event Action<int>? OnPlayerLeft;
    }
}
