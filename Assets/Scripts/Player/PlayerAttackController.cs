using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
public class PlayerAttackController : NetworkBehaviour
{
    #region Events & Event Args

    public event EventHandler OnChargeAttackStopCharging;
    public event EventHandler OnGunChargedAttackTriggered;
    public event EventHandler<OnEnemyHittedEventArgs> OnEnemyHitted;

    public class OnEnemyHittedEventArgs : EventArgs
    {
        public EnemyController hittedEnemy;
    }

    #endregion

    #region Created Classes

    private class SlowEnemyOnHit
    {
        public float slowDuration;
        public float speedDecrease;
        public float effectChance;
        public int effectId;
    }

    private class CritRateIncreaseOnHit
    {
        public float critRateIncreasePerHit;
        public float currentCritRateIncrease;
        public float maxCritRateIncrease;
        public int effectId;
    }

    #endregion

    #region GeneralStats

    [SerializeField] private int baseAttack = 100;
    private int currentAttack;

    private float normalAttackDamageBonus;
    private float chargedAttackDamageBonus;

    [SerializeField] private float baseCritRate = 0.05f;
    [SerializeField] private float baseCritDamage = 0.5f;
    private float currentCritRate;
    private float currentCritDamage;

    #endregion

    #region Effects

    private readonly List<SlowEnemyOnHit> slowEnemyOnHits = new();
    private readonly List<CritRateIncreaseOnHit> critRateIncreaseOnHitBuffs = new();

    #endregion

    #region Combo Attacks

    private int currentAttackCombo = -1;

    [SerializeField] private float comboAttackResetTime = 3f;
    private float comboAttackResetTimer;

    private bool isTryingToChargedAttack;
    private bool isAiming;
    [SerializeField] private float chargedAttackPressTime = 1f;
    private float chargedAttackPressTimer;

    #endregion

    #region Weapons Ranges

    [SerializeField] private float meleeWeaponAttackRange = 0.25f;
    [SerializeField] private Transform weaponCastTransform;
    [SerializeField] private float maxRangeToFindEnemies = 5f;
    [SerializeField] private float maxMoveDistanceToFoundEnemy = 0.25f;
    [SerializeField] private float distanceToMoveIfNotFoundEnemies = 0.05f;

    [SerializeField] private float rangedWeaponAttackRange = 10f;
    [SerializeField] private Transform bulletPrefab;
    [SerializeField] private float bulletLifetime = 3f;
    [SerializeField] private Transform bulletSpawnTransform;
    [SerializeField] private LayerMask enemiesLayer;

    #endregion

    #region References & Other

    private bool isAnyInterfaceOpened;
    private bool isCursorShown;

    private PlayerWeapons playerWeapons;
    private PlayerController playerController;
    private PlayerMovement playerMovement;
    private StaminaController staminaController;
    private Animator animator;

    private static readonly int CurrentNormalAttackCombo = Animator.StringToHash("CurrentNormalAttackCombo");
    private static readonly int IsChargedAttack = Animator.StringToHash("IsChargedAttack");
    private static readonly int WeaponType = Animator.StringToHash("WeaponType");

    private WeaponSO chosenAttackWeapon;
    private bool isCurrentlyAttacking;
    [SerializeField] private List<EnemyController> enemiesWaitingToAttack = new();
    private bool isCurrentAttackNormal;

    #endregion

    #region Initialization & Subscribed events

    private void Awake()
    {
        playerWeapons = GetComponent<PlayerWeapons>();
        playerController = GetComponent<PlayerController>();
        playerMovement = GetComponent<PlayerMovement>();
        staminaController = GetComponent<StaminaController>();
        animator = GetComponent<Animator>();

        currentAttack = baseAttack;

        currentCritRate = baseCritRate;
        currentCritDamage = baseCritDamage;

        comboAttackResetTimer = comboAttackResetTime;
    }

    private void Start()
    {
        if (!IsOwner) return;

        GameInput.Instance.OnAttackAction += GameInput_OnAttackAction;

        GameInput.Instance.OnCursorShowAction += GameInput_OnCursorShowAction;

        GiveCoinsUI.OnInterfaceShown += OnAnyTabOpen;
        GiveCoinsUI.OnInterfaceHidden += OnAnyTabClose;

        PauseUI.OnInterfaceShown += OnAnyTabOpen;
        PauseUI.OnInterfaceHidden += OnAnyTabClose;

        ShopUI.Instance.OnShopOpen += OnAnyTabOpen;
        ShopUI.Instance.OnShopClose += OnAnyTabClose;

        CharacterUI.OnCharacterUIOpen += OnAnyTabOpen;
        CharacterUI.OnCharacterUIClose += OnAnyTabClose;
    }

    private void GameInput_OnCursorShowAction(object sender, EventArgs e)
    {
        isCursorShown = true;
    }

    private void OnAnyTabClose(object sender, EventArgs e)
    {
        isAnyInterfaceOpened = false;
    }

    private void OnAnyTabOpen(object sender, EventArgs e)
    {
        isAnyInterfaceOpened = true;
    }

    private void GameInput_OnAttackAction(object sender, EventArgs e)
    {
        if (isAnyInterfaceOpened) return;
        if (isCursorShown) return;
        if (isCurrentlyAttacking) return;

        NormalAttack();
        isTryingToChargedAttack = true;
    }

    #endregion

    #region Update & Connected

    private void Update()
    {
        if (GameStageManager.Instance.IsPause()) return;
        if (isAnyInterfaceOpened) return;

        if (isCursorShown)
        {
            if (GameInput.Instance.GetBindingValue(GameInput.Binding.ShowCursor) == 1f)
                return;

            isCursorShown = false;
        }

        TryChargedAttack();

        if (isAiming)
            if (GameInput.Instance.GetBindingValue(GameInput.Binding.Attack) != 1f)
                GunFinishAiming();

        if (!IsServer) return;

        if (currentAttackCombo != -1)
        {
            comboAttackResetTimer -= Time.deltaTime;
            if (comboAttackResetTimer <= 0) ResetNormalAttackCombo();
        }
    }

    private void TryChargedAttack()
    {
        if (isAiming) return;

        if (isTryingToChargedAttack)
        {
            if (GameInput.Instance.GetBindingValue(GameInput.Binding.Attack) == 1f)
            {
                chargedAttackPressTimer -= Time.deltaTime;
                if (chargedAttackPressTimer <= 0)
                {
                    isTryingToChargedAttack = false;
                    OnChargeAttackStopCharging?.Invoke(this, EventArgs.Empty);
                    var currentWeaponSo = playerWeapons.GetCurrentWeaponSo();
                    var chargedAttackStaminaCost = currentWeaponSo.chargedAttackStaminaCost;
                    if (staminaController.IsHaveNeededStamina(chargedAttackStaminaCost)) ChargeAttack();
                }
            }
            else
            {
                isTryingToChargedAttack = false;
                OnChargeAttackStopCharging?.Invoke(this, EventArgs.Empty);
            }
        }
        else
        {
            chargedAttackPressTimer = chargedAttackPressTime;
        }
    }

    private void GunFinishAiming()
    {
        isAiming = false;
        GunFinishAimingServerRpc();

        OnGunChargedAttackTriggered?.Invoke(this, EventArgs.Empty);
    }

    [ServerRpc(RequireOwnership = false)]
    private void GunFinishAimingServerRpc()
    {
        var currentWeaponSo = playerWeapons.GetCurrentWeaponSo();

        var bulletAngle = transform.localEulerAngles.y - 90;
        var chargeAttackDamage = CalculateDamage(currentAttack,
            currentWeaponSo.chargedAttackDamageScale,
            chargedAttackDamageBonus, currentCritRate, currentCritDamage);

        staminaController.SpendStamina(currentWeaponSo.chargedAttackStaminaCost);

        var bulletTransform = Instantiate(bulletPrefab, bulletSpawnTransform.position, Quaternion.identity);
        var weaponBullet = bulletTransform.GetComponent<WeaponBullet>();
        weaponBullet.GetComponent<NetworkObject>().Spawn();
        weaponBullet.InitializeBullet(bulletAngle, bulletLifetime, chargeAttackDamage,
            playerController, enemiesLayer);
        weaponBullet.OnEnemyHitted += WeaponBullet_OnEnemyHitted;
    }

    #endregion

    #region Animator Methods

    public void FindEnemiesToAttackAnimator()
    {
        if (!isCurrentlyAttacking) return;

        var enemiesToAttack = FindEnemiesToAttack();

        foreach (var enemyToAttack in enemiesToAttack)
        {
            if (enemiesWaitingToAttack.Contains(enemyToAttack)) continue;

            enemiesWaitingToAttack.Add(enemyToAttack);
        }
    }

    public void FinishAttackAnimator()
    {
        if (!IsServer) return;

        isCurrentlyAttacking = false;

        switch (chosenAttackWeapon.weaponType)
        {
            case WeaponSO.WeaponType.Katana:
                if (isCurrentAttackNormal)
                    KatanaNormalAttack();
                else
                    KatanaChargedAttack();
                break;
            case WeaponSO.WeaponType.Gun:
                if (isCurrentAttackNormal)
                {
                    GunNormalAttack();
                }
                else
                {
                    isAiming = true;
                    OnGunChargedAttackTriggered?.Invoke(this, EventArgs.Empty);
                }

                break;
        }

        comboAttackResetTimer = comboAttackResetTime;

        animator.SetInteger(CurrentNormalAttackCombo, -1);
        animator.SetBool(IsChargedAttack, false);
    }

    #endregion

    #region Find Enemies

    private List<EnemyController> FindEnemiesToAttack()
    {
        var castPosition = weaponCastTransform.position;
        var castCubeLength = new Vector3(meleeWeaponAttackRange, meleeWeaponAttackRange, meleeWeaponAttackRange);

        var raycastHits = Physics.BoxCastAll(castPosition, castCubeLength, Vector3.forward,
            Quaternion.identity, meleeWeaponAttackRange, enemiesLayer);

        List<EnemyController> enemiesToAttack = new();

        foreach (var hit in raycastHits)
            if (hit.transform.gameObject.TryGetComponent(out EnemyController enemyController))
            {
                enemiesToAttack.Add(enemyController);

                OnEnemyHitted?.Invoke(this, new OnEnemyHittedEventArgs
                {
                    hittedEnemy = enemyController
                });
            }

        return enemiesToAttack;
    }

    private Vector3 FindNearestEnemyPositionForRangeAttack()
    {
        var castPosition = bulletSpawnTransform.position;
        var castCubeLength = new Vector3(rangedWeaponAttackRange * 2, rangedWeaponAttackRange * 2,
            rangedWeaponAttackRange * 2);

        var raycastHits = Physics.BoxCastAll(castPosition, castCubeLength, Vector3.forward,
            Quaternion.identity, meleeWeaponAttackRange, enemiesLayer);

        var closestEnemyRange = rangedWeaponAttackRange;
        var closestEnemyTransform = bulletSpawnTransform;

        foreach (var hit in raycastHits)
            if (hit.transform.gameObject.TryGetComponent(out EnemyController _))
                if (closestEnemyRange < rangedWeaponAttackRange || closestEnemyTransform == bulletSpawnTransform)
                {
                    closestEnemyTransform = hit.transform;
                    closestEnemyRange = hit.distance;
                }

        return closestEnemyTransform.position;
    }

    private void TryFindTargetToAttack(out Vector2 additionalMoveToTargetVector,
        out Vector3 newPlayerForwardToFaceEnemy)
    {
        var playerTransform = transform;
        additionalMoveToTargetVector = Vector2.zero;
        newPlayerForwardToFaceEnemy = playerTransform.forward;

        var castPosition = playerTransform.position;
        var castCubeLength = new Vector3(maxRangeToFindEnemies, maxRangeToFindEnemies, maxRangeToFindEnemies);

        var raycastHits = Physics.BoxCastAll(castPosition, castCubeLength, Vector3.forward,
            Quaternion.identity, maxRangeToFindEnemies, enemiesLayer);

        var minDistanceToEnemy = maxRangeToFindEnemies;
        var foundEnemyPosition = Vector3.zero;

        foreach (var hit in raycastHits)
            if (hit.transform.gameObject.TryGetComponent(out EnemyController _))
            {
                if (hit.distance > minDistanceToEnemy) continue;

                var hitTransformPosition = hit.transform.position;
                minDistanceToEnemy = (transform.position - hitTransformPosition).magnitude;
                foundEnemyPosition = hitTransformPosition;
            }

        if (foundEnemyPosition == Vector3.zero)
        {
            additionalMoveToTargetVector = new Vector2(0f, distanceToMoveIfNotFoundEnemies);
        }
        else
        {
            if (minDistanceToEnemy < maxMoveDistanceToFoundEnemy) return;

            var enemyDirectionNormalized = -(transform.position - foundEnemyPosition).normalized;
            newPlayerForwardToFaceEnemy = enemyDirectionNormalized;

            var additionalMoveToTargetVector3 = enemyDirectionNormalized * maxMoveDistanceToFoundEnemy;
            additionalMoveToTargetVector =
                new Vector2(additionalMoveToTargetVector3.x, additionalMoveToTargetVector3.z);
        }
    }

    #endregion

    #region Normal Attack

    private void NormalAttack()
    {
        NormalAttackServerRpc();
    }

    [ServerRpc]
    private void NormalAttackServerRpc()
    {
        chosenAttackWeapon = playerWeapons.GetCurrentWeaponSo();
        if (chosenAttackWeapon == null) return;

        var playerTransform = transform;
        currentAttackCombo++;
        if (currentAttackCombo >= chosenAttackWeapon.comboAttack)
            currentAttackCombo = 0;

        TryFindTargetToAttack(out var vectorToMoveToEnemy, out var newPlayerForwardToFaceEnemy);
        playerMovement.Move(vectorToMoveToEnemy, false);

        playerTransform.forward = newPlayerForwardToFaceEnemy;
        playerTransform.localEulerAngles = new Vector3(0f, playerTransform.localEulerAngles.y, 0f);

        animator.SetInteger(CurrentNormalAttackCombo, currentAttackCombo);
        animator.SetBool(IsChargedAttack, false);
        animator.SetInteger(WeaponType, (int)chosenAttackWeapon.weaponType);

        isCurrentlyAttacking = true;
        isCurrentAttackNormal = true;
    }

    private void KatanaNormalAttack()
    {
        foreach (var enemy in enemiesWaitingToAttack)
        {
            if (!enemy.IsSpawned) return;

            var normalAttackDamage = CalculateDamage(currentAttack,
                chosenAttackWeapon.comboAttackScales[currentAttackCombo],
                normalAttackDamageBonus, currentCritRate, currentCritDamage);

            enemy.ReceiveDamage(normalAttackDamage, playerController);

            var enemyEffectsController = enemy.GetComponent<EnemyEffects>();
            foreach (var slowEnemyOnHit in slowEnemyOnHits)
            {
                var isApplyingEffect = Random.Range(0, 100) <= slowEnemyOnHit.effectChance * 100;

                if (!isApplyingEffect) return;

                enemyEffectsController.AddOrRemoveEffect(EnemyEffects.EnemiesEffects.SlowDebuff,
                    slowEnemyOnHit.speedDecrease, true, false, slowEnemyOnHit.slowDuration);
            }
        }

        for (var i = 0; i < enemiesWaitingToAttack.Count; i++)
        {
            enemiesWaitingToAttack.RemoveAt(i);
            i--;
        }
    }

    private void GunNormalAttack()
    {
        var normalAttackDamage = CalculateDamage(currentAttack,
            chosenAttackWeapon.comboAttackScales[currentAttackCombo],
            normalAttackDamageBonus, currentCritRate, currentCritDamage);

        var closestEnemyPosition = FindNearestEnemyPositionForRangeAttack();

        var bulletAngle = 0f;
        var bulletSpawnPosition = bulletSpawnTransform.position;
        if (closestEnemyPosition != bulletSpawnPosition)
        {
            if (closestEnemyPosition.x == bulletSpawnPosition.x)
                bulletAngle = bulletSpawnPosition.z - closestEnemyPosition.z < 0
                    ? -transform.localEulerAngles.z
                    : -transform.localEulerAngles.z + 180;
            else if (closestEnemyPosition.z == bulletSpawnPosition.z)
                bulletAngle = bulletSpawnPosition.x - closestEnemyPosition.x < 0
                    ? -transform.localEulerAngles.z + 90
                    : -transform.localEulerAngles.z + 270;
            else
                bulletAngle =
                    -Mathf.Atan2(closestEnemyPosition.z - bulletSpawnPosition.z,
                        closestEnemyPosition.x - bulletSpawnPosition.x) * 180 / Mathf.PI;
        }
        else
        {
            bulletAngle = transform.localEulerAngles.y - 90;
        }

        Debug.Log($"{closestEnemyPosition} {bulletSpawnTransform.position} {bulletAngle}");

        var bulletTransform = Instantiate(bulletPrefab, bulletSpawnTransform.position, Quaternion.identity);
        var weaponBullet = bulletTransform.GetComponent<WeaponBullet>();
        weaponBullet.InitializeBullet(bulletAngle, bulletLifetime, normalAttackDamage,
            playerController, enemiesLayer);
        weaponBullet.OnEnemyHitted += WeaponBullet_OnEnemyHitted;
    }

    private void WeaponBullet_OnEnemyHitted(object sender, WeaponBullet.OnEnemyHittedEventArgs e)
    {
        var bullet = sender as WeaponBullet;

        var enemyEffectsController = e.hittedEnemy.GetComponent<EnemyEffects>();
        foreach (var slowEnemyOnHit in slowEnemyOnHits)
        {
            var isApplyingEffect = Random.Range(0, 100) <= slowEnemyOnHit.effectChance * 100;

            if (!isApplyingEffect) return;

            enemyEffectsController.AddOrRemoveEffect(EnemyEffects.EnemiesEffects.SlowDebuff,
                slowEnemyOnHit.speedDecrease, true, false, slowEnemyOnHit.slowDuration);
        }

        OnEnemyHitted?.Invoke(this, new OnEnemyHittedEventArgs
        {
            hittedEnemy = e.hittedEnemy
        });

        bullet.OnEnemyHitted -= WeaponBullet_OnEnemyHitted;
    }

    #endregion

    #region Charged Attack

    private void ChargeAttack()
    {
        ChargeAttackServerRpc();
    }

    private void KatanaChargedAttack()
    {
        foreach (var enemy in enemiesWaitingToAttack)
        {
            if (!enemy) continue;

            var chargeAttackDamage = CalculateDamage(currentAttack,
                chosenAttackWeapon.chargedAttackDamageScale,
                chargedAttackDamageBonus, currentCritRate, currentCritDamage);

            enemy.ReceiveDamage(chargeAttackDamage, playerController);

            var enemyEffectsController = enemy.GetComponent<EnemyEffects>();
            foreach (var slowEnemyOnHit in slowEnemyOnHits)
            {
                var isApplyingEffect = Random.Range(0, 100) <= slowEnemyOnHit.effectChance * 100;

                if (!isApplyingEffect) return;

                enemyEffectsController.AddOrRemoveEffect(EnemyEffects.EnemiesEffects.SlowDebuff,
                    slowEnemyOnHit.speedDecrease, true, false, slowEnemyOnHit.slowDuration);
            }
        }

        for (var i = 0; i < enemiesWaitingToAttack.Count; i++)
        {
            enemiesWaitingToAttack.RemoveAt(i);
            i--;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChargeAttackServerRpc()
    {
        var currentChooseWeaponSo = playerWeapons.GetCurrentWeaponSo();
        if (currentChooseWeaponSo == null) return;

        ResetNormalAttackCombo();

        chosenAttackWeapon = playerWeapons.GetCurrentWeaponSo();

        animator.SetInteger(CurrentNormalAttackCombo, currentAttackCombo);
        animator.SetBool(IsChargedAttack, true);
        animator.SetInteger(WeaponType, (int)chosenAttackWeapon.weaponType);

        isCurrentlyAttacking = true;
        isCurrentAttackNormal = false;

        //Debug.Log($"I'm attacking with damage:{chargeAttackDamage} by C.A.");
    }

    #endregion

    #region Reset Normal Attack Combo

    private void ResetNormalAttackCombo()
    {
        if (!IsServer) return;
        if (isCurrentlyAttacking) return;

        ResetNormalAttackComboClientRpc();
    }

    [ClientRpc]
    private void ResetNormalAttackComboClientRpc()
    {
        comboAttackResetTimer = comboAttackResetTime;
        currentAttackCombo = -1;
    }

    #endregion

    #region Calculate Values

    private int CalculateDamage(int attack, float attackScale, float damageBonus, float critCrate, float critDamage)
    {
        var attackDamage = (int)(attack * attackScale * (1 + damageBonus) * GetCritValue(critCrate, critDamage));

        return attackDamage;
    }

    private float GetCritValue(float critRate, float critDamage)
    {
        var isCrit = Random.Range(0, 1000) < critRate * 1000 ? 1 : 0;
        var critValue = 1 + critDamage * isCrit;

        if (isCrit == 0)
            foreach (var critRateIncreaseOnHit in critRateIncreaseOnHitBuffs)
            {
                if (critRateIncreaseOnHit.currentCritRateIncrease >=
                    critRateIncreaseOnHit.maxCritRateIncrease) continue;

                ChangeCritRateBuff(critRateIncreaseOnHit.critRateIncreasePerHit);
                critRateIncreaseOnHit.currentCritRateIncrease += critRateIncreaseOnHit.critRateIncreasePerHit;
            }
        else
            foreach (var critRateIncreaseOnHit in critRateIncreaseOnHitBuffs)
            {
                if (critRateIncreaseOnHit.currentCritRateIncrease <= 0) continue;

                ChangeCritRateBuff(-critRateIncreaseOnHit.currentCritRateIncrease);
                critRateIncreaseOnHit.currentCritRateIncrease = 0;
            }

        return critValue;
    }

    #endregion

    #region Buffs

    public void ChangeEnemySlowOnHit(float slowValue, float slowDuration, float effectChance, int effectId)
    {
        ChangeEnemySlowOnHitServerRpc(slowValue, slowDuration, effectChance, effectId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeEnemySlowOnHitServerRpc(float slowValue, float slowDuration, float effectChance, int effectId)
    {
        if (slowValue > 0)
        {
            var slowEnemyOnHit = new SlowEnemyOnHit
            {
                slowDuration = slowDuration,
                speedDecrease = slowValue,
                effectChance = effectChance,
                effectId = effectId
            };
            slowEnemyOnHits.Add(slowEnemyOnHit);
        }
        else
        {
            foreach (var slowEnemyOnHit in slowEnemyOnHits)
            {
                if (slowEnemyOnHit.effectId != effectId) continue;

                slowEnemyOnHits.Remove(slowEnemyOnHit);
                break;
            }
        }
    }

    public void ChangeAttackBuff(float percentageBuff = default, int flatBuff = default)
    {
        ChangeAttackBuffServerRpc(percentageBuff, flatBuff);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeAttackBuffServerRpc(float percentageBuff, int flatBuff)
    {
        var newCurrentAttack = (int)(currentAttack + baseAttack * percentageBuff + flatBuff);

        ChangeAttackBuffClientRpc(newCurrentAttack);
    }

    [ClientRpc]
    private void ChangeAttackBuffClientRpc(int newCurrentAttack)
    {
        currentAttack = newCurrentAttack;
    }

    public void ChangeNormalAttackBuff(float percentageBuff)
    {
        ChangeNormalAttackBuffServerRpc(percentageBuff);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeNormalAttackBuffServerRpc(float percentageBuff)
    {
        var newNormalAttackBonus = normalAttackDamageBonus + percentageBuff;

        ChangeNormalAttackBuffClientRpc(newNormalAttackBonus);
    }

    [ClientRpc]
    private void ChangeNormalAttackBuffClientRpc(float newNormalAttackBonus)
    {
        normalAttackDamageBonus = newNormalAttackBonus;
    }

    public void ChangeChargedAttackBuff(float percentageBuff)
    {
        ChangeChargedAttackBuffServerRpc(percentageBuff);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeChargedAttackBuffServerRpc(float percentageBuff)
    {
        var newChargedAttackBonus = chargedAttackDamageBonus + percentageBuff;

        ChangeChargedAttackBuffClientRpc(newChargedAttackBonus);
    }

    [ClientRpc]
    private void ChangeChargedAttackBuffClientRpc(float newChargedAttackBonus)
    {
        chargedAttackDamageBonus = newChargedAttackBonus;
    }

    public void ChangeCritRateBuff(float percentageBuff)
    {
        ChangeCritRateBuffServerRpc(percentageBuff);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeCritRateBuffServerRpc(float percentageBuff)
    {
        var newCritRate = currentCritRate + percentageBuff;

        ChangeCritRateBuffClientRpc(newCritRate);
    }

    [ClientRpc]
    private void ChangeCritRateBuffClientRpc(float newCritRate)
    {
        currentCritRate = newCritRate;
    }

    public void ChangeCritDamageBuff(float percentageBuff)
    {
        ChangeCritDamageBuffServerRpc(percentageBuff);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeCritDamageBuffServerRpc(float percentageBuff)
    {
        var newCritDamage = currentCritDamage + percentageBuff;

        ChangeCritDamageBuffClientRpc(newCritDamage);
    }

    [ClientRpc]
    private void ChangeCritDamageBuffClientRpc(float newCritDamage)
    {
        currentCritDamage = newCritDamage;
    }

    public void ChangeCritRateOnHitIncreaseBuff(float critRateOnHitIncreaseValue, float maxCritRateIncrease,
        int effectId)
    {
        ChangeCritRateOnHitIncreaseBuffServerRpc(critRateOnHitIncreaseValue, maxCritRateIncrease, effectId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeCritRateOnHitIncreaseBuffServerRpc(float critRateOnHitIncreaseValue, float maxCritRateIncrease,
        int effectId)
    {
        if (critRateOnHitIncreaseValue > 0)
        {
            var newCritRateIncreaseBuff = new CritRateIncreaseOnHit
            {
                critRateIncreasePerHit = critRateOnHitIncreaseValue,
                maxCritRateIncrease = maxCritRateIncrease,
                effectId = effectId
            };
            critRateIncreaseOnHitBuffs.Add(newCritRateIncreaseBuff);
        }
        else
        {
            foreach (var critRateIncreaseOnHit in critRateIncreaseOnHitBuffs)
            {
                if (critRateIncreaseOnHit.effectId != effectId) continue;

                critRateIncreaseOnHitBuffs.Remove(critRateIncreaseOnHit);
                break;
            }
        }
    }

    #endregion

    #region GetVariablesData

    public int GetBaseAttack()
    {
        return baseAttack;
    }

    public int GetCurrentAttack()
    {
        return currentAttack;
    }

    public int GetCurrentCritRate()
    {
        return (int)(currentCritRate * 100);
    }

    public int GetCurrentCritDmg()
    {
        return (int)(currentCritDamage * 100);
    }

    public int GetCurrentNaDmgBonus()
    {
        return (int)(normalAttackDamageBonus * 100);
    }

    public int GetCurrentCaDmgBonus()
    {
        return (int)(chargedAttackDamageBonus * 100);
    }

    #endregion
}
