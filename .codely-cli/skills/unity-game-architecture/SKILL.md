---
name: unity-game-architecture
description: Unity game architecture design expert — MVC/MVP/MVVM patterns, layered architecture, dependency injection, Domain-Driven Design (DDD), SOLID principles in game development. Use when designing maintainable, scalable, testable game architectures to avoid spaghetti code.
---

# Unity Game Architecture (MVC/MVP)

## Common Patterns Comparison

| Pattern | Data Flow | Best For |
|---------|-----------|----------|
| MVC | Model→View→Controller→Model | Web, simple games |
| MVP | Model↔Presenter↔View | UI-heavy games, testing |
| MVVM | Model↔ViewModel↔View (binding) | Data-driven UI |
| ECS | Data→Systems | Performance-critical |

## MVC Pattern

```csharp
// Model — pure data, no Unity dependency
public class PlayerModel
{
    public int Health { get; set; } = 100;
    public int Score { get; set; } = 0;
    public event Action<int> OnHealthChanged;
    public event Action<int> OnScoreChanged;

    public void TakeDamage(int damage)
    {
        Health = Mathf.Max(0, Health - damage);
        OnHealthChanged?.Invoke(Health);
    }

    public void AddScore(int points)
    {
        Score += points;
        OnScoreChanged?.Invoke(Score);
    }
}

// View — purely visual, no logic
public class PlayerView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _healthText;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private Slider _healthBar;

    public void UpdateHealth(int health, int maxHealth)
    {
        _healthText.text = $"{health}/{maxHealth}";
        _healthBar.value = (float)health / maxHealth;
    }

    public void UpdateScore(int score)
    {
        _scoreText.text = score.ToString();
    }

    public void PlayDamageEffect()
    {
        // Flash red, screen shake, etc.
    }
}

// Controller — bridges Model and View
public class PlayerController : MonoBehaviour
{
    private PlayerModel _model;
    private PlayerView _view;
    [SerializeField] private int _maxHealth = 100;

    void Awake()
    {
        _model = new PlayerModel();
        _view = GetComponent<PlayerView>();

        _model.OnHealthChanged += (hp) => _view.UpdateHealth(hp, _maxHealth);
        _model.OnScoreChanged += (score) => _view.UpdateScore(score);
    }

    void OnDestroy()
    {
        _model.OnHealthChanged -= null;
        _model.OnScoreChanged -= null;
    }

    public void TakeDamage(int damage)
    {
        _model.TakeDamage(damage);
        _view.PlayDamageEffect();
    }

    public void AddScore(int points) => _model.AddScore(points);
}
```

## Layered Architecture

```
┌─────────────────────────────────┐
│         Presentation Layer       │ ← Views, UI, Input
│  (MonoBehaviour, UI Toolkit)     │
├─────────────────────────────────┤
│         Application Layer        │ ← Controllers, Use Cases
│  (Gameplay logic, State mgmt)    │
├─────────────────────────────────┤
│         Domain Layer            │ ← Models, Rules, Entities
│  (Pure C#, no Unity dependency) │
├─────────────────────────────────┤
│       Infrastructure Layer      │ ← Save, Network, I/O
│  (File IO, HTTP, DB)             │
└─────────────────────────────────┘
```

```csharp
// Domain Layer — no Unity dependency, pure C#
namespace Game.Domain
{
    public class Inventory
    {
        private List<Item> _items = new();
        public IReadOnlyList<Item> Items => _items.AsReadOnly();

        public void Add(Item item)
        {
            if (_items.Count >= 20)
                throw new InventoryFullException();
            _items.Add(item);
        }

        public bool Remove(Item item) => _items.Remove(item);
        public bool Contains(Item item) => _items.Contains(item);
    }
}

// Application Layer — orchestrates use cases
namespace Game.Application
{
    public class PickupItemUseCase
    {
        private readonly Inventory _inventory;
        private readonly IItemRepository _itemRepo;

        public PickupItemUseCase(Inventory inventory, IItemRepository itemRepo)
        {
            _inventory = inventory;
            _itemRepo = itemRepo;
        }

        public bool Execute(string itemId)
        {
            var item = _itemRepo.GetById(itemId);
            if (item == null) return false;
            _inventory.Add(item);
            return true;
        }
    }
}

// Infrastructure Layer — Unity-dependent
namespace Game.Infrastructure
{
    public class PlayerPrefsItemRepository : IItemRepository
    {
        public Item GetById(string id)
        {
            string json = PlayerPrefs.GetString($"item_{id}", "");
            return string.IsNullOrEmpty(json) ? null : JsonUtility.FromJson<Item>(json);
        }
    }
}

// Presentation Layer — Unity MonoBehaviour
namespace Game.Presentation
{
    public class PickupController : MonoBehaviour
    {
        [SerializeField] private string _itemId;
        private PickupItemUseCase _useCase;

        void Start()
        {
            // In practice, use DI container
            var inventory = new Inventory();
            var repo = new PlayerPrefsItemRepository();
            _useCase = new PickupItemUseCase(inventory, repo);
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
                _useCase.Execute(_itemId);
        }
    }
}
```

## Dependency Injection (VContainer)

```csharp
// Install VContainer via UPM: com.hadashienergy.vcontainer
using VContainer;
using VContainer.Unity;

// Define lifetime scope (where objects are created)
public class GameLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // Register services
        builder.Register<IItemRepository, PlayerPrefsItemRepository>(Lifetime.Singleton);
        builder.Register<Inventory>(Lifetime.Singleton);

        // Register use cases
        builder.Register<PickupItemUseCase>(Lifetime.Transient);

        // Register components
        builder.RegisterComponentInHierarchy<PickupController>();
    }
}

// Inject into MonoBehaviour
public class PickupController : MonoBehaviour
{
    [Inject] private PickupItemUseCase _useCase;
    [Inject] private Inventory _inventory;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            _useCase.Execute("sword_01");
    }
}
```

## SOLID Principles in Unity

```csharp
// S — Single Responsibility
// BAD: PlayerController does movement, health, inventory, save, UI
// GOOD: Separate into PlayerMovement, HealthSystem, Inventory, SaveManager

// O — Open/Closed (open for extension, closed for modification)
// BAD: switch(type) { case "fire": ... case "ice": ... } — add case for each
// GOOD: abstract class Element { abstract float GetDamage(Element other); }
//        class Fire : Element { override float GetDamage(Element other) => other is Ice ? 2f : 1f; }

// L — Liskov Substitution
// Base class methods must work for all derived classes
// BAD: class FlyingEnemy : Enemy { override void Move() { throw NotImplemented(); } }

// I — Interface Segregation
// BAD: interface IWeapon { void Attack(); void Reload(); void Aim(); } — melee can't reload
// GOOD: interface IAttack { void Attack(); }
//        interface IReload { void Reload(); }

// D — Dependency Inversion (depend on abstractions)
// BAD: class SaveManager { private PlayerPrefsSave _save = new(); }
// GOOD: class SaveManager { private ISaveSystem _save; constructor(ISaveSystem save) { _save = save; } }
```

## Key Gotchas

1. **Domain layer purity**: Domain models should NOT reference UnityEngine. This makes them testable and portable. Use interfaces for Unity-dependent operations.
2. **MonoBehaviour as Controller**: MonoBehaviour is OK in Presentation layer. Don't put game logic IN MonoBehaviour — delegate to domain/application layer.
3. **Event over reference**: Instead of `healthBar.player = player`, use `player.OnHealthChanged += healthBar.Update`. Reduces coupling.
4. **DI overhead**: VContainer/Zenject adds a small overhead. For small games, manual injection (constructor) is fine.
5. **Assembly definitions**: Split layers into separate assemblies. Domain layer should NOT reference UnityEngine assembly. Prevents accidental coupling.
6. **ScriptableObject as config**: Use SOs for configuration data (not as models — SOs are assets, not runtime-mutable).
7. **Don't over-engineer**: Simple games don't need 4 layers. If a MonoBehaviour is <100 lines and works, leave it. Architecture serves the game, not the other way around.
