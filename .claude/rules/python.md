# Python ルール

## コーディングスタイル

✅ **必須**: PEP8に準拠
```python
# ✅ Good
def calculate_total(items: list) -> int:
    """合計を計算します"""
    return sum(item.price for item in items)

# ❌ Bad
def calculateTotal(items):  # キャメルケース、型ヒントなし
    return sum([item.price for item in items])
```

## 型ヒント

✅ **必須**: すべての関数に型ヒントを記述
```python
from typing import List, Dict, Optional

def validate_assets(paths: List[str]) -> Dict[str, bool]:
    """
    アセットを検証します

    Args:
        paths: 検証するファイルパスのリスト

    Returns:
        検証結果の辞書
    """
    results = {}
    for path in paths:
        results[path] = is_valid(path)
    return results
```

## ファイル操作

✅ **DO**: Pathlibを使用
```python
from pathlib import Path

# ✅ Good
project_dir = Path('UnityProject')
assets_dir = project_dir / 'Assets'

# ❌ Bad
import os
assets_dir = os.path.join('UnityProject', 'Assets')
```

## エラーハンドリング

✅ **DO**:
```python
import logging

logger = logging.getLogger(__name__)

try:
    result = process_file(path)
except FileNotFoundError:
    logger.error(f'ファイルが見つかりません: {path}')
except Exception as e:
    logger.exception('予期しないエラー')
```

❌ **DON'T**:
```python
try:
    result = process_file(path)
except:  # ❌ 広すぎる例外キャッチ
    pass  # ❌ エラーを無視
```

## パフォーマンス

- リスト内包表記を活用
- ジェネレータで大量データ処理
- 不要なグローバル変数を避ける
