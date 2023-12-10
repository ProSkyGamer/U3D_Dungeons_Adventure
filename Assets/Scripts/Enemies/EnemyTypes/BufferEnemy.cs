using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyController))]
public class BufferEnemy : MonoBehaviour
{
    [SerializeField] private EnemyEffects.EnemiesEffects applyingEffect = EnemyEffects.EnemiesEffects.DefBuff;


    [SerializeField] private float buffScale = 0.2f;

    private readonly List<EnemyEffects> buffedEnemies = new();

    public void BuffEnemies(List<EnemyEffects> enemiesToBuff)
    {
        foreach (var enemy in enemiesToBuff)
            if (!buffedEnemies.Contains(enemy))
            {
                buffedEnemies.Add(enemy);

                enemy.AddOrRemoveEffect(applyingEffect, buffScale);
            }
    }

    private void OnDestroy()
    {
        foreach (var buffedEnemy in buffedEnemies) buffedEnemy.AddOrRemoveEffect(applyingEffect, buffScale, false);
    }
}