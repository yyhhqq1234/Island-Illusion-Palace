#if UNITY_EDITOR
using System.Collections.Generic;

namespace ComfyUI
{
    /// <summary>
    /// 资产分类枚举
    /// </summary>
    public enum AssetCategory
    {
        Player,
        Enemy,
        Boss,
        MapTile,
        Prop,
        Weapon,
        UI,
        Effect
    }

    /// <summary>
    /// 提示词模板数据类
    /// </summary>
    [System.Serializable]
    public class PromptTemplate
    {
        public string assetType;        // 资产类型名称
        public string positivePrompt;   // 正向提示词（不含基础风格词）
        public string negativePrompt;   // 反向提示词（null 表示使用默认）
        public int width;               // 生成宽度
        public int height;              // 生成高度
        public int targetWidth;         // 目标精灵宽度（降采样后）
        public int targetHeight;        // 目标精灵高度（降采样后）
        public AssetCategory category;  // 资产分类
    }

    /// <summary>
    /// ComfyUI 提示词模板库
    /// 基于《幻宫_视觉风格指导手册》构建
    /// </summary>
    public static class ComfyUIPromptTemplates
    {
        // ==========================================
        // 概念艺术基础风格词（数字绘画风格）
        // ==========================================
        public const string ConceptBaseStylePrompt =
            "(dark fantasy concept art:1.3), (digital painting:1.2), dramatic lighting, " +
            "(soul glow: blue-purple luminescence #4A3A8C), (crystal erosion: geometric cyan crystals #00D4FF), " +
            "rim light, high contrast, detailed texture, atmospheric perspective";

        // ==========================================
        // 像素精灵基础风格词（像素艺术风格）
        // ==========================================
        public const string SpriteBaseStylePrompt =
            "(pixel art:1.4), (2D top-down game sprite:1.3), dark fantasy, " +
            "(soul glow: blue-purple luminescence #4A3A8C), (crystal erosion: geometric cyan crystals #00D4FF), " +
            "cel-shading, 1px outline, transparent background, " +
            "dark atmospheric, high contrast, limited color palette";

        // ==========================================
        // 概念艺术反向提示词
        // ==========================================
        public const string ConceptNegativePrompt =
            "photorealistic, 3D render, CGI, smooth vector art, flat design, " +
            "bright sunny, cartoon, anime, chibi, pixel art, low resolution, " +
            "blurry, watermark, text, signature, ugly, distorted, bad anatomy, " +
            "extra limbs, fused fingers, low quality, jpeg artifacts";

        // ==========================================
        // 像素精灵反向提示词
        // ==========================================
        public const string SpriteNegativePrompt =
            "photorealistic, 3D render, CGI, smooth vector art, flat design, " +
            "bright sunny, cartoon, anime, chibi, painterly texture, visible brushstrokes, " +
            "blurry, watermark, text, signature, ugly, distorted, bad anatomy, " +
            "extra limbs, fused fingers, low quality, jpeg artifacts, anti-aliasing";

        // ==========================================
        // 旧版基础风格词（保留以兼容旧代码，实际值与 SpriteBaseStylePrompt 一致）
        // ==========================================
        public const string BaseStylePrompt = SpriteBaseStylePrompt;

        // ==========================================
        // 旧版基础反向提示词（保留以兼容旧代码，实际值与 SpriteNegativePrompt 一致）
        // ==========================================
        public const string BaseNegativePrompt = SpriteNegativePrompt;

        // ==========================================
        // 模板字典
        // ==========================================
        public static readonly Dictionary<string, PromptTemplate> Templates;

        // ==========================================
        // 静态构造函数
        // ==========================================
        static ComfyUIPromptTemplates()
        {
            Templates = new Dictionary<string, PromptTemplate>
            {
                // ========== 玩家 (48x48 target, 768x768 gen) ==========
                {
                    "玩家/墨语", new PromptTemplate
                    {
                        assetType = "玩家/墨语",
                        positivePrompt = "(young male warrior:1.3), slender build, dark cloak, short sword, (left hand ring with dark gold glow:1.2), blue-purple soul energy particles around him, (dark base outfit with crimson red #C41E3A accents:1.1), determined expression, 3-heads-tall proportion",
                        negativePrompt = null,
                        width = 768,
                        height = 768,
                        targetWidth = 48,
                        targetHeight = 48,
                        category = AssetCategory.Player
                    }
                },
                // ========== 敌人 (32x32 target, 512x512 gen) ==========
                {
                    "腐化村民", new PromptTemplate
                    {
                        assetType = "腐化村民",
                        positivePrompt = "(corrupted villager:1.3), hunched posture, ragged clothes, (crystal growth on skin:1.2), twisted form, zombie-like, dark earthy colors, semi-transparent blue-purple soul glow, horror fantasy, 2.5-heads-tall proportion",
                        negativePrompt = null,
                        width = 512,
                        height = 512,
                        targetWidth = 32,
                        targetHeight = 32,
                        category = AssetCategory.Enemy
                    }
                },
                {
                    "结晶蜥蜴", new PromptTemplate
                    {
                        assetType = "结晶蜥蜴",
                        positivePrompt = "(crystal lizard monster:1.3), quadruped reptile, (geometric cyan crystal clusters growing from back:1.4), faceted crystal texture, sharp crystal spikes, agile stance, 2-heads-tall proportion, glowing cyan eyes",
                        negativePrompt = null,
                        width = 512,
                        height = 512,
                        targetWidth = 32,
                        targetHeight = 32,
                        category = AssetCategory.Enemy
                    }
                },
                {
                    "冰原狼", new PromptTemplate
                    {
                        assetType = "冰原狼",
                        positivePrompt = "(ice wolf monster:1.3), quadruped beast, frost-covered fur, (ice crystal formations on mane and tail:1.2), cold blue-white fur, glowing blue eyes, snarling expression, 2-heads-tall proportion",
                        negativePrompt = null,
                        width = 512,
                        height = 512,
                        targetWidth = 32,
                        targetHeight = 32,
                        category = AssetCategory.Enemy
                    }
                },
                {
                    "机械残骸", new PromptTemplate
                    {
                        assetType = "机械残骸",
                        positivePrompt = "(broken machine monster:1.3), rusty mechanical construct, exposed gears and wires, (cyan energy tubes:1.2), scrap metal body, orange rust spots, broken joints, sparking, asymmetric design, 2.5-heads-tall",
                        negativePrompt = null,
                        width = 512,
                        height = 512,
                        targetWidth = 32,
                        targetHeight = 32,
                        category = AssetCategory.Enemy
                    }
                },
                {
                    "骷髅战士", new PromptTemplate
                    {
                        assetType = "骷髅战士",
                        positivePrompt = "(skeleton warrior:1.3), animated bones, ancient armor scraps, (blue-purple soul fire in eye sockets:1.2), cracked bone texture, tattered cloth remnants, dark fantasy undead, 3-heads-tall",
                        negativePrompt = null,
                        width = 512,
                        height = 512,
                        targetWidth = 32,
                        targetHeight = 32,
                        category = AssetCategory.Enemy
                    }
                },
                {
                    "怨魂", new PromptTemplate
                    {
                        assetType = "怨魂",
                        positivePrompt = "(floating wraith:1.3), ethereal ghost, transparent body, (blue-purple soul glow:1.4), no legs, tattered robes floating, translucent, wispy edges, haunting presence, 2.5-heads-tall",
                        negativePrompt = null,
                        width = 512,
                        height = 512,
                        targetWidth = 32,
                        targetHeight = 32,
                        category = AssetCategory.Enemy
                    }
                },
                {
                    "石像鬼", new PromptTemplate
                    {
                        assetType = "石像鬼",
                        positivePrompt = "(stone gargoyle:1.3), winged demon statue, cracked stone texture, (cyan crystal veins on stone surface:1.2), bat-like wings, horned head, crouched guardian pose, gothic architecture, 3-heads-tall",
                        negativePrompt = null,
                        width = 512,
                        height = 512,
                        targetWidth = 32,
                        targetHeight = 32,
                        category = AssetCategory.Enemy
                    }
                },
                {
                    "沼泽潜伏者", new PromptTemplate
                    {
                        assetType = "沼泽潜伏者",
                        positivePrompt = "(swamp lurker monster:1.3), amphibious creature, half-submerged in mud, slimy skin, (yellow-green toxic mist:1.1), webbed hands, bulging eyes, camouflage texture, lurking posture, 2-heads-tall",
                        negativePrompt = null,
                        width = 512,
                        height = 512,
                        targetWidth = 32,
                        targetHeight = 32,
                        category = AssetCategory.Enemy
                    }
                },
                // ========== 精英 (48x48 target, 768x768 gen) ==========
                {
                    "灵魂吞噬者", new PromptTemplate
                    {
                        assetType = "灵魂吞噬者",
                        positivePrompt = "(soul devourer monster:1.3), large floating entity, (swirling souls around it:1.2), dark void body, multiple eyes, tentacle-like appendages, blue-purple consumed souls visible inside, ethereal horror, 3-heads-tall",
                        negativePrompt = null,
                        width = 768,
                        height = 768,
                        targetWidth = 48,
                        targetHeight = 48,
                        category = AssetCategory.Enemy
                    }
                },
                {
                    "熔岩元素", new PromptTemplate
                    {
                        assetType = "熔岩元素",
                        positivePrompt = "(lava elemental:1.3), humanoid magma creature, molten rock body, (orange-yellow lava veins:1.2), heat distortion, volcanic rock texture, fire particles, glowing core, 3.5-heads-tall",
                        negativePrompt = null,
                        width = 768,
                        height = 768,
                        targetWidth = 48,
                        targetHeight = 48,
                        category = AssetCategory.Enemy
                    }
                },
                {
                    "机械构造体", new PromptTemplate
                    {
                        assetType = "机械构造体",
                        positivePrompt = "(mechanical construct:1.3), large humanoid robot, brass and steel body, (cyan energy core in chest:1.2), steam pipes, gear mechanisms visible, industrial design, heavy armor plates, 3.5-heads-tall",
                        negativePrompt = null,
                        width = 768,
                        height = 768,
                        targetWidth = 48,
                        targetHeight = 48,
                        category = AssetCategory.Enemy
                    }
                },
                // ========== BOSS ==========
                {
                    "时空守护者", new PromptTemplate
                    {
                        assetType = "时空守护者",
                        positivePrompt = "(time guardian boss:1.4), massive armored entity, clockwork mechanisms embedded in armor, (golden #C8A848 sacred markings:1.2), floating time gears, hourglass core, imposing stance, ancient guardian, 4-heads-tall",
                        negativePrompt = null,
                        width = 512,
                        height = 512,
                        targetWidth = 64,
                        targetHeight = 64,
                        category = AssetCategory.Boss
                    }
                },
                {
                    "记忆守护者", new PromptTemplate
                    {
                        assetType = "记忆守护者",
                        positivePrompt = "(memory guardian boss:1.4), ethereal humanoid figure, (fragmented body made of memory shards:1.3), blue-purple soul glow, translucent, floating, surrounded by floating memory fragments, melancholic presence, 4-heads-tall",
                        negativePrompt = null,
                        width = 640,
                        height = 640,
                        targetWidth = 80,
                        targetHeight = 80,
                        category = AssetCategory.Boss
                    }
                },
                {
                    "S-SN", new PromptTemplate
                    {
                        assetType = "S-SN",
                        positivePrompt = "(Scarlet Soul Shana:1.4), divine female figure, (crimson red #C41E3A and blue-purple #4A3A8C dual energy:1.3), floating, crystalline wings, elegant battle dress, crown of light, god-like presence, surrounded by crystal fragments, 4-heads-tall",
                        negativePrompt = null,
                        width = 768,
                        height = 768,
                        targetWidth = 96,
                        targetHeight = 96,
                        category = AssetCategory.Boss
                    }
                },
                // ========== 地图 Tile (64x64 target, 512x512 gen) ==========
                {
                    "森林地图Tile", new PromptTemplate
                    {
                        assetType = "森林地图Tile",
                        positivePrompt = "(forest terrain tileset:1.3), (dark green ground:1.2), ancient moss-covered trees, blue-purple soul light spots on ground, (cyan crystal veins on tree trunks:1.1), mysterious atmosphere, top-down orthographic view, seamless tileable",
                        negativePrompt = null,
                        width = 512,
                        height = 512,
                        targetWidth = 64,
                        targetHeight = 64,
                        category = AssetCategory.MapTile
                    }
                },
                {
                    "荒原地图Tile", new PromptTemplate
                    {
                        assetType = "荒原地图Tile",
                        positivePrompt = "(wasteland terrain tileset:1.3), barren cracked earth, gray-brown landscape, (dark purple sky reflection:1.1), scattered ruins, dead vegetation, top-down orthographic view, seamless tileable, desolate atmosphere",
                        negativePrompt = null,
                        width = 512,
                        height = 512,
                        targetWidth = 64,
                        targetHeight = 64,
                        category = AssetCategory.MapTile
                    }
                },
                {
                    "沙漠地图Tile", new PromptTemplate
                    {
                        assetType = "沙漠地图Tile",
                        positivePrompt = "(desert terrain tileset:1.3), golden sand dunes, (cyan crystal formations on sand:1.2), heat haze, ancient ruins half-buried, top-down orthographic view, seamless tileable",
                        negativePrompt = null,
                        width = 512,
                        height = 512,
                        targetWidth = 64,
                        targetHeight = 64,
                        category = AssetCategory.MapTile
                    }
                },
                {
                    "岩地地图Tile", new PromptTemplate
                    {
                        assetType = "岩地地图Tile",
                        positivePrompt = "(rocky terrain tileset:1.3), gray stone ground, (blue-purple glowing crystal ore veins:1.3), jagged rock formations, mining tracks, top-down orthographic view, seamless tileable",
                        negativePrompt = null,
                        width = 512,
                        height = 512,
                        targetWidth = 64,
                        targetHeight = 64,
                        category = AssetCategory.MapTile
                    }
                },
                {
                    "湿地地图Tile", new PromptTemplate
                    {
                        assetType = "湿地地图Tile",
                        positivePrompt = "(wetland terrain tileset:1.3), dark swampy ground, murky water patches, (yellow-green toxic fog:1.1), dead trees, moss-covered stones, top-down orthographic view, seamless tileable",
                        negativePrompt = null,
                        width = 512,
                        height = 512,
                        targetWidth = 64,
                        targetHeight = 64,
                        category = AssetCategory.MapTile
                    }
                },
                {
                    "冰原地图Tile", new PromptTemplate
                    {
                        assetType = "冰原地图Tile",
                        positivePrompt = "(ice terrain tileset:1.3), snow-covered frozen ground, (blue-purple glow under ice:1.2), ice crystal formations, ancient frozen machinery visible under ice, top-down orthographic view, seamless tileable",
                        negativePrompt = null,
                        width = 512,
                        height = 512,
                        targetWidth = 64,
                        targetHeight = 64,
                        category = AssetCategory.MapTile
                    }
                },
                {
                    "火山地图Tile", new PromptTemplate
                    {
                        assetType = "火山地图Tile",
                        positivePrompt = "(volcanic terrain tileset:1.3), dark volcanic rock ground, (orange lava rivers:1.2), (cyan crystal veins in lava:1.1), ash particles, obsidian formations, top-down orthographic view, seamless tileable",
                        negativePrompt = null,
                        width = 512,
                        height = 512,
                        targetWidth = 64,
                        targetHeight = 64,
                        category = AssetCategory.MapTile
                    }
                },
                {
                    "废墟都市Tile", new PromptTemplate
                    {
                        assetType = "废墟都市Tile",
                        positivePrompt = "(ruined city tileset:1.3), collapsed buildings, broken streets, (cyan neon signs still glowing:1.2), rusted metal, rubble, post-apocalyptic urban, top-down orthographic view, seamless tileable",
                        negativePrompt = null,
                        width = 512,
                        height = 512,
                        targetWidth = 64,
                        targetHeight = 64,
                        category = AssetCategory.MapTile
                    }
                },
                {
                    "遗忘庄园Tile", new PromptTemplate
                    {
                        assetType = "遗忘庄园Tile",
                        positivePrompt = "(forgotten manor tileset:1.3), decaying noble mansion interior, (faded gold #C8A848 decorations:1.1), cracked marble floors, dusty chandeliers, (blue-purple soul echoes:1.2), Victorian gothic, top-down orthographic view, seamless tileable",
                        negativePrompt = null,
                        width = 512,
                        height = 512,
                        targetWidth = 64,
                        targetHeight = 64,
                        category = AssetCategory.MapTile
                    }
                },
                {
                    "古代神殿Tile", new PromptTemplate
                    {
                        assetType = "古代神殿Tile",
                        positivePrompt = "(ancient temple tileset:1.3), white stone temple, (golden sacred engravings:1.2), cracked stone pillars, (blue-purple soul light from wall cracks:1.1), divine rays from ceiling, top-down orthographic view, seamless tileable",
                        negativePrompt = null,
                        width = 512,
                        height = 512,
                        targetWidth = 64,
                        targetHeight = 64,
                        category = AssetCategory.MapTile
                    }
                },
                {
                    "实验室碎片Tile", new PromptTemplate
                    {
                        assetType = "实验室碎片Tile",
                        positivePrompt = "(laboratory tileset:1.3), sterile white sci-fi lab, (cyan glowing specimen tanks:1.2), metal floors, holographic displays, (crimson red alarm lights:1.1), abandoned research facility, top-down orthographic view, seamless tileable",
                        negativePrompt = null,
                        width = 512,
                        height = 512,
                        targetWidth = 64,
                        targetHeight = 64,
                        category = AssetCategory.MapTile
                    }
                },
                // ========== 道具 (32x32 target, 512x512 gen) ==========
                {
                    "营火", new PromptTemplate
                    {
                        assetType = "营火",
                        positivePrompt = "(campfire:1.3), (blue-purple soul flame:1.4), stone circle, glowing embers, warm light radius, dark surroundings, safe haven, top-down view, cozy atmosphere in dark fantasy",
                        negativePrompt = null,
                        width = 512,
                        height = 512,
                        targetWidth = 32,
                        targetHeight = 32,
                        category = AssetCategory.Prop
                    }
                },
                {
                    "炼药锅", new PromptTemplate
                    {
                        assetType = "炼药锅",
                        positivePrompt = "(alchemy cauldron:1.3), large iron cauldron, (blue-purple bubbling liquid:1.2), crystal decorations on rim, wooden stand, alchemy symbols, glowing runes, top-down view, dark fantasy",
                        negativePrompt = null,
                        width = 512,
                        height = 512,
                        targetWidth = 32,
                        targetHeight = 32,
                        category = AssetCategory.Prop
                    }
                },
                {
                    "时空门", new PromptTemplate
                    {
                        assetType = "时空门",
                        positivePrompt = "(time portal:1.3), (cyan and blue-purple swirling vortex:1.4), floating crystal fragments around portal, energy rings, dimensional gateway, top-down view, mysterious",
                        negativePrompt = null,
                        width = 512,
                        height = 512,
                        targetWidth = 32,
                        targetHeight = 32,
                        category = AssetCategory.Prop
                    }
                },
                {
                    "时空裂隙", new PromptTemplate
                    {
                        assetType = "时空裂隙",
                        positivePrompt = "(time rift:1.3), (unstable dimensional tear:1.3), jagged edges, (cyan and blue-purple energy leaking:1.2), reality distortion, random sparks, top-down view, dangerous",
                        negativePrompt = null,
                        width = 512,
                        height = 512,
                        targetWidth = 32,
                        targetHeight = 32,
                        category = AssetCategory.Prop
                    }
                },
                // ========== 武器 (32x32 target, 512x512 gen) ==========
                {
                    "武器/剑", new PromptTemplate
                    {
                        assetType = "武器/剑",
                        positivePrompt = "(fantasy sword:1.3), (blue-purple soul glow on blade:1.2), elegant design, dark metal hilt, crystal pommel, top-down view, dark fantasy weapon",
                        negativePrompt = null,
                        width = 512,
                        height = 512,
                        targetWidth = 32,
                        targetHeight = 32,
                        category = AssetCategory.Weapon
                    }
                },
                {
                    "武器/法杖", new PromptTemplate
                    {
                        assetType = "武器/法杖",
                        positivePrompt = "(magic staff:1.3), wooden staff, (cyan crystal orb at top:1.4), rune carvings, soul energy swirling around crystal, top-down view, dark fantasy weapon",
                        negativePrompt = null,
                        width = 512,
                        height = 512,
                        targetWidth = 32,
                        targetHeight = 32,
                        category = AssetCategory.Weapon
                    }
                },
                {
                    "武器/镰刀", new PromptTemplate
                    {
                        assetType = "武器/镰刀",
                        positivePrompt = "(fantasy scythe:1.3), large curved blade, (blue-purple soul energy on edge:1.2), dark metal handle, death motif, top-down view, dark fantasy weapon",
                        negativePrompt = null,
                        width = 512,
                        height = 512,
                        targetWidth = 32,
                        targetHeight = 32,
                        category = AssetCategory.Weapon
                    }
                },
                {
                    "武器/晶能武装", new PromptTemplate
                    {
                        assetType = "武器/晶能武装",
                        positivePrompt = "(crystal armament:1.3), (geometric crystal weapon:1.4), cyan and blue-purple crystal blade, faceted design, floating crystal shards around it, top-down view, dark fantasy",
                        negativePrompt = null,
                        width = 512,
                        height = 512,
                        targetWidth = 32,
                        targetHeight = 32,
                        category = AssetCategory.Weapon
                    }
                }
            };
        }

        // ==========================================
        // 概念艺术专用方法
        // ==========================================

        /// <summary>
        /// 获取概念艺术正向提示词
        /// </summary>
        public static string GetConceptPositivePrompt(string assetType)
        {
            if (Templates.TryGetValue(assetType, out var template))
            {
                return $"{ConceptBaseStylePrompt}, {template.positivePrompt}";
            }
            return ConceptBaseStylePrompt;
        }

        /// <summary>
        /// 获取概念艺术反向提示词
        /// </summary>
        public static string GetConceptNegativePrompt(string assetType)
        {
            if (Templates.TryGetValue(assetType, out var template) && !string.IsNullOrEmpty(template.negativePrompt))
            {
                return template.negativePrompt;
            }
            return ConceptNegativePrompt;
        }

        // ==========================================
        // 像素精灵专用方法
        // ==========================================

        /// <summary>
        /// 获取像素精灵正向提示词
        /// </summary>
        public static string GetSpritePositivePrompt(string assetType)
        {
            if (Templates.TryGetValue(assetType, out var template))
            {
                return $"{SpriteBaseStylePrompt}, {template.positivePrompt}";
            }
            return SpriteBaseStylePrompt;
        }

        /// <summary>
        /// 获取像素精灵反向提示词
        /// </summary>
        public static string GetSpriteNegativePrompt(string assetType)
        {
            if (Templates.TryGetValue(assetType, out var template) && !string.IsNullOrEmpty(template.negativePrompt))
            {
                return template.negativePrompt;
            }
            return SpriteNegativePrompt;
        }

        // ==========================================
        // 旧版兼容方法（已过时）
        // ==========================================

        /// <summary>
        /// 获取完整的正向提示词（基础风格词 + 资产类型词）
        /// </summary>
        [System.Obsolete("请使用 GetConceptPositivePrompt 或 GetSpritePositivePrompt")]
        public static string GetFullPositivePrompt(string assetType)
        {
            return GetSpritePositivePrompt(assetType);
        }

        /// <summary>
        /// 获取反向提示词（优先使用资产专属，否则使用默认）
        /// </summary>
        [System.Obsolete("请使用 GetConceptNegativePrompt 或 GetSpriteNegativePrompt")]
        public static string GetNegativePrompt(string assetType)
        {
            return GetSpriteNegativePrompt(assetType);
        }

        // ==========================================
        // 通用工具方法
        // ==========================================

        /// <summary>
        /// 获取所有资产类型名称
        /// </summary>
        public static string[] GetAllAssetTypes()
        {
            var keys = new string[Templates.Count];
            Templates.Keys.CopyTo(keys, 0);
            return keys;
        }

        /// <summary>
        /// 根据资产类型获取模板
        /// </summary>
        public static PromptTemplate GetTemplate(string assetType)
        {
            Templates.TryGetValue(assetType, out var template);
            return template;
        }

        /// <summary>
        /// 检查资产类型是否存在
        /// </summary>
        public static bool HasTemplate(string assetType)
        {
            return Templates.ContainsKey(assetType);
        }
    }
}
#endif
