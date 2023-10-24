using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;

    private Transform playerTransform;

    private void Awake()
    {
        playerTransform = transform;
    }

    public void Move(Vector2 toMove)
    {
        if (cameraTransform.localEulerAngles.y != playerTransform.localEulerAngles.y)
        {
            transform.rotation = cameraTransform.rotation;
            playerTransform.localEulerAngles = new Vector3(0f, playerTransform.localEulerAngles.y, 0f);
        }

        var moveVector = transform.TransformDirection(new Vector3(toMove.x, 0, toMove.y));

        playerTransform.position += moveVector;
        cameraTransform.position += moveVector;
    }
}
