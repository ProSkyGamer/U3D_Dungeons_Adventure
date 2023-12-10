using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyEffects : MonoBehaviour
{
    public enum EnemiesEffects
    {
        DefBuff,
        AtkBuff,
        SlowDebuff
    }

    private class AppliedEnimiesEffects
    {
        public EnemiesEffects enemiesEffectType;
        public bool isBuffEndless;
        public float buffDuration;
        public float buffValue;
    }

    private readonly List<AppliedEnimiesEffects> appliedBuffs = new();

    private EnemyAttackController enemyAttackController;
    private EnemyHealth enemyHealth;
    private NavMeshAgent navMeshAgent;

    private float baseSpeed;

    private void Awake()
    {
        enemyAttackController = GetComponent<EnemyAttackController>();
        enemyHealth = GetComponent<EnemyHealth>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        baseSpeed = navMeshAgent.speed;
    }

    private void Update()
    {
        if (GameStageManager.Instance.IsPause()) return;

        TickBuffTimers();
    }

    private void TickBuffTimers()
    {
        foreach (var buff in appliedBuffs)
            if (!buff.isBuffEndless)
            {
                buff.buffDuration -= Time.deltaTime;
                if (buff.buffDuration <= 0) AddOrRemoveEffect(buff.enemiesEffectType, buff.buffValue, false);
            }
    }

    public void AddOrRemoveEffect(EnemiesEffects enemiesEffectToApply, float percentageScale, bool isAdding = true,
        bool isBuffEndless = true, float buffDuration = 1f)
    {
        if (isAdding)
        {
            var appliedBuff = new AppliedEnimiesEffects
            {
                enemiesEffectType = enemiesEffectToApply,
                buffValue = percentageScale,
                isBuffEndless = isBuffEndless,
                buffDuration = buffDuration
            };

            appliedBuffs.Add(appliedBuff);
        }
        else
        {
            foreach (var appliedBuff in appliedBuffs)
                if (appliedBuff.enemiesEffectType == enemiesEffectToApply && appliedBuff.buffValue == percentageScale)
                {
                    appliedBuffs.Remove(appliedBuff);
                    break;
                }

            percentageScale = -percentageScale;
        }

        switch (enemiesEffectToApply)
        {
            case EnemiesEffects.AtkBuff:
                enemyAttackController.ChangeAttackBuff(percentageScale);
                break;
            case EnemiesEffects.DefBuff:
                enemyHealth.ChangeDefenceBuff(percentageScale);
                break;
            case EnemiesEffects.SlowDebuff:
                navMeshAgent.speed -= baseSpeed * percentageScale;
                break;
        }
    }
}