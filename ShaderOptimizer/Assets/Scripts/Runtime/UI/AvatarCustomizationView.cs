#nullable enable

using UnityEngine;
using UnityEngine.UIElements;
using ShaderOp.UI.ViewModels;
using ShaderOp.Core.Services;
using UniRx;
using Cysharp.Threading.Tasks;
using System;

namespace ShaderOp.Runtime.UI
{
    /// <summary>
    /// アバターカスタマイズ画面のView
    /// </summary>
    /// <remarks>
    /// AvatarCustomization_Portrait.uxml と AvatarCustomizationViewModel を使用
    /// 髪型・目・服・アクセサリーのカスタマイズUIを管理
    /// </remarks>
    [RequireComponent(typeof(UIDocument))]
    public class AvatarCustomizationView : MonoBehaviour
    {
        // ============================================
        // フィールド
        // ============================================

        private UIDocument? _uiDocument;
        private VisualElement? _root;
        private AvatarCustomizationViewModel? _viewModel;
        private readonly CompositeDisposable _disposables = new();

        // UI要素
        private Button? _backButton;
        private Button? _saveButton;
        private Button? _resetButton;

        // カテゴリボタン
        private Button? _hairCategoryButton;
        private Button? _eyesCategoryButton;
        private Button? _outfitCategoryButton;
        private Button? _accessoryCategoryButton;

        // Hair controls
        private Button? _hairPrevButton;
        private Button? _hairNextButton;
        private Label? _hairIdLabel;
        private Slider? _hairColorR;
        private Slider? _hairColorG;
        private Slider? _hairColorB;

        // Eye controls
        private Button? _eyePrevButton;
        private Button? _eyeNextButton;
        private Label? _eyeIdLabel;
        private Slider? _eyeColorR;
        private Slider? _eyeColorG;
        private Slider? _eyeColorB;

        // Outfit controls
        private Button? _outfitPrevButton;
        private Button? _outfitNextButton;
        private Label? _outfitIdLabel;

        // Camera controls
        private Button? _cameraLeftButton;
        private Button? _cameraRightButton;

        // ============================================
        // Unity ライフサイクル
        // ============================================

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
            _viewModel = new AvatarCustomizationViewModel();
        }

        private void OnEnable()
        {
            if (_uiDocument == null || _uiDocument.rootVisualElement == null)
            {
                Debug.LogError("[AvatarCustomizationView] UIDocument または rootVisualElement が null です");
                return;
            }

            _root = _uiDocument.rootVisualElement;

            QueryUIElements();
            RegisterEventHandlers();
            BindViewModel();

            Debug.Log("[AvatarCustomizationView] UI初期化完了");
        }

        private void OnDisable()
        {
            UnregisterEventHandlers();
            UnbindViewModel();
        }

        private void OnDestroy()
        {
            _viewModel?.Dispose();
            _disposables.Dispose();
        }

        // ============================================
        // UI要素取得
        // ============================================

        private void QueryUIElements()
        {
            if (_root == null) return;

            // ヘッダーボタン
            _backButton = _root.Q<Button>("BackButton");
            _saveButton = _root.Q<Button>("SaveButton");
            _resetButton = _root.Q<Button>("ResetButton");

            // カテゴリボタン
            _hairCategoryButton = _root.Q<Button>("HairCategoryButton");
            _eyesCategoryButton = _root.Q<Button>("EyesCategoryButton");
            _outfitCategoryButton = _root.Q<Button>("OutfitCategoryButton");
            _accessoryCategoryButton = _root.Q<Button>("AccessoryCategoryButton");

            // Hair controls
            _hairPrevButton = _root.Q<Button>("HairPrevButton");
            _hairNextButton = _root.Q<Button>("HairNextButton");
            _hairIdLabel = _root.Q<Label>("HairIdLabel");
            _hairColorR = _root.Q<Slider>("HairColorR");
            _hairColorG = _root.Q<Slider>("HairColorG");
            _hairColorB = _root.Q<Slider>("HairColorB");

            // Eye controls
            _eyePrevButton = _root.Q<Button>("EyePrevButton");
            _eyeNextButton = _root.Q<Button>("EyeNextButton");
            _eyeIdLabel = _root.Q<Label>("EyeIdLabel");
            _eyeColorR = _root.Q<Slider>("EyeColorR");
            _eyeColorG = _root.Q<Slider>("EyeColorG");
            _eyeColorB = _root.Q<Slider>("EyeColorB");

            // Outfit controls
            _outfitPrevButton = _root.Q<Button>("OutfitPrevButton");
            _outfitNextButton = _root.Q<Button>("OutfitNextButton");
            _outfitIdLabel = _root.Q<Label>("OutfitIdLabel");

            // Camera controls
            _cameraLeftButton = _root.Q<Button>("CameraRotateLeftButton");
            _cameraRightButton = _root.Q<Button>("CameraRotateRightButton");

            // デバッグ
            if (_backButton == null)
                Debug.LogWarning("[AvatarCustomizationView] BackButton が見つかりません");
            if (_saveButton == null)
                Debug.LogWarning("[AvatarCustomizationView] SaveButton が見つかりません");
        }

        // ============================================
        // イベントハンドラ登録/解除
        // ============================================

        private void RegisterEventHandlers()
        {
            // ヘッダーボタン
            if (_backButton != null)
                _backButton.clicked += OnBackClicked;
            if (_saveButton != null)
                _saveButton.clicked += OnSaveClicked;
            if (_resetButton != null)
                _resetButton.clicked += OnResetClicked;

            // カテゴリボタン
            if (_hairCategoryButton != null)
                _hairCategoryButton.clicked += () => OnCategoryClicked(CustomizationCategory.Hair);
            if (_eyesCategoryButton != null)
                _eyesCategoryButton.clicked += () => OnCategoryClicked(CustomizationCategory.Eyes);
            if (_outfitCategoryButton != null)
                _outfitCategoryButton.clicked += () => OnCategoryClicked(CustomizationCategory.Outfit);
            if (_accessoryCategoryButton != null)
                _accessoryCategoryButton.clicked += () => OnCategoryClicked(CustomizationCategory.Accessory);

            // Hair controls
            if (_hairPrevButton != null)
                _hairPrevButton.clicked += () => _viewModel?.PreviousHairstyle();
            if (_hairNextButton != null)
                _hairNextButton.clicked += () => _viewModel?.NextHairstyle();

            if (_hairColorR != null)
                _hairColorR.RegisterValueChangedCallback(evt => UpdateHairColor());
            if (_hairColorG != null)
                _hairColorG.RegisterValueChangedCallback(evt => UpdateHairColor());
            if (_hairColorB != null)
                _hairColorB.RegisterValueChangedCallback(evt => UpdateHairColor());

            // Eye controls
            if (_eyePrevButton != null)
                _eyePrevButton.clicked += () => _viewModel?.PreviousEyeType();
            if (_eyeNextButton != null)
                _eyeNextButton.clicked += () => _viewModel?.NextEyeType();

            if (_eyeColorR != null)
                _eyeColorR.RegisterValueChangedCallback(evt => UpdateEyeColor());
            if (_eyeColorG != null)
                _eyeColorG.RegisterValueChangedCallback(evt => UpdateEyeColor());
            if (_eyeColorB != null)
                _eyeColorB.RegisterValueChangedCallback(evt => UpdateEyeColor());

            // Outfit controls
            if (_outfitPrevButton != null)
                _outfitPrevButton.clicked += () => _viewModel?.PreviousOutfitType();
            if (_outfitNextButton != null)
                _outfitNextButton.clicked += () => _viewModel?.NextOutfitType();

            // Camera controls
            if (_cameraLeftButton != null)
                _cameraLeftButton.clicked += () => _viewModel?.RotateCameraLeft();
            if (_cameraRightButton != null)
                _cameraRightButton.clicked += () => _viewModel?.RotateCameraRight();
        }

        private void UnregisterEventHandlers()
        {
            if (_backButton != null)
                _backButton.clicked -= OnBackClicked;
            if (_saveButton != null)
                _saveButton.clicked -= OnSaveClicked;
            if (_resetButton != null)
                _resetButton.clicked -= OnResetClicked;

            // 他のイベントハンドラも同様に解除（簡略化のため省略）
        }

        // ============================================
        // ViewModelバインディング
        // ============================================

        private void BindViewModel()
        {
            if (_viewModel == null) return;

            // 髪型IDの変更を監視
            _viewModel.HairstyleId.Subscribe(id =>
            {
                if (_hairIdLabel != null)
                    _hairIdLabel.text = $"Hairstyle {id}";
            }).AddTo(_disposables);

            // 髪の色の変更を監視
            _viewModel.HairColor.Subscribe(color =>
            {
                if (_hairColorR != null && !_hairColorR.value.Equals(color.r))
                    _hairColorR.value = color.r;
                if (_hairColorG != null && !_hairColorG.value.Equals(color.g))
                    _hairColorG.value = color.g;
                if (_hairColorB != null && !_hairColorB.value.Equals(color.b))
                    _hairColorB.value = color.b;
            }).AddTo(_disposables);

            // 目のIDの変更を監視
            _viewModel.EyeTypeId.Subscribe(id =>
            {
                if (_eyeIdLabel != null)
                    _eyeIdLabel.text = $"Eye Type {id}";
            }).AddTo(_disposables);

            // 目の色の変更を監視
            _viewModel.EyeColor.Subscribe(color =>
            {
                if (_eyeColorR != null && !_eyeColorR.value.Equals(color.r))
                    _eyeColorR.value = color.r;
                if (_eyeColorG != null && !_eyeColorG.value.Equals(color.g))
                    _eyeColorG.value = color.g;
                if (_eyeColorB != null && !_eyeColorB.value.Equals(color.b))
                    _eyeColorB.value = color.b;
            }).AddTo(_disposables);

            // 服のIDの変更を監視
            _viewModel.OutfitTypeId.Subscribe(id =>
            {
                if (_outfitIdLabel != null)
                    _outfitIdLabel.text = $"Outfit {id}";
            }).AddTo(_disposables);

            // カテゴリ変更を監視
            _viewModel.CurrentCategory.Subscribe(category =>
            {
                UpdateCategoryUI(category);
            }).AddTo(_disposables);

            // ViewModelイベントを購読
            _viewModel.OnSave += HandleSave;
            _viewModel.OnReset += HandleReset;
        }

        private void UnbindViewModel()
        {
            if (_viewModel != null)
            {
                _viewModel.OnSave -= HandleSave;
                _viewModel.OnReset -= HandleReset;
            }

            _disposables.Clear();
        }

        // ============================================
        // UI更新
        // ============================================

        private void UpdateCategoryUI(CustomizationCategory category)
        {
            // カテゴリボタンのアクティブ状態を更新
            _hairCategoryButton?.RemoveFromClassList("category-active");
            _eyesCategoryButton?.RemoveFromClassList("category-active");
            _outfitCategoryButton?.RemoveFromClassList("category-active");
            _accessoryCategoryButton?.RemoveFromClassList("category-active");

            switch (category)
            {
                case CustomizationCategory.Hair:
                    _hairCategoryButton?.AddToClassList("category-active");
                    break;
                case CustomizationCategory.Eyes:
                    _eyesCategoryButton?.AddToClassList("category-active");
                    break;
                case CustomizationCategory.Outfit:
                    _outfitCategoryButton?.AddToClassList("category-active");
                    break;
                case CustomizationCategory.Accessory:
                    _accessoryCategoryButton?.AddToClassList("category-active");
                    break;
            }

            Debug.Log($"[AvatarCustomizationView] Category changed to {category}");
        }

        private void UpdateHairColor()
        {
            if (_viewModel == null || _hairColorR == null || _hairColorG == null || _hairColorB == null)
                return;

            Color newColor = new Color(_hairColorR.value, _hairColorG.value, _hairColorB.value);
            _viewModel.HairColor.Value = newColor;
        }

        private void UpdateEyeColor()
        {
            if (_viewModel == null || _eyeColorR == null || _eyeColorG == null || _eyeColorB == null)
                return;

            Color newColor = new Color(_eyeColorR.value, _eyeColorG.value, _eyeColorB.value);
            _viewModel.EyeColor.Value = newColor;
        }

        // ============================================
        // イベントハンドラ
        // ============================================

        private void OnBackClicked()
        {
            Debug.Log("[AvatarCustomizationView] Back button clicked");

            // メインメニューに戻る
            var sceneService = ServiceLocator.Instance.Get<ISceneLoaderService>();
            if (sceneService != null)
            {
                sceneService.LoadMainMenuAsync().Forget();
            }
        }

        private void OnSaveClicked()
        {
            Debug.Log("[AvatarCustomizationView] Save button clicked");
            _viewModel?.HandleSave();
        }

        private void OnResetClicked()
        {
            Debug.Log("[AvatarCustomizationView] Reset button clicked");
            _viewModel?.HandleReset();
        }

        private void OnCategoryClicked(CustomizationCategory category)
        {
            Debug.Log($"[AvatarCustomizationView] Category clicked: {category}");
            _viewModel?.ChangeCategory(category);
        }

        // ============================================
        // ViewModelイベントハンドラ
        // ============================================

        private void HandleSave()
        {
            Debug.Log("[AvatarCustomizationView] Customization saved");
            // 保存完了メッセージ表示など（未実装）
        }

        private void HandleReset()
        {
            Debug.Log("[AvatarCustomizationView] Customization reset to default");
        }
    }
}
