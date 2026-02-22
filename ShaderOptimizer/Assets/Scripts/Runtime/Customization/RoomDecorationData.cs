#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShaderOp.Customization
{
    /// <summary>
    /// 家具カテゴリ
    /// </summary>
    public enum FurnitureCategory
    {
        Floor,      // 床
        Wall,       // 壁
        Furniture,  // 家具
        Plant,      // 植物
        Decoration, // 装飾品
        Lighting    // 照明
    }

    /// <summary>
    /// 配置済み家具データ
    /// </summary>
    [Serializable]
    public class PlacedFurnitureData
    {
        /// <summary>家具ID</summary>
        public int FurnitureId;

        /// <summary>カテゴリ</summary>
        public FurnitureCategory Category;

        /// <summary>配置座標</summary>
        public Vector3 Position;

        /// <summary>回転</summary>
        public Quaternion Rotation;

        /// <summary>スケール</summary>
        public Vector3 Scale = Vector3.one;

        /// <summary>カラーティント</summary>
        public Color Tint = Color.white;

        public PlacedFurnitureData(int furnitureId, FurnitureCategory category, Vector3 position)
        {
            FurnitureId = furnitureId;
            Category = category;
            Position = position;
            Rotation = Quaternion.identity;
        }
    }

    /// <summary>
    /// 部屋デコレーションデータ
    /// </summary>
    [Serializable]
    public class RoomDecorationData
    {
        /// <summary>部屋名</summary>
        public string RoomName = "My Room";

        /// <summary>床マテリアルID</summary>
        public int FloorMaterialId = 0;

        /// <summary>壁マテリアルID</summary>
        public int WallMaterialId = 0;

        /// <summary>配置済み家具リスト</summary>
        public List<PlacedFurnitureData> PlacedFurniture = new();

        /// <summary>環境光カラー</summary>
        public Color AmbientColor = new Color(1.0f, 0.95f, 0.9f);

        /// <summary>
        /// デフォルト部屋を作成
        /// </summary>
        public static RoomDecorationData CreateDefault()
        {
            return new RoomDecorationData
            {
                RoomName = "My Room",
                FloorMaterialId = 0,
                WallMaterialId = 0,
                PlacedFurniture = new List<PlacedFurnitureData>(),
                AmbientColor = new Color(1.0f, 0.95f, 0.9f)
            };
        }

        /// <summary>
        /// 家具を追加
        /// </summary>
        public void AddFurniture(PlacedFurnitureData furniture)
        {
            PlacedFurniture.Add(furniture);
        }

        /// <summary>
        /// 家具を削除
        /// </summary>
        public void RemoveFurniture(int index)
        {
            if (index >= 0 && index < PlacedFurniture.Count)
            {
                PlacedFurniture.RemoveAt(index);
            }
        }

        /// <summary>
        /// すべての家具をクリア
        /// </summary>
        public void ClearAllFurniture()
        {
            PlacedFurniture.Clear();
        }

        /// <summary>
        /// データをコピー
        /// </summary>
        public RoomDecorationData Clone()
        {
            RoomDecorationData clone = new RoomDecorationData
            {
                RoomName = this.RoomName,
                FloorMaterialId = this.FloorMaterialId,
                WallMaterialId = this.WallMaterialId,
                AmbientColor = this.AmbientColor,
                PlacedFurniture = new List<PlacedFurnitureData>(this.PlacedFurniture)
            };

            return clone;
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
        public static RoomDecorationData FromJson(string json)
        {
            return JsonUtility.FromJson<RoomDecorationData>(json);
        }
    }
}
