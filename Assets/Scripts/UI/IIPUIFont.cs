using UnityEngine;
using UnityEngine.UI;

namespace IIPUI
{
    /// <summary>
    /// 菜单 UI 统一字体工具。
    /// 根治 LegacyRuntime 动态字体首帧中文字形未烘焙导致的"操作/音画/辅助"等汉字乱码。
    /// 采用 OS 动态字体（微软雅黑 UI），Windows 目标平台保证字形完整。
    /// </summary>
    public static class IIPUIFont
    {
        // 菜单里所有可能出现的中文，提前 Request 进字体贴图，避免首帧豆腐块
        private const string MENU_CHARS =
            "游戏暂停设置音画操作辅助主音量音效音乐返回继续退出移动攻击闪避" +
            "召唤轮盘快捷背包炼金伤害数字显示小地图自动拾取物品色盲模式确定取消" +
            "0123456789%WASDRFICSpace鼠标左键";

        private static Font _font;

        /// <summary>获取共享的菜单字体实例（懒加载）。</summary>
        public static Font Get()
        {
            if (_font == null)
            {
                // Windows 独占项目，微软雅黑 UI 必装；缺失时 Unity 会回退系统默认字体
                _font = Font.CreateDynamicFontFromOSFont("Microsoft YaHei UI", 16);
            }
            return _font;
        }

        /// <summary>
        /// 把 root 子树下所有 Text.font 切到菜单字体，并预热中文常用字形。
        /// 运行时兜底用，防止场景未跑编辑器脚本时仍有乱码。
        /// </summary>
        public static void ApplyTo(Transform root)
        {
            if (root == null) return;
            var font = Get();
            // 多个字号都预热一下（标题32/按钮22/Tab20/键位18）
            font.RequestCharactersInTexture(MENU_CHARS, 32);
            font.RequestCharactersInTexture(MENU_CHARS, 22);
            font.RequestCharactersInTexture(MENU_CHARS, 20);
            font.RequestCharactersInTexture(MENU_CHARS, 18);

            foreach (var t in root.GetComponentsInChildren<Text>(true))
            {
                if (t == null) continue;
                t.font = font;
                t.SetAllDirty();
            }
        }
    }
}
