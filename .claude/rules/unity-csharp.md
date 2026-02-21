# Unity C# ルール

## 命名規則

```csharp
// ✅ Good
private int _health;              // フィールド: _camelCase
public int MaxHealth { get; set; } // プロパティ: PascalCase
public async UniTask LoadAsync()   // 非同期: ...Async
```

## Unity API使用

✅ **DO**:
```csharp
// Null安全チェック
if (obj != null) { }

// コンポーネント取得はAwake/Startで
private Renderer _renderer;
void Awake() {
    _renderer = GetComponent<Renderer>();
}
```

❌ **DON'T**:
```csharp
// Update内でGetComponent（パフォーマンス低下）
void Update() {
    var renderer = GetComponent<Renderer>(); // ❌
}

// FindObjectOfTypeの多用
GameObject.FindObjectOfType<Player>(); // ❌
```

## 非同期処理（UniTask）

✅ **必須**:
```csharp
// async UniTask を使用（async void は禁止）
public async UniTask LoadDataAsync()
{
    await LoadFromServerAsync();
}
```

## メモリ管理

✅ **DO**:
- オブジェクトプール使用
- LINQは適度に（GC Allocに注意）
- Dispose パターン実装

❌ **DON'T**:
- Update内でのnew（GC Alloc）
- メモリリークの放置

## UniRx

```csharp
// ✅ Good: AddToでライフサイクル管理
Observable.EveryUpdate()
    .Subscribe(_ => DoSomething())
    .AddTo(this);

// ❌ Bad: Subscribeしたまま放置
Observable.EveryUpdate()
    .Subscribe(_ => DoSomething()); // メモリリーク
```
