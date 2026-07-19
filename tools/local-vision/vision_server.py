"""Local Vision MCP Server.

将本地 LM Studio 部署的多模态模型（qwen3-vl-4b）暴露为一个 MCP 工具，
让不支持多模态的主 Agent 通过工具调用获得"看图"能力。

工具：
  analyze_image(image, question?) -> 文字描述

设计要点：
- 协议：stdio（Claude Code 本地 MCP 用 stdio 最稳）
- 传输：HTTP JSON 到 LM Studio 的 OpenAI 兼容接口
- 图片输入：本地路径 / http(s) URL / data:base64 URL
  本地路径会被读取并转成 base64 data URL（LM Studio 不支持 file:// 直传）
- 超时：默认 120s，可由调用方通过 _timeout 覆盖
- 错误：失败时返回结构化错误信息而非抛异常，便于主 Agent 决策重试
"""

from __future__ import annotations

import base64
import json
import mimetypes
import os
import sys
import urllib.request
from typing import Any

from mcp.server.fastmcp import FastMCP

LM_STUDIO_URL = os.environ.get("LM_STUDIO_URL", "http://192.168.100.1:1234")
DEFAULT_MODEL = os.environ.get("LM_STUDIO_MODEL", "qwen/qwen3-vl-4b")
DEFAULT_TIMEOUT = float(os.environ.get("LM_STUDIO_TIMEOUT", "120"))

mcp = FastMCP("local-vision")


def _guess_mime(path: str) -> str:
    mime, _ = mimetypes.guess_type(path)
    # LM Studio 对 webp 兼容性一般，统一兜底 png/jpeg
    if mime and mime.startswith("image/"):
        return mime
    return "image/png"


def _load_local_file(path: str) -> str:
    """读取本地文件，返回 data:image/...;base64,... URL。"""
    if not os.path.isfile(path):
        raise FileNotFoundError(f"图片文件不存在: {path}")
    with open(path, "rb") as f:
        raw = f.read()
    # 限制单张 20MB，避免请求体过大
    if len(raw) > 20 * 1024 * 1024:
        raise ValueError(f"图片过大 ({len(raw)} bytes)，上限 20MB: {path}")
    mime = _guess_mime(path)
    return f"data:{mime};base64,{base64.b64encode(raw).decode()}"


def _resolve_image(image: str) -> str:
    """把任意 image 输入归一化为 LM Studio 可接受的 image_url.url 字符串。

    支持三种形式：
      - data:image/...;base64,xxxx  → 原样返回
      - http:// / https://           → 原样返回（LM Studio 会自行抓取）
      - 其它（视为本地路径）          → 读取并转 base64 data URL
    """
    if image.startswith("data:"):
        return image
    if image.startswith("http://") or image.startswith("https://"):
        return image
    # 本地路径：去掉 file:// 前缀
    if image.startswith("file://"):
        image = image[len("file://"):]
    return _load_local_file(image)


def _call_lm_studio(image_url: str, question: str, model: str, timeout: float) -> str:
    payload = {
        "model": model,
        "messages": [
            {
                "role": "user",
                "content": [
                    {"type": "text", "text": question},
                    {"type": "image_url", "image_url": {"url": image_url}},
                ],
            }
        ],
        "max_tokens": 1024,
        "temperature": 0.2,
    }
    req = urllib.request.Request(
        f"{LM_STUDIO_URL.rstrip('/')}/v1/chat/completions",
        data=json.dumps(payload).encode("utf-8"),
        headers={"Content-Type": "application/json"},
        method="POST",
    )
    try:
        with urllib.request.urlopen(req, timeout=timeout) as resp:
            data = json.loads(resp.read().decode("utf-8"))
        return data["choices"][0]["message"]["content"].strip()
    except urllib.error.HTTPError as e:
        body = e.read().decode("utf-8", errors="replace")[:500]
        raise RuntimeError(f"LM Studio HTTP {e.code}: {body}") from None
    except urllib.error.URLError as e:
        raise RuntimeError(f"无法连接 LM Studio ({LM_STUDIO_URL}): {e.reason}") from None


@mcp.tool()
def analyze_image(
    image: str,
    question: str = "请详细描述这张图片的内容，包括画面中的对象、布局、颜色、文字等关键信息。",
) -> str:
    """用本地多模态模型分析图片并返回文字描述。

    当主模型不支持图片解析时，用这个工具"看图"。

    Args:
        image: 图片来源，支持三种：
            1. 本地文件绝对路径，如 "D:/screenshots/frame.png"（推荐，最快最稳）
            2. http(s) URL，LM Studio 会自行下载
            3. data:image/...;base64,... 形式的内联图片
        question: 询问模型的问题。默认整体描述。
            建议具体化，例如：
            - "这是 Unity Game View 截图，描述玩家角色位置和周围敌人"
            - "这张 UI 截图里有哪些按钮，文字分别是什么"
            - "图中是否存在红色报错文字？如有请列出"

    Returns:
        模型对图片的文字回答。失败时返回以 [vision-error] 开头的错误描述。
    """
    try:
        image_url = _resolve_image(image)
    except (FileNotFoundError, ValueError) as e:
        return f"[vision-error] 图片加载失败: {e}"

    try:
        return _call_lm_studio(image_url, question, DEFAULT_MODEL, DEFAULT_TIMEOUT)
    except RuntimeError as e:
        return f"[vision-error] {e}"


if __name__ == "__main__":
    # FastMCP 默认 stdio 传输
    mcp.run()
