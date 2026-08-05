# `RemoveListenerOnDisableTrigger`

## 介绍

- 种类: `class`
- 所在程序集: `Runestone.AesirArchitecture`
- 所在命名空间: `Runestone.AesirArchitecture`

``` csharp
[DisallowMultipleComponent]
public sealed class RemoveListenerOnDisableTrigger : Runestone.AesirArchitecture.RemoveListenerTrigger, 
Sirenix.Serialization.ISupportsPrefabSerialization, 
UnityEngine.ISerializationCallbackReceiver
```

### 注释

- GameObject 禁用时自动移除所有监听。挂载此组件后，当 GameObject 被禁用时将执行批量移除操作。

## 构造方法

| 构造方法签名 [仅包含公共实例方法] | 注释 |
| :--- | :--- |
| `public RemoveListenerOnDisableTrigger()` |  |

## 方法

### 所有方法签名总览

| 方法完整签名 |
| :--- | 
| `[Overload] [Overload] public Component GetComponent(Type type)` |
| `[Overload] [Overload] public Component GetComponent(string type)` |
| `[Overload] [Overload] public Component GetComponentInChildren(Type t)` |
| `[Overload] [Overload] public Component GetComponentInChildren(Type t, bool includeInactive)` |
| `[Overload] [Overload] public Component GetComponentInParent(Type t)` |
| `[Overload] [Overload] public Component GetComponentInParent(Type t, bool includeInactive)` |
| `public Component[] GetComponents(Type type)` |
| `[Overload] [Overload] public Component[] GetComponentsInChildren(Type t)` |
| `[Overload] [Overload] public Component[] GetComponentsInChildren(Type t, bool includeInactive)` |
| `[Overload] [Overload] public Component[] GetComponentsInParent(Type t)` |
| `[Overload] [Overload] public Component[] GetComponentsInParent(Type t, bool includeInactive)` |
| `[Overload] [Overload] [Overload] [Overload] public Coroutine StartCoroutine(IEnumerator routine)` |
| `[Overload] [Overload] [Overload] [Overload] public Coroutine StartCoroutine(string methodName)` |
| `[Overload] [Overload] [Overload] [Overload] public Coroutine StartCoroutine(string methodName, object value)` |
| `public T GetComponent<T>()` |
| `[Overload] [Overload] public T GetComponentInChildren<T>()` |
| `[Overload] [Overload] public T GetComponentInChildren<T>(bool includeInactive)` |
| `[Overload] [Overload] public T GetComponentInParent<T>()` |
| `[Overload] [Overload] public T GetComponentInParent<T>(bool includeInactive)` |
| `public T[] GetComponents<T>()` |
| `[Overload] [Overload] public T[] GetComponentsInChildren<T>()` |
| `[Overload] [Overload] public T[] GetComponentsInChildren<T>(bool includeInactive)` |
| `[Overload] [Overload] public T[] GetComponentsInParent<T>()` |
| `[Overload] [Overload] public T[] GetComponentsInParent<T>(bool includeInactive)` |
| `public Type GetType()` |
| `public bool CompareTag(string tag)` |
| `[Overload] [Overload] public bool IsInvoking()` |
| `[Overload] [Overload] public bool IsInvoking(string methodName)` |
| `public bool TryGetComponent(Type type, out ref Component component)` |
| `public bool TryGetComponent<T>(out ref T component)` |
| `public int GetComponentIndex()` |
| `public int GetInstanceID()` |
| `public override bool Equals(object other)` |
| `public override int GetHashCode()` |
| `public override string ToString()` |
| `public void AddRemoveListenerHandle(AutoRemoveListenerHandle handle)` |
| `[Overload] [Overload] [Overload] [Overload] [Overload] [Overload] public void BroadcastMessage(string methodName)` |
| `[Overload] [Overload] [Overload] [Overload] [Overload] [Overload] public void BroadcastMessage(string methodName, SendMessageOptions options)` |
| `[Overload] [Overload] [Overload] [Overload] [Overload] [Overload] public void BroadcastMessage(string methodName, object parameter)` |
| `[Overload] [Overload] [Overload] [Overload] [Overload] [Overload] public void BroadcastMessage(string methodName, object parameter, SendMessageOptions options)` |
| `[Overload] [Overload] public void CancelInvoke()` |
| `[Overload] [Overload] public void CancelInvoke(string methodName)` |
| `public void GetComponents(Type type, List<Component> results)` |
| `public void GetComponents<T>(List<T> results)` |
| `[Overload] [Overload] public void GetComponentsInChildren<T>(List<T> results)` |
| `[Overload] [Overload] public void GetComponentsInChildren<T>(bool includeInactive, List<T> result)` |
| `public void GetComponentsInParent<T>(bool includeInactive, List<T> results)` |
| `public void Invoke(string methodName, float time)` |
| `public void InvokeRepeating(string methodName, float time, float repeatRate)` |
| `[Overload] [Overload] [Overload] [Overload] [Overload] [Overload] public void SendMessage(string methodName)` |
| `[Overload] [Overload] [Overload] [Overload] [Overload] [Overload] public void SendMessage(string methodName, SendMessageOptions options)` |
| `[Overload] [Overload] [Overload] [Overload] [Overload] [Overload] public void SendMessage(string methodName, object value)` |
| `[Overload] [Overload] [Overload] [Overload] [Overload] [Overload] public void SendMessage(string methodName, object value, SendMessageOptions options)` |
| `[Overload] [Overload] [Overload] [Overload] [Overload] [Overload] public void SendMessageUpwards(string methodName)` |
| `[Overload] [Overload] [Overload] [Overload] [Overload] [Overload] public void SendMessageUpwards(string methodName, SendMessageOptions options)` |
| `[Overload] [Overload] [Overload] [Overload] [Overload] [Overload] public void SendMessageUpwards(string methodName, object value)` |
| `[Overload] [Overload] [Overload] [Overload] [Overload] [Overload] public void SendMessageUpwards(string methodName, object value, SendMessageOptions options)` |
| `public void StopAllCoroutines()` |
| `[Overload] [Overload] [Overload] [Overload] public void StopCoroutine(Coroutine routine)` |
| `[Overload] [Overload] [Overload] [Overload] public void StopCoroutine(IEnumerator routine)` |
| `[Overload] [Overload] [Overload] [Overload] public void StopCoroutine(string methodName)` |
| `protected object MemberwiseClone()` |
| `protected virtual void Finalize()` |
| `protected virtual void OnAfterDeserialize()` |
| `protected virtual void OnBeforeSerialize()` |
| `protected void RemoveAllListeners()` |
| `public Coroutine StartCoroutine_Auto(IEnumerator routine)` |

### 继承的普通方法

| 普通方法名称 | 注释 | 声明方法的类 |
| :--- | :--- | :--- |
| `public Component GetComponent` |  | `UnityEngine.Component` |
| `public Component GetComponent` |  | `UnityEngine.Component` |
| `public Component GetComponentInChildren` |  | `UnityEngine.Component` |
| `public Component GetComponentInChildren` |  | `UnityEngine.Component` |
| `public Component GetComponentInParent` |  | `UnityEngine.Component` |
| `public Component GetComponentInParent` |  | `UnityEngine.Component` |
| `public Component[] GetComponents` |  | `UnityEngine.Component` |
| `public Component[] GetComponentsInChildren` |  | `UnityEngine.Component` |
| `public Component[] GetComponentsInChildren` |  | `UnityEngine.Component` |
| `public Component[] GetComponentsInParent` |  | `UnityEngine.Component` |
| `public Component[] GetComponentsInParent` |  | `UnityEngine.Component` |
| `public Coroutine StartCoroutine` |  | `UnityEngine.MonoBehaviour` |
| `public Coroutine StartCoroutine` |  | `UnityEngine.MonoBehaviour` |
| `public Coroutine StartCoroutine` |  | `UnityEngine.MonoBehaviour` |
| `public T GetComponent<T>` |  | `UnityEngine.Component` |
| `public T GetComponentInChildren<T>` |  | `UnityEngine.Component` |
| `public T GetComponentInChildren<T>` |  | `UnityEngine.Component` |
| `public T GetComponentInParent<T>` |  | `UnityEngine.Component` |
| `public T GetComponentInParent<T>` |  | `UnityEngine.Component` |
| `public T[] GetComponents<T>` |  | `UnityEngine.Component` |
| `public T[] GetComponentsInChildren<T>` |  | `UnityEngine.Component` |
| `public T[] GetComponentsInChildren<T>` |  | `UnityEngine.Component` |
| `public T[] GetComponentsInParent<T>` |  | `UnityEngine.Component` |
| `public T[] GetComponentsInParent<T>` |  | `UnityEngine.Component` |
| `public Type GetType` |  | `System.Object` |
| `public bool CompareTag` |  | `UnityEngine.Component` |
| `public bool IsInvoking` |  | `UnityEngine.MonoBehaviour` |
| `public bool IsInvoking` |  | `UnityEngine.MonoBehaviour` |
| `public bool TryGetComponent` |  | `UnityEngine.Component` |
| `public bool TryGetComponent<T>` |  | `UnityEngine.Component` |
| `public int GetComponentIndex` |  | `UnityEngine.Component` |
| `public int GetInstanceID` |  | `UnityEngine.Object` |
| `public override bool Equals` |  | `UnityEngine.Object` |
| `public override int GetHashCode` |  | `UnityEngine.Object` |
| `public override string ToString` |  | `UnityEngine.Object` |
| `public void AddRemoveListenerHandle` | 添加监听句柄，使其在调用条件满足时自动移除 | `Runestone.AesirArchitecture.RemoveListenerTrigger` |
| `public void BroadcastMessage` |  | `UnityEngine.Component` |
| `public void BroadcastMessage` |  | `UnityEngine.Component` |
| `public void BroadcastMessage` |  | `UnityEngine.Component` |
| `public void BroadcastMessage` |  | `UnityEngine.Component` |
| `public void CancelInvoke` |  | `UnityEngine.MonoBehaviour` |
| `public void CancelInvoke` |  | `UnityEngine.MonoBehaviour` |
| `public void GetComponents` |  | `UnityEngine.Component` |
| `public void GetComponents<T>` |  | `UnityEngine.Component` |
| `public void GetComponentsInChildren<T>` |  | `UnityEngine.Component` |
| `public void GetComponentsInChildren<T>` |  | `UnityEngine.Component` |
| `public void GetComponentsInParent<T>` |  | `UnityEngine.Component` |
| `public void Invoke` |  | `UnityEngine.MonoBehaviour` |
| `public void InvokeRepeating` |  | `UnityEngine.MonoBehaviour` |
| `public void SendMessage` |  | `UnityEngine.Component` |
| `public void SendMessage` |  | `UnityEngine.Component` |
| `public void SendMessage` |  | `UnityEngine.Component` |
| `public void SendMessage` |  | `UnityEngine.Component` |
| `public void SendMessageUpwards` |  | `UnityEngine.Component` |
| `public void SendMessageUpwards` |  | `UnityEngine.Component` |
| `public void SendMessageUpwards` |  | `UnityEngine.Component` |
| `public void SendMessageUpwards` |  | `UnityEngine.Component` |
| `public void StopAllCoroutines` |  | `UnityEngine.MonoBehaviour` |
| `public void StopCoroutine` |  | `UnityEngine.MonoBehaviour` |
| `public void StopCoroutine` |  | `UnityEngine.MonoBehaviour` |
| `public void StopCoroutine` |  | `UnityEngine.MonoBehaviour` |
| `protected object MemberwiseClone` |  | `System.Object` |
| `protected virtual void Finalize` |  | `System.Object` |
| `protected virtual void OnAfterDeserialize` |  | `Sirenix.OdinInspector.SerializedMonoBehaviour` |
| `protected virtual void OnBeforeSerialize` |  | `Sirenix.OdinInspector.SerializedMonoBehaviour` |
| `protected void RemoveAllListeners` | 移除所有已注册的监听并清空列表 | `Runestone.AesirArchitecture.RemoveListenerTrigger` |
| `public Coroutine StartCoroutine_Auto` |  | `UnityEngine.MonoBehaviour` |

## 属性

### 继承的属性

| 属性签名 | 注释 | 声明属性的类 | 
| :--- | :--- | :--- |
| `public CancellationToken destroyCancellationToken { get; }` |  | `UnityEngine.MonoBehaviour` |
| `public GameObject gameObject { get; }` |  | `UnityEngine.Component` |
| `public HideFlags hideFlags { get; set; }` |  | `UnityEngine.Object` |
| `public Transform transform { get; }` |  | `UnityEngine.Component` |
| `public bool enabled { get; set; }` |  | `UnityEngine.Behaviour` |
| `public bool isActiveAndEnabled { get; }` |  | `UnityEngine.Behaviour` |
| `public bool runInEditMode { get; set; }` |  | `UnityEngine.MonoBehaviour` |
| `public bool useGUILayout { get; set; }` |  | `UnityEngine.MonoBehaviour` |
| `public string name { get; set; }` |  | `UnityEngine.Object` |
| `public string tag { get; set; }` |  | `UnityEngine.Component` |
| `public Component animation { get; }` |  | `UnityEngine.Component` |
| `public Component audio { get; }` |  | `UnityEngine.Component` |
| `public Component camera { get; }` |  | `UnityEngine.Component` |
| `public Component collider { get; }` |  | `UnityEngine.Component` |
| `public Component collider2D { get; }` |  | `UnityEngine.Component` |
| `public Component constantForce { get; }` |  | `UnityEngine.Component` |
| `public Component hingeJoint { get; }` |  | `UnityEngine.Component` |
| `public Component light { get; }` |  | `UnityEngine.Component` |
| `public Component networkView { get; }` |  | `UnityEngine.Component` |
| `public Component particleSystem { get; }` |  | `UnityEngine.Component` |
| `public Component renderer { get; }` |  | `UnityEngine.Component` |
| `public Component rigidbody { get; }` |  | `UnityEngine.Component` |
| `public Component rigidbody2D { get; }` |  | `UnityEngine.Component` |

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Aesir Inspector`](https://github.com/yuumixcode/Unity-Aesir-Packages) 辅助生成。
