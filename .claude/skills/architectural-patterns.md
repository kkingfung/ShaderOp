# アーキテクチャパターン

## 概要
Unity開発で使用される主要なアーキテクチャパターンとSOLID原則。

## SOLID原則

### S: Single Responsibility（単一責任の原則）
```csharp
// ❌ Bad: 複数の責任を持つクラス
public class CharacterManager
{
    public void Move() { }
    public void Attack() { }
    public void SaveToDatabase() { } // データ永続化の責任
    public void RenderGraphics() { } // 描画の責任
}

// ✅ Good: 単一の責任
public class CharacterMovement
{
    public void Move() { }
}

public class CharacterCombat
{
    public void Attack() { }
}

public class CharacterDataRepository
{
    public void SaveToDatabase() { }
}
```

### O: Open/Closed（オープン・クローズドの原則）
```csharp
// ✅ Good: 拡張に開いて、修正に閉じている
public interface IWeapon
{
    void Attack();
}

public class Sword : IWeapon
{
    public void Attack() => Debug.Log("剣で攻撃");
}

public class Bow : IWeapon
{
    public void Attack() => Debug.Log("弓で攻撃");
}

// 新しい武器を追加する場合、既存コードを変更せずに拡張
public class Magic : IWeapon
{
    public void Attack() => Debug.Log("魔法で攻撃");
}
```

### L: Liskov Substitution（リスコフの置換原則）
```csharp
// 基底クラス
public abstract class Character
{
    public abstract void Move(Vector3 direction);
}

// 派生クラスは基底クラスと置き換え可能
public class Player : Character
{
    public override void Move(Vector3 direction)
    {
        transform.position += direction * Time.deltaTime;
    }
}

public class Enemy : Character
{
    public override void Move(Vector3 direction)
    {
        // AIによる移動
        transform.position += direction * Time.deltaTime;
    }
}
```

### I: Interface Segregation（インターフェース分離の原則）
```csharp
// ❌ Bad: 巨大なインターフェース
public interface ICharacter
{
    void Move();
    void Attack();
    void Fly(); // すべてのキャラクターが飛べるわけではない
    void Swim(); // すべてのキャラクターが泳げるわけではない
}

// ✅ Good: 小さく分離されたインターフェース
public interface IMovable
{
    void Move();
}

public interface IAttackable
{
    void Attack();
}

public interface IFlyable
{
    void Fly();
}

public interface ISwimmable
{
    void Swim();
}

// 必要なインターフェースのみ実装
public class Player : IMovable, IAttackable
{
    public void Move() { }
    public void Attack() { }
}

public class Bird : IMovable, IFlyable
{
    public void Move() { }
    public void Fly() { }
}
```

### D: Dependency Inversion（依存性逆転の原則）
```csharp
// ❌ Bad: 具象クラスに依存
public class CharacterController
{
    private CharacterDataRepository _repository = new CharacterDataRepository();

    public void SaveCharacter()
    {
        _repository.Save(); // 具象クラスに依存
    }
}

// ✅ Good: インターフェースに依存
public interface IDataRepository
{
    void Save();
}

public class CharacterDataRepository : IDataRepository
{
    public void Save() { }
}

public class CharacterController
{
    private readonly IDataRepository _repository;

    // 依存性注入
    public CharacterController(IDataRepository repository)
    {
        _repository = repository;
    }

    public void SaveCharacter()
    {
        _repository.Save(); // インターフェースに依存
    }
}
```

## デザインパターン

### Service Locator パターン
```csharp
/// <summary>
/// サービスロケーター
/// </summary>
public class ServiceLocator
{
    private static ServiceLocator _instance;
    public static ServiceLocator Instance => _instance ??= new ServiceLocator();

    private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

    /// <summary>サービスを登録</summary>
    public void Register<T>(T service) where T : class
    {
        _services[typeof(T)] = service;
    }

    /// <summary>サービスを取得</summary>
    public T Get<T>() where T : class
    {
        if (_services.TryGetValue(typeof(T), out var service))
        {
            return service as T;
        }
        throw new InvalidOperationException($"Service {typeof(T).Name} not registered");
    }

    /// <summary>サービスの登録を解除</summary>
    public void Unregister<T>() where T : class
    {
        _services.Remove(typeof(T));
    }

    /// <summary>すべてのサービスをクリア</summary>
    public void Clear()
    {
        _services.Clear();
    }
}

// 使用例
public class GameBootstrap : MonoBehaviour
{
    void Awake()
    {
        // サービス登録
        ServiceLocator.Instance.Register<ICharacterService>(new CharacterService());
        ServiceLocator.Instance.Register<IAssetLoader>(new AssetLoader());
    }
}

public class CharacterController : MonoBehaviour
{
    private ICharacterService _characterService;

    void Start()
    {
        // サービス取得
        _characterService = ServiceLocator.Instance.Get<ICharacterService>();
    }
}
```

### Factory パターン
```csharp
/// <summary>
/// キャラクターファクトリー
/// </summary>
public class CharacterFactory
{
    /// <summary>
    /// キャラクタータイプに応じて生成
    /// </summary>
    public ICharacter CreateCharacter(CharacterType type)
    {
        switch (type)
        {
            case CharacterType.Player:
                return new Player();
            case CharacterType.Enemy:
                return new Enemy();
            case CharacterType.NPC:
                return new NPC();
            default:
                throw new ArgumentException($"Unknown character type: {type}");
        }
    }
}

public enum CharacterType
{
    Player,
    Enemy,
    NPC
}
```

### Observer パターン
```csharp
/// <summary>
/// イベントチャネル（ScriptableObject）
/// </summary>
[CreateAssetMenu(fileName = "CharacterEventChannel", menuName = "Events/Character Event")]
public class CharacterEventChannel : ScriptableObject
{
    private event Action<Character> _onCharacterSpawned;

    /// <summary>キャラクター生成イベントを発火</summary>
    public void RaiseCharacterSpawned(Character character)
    {
        _onCharacterSpawned?.Invoke(character);
    }

    /// <summary>イベント購読</summary>
    public void Subscribe(Action<Character> callback)
    {
        _onCharacterSpawned += callback;
    }

    /// <summary>イベント購読解除</summary>
    public void Unsubscribe(Action<Character> callback)
    {
        _onCharacterSpawned -= callback;
    }
}
```

### Repository パターン
```csharp
/// <summary>
/// データリポジトリインターフェース
/// </summary>
public interface ICharacterRepository
{
    UniTask<Character> GetByIdAsync(string id);
    UniTask<List<Character>> GetAllAsync();
    UniTask SaveAsync(Character character);
    UniTask DeleteAsync(string id);
}

/// <summary>
/// キャラクターリポジトリ実装
/// </summary>
public class CharacterRepository : ICharacterRepository
{
    private readonly Dictionary<string, Character> _cache = new Dictionary<string, Character>();

    public async UniTask<Character> GetByIdAsync(string id)
    {
        // キャッシュ確認
        if (_cache.TryGetValue(id, out var character))
        {
            return character;
        }

        // データベースから取得（仮）
        var loadedCharacter = await LoadFromDatabaseAsync(id);
        _cache[id] = loadedCharacter;

        return loadedCharacter;
    }

    public async UniTask<List<Character>> GetAllAsync()
    {
        // すべてのキャラクターを取得
        return await LoadAllFromDatabaseAsync();
    }

    public async UniTask SaveAsync(Character character)
    {
        // データベースに保存
        await SaveToDatabaseAsync(character);

        // キャッシュ更新
        _cache[character.Id] = character;
    }

    public async UniTask DeleteAsync(string id)
    {
        // データベースから削除
        await DeleteFromDatabaseAsync(id);

        // キャッシュから削除
        _cache.Remove(id);
    }

    private async UniTask<Character> LoadFromDatabaseAsync(string id)
    {
        await UniTask.Delay(100); // 仮実装
        return new Character { Id = id };
    }

    private async UniTask<List<Character>> LoadAllFromDatabaseAsync()
    {
        await UniTask.Delay(100);
        return new List<Character>();
    }

    private async UniTask SaveToDatabaseAsync(Character character)
    {
        await UniTask.Delay(100);
    }

    private async UniTask DeleteFromDatabaseAsync(string id)
    {
        await UniTask.Delay(100);
    }
}
```

### Command パターン
```csharp
/// <summary>
/// コマンドインターフェース
/// </summary>
public interface ICommand
{
    void Execute();
    void Undo();
}

/// <summary>
/// 移動コマンド
/// </summary>
public class MoveCommand : ICommand
{
    private readonly Transform _transform;
    private readonly Vector3 _direction;
    private Vector3 _previousPosition;

    public MoveCommand(Transform transform, Vector3 direction)
    {
        _transform = transform;
        _direction = direction;
    }

    public void Execute()
    {
        _previousPosition = _transform.position;
        _transform.position += _direction;
    }

    public void Undo()
    {
        _transform.position = _previousPosition;
    }
}

/// <summary>
/// コマンドマネージャー
/// </summary>
public class CommandManager
{
    private readonly Stack<ICommand> _commandHistory = new Stack<ICommand>();

    public void ExecuteCommand(ICommand command)
    {
        command.Execute();
        _commandHistory.Push(command);
    }

    public void Undo()
    {
        if (_commandHistory.Count > 0)
        {
            var command = _commandHistory.Pop();
            command.Undo();
        }
    }
}
```

## ベストプラクティス

✅ **DO**:
- SOLID原則に従う
- インターフェースに依存する
- 依存性注入を使用
- 単一責任の原則を守る

❌ **DON'T**:
- 具象クラスに依存
- 巨大なクラス（God Object）
- 過度な抽象化（YAGNI違反）

信頼度: 0.95
