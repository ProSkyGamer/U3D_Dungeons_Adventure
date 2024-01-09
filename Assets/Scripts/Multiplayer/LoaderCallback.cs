using Unity.Netcode;

public class LoaderCallback : NetworkBehaviour
{
    #region Initialization

    private void Start()
    {
        Loader.LoaderCallback();
    }

    #endregion
}
