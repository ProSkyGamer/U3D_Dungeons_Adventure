using System.Collections.Generic;
using UnityEngine;

public class AddInteractButtonUI : MonoBehaviour
{
    [SerializeField] private float interactDistance = 2.5f;
    [SerializeField] private LayerMask playerLayer;

    private readonly List<InteractableItem> interactableItemsList = new();

    private bool isHasInteractButtonOnScreen;

    private void Awake()
    {
        interactableItemsList.AddRange(GetComponents<InteractableItem>());
    }

    private void Update()
    {
        if (IsAnyItemInteractable())
        {
            var castPosition = transform.position;
            var castCubeLength = new Vector3(interactDistance, interactDistance, interactDistance);

            var raycastHits = Physics.BoxCastAll(castPosition, castCubeLength,
                Vector3.forward, Quaternion.identity, interactDistance, playerLayer);

            foreach (var hit in raycastHits)
                if (hit.transform.gameObject.TryGetComponent(out PlayerController player))
                {
                    if (!isHasInteractButtonOnScreen)
                    {
                        InteractUI.Instance.AddButtonInteractToScreen(this);
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

    public void OnInteract()
    {
        foreach (var interactableItem in interactableItemsList)
            if (interactableItem.IsCanInteract())
                interactableItem.OnInteract(PlayerController.Instance);

        InteractUI.Instance.RemoveButtonInteractToScreen(this);
    }

    private void OnDestroy()
    {
        if (isHasInteractButtonOnScreen)
            InteractUI.Instance.RemoveButtonInteractToScreen(this);
    }

    private bool IsAnyItemInteractable()
    {
        foreach (var interactableItem in interactableItemsList)
            if (interactableItem.IsCanInteract())
                return true;

        return false;
    }
}
