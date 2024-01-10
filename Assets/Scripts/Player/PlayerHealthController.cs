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
    private float baseHealthIncreaseMultiplayer = 1f;
    private int maxHealth;
    private float healthIncreaseMultiplayer = 1f;
    private int currentHealth;

    [SerializeField] private int baseDefence = 100;
    private float baseDefenceIncreaseMultiplayer = 1f;
    [SerializeField] private int maxDefence = 1000;
    [SerializeField] [Range(0, 0.99f)] private float maxDefenceAbsorption = 0.5f;
    private int additionalDefenceNumberFormula;
    private int currentDefence;
    private float defenceIncreaseMultiplayer = 1f;

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

    public void ChangeBaseHealthBuff(float percentageBuff)
    {
        if (!IsServer) return;

        ChangeBaseHealthBuffServerRpc(percentageBuff);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeBaseHealthBuffServerRpc(float percentageBuff)
    {
        baseHealthIncreaseMultiplayer += percentageBuff;
        var currentHpPercentage = currentHealth / (float)maxHealth;

        var newBaseHealth = (int)(baseHealth * baseHealthIncreaseMultiplayer);
        var newMaxHealth = (int)(newBaseHealth * healthIncreaseMultiplayer);
        var newCurrentHealth = (int)(newMaxHealth * currentHpPercentage);

        ChangeBaseHealthBuffClientRpc(newMaxHealth, newCurrentHealth, baseHealthIncreaseMultiplayer);
    }

    [ClientRpc]
    private void ChangeBaseHealthBuffClientRpc(int newMaxHealth, int newCurrentHealth,
        float newBaseHealthIncreaseMultiplayer)
    {
        maxHealth = newMaxHealth;
        currentHealth = newCurrentHealth;
        baseHealthIncreaseMultiplayer = newBaseHealthIncreaseMultiplayer;

        OnCurrentPlayerHealthChange?.Invoke(this, new OnCurrentPlayerHealthChangeEventArgs
        {
            currentHealth = currentHealth, maxHealth = maxHealth
        });
    }

    public void ChangeHealthBuff(float percentageBuff = default, int flatBuff = default)
    {
        if (!IsServer) return;

        ChangeHealthBuffServerRpc(percentageBuff, flatBuff);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeHealthBuffServerRpc(float percentageBuff, int flatBuff)
    {
        healthIncreaseMultiplayer += percentageBuff;
        var currentHpPercentage = currentHealth / (float)maxHealth;

        var newMaxHealth = (int)(baseHealth * baseHealthIncreaseMultiplayer * healthIncreaseMultiplayer + flatBuff);
        var newCurrentHealth = (int)(newMaxHealth * currentHpPercentage);

        ChangeHealthBuffClientRpc(newMaxHealth, newCurrentHealth, healthIncreaseMultiplayer);
    }

    [ClientRpc]
    private void ChangeHealthBuffClientRpc(int newMaxHealth, int newCurrentHealth, float newHealthIncreaseMultiplayer)
    {
        maxHealth = newMaxHealth;
        currentHealth = newCurrentHealth;
        healthIncreaseMultiplayer = newHealthIncreaseMultiplayer;

        OnCurrentPlayerHealthChange?.Invoke(this, new OnCurrentPlayerHealthChangeEventArgs
        {
            currentHealth = currentHealth, maxHealth = maxHealth
        });
    }

    public void ChangeBaseDefenceBuff(float percentageBuff)
    {
        if (!IsServer) return;

        ChangeBaseDefenceBuffServerRpc(percentageBuff);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeBaseDefenceBuffServerRpc(float percentageBuff)
    {
        baseDefenceIncreaseMultiplayer += percentageBuff;

        var newBaseDefence = (int)(baseDefence * baseDefenceIncreaseMultiplayer);
        var newCurrentDefence = (int)(newBaseDefence * defenceIncreaseMultiplayer);

        ChangeBaseDefenceBuffClientRpc(newCurrentDefence, baseDefenceIncreaseMultiplayer);
    }

    [ClientRpc]
    private void ChangeBaseDefenceBuffClientRpc(int newCurrentDefence, float newBaseDefenceIncreaseMultiplayer)
    {
        currentDefence = newCurrentDefence;
        baseDefenceIncreaseMultiplayer = newBaseDefenceIncreaseMultiplayer;

        OnCurrentDefenceChange?.Invoke(this, EventArgs.Empty);
    }

    public void ChangeDefenceBuff(float percentageBuff = default, int flatBuff = default)
    {
        if (!IsServer) return;

        ChangeDefenceBuffServerRpc(percentageBuff, flatBuff);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeDefenceBuffServerRpc(float percentageBuff, int flatBuff)
    {
        defenceIncreaseMultiplayer += percentageBuff;
        var newCurrentDefence =
            (int)(baseDefence * baseDefenceIncreaseMultiplayer * defenceIncreaseMultiplayer + flatBuff);

        ChangeDefenceBuffClientRpc(newCurrentDefence, defenceIncreaseMultiplayer);
    }

    [ClientRpc]
    private void ChangeDefenceBuffClientRpc(int newCurrentDefence, float newDefenceIncreaseMultiplayer)
    {
        currentDefence = newCurrentDefence;
        defenceIncreaseMultiplayer = newDefenceIncreaseMultiplayer;

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
        return (int)(baseHealth * baseHealthIncreaseMultiplayer);
    }

    public int GetBaseDefence()
    {
        return (int)(baseDefence * baseDefenceIncreaseMultiplayer);
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
