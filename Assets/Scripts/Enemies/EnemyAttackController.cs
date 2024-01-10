using Unity.Netcode;
using UnityEngine;

public class EnemyAttackController : NetworkBehaviour
{
    #region Variables & References

    [SerializeField] private LayerMask playerLayer;

    #endregion

    #region Attack Data

    [SerializeField] private float attackRange = 2.5f;

    [SerializeField] private int baseAttack = 100;
    private int currentAttack;

    [SerializeField] private float basicAttackScale = 0.15f;

    #endregion

    #region Initialization

    private void Awake()
    {
        currentAttack = baseAttack;

        if (!IsServer) return;

        var additionalBaseAtkIncrease =
            DungeonLevelsDifficulty.Instance.GetEnemyBaseStatIncreaseByCurrentConnectedPlayers(DungeonLevelsDifficulty
                .StatsIncreaseOnDungeonDifficulty.EnemiesStatIncrease.ATK);

        var newBaseAtk = (int)(baseAttack * additionalBaseAtkIncrease);

        var additionalAtkIncrease =
            DungeonLevelsDifficulty.Instance.GetCurrentEnemyStatIncreaseMultiplayer(DungeonLevelsDifficulty
                .StatsIncreaseOnDungeonDifficulty.EnemiesStatIncrease.ATK);

        var newCurrentAttack = (int)(currentAttack + newBaseAtk * additionalAtkIncrease);
        DungeonDifficultyAttackChangeClientRpc(newCurrentAttack, newBaseAtk);
    }

    [ClientRpc]
    private void DungeonDifficultyAttackChangeClientRpc(int newCurrentAttack, int newBaseAtk)
    {
        currentAttack = newCurrentAttack;
        baseAttack = newBaseAtk;
    }

    #endregion

    #region Attack

    public void Attack()
    {
        var raycastHits =
            Physics.SphereCastAll(transform.position, attackRange,
                Vector3.forward, attackRange, playerLayer);

        var attackDamage = (int)(-currentAttack * basicAttackScale);

        foreach (var hit in raycastHits)
            if (hit.transform.gameObject.TryGetComponent(out PlayerController playerController))
                playerController.ChangeHealth(attackDamage);
    }

    #endregion

    #region Buffs

    public void ChangeAttackBuff(float percentageBuff)
    {
        if (!IsServer) return;

        ChangeAttackBuffServerRpc(percentageBuff);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeAttackBuffServerRpc(float percentageBuff)
    {
        var newCurrentAttack = (int)(currentAttack + baseAttack * percentageBuff);

        ChangeAttackBuffClientRpc(newCurrentAttack);
    }

    [ClientRpc]
    private void ChangeAttackBuffClientRpc(int newCurrentAttack)
    {
        currentAttack = newCurrentAttack;
    }

    #endregion
}
