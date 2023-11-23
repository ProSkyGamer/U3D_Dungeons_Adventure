using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
public class PlayerAttackController : MonoBehaviour
{
    public static event EventHandler OnChargeAttackStopCharging;
    public static event EventHandler OnGunChargedAttackTriggered;
    public event EventHandler<OnEnemyHittedEventArgs> OnEnemyHitted;

    public class OnEnemyHittedEventArgs : EventArgs
    {
        public EnemyController hittedEnemy;
    }

    private int currentAttackCombo = -1;

    [SerializeField] private float comboAttackResetTime = 3f;
    private float comboAttackResetTimer;

    private bool isTryingToChargedAttack;
    private bool isAiming;
    [SerializeField] private float chargedAttackPressTime = 1f;
    private float chargedAttackPressTimer;

    [SerializeField] private float meleeWeaponAttackRange = 3f;
    [SerializeField] private float rangedWeaponAttackRange = 10f;
    [SerializeField] private Transform bulletPrefab;
    [SerializeField] private float bulletLifetime = 3f;
    [FormerlySerializedAs("bulletSpawnPosition")] [SerializeField] private Transform bulletSpawnTransform;
    [SerializeField] private LayerMask enemiesLayer;

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

    private bool isAnyInterfaceOpened;
    private bool isCursorShown;

    private PlayerWeapons playerWeapons;
    private PlayerController playerController;
    private StaminaController staminaController;

    private void Awake()
    {
        playerWeapons = GetComponent<PlayerWeapons>();
        playerController = GetComponent<PlayerController>();
        staminaController = GetComponent<StaminaController>();

        currentAttack = baseAttack;

        currentCritRate = baseCritRate;
        currentCritDamage = baseCritDamage;

        comboAttackResetTimer = comboAttackResetTime;
    }

    private void Start()
    {
        GameInput.Instance.OnAttackAction += GameInput_OnAttackAction;

        GameInput.Instance.OnCursorShowAction += GameInput_OnCursorShowAction;

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

        NormalAttack();
        isTryingToChargedAttack = true;
    }

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
            {
                isAiming = false;
                var currentWeaponSo = playerWeapons.GetCurrentWeaponSo();

                var bulletAngle = transform.localEulerAngles.y - 90;
                var chargeAttackDamage = CalculateDamage(currentAttack,
                    currentWeaponSo.chargedAttackDamageScale,
                    chargedAttackDamageBonus, currentCritRate, currentCritDamage);

                staminaController.SpendStamina(currentWeaponSo.chargedAttackStaminaCost);

                var bulletTransform = Instantiate(bulletPrefab, bulletSpawnTransform.position, Quaternion.identity);
                var weaponBullet = bulletTransform.GetComponent<WeaponBullet>();
                weaponBullet.InitializeBullet(bulletAngle, bulletLifetime, chargeAttackDamage,
                    playerController, enemiesLayer);
                weaponBullet.OnEnemyHitted += WeaponBullet_OnEnemyHitted;

                OnGunChargedAttackTriggered?.Invoke(this, EventArgs.Empty);
            }

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

    private List<EnemyController> FindEnemiesToAttack()
    {
        var castPosition = transform.position;
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

    private void NormalAttack()
    {
        var currentChooseWeaponSo = playerWeapons.GetCurrentWeaponSo();
        if (currentChooseWeaponSo == null) return;

        currentAttackCombo++;
        if (currentAttackCombo >= currentChooseWeaponSo.comboAttack)
            currentAttackCombo = 0;

        var normalAttackDamage = CalculateDamage(currentAttack,
            currentChooseWeaponSo.comboAttackScales[currentAttackCombo],
            normalAttackDamageBonus, currentCritRate, currentCritDamage);

        switch (currentChooseWeaponSo.weaponType)
        {
            case WeaponSO.WeaponType.Katana:
                var enemiesToAttack = FindEnemiesToAttack();

                foreach (var enemy in enemiesToAttack)
                    enemy.ReceiveDamage(normalAttackDamage, playerController);
                break;
            case WeaponSO.WeaponType.Gun:
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

                break;
        }

        comboAttackResetTimer = comboAttackResetTime;
    }

    private void WeaponBullet_OnEnemyHitted(object sender, WeaponBullet.OnEnemyHittedEventArgs e)
    {
        var bullet = sender as WeaponBullet;

        OnEnemyHitted?.Invoke(this, new OnEnemyHittedEventArgs
        {
            hittedEnemy = e.hittedEnemy
        });

        bullet.OnEnemyHitted -= WeaponBullet_OnEnemyHitted;
    }

    private void ResetNormalAttackCombo()
    {
        comboAttackResetTimer = comboAttackResetTime;
        currentAttackCombo = -1;
    }

    private void ChargeAttack()
    {
        var currentChooseWeaponSo = playerWeapons.GetCurrentWeaponSo();
        if (currentChooseWeaponSo == null) return;

        ResetNormalAttackCombo();

        var chargeAttackDamage = CalculateDamage(currentAttack,
            currentChooseWeaponSo.chargedAttackDamageScale,
            chargedAttackDamageBonus, currentCritRate, currentCritDamage);

        switch (currentChooseWeaponSo.weaponType)
        {
            case WeaponSO.WeaponType.Katana:
                var enemiesToAttack = FindEnemiesToAttack();
                staminaController.SpendStamina(currentChooseWeaponSo.chargedAttackStaminaCost);
                foreach (var enemy in enemiesToAttack) enemy.ReceiveDamage(chargeAttackDamage, playerController);
                break;
            case WeaponSO.WeaponType.Gun:
                isAiming = true;
                OnGunChargedAttackTriggered?.Invoke(this, EventArgs.Empty);
                break;
        }

        //Debug.Log($"I'm attacking with damage:{chargeAttackDamage} by C.A.");
    }

    private int CalculateDamage(int attack, float attackScale, float damageBonus, float critCrate, float critDamage)
    {
        var attackDamage = (int)(attack * attackScale * (1 + damageBonus) * GetCritValue(critCrate, critDamage));

        return attackDamage;
    }

    private float GetCritValue(float critRate, float critDamage)
    {
        var isCrit = Random.Range(0, 1000) < critRate * 1000 ? 1 : 0;
        var critValue = 1 + critDamage * isCrit;

        return critValue;
    }

    #region Buffs

    public void ChangeAttackBuff(float percentageBuff = default, int flatBuff = default)
    {
        currentAttack += (int)(baseAttack * percentageBuff);
        currentAttack += flatBuff;
    }

    public void ChangeNormalAttackBuff(float percentageBuff)
    {
        normalAttackDamageBonus += percentageBuff;
    }

    public void ChangeChargedAttackBuff(float percentageBuff)
    {
        chargedAttackDamageBonus += percentageBuff;
    }

    public void ChangeCritRateBuff(float percentageBuff)
    {
        currentCritRate += percentageBuff;
    }

    public void ChangeCritDamageBuff(float percentageBuff)
    {
        currentCritDamage += percentageBuff;
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

    public static void ResetStaticData()
    {
        OnGunChargedAttackTriggered = null;
        OnChargeAttackStopCharging = null;
    }
}
