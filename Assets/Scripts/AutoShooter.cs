using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[DisallowMultipleComponent]
public sealed class AutoShooter : MonoBehaviour
{
    [SerializeField] private ProjectileController projectilePrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform baseRotationReference;
    [SerializeField] private float fireInterval = 0.35f;
    [SerializeField] private float minTurnCooldown = 0.12f;

    private Vector2 lastFireDirection = Vector2.right;
    private Vector2 previousInputDirection = Vector2.right;
    private float nextFireTime;
    private float turnCooldownUntil;

    private void OnValidate()
    {
        fireInterval = Mathf.Max(0.01f, fireInterval);
        minTurnCooldown = Mathf.Max(0f, minTurnCooldown);
    }

    private void Update()
    {
        Vector2 inputDirection = ReadMovementInput();
        if (inputDirection.sqrMagnitude > Mathf.Epsilon)
        {
            inputDirection.Normalize();
            UpdateLastFireDirection(inputDirection);
        }

        if (Time.time >= nextFireTime && Time.time >= turnCooldownUntil)
        {
            Fire();
        }
    }

    private void UpdateLastFireDirection(Vector2 inputDirection)
    {
        if (Vector2.Dot(previousInputDirection, inputDirection) < 0.999f)
        {
            turnCooldownUntil = Time.time + minTurnCooldown;
        }

        previousInputDirection = inputDirection;
        lastFireDirection = inputDirection;
    }

    private void Fire()
    {
        if (projectilePrefab == null)
        {
            return;
        }

        Transform origin = spawnPoint != null ? spawnPoint : transform;
        Quaternion referenceRotation = baseRotationReference != null
            ? baseRotationReference.rotation
            : Quaternion.identity;

        ProjectileController projectile = Instantiate(
            projectilePrefab,
            origin.position,
            referenceRotation);

        projectile.Initialize(lastFireDirection, referenceRotation);
        nextFireTime = Time.time + fireInterval;
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
