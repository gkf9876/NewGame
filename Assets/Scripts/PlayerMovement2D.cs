using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[DisallowMultipleComponent]
public sealed class PlayerMovement2D : MonoBehaviour
{
    private enum FacingAxis
    {
        Horizontal,
        Vertical
    }

    [SerializeField] private float moveSpeed = 3f;

    private Animator animator;
    private Rigidbody2D rb;
    private BoxCollider2D hitbox;

    private Vector2 rawInput;
    private Vector2 movementInput;
    private Vector2 facingDirection = Vector2.down;
    private FacingAxis lastFacingAxis = FacingAxis.Vertical;

    private static readonly int MoveXHash = Animator.StringToHash("moveX");
    private static readonly int MoveYHash = Animator.StringToHash("moveY");
    private static readonly int IsMovingHash = Animator.StringToHash("isMoving");

    private void Reset()
    {
        EnsureRequiredComponents();
        ApplyPhysicsSettings();
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            EnsureRequiredComponents();
            ApplyPhysicsSettings();
        }

        moveSpeed = Mathf.Max(0f, moveSpeed);
    }

    private void Awake()
    {
        EnsureRequiredComponents();
        ApplyPhysicsSettings();

        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        hitbox = GetComponent<BoxCollider2D>();

        UpdateAnimator(false);
    }

    private void Update()
    {
        rawInput = ReadMovementInput();
        movementInput = rawInput.sqrMagnitude > 1f ? rawInput.normalized : rawInput;

        if (rawInput != Vector2.zero)
        {
            facingDirection = ResolveFacingDirection(rawInput);
        }

        UpdateAnimator(rawInput != Vector2.zero);
    }

    private void FixedUpdate()
    {
        if (movementInput == Vector2.zero)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.MovePosition(rb.position + movementInput * (moveSpeed * Time.fixedDeltaTime));
    }

    private void EnsureRequiredComponents()
    {
        if (!TryGetComponent(out animator))
        {
            animator = gameObject.AddComponent<Animator>();
        }

        if (!TryGetComponent(out rb))
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        if (!TryGetComponent(out hitbox))
        {
            hitbox = gameObject.AddComponent<BoxCollider2D>();
        }
    }

    private void ApplyPhysicsSettings()
    {
        if (rb == null)
        {
            return;
        }

        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.freezeRotation = true;
        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private Vector2 ResolveFacingDirection(Vector2 input)
    {
        bool hasHorizontal = !Mathf.Approximately(input.x, 0f);
        bool hasVertical = !Mathf.Approximately(input.y, 0f);

        if (hasHorizontal && hasVertical)
        {
            bool keepHorizontal = !Mathf.Approximately(facingDirection.x, 0f) &&
                Mathf.Sign(facingDirection.x) == Mathf.Sign(input.x);
            bool keepVertical = !Mathf.Approximately(facingDirection.y, 0f) &&
                Mathf.Sign(facingDirection.y) == Mathf.Sign(input.y);

            if (keepHorizontal && !keepVertical)
            {
                lastFacingAxis = FacingAxis.Horizontal;
                return new Vector2(Mathf.Sign(input.x), 0f);
            }

            if (keepVertical && !keepHorizontal)
            {
                lastFacingAxis = FacingAxis.Vertical;
                return new Vector2(0f, Mathf.Sign(input.y));
            }

            if (lastFacingAxis == FacingAxis.Horizontal)
            {
                return new Vector2(Mathf.Sign(input.x), 0f);
            }

            return new Vector2(0f, Mathf.Sign(input.y));
        }

        if (hasHorizontal)
        {
            lastFacingAxis = FacingAxis.Horizontal;
            return new Vector2(Mathf.Sign(input.x), 0f);
        }

        lastFacingAxis = FacingAxis.Vertical;
        return new Vector2(0f, Mathf.Sign(input.y));
    }

    private void UpdateAnimator(bool isMoving)
    {
        animator.SetBool(IsMovingHash, isMoving);
        animator.SetFloat(MoveXHash, facingDirection.x);
        animator.SetFloat(MoveYHash, facingDirection.y);
    }

    private static Vector2 ReadMovementInput()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            float horizontal =
                (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1f : 0f) -
                (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1f : 0f);
            float vertical =
                (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1f : 0f) -
                (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1f : 0f);

            return new Vector2(horizontal, vertical);
        }
#endif

        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }
}
