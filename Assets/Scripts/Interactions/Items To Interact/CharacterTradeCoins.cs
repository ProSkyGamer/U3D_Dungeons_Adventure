using System;

public class CharacterTradeCoins : InteractableItem
{
    public static event EventHandler<OnStartCharacterTradeCoinsEventArgs> OnStartCharacterTradeCoins;

    private PlayerController playerToReceiveCoins;

    public class OnStartCharacterTradeCoinsEventArgs : EventArgs
    {
        public PlayerController characterToReceiveCoins;
    }

    private void Awake()
    {
        playerToReceiveCoins = GetComponent<PlayerController>();
    }

    public override void OnInteract(PlayerController player)
    {
        base.OnInteract(player);

        OnStartCharacterTradeCoins?.Invoke(this, new OnStartCharacterTradeCoinsEventArgs
        {
            characterToReceiveCoins = playerToReceiveCoins
        });
    }

    public override bool IsCanInteract()
    {
        return isCanInteract && !playerToReceiveCoins.IsOwner &&
               GameInput.Instance.GetBindingValue(GameInput.Binding.ShowCursor) == 1f;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        OnStartCharacterTradeCoins = null;
    }
}
