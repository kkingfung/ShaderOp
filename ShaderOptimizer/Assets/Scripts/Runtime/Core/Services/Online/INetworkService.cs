#nullable enable

using System;
using Cysharp.Threading.Tasks;

namespace ShaderOp.Online.Services
{
    /// <summary>
    /// ネットワークサービスインターフェース
    /// </summary>
    /// <remarks>
    /// Photon PUN2を使用したネットワーク機能を提供します。
    /// 接続状態管理、ルーム作成/参加、マッチメイキングなどを担当します。
    /// </remarks>
    public interface INetworkService
    {
        /// <summary>接続状態</summary>
        bool IsConnected { get; }

        /// <summary>ルームに参加しているか</summary>
        bool IsInRoom { get; }

        /// <summary>プレイヤー名</summary>
        string PlayerName { get; }

        /// <summary>ローカルプレイヤーID</summary>
        string? LocalPlayerId { get; }

        /// <summary>現在のルーム名</summary>
        string? CurrentRoomName { get; }

        /// <summary>接続状態変更イベント</summary>
        event Action<bool>? OnConnectionStatusChanged;

        /// <summary>ルーム参加イベント</summary>
        event Action<string>? OnRoomJoined;

        /// <summary>ルーム退出イベント</summary>
        event Action? OnRoomLeft;

        /// <summary>カスタムイベント受信イベント</summary>
        event Action<byte, object>? OnEventReceived;

        /// <summary>マスターサーバー接続完了イベント</summary>
        event Action? OnConnectedToMaster;

        /// <summary>接続失敗イベント</summary>
        event Action<string>? OnConnectionFailed;

        /// <summary>ルーム参加完了イベント (OnRoomJoinedのエイリアス)</summary>
        event Action<string>? OnJoinedRoom;

        /// <summary>ルーム参加失敗イベント</summary>
        event Action<string>? OnJoinRoomFailed;

        /// <summary>ルーム退出完了イベント (OnRoomLeftのエイリアス)</summary>
        event Action? OnLeftRoom;

        /// <summary>現在のルーム内プレイヤー数</summary>
        int CurrentPlayerCount { get; }

        /// <summary>マスタークライアントか</summary>
        bool IsMasterClient { get; }

        /// <summary>
        /// Photonサーバーに接続
        /// </summary>
        UniTask<bool> ConnectAsync();

        /// <summary>
        /// Photonサーバーから切断
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Photonサーバーから切断（非同期版）
        /// </summary>
        UniTask DisconnectAsync();

        /// <summary>
        /// ルームを作成
        /// </summary>
        /// <param name="roomName">ルーム名</param>
        /// <param name="maxPlayers">最大プレイヤー数</param>
        UniTask<bool> CreateRoomAsync(string roomName, int maxPlayers = 2);

        /// <summary>
        /// ルームに参加
        /// </summary>
        /// <param name="roomName">ルーム名</param>
        UniTask<bool> JoinRoomAsync(string roomName);

        /// <summary>
        /// ルームに参加、なければ作成
        /// </summary>
        /// <param name="roomName">ルーム名</param>
        /// <param name="maxPlayers">最大プレイヤー数</param>
        UniTask<bool> JoinOrCreateRoomAsync(string roomName, int maxPlayers = 2);

        /// <summary>
        /// ランダムなルームに参加
        /// </summary>
        UniTask<bool> JoinRandomRoomAsync();

        /// <summary>
        /// 現在のルームから退出
        /// </summary>
        void LeaveRoom();

        /// <summary>
        /// 現在のルームから退出（非同期版）
        /// </summary>
        UniTask LeaveRoomAsync();

        /// <summary>
        /// カスタムイベントを送信
        /// </summary>
        /// <param name="eventCode">イベントコード</param>
        /// <param name="data">送信データ</param>
        void SendCustomEvent(byte eventCode, object data);

        /// <summary>
        /// イベントを送信（SendCustomEventのエイリアス）
        /// </summary>
        /// <param name="eventCode">イベントコード</param>
        /// <param name="data">送信データ</param>
        void SendEvent(byte eventCode, object data);

        /// <summary>
        /// カスタムイベントハンドラを登録
        /// </summary>
        /// <param name="eventCode">イベントコード</param>
        /// <param name="callback">コールバック</param>
        void RegisterCustomEventHandler(byte eventCode, Action<object> callback);

        /// <summary>
        /// カスタムイベントハンドラを解除
        /// </summary>
        /// <param name="eventCode">イベントコード</param>
        void UnregisterCustomEventHandler(byte eventCode);
    }
}
