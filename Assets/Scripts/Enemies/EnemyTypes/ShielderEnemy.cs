using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyController))]
public class ShielderEnemy : MonoBehaviour
{
    #region Variables

    [SerializeField] private int applyingShieldDurability = 32;

    #endregion

    #region Enemy Type Methods

    public void ShieldEnemies(List<EnemyController> enemiesToShield)
    {
        foreach (var enemy in enemiesToShield)
            if (enemy.GetCurrentShieldDurability() < applyingShieldDurability)
                enemy.ApplyShield(applyingShieldDurability -
                                  enemy.GetCurrentShieldDurability());
    }

    #endregion
}
