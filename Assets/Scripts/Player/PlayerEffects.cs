using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEffects : MonoBehaviour
{
    public event EventHandler<OnPlayerRelicOutOfUsagesCountEventArgs> OnPlayerRelicOutOfUsagesCount;

    public class OnPlayerRelicOutOfUsagesCountEventArgs : EventArgs
    {
        public RelicBuff relicBuff;
    }

    public class PlayerBuff
    {
        public enum Buffs
        {
            AtkBuff,
            CritRateBuff,
            CritDamageBuff,
            NormalAttackDamageBonusBuff,
            ChargedAttackDamageBonusBuff,
            HpBuff,
            DefBuff
        }

        public Buffs buffs;
        public float percentageBuffScale;
        public int flatBuffScale;
        public bool isBuffEndless;
        public float buffRemainingTime;
    }

    public enum RelicBuffTypes
    {
        TakenDmgAbsorption,
        TakenDmgIncrease,
        NaDmgBuff,
        CaDmgBuff,
        ExpBuff,
        DefBuff,
        HpBuff,
        HpRegeneratePerKill
    }

    [Serializable]
    public class RelicBuff
    {
        public RelicBuffTypes relicBuffType;
        public float relicBuffScale;
        public bool isHasLimit;
        public int maxUsagesLimit;
        [HideInInspector] public int currentUsages;
    }

    public class RelicBuffEffectTriggeredEventArgs : EventArgs
    {
        public RelicBuffTypes buffType;
        public int spentValue;
    }

    #region Buffs

    private readonly List<PlayerBuff> allPlayerBuffList = new();
    private readonly List<RelicBuff> allPlayerRelicBuffList = new();

    #endregion

    private PlayerHealth playerHealth;
    private PlayerAttackController playerAttackController;
    private PlayerWeapons playerWeapons;
    private PlayerRelics playerRelics;
    private PlayerController playerController;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        playerAttackController = GetComponent<PlayerAttackController>();
        playerWeapons = GetComponent<PlayerWeapons>();
        playerRelics = GetComponent<PlayerRelics>();
        playerController = GetComponent<PlayerController>();
    }

    private void Start()
    {
        playerWeapons.OnCurrentWeaponChange += PlayerAttackController_OnCurrentWeaponChange;

        playerRelics.OnRelicsChange += PlayerRelics_OnRelicsChange;
    }

    private void PlayerRelics_OnRelicsChange(object sender, PlayerRelics.OnRelicChangeEventArgs e)
    {
        if (e.addedRelic != null)
        {
            e.addedRelic.TryGetRelicSo(out var relicSo);
            foreach (var relicBuff in relicSo.relicBuffs) AddRelicBuff(relicBuff);
        }

        if (e.removedRelic != null)
        {
            e.removedRelic.TryGetRelicSo(out var relicSo);
            foreach (var relicBuff in relicSo.relicBuffs) RemoveRelicBuff(relicBuff);
        }
    }

    private void PlayerAttackController_OnCurrentWeaponChange(object sender,
        PlayerWeapons.OnCurrentWeaponChangeEventArgs e)
    {
        if (e.previousWeapon != null)
        {
            e.previousWeapon.TryGetWeaponSo(out var previousWeaponSo);
            DispelBuff(previousWeaponSo.additionalWeaponStatType, previousWeaponSo.additionalWeaponStatTypeScale);
        }

        e.newWeapon.TryGetWeaponSo(out var newWeaponSo);
        ApplyBuff(newWeaponSo.additionalWeaponStatType, 0, true,
            newWeaponSo.additionalWeaponStatTypeScale);
    }

    private void Update()
    {
        if (GameStageManager.Instance.IsPause()) return;

        TickBuffTimers();
    }

    private void TickBuffTimers()
    {
        foreach (var buff in allPlayerBuffList)
            if (!buff.isBuffEndless)
            {
                buff.buffRemainingTime -= Time.deltaTime;
                if (buff.buffRemainingTime <= 0)
                {
                    AddRemoveBuffToController(buff.buffs, -buff.percentageBuffScale, -buff.flatBuffScale);
                    allPlayerBuffList.Remove(buff);
                }
            }
    }

    public void ApplyBuff(PlayerBuff.Buffs buffs, float totalBuffDuration, bool isBuffEndless = false,
        float percentageBuff = default, int flatBuffScale = 0)
    {
        var buffToAdd = new PlayerBuff
        {
            buffs = buffs,
            buffRemainingTime = totalBuffDuration,
            isBuffEndless = isBuffEndless,
            percentageBuffScale = percentageBuff,
            flatBuffScale = flatBuffScale
        };

        if (!allPlayerBuffList.Contains(buffToAdd))
        {
            allPlayerBuffList.Add(buffToAdd);

            AddRemoveBuffToController(buffToAdd.buffs, buffToAdd.percentageBuffScale, buffToAdd.flatBuffScale);
        }
    }

    private void DispelBuff(PlayerBuff.Buffs buffs, float percentageBuff, int flatBuffScale = 0)
    {
        foreach (var buff in allPlayerBuffList)
            if (buff.buffs == buffs && buff.percentageBuffScale == percentageBuff &&
                buff.flatBuffScale == flatBuffScale)
            {
                AddRemoveBuffToController(buffs, -percentageBuff, -flatBuffScale);
                return;
            }
    }

    private void AddRemoveBuffToController(PlayerBuff.Buffs buffs, float percentageBuffScale,
        int flatBuffScale = default)
    {
        switch (buffs)
        {
            case PlayerBuff.Buffs.HpBuff:
                playerHealth.ChangeHealthBuff(percentageBuffScale, flatBuffScale);
                break;
            case PlayerBuff.Buffs.DefBuff:
                playerHealth.ChangeDefenceBuff(percentageBuffScale, flatBuffScale);
                break;
            case PlayerBuff.Buffs.AtkBuff:
                playerAttackController.ChangeAttackBuff(percentageBuffScale, flatBuffScale);
                break;
            case PlayerBuff.Buffs.CritRateBuff:
                playerAttackController.ChangeCritRateBuff(percentageBuffScale);
                break;
            case PlayerBuff.Buffs.CritDamageBuff:
                playerAttackController.ChangeCritDamageBuff(percentageBuffScale);
                break;
            case PlayerBuff.Buffs.NormalAttackDamageBonusBuff:
                playerAttackController.ChangeNormalAttackBuff(percentageBuffScale);
                break;
            case PlayerBuff.Buffs.ChargedAttackDamageBonusBuff:
                playerAttackController.ChangeChargedAttackBuff(percentageBuffScale);
                break;
        }
    }

    private void AddRelicBuff(RelicBuff relicBuff)
    {
        if (allPlayerRelicBuffList.Contains(relicBuff)) return;

        AddRemoveRelicBuffToController(relicBuff);
        allPlayerRelicBuffList.Add(relicBuff);
    }

    private void RemoveRelicBuff(RelicBuff relicBuff)
    {
        if (!allPlayerRelicBuffList.Contains(relicBuff)) return;

        allPlayerRelicBuffList.Remove(relicBuff);
        AddRemoveRelicBuffToController(relicBuff, true);
    }

    private void AddRemoveRelicBuffToController(RelicBuff relicBuff, bool isRemoving = false)
    {
        var relicBuffType = relicBuff.relicBuffType;
        var percentageValue = relicBuff.relicBuffScale;
        percentageValue = isRemoving ? -percentageValue : percentageValue;
        var isLimited = relicBuff.isHasLimit;

        switch (relicBuffType)
        {
            case RelicBuffTypes.TakenDmgAbsorption:
                playerHealth.ChangeTakenDamageAbsorptionBuff(percentageValue);
                if (isLimited)
                    if (!IsAlreadyHasCurrentLimitedBuff(relicBuffType))
                        if (!isRemoving)
                            playerHealth.OnHealthAbsorptionTriggered += BuffedController_OnLimitedBuffEffectTriggered;
                        else
                            playerHealth.OnHealthAbsorptionTriggered -= BuffedController_OnLimitedBuffEffectTriggered;
                break;
            case RelicBuffTypes.TakenDmgIncrease:
                playerHealth.ChangeTakenDamageIncreaseDebuff(percentageValue);
                break;
            case RelicBuffTypes.NaDmgBuff:
                playerAttackController.ChangeNormalAttackBuff(percentageValue);
                break;
            case RelicBuffTypes.CaDmgBuff:
                playerAttackController.ChangeChargedAttackBuff(percentageValue);
                break;
            case RelicBuffTypes.ExpBuff:
                playerController.ChangeExpAdditionalMultiplayer(percentageValue);
                break;
            case RelicBuffTypes.DefBuff:
                playerHealth.ChangeDefenceBuff(percentageValue);
                break;
            case RelicBuffTypes.HpBuff:
                playerHealth.ChangeHealthBuff(percentageValue);
                break;
            case RelicBuffTypes.HpRegeneratePerKill:
                playerController.AddRegeneratingHpAfterEnemyDeath(percentageValue);
                if (isLimited)
                    if (!IsAlreadyHasCurrentLimitedBuff(relicBuffType))
                        if (!isRemoving)
                            playerController.OnPlayerRegenerateHpAfterEnemyDeath +=
                                BuffedController_OnLimitedBuffEffectTriggered;
                        else
                            playerController.OnPlayerRegenerateHpAfterEnemyDeath -=
                                BuffedController_OnLimitedBuffEffectTriggered;
                break;
        }
    }

    private void BuffedController_OnLimitedBuffEffectTriggered(object sender, RelicBuffEffectTriggeredEventArgs e)
    {
        var relicsToDeleteList = new List<RelicBuff>();

        foreach (var relicBuff in allPlayerRelicBuffList)
            if (relicBuff.isHasLimit && e.buffType == relicBuff.relicBuffType)
            {
                relicBuff.currentUsages += e.spentValue;
                if (relicBuff.maxUsagesLimit <= relicBuff.currentUsages)
                    relicsToDeleteList.Add(relicBuff);
            }

        foreach (var relicBuff in relicsToDeleteList)
            OnPlayerRelicOutOfUsagesCount?.Invoke(this, new OnPlayerRelicOutOfUsagesCountEventArgs
            {
                relicBuff = relicBuff
            });
    }

    private bool IsAlreadyHasCurrentLimitedBuff(RelicBuffTypes relicBuffType)
    {
        foreach (var relicBuff in allPlayerRelicBuffList)
            if (relicBuff.isHasLimit && relicBuff.relicBuffType == relicBuffType)
                return true;

        return false;
    }
}
