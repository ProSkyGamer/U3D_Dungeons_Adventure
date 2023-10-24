using UnityEngine;

public class EnemyEffects : MonoBehaviour
{
    public enum Buffs
    {
        DefBuff,
        AtkBuff
    }

    private EnemyAttackController enemyAttackController;
    private EnemyHealth enemyHealth;

    private void Awake()
    {
        enemyAttackController = GetComponent<EnemyAttackController>();
        enemyHealth = GetComponent<EnemyHealth>();
    }

    public void ApplyBuff(Buffs buffToApply, float percentageScale)
    {
        switch (buffToApply)
        {
            case Buffs.AtkBuff:
                enemyAttackController.ChangeAttackBuff(percentageScale);
                break;
            case Buffs.DefBuff:
                enemyHealth.ChangeDefenceBuff(percentageScale);
                break;
        }
    }

    public void DispelBuff(Buffs buffToApply, float percentageScale)
    {
        switch (buffToApply)
        {
            case Buffs.AtkBuff:
                enemyAttackController.ChangeAttackBuff(-percentageScale);
                break;
            case Buffs.DefBuff:
                enemyHealth.ChangeDefenceBuff(-percentageScale);
                break;
        }
    }
}
