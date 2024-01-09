using System;

public class CharacterTradeCoins : InteractableItem
{
    #region Event & Event Args

    public static event EventHandler<OnStartCharacterTradeCoinsEventArgs> OnStartCharacterTradeCoins;

    public class OnStartCharacterTradeCoinsEventArgs : EventArgs
    {
        public PlayerController characterToReceiveCoins;
    }

    #endregion

    private PlayerController playerToReceiveCoins;

    #region Initialization

    private void Awake()
    {
        playerToReceiveCoins = GetComponent<PlayerController>();
    }

    #endregion

    #region Interact Item

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

    #endregion

    public static void ResetStaticData()
    {
        OnStartCharacterTradeCoins = null;
    }
}
