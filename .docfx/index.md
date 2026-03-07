# ShaderOp API リファレンス

**バージョン**: 1.0.0
**最終更新**: 2026-03-01

---

## 📚 概要

ShaderOpは、モバイルゲーム向けのUnity Shader開発プロジェクトです。
このAPI リファレンスでは、プロジェクトのすべての公開APIを検索・参照できます。

---

## 🚀 クイックスタート

### 主要なnamespace

- **[ShaderOp.Core](api/ShaderOp.Core.html)** - コアシステム（GameManager、SceneLoader等）
- **[ShaderOp.Customization](api/ShaderOp.Customization.html)** - キャラクター・部屋カスタマイズ
- **[ShaderOp.Minigames](api/ShaderOp.Minigames.html)** - ミニゲームシステム
- **[ShaderOp.Minigames.HexGrid](api/ShaderOp.Minigames.HexGrid.html)** - ヘックスグリッド基盤

---

## 📖 よく使うクラス

### コアシステム

| クラス | 説明 |
|--------|------|
| [GameManager](api/ShaderOp.Core.GameManager.html) | ゲーム全体を管理するシングルトン |
| [SceneLoader](api/ShaderOp.Core.SceneLoader.html) | シーン遷移管理（UniTask対応） |
| [InputManager](api/ShaderOp.Core.InputManager.html) | 入力処理管理 |
| [SettingsManager](api/ShaderOp.Core.SettingsManager.html) | 設定管理 |

### カスタマイズシステム

| クラス | 説明 |
|--------|------|
| [CharacterCustomizer](api/ShaderOp.Customization.CharacterCustomizer.html) | キャラクターカスタマイズ制御 |
| [MaterialController](api/ShaderOp.Customization.MaterialController.html) | マテリアル管理 |
| [RoomDecorator](api/ShaderOp.Customization.RoomDecorator.html) | 部屋デコレーション管理 |

### ヘックスグリッド

| クラス | 説明 |
|--------|------|
| [HexCoordinate](api/ShaderOp.Minigames.HexGrid.HexCoordinate.html) | 六角座標系（Axial座標） |
| [HexGrid](api/ShaderOp.Minigames.HexGrid.HexGrid.html) | ヘックスグリッド生成・管理 |
| [HexTile](api/ShaderOp.Minigames.HexGrid.HexTile.html) | ヘックスタイル基底クラス |

### ミニゲーム

| クラス | 説明 |
|--------|------|
| [HexReversiModel](api/ShaderOp.Minigames.Games.HexReversiModel.html) | Reversiゲームロジック |
| [HexCheckersModel](api/ShaderOp.Minigames.Games.HexCheckersModel.html) | Checkersゲームロジック |
| [HexChessModel](api/ShaderOp.Minigames.Games.HexChessModel.html) | Chessゲームロジック |

---

## 🎓 使用例

### シーンロード（非同期）

```csharp
using ShaderOp.Core;
using Cysharp.Threading.Tasks;

public async UniTask LoadGameScene()
{
    await SceneLoader.Instance.LoadSceneAsync("MainGame");
}
```

### キャラクターカスタマイズ

```csharp
using ShaderOp.Customization;
using UnityEngine;

public class CustomizationExample : MonoBehaviour
{
    [SerializeField] private CharacterCustomizer _customizer;

    void Start()
    {
        // 髪色を変更
        _customizer.SetHairColor(new Color(0.8f, 0.2f, 0.2f));

        // 肌色を変更
        _customizer.SetSkinTone(2);
    }
}
```

### ヘックスグリッドの使用

```csharp
using ShaderOp.Minigames.HexGrid;

public class GridExample
{
    void CreateGrid()
    {
        // 3x3のヘックスグリッドを作成
        var grid = new HexGrid(HexGridShape.Hexagon, 3);

        // 座標から距離を計算
        var coord1 = new HexCoordinate(0, 0);
        var coord2 = new HexCoordinate(2, 1);
        int distance = coord1.DistanceTo(coord2); // 結果: 3
    }
}
```

---

## 📂 名前空間一覧

- **ShaderOp.Core** - コアシステム
- **ShaderOp.Customization** - カスタマイズシステム
- **ShaderOp.Minigames** - ミニゲームシステム
- **ShaderOp.Minigames.HexGrid** - ヘックスグリッド基盤
- **ShaderOp.Minigames.Games** - ゲームロジック
- **ShaderOp.Shaders** - シェーダー制御
- **ShaderOp.UI** - UIシステム

---

## 🔗 関連ドキュメント

- [プロジェクトREADME](../README.md)
- [クイックスタートガイド](../docs/GETTING_STARTED.md)
- [ベストプラクティス](../docs/BEST_PRACTICES.md)
- [実装ステータス](../docs/IMPLEMENTATION_STATUS.md)

---

## 📝 ライセンス

MIT License

---

**ドキュメント生成日**: 2026-03-01
**DocFX バージョン**: 2.x
