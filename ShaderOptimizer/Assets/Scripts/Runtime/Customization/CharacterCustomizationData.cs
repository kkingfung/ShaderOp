#nullable enable

using System;
using UnityEngine;

namespace ShaderOp.Customization
{
    /// <summary>
    /// キャラクターの性別
    /// </summary>
    public enum Gender
    {
        Male,
        Female
    }

    /// <summary>
    /// ボディパーツタイプ
    /// </summary>
    public enum BodyPartType
    {
        Head,
        Body,
        Hair,
        Eyes,
        Face,
        Top,
        Bottom,
        Shoes,
        Accessory
    }

    /// <summary>
    /// キャラクターカスタマイズデータ
    /// </summary>
    /// <remarks>
    /// キャラクターの見た目パラメータを保持
    /// </remarks>
    [Serializable]
    public class CharacterCustomizationData
    {
        /// <summary>性別</summary>
        public Gender Gender = Gender.Female;

        /// <summary>肌色ID (0-9)</summary>
        public int SkinToneId = 0;

        /// <summary>髪型ID (0-50)</summary>
        public int HairstyleId = 0;

        /// <summary>髪色</summary>
        public Color HairColor = new Color(0.3f, 0.15f, 0.05f); // 茶色

        /// <summary>瞳色</summary>
        public Color EyeColor = new Color(0.2f, 0.1f, 0.05f); // ダークブラウン

        /// <summary>顔パーツID (0-20)</summary>
        public int FaceId = 0;

        /// <summary>トップス（上着）ID (0-100)</summary>
        public int TopId = 0;

        /// <summary>トップスカラー</summary>
        public Color TopColor = Color.white;

        /// <summary>ボトムス（下着）ID (0-100)</summary>
        public int BottomId = 0;

        /// <summary>ボトムスカラー</summary>
        public Color BottomColor = Color.white;

        /// <summary>靴ID (0-50)</summary>
        public int ShoeId = 0;

        /// <summary>靴カラー</summary>
        public Color ShoeColor = Color.white;

        /// <summary>アクセサリID (0-100)</summary>
        public int AccessoryId = 0;

        /// <summary>身長スケール (0.8 - 1.2)</summary>
        public float HeightScale = 1.0f;

        /// <summary>
        /// デフォルトキャラクターを作成
        /// </summary>
        public static CharacterCustomizationData CreateDefault()
        {
            return new CharacterCustomizationData
            {
                Gender = Gender.Female,
                SkinToneId = 0,
                HairstyleId = 0,
                HairColor = new Color(0.3f, 0.15f, 0.05f),
                EyeColor = new Color(0.2f, 0.1f, 0.05f),
                FaceId = 0,
                TopId = 0,
                TopColor = Color.white,
                BottomId = 0,
                BottomColor = Color.white,
                ShoeId = 0,
                ShoeColor = Color.white,
                AccessoryId = 0,
                HeightScale = 1.0f
            };
        }

        /// <summary>
        /// データをコピー
        /// </summary>
        public CharacterCustomizationData Clone()
        {
            return new CharacterCustomizationData
            {
                Gender = this.Gender,
                SkinToneId = this.SkinToneId,
                HairstyleId = this.HairstyleId,
                HairColor = this.HairColor,
                EyeColor = this.EyeColor,
                FaceId = this.FaceId,
                TopId = this.TopId,
                TopColor = this.TopColor,
                BottomId = this.BottomId,
                BottomColor = this.BottomColor,
                ShoeId = this.ShoeId,
                ShoeColor = this.ShoeColor,
                AccessoryId = this.AccessoryId,
                HeightScale = this.HeightScale
            };
        }

        /// <summary>
        /// JSON形式にシリアライズ
        /// </summary>
        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        /// <summary>
        /// JSON形式からデシリアライズ
        /// </summary>
        public static CharacterCustomizationData FromJson(string json)
        {
            return JsonUtility.FromJson<CharacterCustomizationData>(json);
        }
    }
}
