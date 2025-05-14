using UnityEngine;

public class AppleAttackAnimation : MonoBehaviour
{
    public float attackScale = 1.3f;      // How much to scale up during attack
    public float attackDuration = 0.2f;   // How long the attack animation lasts
    public float attackCooldown = 1f;     // Cooldown between attacks

    private Vector3 originalScale;
    private bool isAttacking = false;
    private float attackTimer = 0f;
    private float lastAttackTime = -Mathf.Infinity;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        if (isAttacking)
        {
            attackTimer += Time.deltaTime;
            float t = attackTimer / attackDuration;
            if (t < 0.5f)
            {
                // Scale up
                transform.localScale = Vector3.Lerp(originalScale, originalScale * attackScale, t * 2);
            }
            else if (t < 1f)
            {
                // Scale back down
                transform.localScale = Vector3.Lerp(originalScale * attackScale, originalScale, (t - 0.5f) * 2);
            }
            else
            {
                // End attack
                transform.localScale = originalScale;
                isAttacking = false;
            }
        }
    }

    public void TriggerAttack()
    {
        if (!isAttacking && Time.time >= lastAttackTime + attackCooldown)
        {
            isAttacking = true;
            attackTimer = 0f;
            lastAttackTime = Time.time;
        }
    }
} 