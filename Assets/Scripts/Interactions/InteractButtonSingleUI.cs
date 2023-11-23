using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InteractButtonSingleUI : MonoBehaviour
{
    [SerializeField] private TextTranslationSingleUI buttonTextTranslationSingleUI;
    [SerializeField] private Transform bindingButtonText;

    private Button interactButton;

    private void Awake()
    {
        interactButton = GetComponent<Button>();
    }

    private void Update()
    {
        bindingButtonText.gameObject.SetActive(interactButton.gameObject ==
                                               EventSystem.current.currentSelectedGameObject);
    }

    public void InitializeButton(AddInteractButtonUI interactButtonUI, TextTranslationsSO textTranslationsSo)
    {
        buttonTextTranslationSingleUI.ChangeTextTranslationSO(textTranslationsSo);

        interactButton.onClick.AddListener(interactButtonUI.OnInteract);
    }

    public Button GetInteractButton()
    {
        return interactButton;
    }
}
