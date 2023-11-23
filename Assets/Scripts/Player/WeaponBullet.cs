using System;
using UnityEngine;

public class WeaponBullet : MonoBehaviour
{
    public event EventHandler<OnEnemyHittedEventArgs> OnEnemyHitted;

    public class OnEnemyHittedEventArgs : EventArgs
    {
        public EnemyController hittedEnemy;
    }

    [SerializeField] private float bulletSpeed = 12.5f;

    private float remainingLifetime = -1f;
    private bool isInitialized;
    private int bulletDamage;
    private PlayerController createdPlayer;
    private LayerMask enemiesLayers;

    public void InitializeBullet(float bulletDirection,
        float lifetime, int damage, PlayerController player, LayerMask enemiesLayer)
    {
        remainingLifetime = lifetime;
        transform.Rotate(0f, bulletDirection, 0f);
        bulletDamage = damage;
        createdPlayer = player;
        enemiesLayers = enemiesLayer;

        isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized) return;

        transform.position += transform.TransformDirection(bulletSpeed * Time.deltaTime, 0f, 0f);

        var castPosition = transform.position;
        var boxCastSide = 0.05f;
        var boxCastLength = new Vector3(boxCastSide + bulletSpeed * Time.deltaTime * 2,
            boxCastSide + bulletSpeed * Time.deltaTime * 2, boxCastSide + bulletSpeed * Time.deltaTime * 2);
        var raycastHit = Physics.BoxCast(castPosition, boxCastLength,
            transform.forward, out var hitInfo, Quaternion.identity, boxCastSide);

        if (raycastHit)
        {
            if (hitInfo.transform.TryGetComponent<EnemyController>(out var enemyController))
            {
                OnEnemyHitted?.Invoke(this, new OnEnemyHittedEventArgs
                {
                    hittedEnemy = enemyController
                });

                enemyController.ReceiveDamage(bulletDamage, createdPlayer);
            }

            Destroy(gameObject);
        }

        remainingLifetime -= Time.deltaTime;

        if (remainingLifetime <= 0)
            Destroy(gameObject);
    }
}
