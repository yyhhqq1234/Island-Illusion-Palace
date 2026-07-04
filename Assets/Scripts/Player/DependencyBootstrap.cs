using UnityEngine;

/// <summary>
/// 依赖注入启动器 - 确保PlayerControllerDependencyInjector在场景启动时正确执行
/// 如果场景中没有注入器，自动创建一个；避免依赖查找遗漏
/// </summary>
[DefaultExecutionOrder(-900)]
public class DependencyBootstrap : MonoBehaviour
{
    void Awake()
    {
        // 确保场景中存在依赖注入器
        var injector = FindObjectOfType<PlayerControllerDependencyInjector>();
        if (injector == null)
        {
            var go = new GameObject("[Bootstrap] PlayerControllerDependencyInjector");
            go.AddComponent<PlayerControllerDependencyInjector>();
            Debug.Log("[DependencyBootstrap] 自动创建PlayerControllerDependencyInjector");
        }
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("GameObject/IIP/Create Dependency Bootstrap", false, 1)]
    static void CreateDependencyBootstrap()
    {
        var go = new GameObject("[IIP] DependencyBootstrap");
        go.AddComponent<DependencyBootstrap>();
        UnityEditor.Selection.activeGameObject = go;
        UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Create Dependency Bootstrap");
    }
#endif
}
