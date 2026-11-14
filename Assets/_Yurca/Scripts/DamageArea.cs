using UnityEngine;

public class DamageArea : MonoBehaviour
{
    private static Sprite circleSprite;

    private Transform owner;
    private LayerMask targetLayers;

    private float radius;
    private float damage;
    private float interval;
    private float expiresAt;
    private float nextTick;

    public void Initialize(
        Transform owner,
        float radius,
        float damage,
        float duration,
        float interval,
        LayerMask targetLayers,
        Color tint
    )
    {
        this.owner = owner;
        this.radius = Mathf.Max(0.01f, radius);
        this.damage = Mathf.Max(0f, damage);
        this.interval = Mathf.Max(0.05f, interval);
        this.targetLayers = targetLayers;

        expiresAt = Time.time + Mathf.Max(0.01f, duration);
        nextTick = Time.time;

        CreateVisual(tint);
    }

    private void Update()
    {
        if (Time.time >= expiresAt)
        {
            Destroy(gameObject);
            return;
        }

        if (Time.time < nextTick)
            return;

        nextTick = Time.time + interval;
        DealDamage();
    }

    private void DealDamage()
    {
        DamageUtility.DealArea(
            transform.position, radius, Vector2.right, 360f,
            damage, targetLayers, owner);
    }

    private void CreateVisual(Color tint)
    {
        if (circleSprite == null)
        {
            const int size = 64;

            Texture2D texture = new Texture2D(
                size,
                size,
                TextureFormat.RGBA32,
                false
            );

            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 point = new Vector2(
                        (x + 0.5f) / size * 2f - 1f,
                        (y + 0.5f) / size * 2f - 1f
                    );

                    float alpha = Mathf.Clamp01(
                        (1f - point.magnitude) * size
                    );

                    pixels[y * size + x] =
                        new Color(1f, 1f, 1f, alpha);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            circleSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                size
            );
        }

        GameObject visual = new GameObject("AreaVisual");
        visual.transform.SetParent(transform, false);

        SpriteRenderer renderer =
            visual.AddComponent<SpriteRenderer>();

        renderer.sprite = circleSprite;
        renderer.color = tint;
        renderer.sortingOrder = 20;

        visual.transform.localScale =
            Vector3.one * radius * 2f;
    }
}