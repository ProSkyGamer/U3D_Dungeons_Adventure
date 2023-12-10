using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    #region GeneralStats

    [SerializeField] private int baseHealth = 100;
    private static int maxHealth;
    private static int currentHealth;

    [SerializeField] private int baseDefence = 100;
    [SerializeField] private int maxDefence = 1000;
    [SerializeField] [Range(0, 0.99f)] private float maxDefenceAbsorption = 0.5f;
    private int additionalDefenceNumberFormula;
    private int currentDefence;

    public class DeathSavingBuff
    {
        public float regenerateHp;
        public int effectId;
    }

    private readonly List<DeathSavingBuff> deathSavingBuffs = new();

    public class DmgAbsorptionBuff
    {
        public float dmgAbsorptionMultiplayer;
        public int effectId;
    }

    private readonly List<DmgAbsorptionBuff> currentDamageAbsorptionBuffs = new();

    #endregion

    public static event EventHandler<OnCurrentPlayerHealthChangeEventArgs> OnCurrentPlayerHealthChange;
    public static event EventHandler OnCurrentDefenceChange;

    public class OnCurrentPlayerHealthChangeEventArgs : EventArgs
    {
        public int maxHealth;
        public int currentHealth;
    }

    public event EventHandler<PlayerEffects.RelicBuffEffectTriggeredEventArgs> OnHealthAbsorptionTriggered;
    public event EventHandler<PlayerEffects.RelicBuffEffectTriggeredEventArgs> OnDeathSavingEffectTriggered;

    private bool isFirstUpdate = true;

    private void Awake()
    {
        if (currentHealth == 0)
        {
            maxHealth = baseHealth;
            currentHealth = maxHealth;
        }

        currentDefence = baseDefence;

        additionalDefenceNumberFormula =
            (int)(maxDefence * (1 - maxDefenceAbsorption) / maxDefenceAbsorption);
    }

    private void Update()
    {
        if (isFirstUpdate)
        {
            isFirstUpdate = false;

            OnCurrentPlayerHealthChange?.Invoke(this, new OnCurrentPlayerHealthChangeEventArgs
            {
                currentHealth = currentHealth, maxHealth = maxHealth
            });
        }
    }

    public void TakeDamage(int damage)
    {
        var dmgAbsorptionMultiplayer = GetDamageAbsorptionMultiplayer(out var usedEffects);
        var takenDamage =
            Mathf.Clamp(
                (int)(damage * (1 - (float)currentDefence / (additionalDefenceNumberFormula + currentDefence)) *
                      (1 - dmgAbsorptionMultiplayer)), 0, damage);

        if (dmgAbsorptionMultiplayer != 0f)
        {
            var absorbedDamage =
                (int)(damage * (1 - (float)currentDefence / (additionalDefenceNumberFormula + currentDefence))) -
                takenDamage;

            foreach (var usedEffect in usedEffects)
            {
                var effectAbsorbedDamage = (int)(absorbedDamage * usedEffect.dmgAbsorptionMultiplayer);

                OnHealthAbsorptionTriggered?.Invoke(this, new PlayerEffects.RelicBuffEffectTriggeredEventArgs
                {
                    spentValue = effectAbsorbedDamage,
                    effectID = usedEffect.effectId
                });
                Debug.Log(usedEffect.effectId);
            }
        }

        currentHealth = Mathf.Clamp(currentHealth - takenDamage, 0, maxHealth);
        OnCurrentPlayerHealthChange?.Invoke(this, new OnCurrentPlayerHealthChangeEventArgs
        {
            currentHealth = currentHealth, maxHealth = maxHealth
        });

        if (currentHealth <= 0)
        {
            if (deathSavingBuffs.Count > 0)
            {
                RegenerateHealth(deathSavingBuffs[0].regenerateHp);
                OnDeathSavingEffectTriggered?.Invoke(this, new PlayerEffects.RelicBuffEffectTriggeredEventArgs
                {
                    spentValue = 1, effectID = deathSavingBuffs[0].effectId
                });
                return;
            }

            Debug.Log("U'r dead!");
        }
    }

    public void RegenerateHealth(int healthToRegenerate)
    {
        currentHealth = Mathf.Clamp(currentHealth + healthToRegenerate, 0, maxHealth);
        OnCurrentPlayerHealthChange?.Invoke(this, new OnCurrentPlayerHealthChangeEventArgs
        {
            currentHealth = currentHealth, maxHealth = maxHealth
        });
    }

    public void RegenerateHealth(float healthPercentageToRegenerate)
    {
        var healthToRegenerate = (int)(healthPercentageToRegenerate * maxHealth);
        currentHealth = Mathf.Clamp(currentHealth + healthToRegenerate, 0, maxHealth);
        OnCurrentPlayerHealthChange?.Invoke(this, new OnCurrentPlayerHealthChangeEventArgs
        {
            currentHealth = currentHealth, maxHealth = maxHealth
        });
    }

    public void ChangeHealthBuff(float percentageBuff = default, int flatBuff = default)
    {
        float currentHpPercentage = currentHealth / maxHealth;

        maxHealth += (int)(baseHealth * percentageBuff);
        maxHealth += flatBuff;

        currentHealth = (int)(maxHealth * currentHpPercentage);
    }

    public void ChangeDefenceBuff(float percentageBuff = default, int flatBuff = default)
    {
        currentDefence += (int)(baseDefence * percentageBuff);
        currentDefence += flatBuff;

        OnCurrentDefenceChange?.Invoke(this, EventArgs.Empty);
    }

    public void ChangeTakenDamageAbsorptionBuff(float percentageBuff, int buffId)
    {
        if (percentageBuff > 0)
        {
            var absorptionDamageBuff = new DmgAbsorptionBuff
            {
                dmgAbsorptionMultiplayer = percentageBuff,
                effectId = buffId
            };
            currentDamageAbsorptionBuffs.Add(absorptionDamageBuff);
        }
        else
        {
            foreach (var dmgAbsorptionBuff in currentDamageAbsorptionBuffs)
            {
                if (dmgAbsorptionBuff.effectId != buffId) continue;

                currentDamageAbsorptionBuffs.Remove(dmgAbsorptionBuff);
                break;
            }
        }
    }

    public void ChangeTakenDamageIncreaseDebuff(float percentageDebuff)
    {
        if (percentageDebuff > 0)
        {
            var absorptionDamageBuff = new DmgAbsorptionBuff
            {
                dmgAbsorptionMultiplayer = percentageDebuff,
                effectId = -1
            };
            currentDamageAbsorptionBuffs.Add(absorptionDamageBuff);
        }
        else
        {
            foreach (var dmgAbsorptionBuff in currentDamageAbsorptionBuffs)
            {
                if (dmgAbsorptionBuff.effectId != -1) continue;

                currentDamageAbsorptionBuffs.Remove(dmgAbsorptionBuff);
                break;
            }
        }
    }

    public void ChangeDeathSavingEffect(float hpRegeneration, int effectId)
    {
        if (hpRegeneration > 0)
        {
            var deathSavingBuff = new DeathSavingBuff
            {
                regenerateHp = hpRegeneration,
                effectId = effectId
            };
            deathSavingBuffs.Add(deathSavingBuff);
        }
        else
        {
            foreach (var deathSavingBuff in deathSavingBuffs)
            {
                if (effectId != deathSavingBuff.effectId) continue;

                deathSavingBuffs.Remove(deathSavingBuff);
                break;
            }
        }
    }

    private float GetDamageAbsorptionMultiplayer(out List<DmgAbsorptionBuff> usedEffectsId)
    {
        usedEffectsId = new List<DmgAbsorptionBuff>();
        var dmgAbsorptionMultiplayer = 0f;

        foreach (var dmgAbsorptionBuff in currentDamageAbsorptionBuffs)
        {
            if (dmgAbsorptionMultiplayer >= 1 && dmgAbsorptionBuff.dmgAbsorptionMultiplayer > 0)
                continue;

            dmgAbsorptionMultiplayer += dmgAbsorptionBuff.dmgAbsorptionMultiplayer;
            usedEffectsId.Add(dmgAbsorptionBuff);
        }

        return dmgAbsorptionMultiplayer;
    }

    #region GetVariablesData

    public int GetBaseHp()
    {
        return baseHealth;
    }

    public int GetBaseDefence()
    {
        return baseDefence;
    }

    public int GetCurrentMaxHp()
    {
        return maxHealth;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetCurrentDefence()
    {
        return currentDefence;
    }

    #endregion

    public static void ResetStaticData()
    {
        OnCurrentPlayerHealthChange = null;
    }
}
