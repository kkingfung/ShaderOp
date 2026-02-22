#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace ShaderOp.Customization
{
    /// <summary>
    /// 部屋デコレーター（メインコントローラー）
    /// </summary>
    /// <remarks>
    /// 部屋のデコレーション管理を統括
    /// 家具配置、床/壁変更、環境光設定を提供
    /// </remarks>
    public class RoomDecorator : MonoBehaviour
    {
        /// <summary>デコレーションデータ</summary>
        [SerializeField] private RoomDecorationData _decorationData = new();

        /// <summary>部屋のルートオブジェクト</summary>
        [SerializeField] private GameObject? _roomRoot;

        /// <summary>床オブジェクト</summary>
        [SerializeField] private GameObject? _floor;

        /// <summary>壁オブジェクト</summary>
        [SerializeField] private GameObject? _wall;

        /// <summary>家具の親Transform</summary>
        [SerializeField] private Transform? _furnitureParent;

        /// <summary>配置済み家具のGameObject管理</summary>
        private List<GameObject> _placedFurnitureObjects = new();

        /// <summary>床マテリアルリスト（AssetDatabase or Addressables）</summary>
        [SerializeField] private List<Material> _floorMaterials = new();

        /// <summary>壁マテリアルリスト</summary>
        [SerializeField] private List<Material> _wallMaterials = new();

        /// <summary>家具Prefabリスト（カテゴリごと）</summary>
        [SerializeField] private List<GameObject> _furniturePrefabs = new();

        /// <summary>デコレーション変更イベント</summary>
        public event Action<RoomDecorationData>? OnDecorationChanged;

        private void Awake()
        {
            Initialize();
        }

        /// <summary>
        /// 初期化
        /// </summary>
        public void Initialize()
        {
            if (_roomRoot == null)
            {
                Debug.LogError("[RoomDecorator] Room root is not assigned!");
                return;
            }

            if (_furnitureParent == null)
            {
                // 自動生成
                GameObject furnitureContainer = new GameObject("FurnitureContainer");
                furnitureContainer.transform.SetParent(_roomRoot.transform);
                _furnitureParent = furnitureContainer.transform;
            }

            // 初期デコレーションを適用
            ApplyDecoration(_decorationData);
        }

        /// <summary>
        /// デコレーションデータを適用
        /// </summary>
        public void ApplyDecoration(RoomDecorationData data)
        {
            _decorationData = data.Clone();

            // 床と壁を変更
            SetFloorMaterial(data.FloorMaterialId);
            SetWallMaterial(data.WallMaterialId);

            // 環境光を変更
            RenderSettings.ambientLight = data.AmbientColor;

            // 既存の家具をクリア
            ClearAllFurniture();

            // 家具を配置
            foreach (PlacedFurnitureData furnitureData in data.PlacedFurniture)
            {
                PlaceFurnitureFromData(furnitureData);
            }

            OnDecorationChanged?.Invoke(_decorationData);
        }

        /// <summary>
        /// 床マテリアルを設定
        /// </summary>
        public void SetFloorMaterial(int materialId)
        {
            if (_floor == null) return;

            materialId = Mathf.Clamp(materialId, 0, _floorMaterials.Count - 1);
            _decorationData.FloorMaterialId = materialId;

            Renderer? renderer = _floor.GetComponent<Renderer>();
            if (renderer != null && _floorMaterials.Count > materialId)
            {
                renderer.material = _floorMaterials[materialId];
            }

            OnDecorationChanged?.Invoke(_decorationData);
        }

        /// <summary>
        /// 壁マテリアルを設定
        /// </summary>
        public void SetWallMaterial(int materialId)
        {
            if (_wall == null) return;

            materialId = Mathf.Clamp(materialId, 0, _wallMaterials.Count - 1);
            _decorationData.WallMaterialId = materialId;

            Renderer? renderer = _wall.GetComponent<Renderer>();
            if (renderer != null && _wallMaterials.Count > materialId)
            {
                renderer.material = _wallMaterials[materialId];
            }

            OnDecorationChanged?.Invoke(_decorationData);
        }

        /// <summary>
        /// 家具を配置
        /// </summary>
        public GameObject? PlaceFurniture(int furnitureId, FurnitureCategory category, Vector3 position)
        {
            if (furnitureId < 0 || furnitureId >= _furniturePrefabs.Count)
            {
                Debug.LogWarning($"[RoomDecorator] Invalid furniture ID: {furnitureId}");
                return null;
            }

            GameObject prefab = _furniturePrefabs[furnitureId];
            GameObject furnitureObj = Instantiate(prefab, _furnitureParent);
            furnitureObj.transform.position = position;

            _placedFurnitureObjects.Add(furnitureObj);

            // データに追加
            PlacedFurnitureData furnitureData = new PlacedFurnitureData(furnitureId, category, position);
            _decorationData.AddFurniture(furnitureData);

            OnDecorationChanged?.Invoke(_decorationData);
            return furnitureObj;
        }

        /// <summary>
        /// データから家具を配置
        /// </summary>
        private GameObject? PlaceFurnitureFromData(PlacedFurnitureData data)
        {
            if (data.FurnitureId < 0 || data.FurnitureId >= _furniturePrefabs.Count)
            {
                return null;
            }

            GameObject prefab = _furniturePrefabs[data.FurnitureId];
            GameObject furnitureObj = Instantiate(prefab, _furnitureParent);
            furnitureObj.transform.position = data.Position;
            furnitureObj.transform.rotation = data.Rotation;
            furnitureObj.transform.localScale = data.Scale;

            // カラーティントを適用
            Renderer? renderer = furnitureObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = data.Tint;
            }

            _placedFurnitureObjects.Add(furnitureObj);
            return furnitureObj;
        }

        /// <summary>
        /// 家具を削除
        /// </summary>
        public void RemoveFurniture(int index)
        {
            if (index >= 0 && index < _placedFurnitureObjects.Count)
            {
                GameObject furnitureObj = _placedFurnitureObjects[index];
                _placedFurnitureObjects.RemoveAt(index);
                Destroy(furnitureObj);

                _decorationData.RemoveFurniture(index);
                OnDecorationChanged?.Invoke(_decorationData);
            }
        }

        /// <summary>
        /// すべての家具をクリア
        /// </summary>
        public void ClearAllFurniture()
        {
            foreach (GameObject furnitureObj in _placedFurnitureObjects)
            {
                if (furnitureObj != null)
                {
                    Destroy(furnitureObj);
                }
            }

            _placedFurnitureObjects.Clear();
            _decorationData.ClearAllFurniture();
        }

        /// <summary>
        /// 環境光カラーを設定
        /// </summary>
        public void SetAmbientColor(Color color)
        {
            _decorationData.AmbientColor = color;
            RenderSettings.ambientLight = color;
            OnDecorationChanged?.Invoke(_decorationData);
        }

        /// <summary>
        /// 現在のデコレーションデータを取得
        /// </summary>
        public RoomDecorationData GetDecorationData()
        {
            return _decorationData.Clone();
        }

        /// <summary>
        /// デコレーションをリセット
        /// </summary>
        public void ResetToDefault()
        {
            ApplyDecoration(RoomDecorationData.CreateDefault());
        }

        /// <summary>
        /// デコレーションデータを保存
        /// </summary>
        public void SaveDecoration(string saveKey = "RoomDecoration")
        {
            string json = _decorationData.ToJson();
            PlayerPrefs.SetString(saveKey, json);
            PlayerPrefs.Save();
            Debug.Log($"[RoomDecorator] Saved decoration: {saveKey}");
        }

        /// <summary>
        /// デコレーションデータを読み込み
        /// </summary>
        public void LoadDecoration(string saveKey = "RoomDecoration")
        {
            if (PlayerPrefs.HasKey(saveKey))
            {
                string json = PlayerPrefs.GetString(saveKey);
                RoomDecorationData data = RoomDecorationData.FromJson(json);
                ApplyDecoration(data);
                Debug.Log($"[RoomDecorator] Loaded decoration: {saveKey}");
            }
            else
            {
                Debug.LogWarning($"[RoomDecorator] No saved data found: {saveKey}");
            }
        }
    }
}
