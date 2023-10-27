using System.Collections.Generic;
using UnityEngine;

public class PlayerEffects : MonoBehaviour
{
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

    #region Buffs

    private readonly List<PlayerBuff> allPlayerBuffList = new();

    #endregion

    private PlayerHealth playerHealth;
    private PlayerAttackController playerAttackController;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        playerAttackController = GetComponent<PlayerAttackController>();
    }

    private void Start()
    {
        playerAttackController.OnCurrentWeaponChange += PlayerAttackController_OnCurrentWeaponChange;
    }

    private void PlayerAttackController_OnCurrentWeaponChange(object sender,
        PlayerAttackController.OnCurrentWeaponChangeEventArgs e)
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
        TickAttackBuffTimers();
    }

    private void TickAttackBuffTimers()
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
                AddRemoveBuffToController(buffs, percentageBuff, flatBuffScale);
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
}
