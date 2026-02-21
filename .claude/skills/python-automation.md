# Python 自動化パターン

## 概要
Unityアセット検証と自動化のためのPythonパターン。

## アセット検証

```python
from pathlib import Path
from PIL import Image

def validate_texture(texture_path: str) -> dict:
    """
    テクスチャを検証します

    Returns:
        検証結果の辞書
    """
    result = {'valid': True, 'errors': []}

    # 画像を読み込み
    img = Image.open(texture_path)
    width, height = img.size

    # サイズチェック
    if width > 2048 or height > 2048:
        result['valid'] = False
        result['errors'].append(f'サイズ超過: {width}x{height}')

    # 2のべき乗チェック
    if not is_power_of_two(width) or not is_power_of_two(height):
        result['valid'] = False
        result['errors'].append('2のべき乗ではありません')

    return result

def is_power_of_two(n: int) -> bool:
    """2のべき乗かチェック"""
    return n > 0 and (n & (n - 1)) == 0
```

## ファイル操作

```python
from pathlib import Path

# Pathlibを使用（os.pathより推奨）
project_dir = Path('UnityProject/Assets')

# 全テクスチャを検索
for texture in project_dir.glob('**/*.png'):
    validate_texture(str(texture))
```

## エラーハンドリング

```python
import logging

# ロギング設定
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s'
)

logger = logging.getLogger(__name__)

try:
    result = validate_texture(path)
except FileNotFoundError:
    logger.error(f'ファイルが見つかりません: {path}')
except Exception as e:
    logger.exception(f'予期しないエラー: {e}')
```

## ベストプラクティス

✅ **DO**:
- Pathlibを使用
- 型ヒントを記述
- ロギングを活用

❌ **DON'T**:
- ハードコードされたパス
- 例外の無視
- グローバル変数の多用

信頼度: 0.90
