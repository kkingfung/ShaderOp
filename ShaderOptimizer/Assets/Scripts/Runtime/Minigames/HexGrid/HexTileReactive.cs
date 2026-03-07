#nullable enable

using System;
using UniRx;
using UnityEngine;

namespace ShaderOp.Minigames.HexGrid
{
    /// <summary>
    /// ヘックスタイルの状態を表現するリアクティブデータクラス（UniRx版）
    /// </summary>
    /// <remarks>
    /// ReactivePropertyを使用してプロパティ変更を自動通知
    /// より宣言的でクリーンなコード
    /// </remarks>
    [Serializable]
    public class HexTileReactive : IDisposable
    {
        /// <summary>タイルの座標</summary>
        public HexCoordinate Coordinate { get; private set; }

        /// <summary>タイル上のゲーム駒（ReactiveProperty）</summary>
        private readonly ReactiveProperty<PieceType> _piece;
        public IReadOnlyReactiveProperty<PieceType> Piece => _piece;

        /// <summary>タイルが有効か（プレイ可能、ReactiveProperty）</summary>
        private readonly ReactiveProperty<bool> _isValid;
        public IReadOnlyReactiveProperty<bool> IsValid => _isValid;

        /// <summary>タイルのハイライト状態（ReactiveProperty）</summary>
        private readonly ReactiveProperty<bool> _isHighlighted;
        public IReadOnlyReactiveProperty<bool> IsHighlighted => _isHighlighted;

        /// <summary>タイルの選択状態（ReactiveProperty）</summary>
        private readonly ReactiveProperty<bool> _isSelected;
        public IReadOnlyReactiveProperty<bool> IsSelected => _isSelected;

        /// <summary>タイルが空いているか（Computed Observable）</summary>
        public IReadOnlyReactiveProperty<bool> IsEmpty { get; }

        /// <summary>タイルのワールド座標</summary>
        public Vector3 WorldPosition { get; private set; }

        /// <summary>タイル状態変更の統合ストリーム</summary>
        public IObservable<Unit> OnAnyStateChanged { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public HexTileReactive(HexCoordinate coordinate, Vector3 worldPosition)
        {
            Coordinate = coordinate;
            WorldPosition = worldPosition;

            // ReactivePropertyを初期化
            _piece = new ReactiveProperty<PieceType>(PieceType.None);
            _isValid = new ReactiveProperty<bool>(true);
            _isHighlighted = new ReactiveProperty<bool>(false);
            _isSelected = new ReactiveProperty<bool>(false);

            // IsEmptyを計算されたプロパティとして定義
            IsEmpty = _piece.Select(p => p == PieceType.None).ToReadOnlyReactiveProperty();

            // すべての状態変更を統合
            OnAnyStateChanged = Observable.Merge(
                _piece.AsUnitObservable(),
                _isValid.AsUnitObservable(),
                _isHighlighted.AsUnitObservable(),
                _isSelected.AsUnitObservable()
            );
        }

        /// <summary>
        /// 駒を設定
        /// </summary>
        public void SetPiece(PieceType piece)
        {
            _piece.Value = piece;
        }

        /// <summary>
        /// 駒を取得
        /// </summary>
        public PieceType GetPiece()
        {
            return _piece.Value;
        }

        /// <summary>
        /// 有効状態を設定
        /// </summary>
        public void SetValid(bool isValid)
        {
            _isValid.Value = isValid;
        }

        /// <summary>
        /// ハイライト状態を設定
        /// </summary>
        public void SetHighlighted(bool isHighlighted)
        {
            _isHighlighted.Value = isHighlighted;
        }

        /// <summary>
        /// 選択状態を設定
        /// </summary>
        public void SetSelected(bool isSelected)
        {
            _isSelected.Value = isSelected;
        }

        /// <summary>
        /// タイルをリセット
        /// </summary>
        public void Reset()
        {
            _piece.Value = PieceType.None;
            _isHighlighted.Value = false;
            _isSelected.Value = false;
        }

        /// <summary>
        /// 駒を配置
        /// </summary>
        public bool PlacePiece(PieceType piece)
        {
            if (!IsEmpty.Value || !_isValid.Value)
            {
                return false;
            }

            _piece.Value = piece;
            return true;
        }

        /// <summary>
        /// 駒を削除
        /// </summary>
        public void RemovePiece()
        {
            _piece.Value = PieceType.None;
        }

        /// <summary>
        /// Dispose（ReactivePropertyのクリーンアップ）
        /// </summary>
        public void Dispose()
        {
            _piece?.Dispose();
            _isValid?.Dispose();
            _isHighlighted?.Dispose();
            _isSelected?.Dispose();
        }

        public override string ToString()
        {
            return $"HexTileReactive[{Coordinate}] Piece:{_piece.Value} Valid:{_isValid.Value}";
        }
    }
}
