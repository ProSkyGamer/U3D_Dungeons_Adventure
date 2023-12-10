using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(EnemyHealth))]
[RequireComponent(typeof(EnemyAttackController))]
[RequireComponent(typeof(EnemyEffects))]
public class EnemyController : MonoBehaviour
{
    public event EventHandler<OnEnemyDeathEventArgs> OnEnemyDeath;

    public class OnEnemyDeathEventArgs : EventArgs
    {
        public int coinsValue;
        public int expValue;
    }

    [SerializeField] private int coinsForKill = 1;
    [SerializeField] private int experienceForKill = 1;
    private readonly List<PlayerController> playerAttackedEnemy = new();

    [SerializeField] private float timeBetweenAttacks = 5f;
    private float timerBetweenAttacks;

    [SerializeField] private float playerDetectionRange = 12f;
    private Transform currentFollowingPlayer;
    private float currentDistanceToPlayer;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask enemyLayer;

    private NavMeshAgent navMeshAgent;
    private Vector3 basePosition;

    private EnemyAttackController enemyAttackController;
    private EnemyHealth enemyHealth;

    private BufferEnemy bufferEnemy;
    private HealerEnemy healerEnemy;
    private ShielderEnemy shielderEnemy;

    [SerializeField] private float supportEnemyDistanceFromPlayer = 5f;

    [SerializeField] private float buffsReapplyingTime = 15f;
    private float buffsReapplyingTimer;


    [SerializeField] private float healsIntervalTime = 12.5f;
    private float healsIntervalTimer;

    [SerializeField] private float shieldsReapplyingTime = 12.5f;
    private float shieldsReapplyingTimer;

    [SerializeField] private float classEffectRange = 10f;

    private bool isHasAnyClass;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        enemyAttackController = GetComponent<EnemyAttackController>();
        enemyHealth = GetComponent<EnemyHealth>();

        TryGetComponent(out bufferEnemy);
        TryGetComponent(out healerEnemy);
        TryGetComponent(out shielderEnemy);

        basePosition = transform.position;

        timerBetweenAttacks = timeBetweenAttacks;

        buffsReapplyingTimer = buffsReapplyingTime;
        healsIntervalTimer = healsIntervalTime;
        shieldsReapplyingTimer = shieldsReapplyingTime;

        if (bufferEnemy != null || healerEnemy != null || shielderEnemy != null)
            isHasAnyClass = true;
    }

    private void Start()
    {
        enemyHealth.OnEnemyDie += EnemyHealth_OnEnemyDie;
    }

    private void EnemyHealth_OnEnemyDie(object sender, EventArgs e)
    {
        OnEnemyDeath?.Invoke(this, new OnEnemyDeathEventArgs
        {
            coinsValue = coinsForKill,
            expValue = experienceForKill
        });

        Destroy(gameObject);
    }

    private void Update()
    {
        if (GameStageManager.Instance.IsPause()) currentFollowingPlayer = transform;
        if (!GameStageManager.Instance.IsPlaying()) return;

        TryFindNearestPlayer();
        TryAttack();
        EnemyClassesTimerTick();
    }

    private void TryFindNearestPlayer()
    {
        var castPosition = transform.position;
        var castCubeLength = new Vector3(playerDetectionRange, playerDetectionRange, playerDetectionRange);

        List<RaycastHit> raycastHits = new();
        raycastHits.AddRange(Physics.BoxCastAll(castPosition, castCubeLength, Vector3.forward,
            quaternion.identity, playerDetectionRange, playerLayer));

        if (currentFollowingPlayer == null)
        {
            var nearestDistance = playerDetectionRange;
            var nearedPlayerTransform = transform;

            foreach (var hit in raycastHits)
                if (hit.transform.gameObject.TryGetComponent(out PlayerController playerController))
                    if (nearestDistance > hit.distance)
                    {
                        nearestDistance = hit.distance;
                        nearedPlayerTransform = hit.transform;
                    }

            if (nearedPlayerTransform != transform)
                currentFollowingPlayer = nearedPlayerTransform;
            else
                navMeshAgent.destination = basePosition;
        }
        else
        {
            foreach (var hit in raycastHits)
                if (hit.transform == currentFollowingPlayer)
                {
                    currentDistanceToPlayer = hit.distance;

                    navMeshAgent.destination =
                        currentDistanceToPlayer <= supportEnemyDistanceFromPlayer && isHasAnyClass
                            ? transform.position
                            : currentFollowingPlayer.position;
                    return;
                }

            currentFollowingPlayer = null;
            enemyHealth.RegenerateHealth(1f);
        }
    }

    private void TryAttack()
    {
        if (!isHasAnyClass)
        {
            var maxDistanceToPlayerToAttack = 2.5f;
            if (currentFollowingPlayer == null ||
                currentDistanceToPlayer > maxDistanceToPlayerToAttack) return;

            timerBetweenAttacks -= Time.deltaTime;

            if (timerBetweenAttacks <= 0)
            {
                timerBetweenAttacks = timeBetweenAttacks;
                enemyAttackController.Attack();
            }
        }
    }

    private void EnemyClassesTimerTick()
    {
        if (!isHasAnyClass) return;

        var isNeedEffectControllers = bufferEnemy != null;
        var enemiesInEffectRange = FindEnemiesForClass(
            out var enemyEffectsControllers, isNeedEffectControllers);

        if (bufferEnemy != null)
        {
            buffsReapplyingTimer -= Time.deltaTime;

            if (buffsReapplyingTimer <= 0)
            {
                buffsReapplyingTime = buffsReapplyingTimer;

                bufferEnemy.BuffEnemies(enemyEffectsControllers);
            }
        }

        if (healerEnemy != null)
        {
            healsIntervalTimer -= Time.deltaTime;

            if (healsIntervalTimer <= 0)
            {
                healsIntervalTimer = healsIntervalTime;

                healerEnemy.HealEnemies(enemiesInEffectRange);
            }
        }

        if (shielderEnemy != null)
        {
            shieldsReapplyingTimer -= Time.deltaTime;

            if (shieldsReapplyingTimer <= 0)
            {
                shieldsReapplyingTimer = shieldsReapplyingTime;

                shielderEnemy.ShieldEnemies(enemiesInEffectRange);
            }
        }
    }

    private List<EnemyController> FindEnemiesForClass(out List<EnemyEffects> enemyEffectsControllers,
        bool isNeedEffectsController = false)
    {
        enemyEffectsControllers = new List<EnemyEffects>();
        List<EnemyController> foundEnemies = new();

        var raycastHits = Physics.SphereCastAll(transform.position,
            classEffectRange, Vector3.up, classEffectRange, enemyLayer);

        foreach (var hit in raycastHits)
            if (hit.transform.gameObject.TryGetComponent(out EnemyController enemyController))
            {
                foundEnemies.Add(enemyController);
                if (isNeedEffectsController)
                    enemyEffectsControllers.Add(hit.transform.gameObject.GetComponent<EnemyEffects>());
            }

        return foundEnemies;
    }

    private void OnDrawGizmos()
    {
        var castPosition = transform.position;

        Gizmos.color = Color.red;
        var castCubeLength = new Vector3(playerDetectionRange, playerDetectionRange, playerDetectionRange);

        Gizmos.DrawCube(castPosition, castCubeLength);

        Gizmos.color = Color.green;
        var maxDistanceToPlayerToAttack = 2.5f;

        Gizmos.DrawSphere(castPosition, maxDistanceToPlayerToAttack);


        if (isHasAnyClass)
        {
            Gizmos.color = Color.cyan;

            Gizmos.DrawSphere(castPosition, classEffectRange);
        }
    }

    public void ReceiveDamage(int damage, PlayerController attackedPlayerController)
    {
        if (!playerAttackedEnemy.Contains(attackedPlayerController))
            playerAttackedEnemy.Add(attackedPlayerController);

        enemyHealth.TakeDamage(damage);
    }

    public void Heal(int healingAmount)
    {
        enemyHealth.RegenerateHealth(healingAmount);
    }

    public void ApplyShield(int shieldDurability)
    {
        enemyHealth.ApplyShield(shieldDurability);
    }

    public int GetCurrentShieldDurability()
    {
        return enemyHealth.GetCurrentShieldDurability();
    }
}
