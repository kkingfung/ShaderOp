#nullable enable

using System.Collections.Generic;
using UnityEngine;

namespace ShaderOp.Runtime.Core.Services.Online
{
    /// <summary>
    /// PlayerId ↔ GameId の双方向マッピングを管理する実装
    /// </summary>
    public class PlayerIdService : MonoBehaviour, IPlayerIdService
    {
        /// <summary>PlayerId（文字列） → GameId（整数）のマッピング</summary>
        private readonly Dictionary<string, int> _playerIdToGameId = new();

        /// <summary>GameId（整数） → PlayerId（文字列）のマッピング</summary>
        private readonly Dictionary<int, string> _gameIdToPlayerId = new();

        /// <summary>ローカルプレイヤーのPlayerId</summary>
        private string? _localPlayerId;

        /// <summary>
        /// ローカルプレイヤーのGameId
        /// </summary>
        public int LocalGameId
        {
            get
            {
                if (_localPlayerId == null)
                {
                    Debug.LogWarning("[PlayerIdService] Local player not registered yet.");
                    return -1;
                }

                return GetGameId(_localPlayerId);
            }
        }

        /// <summary>
        /// PlayerIdからGameIdを取得
        /// </summary>
        public int GetGameId(string playerId)
        {
            if (_playerIdToGameId.TryGetValue(playerId, out int gameId))
            {
                return gameId;
            }

            Debug.LogWarning($"[PlayerIdService] PlayerId not found: {playerId}");
            return -1;
        }

        /// <summary>
        /// GameIdからPlayerIdを取得
        /// </summary>
        public string? GetPlayerId(int gameId)
        {
            if (_gameIdToPlayerId.TryGetValue(gameId, out string? playerId))
            {
                return playerId;
            }

            Debug.LogWarning($"[PlayerIdService] GameId not found: {gameId}");
            return null;
        }

        /// <summary>
        /// ローカルプレイヤーを登録
        /// </summary>
        public void RegisterLocalPlayer(string playerId, int gameId)
        {
            _localPlayerId = playerId;
            RegisterPlayer(playerId, gameId);
            Debug.Log($"[PlayerIdService] Local player registered: PlayerId={playerId}, GameId={gameId}");
        }

        /// <summary>
        /// プレイヤーを登録（双方向マッピング）
        /// </summary>
        public void RegisterPlayer(string playerId, int gameId)
        {
            // 既存のマッピングをクリア（再登録の場合）
            if (_playerIdToGameId.ContainsKey(playerId))
            {
                int oldGameId = _playerIdToGameId[playerId];
                _gameIdToPlayerId.Remove(oldGameId);
            }

            if (_gameIdToPlayerId.ContainsKey(gameId))
            {
                string? oldPlayerId = _gameIdToPlayerId[gameId];
                if (oldPlayerId != null)
                {
                    _playerIdToGameId.Remove(oldPlayerId);
                }
            }

            // 双方向マッピングを登録
            _playerIdToGameId[playerId] = gameId;
            _gameIdToPlayerId[gameId] = playerId;

            Debug.Log($"[PlayerIdService] Player registered: PlayerId={playerId}, GameId={gameId}");
        }

        /// <summary>
        /// プレイヤーをマッピングから削除
        /// </summary>
        public void RemovePlayer(string playerId)
        {
            if (_playerIdToGameId.TryGetValue(playerId, out int gameId))
            {
                _playerIdToGameId.Remove(playerId);
                _gameIdToPlayerId.Remove(gameId);
                Debug.Log($"[PlayerIdService] Player removed: PlayerId={playerId}, GameId={gameId}");
            }
            else
            {
                Debug.LogWarning($"[PlayerIdService] Cannot remove player - PlayerId not found: {playerId}");
            }
        }

        /// <summary>
        /// 次に割り当てるべきGameIdを取得（未使用の最小値）
        /// </summary>
        public int GetNextGameId()
        {
            int nextId = 0;
            while (_gameIdToPlayerId.ContainsKey(nextId))
            {
                nextId++;
            }

            Debug.Log($"[PlayerIdService] Next available GameId: {nextId}");
            return nextId;
        }

        private void OnDestroy()
        {
            _playerIdToGameId.Clear();
            _gameIdToPlayerId.Clear();
            _localPlayerId = null;
            Debug.Log("[PlayerIdService] Service destroyed, mappings cleared.");
        }
    }
}
