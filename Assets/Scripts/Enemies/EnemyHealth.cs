using System;
using Unity.Netcode;
using UnityEngine;

public class EnemyHealth : NetworkBehaviour
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
        if (!IsServer) return;

        DungeonSettings.OnDungeonDifficultyChange += DungeonDifficulty_OnDungeonDifficultyChange;
    }

    private void DungeonDifficulty_OnDungeonDifficultyChange(object sender,
        DungeonSettings.OnDungeonDifficultyChangeEventArgs e)
    {
        var currentHpDifficultyMultiplayer =
            DungeonSettings.GetEnemiesHpMultiplayerByDungeonDifficulty(e.newDungeonDifficulty);
        var currentHpPlayersCountMultiplayer = DungeonSettings.GetEnemiesHpMultiplayerByPlayersCount();

        var healthPercentage = (float)currentHealth / maxHealth;

        var newMaxHealth = (int)(maxHealth * currentHpDifficultyMultiplayer * currentHpPlayersCountMultiplayer);

        var newCurrentHealth = currentHealth = (int)(maxHealth * healthPercentage);

        OnDungeonDifficultyChangeClientRpc(newCurrentHealth, newMaxHealth);
    }

    [ClientRpc]
    private void OnDungeonDifficultyChangeClientRpc(int newCurrentHealth, int newMaxHealth)
    {
        currentHealth = newCurrentHealth;
        maxHealth = newMaxHealth;

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
        if (!IsServer) return;

        TakeDamageServerRpc(damage);
    }

    [ServerRpc(RequireOwnership = false)]
    private void TakeDamageServerRpc(int damage)
    {
        var takenDamage =
            (int)(damage * (1 - (float)currentDefence / (additionalDefenceNumberFormula + currentDefence)));
        var newCurrentShieldDurability = currentShieldDurability;

        if (currentShieldDurability > 0)
        {
            newCurrentShieldDurability = Mathf.Clamp(currentShieldDurability - takenDamage, 0, currentShieldDurability);

            takenDamage -= currentShieldDurability;
        }

        var newCurrentHealth = Mathf.Clamp(currentHealth - takenDamage, 0, maxHealth);

        if (currentHealth <= 0)
            OnEnemyDie?.Invoke(this, EventArgs.Empty);
        else
            TakeDamageClientRpc(newCurrentHealth, newCurrentShieldDurability, takenDamage);
    }

    [ClientRpc]
    private void TakeDamageClientRpc(int newCurrentHealth, int newCurrentShieldDurability, int takenDamage)
    {
        currentHealth = newCurrentHealth;
        currentShieldDurability = newCurrentShieldDurability;

        OnEnemyHealthChange?.Invoke(this, new OnEnemyHealthChangeEventArgs
        {
            currentHealth = currentHealth, maxHealth = maxHealth, lostHealth = takenDamage, obtainedHealth = 0
        });
    }

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
    private void RegenerateHealthServerRpc(float healthToRegenerate, bool isValuePercentage)
    {
        if (isValuePercentage)
            healthToRegenerate *= maxHealth;

        var newCurrentHealth = Mathf.Clamp(currentHealth + (int)healthToRegenerate, 0, maxHealth);

        RegenerateHealthClientRpc(newCurrentHealth, (int)healthToRegenerate);
    }

    [ClientRpc]
    private void RegenerateHealthClientRpc(int newCurrentHealth, int regeneratedHealth)
    {
        currentHealth = newCurrentHealth;

        OnEnemyHealthChange?.Invoke(this, new OnEnemyHealthChangeEventArgs
        {
            currentHealth = currentHealth, maxHealth = maxHealth, lostHealth = 0, obtainedHealth = regeneratedHealth
        });
    }

    public void ApplyShield(int shieldDurability)
    {
        if (!IsServer) return;

        ApplyShieldServerRpc(shieldDurability);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ApplyShieldServerRpc(int shieldDurability)
    {
        var newCurrentShieldDurability = currentShieldDurability + shieldDurability;

        ApplyShieldClientRpc(newCurrentShieldDurability);
    }

    [ClientRpc]
    private void ApplyShieldClientRpc(int newCurrentShieldDurability)
    {
        currentShieldDurability = newCurrentShieldDurability;
    }

    public void ChangeDefenceBuff(float percentageBuff)
    {
        if (!IsServer) return;

        ChangeDefenceBuffServerRpc(percentageBuff);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeDefenceBuffServerRpc(float percentageBuff)
    {
        var newCurrentDefence = (int)(currentDefence + baseDefence * percentageBuff);

        ChangeDefenceBuffClientRpc(newCurrentDefence);
    }

    [ClientRpc]
    private void ChangeDefenceBuffClientRpc(int newCurrentDefence)
    {
        currentDefence = newCurrentDefence;
    }

    public int GetCurrentShieldDurability()
    {
        return currentShieldDurability;
    }
}
