# コーディング規約

## 📋 目次
1. [命名規則](#命名規則)
2. [MonoBehaviour パターン](#monobehaviour-パターン)
3. [パフォーマンス最適化](#パフォーマンス最適化)
4. [メモリ管理](#メモリ管理)
5. [非同期処理 (UniTask)](#非同期処理-unitask)
6. [リアクティブプログラミング (UniRx)](#リアクティブプログラミング-unirx)
7. [エラーハンドリング](#エラーハンドリング)
8. [Shader開発](#shader開発)

---

## 命名規則

### C# クラス・メンバー

```csharp
// ✅ Good
public class CharacterMovementController : MonoBehaviour
{
    // フィールド: _camelCase（privateは必ず_接頭辞）
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private Rigidbody _rigidbody;

    // プロパティ: PascalCase
    public bool IsGrounded { get; private set; }
    public Vector3 Velocity { get; private set; }

    // メソッド: PascalCase、動詞で始める
    public void Move(Vector3 direction) { }
    public async UniTask LoadDataAsync() { }

    // イベントハンドラ: On...
    private void OnCollisionEnter(Collision collision) { }

    // コルーチン: ...Coroutine
    private IEnumerator FadeOutCoroutine() { }

    // 定数: UPPER_SNAKE_CASE
    private const float MAX_JUMP_HEIGHT = 3f;
    private const int PLAYER_LAYER = 8;
}

// ❌ Bad
public class controller : MonoBehaviour
{
    public float speed;              // privateなのにpublic
    private Rigidbody rb;            // 略語
    public bool grounded;            // プロパティにすべき

    void move() { }                  // PascalCaseでない
    void collision(Collision c) { }  // イベントハンドラがOnで始まらない
}
```

### アセット命名

```
✅ Good:
- P_Character_Player.prefab      # P_: Prefab
- M_Skin_Default.mat             # M_: Material
- T_Character_Diffuse.png        # T_: Texture
- SG_Character_Base.shadergraph  # SG_: Shader Graph
- SM_Rock_01.fbx                 # SM_: Static Mesh

❌ Bad:
- Player.prefab
- material.mat
- texture1.png
```

---

## MonoBehaviour パターン

### ライフサイクルメソッド順序

```csharp
public class ExampleBehaviour : MonoBehaviour
{
    // 1. SerializeField フィールド
    [Header("Movement Settings")]
    [SerializeField] private float _speed = 5f;

    [Header("References")]
    [SerializeField] private Rigidbody _rigidbody;

    // 2. private フィールド
    private bool _isInitialized;

    // 3. プロパティ
    public bool IsGrounded { get; private set; }

    // 4. Unity メッセージ（実行順）
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerDeath += HandlePlayerDeath;
    }

    private void Start()
    {
        Initialize();
    }

    private void Update() { }
    private void FixedUpdate() { }
    private void LateUpdate() { }

    private void OnDisable()
    {
        GameEvents.OnPlayerDeath -= HandlePlayerDeath;
    }

    private void OnDestroy()
    {
        Dispose();
    }

    // 5. public メソッド
    public void DoSomething() { }

    // 6. private メソッド
    private void Initialize() { }

    // 7. イベントハンドラ
    private void HandlePlayerDeath() { }
}
```

### コンポーネント取得

```csharp
// ✅ Good: Awake/Startでキャッシュ
private Rigidbody _rigidbody;
private Transform _transform;

private void Awake()
{
    _rigidbody = GetComponent<Rigidbody>();
    _transform = transform;  // transformもキャッシュ推奨

    Debug.Assert(_rigidbody != null, "Rigidbody が見つかりません", this);
}

private void Update()
{
    _rigidbody.velocity = _transform.forward * 5f;
}

// ❌ Bad: 毎フレームGetComponent
private void Update()
{
    GetComponent<Rigidbody>().velocity = transform.forward * 5f;  // ❌
}
```

### 必須コンポーネント

```csharp
// ✅ Good
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class PhysicsObject : MonoBehaviour
{
    private Rigidbody _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }
}
```

---

## パフォーマンス最適化

### Update 最適化

```csharp
// ✅ Good
private void Update()
{
    if (!_isActive) return;  // 早期リターン
    if (_isDead) return;

    ProcessMovement();
}

// ❌ Bad
private void Update()
{
    string message = "Player: " + playerName;  // ❌ GC Alloc

    var enemies = FindObjectsOfType<Enemy>()
        .Where(e => e.IsAlive)
        .ToList();  // ❌ GC Alloc大量発生
}
```

### オブジェクトプール

```csharp
// ✅ Good
public class BulletPool : MonoBehaviour
{
    private Queue<Bullet> _pool = new Queue<Bullet>();

    public Bullet Get()
    {
        if (_pool.Count > 0)
        {
            var bullet = _pool.Dequeue();
            bullet.gameObject.SetActive(true);
            return bullet;
        }
        return Instantiate(_bulletPrefab);
    }

    public void Return(Bullet bullet)
    {
        bullet.gameObject.SetActive(false);
        _pool.Enqueue(bullet);
    }
}

// ❌ Bad
void Shoot()
{
    var bullet = Instantiate(_bulletPrefab);  // ❌
    Destroy(bullet, 5f);                      // ❌
}
```

### 文字列操作

```csharp
// ✅ Good
private StringBuilder _sb = new StringBuilder(100);

private string BuildMessage(int score)
{
    _sb.Clear();
    _sb.Append("Score: ");
    _sb.Append(score);
    return _sb.ToString();
}

// ❌ Bad
private string BuildMessage(int score)
{
    return "Score: " + score;  // ❌ GC Alloc
}
```

### Find系メソッド

```csharp
// ✅ Good: シングルトン
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

// 使用側
GameManager.Instance.StartGame();

// ❌ Bad
private void Update()
{
    var manager = FindObjectOfType<GameManager>();  // ❌ 毎フレーム検索
}
```

---

## メモリ管理

### Dispose パターン

```csharp
// ✅ Good
public class ResourceManager : MonoBehaviour, IDisposable
{
    private Texture2D _texture;
    private bool _disposed;

    public void Dispose()
    {
        if (_disposed) return;

        if (_texture != null)
        {
            Destroy(_texture);
            _texture = null;
        }

        _disposed = true;
    }

    private void OnDestroy()
    {
        Dispose();
    }
}
```

### Addressables

```csharp
// ✅ Good
using UnityEngine.AddressableAssets;

public async UniTask<GameObject> LoadCharacterAsync(AssetReference assetRef)
{
    var handle = assetRef.InstantiateAsync();
    await handle.ToUniTask();

    if (handle.Status == AsyncOperationStatus.Succeeded)
    {
        return handle.Result;
    }
    return null;
}

private void OnDestroy()
{
    if (_handle.IsValid())
    {
        Addressables.Release(_handle);
    }
}

// ❌ Bad
var prefab = Resources.Load<GameObject>("Characters/Player");  // ❌
```

---

## 非同期処理 (UniTask)

### 基本パターン

```csharp
// ✅ Good: async UniTask（async void 禁止）
using Cysharp.Threading.Tasks;

public class DataLoader : MonoBehaviour
{
    private CancellationTokenSource _cts;

    private void OnEnable()
    {
        _cts = new CancellationTokenSource();
    }

    private void OnDisable()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }

    public async UniTask<PlayerData> LoadPlayerDataAsync()
    {
        try
        {
            var data = await FetchFromServerAsync(_cts.Token);
            return data;
        }
        catch (OperationCanceledException)
        {
            Debug.Log("読み込みがキャンセルされました");
            return null;
        }
    }
}

// ❌ Bad
public async void LoadData()  // ❌ async void
{
    var data = await FetchFromServerAsync();
}
```

---

## リアクティブプログラミング (UniRx)

### Dispose 管理

```csharp
// ✅ Good
using UniRx;

public class PlayerHealth : MonoBehaviour
{
    private CompositeDisposable _disposables = new CompositeDisposable();

    private void Start()
    {
        Observable.EveryUpdate()
            .Subscribe(_ => CheckHealth())
            .AddTo(_disposables);

        GameEvents.OnDamageReceived
            .Subscribe(damage => TakeDamage(damage))
            .AddTo(_disposables);
    }

    private void OnDestroy()
    {
        _disposables?.Dispose();
    }
}

// または AddTo(this)
Observable.EveryUpdate()
    .Subscribe(_ => CheckHealth())
    .AddTo(this);  // 自動Dispose

// ❌ Bad
Observable.EveryUpdate()
    .Subscribe(_ => CheckHealth());  // ❌ メモリリーク
```

---

## エラーハンドリング

### Assert

```csharp
// ✅ Good
using UnityEngine.Assertions;

private void Awake()
{
    var rigidbody = GetComponent<Rigidbody>();
    Assert.IsNotNull(rigidbody, "Rigidbody が見つかりません", this);
    Assert.IsTrue(_speed > 0, "速度は0より大きい必要があります", this);
}
```

### 例外処理

```csharp
// ✅ Good
public async UniTask<SaveData> LoadSaveDataAsync(string path)
{
    try
    {
        var data = await LoadFromFileAsync(path);
        return data;
    }
    catch (FileNotFoundException ex)
    {
        Debug.LogWarning($"セーブデータが見つかりません: {path}");
        return CreateDefaultSaveData();
    }
    catch (Exception ex)
    {
        Debug.LogError($"セーブデータ読み込みエラー: {ex.Message}");
        throw;
    }
}

// ❌ Bad
try
{
    var data = await LoadFromFileAsync(path);
}
catch
{
    // 何もしない ❌
}
```

---

## Shader開発

### Shader Graph 命名

```
✅ Good:
- SG_Character_Base.shadergraph
- SG_Character_Hair.shadergraph
- SG_HexTile.shadergraph

❌ Bad:
- shader1.shadergraph
- CharacterShader.shadergraph
```

### HLSL コーディング

```hlsl
// ✅ Good: 明確な関数名、コメント
/// <summary>
/// トゥーンシェーディング計算
/// </summary>
/// <param name="normal">法線ベクトル</param>
/// <param name="lightDir">ライト方向</param>
/// <returns>シェーディング結果</returns>
float CalculateToonShading(float3 normal, float3 lightDir)
{
    float NdotL = dot(normal, lightDir);

    // 2段階トゥーン
    if (NdotL > 0.5)
        return 1.0;
    else if (NdotL > 0.0)
        return 0.5;
    else
        return 0.2;
}

// ❌ Bad
float calc(float3 n, float3 l)  // 略語、コメントなし
{
    return dot(n, l);
}
```

### マテリアルプロパティ

```hlsl
// ✅ Good: わかりやすいプロパティ名
Properties
{
    _BaseColor("Base Color", Color) = (1,1,1,1)
    _MainTex("Main Texture", 2D) = "white" {}
    _Smoothness("Smoothness", Range(0, 1)) = 0.5
}

// ❌ Bad
Properties
{
    _C("Color", Color) = (1,1,1,1)  // 略語
    _T("Tex", 2D) = "white" {}
}
```

---

## 参考リソース

- **Unity Performance Best Practices**: https://docs.unity3d.com/Manual/BestPracticeUnderstandingPerformanceInUnity.html
- **UniTask**: https://github.com/Cysharp/UniTask
- **UniRx**: https://github.com/neuecc/UniRx
- **C# Coding Conventions**: https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions

---
最終更新: 2026-02-21
