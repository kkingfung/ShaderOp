#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Cysharp.Threading.Tasks;
using ShaderOp.Core;
using ShaderOp.Core.Services;

namespace ShaderOp.Customization
{
    /// <summary>
    /// 布シェーダーショーケース用部屋デコレーションコントローラー
    /// </summary>
    /// <remarks>
    /// 4種類の布マテリアル（Cotton, Silk, Denim, Leather）を3D空間で展示
    /// - カーテン（Silk）: 異方性反射とサテン質感
    /// - ラグ（Cotton）: マットな拡散反射
    /// - クッション（Denim）: 多層ブレンドパターン
    /// - テーブルクロス（Leather）: レザー質感
    ///
    /// UI Toolkitでリアルタイムカスタマイズ可能
    /// </remarks>
    [RequireComponent(typeof(UIDocument))]
    public class RoomDecoratorController : MonoBehaviour
    {
        // ============================================
        // シーン参照
        // ============================================

        [Header("Cloth Objects")]
        [SerializeField] private GameObject? _curtainLeft;
        [SerializeField] private GameObject? _curtainRight;
        [SerializeField] private GameObject? _rug;
        [SerializeField] private GameObject? _cushion1;
        [SerializeField] private GameObject? _cushion2;
        [SerializeField] private GameObject? _cushion3;
        [SerializeField] private GameObject? _tableCloth;

        [Header("Materials")]
        [SerializeField] private Material? _cottonMaterial;
        [SerializeField] private Material? _silkMaterial;
        [SerializeField] private Material? _denimMaterial;
        [SerializeField] private Material? _leatherMaterial;

        [Header("Camera")]
        [SerializeField] private OrbitCameraController? _orbitCamera;

        // ============================================
        // カラープリセット
        // ============================================

        private readonly Color[] _curtainColors = new Color[]
        {
            new Color(0.7f, 0.2f, 0.2f), // 赤
            new Color(0.2f, 0.3f, 0.7f), // 青
            new Color(0.2f, 0.6f, 0.3f), // 緑
            new Color(0.6f, 0.4f, 0.7f), // 紫
            new Color(0.9f, 0.7f, 0.2f), // ゴールド
            new Color(0.3f, 0.3f, 0.3f), // グレー
            new Color(0.8f, 0.5f, 0.3f), // ブラウン
            new Color(0.9f, 0.4f, 0.5f), // ピンク
        };

        private readonly Color[] _rugColors = new Color[]
        {
            new Color(0.8f, 0.6f, 0.4f), // ベージュ
            new Color(0.6f, 0.3f, 0.2f), // ブラウン
            new Color(0.7f, 0.2f, 0.2f), // レッド
            new Color(0.3f, 0.4f, 0.6f), // ネイビー
            new Color(0.4f, 0.6f, 0.4f), // グリーン
            new Color(0.5f, 0.5f, 0.5f), // グレー
            new Color(0.8f, 0.8f, 0.7f), // クリーム
            new Color(0.4f, 0.3f, 0.5f), // パープル
        };

        private readonly Color[] _cushionColors = new Color[]
        {
            new Color(0.2f, 0.3f, 0.8f), // ブルーデニム
            new Color(0.3f, 0.3f, 0.4f), // ダークデニム
            new Color(0.5f, 0.6f, 0.7f), // ライトデニム
            new Color(0.3f, 0.4f, 0.5f), // グレーデニム
            new Color(0.4f, 0.2f, 0.3f), // レッドデニム
            new Color(0.3f, 0.4f, 0.3f), // グリーンデニム
            new Color(0.6f, 0.5f, 0.4f), // ブラウンデニム
            new Color(0.5f, 0.3f, 0.4f), // パープルデニム
        };

        // ============================================
        // 現在のインデックス
        // ============================================

        private int _currentCurtainColorIndex = 0;
        private int _currentRugColorIndex = 0;
        private int _currentCushionColorIndex = 0;

        // ============================================
        // UI 要素
        // ============================================

        private VisualElement? _root;

        // Curtain Controls
        private Button? _curtainColorPrevButton;
        private Button? _curtainColorNextButton;
        private Label? _curtainColorLabel;

        // Rug Controls
        private Button? _rugColorPrevButton;
        private Button? _rugColorNextButton;
        private Label? _rugColorLabel;

        // Cushion Controls
        private Button? _cushionColorPrevButton;
        private Button? _cushionColorNextButton;
        private Label? _cushionColorLabel;

        // Other Buttons
        private Button? _resetButton;
        private Button? _backButton;

        // ============================================
        // Unity ライフサイクル
        // ============================================

        private void OnEnable()
        {
            // UI Toolkit の初期化
            var uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                Debug.LogError("[RoomDecoratorController] UIDocument が見つかりません");
                return;
            }

            _root = uiDocument.rootVisualElement;
            if (_root == null)
            {
                Debug.LogError("[RoomDecoratorController] Root VisualElement が null です");
                return;
            }

            // UI 要素を取得
            QueryUIElements();

            // イベントハンドラ登録
            RegisterEventHandlers();

            // 初期状態を適用
            ApplyDefaultColors();

            Debug.Log("[RoomDecoratorController] 初期化完了");
        }

        private void OnDisable()
        {
            // イベントハンドラ解除
            UnregisterEventHandlers();
        }

        // ============================================
        // UI 要素取得
        // ============================================

        private void QueryUIElements()
        {
            if (_root == null) return;

            // Curtain Controls
            _curtainColorPrevButton = _root.Q<Button>("CurtainColorPrev");
            _curtainColorNextButton = _root.Q<Button>("CurtainColorNext");
            _curtainColorLabel = _root.Q<Label>("CurtainColorValue");

            // Rug Controls
            _rugColorPrevButton = _root.Q<Button>("RugColorPrev");
            _rugColorNextButton = _root.Q<Button>("RugColorNext");
            _rugColorLabel = _root.Q<Label>("RugColorValue");

            // Cushion Controls
            _cushionColorPrevButton = _root.Q<Button>("CushionColorPrev");
            _cushionColorNextButton = _root.Q<Button>("CushionColorNext");
            _cushionColorLabel = _root.Q<Label>("CushionColorValue");

            // Other Buttons
            _resetButton = _root.Q<Button>("ResetButton");
            _backButton = _root.Q<Button>("BackButton");

            // デバッグ警告
            if (_curtainColorPrevButton == null)
                Debug.LogWarning("[RoomDecoratorController] CurtainColorPrev が見つかりません");
            if (_resetButton == null)
                Debug.LogWarning("[RoomDecoratorController] ResetButton が見つかりません");
        }

        // ============================================
        // イベントハンドラ登録/解除
        // ============================================

        private void RegisterEventHandlers()
        {
            // Curtain
            if (_curtainColorPrevButton != null)
                _curtainColorPrevButton.clicked += OnCurtainColorPrevClicked;
            if (_curtainColorNextButton != null)
                _curtainColorNextButton.clicked += OnCurtainColorNextClicked;

            // Rug
            if (_rugColorPrevButton != null)
                _rugColorPrevButton.clicked += OnRugColorPrevClicked;
            if (_rugColorNextButton != null)
                _rugColorNextButton.clicked += OnRugColorNextClicked;

            // Cushion
            if (_cushionColorPrevButton != null)
                _cushionColorPrevButton.clicked += OnCushionColorPrevClicked;
            if (_cushionColorNextButton != null)
                _cushionColorNextButton.clicked += OnCushionColorNextClicked;

            // Other
            if (_resetButton != null)
                _resetButton.clicked += OnResetClicked;
            if (_backButton != null)
                _backButton.clicked += OnBackClicked;
        }

        private void UnregisterEventHandlers()
        {
            // Curtain
            if (_curtainColorPrevButton != null)
                _curtainColorPrevButton.clicked -= OnCurtainColorPrevClicked;
            if (_curtainColorNextButton != null)
                _curtainColorNextButton.clicked -= OnCurtainColorNextClicked;

            // Rug
            if (_rugColorPrevButton != null)
                _rugColorPrevButton.clicked -= OnRugColorPrevClicked;
            if (_rugColorNextButton != null)
                _rugColorNextButton.clicked -= OnRugColorNextClicked;

            // Cushion
            if (_cushionColorPrevButton != null)
                _cushionColorPrevButton.clicked -= OnCushionColorPrevClicked;
            if (_cushionColorNextButton != null)
                _cushionColorNextButton.clicked -= OnCushionColorNextClicked;

            // Other
            if (_resetButton != null)
                _resetButton.clicked -= OnResetClicked;
            if (_backButton != null)
                _backButton.clicked -= OnBackClicked;
        }

        // ============================================
        // イベントハンドラ実装
        // ============================================

        private void OnCurtainColorPrevClicked()
        {
            _currentCurtainColorIndex = (_currentCurtainColorIndex - 1 + _curtainColors.Length) % _curtainColors.Length;
            ApplyCurtainColor();
            UpdateCurtainColorLabel();
        }

        private void OnCurtainColorNextClicked()
        {
            _currentCurtainColorIndex = (_currentCurtainColorIndex + 1) % _curtainColors.Length;
            ApplyCurtainColor();
            UpdateCurtainColorLabel();
        }

        private void OnRugColorPrevClicked()
        {
            _currentRugColorIndex = (_currentRugColorIndex - 1 + _rugColors.Length) % _rugColors.Length;
            ApplyRugColor();
            UpdateRugColorLabel();
        }

        private void OnRugColorNextClicked()
        {
            _currentRugColorIndex = (_currentRugColorIndex + 1) % _rugColors.Length;
            ApplyRugColor();
            UpdateRugColorLabel();
        }

        private void OnCushionColorPrevClicked()
        {
            _currentCushionColorIndex = (_currentCushionColorIndex - 1 + _cushionColors.Length) % _cushionColors.Length;
            ApplyCushionColor();
            UpdateCushionColorLabel();
        }

        private void OnCushionColorNextClicked()
        {
            _currentCushionColorIndex = (_currentCushionColorIndex + 1) % _cushionColors.Length;
            ApplyCushionColor();
            UpdateCushionColorLabel();
        }

        private void OnResetClicked()
        {
            Debug.Log("[RoomDecoratorController] Reset clicked");
            ResetToDefaults();
        }

        private async void OnBackClicked()
        {
            Debug.Log("[RoomDecoratorController] Back clicked");

            // SceneLoaderを使用してメインメニューに戻る
            var sceneLoader = ServiceLocator.Instance?.Get<ISceneLoaderService>();
            if (sceneLoader != null)
            {
                await sceneLoader.LoadMainMenuAsync();
            }
            else
            {
                Debug.LogWarning("[RoomDecoratorController] SceneLoaderService が見つかりません");
            }
        }

        // ============================================
        // マテリアル適用
        // ============================================

        private void ApplyDefaultColors()
        {
            _currentCurtainColorIndex = 4; // ゴールド
            _currentRugColorIndex = 0;     // ベージュ
            _currentCushionColorIndex = 0; // ブルーデニム

            ApplyCurtainColor();
            ApplyRugColor();
            ApplyCushionColor();

            UpdateAllLabels();
        }

        private void ApplyCurtainColor()
        {
            Color color = _curtainColors[_currentCurtainColorIndex];

            // カーテン用のSilkマテリアルをインスタンス化して適用
            if (_curtainLeft != null && _silkMaterial != null)
            {
                var renderer = _curtainLeft.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material instanceMat = new Material(_silkMaterial);
                    instanceMat.SetColor("_BaseColor", color);
                    renderer.material = instanceMat;
                }
            }

            if (_curtainRight != null && _silkMaterial != null)
            {
                var renderer = _curtainRight.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material instanceMat = new Material(_silkMaterial);
                    instanceMat.SetColor("_BaseColor", color);
                    renderer.material = instanceMat;
                }
            }
        }

        private void ApplyRugColor()
        {
            Color color = _rugColors[_currentRugColorIndex];

            // ラグ用のCottonマテリアルを適用
            if (_rug != null && _cottonMaterial != null)
            {
                var renderer = _rug.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material instanceMat = new Material(_cottonMaterial);
                    instanceMat.SetColor("_BaseColor", color);
                    renderer.material = instanceMat;
                }
            }
        }

        private void ApplyCushionColor()
        {
            Color color = _cushionColors[_currentCushionColorIndex];

            // クッション用のDenimマテリアルを適用
            GameObject?[] cushions = { _cushion1, _cushion2, _cushion3 };
            foreach (GameObject? cushion in cushions)
            {
                if (cushion != null && _denimMaterial != null)
                {
                    var renderer = cushion.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        Material instanceMat = new Material(_denimMaterial);
                        instanceMat.SetColor("_BaseColor", color);
                        renderer.material = instanceMat;
                    }
                }
            }
        }

        // ============================================
        // UI ラベル更新
        // ============================================

        private void UpdateAllLabels()
        {
            UpdateCurtainColorLabel();
            UpdateRugColorLabel();
            UpdateCushionColorLabel();
        }

        private void UpdateCurtainColorLabel()
        {
            if (_curtainColorLabel != null)
            {
                string[] names = { "Red", "Blue", "Green", "Purple", "Gold", "Gray", "Brown", "Pink" };
                _curtainColorLabel.text = names[_currentCurtainColorIndex];
            }
        }

        private void UpdateRugColorLabel()
        {
            if (_rugColorLabel != null)
            {
                string[] names = { "Beige", "Brown", "Red", "Navy", "Green", "Gray", "Cream", "Purple" };
                _rugColorLabel.text = names[_currentRugColorIndex];
            }
        }

        private void UpdateCushionColorLabel()
        {
            if (_cushionColorLabel != null)
            {
                string[] names = { "Blue Denim", "Dark Denim", "Light Denim", "Gray Denim", "Red Denim", "Green Denim", "Brown Denim", "Purple Denim" };
                _cushionColorLabel.text = names[_currentCushionColorIndex];
            }
        }

        // ============================================
        // リセット
        // ============================================

        private void ResetToDefaults()
        {
            ApplyDefaultColors();

            // カメラもリセット
            if (_orbitCamera != null)
            {
                _orbitCamera.ResetCamera();
            }

            Debug.Log("[RoomDecoratorController] デフォルトにリセットしました");
        }

        // ============================================
        // カラー取得（カラー名を返す）
        // ============================================

        private string GetColorName(int index, string[] names)
        {
            if (index >= 0 && index < names.Length)
            {
                return names[index];
            }
            return "Unknown";
        }
    }
}
