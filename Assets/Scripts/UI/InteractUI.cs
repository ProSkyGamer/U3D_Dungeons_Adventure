using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InteractUI : MonoBehaviour
{
    public static InteractUI Instance { get; private set; }

    private readonly List<Button> allInteractButtonsList = new();
    private readonly List<AddInteractButtonUI> allInteractableItemButtonsList = new();

    [SerializeField] private Transform interactButtonPrefab;
    [SerializeField] private Transform buttonsLayoutGroup;

    private Button activeButton;
    private readonly float timeBetweenScrolls = 0.1f;
    private float timerBetweenScrolls;

    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;
    }

    private void Start()
    {
        Hide();

        GiveCoinsUI.OnInterfaceShown += GiveCoinsUI_OnInterfaceShown;
        GiveCoinsUI.OnInterfaceHidden += GiveCoinsUI_OnInterfaceHidden;
    }

    private void GiveCoinsUI_OnInterfaceHidden(object sender, EventArgs e)
    {
        Show();
    }

    private void GiveCoinsUI_OnInterfaceShown(object sender, EventArgs e)
    {
        Hide();
    }

    public void Update()
    {
        if (GameStageManager.Instance.IsPause()) return;

        if (allInteractButtonsList.Count > 0)
        {
            if (EventSystem.current.currentSelectedGameObject != activeButton.gameObject) activeButton.Select();

            if (allInteractButtonsList.Count > 1)
            {
                if (timerBetweenScrolls <= 0)
                {
                    var mouseScroll = GameInput.Instance.GetMouseScroll();
                    if (mouseScroll != 0)
                    {
                        var isScrollUp = mouseScroll > 0;

                        for (var i = 0; i < allInteractButtonsList.Count; i++)
                            if (activeButton == allInteractButtonsList[i])
                            {
                                if (isScrollUp)
                                    activeButton = i != allInteractButtonsList.Count - 1
                                        ? allInteractButtonsList[i + 1]
                                        : allInteractButtonsList[0];
                                else
                                    activeButton = i != 0
                                        ? allInteractButtonsList[i - 1]
                                        : allInteractButtonsList[allInteractButtonsList.Count - 1];

                                timerBetweenScrolls = timeBetweenScrolls;
                                break;
                            }
                    }
                }
                else
                {
                    timerBetweenScrolls -= Time.deltaTime;
                }
            }
        }
    }

    public void AddButtonInteractToScreen(
        AddInteractButtonUI interactButtonUI, TextTranslationsSO textTranslationsSo)
    {
        if (allInteractButtonsList.Count == 0)
            Show();

        var interactableItemButtonTransform = Instantiate(interactButtonPrefab, buttonsLayoutGroup);
        interactableItemButtonTransform.gameObject.name = interactButtonUI.gameObject.name;
        var interactButtonSingle = interactableItemButtonTransform.GetComponent<InteractButtonSingleUI>();
        interactButtonSingle.InitializeButton(interactButtonUI, textTranslationsSo);
        var interactableItemButton = interactButtonSingle.GetInteractButton();

        if (activeButton == null)
            activeButton = interactableItemButton;

        allInteractButtonsList.Add(interactableItemButton);
        allInteractableItemButtonsList.Add(interactButtonUI);

        if (allInteractButtonsList.Count == 1)
            GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
    }

    public void RemoveButtonInteractToScreen(AddInteractButtonUI interactButtonUI)
    {
        for (var i = 0; i < allInteractableItemButtonsList.Count; i++)
            if (allInteractableItemButtonsList[i] == interactButtonUI)
            {
                if (activeButton == allInteractButtonsList[i])
                    if (allInteractButtonsList.Count != 1)
                        activeButton = i != 0 ? allInteractButtonsList[i - 1] : allInteractButtonsList[i + 1];

                Destroy(allInteractButtonsList[i].gameObject);
                allInteractButtonsList.RemoveAt(i);
                allInteractableItemButtonsList.RemoveAt(i);

                if (allInteractButtonsList.Count == 0)
                {
                    activeButton = null;
                    Hide();
                }

                break;
            }
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if (GameStageManager.Instance.IsPause()) return;

        for (var i = 0; i < allInteractButtonsList.Count; i++)
            if (allInteractButtonsList[i] == activeButton)
                allInteractableItemButtonsList[i].OnInteract();
    }

    private void Hide()
    {
        gameObject.SetActive(false);

        GameInput.Instance.OnInteractAction -= GameInput_OnInteractAction;
    }

    public bool IsShow()
    {
        return gameObject.activeSelf;
    }
}
