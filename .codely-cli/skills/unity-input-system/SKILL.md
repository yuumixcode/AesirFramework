---
name: unity-input-system
description: Unity New Input System advanced expert — Input Action Asset, Player Input Component, Action Maps, C# generated class, event binding, multi-device support, local multiplayer, gamepad/keyboard/touch switching. Use when implementing remappable input, multi-device support, local split-screen multiplayer, or custom key bindings.
---

# Unity Input System (Advanced)

## Setup

1. Install `com.unity.inputsystem` via Package Manager.
2. `Edit > Project Settings > Player > Active Input Handling` → "Input System Package (New)".
3. Create Input Action Asset: `Create > Input Actions`.

## Input Action Asset

Define Actions → Action Maps → Bindings in the editor:

```
Player (Action Map)
├── Move (Action, Value, Vector2)
│   ├── <Keyboard>/w (Binding)
│   ├── <Keyboard>/s
│   ├── <Keyboard>/a
│   ├── <Keyboard>/d
│   ├── <Gamepad>/leftStick
│   └── <Keyboard>/arrowKeys (Composite: 2D Vector)
├── Jump (Action, Button)
│   ├── <Keyboard>/space
│   └── <Gamepad>/buttonSouth
├── Attack (Action, Button)
│   ├── <Mouse>/leftButton
│   └── <Gamepad>/buttonWest
└── Look (Action, Value, Vector2)
    ├── <Mouse>/delta
    └── <Gamepad>/rightStick

UI (Action Map)
├── Navigate (Action, Pass Through, Vector2)
├── Submit (Action, Button)
└── Cancel (Action, Button)
```

## C# Generated Class

Enable "Generate C# Class" in Input Action Asset settings:

```csharp
// Auto-generated class — access actions as properties
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private GameInput _input; // Auto-generated class

    void Awake()
    {
        _input = new GameInput();
    }

    void OnEnable() => _input.Enable();
    void OnDisable() => _input.Disable();

    void Update()
    {
        // Read values
        Vector2 move = _input.Player.Move.ReadValue<Vector2>();
        transform.Translate(move * 5f * Time.deltaTime);

        // Button press (was pressed this frame)
        if (_input.Player.Jump.WasPressedThisFrame())
            Jump();
    }

    // Or use callbacks
    void Jump_performed(InputAction.CallbackContext ctx)
    {
        // Called when action is triggered
        Jump();
    }

    void Jump_canceled(InputAction.CallbackContext ctx)
    {
        // Called when button released (for charge attacks)
    }
}
```

## Player Input Component

```
1. Add "Player Input" component to player GameObject
2. Assign Input Action Asset
3. Set "Behavior":
   - Send Messages: calls SendMessage("OnMove", value)
   - Invoke Unity Events: uses UnityEvent (assign in Inspector)
   - Invoke C Sharp Events: uses C# events (subscribe in code)
```

```csharp
// Behavior: Invoke C Sharp Events
public class PlayerController : MonoBehaviour
{
    private PlayerInput _playerInput;

    void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _playerInput.actions["Move"].performed += OnMove;
        _playerInput.actions["Jump"].performed += OnJump;
    }

    void OnMove(InputAction.CallbackContext ctx)
    {
        Vector2 move = ctx.ReadValue<Vector2>();
        // Move player
    }

    void OnJump(InputAction.CallbackContext ctx)
    {
        // Jump
    }

    void OnDestroy()
    {
        _playerInput.actions["Move"].performed -= OnMove;
        _playerInput.actions["Jump"].performed -= OnJump;
    }
}
```

## Multi-Device Support

```csharp
// Detect current device
var currentDevice = InputSystem.GetDevice<Gamepad>();
var keyboard = Keyboard.current;
var mouse = Mouse.current;

// Check if gamepad connected
if (Gamepad.current != null)
    Debug.Log($"Gamepad: {Gamepad.current.name}");

// Listen for device change
InputSystem.onDeviceChange += (device, change) =>
{
    switch (change)
    {
        case InputDeviceChange.Added:
            Debug.Log($"Device added: {device.name}");
            break;
        case InputDeviceChange.Removed:
            Debug.Log($"Device removed: {device.name}");
            break;
    }
};

// Switch control scheme based on last used device
void Update()
{
    if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        SetControlScheme("KeyboardMouse");
    if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
        SetControlScheme("Gamepad");
}
```

## Local Multiplayer

```csharp
// PlayerInputManager handles spawning multiple players
// Add "Player Input Manager" component to a manager GameObject

// Config:
//   Join Behavior: Join Players When Button Is Pressed
//   Player Prefab: your player prefab (must have Player Input)
//   Max Player Count: 4

// Each player gets a different control scheme + index
public class MultiplayerManager : MonoBehaviour
{
    public PlayerInputManager inputManager;

    void Start()
    {
        PlayerInputManager.instance.onPlayerJoined += OnPlayerJoined;
        PlayerInputManager.instance.onPlayerLeft += OnPlayerLeft;
    }

    void OnPlayerJoined(PlayerInput playerInput)
    {
        int index = playerInput.playerIndex;
        Debug.Log($"Player {index + 1} joined");

        // Assign control scheme
        playerInput.SwitchCurrentControlScheme(
            index == 0 ? "KeyboardMouse" : "Gamepad",
            Gamepad.all.ElementAtOrDefault(index - 1));

        // Set up split screen (optional)
        if (index == 0)
            playerInput.camera.rect = new Rect(0, 0.5f, 1, 0.5f);
        else
            playerInput.camera.rect = new Rect(0, 0, 1, 0.5f);
    }

    void OnPlayerLeft(PlayerInput playerInput)
    {
        Debug.Log($"Player {playerInput.playerIndex + 1} left");
    }
}
```

## Action Maps (Switching Contexts)

```csharp
// Switch between gameplay and UI controls
public class InputManager : MonoBehaviour
{
    private GameInput _input;

    void Awake()
    {
        _input = new GameInput();
        _input.Player.Enable();
    }

    public void SwitchToUI()
    {
        _input.Player.Disable();
        _input.UI.Enable();
    }

    public void SwitchToPlayer()
    {
        _input.UI.Disable();
        _input.Player.Enable();
    }
}
```

## Key Gotchas

1. **Old vs New**: Can't use `Input.GetKey` with new Input System (unless "Both" is selected). New system is event-driven.
2. **Enable/Disable**: Action maps must be `.Enable()`d before use. Disabled maps don't fire callbacks.
3. **performed vs started**: `started` = button just pressed. `performed` = action completed (for Value type, fires continuously). `canceled` = released.
4. **WasPressedThisFrame**: Convenience method, but only works with generated C# class, not raw callbacks.
5. **Control schemes**: Define in Input Action Asset (e.g., "KeyboardMouse", "Gamepad"). Each scheme groups bindings for a device set.
6. **Deadzone**: Analog sticks have built-in deadzone in new Input System. Configure in binding processor: `Stick Deadzone(min=0.125, max=0.925)`.
7. **Touch**: Use `<Touchscreen>/touch0/position` and `<Touchscreen>/touch0/phase`. Multi-touch: `touch1`, `touch2`, etc.
8. **Polling vs events**: Use events (`performed`, `canceled`) for buttons. Use polling (`ReadValue<Vector2>()`) for continuous values like movement.
