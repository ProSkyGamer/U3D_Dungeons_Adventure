using Unity.Netcode;
using UnityEngine;

public class EnemyAttackController : NetworkBehaviour
{
    [SerializeField] private LayerMask playerLayer;

    [SerializeField] private float attackRange = 2.5f;

    [SerializeField] private int baseAttack = 15;
    private int currentAttack;

    private float currentAttackPercentageBuff = 1f;

    private void Awake()
    {
        currentAttack = baseAttack;
    }

    private void Start()
    {
        if (!IsServer) return;

        DungeonSettings.OnDungeonDifficultyChange += DungeonDifficulty_OnDungeonDifficultyChange;
    }

    private void DungeonDifficulty_OnDungeonDifficultyChange(object sender,
        DungeonSettings.OnDungeonDifficultyChangeEventArgs e)
    {
        var currentAtkDifficultyMultiplayer =
            DungeonSettings.GetEnemiesAtkMultiplayerByDungeonDifficulty(e.newDungeonDifficulty);
        var currentAtkPlayersCountMultiplayer = DungeonSettings.GetEnemiesAtkMultiplayerByPlayersCount();

        var newBaseAttack = (int)(baseAttack * currentAtkDifficultyMultiplayer * currentAtkPlayersCountMultiplayer);

        var newCurrentAttack = (int)(baseAttack * currentAttackPercentageBuff);

        OnDungeonDifficultyChangeClientRpc(newBaseAttack, newCurrentAttack);
    }

    [ClientRpc]
    private void OnDungeonDifficultyChangeClientRpc(int newBaseAttack, int newCurrentAttack)
    {
        baseAttack = newBaseAttack;
        currentAttack = newCurrentAttack;
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
        currentAttackPercentageBuff += percentageBuff;

        ChangeAttackBuffClientRpc(newCurrentAttack);
    }

    [ClientRpc]
    private void ChangeAttackBuffClientRpc(int newCurrentAttack)
    {
        currentAttack = newCurrentAttack;
    }
}
