using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InteractButtonSingleUI : MonoBehaviour
{
    #region Variables & References

    [SerializeField] private TextTranslationSingleUI buttonTextTranslationSingleUI;
    [SerializeField] private Transform bindingButtonText;

    private Button interactButton;

    #endregion

    #region Initialization

    private void Awake()
    {
        interactButton = GetComponent<Button>();
    }

    #endregion

    #region Update

    private void Update()
    {
        bindingButtonText.gameObject.SetActive(interactButton.gameObject ==
                                               EventSystem.current.currentSelectedGameObject);
    }

    #endregion

    #region Button Initialization

    public void InitializeButton(AddInteractButtonUI interactButtonUI, TextTranslationsSO textTranslationsSo)
    {
        buttonTextTranslationSingleUI.ChangeTextTranslationSO(textTranslationsSo);

        interactButton.onClick.AddListener(interactButtonUI.OnInteract);
    }

    #endregion

    #region Get Button Data

    public Button GetInteractButton()
    {
        return interactButton;
    }

    #endregion
}
