@echo off
chcp 65001 >nul
setlocal

REM 一键启动 ComfyUI 项目客户端
REM 位置：项目根目录

cd /d "%~dp0"

if not exist "Scripts\comfyui_client.py" (
    echo [ERROR] 未找到 Scripts\comfyui_client.py
    echo 请确认脚本位于 Island-Illusion-Palace 项目根目录。
    pause
    exit /b 1
)

echo ========================================
echo  幻宫：时空回响 - ComfyUI 客户端
echo ========================================
echo 项目目录: %cd%
echo 启动脚本: Scripts\comfyui_client.py
echo.

python --version >nul 2>&1
if %errorlevel% equ 0 (
    python "Scripts\comfyui_client.py"
    goto :end
)

py --version >nul 2>&1
if %errorlevel% equ 0 (
    py "Scripts\comfyui_client.py"
    goto :end
)

echo [ERROR] 未检测到 Python。
echo 请安装 Python，或将 Python 添加到 PATH 后重试。
pause
exit /b 1

:end
if errorlevel 1 (
    echo.
    echo [ERROR] 客户端异常退出，错误码: %errorlevel%
    pause
)

endlocal
