using UnityEngine;

public class Dash : MonoBehaviour
{
    public float dashSpeed = 20f;         // How fast the dash is
    public float dashDuration = 0.2f;     // How long the dash lasts
    public float dashCooldown = 1f;       // Cooldown between dashes
    public KeyCode dashKey = KeyCode.LeftShift;

    private CharacterController characterController;
    private bool isDashing = false;
    private float dashTime;
    private float lastDashTime = -Mathf.Infinity;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (!isDashing && Time.time >= lastDashTime + dashCooldown && Input.GetKeyDown(dashKey))
        {
            StartDash();
        }

        if (isDashing)
        {
            DashMove();
        }
    }

    void StartDash()
    {
        isDashing = true;
        dashTime = 0f;
        lastDashTime = Time.time;
    }

    void DashMove()
    {
        dashTime += Time.deltaTime;
        Vector3 dashDirection = GetInputDirection();

        // If no input, fallback to forward
        if (dashDirection == Vector3.zero)
            dashDirection = transform.forward;

        characterController.Move(dashDirection.normalized * dashSpeed * Time.deltaTime);

        if (dashTime >= dashDuration)
        {
            isDashing = false;
        }
    }

    Vector3 GetInputDirection()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(h, 0, v);

        // Convert local to world direction
        direction = transform.TransformDirection(direction);

        return direction;
    }
} 