using UnityEngine;

public class DeleteObjectAfterTime : MonoBehaviour
{
    [SerializeField] private float deleteAfterSeconds = 30f;

    private void Update()
    {
        deleteAfterSeconds -= Time.deltaTime;

        if (deleteAfterSeconds <= 0)
            Destroy(gameObject);
    }
}
