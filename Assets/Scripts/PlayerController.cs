using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Player Info")]
    public int playerIndex;

    [Header("State")]
    public bool isDead = false;
    public bool controlsEnabled = false;

    [Header("Respawn Settings")]
    [SerializeField] private float invulnerabilityDuration = 2f;
    private bool isInvulnerable = false;

    // Components
    private PlayerInput playerInput;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    // Events
    public event Action<int> OnPlayerDeath;

    // For your existing character controller
    private MonoBehaviour characterMovement; // Reference to your actual movement script

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // Get your existing movement/combat scripts
        // characterMovement = GetComponent<YourCharacterMovementScript>();
    }

    void Start()
    {
        // Start with controls disabled until match begins
        DisableControls();
    }

    public void EnableControls()
    {
        controlsEnabled = true;
        if (playerInput != null)
        {
            playerInput.ActivateInput();
        }
        // Enable your character movement script
        // if (characterMovement != null) characterMovement.enabled = true;
    }

    public void DisableControls()
    {
        controlsEnabled = false;
        if (playerInput != null)
        {
            playerInput.DeactivateInput();
        }
        // Disable your character movement script
        // if (characterMovement != null) characterMovement.enabled = false;
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        DisableControls();

        // Visual feedback
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        // Disable physics
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        // Trigger death event
        OnPlayerDeath?.Invoke(playerIndex);
    }

    public void Respawn(Vector3 position)
    {
        transform.position = position;
        isDead = false;

        // Reset physics
        if (rb != null)
        {
            rb.simulated = true;
            rb.linearVelocity = Vector2.zero;
        }

        // Visual feedback
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        EnableControls();
        StartInvulnerability();
    }

    void StartInvulnerability()
    {
        isInvulnerable = true;
        Invoke(nameof(EndInvulnerability), invulnerabilityDuration);

        // Visual feedback - flashing
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashSprite());
        }
    }

    void EndInvulnerability()
    {
        isInvulnerable = false;
        StopCoroutine(FlashSprite());

        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }
    }

    System.Collections.IEnumerator FlashSprite()
    {
        while (isInvulnerable)
        {
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = 0.3f;
                spriteRenderer.color = color;
            }

            yield return new WaitForSeconds(0.1f);

            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = 1f;
                spriteRenderer.color = color;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    public bool IsInvulnerable()
    {
        return isInvulnerable;
    }

    // This can be called by your combat system when a player takes lethal damage
    public void TakeDamage(float damage)
    {
        if (isInvulnerable || isDead) return;

        // Your damage logic here
        // If health <= 0, call Die()
    }

    // This can be called by your knockback system when percent gets too high
    public void ApplyKnockback(Vector2 force, float damagePercent)
    {
        if (isInvulnerable || isDead) return;

        // Your knockback logic here
        if (rb != null)
        {
            rb.AddForce(force, ForceMode2D.Impulse);
        }
    }
}