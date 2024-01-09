using TMPro;
using UnityEngine;

public class EnemyStatsUI : MonoBehaviour
{
    #region Vatiables & References

    [SerializeField] private EnemyController followingEnemyStats;

    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI healedText;
    [SerializeField] private TextMeshProUGUI damagedText;
    [SerializeField] private TextMeshProUGUI shieldText;

    private EnemyHealth enemyHealth;

    #endregion

    #region Initalization & Subscribed events

    private void Awake()
    {
        enemyHealth = followingEnemyStats.GetComponent<EnemyHealth>();
    }

    private void Start()
    {
        enemyHealth.OnEnemyHealthChange += EnemyHealth_OnEnemyHealthChange;
    }

    private void EnemyHealth_OnEnemyHealthChange(object sender, EnemyHealth.OnEnemyHealthChangeEventArgs e)
    {
        healthText.text = $"{e.currentHealth} / {e.maxHealth}";

        if (e.lostHealth != 0)
        {
            damagedText.gameObject.SetActive(true);
            damagedText.text = $"-{e.lostHealth}";
        }
        else
        {
            damagedText.gameObject.SetActive(false);
        }

        if (e.obtainedHealth != 0)
        {
            healedText.gameObject.SetActive(true);
            healedText.text = $"+{e.obtainedHealth}";
        }
        else
        {
            healedText.gameObject.SetActive(false);
        }
    }

    #endregion
}
