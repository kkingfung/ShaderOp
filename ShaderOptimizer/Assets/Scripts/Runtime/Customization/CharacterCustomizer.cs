#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShaderOp.Customization
{
    /// <summary>
    /// キャラクターカスタマイザー（メインコントローラー）
    /// </summary>
    /// <remarks>
    /// キャラクターの外見カスタマイズを統括管理
    /// パーツ切り替え、カラー変更、プレビュー機能を提供
    /// </remarks>
    public class CharacterCustomizer : MonoBehaviour
    {
        /// <summary>カスタマイズデータ</summary>
        [SerializeField] private CharacterCustomizationData _customizationData = new();

        /// <summary>キャラクタールートオブジェクト</summary>
        [SerializeField] private GameObject? _characterRoot;

        /// <summary>ボディパーツのMaterialController管理</summary>
        private Dictionary<BodyPartType, MaterialController> _materialControllers = new();

        /// <summary>ボディパーツのGameObject管理</summary>
        private Dictionary<BodyPartType, GameObject> _bodyParts = new();

        /// <summary>カスタマイズ変更イベント</summary>
        public event Action<CharacterCustomizationData>? OnCustomizationChanged;

        private void Awake()
        {
            Initialize();
        }

        /// <summary>
        /// 初期化
        /// </summary>
        public void Initialize()
        {
            if (_characterRoot == null)
            {
                Debug.LogError("[CharacterCustomizer] Character root is not assigned!");
                return;
            }

            // ボディパーツとMaterialControllerを自動検出
            FindBodyParts();
            SetupMaterialControllers();

            // 初期カスタマイズを適用
            ApplyCustomization(_customizationData);
        }

        /// <summary>
        /// ボディパーツを検出
        /// </summary>
        private void FindBodyParts()
        {
            if (_characterRoot == null) return;

            // 命名規則に基づいてボディパーツを検出
            // 例: "Hair", "Body", "Top", "Bottom" などの名前を持つGameObject
            FindBodyPart(BodyPartType.Hair, "Hair");
            FindBodyPart(BodyPartType.Body, "Body");
            FindBodyPart(BodyPartType.Head, "Head");
            FindBodyPart(BodyPartType.Eyes, "Eyes");
            FindBodyPart(BodyPartType.Face, "Face");
            FindBodyPart(BodyPartType.Top, "Top");
            FindBodyPart(BodyPartType.Bottom, "Bottom");
            FindBodyPart(BodyPartType.Shoes, "Shoes");
            FindBodyPart(BodyPartType.Accessory, "Accessory");
        }

        /// <summary>
        /// 特定のボディパーツを検出
        /// </summary>
        private void FindBodyPart(BodyPartType partType, string partName)
        {
            if (_characterRoot == null) return;

            Transform? partTransform = _characterRoot.transform.Find(partName);
            if (partTransform != null)
            {
                _bodyParts[partType] = partTransform.gameObject;
            }
        }

        /// <summary>
        /// MaterialControllerをセットアップ
        /// </summary>
        private void SetupMaterialControllers()
        {
            foreach (var kvp in _bodyParts)
            {
                BodyPartType partType = kvp.Key;
                GameObject partObject = kvp.Value;

                MaterialController? controller = partObject.GetComponent<MaterialController>();
                if (controller == null)
                {
                    controller = partObject.AddComponent<MaterialController>();
                    controller.Initialize();
                }

                _materialControllers[partType] = controller;
            }
        }

        /// <summary>
        /// カスタマイズデータを適用
        /// </summary>
        public void ApplyCustomization(CharacterCustomizationData data)
        {
            _customizationData = data.Clone();

            // 髪色を適用
            if (_materialControllers.TryGetValue(BodyPartType.Hair, out MaterialController? hairController))
            {
                hairController.SetHairColor(data.HairColor);
            }

            // 肌色を適用
            if (_materialControllers.TryGetValue(BodyPartType.Body, out MaterialController? bodyController))
            {
                bodyController.SetSkinColor(GetSkinColorFromId(data.SkinToneId));
            }

            // 瞳色を適用
            if (_materialControllers.TryGetValue(BodyPartType.Eyes, out MaterialController? eyesController))
            {
                eyesController.SetEyeColor(data.EyeColor);
            }

            // トップスカラーを適用
            if (_materialControllers.TryGetValue(BodyPartType.Top, out MaterialController? topController))
            {
                topController.SetClothColor(data.TopColor);
            }

            // ボトムスカラーを適用
            if (_materialControllers.TryGetValue(BodyPartType.Bottom, out MaterialController? bottomController))
            {
                bottomController.SetClothColor(data.BottomColor);
            }

            // 靴カラーを適用
            if (_materialControllers.TryGetValue(BodyPartType.Shoes, out MaterialController? shoeController))
            {
                shoeController.SetClothColor(data.ShoeColor);
            }

            // 身長スケールを適用
            if (_characterRoot != null)
            {
                Vector3 scale = _characterRoot.transform.localScale;
                scale.y = data.HeightScale;
                _characterRoot.transform.localScale = scale;
            }

            OnCustomizationChanged?.Invoke(_customizationData);
        }

        /// <summary>
        /// 髪色を変更
        /// </summary>
        public void SetHairColor(Color color)
        {
            _customizationData.HairColor = color;

            if (_materialControllers.TryGetValue(BodyPartType.Hair, out MaterialController? controller))
            {
                controller.SetHairColor(color);
            }

            OnCustomizationChanged?.Invoke(_customizationData);
        }

        /// <summary>
        /// 瞳色を変更
        /// </summary>
        public void SetEyeColor(Color color)
        {
            _customizationData.EyeColor = color;

            if (_materialControllers.TryGetValue(BodyPartType.Eyes, out MaterialController? controller))
            {
                controller.SetEyeColor(color);
            }

            OnCustomizationChanged?.Invoke(_customizationData);
        }

        /// <summary>
        /// 肌色IDを変更
        /// </summary>
        public void SetSkinTone(int skinToneId)
        {
            _customizationData.SkinToneId = Mathf.Clamp(skinToneId, 0, 9);

            if (_materialControllers.TryGetValue(BodyPartType.Body, out MaterialController? controller))
            {
                controller.SetSkinColor(GetSkinColorFromId(_customizationData.SkinToneId));
            }

            OnCustomizationChanged?.Invoke(_customizationData);
        }

        /// <summary>
        /// 衣装カラーを変更
        /// </summary>
        public void SetTopColor(Color color)
        {
            _customizationData.TopColor = color;

            if (_materialControllers.TryGetValue(BodyPartType.Top, out MaterialController? controller))
            {
                controller.SetClothColor(color);
            }

            OnCustomizationChanged?.Invoke(_customizationData);
        }

        /// <summary>
        /// ボトムスカラーを変更
        /// </summary>
        public void SetBottomColor(Color color)
        {
            _customizationData.BottomColor = color;

            if (_materialControllers.TryGetValue(BodyPartType.Bottom, out MaterialController? controller))
            {
                controller.SetClothColor(color);
            }

            OnCustomizationChanged?.Invoke(_customizationData);
        }

        /// <summary>
        /// 身長スケールを変更
        /// </summary>
        public void SetHeightScale(float scale)
        {
            _customizationData.HeightScale = Mathf.Clamp(scale, 0.8f, 1.2f);

            if (_characterRoot != null)
            {
                Vector3 localScale = _characterRoot.transform.localScale;
                localScale.y = _customizationData.HeightScale;
                _characterRoot.transform.localScale = localScale;
            }

            OnCustomizationChanged?.Invoke(_customizationData);
        }

        /// <summary>
        /// 肌色IDから肌色を取得
        /// </summary>
        private Color GetSkinColorFromId(int id)
        {
            // 肌色パレット（10段階）
            Color[] skinTones = new Color[]
            {
                new Color(1.0f, 0.87f, 0.78f),  // 0: とても明るい
                new Color(0.95f, 0.82f, 0.73f), // 1: 明るい
                new Color(0.90f, 0.77f, 0.68f), // 2: やや明るい
                new Color(0.85f, 0.72f, 0.63f), // 3: 標準
                new Color(0.80f, 0.67f, 0.58f), // 4: やや暗い
                new Color(0.75f, 0.62f, 0.53f), // 5: 暗い
                new Color(0.70f, 0.57f, 0.48f), // 6: かなり暗い
                new Color(0.60f, 0.47f, 0.38f), // 7: とても暗い
                new Color(0.50f, 0.37f, 0.28f), // 8: 非常に暗い
                new Color(0.40f, 0.27f, 0.18f)  // 9: 最も暗い
            };

            return skinTones[Mathf.Clamp(id, 0, skinTones.Length - 1)];
        }

        /// <summary>
        /// 現在のカスタマイズデータを取得
        /// </summary>
        public CharacterCustomizationData GetCustomizationData()
        {
            return _customizationData.Clone();
        }

        /// <summary>
        /// カスタマイズをリセット
        /// </summary>
        public void ResetToDefault()
        {
            ApplyCustomization(CharacterCustomizationData.CreateDefault());
        }

        /// <summary>
        /// カスタマイズデータを保存
        /// </summary>
        public void SaveCustomization(string saveKey = "CharacterCustomization")
        {
            string json = _customizationData.ToJson();
            PlayerPrefs.SetString(saveKey, json);
            PlayerPrefs.Save();
            Debug.Log($"[CharacterCustomizer] Saved customization: {saveKey}");
        }

        /// <summary>
        /// カスタマイズデータを読み込み
        /// </summary>
        public void LoadCustomization(string saveKey = "CharacterCustomization")
        {
            if (PlayerPrefs.HasKey(saveKey))
            {
                string json = PlayerPrefs.GetString(saveKey);
                CharacterCustomizationData data = CharacterCustomizationData.FromJson(json);
                ApplyCustomization(data);
                Debug.Log($"[CharacterCustomizer] Loaded customization: {saveKey}");
            }
            else
            {
                Debug.LogWarning($"[CharacterCustomizer] No saved data found: {saveKey}");
            }
        }
    }
}
