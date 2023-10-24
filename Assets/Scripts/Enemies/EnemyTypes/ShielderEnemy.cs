using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyController))]
public class ShielderEnemy : MonoBehaviour
{
    [SerializeField] private int applyingShieldDurability = 32;

    public void ShieldEnemies(List<EnemyController> enemiesToShield)
    {
        foreach (var enemy in enemiesToShield)
            if (enemy.GetCurrentShieldDurability() < applyingShieldDurability)
                enemy.ApplyShield(applyingShieldDurability -
                                  enemy.GetCurrentShieldDurability());
    }
}
