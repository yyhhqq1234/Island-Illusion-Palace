#!/bin/bash
# 离线编译检查:不依赖 Unity/MCP,用 Unity 自带 Roslyn 编译全部脚本验证语法/类型错误。
# 用法: bash tools/compile-check.sh   (在项目根目录)
set -e
cd "$(dirname "$0")/.."
UNITY_DATA="D:/Program Files/Unity/Hub/Editor/2022.3.62f3c1/Editor/Data"
CSC="$UNITY_DATA/DotNetSdkRoslyn/csc.dll"
python tools/gen-compile-rsp.py
echo "── 编译运行时程序集 (Assembly-CSharp) ──"
dotnet exec "$CSC" @tools/runtime.rsp 2>&1 | grep -E "error CS" || echo "✅ 运行时:0 错误"
echo "── 编译编辑器程序集 (Assembly-CSharp-Editor) ──"
dotnet exec "$CSC" @tools/editor.rsp 2>&1 | grep -E "error CS" || echo "✅ 编辑器:0 错误"
rm -f tools/_check_runtime.dll tools/_check_editor.dll
