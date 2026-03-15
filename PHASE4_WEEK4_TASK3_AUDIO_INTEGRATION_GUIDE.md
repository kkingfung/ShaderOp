# Phase 4 Week 4 Task 3 - Audio Asset Integration Guide

**Date**: 2026-03-15  
**Phase**: Phase 4 Week 4 - Task 3  
**Status**: Implementation Guide  
**Agent**: unity-developer

---

## Executive Summary

このドキュメントは、UIButtonSoundPlayerで使用するボタンサウンド（hover, click）の作成/取得/統合手順を提供します。

**目標**: プロフェッショナルなUIサウンドフィードバックの実装

**必要なオーディオ**:
1. `button_hover.wav` - ホバー時のサウンド（50-100ms）
2. `button_click.wav` - クリック時のサウンド（100-200ms）

---

## 1. オーディオ要件

### 1.1 技術仕様

| 項目 | button_hover.wav | button_click.wav |
|------|-----------------|-----------------|
| **フォーマット** | WAV (推奨) / OGG | WAV (推奨) / OGG |
| **サンプルレート** | 44.1kHz | 44.1kHz |
| **ビット深度** | 16-bit | 16-bit |
| **チャンネル** | Mono (推奨) | Mono (推奨) |
| **長さ** | 50-100ms | 100-200ms |
| **ファイルサイズ** | <50KB | <50KB |
| **音量** | -6dB ~ -12dB | -3dB ~ -9dB |

### 1.2 サウンドデザイン指針

**button_hover.wav** (ホバー音):
- **特徴**: 軽快で控えめ、高周波数（2-5kHz）
- **目的**: マウスホバー時のフィードバック（視覚的補助）
- **音量**: やや小さめ（-12dB推奨）
- **例**: 「シュッ」「ピッ」「ティン」

**button_click.wav** (クリック音):
- **特徴**: はっきりとした中周波数（1-3kHz）、短いアタック
- **目的**: クリック確定時のフィードバック（行動確認）
- **音量**: やや大きめ（-6dB推奨）
- **例**: 「カチッ」「ポン」「タン」

---

## 2. オーディオ取得方法

### 方法1: フリー素材サイトから取得（推奨）

#### Kenney.nl (パブリックドメイン)

**URL**: https://kenney.nl/assets/ui-audio

**手順**:
1. 上記URLにアクセス
2. "Download" をクリック
3. `ui-audio.zip` をダウンロード
4. 解凍して以下のファイルを選択:
   - **Hover音候補**: `click1.ogg`, `switch1.ogg`, `switch3.ogg`
   - **Click音候補**: `click2.ogg`, `switch2.ogg`, `confirmation_002.ogg`
5. 選択したファイルを `Assets/Audio/UI/` にコピー
6. ファイル名を変更:
   - `click1.ogg` → `button_hover.wav` (WAV変換後)
   - `click2.ogg` → `button_click.wav` (WAV変換後)

**ライセンス**: CC0 (パブリックドメイン)、商用利用可能、クレジット不要

---

#### Freesound.org (クリエイティブ・コモンズ)

**URL**: https://freesound.org/

**検索キーワード**:
- "UI button click"
- "button hover"
- "interface click"
- "menu beep"

**フィルタ設定**:
- License: CC0 または CC BY
- Duration: 0-1 seconds
- Channels: Mono
- Samplerate: 44100Hz

**手順**:
1. 上記キーワードで検索
2. プレビュー試聴して適切なサウンドを選択
3. Download をクリック（要ログイン）
4. `Assets/Audio/UI/` にコピー

**ライセンス**: CC0またはCC BY（クレジット表記必要な場合あり）

---

### 方法2: Unity AudioClip.Createで生成（プレースホルダー）

**シンプルなビープ音を生成** (コード例):

```csharp
using UnityEngine;

public class AudioPlaceholderGenerator : MonoBehaviour
{
    [ContextMenu("Generate Placeholder Audio")]
    public void GeneratePlaceholderAudio()
    {
        // Hover音生成（高周波数、短い）
        AudioClip hoverClip = CreateBeepSound("button_hover", 0.05f, 3000f, 0.3f);
        SaveAudioClip(hoverClip, "Assets/Audio/UI/button_hover.wav");

        // Click音生成（中周波数、やや長い）
        AudioClip clickClip = CreateBeepSound("button_click", 0.1f, 1500f, 0.5f);
        SaveAudioClip(clickClip, "Assets/Audio/UI/button_click.wav");

        Debug.Log("[AudioPlaceholderGenerator] Placeholder audio created!");
    }

    private AudioClip CreateBeepSound(string name, float duration, float frequency, float volume)
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * duration);
        AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);

        float[] samples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float wave = Mathf.Sin(2 * Mathf.PI * frequency * t);
            
            // エンベロープ（フェードアウト）
            float envelope = 1f - (float)i / sampleCount;
            samples[i] = wave * volume * envelope;
        }

        clip.SetData(samples, 0);
        return clip;
    }

    private void SaveAudioClip(AudioClip clip, string path)
    {
        // Unity Editorでは SavWav.Save() などのアセットストア拡張が必要
        // または、以下のライブラリを使用:
        // https://gist.github.com/darktable/2317063

        Debug.LogWarning($"[AudioPlaceholderGenerator] Manual save required: {path}");
        // 実装が必要な場合は SavWav アセットをインポート
    }
}
```

**注意**: Unity標準ではAudioClipをWAVファイルとして保存できないため、外部ライブラリ（SavWav等）が必要です。

**推奨**: プレースホルダーとしては方法1のフリー素材使用が簡単です。

---

### 方法3: Audacity等で自作

**手順**:

1. **Audacityをダウンロード**: https://www.audacityteam.org/
2. **Hover音作成**:
   - Generate → Tone
   - Frequency: 3000Hz
   - Amplitude: 0.3
   - Duration: 0.05s
   - Apply → Fade Out (最後の0.02s)
   - Export → Export as WAV (16-bit PCM)
3. **Click音作成**:
   - Generate → Tone
   - Frequency: 1500Hz
   - Amplitude: 0.5
   - Duration: 0.1s
   - Apply → Fade Out (最後の0.03s)
   - Export → Export as WAV (16-bit PCM)

---

## 3. Unity統合手順

### 3.1 ファイル配置

1. **フォルダ作成** (すでに作成済み):
   ```
   Assets/Audio/UI/
   ```

2. **ファイルコピー**:
   - `button_hover.wav` → `Assets/Audio/UI/button_hover.wav`
   - `button_click.wav` → `Assets/Audio/UI/button_click.wav`

3. **Unity Editor で確認**:
   - Project Windowで `Assets/Audio/UI/` を開く
   - 2つのAudioClipが表示されることを確認

---

### 3.2 AudioClip Import設定

**各AudioClipを選択してInspectorで設定**:

#### button_hover.wav 設定

```
Force To Mono: ✓ (チェック)
Load In Background: ✗ (チェック外す - 小さいファイルなので即座にロード)
Preload Audio Data: ✓ (チェック)
Load Type: Decompress On Load (メモリ効率)
Compression Format: PCM (品質優先)
Sample Rate Setting: Preserve Sample Rate
Quality: 100 (最高品質)
```

#### button_click.wav 設定

```
Force To Mono: ✓
Load In Background: ✗
Preload Audio Data: ✓
Load Type: Decompress On Load
Compression Format: PCM
Sample Rate Setting: Preserve Sample Rate
Quality: 100
```

**Apply** をクリックして設定を保存。

---

### 3.3 UIButtonSoundPlayerへの設定

**各シーンで実行**:

1. **UIButtonSoundPlayerコンポーネントを持つGameObjectを選択**
2. **Inspector設定**:
   ```
   Hover Sound: Assets/Audio/UI/button_hover.wav (ドラッグ&ドロップ)
   Click Sound: Assets/Audio/UI/button_click.wav (ドラッグ&ドロップ)
   Volume: 1.0
   Enable Hover Sound: ✓ (PC), ✗ (Mobile)
   Use Audio Manager: ✓
   ```
3. **Apply** をクリック

**対象シーン**:
- MainMenu.unity
- MainCustomization.unity
- TicTacToeHex.unity
- HexReversi.unity
- HexCheckers.unity
- HexChess.unity
- RoomDecoration.unity

---

## 4. 動作テスト

### 4.1 Play Modeテスト

**各シーンで実行**:

1. Play Mode開始
2. **ホバーテスト** (PC):
   - マウスをボタンにホバー
   - `button_hover.wav` が再生されることを確認
   - 音量が適切か確認（うるさすぎないか）
3. **クリックテスト**:
   - ボタンをクリック
   - `button_click.wav` が再生されることを確認
   - クリック確定感があるか確認
4. **連続クリックテスト**:
   - ボタンを連続クリック（5回/秒）
   - サウンドが重複再生されることを確認（AudioSource.PlayClipAtPoint使用時）
   - 音割れがないか確認
5. **Consoleエラー確認**:
   - `NullReferenceException` がないことを確認
   - AudioClip missing警告がないことを確認

---

### 4.2 モバイルテスト（オプション）

**Unity Remote または実機ビルドで確認**:

1. `Enable Hover Sound` を **OFF** に設定（モバイル用）
2. タッチでボタンをタップ
3. `button_click.wav` のみ再生されることを確認
4. ホバー音が再生されないことを確認

---

## 5. 音量調整

### 5.1 音量バランス

**推奨設定**:
```csharp
// UIButtonSoundPlayer Inspector
Volume: 0.7 - 1.0 (全体音量)

// 個別調整が必要な場合
button_hover.wav: 0.5 - 0.7 (控えめ)
button_click.wav: 0.8 - 1.0 (はっきり)
```

**調整手順**:

1. Play Modeで実際に再生
2. 音量が適切か判断:
   - **大きすぎる**: UIButtonSoundPlayer の Volume を 0.5 に下げる
   - **小さすぎる**: Volume を 1.0 に上げる、またはAudacityで音源を増幅
3. ホバー音とクリック音のバランスを調整:
   - ホバー音はクリック音の 60-70% の音量が適切

---

### 5.2 Audacityでの音量調整

**手順**:

1. Audacityで `button_hover.wav` を開く
2. Effect → Amplify
3. New Peak Amplitude: **-12dB** (ホバー音は控えめ)
4. OK → Export as WAV (上書き)
5. `button_click.wav` も同様に **-6dB** に設定

---

## 6. オーディオマネージャー統合（オプション）

### 6.1 AudioManager Service作成

**より高度な制御が必要な場合**:

```csharp
using UnityEngine;

public interface IAudioManagerService
{
    void PlayOneShotSFX(AudioClip clip, float volume);
    void SetSFXVolume(float volume);
    void SetMasterVolume(float volume);
}

public class AudioManagerService : MonoBehaviour, IAudioManagerService
{
    private AudioSource? _sfxAudioSource;
    private float _sfxVolume = 1.0f;
    private float _masterVolume = 1.0f;

    private void Awake()
    {
        _sfxAudioSource = gameObject.AddComponent<AudioSource>();
        _sfxAudioSource.playOnAwake = false;
    }

    public void PlayOneShotSFX(AudioClip clip, float volume)
    {
        if (_sfxAudioSource != null && clip != null)
        {
            float finalVolume = volume * _sfxVolume * _masterVolume;
            _sfxAudioSource.PlayOneShot(clip, finalVolume);
        }
    }

    public void SetSFXVolume(float volume)
    {
        _sfxVolume = Mathf.Clamp01(volume);
    }

    public void SetMasterVolume(float volume)
    {
        _masterVolume = Mathf.Clamp01(volume);
    }
}
```

**UIButtonSoundPlayerでの使用**:
```csharp
private void PlaySound(AudioClip? clip)
{
    if (clip == null) return;

    if (_audioManager != null && _useAudioManager)
    {
        _audioManager.PlayOneShotSFX(clip, _volume);
    }
    else
    {
        Vector3 position = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
        AudioSource.PlayClipAtPoint(clip, position, _volume);
    }
}
```

---

## 7. トラブルシューティング

### 問題1: サウンドが再生されない

**原因候補**:
1. AudioClipがInspectorに設定されていない
2. AudioListenerがシーンに存在しない
3. Volumeが0になっている
4. AudioClipのファイル破損

**解決策**:
1. Inspector確認: Hover Sound / Click Sound が **None (Audio Clip)** でないこと
2. Main CameraにAudioListenerコンポーネントがあることを確認
3. Volume を 1.0 に設定
4. AudioClipを再インポート（Project Window → 右クリック → Reimport）

---

### 問題2: 音割れ・ノイズが発生する

**原因候補**:
1. 音量が大きすぎる（クリッピング）
2. 複数のサウンドが同時再生されている
3. サンプルレートの不一致

**解決策**:
1. Volumeを0.5に下げる、またはAudacityで-6dB減衰
2. AudioSource.PlayClipAtPoint は複数同時再生可能（問題なし）
3. AudioClip Import設定で "Preserve Sample Rate" を確認

---

### 問題3: モバイルでホバー音が再生される

**原因**: `Enable Hover Sound` が ON のまま

**解決策**:
```csharp
// UIButtonSoundPlayer.cs の Awake() に追加
#if UNITY_IOS || UNITY_ANDROID
    _enableHoverSound = false;
#endif
```

または、Inspector で手動で OFF に設定。

---

## 8. 完了チェックリスト

### Task 3完了条件

- [ ] **オーディオファイル取得**:
  - [ ] `button_hover.wav` 取得/作成完了
  - [ ] `button_click.wav` 取得/作成完了
  - [ ] ファイルサイズ確認（各<50KB）
  - [ ] 長さ確認（hover: 50-100ms, click: 100-200ms）

- [ ] **Unity統合**:
  - [ ] `Assets/Audio/UI/` に配置
  - [ ] AudioClip Import設定完了
  - [ ] 7シーンすべてでInspector設定完了

- [ ] **動作テスト**:
  - [ ] ホバー音再生確認（PC）
  - [ ] クリック音再生確認
  - [ ] 音量バランス調整完了
  - [ ] 連続クリックテスト完了
  - [ ] Consoleエラーなし

- [ ] **モバイル対応**:
  - [ ] `Enable Hover Sound` をモバイルで OFF
  - [ ] タッチでクリック音のみ再生確認

---

## 9. 推奨オーディオソース

### Kenney.nl UI Audio Pack (推奨)

**URL**: https://kenney.nl/assets/ui-audio

**内容**:
- 38種類のUIサウンド（click, switch, hover, confirmation等）
- WAV, OGG形式
- パブリックドメイン（CC0）
- 商用利用可能、クレジット不要

**推奨ファイル**:
- **Hover**: `click1.ogg`, `switch1.ogg`, `rollover1.ogg`
- **Click**: `click2.ogg`, `confirmation_002.ogg`, `switch2.ogg`

---

### Freesound.org 検索例

**URL**: https://freesound.org/search/?q=ui+button

**フィルタ**:
- License: CC0
- Duration: 0-1s
- Pack: "UI SFX" で検索

**人気パック**:
- "Interface Sounds Starter Pack" by Lokif
- "UI Essential Pack" by HenryRichard

---

## 10. 次のステップ

Task 3完了後、Task 4 (Mobile Build Creation) に進む:
- Android SDK設定
- IL2CPP バックエンド設定
- Build Automation Script作成
- APK生成

---

**END OF DOCUMENT**
