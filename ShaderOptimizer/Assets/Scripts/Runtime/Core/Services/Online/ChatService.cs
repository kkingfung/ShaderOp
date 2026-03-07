#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using ShaderOp.Online.Models;
using ShaderOp.Online.Services;
using UnityEngine;

namespace ShaderOp.Online.Services
{
    /// <summary>
    /// チャットサービス実装
    /// </summary>
    /// <remarks>
    /// Photon Eventsを使用してリアルタイムチャットを実現します。
    /// メッセージはローカルにキャッシュされ、履歴表示に使用されます。
    /// </remarks>
    public class ChatService : IChatService
    {
        // Photonイベントコード
        private const byte EVENT_CHAT_MESSAGE = 1;
        private const byte EVENT_CHAT_STAMP = 2;

        private readonly INetworkService _networkService;
        private readonly List<ChatMessage> _messageCache = new();
        private readonly HashSet<string> _blockedUsers = new();
        private readonly int _maxCacheSize = 100;

        // イベント
        public event Action<ChatMessage>? OnMessageReceived;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="networkService">ネットワークサービス</param>
        public ChatService(INetworkService networkService)
        {
            _networkService = networkService ?? throw new ArgumentNullException(nameof(networkService));

            // Photonイベントをリスニング
            _networkService.OnEventReceived += OnPhotonEventReceived;
        }

        /// <summary>
        /// テキストメッセージを送信します
        /// </summary>
        public async UniTask<bool> SendMessageAsync(string text)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    Debug.LogWarning("[ChatService] 空のメッセージは送信できません。");
                    return false;
                }

                if (!_networkService.IsInRoom)
                {
                    Debug.LogWarning("[ChatService] ルームに参加していません。");
                    return false;
                }

                var message = new ChatMessage
                {
                    MessageId = Guid.NewGuid().ToString(),
                    SenderId = _networkService.LocalPlayerId ?? "Unknown",
                    SenderName = _networkService.LocalPlayerId ?? "Unknown",
                    MessageType = MessageType.Text,
                    Content = text,
                    Timestamp = DateTime.UtcNow.ToString("o")
                };

                // Photonイベントで送信
                _networkService.SendEvent(EVENT_CHAT_MESSAGE, message);

                // ローカルキャッシュに追加
                AddMessageToCache(message);

                Debug.Log($"[ChatService] メッセージ送信: {text}");
                await UniTask.Yield();
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ChatService] メッセージ送信エラー: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// スタンプを送信します
        /// </summary>
        public async UniTask<bool> SendStampAsync(int stampId)
        {
            try
            {
                if (!_networkService.IsInRoom)
                {
                    Debug.LogWarning("[ChatService] ルームに参加していません。");
                    return false;
                }

                var message = new ChatMessage
                {
                    MessageId = Guid.NewGuid().ToString(),
                    SenderId = _networkService.LocalPlayerId ?? "Unknown",
                    SenderName = _networkService.LocalPlayerId ?? "Unknown",
                    MessageType = MessageType.Stamp,
                    Content = $"[Stamp:{stampId}]",
                    Timestamp = DateTime.UtcNow.ToString("o")
                };

                // Photonイベントで送信
                _networkService.SendEvent(EVENT_CHAT_STAMP, message);

                // ローカルキャッシュに追加
                AddMessageToCache(message);

                Debug.Log($"[ChatService] スタンプ送信: ID={stampId}");
                await UniTask.Yield();
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ChatService] スタンプ送信エラー: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 最近のメッセージを取得します
        /// </summary>
        public List<ChatMessage> GetRecentMessages(int count)
        {
            return _messageCache
                .OrderByDescending(m => m.Timestamp)
                .Take(count)
                .OrderBy(m => m.Timestamp)
                .ToList();
        }

        /// <summary>
        /// すべてのメッセージをクリアします
        /// </summary>
        public void ClearMessages()
        {
            _messageCache.Clear();
            Debug.Log("[ChatService] メッセージキャッシュをクリアしました。");
        }

        /// <summary>
        /// ユーザーをブロックします
        /// </summary>
        public async UniTask BlockUserAsync(string userId)
        {
            try
            {
                if (_blockedUsers.Add(userId))
                {
                    Debug.Log($"[ChatService] ユーザーをブロック: {userId}");

                    // TODO: REST APIでサーバーに通知
                    await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ChatService] ブロックエラー: {ex.Message}");
            }
        }

        /// <summary>
        /// ユーザーのブロックを解除します
        /// </summary>
        public async UniTask UnblockUserAsync(string userId)
        {
            try
            {
                if (_blockedUsers.Remove(userId))
                {
                    Debug.Log($"[ChatService] ブロック解除: {userId}");

                    // TODO: REST APIでサーバーに通知
                    await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ChatService] ブロック解除エラー: {ex.Message}");
            }
        }

        /// <summary>
        /// ユーザーがブロックされているか確認します
        /// </summary>
        public bool IsUserBlocked(string userId)
        {
            return _blockedUsers.Contains(userId);
        }

        /// <summary>
        /// Photonイベント受信ハンドラ
        /// </summary>
        private void OnPhotonEventReceived(byte eventCode, object data)
        {
            if (data is not ChatMessage message)
            {
                return;
            }

            // ブロックされたユーザーからのメッセージは無視
            if (IsUserBlocked(message.SenderId))
            {
                Debug.Log($"[ChatService] ブロックされたユーザーからのメッセージを無視: {message.SenderId}");
                return;
            }

            // キャッシュに追加
            AddMessageToCache(message);

            // イベント発火
            OnMessageReceived?.Invoke(message);

            Debug.Log($"[ChatService] メッセージ受信: {message.SenderName}: {message.Content}");
        }

        /// <summary>
        /// メッセージをキャッシュに追加します
        /// </summary>
        private void AddMessageToCache(ChatMessage message)
        {
            _messageCache.Add(message);

            // キャッシュサイズ制限
            if (_messageCache.Count > _maxCacheSize)
            {
                _messageCache.RemoveAt(0);
            }
        }
    }
}
