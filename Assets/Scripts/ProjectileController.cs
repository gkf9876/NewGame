using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public sealed class ProjectileController : MonoBehaviour
{
    [SerializeField] private float damage = 1f;
    [SerializeField] private float speed = 8f;
    [SerializeField] private float lifetime = 2f;
    [SerializeField] private float scale = 1f;
    [SerializeField] private LayerMask enemyLayer;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveDirection = Vector2.right;
    private Quaternion baseRotation = Quaternion.identity;
    private float spawnTime;
    private bool isInitialized;

    public float Damage
    {
        get => damage;
        set => damage = Mathf.Max(0f, value);
    }

    public float Speed
    {
        get => speed;
        set => speed = Mathf.Max(0f, value);
    }

    public float Lifetime
    {
        get => lifetime;
        set => lifetime = Mathf.Max(0f, value);
    }

    public float Scale
    {
        get => scale;
        set => scale = Mathf.Max(0.01f, value);
    }

    private void Reset()
    {
        enemyLayer = LayerMask.GetMask("Enemy");
        EnsureRequiredComponents();
        ApplyPhysicsSettings();
    }

    private void OnValidate()
    {
        damage = Mathf.Max(0f, damage);
        speed = Mathf.Max(0f, speed);
        lifetime = Mathf.Max(0f, lifetime);
        scale = Mathf.Max(0.01f, scale);

        if (!Application.isPlaying)
        {
            EnsureRequiredComponents();
            ApplyPhysicsSettings();
            ApplyScale();
        }
    }

    private void Awake()
    {
        EnsureRequiredComponents();
        ApplyPhysicsSettings();
    }

    private void OnEnable()
    {
        spawnTime = Time.time;
        ApplyScale();

        if (!isInitialized)
        {
            Initialize(Vector2.right, transform.rotation);
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = moveDirection * speed;
    }

    private void Update()
    {
        if (Time.time >= spawnTime + lifetime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamageEnemy(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDamageEnemy(collision.gameObject);
    }

    public void Initialize(Vector2 direction, Quaternion referenceRotation)
    {
        if (direction.sqrMagnitude <= Mathf.Epsilon)
        {
            direction = Vector2.right;
        }

        moveDirection = direction.normalized;
        baseRotation = referenceRotation;
        spawnTime = Time.time;
        isInitialized = true;

        ApplyScale();
        ApplyVisualDirection();

        if (rb != null)
        {
            rb.linearVelocity = moveDirection * speed;
        }
    }

    private void TryDamageEnemy(GameObject target)
    {
        if (!IsInLayerMask(target.layer, enemyLayer))
        {
            return;
        }

        target.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
        Destroy(gameObject);
    }

    private void EnsureRequiredComponents()
    {
        if (!TryGetComponent(out rb))
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        TryGetComponent(out spriteRenderer);

        Collider2D projectileCollider = GetComponent<Collider2D>();
        if (projectileCollider == null)
        {
            projectileCollider = gameObject.AddComponent<CircleCollider2D>();
        }

        if (projectileCollider != null)
        {
            projectileCollider.isTrigger = true;
        }
    }

    private void ApplyPhysicsSettings()
    {
        if (rb == null)
        {
            return;
        }

        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.freezeRotation = true;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void ApplyScale()
    {
        transform.localScale = Vector3.one * scale;
    }

    private void ApplyVisualDirection()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = moveDirection.x < -Mathf.Epsilon;
        }

        bool flipLeft = moveDirection.x < -Mathf.Epsilon;
        float angle = Mathf.Atan2(moveDirection.y, Mathf.Abs(moveDirection.x)) * Mathf.Rad2Deg;
        if (flipLeft)
        {
            angle = -angle;
        }

        transform.rotation = baseRotation * Quaternion.Euler(0f, 0f, angle);
    }

    private static bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return (layerMask.value & (1 << layer)) != 0;
    }
}
