using System;
using UnityEngine;
using UnityEngine.UI;

public class CharacterUI : MonoBehaviour
{
    [SerializeField] private Button closeButton;

    [SerializeField] private Button statsTabButton;
    public static event EventHandler OnStatsTabButtonClick;

    [SerializeField] private Button upgradesTabButton;
    public static event EventHandler OnUpgradesTabButtonClick;

    [SerializeField] private Button weaponsTabButton;
    [SerializeField] private Button relicsTabButton;

    private bool isFirstUpdate = true;

    private void Awake()
    {
        closeButton.onClick.AddListener(Hide);

        statsTabButton.onClick.AddListener(() => { OnStatsTabButtonClick?.Invoke(this, EventArgs.Empty); });
        upgradesTabButton.onClick.AddListener(() => { OnUpgradesTabButtonClick?.Invoke(this, EventArgs.Empty); });
        weaponsTabButton.onClick.AddListener(() => { });
        relicsTabButton.onClick.AddListener(() => { });
    }

    private void Start()
    {
        GameInput.Instance.OnOpenCharacterInfoAction += GameInput_OnOpenCharacterInfoAction;
    }

    private void Update()
    {
        if (isFirstUpdate)
        {
            isFirstUpdate = false;
            Hide();
        }
    }

    private void GameInput_OnOpenCharacterInfoAction(object sender, EventArgs e)
    {
        Show();
    }

    private void Show()
    {
        gameObject.SetActive(true);

        OnStatsTabButtonClick?.Invoke(this, EventArgs.Empty);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}