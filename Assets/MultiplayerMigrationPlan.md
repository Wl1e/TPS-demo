# TPS-Demo 多人化迁移方案

---

## 前置配置

### 1. Player Prefab 根节点加 `NetworkObject`
**理由**: NetCode 所有网络同步的前提——没有它，`NetworkBehaviour` 不生效、不能 Spawn、不能同步 Transform。

### 2. Player Prefab 根节点加 `ClientNetworkTransform`（独立组件）
**理由**: 不使用 `PlayerMovement : NetworkTransform` 的继承方式——组合优于继承，且避免 CustomEditor 被子类属性消失问题。`ClientNetworkTransform` 默认 `AuthoritateMode = Owner`，适合客户端权威移动。

### 3. 注册到 `NetworkManager.PrefabList`（或 `DefaultNetworkPrefabs.asset`）
**理由**: 未注册的 Prefab 无法通过 `Spawn` / `SpawnAsPlayerObject` 生成。

### 4. `PlayerMovement` 保持继承 `MonoBehaviour`
**理由**: 职责分离——`PlayerMovement` 专注移动逻辑和物理计算（`CharacterController.Move`），`ClientNetworkTransform` 专注位置/旋转的网络同步。不混在一起。

---

## 改为 NetworkBehaviour 的文件

| # | 文件 | 理由 | 新增内容 |
|---|------|------|----------|
| 1 | `Scripts/Player/PlayerController.cs` | 玩家中枢——承载所有 `[ServerRpc]` / `[ClientRpc]`，协调输入→网络交互。非 Owner 端只需查询只读引用，不需要执行输入驱动逻辑。 | `OnNetworkSpawn` 内判断 `IsOwner` 分叉初始化；新增 `FireServerRpc`、`ReloadServerRpc`、`SwitchWeaponServerRpc`、`TakeDamageClientRpc`；`IsSpawned` 守卫 |
| 2 | `Scripts/Shared/Health.cs` | 血量是所有玩家必须看到的同步状态，服务端需权威计算伤害然后同步给所有客户端。 | `NetworkVariable<float>` 代替 `m_HealthValue.Value`；订阅 `OnValueChanged` 驱动 UI 更新；`TakeDamage` 加 `[ServerRpc]` |
| 3 | `Scripts/Combat/Gun/Weapon.cs` | 开火动作是"有外部观察者"的行为，需服务端仲裁是否合法、生成子弹、广播效果。 | `OnFire` 改为 `FireServerRpc`；弹药消耗在服务端执行；`[ClientRpc]` 广播枪口特效和音效给所有客户端 |
| 4 | `Scripts/Combat/Gun/AmmoHandler.cs` | 弹药数是 UI 显示 + 换弹逻辑的核心数据，非 Owner 也需要看到（别人换弹时弹药变化要同步）。 | `NetworkVariable<int>` 存储当前弹匣数；`ComsumeAmmo` 和 `EndReload` 只在服务端执行 |
| 5 | `Scripts/Enemy/EnemyController.cs` | 敌人 AI 由服务端统一驱动，保证所有客户端看到相同的敌人行为。生成/死亡由服务端控制。 | `OnNetworkSpawn` 中 `if (IsServer) { 启动 AI }`；`OnDied` 改为 `Destroy` 或 `Despawn`；`Health` 的 `NetworkVariable` 自动同步血条 |
| 6 | `Scripts/Combat/Bullet/NormalBulletController.cs` | 子弹是动态生成的网络对象，需要在所有客户端显示飞行轨迹和命中。 | 子弹预制体加 `NetworkObject`；服务端 `Spawn`；飞行过程中非 Owner 不计算物理，只接收网络位置；命中 → 服务端计算伤害 → 广播结果 |
| 7 | `Scripts/Combat/Bullet/LaserBulletController.cs` | 同上，只是 hitscan 方式不同（瞬时命中），但视觉效果仍需网络同步。 | 同子弹预制体加 `NetworkObject`；hitscan 结果由服务端 `[ServerRpc]` 处理 |
| 8 | `Scripts/Combat/Grenade/Grenade.cs` | 手雷是动态生成的网络对象，飞行轨迹和爆炸效果需所有人看到。 | 预制体加 `NetworkObject`；`[ServerRpc]` 请求投掷 → 服务器生成 → 爆炸伤害由服务端计算 |
| 9 | `Scripts/Player/Loadout.cs` | 已完成（当前已是 `NetworkBehaviour`）。武器槽内容影响其他玩家看到的角色武器模型。 | 已完成 |
| 10 | `Scripts/Player/InteractionController.cs` | 交互需 Owner 触发但服务端仲裁（防作弊），拾取结果需广播。 | `TryInteract` → `[ServerRpc]`；交互结果 → `[ClientRpc]` 广播 |
| 11 | `Scripts/Item/ItemPickup.cs` | 世界掉落物被多人看到，拾取后需在所有客户端消失。 | 加 `NetworkObject`；拾取 → `Despawn`；`NetworkVariable<bool>` 标记是否被拾取 |
| 12 | `Scripts/Inventory/Inventory.cs` | 背包内容影响武器可切换性和弹药可用性，需要网络同步。 | `NetworkList<int>` 存储物品 ID 列表；添加/移除通过 `[ServerRpc]` |
| 13 | `Scripts/Actor.cs` | Actor ID 分配需网络层统一管理，避免 Host 和 Client 各自分配导致碰撞。 | `NetworkVariable<int>` 替代 `NextId.GetNextId()`；改为由服务端分配并同步 |

---

## 保留 MonoBehaviour 不需要改的文件

| 文件 | 理由 |
|------|------|
| `CameraController.cs` | ✅ 已是 `NetworkBehaviour`，且已做了 `IsOwner` 守卫 |
| `PlayerInputHandler.cs` | ✅ 已是 `NetworkBehaviour`，且已做了 `IsOwner` 守卫 |
| `CombatController.cs` | 本地编排——切枪/开火输入仅触发 `PlayerController` 上的 RPC，本身不持有需要同步的状态 |
| `WeaponManager.cs` | 同上——本地逻辑编排，弹药同步由 `AmmoHandler.NetworkVariable` 完成，开火由 `Weapon` 的 RPC 完成 |
| `ThrowController.cs` | 本地逻辑编排——投掷仅触发 `PlayerController` 上的 `ThrowGrenadeServerRpc` |
| `ClimbContoller.cs` | 本地射线检测——攀爬判定是纯客户端物理，结果通过 `PlayerMovement` 的位置变化自然同步 |
| `AnimatorController.cs` | 动画同步用 `NetworkAnimator` 组件（挂在 Prefab 上即可），不需要改脚本 |
| `PlayerStateMachine.cs` | 纯客户端本地状态机，状态变化的结果通过 Transform 和 NetworkAnimator 自动体现 |
| `UIController.cs` | 纯客户端渲染——别人的屏幕不需要你的 UI，保持 MonoBehaviour |
| `HealthUI.cs` / `CrosshairUI.cs` / `WeaponHUDUI.cs` | 同上——改为监听 `NetworkVariable.OnValueChanged` 而非全局事件即可 |
| `ShopManager.cs` | 已完成（当前已有 `IsServer` 守卫） |
| `DialogueSystem.cs` | 同上 |
| `NpcBase.cs` | 纯客户端交互触发，对话/购物请求通过 `[ServerRpc]` 转发 |

---

## 需要新增的文件

### 1. `Scripts/NetworkGameManager.cs` — 替换 `GameFlowManager`
**内容**: 继承 `NetworkBehaviour`，处理连接/断连回调，动态 Spawn/Despawn 玩家，管理全局网络游戏状态。
```csharp
// 核心逻辑
OnClientConnectedCallback(clientId) → SpawnPlayerServerRpc(clientId)
OnClientDisconnectedCallback(clientId) → DespawnPlayer(clientId)
```

### 2. `Scripts/PlayerDataProxy.cs` 改造（非新增文件，但需大幅重写）
**内容**: `PlayerDataProxy` 目前是全局单例，只能存一个 Player。改造成支持多个玩家数据的查找表（按 `NetworkObjectId` 索引），UI 组件根据 `IsOwner` 找自己的数据。

### 3. 自定义 Editor（可选）
**内容**: 如果还是要走 `PlayerMovement : NetworkTransform` 的路，需要参考 MultiplayerTPS 项目的 `CoreMovementEditor` 模板写一个 CustomEditor，否则子类字段在 Inspector 里不显示。

---

## 需要改造的全局系统

### `EventManager` 改造
**当前问题**: 静态全局事件总线——Host 端一个 NPC 触发事件会被所有 Client 的监听器收到（因为进程内共享静态变量）。

**改造方案**:
| 场景 | 处理方式 |
|------|----------|
| 玩家自身变化（血量、弹药、切枪） | 不用全局事件，直接监听 `NetworkVariable.OnValueChanged` |
| 跨玩家广播（子弹命中、死亡） | 改为 `[ClientRpc]` 驱动，在 RPC 内手动触发本地 UI/音效回调 |
| 纯客户端 UI 事件（开背包、关菜单） | 保持 `EventManager` 但限制注册范围——只在 `IsOwner` 的 `OnNetworkSpawn`/`OnNetworkDespawn` 里注册/注销，避免跨客户端泄漏 |

### `PlayerManager` 改造
**当前**: `Instantiate(m_PlayerPrefab)` — 纯本地生成。

**改造**: 改为网络 Spawn：
```csharp
public void SpawnPlayer(ulong clientId)
{
    var instance = Instantiate(m_PlayerPrefab);
    instance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
}
```

### `GameFlowManager` 改造
**当前**: `Start()` 里直接 Spawn Player + 初始化 UI。

**改造**: 改为由 `NetworkGameManager` 在 `OnClientConnectedCallback` 中触发。UI 初始化移到每个 Player 自己 `OnNetworkSpawn` 的 `IsOwner` 分支里。

---

## 伤害系统改造流程

```
旧流程（单机）:
  Bullet.OnHit → target.Damageable.InflictDamage() → Health.TakeDamage() → EventManager.Broadcast

新流程（网络）:
  Bullet.OnHit (Client) 
    → [ServerRpc] HitServerRpc(targetId, damage)
    → Server: Health.TakeDamage() 修改 NetworkVariable
    → NetworkVariable.OnValueChanged 自动同步所有 Client
    → Client: 本地触发 UI 更新、受伤动画、死亡逻辑
```

---

## 总结优先级

| 优先级 | 步骤 | 涉及文件 |
|--------|------|----------|
| 🔴 P0 | Prefab 挂 NetworkObject + ClientNetworkTransform + 注册 PrefabList | Player Prefab |
| 🔴 P0 | `PlayerManager` 改为网络 Spawn | PlayerManager.cs |
| 🔴 P0 | 新建 `NetworkGameManager` | 新文件 |
| 🟡 P1 | `Health.cs` → NetworkBehaviour + NetworkVariable | Health.cs |
| 🟡 P1 | `PlayerController.cs` → NetworkBehaviour + RPC | PlayerController.cs |
| 🟡 P1 | `Weapon.cs` + `AmmoHandler.cs` → NetworkBehaviour | Weapon.cs, AmmoHandler.cs |
| 🟡 P1 | `EnemyController.cs` → NetworkBehaviour | EnemyController.cs |
| 🟢 P2 | `Bullet/Grenade` → NetworkObject | BulletController, Grenade |
| 🟢 P2 | `Inventory/Loadout` → NetworkVariable/NetworkList | Inventory.cs, Loadout.cs |
| 🟢 P2 | `EventManager` 隔离改造 | 全局 |
| 🟢 P2 | `PlayerDataProxy` 多玩家支持 | PlayerDataProxy.cs |
| ⚪ P3 | `InteractionController` / `ItemPickup` | 交互系统 |
