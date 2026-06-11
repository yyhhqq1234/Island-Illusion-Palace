# CLAUDE.md

## Project Overview

**幻宫：时空回响** (Island: Illusion Palace) is a 2D top-down Roguelite Action RPG built in Unity, combining real-time combat, soul summoning, alchemy crafting, and memory fragment narrative. Set in the ISLAND universe (ISLAND1 "if" timeline), the player explores procedurally generated maps across multiple cycles to collect Shana's soul fragments and determine the ending.

- **Engine**: Unity (2D, top-down perspective, XY plane)
- **Language**: C#
- **Platform**: PC (Steam)
- **Genre**: 2D Roguelite Action RPG + Strategy Alchemy + Soul Summoning

## Build & Run

- Open the project in Unity Editor at `d:\Program Files\Unity\U3Dproject\Island-Illusion-Palace`
- The `GamePlay` scene is the main gameplay scene (`Assets/Scenes/GamePlay.unity`)
- The `MainMenu` scene is the title screen (`Assets/Scenes/MainMenu.unity`)
- Build target: Windows (StandaloneWindows64)
- Use `manage_editor` MCP tool for play/pause/stop operations

## Architecture

### Project Structure

```
Assets/
├── Scripts/
│   ├── Alchemy/        # AlchemySystem, AlchemyUI, RecipeType enum
│   ├── Battle/         # BattleSystem, BattleEventManager
│   ├── Boss/           # BossAI, TimeGuardianBoss, BossRoomManager
│   ├── Core/           # HealthSystem, BurdenSystem, CharacterStats, ManaSystem
│   ├── DataAssets/     # ScriptableObjects: DropTableData, WeaponData, etc.
│   ├── Difficulty/     # DifficultyManager
│   ├── Editor/         # Custom inspectors (IntegratedMapSystemEditor, etc.)
│   ├── Ending/         # EndingSystem
│   ├── Enemy/          # EnemyAI, EnemySpawner, BossAI, SummonedCreatureAI
│   ├── Interfaces/     # Service contracts (IAlchemyService, IBurdenProvider, etc.)
│   ├── Inventory/      # InventorySystem, InventoryUI, MaterialItem, ItemSlot
│   ├── Managers/       # Singletons: GlobalEventManager, GameplayAudioManager, GameManager, ResourceManager
│   ├── Map/            # IntegratedMapSystem, SafeZoneDetector, CampfireSystem, CauldronSystem, etc.
│   ├── Memory/         # MemoryFragmentSystem, NarrativeEventSystem
│   ├── Player/         # PlayerController, WeaponSystem, AttackTrigger
│   ├── Portal/         # TimePortal, TimeRift, TimeRiftSpawner
│   ├── SaveLoad/       # SaveSystem
│   ├── Summon/         # SummonSystem, SummonWheelUI, SoulCore
│   ├── Test/           # InventoryTest
│   └── UI/             # UIManager, PauseMenu, SceneController
├── ArtMaterials/       # Sprites and textures organized by category
├── Music/              # Audio organized by type (Battle, Effect, GlobalMusic, MapMusic, UIEffect)
├── Animations/         # Animation assets (Player, Enemies, Map, Effect, Weapon)
└── I-IP markdown/      # Design documents (read-only reference)
```

### Naming Conventions

- **Namespace**: `GameSystems` is used for shared types (enums, configs like `MaterialTypeEnum`, `RecipeType`, `MapMusicType`, `MapMusicConfig`). Most scripts are in the global namespace.
- **Scripts**: PascalCase, one class per file, filename matches class name
- **Enums**: In-class for type-specific enums (e.g., `EnemyAI.EnemyType`), or in the `GameSystems` namespace for shared enums
- **Interfaces**: Prefixed with `I` (e.g., `IAlchemyService`, `IBurdenProvider`)

### Core Design Patterns

1. **Singleton Managers**: `GlobalEventManager`, `GameplayAudioManager`, `ResourceManager`, `GameManager` all follow the `Instance` + `DontDestroyOnLoad` pattern
2. **Event Bus**: `GlobalEventManager` is the central event bus. All cross-system communication goes through it — never directly call methods across systems. Subscribe via `OnEnable`/`OnDisable`, trigger via public `Trigger*` methods
3. **Interface Contracts**: `Interfaces/` folder contains service contracts like `IInventoryService`, `IHealthProvider`, `IBurdenProvider`. Components implement interfaces for loose coupling
4. **ScriptableObject Data**: `DataAssets/` contains `ScriptableObject` assets for data-driven design (drop tables, weapon data, recipe effects)

### Key Event Flow (GlobalEventManager)

```
Scene Lifecycle:  OnSceneWillLoad → OnSceneDidLoad → OnSceneWillUnload
Game State:       OnGamePaused / OnGameResumed / OnPlayerDeath
Safe Zone:        OnPlayerEnterSafeZone / OnPlayerExitSafeZone
Combat:           OnBattleStart / OnBattleEnd / OnBossDefeated / OnBossEncounter
Burden:           OnBurdenChanged / OnBurdenHigh / OnBurdenCritical / OnBurdenCleared
Map:              OnMapTypeChanged (broadcasts MapMusicType)
Music:            OnMusicStateChange (MusicState: MainMenu, Exploration, Battle, BossBattle, Camp, Silence)
```

## Key Systems

### Map System (`IntegratedMapSystem`)
- 5x5 grid of sub-maps (each 39x26 tiles)
- Map types: `MapCategory` (Natural/Human/Special/Final) + `MapType` (Forest, Wasteland, Desert, etc.)
- Generates walls, boss rooms, safe zones, treasure chests
- Broadcasts `OnMapTypeChanged` when a new map generates
- Implements `IMapSystem` and `ITimeRiftProvider`

### Player (`PlayerController`)
- WASD movement, Shift to run, Space to dash (invincibility frames)
- Uses `Rigidbody2D` (Dynamic) — control via velocity, never directly modify Transform
- References: WeaponSystem, SummonSystem, BurdenSystem, InventorySystem, CharacterStats

### Combat
- **WeaponSystem**: Sword/Staff/Scythe/Crystal Armament, each with element type
- **Weakness system**: 7 elements (Frost, Fire, Thunder, Water, Soul, Holy, Physical), ×1.8 weakness, ×0.4 resistance
- **BattleEventManager**: Battle lifecycle events
- **Damage**: AttackTrigger on weapon colliders

### Boss System
- `BossAI` base class → `TimeGuardianBoss` (with time-warp mechanics, slow fields, health regen)
- `BossRoomManager`: Spawns boss, handles defeat rewards (drops + TimePortal)
- TimePortal spawns at boss room center after defeat

### Summon System
- Enemies drop Soul Cores (White/Blue/Gold rarity)
- R key for summon wheel, F for quick summon, Alt to recall all
- Max 3 active summons, 2-second global cooldown
- `SummonSystem` manages slots and cooldowns

### Alchemy System (`AlchemySystem`)
- 24 materials across Basic/Rare/Epic/Legendary tiers (alchemical value 1-40)
- 14 recipes (4 basic known, 10 hidden to discover)
- Priority: match recipes first, then material synthesis, failure returns primes
- Success rate affected by Wisdom, Burden, Memory Fragments

### Burden System (`BurdenSystem`)
- 0-100 range; >70 = high (stat penalties), >90 = critical, 100 = forced teleport to campfire
- Increases from summon use, skill use, memory fragment activation
- Restored by campfires and alchemy items
- `IIPConstants` defines thresholds: `BurdenHighThreshold = 70`, `BurdenCriticalThreshold = 90`

### Audio System (`GameplayAudioManager`)
- Dynamic music switching based on: MapType → BurdenLevel → MusicState
- Music hierarchy: SafeZone > Boss > Battle > Exploration
- `MapMusicConfig` per map type in Inspector, fallback to defaults
- Map-type change triggers music refresh via `OnMapTypeChanged` event

### Portal System
- **TimePortal**: Stable connector, spawns at boss room center after defeat, connects 2-3 specific map types
- **TimeRift**: Unstable random portal, spawns by `TimeRiftSpawner` with configurable probability, lifetime 45s
- **TimeRiftSpawner**: Avoids safe zones, boss rooms, and grid map obstacles when spawning

### Save/Load
- `SaveSystem`: Serializes player state, inventory, map progress
- Multi-cycle progression: up to 30 attribute points carry over between cycles

## Development Rules

### Unity 2D Best Practices
- Always use `Rigidbody2D` + `Collider2D`, control via velocity/forces in `FixedUpdate`
- Z-axis used only for rendering sorting; set Transparency Sort Mode to Custom Axis (0,1,0)
- Camera must be Orthographic; use `manage_camera` tool for adjustments
- Tilemap system preferred for map construction

### Code Style
- C# comments in Chinese are acceptable (team is Chinese-speaking)
- `[Header]` and `[Tooltip]` attributes for Inspector: Header in Chinese, field names in English, Tooltip in Chinese
- Use `Debug.Log` with `[ClassName]` prefix for runtime diagnostics
- Follow single responsibility: one class = one responsibility

### MCP Tool Usage (Unity Editor Interaction)
- Use `mcp_unityMCP_batch_execute` for bulk operations (creating multiple objects, adding components)
- Use `mcp_unityMCP_find_gameobjects` for searching, `mcp_unityMCP_manage_gameobject` for CRUD
- Use `mcp_unityMCP_manage_components` for component management
- Use `mcp_unityMCP_manage_asset` for asset operations
- Use `mcp_unityMCP_execute_code` for ad-hoc C# code execution in the Editor
- Never modify scenes during Play Mode
- Always confirm with user before deleting/overwriting assets

### Design Document Reference
- All game design documents are in `Assets/I-IP markdown/` — treat as read-only canonical reference
- When implementing features, cross-reference the relevant system breakdown document
- If code conflicts with markdown specs, fix code, never modify markdown

## Important Constants (`IIPConstants`)
```csharp
MaxActiveSummons = 3
SummonGlobalCooldown = 2f
MaxMemoryFragmentSlots = 3
BurdenHighThreshold = 70f
BurdenCriticalThreshold = 90f
BurdenAbsoluteMax = 100f
MaxEnhancementLevel = 5
NewbieProtectionCount = 3
```

## Tags & Layers
- Tags: `Player`, `Enemy`, `SummonedCreature`, `SafeZone`, `Campfire`
- Layers: `Player`, `Enemy`, `Default`