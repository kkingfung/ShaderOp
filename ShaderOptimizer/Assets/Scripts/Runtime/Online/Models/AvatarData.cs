#nullable enable

using System;
using UnityEngine;

namespace ShaderOp.Online.Models
{
    /// <summary>
    /// アバターカスタマイズデータ
    /// </summary>
    /// <remarks>
    /// 2Dアバターの見た目設定を保持します。
    /// カラーマスクシェーダーと連携してカスタマイズを実現します。
    /// </remarks>
    [Serializable]
    public class AvatarData
    {
        /// <summary>プレイヤーID</summary>
        [SerializeField] private string _playerId = string.Empty;
        public string PlayerId
        {
            get => _playerId;
            set => _playerId = value;
        }

        /// <summary>肌の色ID（1-9）</summary>
        [SerializeField] private int _skinToneId = 1;
        public int SkinToneId
        {
            get => _skinToneId;
            set => _skinToneId = Mathf.Clamp(value, 1, 9);
        }

        /// <summary>顔タイプID（0-1）</summary>
        [SerializeField] private int _faceTypeId = 0;
        public int FaceTypeId
        {
            get => _faceTypeId;
            set => _faceTypeId = Mathf.Clamp(value, 0, 1);
        }

        /// <summary>目の色ID（1-9）</summary>
        [SerializeField] private int _eyeColorId = 1;
        public int EyeColorId
        {
            get => _eyeColorId;
            set => _eyeColorId = Mathf.Clamp(value, 1, 9);
        }

        /// <summary>髪型ID（0-14）</summary>
        [SerializeField] private int _hairstyleId = 0;
        public int HairstyleId
        {
            get => _hairstyleId;
            set => _hairstyleId = Mathf.Clamp(value, 0, 14);
        }

        /// <summary>髪の色ID（1-9）</summary>
        [SerializeField] private int _hairColorId = 1;
        public int HairColorId
        {
            get => _hairColorId;
            set => _hairColorId = Mathf.Clamp(value, 1, 9);
        }

        /// <summary>服装タイプID（0-10）</summary>
        [SerializeField] private int _outfitId = 0;
        public int OutfitId
        {
            get => _outfitId;
            set => _outfitId = Mathf.Clamp(value, 0, 10);
        }

        /// <summary>服の色（プライマリ）</summary>
        [SerializeField] private Color _clothingColorPrimary = Color.white;
        public Color ClothingColorPrimary
        {
            get => _clothingColorPrimary;
            set => _clothingColorPrimary = value;
        }

        /// <summary>服の色（セカンダリ）</summary>
        [SerializeField] private Color _clothingColorSecondary = Color.gray;
        public Color ClothingColorSecondary
        {
            get => _clothingColorSecondary;
            set => _clothingColorSecondary = value;
        }

        /// <summary>アクセサリーID（0=なし、1-20=各種アクセサリー）</summary>
        [SerializeField] private int _accessoryId = 0;
        public int AccessoryId
        {
            get => _accessoryId;
            set => _accessoryId = Mathf.Clamp(value, 0, 20);
        }

        /// <summary>エフェクトID（0=なし、1-10=各種エフェクト）</summary>
        [SerializeField] private int _effectId = 0;
        public int EffectId
        {
            get => _effectId;
            set => _effectId = Mathf.Clamp(value, 0, 10);
        }

        /// <summary>最終更新日時</summary>
        [SerializeField] private string _updatedAt = string.Empty;
        public string UpdatedAt
        {
            get => _updatedAt;
            set => _updatedAt = value;
        }

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public AvatarData() { }

        /// <summary>
        /// デフォルトアバター作成用コンストラクタ
        /// </summary>
        public AvatarData(string playerId)
        {
            _playerId = playerId;
            _skinToneId = 1;
            _faceTypeId = 0;
            _eyeColorId = 1;
            _hairstyleId = 0;
            _hairColorId = 1;
            _outfitId = 0;
            _clothingColorPrimary = new Color(0.8f, 0.8f, 1.0f); // ライトブルー
            _clothingColorSecondary = new Color(0.3f, 0.3f, 0.5f); // ダークブルー
            _accessoryId = 0;
            _effectId = 0;
            _updatedAt = DateTime.UtcNow.ToString("o");
        }

        /// <summary>
        /// 更新日時を現在時刻に設定
        /// </summary>
        public void MarkAsUpdated()
        {
            _updatedAt = DateTime.UtcNow.ToString("o");
        }
    }
}
