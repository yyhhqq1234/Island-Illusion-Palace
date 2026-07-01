# 地图音乐配置指南

## 系统概述

本音乐配置系统根据《幻宫：时空回响》的地图清单设计，支持：
- 每张地图根据灵魂负担等级播放不同的探索音乐
- 安全区（营火）专属音乐
- 独立的战斗音乐和Boss战斗音乐
- 特殊区域和最终区域的特殊配置

## 地图分类与音乐配置

### 一、自然地图（7张）

| 地图名称 | 地图类型 | 安全区 | 低负担探索(0-30) | 中负担探索(31-60) | 高负担探索(61-100) | 战斗曲 | Boss战 |
|---------|---------|-------|-----------------|------------------|-------------------|-------|--------|
| 森林 | Forest | 营火曲 | 轻盈神秘 | 压抑紧张 | 恐惧不安 | 战斗曲 | Boss曲 |
| 荒原 | Wasteland | 营火曲 | 荒凉孤独 | 压抑紧张 | 恐惧不安 | 战斗曲 | Boss曲 |
| 沙漠 | Desert | 营火曲 | 空旷神秘 | 压抑紧张 | 恐惧不安 | 战斗曲 | Boss曲 |
| 岩地 | RockyLand | 营火曲 | 崎岖险峻 | 压抑紧张 | 恐惧不安 | 战斗曲 | Boss曲 |
| 湿地 | Wetland | 营火曲 | 幽暗潮湿 | 压抑紧张 | 恐惧不安 | 战斗曲 | Boss曲 |
| 冰原 | IcePlains | 营火曲 | 寒冷孤寂 | 压抑紧张 | 恐惧不安 | 战斗曲 | Boss曲 |
| 火山地 | VolcanicLand | 营火曲 | 炽热危险 | 压抑紧张 | 恐惧不安 | 战斗曲 | Boss曲 |

### 二、人文地图（3张）

| 地图名称 | 地图类型 | 安全区 | 低负担探索 | 中负担探索 | 高负担探索 | 战斗曲 | Boss战 |
|---------|---------|-------|-----------|-----------|-----------|-------|--------|
| 废墟都市 | RuinedCity | 营火曲 | 废墟追忆 | 压抑紧张 | 恐惧不安 | 战斗曲 | Boss曲 |
| 遗忘庄园 | ForgottenManor | 营火曲 | 诡异神秘 | 压抑紧张 | 恐惧不安 | 战斗曲 | Boss曲 |
| 古代神殿 | AncientTemple | 营火曲 | 神圣古老 | 压抑紧张 | 恐惧不安 | 战斗曲 | Boss曲 |

### 三、特殊区域（2个）

特殊区域只有**一种探索音乐**，不使用负担等级区分。

| 地图名称 | 地图类型 | 探索音乐 | 战斗曲 | Boss战 |
|---------|---------|---------|-------|--------|
| 实验室碎片 | LabFragment | 炼金科技感 | 战斗曲 | Boss曲 |
| 记忆碎片区域 | MemoryFragmentArea | 记忆情感 | 战斗曲 | Boss曲 |

### 四、最终区域（1个）

最终区域只有**战斗音乐**，无探索音乐和Boss战音乐。

| 地图名称 | 地图类型 | 战斗曲 |
|---------|---------|-------|
| 真理时空回廊入口 | TruthTemporalCorridor | 最终决战曲 |

### 五、全局音乐

| 音乐类型 | 说明 |
|---------|-----|
| 主界面主题曲 | MainMenu |
| 片尾曲 | Ending |

## 负担等级音乐设计建议

### 低负担（0-30）- "希望与探索"
- **情绪**: 轻盈、神秘、充满希望
- **节奏**: 缓慢到中等
- **乐器**: 弦乐为主，辅以空灵 synth
- **氛围**: 适合探索未知

### 中负担（31-60）- "压力与紧张"
- **情绪**: 压抑、紧张、不安
- **节奏**: 中等加快
- **乐器**: 铜管加入，鼓点加重
- **氛围**: 危机感上升

### 高负担（61-100）- "恐惧与危险"
- **情绪**: 恐惧、压迫、危险
- **节奏**: 快速、激烈
- **乐器**: 金属打击乐、低频 bass
- **氛围**: 灵魂即将崩溃

## Unity编辑器配置步骤

### 1. 创建 MapMusicDatabase
1. 在 `Assets/ScriptableObjects/` 目录下
2. 右键 → Create → GameSystems → Map Music Database
3. 命名为 `MapMusicDatabase`

### 2. 配置默认音乐
在 MapMusicDatabase 中配置：
- `MainMenu Music` - 主界面主题曲
- `Ending Music` - 片尾曲
- `Default Exploration Music (Low/Medium/High Burden)` - 各负担等级默认探索曲
- `Default Battle Music` - 默认战斗曲
- `Default Boss Battle Music` - 默认Boss战曲
- `Default Safe Zone Music` - 默认营火曲

### 3. 配置各地图音乐
在 `MapConfigs` 列表中添加 MapMusicConfig：

#### 自然地图配置示例（森林）
```
MapType: Forest
MapName: 森林
SafeZoneMusic: [营火曲]
ExplorationMusicLowBurden: [森林-低负担探索曲]
ExplorationMusicMediumBurden: [森林-中负担探索曲]
ExplorationMusicHighBurden: [森林-高负担探索曲]
BattleMusic: [森林-战斗曲]
BossBattleMusic: [时空守护者Boss战曲]
IsSpecialArea: false
IsFinalArea: false
```

#### 特殊区域配置示例（记忆碎片区域）
```
MapType: MemoryFragmentArea
MapName: 记忆碎片区域
SafeZoneMusic: [营火曲]
ExplorationMusic: [记忆情感探索曲]  // 只用一种
BattleMusic: [记忆区域战斗曲]
BossBattleMusic: [记忆守护者Boss战曲]
IsSpecialArea: true
IsFinalArea: false
```

#### 最终区域配置示例（真理时空回廊入口）
```
MapType: TruthTemporalCorridor
MapName: 真理时空回廊入口
SafeZoneMusic: null
ExplorationMusic: null  // 无探索曲
BattleMusic: [最终决战曲]
BossBattleMusic: null  // 使用最终Boss自己的音乐
IsSpecialArea: false
IsFinalArea: true
```

## 音乐风格建议

### 时空守护者Boss战
- **主题**: 空间撕裂、规则抗争、史诗宿命
- **风格**: Epic orchestral + industrial metal
- **元素**: 空间撕裂音效、时钟滴答、命运主题
- **参考提示词**: 见 `Suno提示词-时空守护者.txt`

### 主界面主题曲
- **主题**: 幻宫世界、时空回响
- **风格**: 空灵神秘、史诗前奏
- **元素**: 空灵 synth、轻微弦乐、神秘氛围

### 记忆碎片区域
- **主题**: 莎娜的记忆、情感共鸣
- **风格**: 情感深沉、回忆感
- **元素**: 钢琴、弦乐、情感旋律

### 实验室碎片
- **主题**: 晶能科技、炼金主题
- **风格**: 科技感、机械节奏
- **元素**: 电子音效、机械节拍、科学氛围

## 文件结构

```
Assets/
├── ScriptableObjects/
│   └── MapMusicDatabase.cs          // 音乐数据库 ScriptableObject
├── Scripts/
│   ├── Managers/
│   │   └── GameplayAudioManager.cs  // 音频管理器
│   ├── Components/
│   │   └── MapMusicPreset.cs        // 地图音乐预制组件
│   └── GameSystems/
│       └── MapMusicDatabase.cs      // 配置数据结构
└── Audio/
    ├── Music/
    │   ├── MainMenu/
    │   ├── Exploration/
    │   │   ├── Forest/
    │   │   ├── Wasteland/
    │   │   └── ...
    │   ├── Battle/
    │   ├── BossBattle/
    │   ├── Camp/
    │   └── Ending/
    └── SFX/
```

## 负担系统与音乐联动

音乐系统会根据 `BurdenSystem` 的负担值自动切换探索音乐：

```csharp
BurdenLevel CalculateBurdenLevel(float burden)
{
    if (burden <= 30f) return BurdenLevel.Low;      // 0-30
    if (burden <= 60f) return BurdenLevel.Medium;   // 31-60
    return BurdenLevel.High;                        // 61-100
}
```

当负担等级变化时（通过 `OnBurdenChanged` 事件），如果当前状态是探索状态，会自动切换到对应等级的探索音乐。

---
**代码实现状态 (2026-06-23)**

| 模块 | 状态 | 说明 |
|------|------|------|
| 音乐系统架构(3音源) | ✅ 已完成 | `GameplayAudioManager.cs` music/SFX/UI 三音源 |
| 按地图类型切换音乐 | ✅ 已完成 | 12+地图类型→MapMusicConfig 映射 |
| 负担等级联动(三段) | ✅ 已完成 | BurdenLevel Low/Medium/High 对应不同音乐 |
| 战斗/探索/营火状态 | ✅ 已完成 | MusicState: Battle/Exploration/Camp/Boss |
| 安全区覆盖Boss范围 | ✅ 已完成 | 安全区 > Boss > 战斗 > 探索 优先级 |
| 事件驱动切换 | ✅ 已完成 | OnMapTypeChanged + OnBurdenChanged + OnMusicStateChange |
| 26个音频文件引用 | ✅ 已完成 | `ResourceManager.cs` 音乐路径配置 |
| MapType→MusicType转换 | ✅ 已完成 | `IntegratedMapSystem.cs` 映射 |
