# -*- coding: utf-8 -*-
"""生成 csc 响应文件(供 tools/compile-check.sh 使用)。"""
import os, glob

UNITY = r"D:/Program Files/Unity/Hub/Editor/2022.3.62f3c1/Editor/Data"
PROJ = os.getcwd()

runtime_srcs, editor_srcs = [], []
for root, dirs, files in os.walk(os.path.join(PROJ, "Assets", "Scripts")):
    for f in files:
        if f.endswith(".cs"):
            p = os.path.join(root, f)
            (editor_srcs if os.sep + "Editor" in p else runtime_srcs).append(p)

refs = []
for d in (UNITY + "/Managed/UnityEngine", UNITY + "/Managed/UnityEditor"):
    refs += glob.glob(d + "/*.dll")
refs.append(UNITY + "/NetStandard/ref/2.1.0/netstandard.dll")
for dll in glob.glob(PROJ + "/Library/ScriptAssemblies/*.dll"):
    if not os.path.basename(dll).startswith("Assembly-CSharp"):
        refs.append(dll)

defines = ("UNITY_EDITOR;UNITY_2022_3;UNITY_2022_3_62;UNITY_2022_3_OR_NEWER;"
           "UNITY_2022_OR_NEWER;UNITY_STANDALONE_WIN;UNITY_STANDALONE;DEBUG;TRACE;UNITY_ASSERTIONS")

with open("tools/runtime.rsp", "w", encoding="utf-8") as f:
    f.write("-target:library\n-nologo\n-nowarn:1701,1702\n-nostdlib\n")
    f.write("-define:" + defines + "\n-out:tools/_check_runtime.dll\n")
    for r in refs:
        f.write('-r:"' + r + '"\n')
    for s in runtime_srcs:
        f.write('"' + s + '"\n')

with open("tools/editor.rsp", "w", encoding="utf-8") as f:
    f.write("-target:library\n-nologo\n-nowarn:1701,1702\n-nostdlib\n")
    f.write("-define:" + defines + "\n-out:tools/_check_editor.dll\n")
    for r in refs:
        f.write('-r:"' + r + '"\n')
    f.write('-r:"' + PROJ + '/tools/_check_runtime.dll"\n')
    for s in editor_srcs:
        f.write('"' + s + '"\n')
print(f"rsp: runtime={len(runtime_srcs)} editor={len(editor_srcs)} refs={len(refs)}")
