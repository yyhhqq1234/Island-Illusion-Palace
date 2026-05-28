using System.Collections.Generic;

public interface IEnemyProvider
{
    float damage { get; set; }
    bool isBerserk { get; set; }
    void EnterBerserkState();
    void TakeDamage(float amount);
}
