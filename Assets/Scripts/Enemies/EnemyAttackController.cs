using UnityEngine;

public class EnemyAttackController : MonoBehaviour
{
    [SerializeField] private LayerMask playerLayer;

    [SerializeField] private float attackRange = 2.5f;

    [SerializeField] private int baseAttack = 15;
    private int currentAttack;

    private void Awake()
    {
        currentAttack = baseAttack;
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
        currentAttack += (int)(baseAttack * percentageBuff);
    }
}
