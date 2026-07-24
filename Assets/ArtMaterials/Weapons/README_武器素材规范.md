# 武器美术素材规范

本目录存放所有武器的手持精灵（handheld sprite）与库存图标（icon）。所有美术素材通过 `Assets/DataAssets/Weapons/Weapon_<英文名>.asset` 接入游戏。

## 目录结构

```
Assets/ArtMaterials/Weapons/
├── Sword/                # 剑类（Sword）
│   ├── Dawnblade.png              # 破晓之刃 手持
│   ├── Dawnblade_Icon.png         # 破晓之刃 图标
│   ├── MoltenGreatsword.png
│   ├── MoltenGreatsword_Icon.png
│   ├── CrimsonMemoryBlade.png
│   └── CrimsonMemoryBlade_Icon.png
├── Staff/                # 法杖类（Staff）
├── Scythe/               # 镰刀类（Scythe）
├── CrystalArmament/      # 晶能武装类（CrystalArm）
├── README_武器素材规范.md
└── Sword/ 既有占位：Writer.png, Singer.png（仅占位，正式武器请用下方英文名命名）
```

> CrystalArm 类型对应目录名为 `CrystalArmament`（与策划美术目录命名一致），与代码枚举 `WeaponType.CrystalArm` 对应。

## 命名规范

- 手持精灵：`{EnglishPascalCase}.png`
- 库存图标：`{EnglishPascalCase}_Icon.png`
- 文件名必须与 `WeaponData.asset` 的英文 PascalCase 名完全一致（区分大小写）
- 不要使用中文文件名、空格、全角字符

## 尺寸标准（来自策划案04）

| 用途 | 像素尺寸 | 说明 |
|------|----------|------|
| 手持精灵 | 64-90 × 128-160 | 玩家手持显示，长条形 |
| 库存图标 | 56 × 56 | 背包格子内显示，方形 |

## 接入步骤

1. 将 png 文件拖入对应武器类型子目录（如剑类拖到 `Sword/`）
2. 在 Unity Project 窗口选中该 png，Inspector 中确认 Texture Type = Sprite (2D and UI)
3. 打开 `Assets/DataAssets/Weapons/Weapon_<英文名>.asset`
4. 将 Sprite 拖入对应字段：
   - `weaponSprite`：手持精灵
   - `icon`：库存图标
5. 保存（Ctrl+S）

## 15把武器中英文名对照表

| 中文名 | 英文名(PascalCase) | weaponType | 目录 |
|--------|-------------------|-----------|------|
| 破晓之刃 | Dawnblade | Sword | Sword/ |
| 结晶法杖 | CrystalStaff | Staff | Staff/ |
| 沼泽镰刀 | SwampScythe | Scythe | Scythe/ |
| 晶能短刃 | CrystalShard | CrystalArm | CrystalArmament/ |
| 灵魂引导者 | SoulGuide | Staff | Staff/ |
| 熔火重剑 | MoltenGreatsword | Sword | Sword/ |
| 冰霜法杖 | FrostStaff | Staff | Staff/ |
| 雷霆镰刃 | ThunderScythe | Scythe | Scythe/ |
| 圣光裁决 | HolyJudgment | CrystalArm | CrystalArmament/ |
| 腐化吞噬者 | CorruptionDevourer | Scythe | Scythe/ |
| 时空裂隙匕首 | TemporalRiftDagger | CrystalArm | CrystalArmament/ |
| 绯红记忆之刃 | CrimsonMemoryBlade | Sword | Sword/ |
| 岩地重锤 | StoneHammer | CrystalArm | CrystalArmament/ |
| 灵魂共鸣法杖 | SoulResonanceStaff | Staff | Staff/ |
| 绯红契约 | CrimsonPact | Scythe | Scythe/ |

> 英文名必须与 `WeaponDataGenerator` 中 `Specs[].englishName` 完全一致，否则生成器产出的 `.asset` 文件名与美术素材无法对应。

## 资产生成

运行菜单项 `IIP/生成武器/15把武器Data资产`，将在 `Assets/DataAssets/Weapons/` 下幂等生成 15 个 `Weapon_<英文名>.asset`。已存在且非空的 asset 会被跳过，避免覆盖美术已拖入的 Sprite。

## 注意事项

- 武器类型枚举：`Sword, Staff, Scythe, CrystalArm`（注意 CrystalArm 不是 CrystalArmament）
- 元素类型枚举：`None, Frost, Water, Fire, Lightning, Soul, Holy`
- 稀有度枚举：`Common, Uncommon, Rare, Epic, Legendary`（策划案中 Rare 对应史诗，无 Epic 武器）
