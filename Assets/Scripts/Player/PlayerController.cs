using System;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerAttackController))]
[RequireComponent(typeof(PlayerHealth))]
[RequireComponent(typeof(StaminaController))]
[RequireComponent(typeof(PlayerEffects))]
[RequireComponent(typeof(PlayerInventory))]
[RequireComponent(typeof(PlayerRelics))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    public event EventHandler<OnExperienceChangeEventArgs> OnExperienceChange;
    public event EventHandler OnCoinsValueChange;
    public event EventHandler OnSkillPointsValueChange;

    public class OnExperienceChangeEventArgs : EventArgs
    {
        public int currentXp;
        public int maxXp;
    }

    [SerializeField] private int experienceForFirstLevel = 100;
    [SerializeField] private float experienceIncreaseForNextLevel = 0.2f;
    private int currentLevelXpNeeded;
    private int currentXp;
    private float additionalExpMultiplayer;

    private int currentAvailableSkillPoint;
    private int currentCoins;

    [SerializeField] private float walkSpeed = 1f;
    [SerializeField] private float runSpeed = 3f;
    [SerializeField] private float sprintSpeed = 5f;
    [SerializeField] private float staminaSprintCostPerSecond = 7.5f;
    private bool isRunning = true;
    private bool isSprinting;
    private readonly float timeForConstantSprint = 2f;
    private float timerForConstantSprint;

    [SerializeField] private float waitingForStaminaRegenerationTime = 2.5f;
    private float waitingForStaminaRegenerationTimer;

    private bool isTryingToChargedAttack;
    [SerializeField] private float chargedAttackPressTime = 1f;
    private float chargedAttackPressTimer;

    [SerializeField] private CameraController cameraController;
    private PlayerMovement playerMovement;
    private PlayerAttackController playerAttackController;
    private PlayerHealth playerHealth;
    private StaminaController staminaController;
    private PlayerEffects playerEffects;
    private PlayerInventory playerInventory;
    private PlayerRelics playerRelics;

    private bool isFirstUpdate = true;

    private void Awake()
    {
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

        timerForConstantSprint = timeForConstantSprint;
        waitingForStaminaRegenerationTimer = waitingForStaminaRegenerationTime;

        currentLevelXpNeeded = experienceForFirstLevel;
    }

    private void Start()
    {
        GameInput.Instance.OnChangeCameraModeAction += GameInput_OnChangeCameraModeAction;
        GameInput.Instance.OnSprintAction += GameInput_OnSprintAction;
        GameInput.Instance.OnChangeMovementModeAction += GameInput_OnChangeMovementMode;
        GameInput.Instance.OnAttackAction += GameInput_OnAttackAction;
        GameInput.Instance.OnChangeCurrentWeaponAction += GameInput_OnChangeCurrentWeaponAction;
        GameInput.Instance.OnDropWeaponAction += GameInput_OnDropWeaponAction;

        staminaController.OnAllStaminaSpend += StaminaController_OnAllStaminaSpend;
    }

    #region SubscribedEvents

    private void StaminaController_OnAllStaminaSpend(object sender, EventArgs e)
    {
        isSprinting = false;
    }

    private void GameInput_OnDropWeaponAction(object sender, EventArgs e)
    {
        playerAttackController.TryDropCurrentWeapon();
    }

    private void GameInput_OnChangeCurrentWeaponAction(object sender, EventArgs e)
    {
        playerAttackController.TryChangeToNextWeapon();
    }

    private void GameInput_OnAttackAction(object sender, EventArgs e)
    {
        NormalAttack();
        isTryingToChargedAttack = true;
    }

    private void GameInput_OnChangeMovementMode(object sender, EventArgs e)
    {
        isRunning = !isRunning;
    }

    private void GameInput_OnSprintAction(object sender, EventArgs e)
    {
        if (staminaController.IsHaveAvailableStamina())
            isSprinting = true;

        waitingForStaminaRegenerationTimer = waitingForStaminaRegenerationTime;
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
        if (isFirstUpdate)
        {
            isFirstUpdate = false;

            OnExperienceChange?.Invoke(this, new OnExperienceChangeEventArgs
            {
                currentXp = currentXp, maxXp = currentLevelXpNeeded
            });
        }

        TryMove(out var isMoving);
        TryChargedAttack();
        TryStartStaminaRegeneration();
    }

    private void TryMove(out bool isMoving)
    {
        isMoving = false;
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
                    }
            }
            else
            {
                if (isRunning)
                    moveVector *= Time.deltaTime * runSpeed;
                else
                    moveVector *= Time.deltaTime * walkSpeed;
            }

            isMoving = true;

            playerMovement.Move(moveVector);

            return;
        }

        if (timerForConstantSprint != timeForConstantSprint)
            timerForConstantSprint = timeForConstantSprint;

        if (isSprinting)
            isSprinting = false;
    }

    private void TryChargedAttack()
    {
        if (isTryingToChargedAttack)
        {
            if (GameInput.Instance.GetBindingValue(GameInput.Binding.Attack) == 1f)
            {
                chargedAttackPressTimer -= Time.deltaTime;
                if (chargedAttackPressTimer <= 0)
                {
                    playerAttackController.GetCurrentInventoryObjectWeapon().TryGetWeaponSo(out var currentWeaponSo);
                    var chargedAttackStaminaCost = currentWeaponSo.chargedAttackStaminaCost;
                    if (staminaController.IsHaveNeededStamina(chargedAttackStaminaCost))
                    {
                        ChargedAttack();

                        staminaController.SpendStamina(chargedAttackStaminaCost);

                        waitingForStaminaRegenerationTimer = waitingForStaminaRegenerationTime;
                    }
                }
            }
            else
            {
                isTryingToChargedAttack = false;
            }
        }
        else
        {
            if (chargedAttackPressTimer != chargedAttackPressTime)
                chargedAttackPressTimer = chargedAttackPressTime;
        }
    }

    private void TryStartStaminaRegeneration()
    {
        if (!staminaController.IsRegeneratingStamina() && !staminaController.IsStaminaFull()
                                                       && !isSprinting)
        {
            waitingForStaminaRegenerationTimer -= Time.deltaTime;

            if (waitingForStaminaRegenerationTimer <= 0) staminaController.StartStaminaRegeneration();
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

    public void ReceiveExperience(int experience)
    {
        currentXp += (int)(experience * additionalExpMultiplayer);

        if (currentXp >= currentLevelXpNeeded)
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

    private void NormalAttack()
    {
        playerAttackController.NormalAttack();
    }

    private void ChargedAttack()
    {
        isTryingToChargedAttack = false;
        chargedAttackPressTimer = chargedAttackPressTime;

        playerAttackController.ChargeAttack();
    }

    private void ChangeWeapon(InventoryObject weaponInventoryObject)
    {
        playerAttackController.TryChangeWeapon(weaponInventoryObject);
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

    public int GetMaxOwnedWeaponsCount()
    {
        return playerAttackController.GetMaxOwnedWeaponsCount();
    }

    public PlayerEffects GetPlayerEffects()
    {
        return playerEffects;
    }

    public PlayerInventory GetPlayerInventory()
    {
        return playerInventory;
    }

    public IInventoryParent GetPlayerAttackInventory()
    {
        return playerAttackController;
    }

    public IInventoryParent GetPlayerRelicsInventory()
    {
        return playerRelics;
    }

    #endregion
}
