#nullable enable

using System;
using UniRx;
using UnityEngine;

namespace ShaderOp.UI.ViewModels
{
    /// <summary>
    /// カスタマイズカテゴリ
    /// </summary>
    public enum CustomizationCategory
    {
        Hair,
        Eyes,
        Outfit,
        Accessory
    }

    /// <summary>
    /// アバターカスタマイズ画面のViewModel
    /// </summary>
    public class AvatarCustomizationViewModel : IDisposable
    {
        private readonly CompositeDisposable _disposables = new();

        /// <summary>現在選択中のカテゴリ</summary>
        public ReactiveProperty<CustomizationCategory> CurrentCategory { get; } = new(CustomizationCategory.Hair);

        /// <summary>髪の色（RGB）</summary>
        public ReactiveProperty<Color> HairColor { get; } = new(Color.black);

        /// <summary>目の色（RGB）</summary>
        public ReactiveProperty<Color> EyeColor { get; } = new(new Color(0.3f, 0.3f, 1f));

        /// <summary>服の色（RGB）</summary>
        public ReactiveProperty<Color> OutfitColor { get; } = new(Color.white);

        /// <summary>髪型ID</summary>
        public ReactiveProperty<int> HairstyleId { get; } = new(0);

        /// <summary>髪型の最大数</summary>
        public int MaxHairstyles { get; } = 15;

        /// <summary>目のタイプID</summary>
        public ReactiveProperty<int> EyeTypeId { get; } = new(0);

        /// <summary>目のタイプの最大数</summary>
        public int MaxEyeTypes { get; } = 10;

        /// <summary>服のタイプID</summary>
        public ReactiveProperty<int> OutfitTypeId { get; } = new(0);

        /// <summary>服のタイプの最大数</summary>
        public int MaxOutfitTypes { get; } = 20;

        /// <summary>カメラ回転角度</summary>
        public ReactiveProperty<float> CameraRotation { get; } = new(0f);

        /// <summary>カメラズーム</summary>
        public ReactiveProperty<float> CameraZoom { get; } = new(1f);

        /// <summary>カテゴリ変更イベント</summary>
        public event Action<CustomizationCategory>? OnCategoryChanged;

        /// <summary>プレビュー更新イベント</summary>
        public event Action? OnPreviewUpdate;

        /// <summary>保存イベント</summary>
        public event Action? OnSave;

        /// <summary>リセットイベント</summary>
        public event Action? OnReset;

        public AvatarCustomizationViewModel()
        {
            // カラー変更を監視してプレビュー更新
            HairColor.Subscribe(_ => UpdatePreview()).AddTo(_disposables);
            EyeColor.Subscribe(_ => UpdatePreview()).AddTo(_disposables);
            OutfitColor.Subscribe(_ => UpdatePreview()).AddTo(_disposables);

            // ID変更を監視
            HairstyleId.Subscribe(_ => UpdatePreview()).AddTo(_disposables);
            EyeTypeId.Subscribe(_ => UpdatePreview()).AddTo(_disposables);
            OutfitTypeId.Subscribe(_ => UpdatePreview()).AddTo(_disposables);
        }

        /// <summary>
        /// カテゴリを変更
        /// </summary>
        public void ChangeCategory(CustomizationCategory category)
        {
            if (CurrentCategory.Value == category) return;

            CurrentCategory.Value = category;
            Debug.Log($"[AvatarCustomizationViewModel] Category changed to {category}");
            OnCategoryChanged?.Invoke(category);
        }

        /// <summary>
        /// 髪型を次へ
        /// </summary>
        public void NextHairstyle()
        {
            HairstyleId.Value = (HairstyleId.Value + 1) % MaxHairstyles;
        }

        /// <summary>
        /// 髪型を前へ
        /// </summary>
        public void PreviousHairstyle()
        {
            HairstyleId.Value = (HairstyleId.Value - 1 + MaxHairstyles) % MaxHairstyles;
        }

        /// <summary>
        /// 目のタイプを次へ
        /// </summary>
        public void NextEyeType()
        {
            EyeTypeId.Value = (EyeTypeId.Value + 1) % MaxEyeTypes;
        }

        /// <summary>
        /// 目のタイプを前へ
        /// </summary>
        public void PreviousEyeType()
        {
            EyeTypeId.Value = (EyeTypeId.Value - 1 + MaxEyeTypes) % MaxEyeTypes;
        }

        /// <summary>
        /// 服のタイプを次へ
        /// </summary>
        public void NextOutfitType()
        {
            OutfitTypeId.Value = (OutfitTypeId.Value + 1) % MaxOutfitTypes;
        }

        /// <summary>
        /// 服のタイプを前へ
        /// </summary>
        public void PreviousOutfitType()
        {
            OutfitTypeId.Value = (OutfitTypeId.Value - 1 + MaxOutfitTypes) % MaxOutfitTypes;
        }

        /// <summary>
        /// カメラを左回転
        /// </summary>
        public void RotateCameraLeft()
        {
            CameraRotation.Value -= 15f;
        }

        /// <summary>
        /// カメラを右回転
        /// </summary>
        public void RotateCameraRight()
        {
            CameraRotation.Value += 15f;
        }

        /// <summary>
        /// プリセットカラーを適用
        /// </summary>
        public void ApplyPresetColor(int presetIndex)
        {
            Color[] hairPresets = new[]
            {
                Color.black,
                new Color(0.6f, 0.4f, 0.2f), // Brown
                new Color(1f, 0.9f, 0.2f),    // Blonde
                Color.red
            };

            if (CurrentCategory.Value == CustomizationCategory.Hair && presetIndex < hairPresets.Length)
            {
                HairColor.Value = hairPresets[presetIndex];
            }
        }

        /// <summary>
        /// プレビュー更新
        /// </summary>
        private void UpdatePreview()
        {
            Debug.Log("[AvatarCustomizationViewModel] Preview updated");
            OnPreviewUpdate?.Invoke();
        }

        /// <summary>
        /// 保存処理
        /// </summary>
        public void HandleSave()
        {
            Debug.Log("[AvatarCustomizationViewModel] Saving customization...");
            // TODO: 実際のサービスに保存
            OnSave?.Invoke();
        }

        /// <summary>
        /// リセット処理
        /// </summary>
        public void HandleReset()
        {
            Debug.Log("[AvatarCustomizationViewModel] Resetting to default...");
            HairColor.Value = Color.black;
            EyeColor.Value = new Color(0.3f, 0.3f, 1f);
            OutfitColor.Value = Color.white;
            HairstyleId.Value = 0;
            EyeTypeId.Value = 0;
            OutfitTypeId.Value = 0;
            CameraRotation.Value = 0f;
            CameraZoom.Value = 1f;

            OnReset?.Invoke();
        }

        public void Dispose()
        {
            _disposables?.Dispose();
        }
    }
}
