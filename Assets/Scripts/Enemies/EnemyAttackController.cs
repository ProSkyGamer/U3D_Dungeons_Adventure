using Unity.Netcode;
using UnityEngine;

public class EnemyAttackController : NetworkBehaviour
{
    [SerializeField] private LayerMask playerLayer;

    [SerializeField] private float attackRange = 2.5f;

    [SerializeField] private int baseAttack = 15;
    private int currentAttack;

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

    public void Attack()
    {
        var raycastHits =
            Physics.SphereCastAll(transform.position, attackRange,
                Vector3.forward, attackRange, playerLayer);

        foreach (var hit in raycastHits)
            if (hit.transform.gameObject.TryGetComponent(out PlayerController playerController))
                playerController.ChangeHealth(-currentAttack);
    }

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
}
