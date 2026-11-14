using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

[MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "Projectile")]
public class Projectile : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float referenceVisualRadius = 0.15f;

    private Transform owner;
    private Vector2 direction;

    private float damage;
    private float remainingDistance;
    private float speed;
    private float radius;

    private LayerMask targetLayers;
    private LayerMask obstacleLayers;

    private bool initialized;

    public void Initialize(
        Transform owner,
        Vector2 direction,
        float damage,
        float range,
        float speed,
        float radius,
        LayerMask targetLayers,
        LayerMask obstacleLayers
    )
    {
        this.owner = owner;
        this.direction = direction.normalized;
        this.damage = damage;

        remainingDistance = Mathf.Max(0f, range);
        this.speed = Mathf.Max(0.1f, speed);
        this.radius = Mathf.Max(0.01f, radius);

        this.targetLayers = targetLayers;
        this.obstacleLayers = obstacleLayers;

        transform.localScale *= this.radius / Mathf.Max(0.01f, referenceVisualRadius);

        initialized = true;

        if (remainingDistance <= 0f)
            Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        if (!initialized || remainingDistance <= 0f)
            return;

        float step = Mathf.Min(
            speed * Time.fixedDeltaTime,
            remainingDistance
        );

        Vector2 origin = transform.position;

        int collisionMask =
            targetLayers.value | obstacleLayers.value;

        RaycastHit2D[] hits = Physics2D.CircleCastAll(
            origin,
            radius,
            direction,
            step,
            collisionMask
        );

        // CircleCastAll возвращает попадания
        // по возрастанию расстояния.
        foreach (RaycastHit2D hit in hits)
        {
            Collider2D target = hit.collider;

            if (target == null)
                continue;

            // Пропускаем самого стрелявшего.
            if (owner != null &&
                (target.transform == owner || target.transform.IsChildOf(owner)))
            {
                continue;
            }

            // Пропускаем собственные коллайдеры,
            // если они были добавлены на префаб.
            if (target.transform == transform || target.transform.IsChildOf(transform))
                continue;

            int layerBit = 1 << target.gameObject.layer;

            bool isObstacle =
                (obstacleLayers.value & layerBit) != 0;

            bool isTarget =
                (targetLayers.value & layerBit) != 0;

            if (!isObstacle && !isTarget)
                continue;

            transform.position = new Vector3(
                hit.centroid.x,
                hit.centroid.y,
                transform.position.z
            );

            // Препятствие блокирует снаряд.
            if (!isObstacle && isTarget)
            {
                IDamageable damageable =
                    target.GetComponentInParent<IDamageable>();

                damageable?.TakeDamage(damage);
            }

            Destroy(gameObject);
            return;
        }

        transform.position +=
            (Vector3)(direction * step);

        remainingDistance -= step;

        if (remainingDistance <= 0f)
            Destroy(gameObject);
    }
}
