using Unity.Netcode;
using UnityEngine;

public class DeleteObjectAfterTime : NetworkBehaviour
{
    #region Variables

    [SerializeField] private float deleteAfterSeconds = 30f;

    #endregion

    #region Update

    private void Update()
    {
        if (!IsServer) return;

        deleteAfterSeconds -= Time.deltaTime;

        if (deleteAfterSeconds <= 0)
            Destroy(gameObject);
    }

    #endregion
}
