# ShaderOp - Unity Shader開発プロジェクト

## プロジェクト概要

Unity向けのシェーダー開発と自動化ツールプロジェクト。キャラクターカスタマイズ系モバイルゲーム向け。

**技術スタック**: Unity Shader Graph, HLSL, C# (UniTask/UniRx), Python, Jenkins

## コーディング規約

### すべてのコメントは日本語で記述

```csharp
/// <summary>
/// マルチレイヤーシェーダーの制御クラス
/// </summary>
public class MultiLayerShader : MonoBehaviour
{
    /// <summary>ベースカラー</summary>
    private Color _baseColor;

    /// <summary>
    /// マテリアルを更新します
    /// </summary>
    public async UniTask UpdateMaterialAsync()
    {
        // テクスチャを非同期で読み込み
        var texture = await LoadTextureAsync();
        _material.SetTexture("_MainTex", texture);
    }
}
```

### 命名規則

**C# (Unity)**
- フィールド: `_camelCase`
- プロパティ/メソッド: `PascalCase`
- 非同期メソッド: `...Async`

**シェーダー**
- Shader Graph: `SG_機能名_用途` (例: `SG_Cloth_Satin`)
- HLSL: `機能名.shader` (例: `MultiLayerClothing.shader`)

**Python**
- ファイル名: `snake_case.py`
- クラス名: `PascalCase`
- 関数名: `snake_case`

## 利用可能なエージェント

### プロジェクト管理
```
project-planner      - プロジェクト計画・要件整理・進捗管理（対話型）
```

### 開発・設計
```
architect            - アーキテクチャ設計・機能計画・タスク分解
unity-developer      - Unity C#開発・UI Toolkit・Addressables
shader-dev           - シェーダー開発・レビュー・最適化
automation-dev       - Python/JS自動化ツール開発
ui-ux-designer       - UI/UX技術設計・デザインシステム・アクセシビリティ
```

### 品質・最適化
```
code-reviewer        - コード品質レビュー・リファクタリング提案
test-engineer        - Unity Test Framework・TDD・テスト設計
performance-analyzer - パフォーマンス分析・UniTask/UniRx最適化
security-auditor     - セキュリティ監査・脆弱性検出
```

### インフラ・ドキュメント
```
cicd-helper          - Jenkins/CI/CD支援・パイプライン構築
doc-writer           - 技術ドキュメント作成
```

## エージェントの使い方

**`/agents` コマンドでエージェント一覧を表示**:
```
/agents
```

**エージェントを呼び出す**:
```
「shader-devエージェントでサテンシェーダーを作成してください」
「project-plannerエージェントとプロジェクトを計画したいです」
「code-reviewerエージェントでコードをレビューしてください」
```

Claudeが自動的にTaskツールで適切なエージェントを起動します。

## 利用可能なスキル

`.claude/skills/` に各種開発パターンを格納:

### Unity開発
- `shader-development.md` - Shader Graph/HLSL開発パターン
- `unitask-patterns.md` - UniTask非同期処理パターン
- `unirx-patterns.md` - UniRxリアクティブプログラミング
- `asset-optimization.md` - テクスチャ/メッシュ/Addressables最適化
- `performance-profiling.md` - Unity Profilerとパフォーマンス最適化
- `unity-test-framework.md` - Unity Test Framework・TDD・モック/スタブ
- `ui-toolkit-patterns.md` - UI Toolkit・UXML/USS・データバインディング

### アーキテクチャ・設計
- `architectural-patterns.md` - SOLID原則・デザインパターン（Factory、Observer、Repository等）

### 自動化・CI/CD
- `python-automation.md` - Pythonアセット検証・自動化
- `jenkins-cicd.md` - Jenkins CI/CDパイプライン・デプロイ自動化

## Hooksシステム

セッション開始/終了時に自動実行:
- セキュリティチェック（機密ファイル、危険なコマンド）
- 自動バックアップ作成
- コーディングパターン抽出
- 統計記録とレポート生成

## 参考リソース

- Unity Shader Graph: https://docs.unity3d.com/Packages/com.unity.shadergraph@latest
- HLSL: https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl
- UniTask: https://github.com/Cysharp/UniTask
- UniRx: https://github.com/neuecc/UniRx

---
最終更新: 2026-02-21
