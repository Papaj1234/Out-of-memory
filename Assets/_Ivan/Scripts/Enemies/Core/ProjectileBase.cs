using UnityEngine;

public class ProjectileBase : MonoBehaviour
{
    [SerializeField] protected float speed = 5f;
    [SerializeField] protected float lifeTime = 5f;

    protected Vector2 direction;

    public void Initialize(Vector2 direction)
    {
        this.direction = direction.normalized;

        Destroy(gameObject, lifeTime);
    }

    protected virtual void Update()
    {
        transform.position +=
            (Vector3)(direction *
                      speed *
                      Time.deltaTime);
    }
}