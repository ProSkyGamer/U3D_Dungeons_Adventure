using System.Collections.Generic;
using UnityEngine;

public class AddInteractButtonUI : MonoBehaviour
{
    #region Variables & References

    [SerializeField] private float interactDistance = 2.5f;
    [SerializeField] private TextTranslationsSO buttonTextTranslationsSo;

    private readonly List<InteractableItem> interactableItemsList = new();

    private bool isHasInteractButtonOnScreen;

    private bool isPlayerSpawned;

    #endregion

    #region Initialization

    private void Awake()
    {
        interactableItemsList.AddRange(GetComponents<InteractableItem>());
    }

    #endregion

    #region Update

    private void Update()
    {
        if (!SpawnPlayers.isAllPlayersSpawned) return;

        if (IsAnyItemInteractable())
        {
            var castPosition = transform.position;
            var castCubeLength = new Vector3(interactDistance, interactDistance, interactDistance);

            var raycastHits = Physics.BoxCastAll(castPosition, castCubeLength,
                Vector3.forward, Quaternion.identity, interactDistance);

            foreach (var hit in raycastHits)
                if (hit.transform.gameObject.TryGetComponent(out PlayerController playerController) &&
                    playerController.IsOwner)
                {
                    if (!isHasInteractButtonOnScreen)
                    {
                        InteractUI.Instance.AddButtonInteractToScreen(this, buttonTextTranslationsSo);
                        isHasInteractButtonOnScreen = true;
                    }

                    return;
                }
        }

        if (isHasInteractButtonOnScreen)
        {
            InteractUI.Instance.RemoveButtonInteractToScreen(this);
            isHasInteractButtonOnScreen = false;
        }
    }

    #endregion

    #region Interact Button Methods

    public void OnInteract()
    {
        foreach (var interactableItem in interactableItemsList)
            if (interactableItem.IsCanInteract())
                interactableItem.OnInteract(PlayerController.Instance);

        InteractUI.Instance.RemoveButtonInteractToScreen(this);
        isHasInteractButtonOnScreen = false;
    }

    public void ChangeButtonText(TextTranslationsSO textTranslationsSo)
    {
        buttonTextTranslationsSo = textTranslationsSo;
    }

    private bool IsAnyItemInteractable()
    {
        foreach (var interactableItem in interactableItemsList)
            if (interactableItem.IsCanInteract())
                return true;

        return false;
    }

    #endregion

    public void OnDestroy()
    {
        if (isHasInteractButtonOnScreen)
            InteractUI.Instance.RemoveButtonInteractToScreen(this);
    }
}
