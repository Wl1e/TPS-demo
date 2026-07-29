# TPS Demo — 第三人称射击游戏（Unity Netcode + 行为树 AI + 技能系统）

一个 **Unity 6** 全栈第三人称射击 Demo，涵盖**多人网络同步、行为树 AI、Boss 技能、任务系统**、**Addressables 热更新**等功能。

> **260+ C# 脚本** | **Unity 6** | **Universal Render Pipeline** | **Netcode for GameObjects 2.12**

---

## 核心功能

- **Player 控制** — FSM 状态机控制角色移动（走 / 跑 / 蹲 / 跳 / 攀爬），瞄准 / 射击 / 换弹 / 扔手雷
- **多人网络同步** — Server-authoritative 架构，NetworkVariable 同步血量、装备、武器；ClientRpc / ServerRpc 分发动作
- **AI 行为树** — Unity Behavior（Selector / ParallelAll / ObserverAbort），FindTarget → Navigate → Attack 带记忆
- **Boss 技能** — Boss 有 SkillBase 子类（Charge 冲撞 / SkyDive 砸地），Windup → Active → Recovery 三阶段、HitBox 判定
- **Inventory / 商店 / 任务** — 拾取道具、武器配备、配件系统（消音器 / 瞄具），商店买卖，任务系统（Kill / Pickup）
- **Addressables 热更新** — 远程 catalog 检测 + 资源下载 + 缓存清空，实现 Npc 语音和 Poster 实时切换
- **本地化** — 中英文切换，Npc 语音 / 文字同步按 Label 加载 AA 资源
- **UI 系统** — HealthBar、Inventory、Shop、QuestTracker、SettingUI，完全事件驱动
- **地图系统** — 多个关卡 + 地图目标，不同 BGM 随 MapID 切换
- **Setting** — 音量 / 帧率控制 / 语言控制
- **虚空死亡** — VoidDeathChecker（Server 权威）检测角色 Y 轴过低 → 触发死亡

---

## 架构设计

```
Input Layer     →  PlayerInputHandler (输入系统)
Logic Layer     →  FSM States | 战斗逻辑 | AI 行为树 | Quest 管线
UI Layer        →  EventManager 解耦 → UI 组件监听事件自行刷新
Data Layer      →  ScriptableObject 驱动数据：ItemData / SkillDatabase / QuestConfig
```

### 关键技术决策

| 决策 | 原因 |
|------|------|
| 玩家 FSM 状态机 | 攀爬 / 跳跃依赖物理的 CharacterController 状态切换，不能硬编码延迟 |
| PlayableController | 替换 Animator 实现手动动画混合与过渡，支持网络同步 |
| SkillController 三段状态 | Windup(前摇) → Active(Hitbox 开启) → Recovery(后摇)，每个技能独立配置 |
| Addressables 热更 | UpdateCatalogs 前调用 ClearDependencyCacheAsync 防止旧 bundle 残留 |

---

## 技术栈

| 系统 | 技术 |
|------|------|
| **引擎** | Unity 6 |
| **渲染** | Universal Render Pipeline (URP) |
| **网络** | Netcode for GameObjects 2.12 |
| **AI** | Unity Behavior 1.0 (行为树) |
| **动画** | PlayableController (AnimationMixerPlayable) |
| **输入** | New Input System |
| **UI** | uGUI + TextMeshPro |
| **资产管理** | Addressables System 2.9 |

---

## 目录结构

```
Assets/
├── Scripts/               # 244 个 C# 脚本
│   ├── Player/            # PlayerController、Movement、Animator、Combat、Attachment
│   ├── Combat/            # Skill (Boss 技能)、Attacker、Hitbox、HurtBox
│   ├── Enemy/             # EnemyController、BossArea
│   ├── AI/                # 行为树 Actions、Conditions、EventChannel
│   ├── Shop/              # 商店、商店配置
│   ├── Quest/             # 任务 & 目标管线
│   ├── Map/               # MapManager、地图目标
│   ├── Audio/             # Director、AEPlayer、BGMPlayer
│   ├── UI/                # HUD、Inventory、Shop、Quest、MapUI、Setting
│   ├── Network/           # GameNetworkManager
│   ├── Tools/             # AssetCache、ResourceManager、HotUpdateChecker
│   ├── Events/            # 事件系统 EventManager
│   └── Shared/            # Health、DamageInfo、PlayableController、RangedFloat
├── Scenes/                # Boot / Hub / Combat1 等场景
└── AddressableAssetsData/ # Addressables 效果&资源
```

---

## 遇到的关键问题及解决

| 问题 | 解决方案 |
|------|---------|
| 热更新重复加载同一 bundle 导致失败 | UpdateCatalogs 前调用 `ClearDependencyCacheAsync(catalogs, false)` |
| 切语言连续加载报 `Invalid operation handle` | ReleaseByLabel 同时 Remove dict 项，回调里加 null checking |
| Weapon 在 Attacher 中已销毁仍被 Attach | Attach 两次被触前判断 GameObject 是否存活 |
| NetworkObjectReference.TryGet 在 OnValueChange 中失败 | SpawnManager 注册晚于 NetworkVariable 反序列化 → 延迟一帧重试 |
| Enemy 共用 channel 的问题 | 不去改 channel，直接把目标设到 Enemy Blackboard 变量 |

---

## 项目能力展示

- 能独立设计**完整的游戏架构**（Input → Logic → UI → Data 分层）
- 熟练运用 **Netcode 网络同步**、RPC、Owner 权限设计
- 能解决 BT**行为树阻塞**、**channel 共享**等运行时设计问题
- 有**自定义 Playable 动画系统**能力：混合、过渡、网络同步
- 能独立构建 **Boss 多阶段技能系统**，继承 SkillBase 扩展

---


## 演示视频
[多人TPS游戏演示](https://www.bilibili.com/video/BV1KK3k6bE98/?share_source=copy_web&vd_source=804c91e1c8180e51b97beec1333ade3b)

## 代码统计(Cloc)
|Language|files|blank|comment|code|
|--------|-----|-----|-------|---|
|C#|243|2466|1415|14048|
|SUM|243|2466|1415|14048|