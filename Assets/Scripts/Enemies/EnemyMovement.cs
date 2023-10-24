using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    private Transform playerTransform;

    private void Awake()
    {
        playerTransform = transform;
    }

    public void Move(Vector2 toMove)
    {
        var moveVector = transform.TransformDirection(new Vector3(toMove.x, 0, toMove.y));

        playerTransform.position += moveVector;
    }
}
