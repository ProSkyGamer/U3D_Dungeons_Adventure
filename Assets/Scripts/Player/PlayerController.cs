using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerAttackController))]
[RequireComponent(typeof(PlayerHealth))]
[RequireComponent(typeof(StaminaController))]
[RequireComponent(typeof(PlayerEffects))]
[RequireComponent(typeof(PlayerInventory))]
[RequireComponent(typeof(PlayerRelics))]
[RequireComponent(typeof(PlayerWeapons))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    public event EventHandler<OnExperienceChangeEventArgs> OnExperienceChange;

    public class OnExperienceChangeEventArgs : EventArgs
    {
        public int currentXp;
        public int maxXp;
    }

    public event EventHandler OnStopSprinting;

    public event EventHandler OnCoinsValueChange;
    public event EventHandler OnSkillPointsValueChange;
    public event EventHandler<PlayerEffects.RelicBuffEffectTriggeredEventArgs> OnPlayerRegenerateHpAfterEnemyDeath;

    [SerializeField] private int experienceForFirstLevel = 100;
    [SerializeField] private float experienceIncreaseForNextLevel = 0.2f;
    private int currentLevelXpNeeded;
    private int currentXp;
    private float additionalExpMultiplayer;

    private int currentAvailableSkillPoint;
    private int currentCoins;
    private float coinsOnEnemyDeathMultiplayer = 1f;

    [SerializeField] private float walkSpeed = 1f;
    [SerializeField] private float runSpeed = 3f;
    [SerializeField] private float sprintSpeed = 5f;
    private float speedModifier = 1f;
    [SerializeField] private float staminaSprintCostPerSecond = 7.5f;
    private bool isRunning = true;
    private bool isSprinting;
    private readonly float timeForConstantSprint = 2f;
    private float timerForConstantSprint;

    [SerializeField] private float waitingForStaminaRegenerationTime = 2.5f;
    private float waitingForStaminaRegenerationTimer;

    private readonly List<EnemyController> attackedNotDeadEnemies = new();

    private class HpRegenerationAfterEnemyDeath
    {
        public float hpRegenerationAmount;
        public int effectID;
    }

    private readonly List<HpRegenerationAfterEnemyDeath> hpRegenerationAfterEnemyDeathEffects = new();

    [SerializeField] private CameraController cameraController;
    private PlayerMovement playerMovement;
    private PlayerAttackController playerAttackController;
    private PlayerHealth playerHealth;
    private StaminaController staminaController;
    private PlayerEffects playerEffects;
    private PlayerInventory playerInventory;
    private PlayerRelics playerRelics;
    private PlayerWeapons playerWeapons;

    private bool isFirstUpdate = true;

    private void Awake()
    {
        var str = "Text text {1} {0}";

        var newstr = string.Format(str, 0, 1);
        Debug.Log(newstr);

        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;

        playerMovement = GetComponent<PlayerMovement>();
        playerAttackController = GetComponent<PlayerAttackController>();
        playerHealth = GetComponent<PlayerHealth>();
        staminaController = GetComponent<StaminaController>();
        playerEffects = GetComponent<PlayerEffects>();
        playerInventory = GetComponent<PlayerInventory>();
        playerRelics = GetComponent<PlayerRelics>();
        playerWeapons = GetComponent<PlayerWeapons>();

        timerForConstantSprint = timeForConstantSprint;
        waitingForStaminaRegenerationTimer = waitingForStaminaRegenerationTime;

        if (currentLevelXpNeeded != 0) return;
        currentLevelXpNeeded = experienceForFirstLevel;
    }

    private void Start()
    {
        GameInput.Instance.OnChangeCameraModeAction += GameInput_OnChangeCameraModeAction;
        GameInput.Instance.OnSprintAction += GameInput_OnSprintAction;
        GameInput.Instance.OnChangeMovementModeAction += GameInput_OnChangeMovementMode;

        staminaController.OnAllStaminaSpend += StaminaController_OnAllStaminaSpend;

        playerAttackController.OnEnemyHitted += PlayerAttackController_OnEnemyHitted;
    }

    private void PlayerAttackController_OnEnemyHitted(object sender, PlayerAttackController.OnEnemyHittedEventArgs e)
    {
        if (!attackedNotDeadEnemies.Contains(e.hittedEnemy))
        {
            attackedNotDeadEnemies.Add(e.hittedEnemy);
            e.hittedEnemy.OnEnemyDeath += EnemyController_OnEnemyDeath;
        }
    }

    #region SubscribedEvents

    private void StaminaController_OnAllStaminaSpend(object sender, EventArgs e)
    {
        isSprinting = false;
        OnStopSprinting?.Invoke(this, EventArgs.Empty);
    }

    private void GameInput_OnChangeMovementMode(object sender, EventArgs e)
    {
        isRunning = !isRunning;
    }

    private void GameInput_OnSprintAction(object sender, EventArgs e)
    {
        if (staminaController.IsHaveAvailableStamina())
            isSprinting = true;
    }

    private void GameInput_OnChangeCameraModeAction(object sender, EventArgs e)
    {
        switch (cameraController.GetCurrentCameraMode())
        {
            case CameraController.CameraModes.ThirdPerson:
                cameraController.ChangeCameraMode(CameraController.CameraModes.Weapon);
                break;
            case CameraController.CameraModes.Weapon:
                cameraController.ChangeCameraMode(CameraController.CameraModes.ThirdPerson);
                break;
        }
    }

    #endregion


    #region Update & Connected

    private void Update()
    {
        if (GameStageManager.Instance.IsPause()) return;

        if (isFirstUpdate)
        {
            isFirstUpdate = false;

            OnExperienceChange?.Invoke(this, new OnExperienceChangeEventArgs
            {
                currentXp = currentXp, maxXp = currentLevelXpNeeded
            });
            OnSkillPointsValueChange?.Invoke(this, EventArgs.Empty);
        }

        TryMove();
        TryStartStaminaRegeneration();
    }

    private void TryMove()
    {
        var moveVector = GameInput.Instance.GetMovementVectorNormalized();
        if (moveVector != Vector2.zero)
        {
            if (isSprinting)
            {
                staminaController.SpendStamina(staminaSprintCostPerSecond * Time.deltaTime);

                moveVector *= Time.deltaTime * sprintSpeed;

                if (timerForConstantSprint > 0)
                    if (GameInput.Instance.GetBindingValue(GameInput.Binding.Sprint) == 1f)
                    {
                        timerForConstantSprint -= Time.deltaTime;
                    }
                    else
                    {
                        timerForConstantSprint = timeForConstantSprint;
                        isSprinting = false;
                        OnStopSprinting?.Invoke(this, EventArgs.Empty);
                    }
            }
            else
            {
                if (isRunning)
                    moveVector *= Time.deltaTime * runSpeed;
                else
                    moveVector *= Time.deltaTime * walkSpeed;
            }

            moveVector *= speedModifier;
            playerMovement.Move(moveVector);

            return;
        }

        timerForConstantSprint = timeForConstantSprint;

        isSprinting = false;
        OnStopSprinting?.Invoke(this, EventArgs.Empty);
    }

    private void TryStartStaminaRegeneration()
    {
        if (!staminaController.IsRegeneratingStamina() && !staminaController.IsStaminaFull()
                                                       && !isSprinting)
        {
            waitingForStaminaRegenerationTimer -= Time.deltaTime;

            if (waitingForStaminaRegenerationTimer > 0) return;

            staminaController.StartStaminaRegeneration();
            waitingForStaminaRegenerationTimer = waitingForStaminaRegenerationTime;
        }
    }

    #endregion

    public void ChangeHealth(int healthChangeValue)
    {
        if (healthChangeValue > 0)
            playerHealth.RegenerateHealth(healthChangeValue);
        else
            playerHealth.TakeDamage(-healthChangeValue);
    }

    public void AddRegeneratingHpAfterEnemyDeath(float hpPercentageValue, int effectID)
    {
        if (hpPercentageValue > 0)
        {
            var hpRegenerationOnEnemyDeath = new HpRegenerationAfterEnemyDeath
            {
                hpRegenerationAmount = hpPercentageValue, effectID = effectID
            };
            hpRegenerationAfterEnemyDeathEffects.Add(hpRegenerationOnEnemyDeath);
        }
        else
        {
            foreach (var hpRegeneration in hpRegenerationAfterEnemyDeathEffects)
            {
                if (hpRegeneration.effectID != effectID) continue;

                hpRegenerationAfterEnemyDeathEffects.Remove(hpRegeneration);
                break;
            }
        }
    }

    public void ChangeSpeedModifier(float deltaValue)
    {
        speedModifier += deltaValue;
    }

    public void ReceiveExperience(int experience)
    {
        currentXp += (int)(experience * (1 + additionalExpMultiplayer));

        while (currentXp >= currentLevelXpNeeded)
        {
            currentXp -= currentLevelXpNeeded;
            currentLevelXpNeeded += (int)(currentLevelXpNeeded * experienceIncreaseForNextLevel);
            currentAvailableSkillPoint += 1;
            OnSkillPointsValueChange?.Invoke(this, EventArgs.Empty);
        }

        OnExperienceChange?.Invoke(this, new OnExperienceChangeEventArgs
        {
            currentXp = currentXp, maxXp = currentLevelXpNeeded
        });
    }

    public void ChangeExpAdditionalMultiplayer(float additionalValue)
    {
        additionalExpMultiplayer += additionalValue;
    }

    public void ReceiveCoins(int coins)
    {
        currentCoins += coins;
        OnCoinsValueChange?.Invoke(this, EventArgs.Empty);
    }

    public void ChangeCoinsPerKillMultiplayer(float deltaValue)
    {
        coinsOnEnemyDeathMultiplayer += deltaValue;
    }

    public void SpendCoins(int coins)
    {
        currentCoins -= coins;
        OnCoinsValueChange?.Invoke(this, EventArgs.Empty);
    }

    public void SpendSkillPoints(int toSpend)
    {
        currentAvailableSkillPoint -= toSpend;
        OnSkillPointsValueChange?.Invoke(this, EventArgs.Empty);
    }

    #region Attack & Weapons

    private void EnemyController_OnEnemyDeath(object sender, EnemyController.OnEnemyDeathEventArgs e)
    {
        var enemyController = sender as EnemyController;

        attackedNotDeadEnemies.Remove(enemyController);

        ReceiveCoins((int)(e.coinsValue * coinsOnEnemyDeathMultiplayer));
        ReceiveExperience(e.expValue);

        if (enemyController != null)
            enemyController.OnEnemyDeath -= EnemyController_OnEnemyDeath;

        if (hpRegenerationAfterEnemyDeathEffects.Count == 0) return;

        foreach (var hpRegeneration in hpRegenerationAfterEnemyDeathEffects)
        {
            playerHealth.RegenerateHealth(hpRegeneration.hpRegenerationAmount);

            OnPlayerRegenerateHpAfterEnemyDeath?.Invoke(this, new PlayerEffects.RelicBuffEffectTriggeredEventArgs
            {
                spentValue = 1, effectID = hpRegeneration.effectID
            });
        }
    }

    #endregion

    public bool IsEnoughCoins(int coins)
    {
        return currentCoins >= coins;
    }

    public bool IsEnoughSkillPoints(int skillPoints)
    {
        return currentAvailableSkillPoint >= skillPoints;
    }

    #region GetVariablesData

    public int GetCurrentCoinsValue()
    {
        return currentCoins;
    }

    public int GetExperienceForCurrentLevel()
    {
        return currentLevelXpNeeded;
    }

    public int GetCurrentSkillPointsValue()
    {
        return currentAvailableSkillPoint;
    }

    public int GetBaseHp()
    {
        return playerHealth.GetBaseHp();
    }

    public int GetBaseAttack()
    {
        return playerAttackController.GetBaseAttack();
    }

    public int GetBaseDefence()
    {
        return playerHealth.GetBaseDefence();
    }

    public int GetCurrentMaxHp()
    {
        return playerHealth.GetCurrentMaxHp();
    }

    public int GetCurrentAttack()
    {
        return playerAttackController.GetCurrentAttack();
    }

    public int GetCurrentDefence()
    {
        return playerHealth.GetCurrentDefence();
    }

    public int GetCurrentCritRate()
    {
        return playerAttackController.GetCurrentCritRate();
    }

    public int GetCurrentCritDmg()
    {
        return playerAttackController.GetCurrentCritDmg();
    }

    public int GetCurrentNaDmgBonus()
    {
        return playerAttackController.GetCurrentNaDmgBonus();
    }

    public int GetCurrentCaDmgBonus()
    {
        return playerAttackController.GetCurrentCaDmgBonus();
    }

    public int GetCurrentAdditionalHp()
    {
        return playerHealth.GetCurrentMaxHp() - playerHealth.GetBaseHp();
    }

    public int GetCurrentAdditionalAttack()
    {
        return playerAttackController.GetCurrentAttack() - playerAttackController.GetBaseAttack();
    }

    public int GetCurrentAdditionalDefence()
    {
        return playerHealth.GetCurrentDefence() - playerHealth.GetBaseDefence();
    }

    public PlayerEffects GetPlayerEffects()
    {
        return playerEffects;
    }

    public IInventoryParent GetPlayerInventory()
    {
        return playerInventory;
    }

    public IInventoryParent GetPlayerAttackInventory()
    {
        return playerWeapons;
    }

    public IInventoryParent GetPlayerRelicsInventory()
    {
        return playerRelics;
    }

    #endregion
}
