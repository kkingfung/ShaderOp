#nullable enable

using System;
using UnityEngine;

namespace ShaderOp.Minigames.HexGrid
{
    /// <summary>
    /// ゲーム駒の種類
    /// </summary>
    public enum PieceType
    {
        None = 0,
        Player1 = 1,
        Player2 = 2,
        Player3 = 3,
        Player4 = 4
    }

    /// <summary>
    /// ヘックスタイルの状態を表現するデータクラス
    /// </summary>
    [Serializable]
    public class HexTile
    {
        /// <summary>タイルの座標</summary>
        public HexCoordinate Coordinate { get; private set; }

        /// <summary>タイル上のゲーム駒（プライベートフィールド）</summary>
        private PieceType _piece;

        /// <summary>タイル上のゲーム駒（プロパティ）</summary>
        public PieceType Piece
        {
            get => _piece;
            set
            {
                if (_piece != value)
                {
                    _piece = value;
                    NotifyStateChanged();
                }
            }
        }

        /// <summary>タイルが空いているか</summary>
        public bool IsEmpty => Piece == PieceType.None;

        /// <summary>タイルが有効か（プレイ可能、プライベートフィールド）</summary>
        private bool _isValid;

        /// <summary>タイルが有効か（プレイ可能）</summary>
        public bool IsValid
        {
            get => _isValid;
            set
            {
                if (_isValid != value)
                {
                    _isValid = value;
                    NotifyStateChanged();
                }
            }
        }

        /// <summary>タイルのワールド座標</summary>
        public Vector3 WorldPosition { get; private set; }

        /// <summary>タイルのハイライト状態（プライベートフィールド）</summary>
        private bool _isHighlighted;

        /// <summary>タイルのハイライト状態</summary>
        public bool IsHighlighted
        {
            get => _isHighlighted;
            set
            {
                if (_isHighlighted != value)
                {
                    _isHighlighted = value;
                    NotifyStateChanged();
                }
            }
        }

        /// <summary>タイルの選択状態（プライベートフィールド）</summary>
        private bool _isSelected;

        /// <summary>タイルの選択状態</summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    NotifyStateChanged();
                }
            }
        }

        /// <summary>タイルの状態が変更されたときのイベント</summary>
        public event Action? OnStateChanged;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public HexTile(HexCoordinate coordinate, Vector3 worldPosition)
        {
            Coordinate = coordinate;
            WorldPosition = worldPosition;
            _piece = PieceType.None;
            _isValid = true;
            _isHighlighted = false;
            _isSelected = false;
        }

        /// <summary>
        /// 状態変更を通知（パフォーマンス最適化）
        /// </summary>
        private void NotifyStateChanged()
        {
            OnStateChanged?.Invoke();
        }

        /// <summary>
        /// タイルをリセット
        /// </summary>
        public void Reset()
        {
            Piece = PieceType.None;
            IsHighlighted = false;
            IsSelected = false;
        }

        /// <summary>
        /// 駒を配置
        /// </summary>
        public bool PlacePiece(PieceType piece)
        {
            if (!IsEmpty || !IsValid)
            {
                return false;
            }

            Piece = piece;
            return true;
        }

        /// <summary>
        /// 駒を削除
        /// </summary>
        public void RemovePiece()
        {
            Piece = PieceType.None;
        }

        public override string ToString()
        {
            return $"HexTile[{Coordinate}] Piece:{Piece} Valid:{IsValid}";
        }
    }
}
