# UniRx パターン

## 概要
UniRxを使用したリアクティブプログラミングのベストプラクティス。

## 基本パターン

```csharp
using UniRx;

// ReactivePropertyを使用したプロパティ変更通知
public class CharacterData
{
    public ReactiveProperty<int> Health { get; } = new ReactiveProperty<int>(100);
    public ReactiveProperty<string> EquipmentId { get; } = new ReactiveProperty<string>("");
}

// 購読
_character.Health
    .Subscribe(health => Debug.Log($"体力: {health}"))
    .AddTo(this);
```

## イベント処理

```csharp
// ReactiveCommandでボタンイベントを扱う
public class UIController : MonoBehaviour
{
    private Subject<Unit> _onButtonClicked = new Subject<Unit>();
    public IObservable<Unit> OnButtonClicked => _onButtonClicked;

    void Start()
    {
        // クリック処理
        _onButtonClicked
            .ThrottleFirst(TimeSpan.FromSeconds(0.5)) // 連打防止
            .Subscribe(_ => ExecuteAction())
            .AddTo(this);
    }
}
```

## 通信バッチング

```csharp
// サーバー通信を一定時間ごとにまとめて実行
public class NetworkManager
{
    private Subject<ItemRequest> _itemRequests = new Subject<ItemRequest>();

    void Start()
    {
        _itemRequests
            .Buffer(TimeSpan.FromSeconds(1)) // 1秒ごとにバッファリング
            .Where(requests => requests.Count > 0)
            .Subscribe(async requests =>
            {
                // バッチ送信
                await SendBatchRequestAsync(requests);
            })
            .AddTo(this);
    }

    public void RequestItem(ItemRequest request)
    {
        _itemRequests.OnNext(request);
    }
}
```

## 非同期処理との連携

```csharp
// UniTaskとUniRxの組み合わせ
public async UniTask LoadAssetReactive()
{
    await Observable
        .FromCoroutine(() => LoadAssetCoroutine())
        .ToUniTask();
}

// ストリーム処理
Observable
    .Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1))
    .SelectMany(_ => LoadDataAsync().ToObservable())
    .Subscribe(data => ProcessData(data))
    .AddTo(this);
```

## ライフサイクル管理

```csharp
// ✅ Good: AddToでライフサイクル管理
void Start()
{
    Observable.EveryUpdate()
        .Subscribe(_ => UpdateLogic())
        .AddTo(this); // MonoBehaviourと連動して自動Dispose
}

// ✅ Good: CompositeDisposableで手動管理
private CompositeDisposable _disposables = new CompositeDisposable();

void OnEnable()
{
    _characterData.Health
        .Subscribe(OnHealthChanged)
        .AddTo(_disposables);
}

void OnDisable()
{
    _disposables.Clear(); // 購読解除
}
```

## ベストプラクティス

✅ **DO**:
- `AddTo()` でライフサイクル管理
- `ThrottleFirst/Throttle` で連打・高頻度イベント制御
- `Buffer/Window` で通信バッチング

❌ **DON'T**:
- Subscribeしたまま放置（メモリリーク）
- Update内でSubscribe（パフォーマンス低下）
- 過度なネストしたストリーム処理

信頼度: 0.91
