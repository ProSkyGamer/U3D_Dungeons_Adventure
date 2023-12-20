using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class EnemyEffects : NetworkBehaviour
{
    public enum EnemiesEffects
    {
        DefBuff,
        AtkBuff,
        SlowDebuff
    }

    private class AppliedEnemiesEffects
    {
        public EnemiesEffects enemiesEffectType;
        public bool isBuffEndless;
        public float buffDuration;
        public float buffValue;
    }

    private readonly List<AppliedEnemiesEffects> appliedEffects = new();

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
        if (!IsServer) return;
        if (GameStageManager.Instance.IsPause()) return;

        TickEffectsTimers();
    }

    private void TickEffectsTimers()
    {
        List<AppliedEnemiesEffects> effectsToRemove = new();
        foreach (var appliedEffect in appliedEffects)
            if (!appliedEffect.isBuffEndless)
            {
                appliedEffect.buffDuration -= Time.deltaTime;
                if (appliedEffect.buffDuration <= 0)
                    effectsToRemove.Add(appliedEffect);
            }

        foreach (var effectToRemove in effectsToRemove)
            AddOrRemoveEffect(effectToRemove.enemiesEffectType, effectToRemove.buffValue, false);
    }

    public void AddOrRemoveEffect(EnemiesEffects enemiesEffectToApply, float percentageScale, bool isAdding = true,
        bool isBuffEndless = true, float buffDuration = 1f)
    {
        if (!IsServer) return;

        if (isAdding)
        {
            var appliedBuff = new AppliedEnemiesEffects
            {
                enemiesEffectType = enemiesEffectToApply,
                buffValue = percentageScale,
                isBuffEndless = isBuffEndless,
                buffDuration = buffDuration
            };

            appliedEffects.Add(appliedBuff);
        }
        else
        {
            foreach (var appliedBuff in appliedEffects)
                if (appliedBuff.enemiesEffectType == enemiesEffectToApply && appliedBuff.buffValue == percentageScale)
                {
                    appliedEffects.Remove(appliedBuff);
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