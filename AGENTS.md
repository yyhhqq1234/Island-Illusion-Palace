# AGENTS.md

## Project: 幻宫：时空回响 (Island: Illusion Palace)

A Unity 2D top-down Roguelite Action RPG. Read [CLAUDE.md](./CLAUDE.md) for the full architecture and system reference.

## Agent Workflow

### Before Any Code Change
1. Read the relevant script(s) with `Read` tool first — never edit code you haven't read
2. Check `Assets/I-IP markdown/` for the design document if the task involves game mechanics
3. Use `mcp_unityMCP_find_gameobjects` or `mcp_unityMCP_manage_asset` to inspect scene/asset state before modifying

### Making Changes
1. **Prefer editing existing files** — do not create new scripts unless absolutely necessary
2. **Use `mcp_unityMCP_batch_execute`** for bulk Unity operations (creating objects, adding components, etc.)
3. **Use `Edit` tool** for code modifications (not shell commands)
4. After code changes, check for compilation errors via `GetDiagnostics`

### After Changes
1. Verify no compilation errors exist
2. If runtime testing is needed, suggest the user enter Play Mode in Unity

## Key Constraints

### DO NOT
- Modify files in `Assets/I-IP markdown/` — these are read-only design references
- Modify the scene during Play Mode
- Delete or overwrite assets without user confirmation
- Create new ScriptableObject files unless the user explicitly requests it
- Create documentation files (\*.md) unless the user explicitly requests it

### Unity 2D Rules
- Movement must use `Rigidbody2D` velocity, never direct Transform modification
- Z-axis is for rendering sorting only, not gameplay positioning
- Camera must stay orthographic
- Use Tilemap system for map construction when possible

### Code Style
- Inspector fields: English names, `[Header("中文")]` for grouping, `[Tooltip("中文提示")]` for descriptions
- Debug logs: `Debug.Log($"[ClassName] message")` pattern
- Shared types (enums, config classes) go in the `GameSystems` namespace
- Comments can be in Chinese or English — Chinese preferred for team communication

## Event-Driven Communication

All inter-system communication flows through `GlobalEventManager.Instance`:

```
To broadcast:  GlobalEventManager.Instance.TriggerXxx(args)
To listen:     GlobalEventManager.Instance.OnXxx += handler   (in OnEnable)
               GlobalEventManager.Instance.OnXxx -= handler   (in OnDisable)
```

Key events available:
- `OnMapTypeChanged(MapMusicType)` — map type changes
- `OnMusicStateChange(MusicState)` — music state request
- `OnBurdenChanged(float)` — burden value changed
- `OnBattleStart/End(GameObject)` — combat lifecycle
- `OnBossDefeated(GameObject)` — boss killed
- `OnPlayerEnterSafeZone/ExitSafeZone(GameObject)` — safe zone detection

## MCP Tool Quick Reference

| Task | Tool |
|------|------|
| Find GameObjects | `mcp_unityMCP_find_gameobjects` |
| Create/Modify/Delete GameObjects | `mcp_unityMCP_manage_gameobject` |
| Add/Remove/Set components | `mcp_unityMCP_manage_components` |
| Bulk operations | `mcp_unityMCP_batch_execute` |
| Asset operations | `mcp_unityMCP_manage_asset` |
| Scene operations | `mcp_unityMCP_manage_scene` |
| Camera control | `mcp_unityMCP_manage_camera` |
| Play/Pause/Stop Editor | `mcp_unityMCP_manage_editor` |
| Execute ad-hoc C# | `mcp_unityMCP_execute_code` |
| Create animations | `mcp_unityMCP_manage_animation` |
| Check errors | `GetDiagnostics` |

## Common Task Patterns

### Adding a new game mechanic
1. Read the relevant system breakdown markdown in `I-IP markdown/`
2. Identify which existing script to extend (or if a new one is truly needed)
3. Implement using the event bus pattern — use `GlobalEventManager` for cross-system signals
4. Add `[Header]`/`[Tooltip]` attributes for Inspector visibility
5. Test via `GetDiagnostics` for compilation, then suggest Play Mode testing

### Fixing a bug
1. Read console output via `GetDiagnostics`
2. Read the relevant script files
3. Locate the root cause (null reference, logic error, missing reference)
4. Fix minimally — don't refactor unrelated code
5. Verify with `GetDiagnostics`

### Adding Inspector-configurable fields
- Field name: English (camelCase or PascalCase)
- `[Header("中文分类名")]` for grouping
- `[Tooltip("鼠标悬停时的中文说明")]` for hover tooltip
- Enum values must be in English (not Chinese)

## Music & Map Integration

When working on map/music features:
- Map types are defined in `IntegratedMapSystem.MapType` enum
- Music types are defined in `GameSystems.MapMusicType` enum (in `GameplayAudioManager.cs`)
- `IntegratedMapSystem` broadcasts `OnMapTypeChanged` when a new map generates
- `GameplayAudioManager` listens and switches music accordingly
- Music state priority: SafeZone > Boss > Battle > Exploration
- Music files organized in `Assets/Music/MapMusic/{MapType}/`