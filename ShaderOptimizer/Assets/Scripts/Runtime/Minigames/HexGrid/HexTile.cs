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

        /// <summary>タイル上のゲーム駒</summary>
        public PieceType Piece { get; set; }

        /// <summary>タイルが空いているか</summary>
        public bool IsEmpty => Piece == PieceType.None;

        /// <summary>タイルが有効か（プレイ可能）</summary>
        public bool IsValid { get; set; }

        /// <summary>タイルのワールド座標</summary>
        public Vector3 WorldPosition { get; private set; }

        /// <summary>タイルのハイライト状態</summary>
        public bool IsHighlighted { get; set; }

        /// <summary>タイルの選択状態</summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public HexTile(HexCoordinate coordinate, Vector3 worldPosition)
        {
            Coordinate = coordinate;
            WorldPosition = worldPosition;
            Piece = PieceType.None;
            IsValid = true;
            IsHighlighted = false;
            IsSelected = false;
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
