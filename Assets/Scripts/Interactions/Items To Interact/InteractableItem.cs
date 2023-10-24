using UnityEngine;

public class InteractableItem : MonoBehaviour
{
    [SerializeField] protected bool isCanInteract = true;

    protected PlayerController interactedPlayer;

    public virtual void OnInteract(PlayerController player)
    {
        interactedPlayer = player;
    }

    public virtual bool IsCanInteract()
    {
        return isCanInteract;
    }
}
