using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerAttackController))]
[RequireComponent(typeof(PlayerHealthController))]
[RequireComponent(typeof(StaminaController))]
[RequireComponent(typeof(PlayerEffectsController))]
[RequireComponent(typeof(PlayerInventory))]
[RequireComponent(typeof(PlayerRelics))]
[RequireComponent(typeof(PlayerWeapons))]
public class PlayerController : NetworkBehaviour
{
    #region Events & Event Args

    public static event EventHandler OnPlayerSpawned;

    public event EventHandler<OnExperienceChangeEventArgs> OnExperienceChange;

    public class OnExperienceChangeEventArgs : EventArgs
    {
        public int currentLevelExp;
        public int currentSkillPointExp;
        public int neededLevelExp;
        public int neededSkillPointExp;
    }

    public event EventHandler OnStopSprinting;
    public event EventHandler OnCoinsValueChange;
    public event EventHandler OnSkillPointsValueChange;
    public event EventHandler<OnNewLevelReachedEventArgs> OnNewLevelReached;

    public class OnNewLevelReachedEventArgs : EventArgs
    {
        public float baseHealthPercentageIncrease;
        public float baseAtkPercentageIncrease;
        public float baseDefencePercentageIncrease;
    }

    public event EventHandler<OnNewUpgradeBoughtEventArgs> OnNewUpgradeBought;

    public class OnNewUpgradeBoughtEventArgs : EventArgs
    {
        public int boughtUpgradeID;
        public List<PlayerEffectsController.AppliedEffect> upgradeEffects;
    }


    public event EventHandler<PlayerEffectsController.RelicBuffEffectTriggeredEventArgs>
        OnPlayerRegenerateHpAfterEnemyDeath;

    #endregion

    #region Created Classes

    private class HpRegenerationAfterEnemyDeath
    {
        public float hpRegenerationAmount;
        public int effectID;
    }

    #endregion

    public static PlayerController Instance { get; private set; }

    private string playerName;

    #region Experience & Skill Points

    [SerializeField] private int experienceForFirstLevel = 1000;
    [SerializeField] private int experienceForSkillPoint = 250;
    [SerializeField] private float experienceIncreaseForNextLevel = 0.15f;
    private int currentSkillPointExperienceNeeded;
    private int currentSkillPointExperience;
    private int currentLevelExperienceNeeded;
    private int currentLevelExperience;
    private float additionalExperienceMultiplayer;
    [SerializeField] private Sprite experienceIconSprite;
    [SerializeField] private TextTranslationsSO experienceTextTranslationsSo;
    private int currentAvailableSkillPoint;
    private readonly List<int> currentBoughtUpgradesIDs = new();

    private int currentCoins;

    [SerializeField] private Sprite coinsIconSprite;
    [SerializeField] private TextTranslationsSO coinsTextTranslationsSo;
    private float coinsOnEnemyDeathMultiplayer = 1f;

    #endregion

    #region Level stats increase values

    [SerializeField] private float hpIncreasePercentagePerLevel = 0.15f;
    [SerializeField] private float atkIncreasePercentagePerLevel = 0.1f;
    [SerializeField] private float defIncreasePercentagePerLevel = 0.125f;

    #endregion

    #region Movement & Stamina

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

    #endregion

    #region Effects

    private readonly List<EnemyController> attackedNotDeadEnemies = new();

    private readonly List<HpRegenerationAfterEnemyDeath> hpRegenerationAfterEnemyDeathEffects = new();

    #endregion

    #region References & Other

    [SerializeField] private Transform playerVisual;
    private PlayerMovement playerMovement;
    private PlayerAttackController playerAttackController;
    private PlayerHealthController playerHealthController;
    private StaminaController staminaController;
    private PlayerEffectsController playerEffectsController;
    private PlayerInventory playerInventory;
    private PlayerRelics playerRelics;
    private PlayerWeapons playerWeapons;
    private CameraController cameraController;
    private NetworkObject networkObject;

    private bool isFirstUpdate;

    #endregion

    #region Initialize & Subscribed Events

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        playerMovement = GetComponent<PlayerMovement>();
        playerAttackController = GetComponent<PlayerAttackController>();
        playerHealthController = GetComponent<PlayerHealthController>();
        staminaController = GetComponent<StaminaController>();
        playerEffectsController = GetComponent<PlayerEffectsController>();
        playerInventory = GetComponent<PlayerInventory>();
        playerRelics = GetComponent<PlayerRelics>();
        playerWeapons = GetComponent<PlayerWeapons>();
        cameraController = CameraController.Instance;
        networkObject = GetComponent<NetworkObject>();

        currentLevelExperienceNeeded = experienceForFirstLevel;
        currentSkillPointExperienceNeeded = experienceForSkillPoint;

        timerForConstantSprint = timeForConstantSprint;
        waitingForStaminaRegenerationTimer = waitingForStaminaRegenerationTime;

        isFirstUpdate = true;


        if (!IsOwner) return;

        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

        OnPlayerSpawned?.Invoke(this, EventArgs.Empty);

        CameraController.Instance.ChangeFollowingObject(playerVisual);
        MinimapCameraController.Instance.ChangeFollowingObject(transform);

        GameInput.Instance.OnChangeCameraModeAction += GameInput_OnChangeCameraModeAction;
        GameInput.Instance.OnSprintAction += GameInput_OnSprintAction;
        GameInput.Instance.OnChangeMovementModeAction += GameInput_OnChangeMovementMode;

        staminaController.OnAllStaminaSpend += StaminaController_OnAllStaminaSpend;
    }

    private void UpgradesTabUI_OnNewUpgradeBought(object sender, UpgradesTabUI.OnNewUpgradeBoughtEventArgs e)
    {
        OnNewUpgradeBoughtServerRpc(e.boughtUpgradeID);
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnNewUpgradeBoughtServerRpc(int boughtUpgradeID)
    {
        SpendSkillPoints(1);
        var upgradeApplyingEffects = UpgradesTabUI.Instance.GetUpgradeApplyingEffectsByUpgradeID(boughtUpgradeID);

        OnNewUpgradeBought?.Invoke(this, new OnNewUpgradeBoughtEventArgs
        {
            boughtUpgradeID = boughtUpgradeID,
            upgradeEffects = upgradeApplyingEffects
        });

        currentBoughtUpgradesIDs.Add(boughtUpgradeID);

        OnNewUpgradeBoughtClientRpc(boughtUpgradeID);
    }

    [ClientRpc]
    private void OnNewUpgradeBoughtClientRpc(int boughtUpgradeID)
    {
        if (IsServer) return;
        if (!IsOwner) return;

        OnNewUpgradeBought?.Invoke(this, new OnNewUpgradeBoughtEventArgs
        {
            boughtUpgradeID = boughtUpgradeID
        });
    }

    [ClientRpc]
    private void SetStoredDataClientRpc(int storedCurrentLevelExperienceNeeded,
        int storedCurrentSkillPointExperience,
        int storedCurrentExperience, int storedCurrentAvailableSkillPoint, int storedCurrentCoins,
        int storedCurrentHealth, int[] boughtUpgradesID)
    {
        currentSkillPointExperience = storedCurrentSkillPointExperience;
        currentLevelExperienceNeeded = storedCurrentLevelExperienceNeeded;
        currentLevelExperience = storedCurrentExperience;
        currentAvailableSkillPoint = storedCurrentAvailableSkillPoint;
        currentCoins = storedCurrentCoins;

        if (playerHealthController.GetCurrentHealth() != storedCurrentHealth)
        {
            var deltaHealth = playerHealthController.GetCurrentHealth() - storedCurrentHealth;
            playerHealthController.TakePureDamage(deltaHealth);
        }

        foreach (var boughtUpgradeID in boughtUpgradesID)
        {
            var upgradeApplyingEffects =
                UpgradesTabUI.Instance.GetUpgradeApplyingEffectsByUpgradeID(boughtUpgradeID);

            OnNewUpgradeBought?.Invoke(this, new OnNewUpgradeBoughtEventArgs
            {
                upgradeEffects = upgradeApplyingEffects,
                boughtUpgradeID = boughtUpgradeID
            });
        }
    }

    private void PlayerAttackController_OnEnemyHitted(object sender, PlayerAttackController.OnEnemyHittedEventArgs e)
    {
        if (!attackedNotDeadEnemies.Contains(e.hittedEnemy))
        {
            attackedNotDeadEnemies.Add(e.hittedEnemy);
            e.hittedEnemy.OnEnemyDeath += EnemyController_OnEnemyDeath;
        }
    }

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

    private void TryGetDataFromPreviousLevel()
    {
        if (IsServer)
        {
            var currentStoredData = StoredPlayerData.GetPersonStoredData(GetPlayerOwnerID());

            if (currentStoredData != null)
            {
                var storedEquippedRelicsCount = currentStoredData.relicsInventory.Length;

                while (storedEquippedRelicsCount > 0)
                    for (var i = 0; i < playerRelics.GetMaxSlotsCount(); i++)
                    {
                        if (currentStoredData.relicsInventory[i] == null) continue;

                        var currentStoredRelicInventoryObjectTransform =
                            Instantiate(currentStoredData.relicsInventory[i]);
                        var currentStoredRelicNetworkObject =
                            currentStoredRelicInventoryObjectTransform.GetComponent<NetworkObject>();
                        currentStoredRelicNetworkObject.Spawn();

                        var currentStoredRelicInventoryObject =
                            currentStoredRelicInventoryObjectTransform.GetComponent<InventoryObject>();
                        var currentPlayerNetworkObjectReference = new NetworkObjectReference(GetPlayerNetworkObject());
                        currentStoredRelicInventoryObject.SetInventoryParentBySlot(currentPlayerNetworkObjectReference,
                            (int)CharacterInventoryUI.InventoryType.PlayerRelicsInventory, i);
                        currentStoredData.relicsInventory[i] = null;
                        storedEquippedRelicsCount--;
                    }

                for (var i = 0; i < playerInventory.GetMaxSlotsCount(); i++)
                {
                    if (currentStoredData.playerInventory[i] == null) continue;

                    var currentStoredRelicInventoryObjectTransform =
                        Instantiate(currentStoredData.playerInventory[i]);
                    var currentStoredRelicNetworkObject =
                        currentStoredRelicInventoryObjectTransform.GetComponent<NetworkObject>();
                    currentStoredRelicNetworkObject.Spawn();

                    var currentStoredRelicInventoryObject =
                        currentStoredRelicInventoryObjectTransform.GetComponent<InventoryObject>();
                    var currentPlayerNetworkObjectReference = new NetworkObjectReference(GetPlayerNetworkObject());
                    currentStoredRelicInventoryObject.SetInventoryParentBySlot(currentPlayerNetworkObjectReference,
                        (int)CharacterInventoryUI.InventoryType.PlayerInventory, i);
                    currentStoredData.playerInventory[i] = null;
                }

                for (var i = 0; i < playerWeapons.GetMaxSlotsCount(); i++)
                {
                    if (currentStoredData.weaponsInventory[i] == null) continue;

                    var currentStoredRelicInventoryObjectTransform =
                        Instantiate(currentStoredData.weaponsInventory[i]);
                    var currentStoredRelicNetworkObject =
                        currentStoredRelicInventoryObjectTransform.GetComponent<NetworkObject>();
                    currentStoredRelicNetworkObject.Spawn();

                    var currentStoredRelicInventoryObject =
                        currentStoredRelicInventoryObjectTransform.GetComponent<InventoryObject>();
                    var currentPlayerNetworkObjectReference = new NetworkObjectReference(GetPlayerNetworkObject());
                    currentStoredRelicInventoryObject.SetInventoryParentBySlot(currentPlayerNetworkObjectReference,
                        (int)CharacterInventoryUI.InventoryType.PlayerWeaponInventory, i);
                    currentStoredData.weaponsInventory[i] = null;
                }

                var boughtUpgradesID = new int[currentStoredData.boughtUpgradesID.Count];
                for (var i = 0; i < currentStoredData.boughtUpgradesID.Count; i++)
                    boughtUpgradesID[i] = currentStoredData.boughtUpgradesID[i];

                SetStoredDataClientRpc(currentStoredData.currentLevelExperienceNeeded,
                    currentStoredData.currentSkillPointExperience,
                    currentStoredData.currentExperience, currentStoredData.currentAvailableSkillPoint,
                    currentStoredData.currentCoins, currentStoredData.currentHealth,
                    boughtUpgradesID);
            }
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

            SpawnPlayers.Instance.SetPlayerAsSpawned();

            if (IsServer)
            {
                TryGetDataFromPreviousLevel();

                playerAttackController.OnEnemyHitted += PlayerAttackController_OnEnemyHitted;
                AllConnectedPlayers.Instance.AddConnectedPlayerController(this);
            }

            if (!IsOwner) return;

            UpgradesTabUI.Instance.OnNewUpgradeBought += UpgradesTabUI_OnNewUpgradeBought;

            OnExperienceChange?.Invoke(this, new OnExperienceChangeEventArgs
            {
                currentLevelExp = currentLevelExperience, neededLevelExp = currentLevelExperienceNeeded,
                currentSkillPointExp = currentSkillPointExperience, neededSkillPointExp = experienceForSkillPoint
            });
            OnSkillPointsValueChange?.Invoke(this, EventArgs.Empty);
        }

        if (!IsOwner) return;

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
            playerMovement.Move(moveVector, true);

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

    #region Health & Connected Effects

    public void ChangeHealth(int healthChangeValue)
    {
        if (!IsServer) return;

        if (healthChangeValue > 0)
            playerHealthController.RegenerateHealth(healthChangeValue);
        else
            playerHealthController.TakeDamage(-healthChangeValue);
    }

    public void AddRegeneratingHpAfterEnemyDeath(float hpPercentageValue, int effectID)
    {
        AddRegeneratingHpAfterEnemyDeathServerRpc(hpPercentageValue, effectID);
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddRegeneratingHpAfterEnemyDeathServerRpc(float hpPercentageValue, int effectID)
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

    #endregion

    #region Speed & Connected Effects

    public void ChangeSpeedModifier(float deltaValue)
    {
        if (!IsServer) return;

        ChangeSpeedModifierServerRpc(deltaValue);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeSpeedModifierServerRpc(float deltaValue)
    {
        var newSpeedModifier = speedModifier + deltaValue;

        ChangeSpeedModifierClientRpc(newSpeedModifier);
    }

    [ClientRpc]
    private void ChangeSpeedModifierClientRpc(float newSpeedModifier)
    {
        speedModifier = newSpeedModifier;
    }

    #endregion

    #region Experience, Skill Points & Connected Effects

    public void ReceiveExperience(int experience)
    {
        if (!IsServer) return;

        ReceiveExperienceServerRpc(experience);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ReceiveExperienceServerRpc(int experience)
    {
        var addingExperience = (int)(experience * (1 + additionalExperienceMultiplayer));
        var newCurrentLevelExperience = currentLevelExperience + addingExperience;
        var newCurrentSkillPointExperience = currentSkillPointExperience + addingExperience;
        var newCurrentLevelExperienceNeeded = currentLevelExperienceNeeded;
        var newCurrentSkillPointExperienceNeeded = currentSkillPointExperienceNeeded;
        var newCurrentAvailableSkillPoints = currentAvailableSkillPoint;

        while (newCurrentLevelExperience >= newCurrentLevelExperienceNeeded)
        {
            newCurrentLevelExperience -= newCurrentLevelExperienceNeeded;
            newCurrentLevelExperienceNeeded += (int)(newCurrentLevelExperienceNeeded * experienceIncreaseForNextLevel);
            OnNewLevelReached?.Invoke(this, new OnNewLevelReachedEventArgs
            {
                baseAtkPercentageIncrease = atkIncreasePercentagePerLevel,
                baseDefencePercentageIncrease = defIncreasePercentagePerLevel,
                baseHealthPercentageIncrease = hpIncreasePercentagePerLevel
            });
        }

        while (newCurrentSkillPointExperience >= newCurrentSkillPointExperienceNeeded)
        {
            newCurrentSkillPointExperience -= newCurrentSkillPointExperienceNeeded;
            newCurrentAvailableSkillPoints += 1;
        }

        ReceiveExperienceClientRpc(newCurrentLevelExperience, newCurrentSkillPointExperience,
            newCurrentLevelExperienceNeeded, newCurrentSkillPointExperienceNeeded,
            newCurrentAvailableSkillPoints, experience);
    }

    [ClientRpc]
    private void ReceiveExperienceClientRpc(int newCurrentLevelExperience, int newCurrentSkillPointExperience,
        int newCurrentLevelExperienceNeeded, int newCurrentSkillPointExperienceNeeded,
        int newCurrentAvailableSkillPoints, int deltaExperience)
    {
        currentLevelExperience = newCurrentLevelExperience;
        currentSkillPointExperience = newCurrentSkillPointExperience;
        currentLevelExperienceNeeded = newCurrentLevelExperienceNeeded;
        currentSkillPointExperienceNeeded = newCurrentSkillPointExperienceNeeded;
        currentAvailableSkillPoint = newCurrentAvailableSkillPoints;

        if (!IsOwner) return;

        OnSkillPointsValueChange?.Invoke(this, EventArgs.Empty);
        OnExperienceChange?.Invoke(this, new OnExperienceChangeEventArgs
        {
            currentLevelExp = currentLevelExperience, neededLevelExp = currentLevelExperienceNeeded,
            currentSkillPointExp = currentSkillPointExperience, neededSkillPointExp = currentSkillPointExperienceNeeded
        });

        ReceivingItemsUI.Instance.AddReceivedItem(experienceIconSprite, experienceTextTranslationsSo,
            deltaExperience, 100);
    }

    public void ChangeExpAdditionalMultiplayer(float additionalValue)
    {
        if (!IsServer) return;

        ChangeExpAdditionalMultiplayerServerRpc(additionalValue);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeExpAdditionalMultiplayerServerRpc(float additionalValue)
    {
        var newAdditionalExperienceMultiplier = additionalExperienceMultiplayer + additionalValue;

        ChangeExpAdditionalMultiplayerClientRpc(newAdditionalExperienceMultiplier);
    }

    [ClientRpc]
    private void ChangeExpAdditionalMultiplayerClientRpc(float newAdditionalExperienceMultiplier)
    {
        additionalExperienceMultiplayer = newAdditionalExperienceMultiplier;
    }

    public void SpendSkillPoints(int deltaValue)
    {
        SpendSkillPointServerRpc(deltaValue);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpendSkillPointServerRpc(int deltaValue)
    {
        var newCurrentAvailableSkillPoint = currentAvailableSkillPoint - deltaValue;

        SpendSkillPointClientRpc(newCurrentAvailableSkillPoint);
    }

    [ClientRpc]
    private void SpendSkillPointClientRpc(int newCurrentAvailableSkillPoint)
    {
        currentAvailableSkillPoint = newCurrentAvailableSkillPoint;

        if (!IsOwner) return;

        OnSkillPointsValueChange?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region Coins & Connected Effects

    public void TransferCoinsTo(PlayerController transferToPlayer, int coinsToTransfer)
    {
        if (!IsOwner) return;

        var transferToPlayerNetworkObjectReference =
            new NetworkObjectReference(transferToPlayer.GetPlayerNetworkObject());
        TransferCoinsToServerRpc(transferToPlayerNetworkObjectReference, coinsToTransfer);
    }

    [ServerRpc(RequireOwnership = false)]
    private void TransferCoinsToServerRpc(NetworkObjectReference transferToPlayerNetworkObjectReference,
        int coinsToTransfer)
    {
        if (currentCoins < coinsToTransfer) return;

        transferToPlayerNetworkObjectReference.TryGet(out var transferToPlayerNetworkObject);
        var transferToPlayer = transferToPlayerNetworkObject.GetComponent<PlayerController>();

        SpendCoins(coinsToTransfer);
        transferToPlayer.ReceiveCoins(coinsToTransfer);
    }

    public void ReceiveCoins(int deltaCoins)
    {
        if (!IsServer) return;

        ReceiveCoinsServerRpc(deltaCoins);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ReceiveCoinsServerRpc(int deltaCoins)
    {
        var newCurrentCoinsValue = currentCoins + deltaCoins;

        ReceiveCoinsClientRpc(newCurrentCoinsValue, deltaCoins);
    }

    [ClientRpc]
    private void ReceiveCoinsClientRpc(int newCurrentCoinsValue, int deltaCoins)
    {
        currentCoins = newCurrentCoinsValue;

        if (!IsOwner) return;

        OnCoinsValueChange?.Invoke(this, EventArgs.Empty);

        ReceivingItemsUI.Instance.AddReceivedItem(coinsIconSprite, coinsTextTranslationsSo,
            deltaCoins, 99);
    }

    public void ChangeCoinsPerKillMultiplayer(float deltaValue)
    {
        if (!IsServer) return;

        ChangeCoinsPerKillMultiplayerServerRpc(deltaValue);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeCoinsPerKillMultiplayerServerRpc(float deltaValue)
    {
        var newCoinsOnEnemyDeathMultiplayer = coinsOnEnemyDeathMultiplayer + deltaValue;

        ChangeCoinsPerKillMultiplayerClientRpc(newCoinsOnEnemyDeathMultiplayer);
    }

    [ClientRpc]
    private void ChangeCoinsPerKillMultiplayerClientRpc(float newCoinsOnEnemyDeathMultiplayer)
    {
        coinsOnEnemyDeathMultiplayer = newCoinsOnEnemyDeathMultiplayer;
    }

    public void SpendCoins(int deltaCoins)
    {
        if (!IsServer) return;

        SpendCoinsServerRpc(deltaCoins);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpendCoinsServerRpc(int deltaCoins)
    {
        var newCurrentCoinsValue = currentCoins - deltaCoins;

        SpendCoinsClientRpc(newCurrentCoinsValue);
    }

    [ClientRpc]
    private void SpendCoinsClientRpc(int newCurrentCoinsValue)
    {
        currentCoins = newCurrentCoinsValue;

        if (!IsOwner) return;

        OnCoinsValueChange?.Invoke(this, EventArgs.Empty);
    }

    #endregion

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

        var hpRegenerationsToTrigger = new List<HpRegenerationAfterEnemyDeath>();
        hpRegenerationsToTrigger.AddRange(hpRegenerationAfterEnemyDeathEffects);

        foreach (var hpRegeneration in hpRegenerationsToTrigger)
        {
            playerHealthController.RegenerateHealth(hpRegeneration.hpRegenerationAmount);

            OnPlayerRegenerateHpAfterEnemyDeath?.Invoke(this,
                new PlayerEffectsController.RelicBuffEffectTriggeredEventArgs
                {
                    spentValue = 1, effectID = hpRegeneration.effectID
                });
        }
    }

    #endregion

    #region Check Enough Value

    public bool IsEnoughCoins(int coins)
    {
        return currentCoins >= coins;
    }

    public bool IsEnoughSkillPoints(int skillPoints)
    {
        return currentAvailableSkillPoint >= skillPoints;
    }

    #endregion

    #region GetVariablesData

    public ulong GetPlayerOwnerID()
    {
        return networkObject.OwnerClientId;
    }

    public int GetCurrentCoinsValue()
    {
        return currentCoins;
    }

    public int GetExperienceForCurrentLevel()
    {
        return currentLevelExperienceNeeded;
    }

    public int GetCurrentLevelExperience()
    {
        return currentLevelExperience;
    }

    public int GetExperienceForSkillPoint()
    {
        return currentSkillPointExperienceNeeded;
    }

    public int GetCurrentSkillPointExperience()
    {
        return currentSkillPointExperience;
    }

    public int GetCurrentSkillPointsValue()
    {
        return currentAvailableSkillPoint;
    }

    public List<int> GetCurrentBoughtUpgradeIDs()
    {
        return currentBoughtUpgradesIDs;
    }

    public int GetBaseHp()
    {
        return playerHealthController.GetBaseHp();
    }

    public int GetBaseAttack()
    {
        return playerAttackController.GetBaseAttack();
    }

    public int GetBaseDefence()
    {
        return playerHealthController.GetBaseDefence();
    }

    public int GetCurrentMaxHp()
    {
        return playerHealthController.GetCurrentMaxHp();
    }

    public int GetCurrentAttack()
    {
        return playerAttackController.GetCurrentAttack();
    }

    public int GetCurrentDefence()
    {
        return playerHealthController.GetCurrentDefence();
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
        return playerHealthController.GetCurrentMaxHp() - playerHealthController.GetBaseHp();
    }

    public int GetCurrentAdditionalAttack()
    {
        return playerAttackController.GetCurrentAttack() - playerAttackController.GetBaseAttack();
    }

    public int GetCurrentAdditionalDefence()
    {
        return playerHealthController.GetCurrentDefence() - playerHealthController.GetBaseDefence();
    }

    public PlayerEffectsController GetPlayerEffects()
    {
        return playerEffectsController;
    }

    public IInventoryParent GetPlayerInventory()
    {
        return playerInventory;
    }

    public IInventoryParent GetPlayerWeaponsInventory()
    {
        return playerWeapons;
    }

    public IInventoryParent GetPlayerRelicsInventory()
    {
        return playerRelics;
    }

    public NetworkObject GetPlayerNetworkObject()
    {
        return networkObject;
    }

    public PlayerAttackController GetPlayerAttackController()
    {
        return playerAttackController;
    }

    public StaminaController GetPlayerStaminaController()
    {
        return staminaController;
    }

    public PlayerHealthController GetPlayerHealthController()
    {
        return playerHealthController;
    }

    #endregion

    public static void ResetStaticData()
    {
        OnPlayerSpawned = null;
    }
}
