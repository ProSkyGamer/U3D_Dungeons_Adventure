using Unity.Netcode;
using UnityEngine;

public class InteractableItem : NetworkBehaviour
{
    #region Variables & References

    [SerializeField] protected bool isCanInteract = true;

    protected PlayerController interactedPlayer;

    #endregion

    #region Interactable Item

    public virtual void OnInteract(PlayerController player)
    {
        interactedPlayer = player;
    }

    public virtual bool IsCanInteract()
    {
        return isCanInteract;
    }

    #endregion
}
