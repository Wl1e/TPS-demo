# TPS-Demo 多人化迁移方案（最终状态）

---

## ✅ 全部已完成

| 系统 | 文件 | 现状 |
|------|------|------|
| Network 基础 | Player Prefab | `NetworkObject` + `PlayerMovement : NetworkTransform` + `DefaultPlayerPrefab` |
| Player Spawn | `NetworkManager.DefaultPlayerPrefab` | 标准机制，无需自定义 |
| 玩家输入 | `PlayerInputHandler.cs` | `NetworkBehaviour` + `IsOwner` |
| 相机 | `CameraController.cs` | `NetworkBehaviour` + `IsOwner` |
| 移动同步 | `PlayerMovement.cs` | `NetworkTransform` 子类 |
| 血量同步 | `Health.cs` | `NetworkBehaviour` + `NetworkVariable<RangedFloat>` + `OnValueChanged` |
| 开火仲裁 | `Weapon.cs` | `NetworkBehaviour` + `FireServerRpc` / `FireClientRpc` |
| 弹药同步 | `AmmoHandler.cs` | `NetworkBehaviour` + `NetworkVariable<int>` |
| 切枪/换弹 | `WeaponManager.cs` | `NetworkBehaviour` + `TrySwitchFirearmServerRpc` + `TryReloadServerRpc` + `NetworkVariable<int>` + `NetworkVariable<bool>` |
| 敌人 AI | `EnemyController.cs` | `NetworkBehaviour`，服务端驱动 |
| 武器槽 | `Loadout.cs` | `NetworkBehaviour` |
| 子弹 | Bullet 预制体 | 已有 `NetworkObject` |
| 手雷 | Grenade 预制体 | 已有 `NetworkObject` |
| 商店/对话 | `ShopManager.cs`, `DialogueSystem.cs` | 已有 `IsServer` 守卫 |
| UI 数据源 | `PlayerDataProxy.cs` | 单例模式，Owner 用，无需多玩家支持 |
| `EventManager` | `EventManager.cs` | 纯客户端 UI 事件走 EventManager；网络数据已走 `NetworkVariable.OnValueChanged` |

---

## ❌ 不需要处理

| 之前认为要改 | 实际原因 |
|-------------|---------|
| `ClientNetworkTransform` | `PlayerMovement` 直接继承 `NetworkTransform`，工作正常 |
| `PlayerManager` | 不需要，`DefaultPlayerPrefab` 自动 Spawn |
| `PlayerController` 加 RPC | 战斗 RPC 在 `Weapon.cs` / `WeaponManager.cs`，职责分离正确 |
| `EventManager` 隔离 | 跨进程不共享内存；Host 端也无需隔离（网络数据已走 NetworkVariable，纯 UI 事件无害） |
| `PlayerDataProxy` 多玩家 | 纯 Owner 用，不需要 |
| `Actor` ID 分配 | 待确认，但大概率单机逻辑已够用 |
| `Inventory` 背包同步 | 待确认，但属于玩家本地数据 |
