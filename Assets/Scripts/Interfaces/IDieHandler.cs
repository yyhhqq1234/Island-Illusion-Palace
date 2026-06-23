/// <summary>
/// 死亡处理器接口 — 实现此接口的 MonoBehaviour 可通过 HealthSystem
/// 事件驱动方式触发 OnDie()，替代 HealthSystem 直接 GetComponent 调用。
/// </summary>
public interface IDieHandler
{
    void OnDie();
}
