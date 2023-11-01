using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    #region GeneralStats

    [SerializeField] private int baseHealth = 100;
    private int maxHealth;
    private int currentHealth;

    [SerializeField] private int baseDefence = 100;
    [SerializeField] private int maxDefence = 1000;
    [SerializeField] [Range(0, 0.99f)] private float maxDefenceAbsorption = 0.5f;
    private int additionalDefenceNumberFormula;
    private int currentDefence;

    private float currentTakenDmgAbsorption;

    #endregion

    public static event EventHandler<OnCurrentPlayerHealthChangeEventArgs> OnCurrentPlayerHealthChange;

    public class OnCurrentPlayerHealthChangeEventArgs : EventArgs
    {
        public int maxHealth;
        public int currentHealth;
    }

    private bool isFirstUpdate = true;

    private void Awake()
    {
        maxHealth = baseHealth;
        currentHealth = maxHealth;

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
        var takenDamage =
            (int)(damage * (1 - (float)currentDefence / (additionalDefenceNumberFormula + currentDefence)) *
                  (1 - currentTakenDmgAbsorption));

        Debug.Log($"Taken damage {takenDamage} Def: {currentDefence} AddDef: {additionalDefenceNumberFormula}");

        currentHealth = Mathf.Clamp(currentHealth - takenDamage, 0, maxHealth);
        OnCurrentPlayerHealthChange?.Invoke(this, new OnCurrentPlayerHealthChangeEventArgs
        {
            currentHealth = currentHealth, maxHealth = maxHealth
        });

        if (currentHealth <= 0) Debug.Log("U'r dead!");
    }

    public void RegenerateHealth(int healthToRegenerate)
    {
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
    }

    public void ChangeTakenDamageAbsorptionBuff(float percentageBuff)
    {
        currentTakenDmgAbsorption += percentageBuff;
    }

    public void ChangeTakenDamageIncreaseDebuff(float percentageDebuff)
    {
        currentTakenDmgAbsorption -= percentageDebuff;
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

    public int GetCurrentDefence()
    {
        return currentDefence;
    }

    #endregion
}
