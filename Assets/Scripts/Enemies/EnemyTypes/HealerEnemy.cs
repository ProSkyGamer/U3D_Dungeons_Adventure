using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyController))]
public class HealerEnemy : MonoBehaviour
{
    #region Variables

    [SerializeField] private int healingAmount = 17;

    #endregion

    #region Enemy Type Methods

    public void HealEnemies(List<EnemyController> enemiesHeal)
    {
        foreach (var enemy in enemiesHeal)
            enemy.Heal(healingAmount);
    }

    #endregion
}
