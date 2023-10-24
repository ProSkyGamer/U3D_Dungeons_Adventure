using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyController))]
public class HealerEnemy : MonoBehaviour
{
    [SerializeField] private int healingAmount = 17;

    public void HealEnemies(List<EnemyController> enemiesHeal)
    {
        foreach (var enemy in enemiesHeal)
            enemy.Heal(healingAmount);
    }
}
