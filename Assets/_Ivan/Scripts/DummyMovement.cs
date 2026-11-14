using UnityEngine;

public class DummyMovement : MonoBehaviour
{
    [SerializeField]
    private float speed = 5f;

    private void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector2 direction =
            new Vector2(horizontal, vertical).normalized;

        transform.position +=
            (Vector3)(direction * speed * Time.deltaTime);
    }
}