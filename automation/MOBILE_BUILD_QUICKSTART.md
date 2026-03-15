# モバイルビルド自動化 - クイックスタートガイド

**最短5分でAndroid APKをビルド**

---

## 1分でわかる使い方

```bash
# ステップ1: 設定ファイルを生成
python build_mobile.py --generate-config build_config.json

# ステップ2: ビルド実行
python build_mobile.py --config build_config.json

# 完了！ builds/Android/ShaderOp_*.apk が生成されます
```

---

## よく使うコマンド

### 設定ファイル生成

```bash
python build_mobile.py --generate-config android_dev.json
```

### Android Development ビルド

```bash
python build_mobile.py --config android_dev.json
```

### ドライラン（実行せずに確認）

```bash
python build_mobile.py --config android_dev.json --dry-run
```

### Unity パスを手動指定

```bash
python build_mobile.py --config android_dev.json --unity-path "C:\Program Files\Unity\Hub\Editor\2022.3.10f1\Editor\Unity.exe"
```

---

## 設定ファイル編集（必要に応じて）

**build_config.json**:

```json
{
  "platform": "Android",        // "Android" or "iOS"
  "buildType": "Development",   // "Development" or "Release"
  "version": "0.4.0",           // アプリバージョン
  "scriptingBackend": "IL2CPP"  // "IL2CPP" or "Mono"
}
```

その他の設定項目は自動で最適化されます。

---

## ビルド成果物の確認

```
builds/
├── Android/
│   └── ShaderOp_0.4.0_Development_20260315.apk  ← これをインストール
├── build_report_Android_20260315_143022.json
└── build_report_Android_20260315_143022.md      ← レポート確認
```

---

## デバイスにインストール

```bash
# ADB経由でインストール
adb install -r builds/Android/ShaderOp_*.apk

# アプリ起動
adb shell am start -n com.shaderop.mobile/.MainActivity
```

---

## トラブルシューティング

### Unity が見つからない

```bash
# 環境変数で指定
set UNITY_PATH=C:\Program Files\Unity\Hub\Editor\2022.3.10f1\Editor\Unity.exe
```

### ビルドが失敗する

```bash
# ログファイルを確認
type builds\build_Android_*.log
```

### APK サイズが大きすぎる

設定ファイルで `"managedStripping": "High"` に変更

---

## 詳細ドキュメント

- **完全ガイド**: `automation/MOBILE_BUILD_README.md`
- **検証テンプレート**: `MOBILE_BUILD_REPORT.md`
- **完了レポート**: `TASK4_MOBILE_BUILD_COMPLETE.md`

---

**以上！シンプルで強力なモバイルビルド自動化をお楽しみください。**
