# Project Status & Todo

## 一、Bug 修复

| Bug | 状态 | 修复方案 |
|-----|------|---------|
| 子弹碰撞跳过玩家 Trigger Hurtbox | 已定位 | Player.prefab → Hurtbox CapsuleCollider → 取消 IsTrigger |
| `Health.TakeDamage` 不扣血 | 已定位 | `Health.cs` 写回 `m_HealthValue.Value = current` |
| `SwapWeaponServerRpc` 变量赋值 Bug | 未触发 | `NetworkVariable` 是 class，字段赋值=引用赋值，应改为值交换 |
| `SetInputActive` 被注释掉 | UI 不再锁定玩家移动 | 需要时恢复 `PlayerController.SetInputActive` |

## 二、缺失的核心功能

- [ ] **Player 死亡/重生** — `Health.OnDied` 已有，缺死亡流程（UI、重生、相机切换）
- [ ] **游戏循环**（胜负条件） — 没有 GameManager 管理 Match 状态
- [ ] **主菜单** — 只有 Boot 场景直接进游戏
- [ ] **设置持久化**（音量、语言保存） — SettingUI 只有 UI，没有 PlayerPrefs/Save
- [ ] **地图系统**（Minimap/大地图） — 没有
- [ ] **网络大厅/匹配** — 直接本地 Host/Client
- [ ] **存档系统** — 没有

## 三、功能不完整的系统

- [ ] **LoadingScreenManager** — 加载进度对接可能不完整
- [ ] **Addressables 远程构建** — 未做 Full Build，SSL 问题待解决
- [ ] **Language 切换** — 架构已确定，代码未实现
- [ ] **UIManager** — 方案已定（单面板 + ESC），代码未实现
- [ ] **Voice 对话语音** — Label + 命名约定方案已定，AssetCache.PreloadByLabel 未实现
- [ ] **NetworkEffectService** — 替代 AudioAndEffectPlayGlobal 方案已定，未实现

## 四、优先级

```
P0 修复 → P1 核心功能 → P2 完善现有系统 → P3 新功能

P0:
  - 子弹不扣血（Health.cs 写回一行）
  - Trigger Hurtbox（Prefab 一个勾选）

P1:
  - 死亡/重生流程
  - UIManager + ESC
  - 语言切换

P2:
  - Addressables Full Build
  - Dialogue voice（PreloadByLabel）
  - LoadingScreenManager 完善
  - 设置持久化

P3:
  - 游戏循环（胜负条件）
  - 主菜单
  - 网络大厅
  - 存档系统
```
