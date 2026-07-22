# 子 Agent 视觉能力共享规范

所有 `.claude/agents/` 下的子 Agent 都具备「看图」能力，通过本地 VL 小模型实现，**图片字节永不进入任何 Agent 上下文**。

## 链路

```
子 Agent  ──> analyze_image(image=<落盘路径>, question=<具体问题>)  ──>  LM Studio qwen3-vl-4b  ──>  文字描述返回
                 ↑
                 路径来源：
                   ① 已有截图（tools/local-vision/shots/ 下）
                   ② 主 Agent / 其他 agent 传来的路径
                   ③ 自己用 script-execute 跑反射 C# 截 GameView（仅视觉角色有此权限）
```

## 工具

- `mcp__local-vision__analyze_image(image, question)` — **所有子 Agent 都有**
  - `image`：本地绝对路径（推荐）/ http(s) URL / `data:image/...;base64,...`
  - `question`：越具体越好。4B 小模型对模糊问题泛泛而谈
  - 返回：模型的中文文字描述。失败返回 `[vision-error]...`

## 截图能力（仅视觉角色：developer / debugger / uiux-designer / art-style / level-designer）

用 `mcp__ai-game-developer__script-execute` 跑下面这段 C#（`isMethodBody=false`，类名 `CaptureFromRT`，方法 `Run`），反射读 GameView 的 `m_RenderTexture` 截图落盘，返回路径字符串。

> **禁止**用 `screenshot-game-view` / `screenshot-camera` / `screenshot-scene-view`——它们把图片 base64 直接返回上下文，4B 主模型不支持多模态且一张 1080p ≈ 479k token，会触发 400 context length 报错。**唯一可靠方法**是反射读 `m_RenderTexture`（`Screen.ReadPixels` 在 PlayMode 下读到的全是空灰色）。

```csharp
using UnityEngine; using UnityEditor; using System; using System.IO; using System.Reflection;

public static class CaptureFromRT {
    public static string Run() {
        var asm = typeof(EditorWindow).Assembly;
        var gvType = asm.GetType("UnityEditor.GameView");
        var gv = EditorWindow.GetWindow(gvType);
        var rt = gvType.GetField("m_RenderTexture",
            BindingFlags.NonPublic | BindingFlags.Instance).GetValue(gv) as RenderTexture;
        int w = rt.width, h = rt.height;
        var prev = RenderTexture.active; RenderTexture.active = rt;
        var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0, false); tex.Apply();
        RenderTexture.active = prev;
        int maxSide = 1024; float scale = Mathf.Min(1f, (float)maxSide / Mathf.Max(w, h));
        int tw = Mathf.RoundToInt(w * scale), th = Mathf.RoundToInt(h * scale);
        var scaled = new Texture2D(tw, th, TextureFormat.RGB24, false);
        var src = tex.GetPixels(); var dst = new Color[tw * th];
        for (int y = 0; y < th; y++) { int sy = Mathf.Clamp(Mathf.RoundToInt(y/scale),0,h-1);
            for (int x = 0; x < tw; x++) { int sx = Mathf.Clamp(Mathf.RoundToInt(x/scale),0,w-1);
                dst[y*tw+x] = src[sy*w+sx]; } }
        scaled.SetPixels(dst); scaled.Apply();
        string path = @"d:\Program Files\Unity\U3Dproject\Island-Illusion-Palace\tools\local-vision\shots\gameview.jpg";
        File.WriteAllBytes(path, scaled.EncodeToJPG(82));
        UnityEngine.Object.DestroyImmediate(tex); UnityEngine.Object.DestroyImmediate(scaled);
        return $"OK {tw}x{th} -> {path}";
    }
}
```

落盘目录：`tools/local-vision/shots/`（已 `.gitignore`）。可改文件名避免覆盖（如 `hud_audit.jpg`、`boss_room.jpg`）。

## 主动调用与提示词优化原则

子 Agent 应**主动判断是否需要看画面**，而不是等主 Agent 喂图。判断后：

1. **先想清楚要问什么**——视觉小模型能力有限，问题必须具体、可验证
2. **自己写 `question`**——根据当前任务目标定制，不要用万能问句
3. **拿到描述后反思**——若描述太泛 / 漏了关键信息，**优化 question 重问**（迭代 1-2 次即可，避免无限循环）
4. **基于描述做专业判断**——用本角色的专业视角解读画面，而非照搬模型原话

### question 优化示例

| 差（模糊） | 好（具体、可验证） |
|---|---|
| "描述这张图" | "这是 GameView 截图，玩家角色在画面什么位置？左上角血条数值是多少？周围有几个敌人？" |
| "UI 好看吗" | "这是背包 UI，列出所有可见的物品图标和它们的网格位置；按钮文字是否清晰可读？有无重叠？" |
| "颜色搭配如何" | "提取画面主色调（前 3 个），判断是否符合暗色奇幻调性；有无高饱和度色块破坏氛围？" |

### 何时该看图

- **developer/debugger**：验证 UI 布局、确认战斗效果、定位视觉 Bug
- **uiux-designer**：审计现有 UI 信息层级、反馈动效、字数溢出
- **art-style**：核对生成资产风格一致性、配色合规、Mood Board 比对
- **level-designer**：检查地图动线、刷怪点分布、引导线索可见性
- **narrative-designer**：确认场景氛围与叙事调性匹配
- **其他角色（数值/运营/主策划/代码审查/安全）**：偶尔需确认实现是否符合设计预期时调用，非主要工作流

## 输出要求

调用视觉后，在交付报告里注明：
- 看了哪张图（路径）
- 问了什么（question 摘要）
- 模型回答要点
- 据此做出的专业判断 / 发现的问题

避免把模型原文长篇粘贴进报告——提炼对自己决策有用的结论。
