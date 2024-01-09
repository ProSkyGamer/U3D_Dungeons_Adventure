using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerHealthController : NetworkBehaviour
{
    #region Effects & Event Args

    public event EventHandler<OnCurrentPlayerHealthChangeEventArgs> OnCurrentPlayerHealthChange;
    public event EventHandler OnCurrentDefenceChange;

    public class OnCurrentPlayerHealthChangeEventArgs : EventArgs
    {
        public int maxHealth;
        public int currentHealth;
    }

    public event EventHandler<PlayerEffectsController.RelicBuffEffectTriggeredEventArgs> OnHealthAbsorptionTriggered;
    public event EventHandler<PlayerEffectsController.RelicBuffEffectTriggeredEventArgs> OnDeathSavingEffectTriggered;

    #endregion

    #region Created Classes

    public class DmgAbsorptionBuff
    {
        public float dmgAbsorptionMultiplayer;
        public int effectId;
    }

    public class DeathSavingBuff
    {
        public float regenerateHp;
        public int effectId;
    }

    #endregion

    #region GeneralStats

    [SerializeField] private int baseHealth = 100;
    private int maxHealth;
    private int currentHealth;

    [SerializeField] private int baseDefence = 100;
    [SerializeField] private int maxDefence = 1000;
    [SerializeField] [Range(0, 0.99f)] private float maxDefenceAbsorption = 0.5f;
    private int additionalDefenceNumberFormula;
    private int currentDefence;

    #endregion

    #region Effects

    private readonly List<DeathSavingBuff> deathSavingBuffs = new();

    private readonly List<DmgAbsorptionBuff> currentDamageAbsorptionBuffs = new();

    #endregion

    private bool isFirstUpdate = true;

    #region Initialization

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

    #endregion

    #region Update

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

    #endregion

    #region Take Damage

    public void TakePureDamage(int damage)
    {
        TakePureDamageServerRpc(damage);
    }

    [ServerRpc(RequireOwnership = false)]
    private void TakePureDamageServerRpc(int damage)
    {
        var newCurrentHealthValue = currentHealth - damage;

        TakePureDamageClientRpc(newCurrentHealthValue);
    }

    [ClientRpc]
    private void TakePureDamageClientRpc(int newCurrentHealthValue)
    {
        currentHealth = newCurrentHealthValue;

        OnCurrentPlayerHealthChange?.Invoke(this, new OnCurrentPlayerHealthChangeEventArgs
        {
            currentHealth = currentHealth, maxHealth = maxHealth
        });
    }

    public void TakeDamage(int damage)
    {
        TakeDamageServerRpc(damage);
    }

    [ServerRpc(RequireOwnership = false)]
    private void TakeDamageServerRpc(int damage)
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

                OnHealthAbsorptionTriggered?.Invoke(this, new PlayerEffectsController.RelicBuffEffectTriggeredEventArgs
                {
                    spentValue = effectAbsorbedDamage,
                    effectID = usedEffect.effectId
                });
                Debug.Log(usedEffect.effectId);
            }
        }

        var newCurrentHealth = Mathf.Clamp(currentHealth - takenDamage, 0, maxHealth);

        if (newCurrentHealth <= 0)
        {
            if (deathSavingBuffs.Count > 0)
            {
                RegenerateHealth(deathSavingBuffs[0].regenerateHp);
                OnDeathSavingEffectTriggered?.Invoke(this, new PlayerEffectsController.RelicBuffEffectTriggeredEventArgs
                {
                    spentValue = 1, effectID = deathSavingBuffs[0].effectId
                });
                return;
            }

            Debug.Log("U'r dead!");
        }

        TakeDamageClientRpc(newCurrentHealth);
    }

    [ClientRpc]
    private void TakeDamageClientRpc(int newHealth)
    {
        currentHealth = newHealth;

        OnCurrentPlayerHealthChange?.Invoke(this, new OnCurrentPlayerHealthChangeEventArgs
        {
            currentHealth = currentHealth, maxHealth = maxHealth
        });
    }

    #endregion

    #region Regenerate Health

    public void RegenerateHealth(int healthToRegenerate)
    {
        if (!IsServer) return;

        RegenerateHealthServerRpc(healthToRegenerate, false);
    }

    public void RegenerateHealth(float healthPercentageToRegenerate)
    {
        if (!IsServer) return;

        RegenerateHealthServerRpc(healthPercentageToRegenerate, true);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RegenerateHealthServerRpc(float deltaHealth, bool isValuePercentage)
    {
        if (isValuePercentage) deltaHealth *= maxHealth;
        var newCurrentHealth = Mathf.Clamp(currentHealth + (int)deltaHealth, 0, maxHealth);

        RegenerateHealthClientRpc(newCurrentHealth);
    }

    [ClientRpc]
    private void RegenerateHealthClientRpc(int newCurrentHealth)
    {
        currentHealth = newCurrentHealth;

        OnCurrentPlayerHealthChange?.Invoke(this, new OnCurrentPlayerHealthChangeEventArgs
        {
            currentHealth = currentHealth, maxHealth = maxHealth
        });
    }

    #endregion

    #region Buffs

    public void ChangeHealthBuff(float percentageBuff = default, int flatBuff = default)
    {
        ChangeHealthBuffServerRpc(percentageBuff, flatBuff);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeHealthBuffServerRpc(float percentageBuff, int flatBuff)
    {
        var currentHpPercentage = currentHealth / (float)maxHealth;

        var newMaxHealth = (int)(maxHealth + baseHealth * percentageBuff + flatBuff);
        var newCurrentHealth = (int)(newMaxHealth * currentHpPercentage);

        ChangeHealthBuffClientRpc(newMaxHealth, newCurrentHealth);
    }

    [ClientRpc]
    private void ChangeHealthBuffClientRpc(int newMaxHealth, int newCurrentHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = newCurrentHealth;

        OnCurrentPlayerHealthChange?.Invoke(this, new OnCurrentPlayerHealthChangeEventArgs
        {
            currentHealth = currentHealth, maxHealth = maxHealth
        });
    }

    public void ChangeDefenceBuff(float percentageBuff = default, int flatBuff = default)
    {
        ChangeDefenceBuffServerRpc(percentageBuff, flatBuff);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeDefenceBuffServerRpc(float percentageBuff, int flatBuff)
    {
        var newCurrentDefence = (int)(currentDefence + baseDefence * percentageBuff + flatBuff);

        ChangeDefenceBuffClientRpc(newCurrentDefence);
    }

    [ClientRpc]
    private void ChangeDefenceBuffClientRpc(int newCurrentDefence)
    {
        currentDefence = newCurrentDefence;

        OnCurrentDefenceChange?.Invoke(this, EventArgs.Empty);
    }

    public void ChangeTakenDamageAbsorptionBuff(float percentageBuff, int buffId)
    {
        ChangeTakenDamageAbsorptionBuffServerRpc(percentageBuff, buffId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeTakenDamageAbsorptionBuffServerRpc(float percentageBuff, int buffId)
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
        ChangeTakenDamageIncreaseDebuffServerRpc(percentageDebuff);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeTakenDamageIncreaseDebuffServerRpc(float percentageDebuff)
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
        ChangeDeathSavingEffectServerRpc(hpRegeneration, effectId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeDeathSavingEffectServerRpc(float hpRegeneration, int effectId)
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

    #endregion

    #region Get Damage Absorbtion

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

    #endregion

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
}
