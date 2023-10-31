using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 75;
    private int currentHealth;

    [SerializeField] private int baseDefence = 75;
    [SerializeField] private int maxDefence = 1000;
    [SerializeField] [Range(0, 0.99f)] private float maxDefenceAbsorption = 0.5f;
    private int additionalDefenceNumberFormula;
    private int currentDefence;

    private int currentShieldDurability;

    public event EventHandler<OnEnemyHealthChangeEventArgs> OnEnemyHealthChange;
    public event EventHandler OnEnemyDie;

    public class OnEnemyHealthChangeEventArgs : EventArgs
    {
        public int currentHealth;
        public int maxHealth;
        public int lostHealth;
        public int obtainedHealth;
    }

    private bool isFirstUpdate = true;

    private void Awake()
    {
        currentHealth = maxHealth;
        currentDefence = baseDefence;

        additionalDefenceNumberFormula =
            (int)(maxDefence * (1 - maxDefenceAbsorption) / maxDefenceAbsorption);
    }

    private void Start()
    {
        DungeonDifficulty.OnDungeonDifficultyChange += DungeonDifficulty_OnDungeonDifficultyChange;
    }

    private void DungeonDifficulty_OnDungeonDifficultyChange(object sender,
        DungeonDifficulty.OnDungeonDifficultyChangeEventArgs e)
    {
        var currentHpDifficultyMultiplayer =
            DungeonDifficulty.GetEnemiesHpMultiplayerByDungeonDifficulty(e.newDungeonDifficulty);
        var currentHpPlayersCountMultiplayer = DungeonDifficulty.GetEnemiesHpMultiplayerByPlayersCount();

        var healthPercentage = (float)currentHealth / maxHealth;

        maxHealth = (int)(maxHealth * currentHpDifficultyMultiplayer * currentHpPlayersCountMultiplayer);

        currentHealth = (int)(maxHealth * healthPercentage);

        OnEnemyHealthChange?.Invoke(this, new OnEnemyHealthChangeEventArgs
        {
            currentHealth = currentHealth, maxHealth = maxHealth, lostHealth = 0, obtainedHealth = 0
        });
    }

    private void Update()
    {
        if (isFirstUpdate)
        {
            isFirstUpdate = false;
            OnEnemyHealthChange?.Invoke(this, new OnEnemyHealthChangeEventArgs
            {
                currentHealth = currentHealth, maxHealth = maxHealth, lostHealth = 0, obtainedHealth = 0
            });
        }
    }

    public void TakeDamage(int damage)
    {
        var takenDamage =
            (int)(damage * (1 - (float)currentDefence / (additionalDefenceNumberFormula + currentDefence)));

        if (currentShieldDurability > 0)
        {
            if (currentShieldDurability >= takenDamage)
            {
                currentShieldDurability -= takenDamage;
                return;
            }

            takenDamage -= currentShieldDurability;
            currentShieldDurability = 0;
        }

        currentHealth = Mathf.Clamp(currentHealth - takenDamage, 0, maxHealth);
        OnEnemyHealthChange?.Invoke(this, new OnEnemyHealthChangeEventArgs
        {
            currentHealth = currentHealth, maxHealth = maxHealth, lostHealth = takenDamage, obtainedHealth = 0
        });

        if (currentHealth <= 0)
            OnEnemyDie?.Invoke(this, EventArgs.Empty);
    }

    public void RegenerateHealth(int healthToRegenerate)
    {
        currentHealth = Mathf.Clamp(currentHealth + healthToRegenerate, 0, maxHealth);
        OnEnemyHealthChange?.Invoke(this, new OnEnemyHealthChangeEventArgs
        {
            currentHealth = currentHealth, maxHealth = maxHealth, lostHealth = 0, obtainedHealth = healthToRegenerate
        });
    }

    public void RegenerateHealth(float healthPercentageToRegenerate)
    {
        var healthToRegenerate = (int)(maxHealth * healthPercentageToRegenerate);

        currentHealth = Mathf.Clamp(currentHealth + healthToRegenerate, 0, maxHealth);
        OnEnemyHealthChange?.Invoke(this, new OnEnemyHealthChangeEventArgs
        {
            currentHealth = currentHealth, maxHealth = maxHealth, lostHealth = 0, obtainedHealth = healthToRegenerate
        });
    }

    public void ApplyShield(int shieldDurability)
    {
        currentShieldDurability += shieldDurability;
    }

    public void ChangeDefenceBuff(float percentageBuff)
    {
        currentDefence += (int)(baseDefence * percentageBuff);
    }

    public int GetCurrentShieldDurability()
    {
        return currentShieldDurability;
    }
}
