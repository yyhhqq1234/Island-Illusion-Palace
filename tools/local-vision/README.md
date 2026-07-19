# Local Vision MCP Server

让不支持多模态的主 Agent 通过工具调用获得"看图"能力。
后端是本地 LM Studio 部署的多模态模型（默认 `qwen/qwen3-vl-4b`）。

## 架构

```
主 Agent  ──stdio──>  vision_server.py (FastMCP)  ──HTTP──>  LM Studio :1234
                         工具: analyze_image                    qwen3-vl-4b
```

主 Agent 调用 `analyze_image(image, question)`，传入图片路径/URL 和问题，
脚本读取图片转 base64 发给 LM Studio 的 OpenAI 兼容 `/v1/chat/completions`，
把模型回答作为工具结果返回。

## 安装

```bash
# 推荐用 uv（项目里已有）
uv tool install "mcp[cli]"
# 或直接装到当前 python
pip install "mcp>=1.2"
```

## 配置

环境变量（可在 `.mcp.json` 的 `env` 里覆盖）：

| 变量 | 默认值 | 说明 |
|------|--------|------|
| `LM_STUDIO_URL` | `http://192.168.100.1:1234` | LM Studio 服务地址 |
| `LM_STUDIO_MODEL` | `qwen/qwen3-vl-4b` | 模型名 |
| `LM_STUDIO_TIMEOUT` | `120` | 单次请求超时秒数 |

## 注册到 Claude Code

已写入项目根 `.mcp.json`。重启 Claude Code 后，工具名为 `mcp__local-vision__analyze_image`。

## 使用示例

主 Agent 在需要看图时调用：

```
mcp__local-vision__analyze_image(
  image="D:/screenshots/frame.png",
  question="这是 Unity Game View 截图，玩家在画面什么位置？周围有敌人吗？"
)
```

配合 Unity MCP 的 `screenshot-game-view` / `screenshot-scene-view` 工具：
1. 先用 `screenshot-game-view` 截图到磁盘
2. 再用 `analyze_image` 看图（截图工具返回的图给主 Agent 看不到，但路径可传给本工具）
3. 根据文字描述决策

> 注意：Unity MCP 的 screenshot-* 工具直接返回图片二进制给主 Agent，
> 但主 Agent 看不懂。需要先把图存到磁盘再传路径给 `analyze_image`。
> 若 screenshot 返回的是 base64，可包装成 `data:image/png;base64,...` 直接传。
