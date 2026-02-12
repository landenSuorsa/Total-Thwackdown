using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class Character : MonoBehaviour
{
    public Rigidbody2D _rb;
    public BoxCollider2D _bc;
    public Animator _anim;

    public StageManagerScript stage;

    private float _damageTaken;

    [SerializeField] private float damageToKnockbackRatio = 0.5f;

    [Header("Movement")]
    [SerializeField] private Transform _visualRoot;
    [SerializeField] public Transform UI;
    [SerializeField] private float MOVEMENT_SPEED = 5f;
    [SerializeField] private float JUMP_FORCE = 5f;
    [SerializeField] private float JUMP_REDUCTION = 0.8f;
    [SerializeField] private float STICK_THRESHOLD = 0.1f;

    [Header("Attack Hitboxes")]
    [SerializeField] private List<AttackScript> attackHitboxes;

    [Header("Knockback Bounce")]
    [SerializeField] private float bounceDamping = 0.8f; 
    [SerializeField] private float minBounceSpeed = 2f;


    public Dictionary<int, AttackScript> _hitboxMap;

    public bool _isGrounded;
    private InputState _input;
    public bool _inAttack;
    private bool _isStunned;
    public float attackModifier = 1f;
    public bool isFacingRight = true;
    public bool canUpSpecial = true;
    public float gravity;
    public IInputProvider _inputProvider;
    public bool inMatch = true;


    public int _currentAttackState;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _bc = GetComponent<BoxCollider2D>();
        _anim = _visualRoot.GetComponent<Animator>();
        gravity = _rb.gravityScale;

        _hitboxMap = new Dictionary<int, AttackScript>();
        foreach (var hb in attackHitboxes)
        {
            if (hb == null) continue;
            hb.enabled = false;
            _hitboxMap[hb.attackState] = hb;
        }
    }

    private void Update()
    {
        SetInput(_inputProvider.GetInput());
        HandleAnimationState();
        UpdateAttackLifecycle();

        if (Mathf.Abs(_input.Move.x) > 0.05f)
        {
            isFacingRight = _input.Move.x >= 0 ? true : false;
            _visualRoot.localScale = new Vector3(_input.Move.x >= 0 ? 1 : -1, 1, 1);
        }
    }

    private void FixedUpdate()
    {
        _isGrounded = Grounded();
        if (_isGrounded)
        {
            canUpSpecial = true;
        }
        HandleVelocity();
        HandleAttack();
    }

    // ================= ATTACK SYSTEM =================

    void HandleAttack()
    {
        if (_inAttack || _isStunned) return;

        int attackState = 0;

        if (_input.NormalPressed)
        {
            _input.NormalPressed = false;
            attackState = DirectionToAttackState(_input.Move);
        }
        else if (_input.SpecialPressed)
        {
            _input.SpecialPressed = false;
            attackState = 10 + DirectionToAttackState(_input.Move);
        }

        if (attackState > 0)
            StartAttack(attackState);
    }

    public virtual void StartAttack(int attackState)
    {
         
        _inAttack = true;
        _currentAttackState = attackState;

        _anim.SetInteger("AttackState", attackState);
        DisableAllHitboxes();

        if (_hitboxMap.TryGetValue(attackState, out var hitbox))
            hitbox.enabled = true;
    }

    void UpdateAttackLifecycle()
    {
        if (!_inAttack) return;

        var state = _anim.GetCurrentAnimatorStateInfo(0);

        if (!state.IsTag("Attack")) return;

        if (state.normalizedTime >= 0.95f)
            EndAttack();
    }

    private void EndAttack()
    {
        _inAttack = false;
        _currentAttackState = 0;

        _anim.SetInteger("AttackState", 0);
        DisableAllHitboxes();
    }

    public void DisableAllHitboxes()
    {
        foreach (var hb in _hitboxMap.Values)
            hb.enabled = false;
    }

    // ================= DAMAGE =================

    public void TakeDamage(AttackData data, float dir, float attackModifier)
    {
        _damageTaken += data.damageAmount * attackModifier;
        print(_damageTaken);
        ApplyStun(data.stunAmount);
        ApplyKnockback(data.damageAmount, new Vector2(data.knockback_dir.x * dir, data.knockback_dir.y));
        stage.UpdateHealth(_damageTaken, gameObject);
    }

    private void ApplyStun(float duration)
    {
        _isStunned = true;
        DisableAllHitboxes();
        _inAttack = false;

        _anim.SetInteger("AttackState", 0);
        _anim.SetTrigger("IsStunned");

        Invoke(nameof(RemoveStun), duration);
    }

    private void RemoveStun() => _isStunned = false;

    private void ApplyKnockback(float baseDamage, Vector2 dir)
    {
        dir = dir.normalized;

        // Scale knockback with accumulated damage (Smash-style curve)
        float knockback =
            baseDamage * damageToKnockbackRatio *
            Mathf.Pow(1f + _damageTaken * 0.01f, 0.75f);

        // Cancel opposing momentum only
        Vector2 v = _rb.linearVelocity;

        if (Mathf.Sign(v.x) != Mathf.Sign(dir.x))
            v.x = 0f;

        if (dir.y > 0f && v.y < 0f)
            v.y = 0f;

        _rb.linearVelocity = v;

        // Apply impulse force
        _rb.linearVelocity = Vector2.zero;
        _rb.AddForce(dir * knockback, ForceMode2D.Impulse);

    }


    // ================= MOVEMENT =================

    private void HandleAnimationState()
    {
        if (_inAttack) return;

        _anim.SetBool("IsJumping", _rb.linearVelocity.y > 0.05f);
        _anim.SetBool("IsFalling", _rb.linearVelocity.y < -0.05f);
        _anim.SetBool("IsMoving", Mathf.Abs(_rb.linearVelocity.x) > 0.05f);
    }

    private void HandleVelocity()
    {
        if (_isStunned) return;

        _rb.linearVelocity = new Vector2(_input.Move.x * MOVEMENT_SPEED, _rb.linearVelocity.y);

        if (_isGrounded && _input.JumpPressed)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, JUMP_FORCE);
            _input.JumpPressed = false;
        }
        else if (_input.JumpReleased && _rb.linearVelocity.y > 0.01f)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _rb.linearVelocity.y * JUMP_REDUCTION);
        }
    }

    private int DirectionToAttackState(Vector2 move)
    {
        if (move.y > STICK_THRESHOLD) return 2;
        if (move.y < -STICK_THRESHOLD) return 3;
        if (Mathf.Abs(move.x) > STICK_THRESHOLD) return 1;
        return 4;
    }

    // ================= INPUT =================

    public void SetInput(InputState input)
    {
        _input = input;
    }

    // ================= GROUND =================

    private bool Grounded()
    {
        Vector2 size = new(_bc.size.x * 0.9f, 0.1f);
        Vector2 center = (Vector2)_bc.bounds.center +
                         Vector2.down * (_bc.bounds.extents.y - size.y * 0.5f);

        return Physics2D.BoxCast(center, size, 0f, Vector2.down, 0.1f, 1 << 6);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!_isStunned) return;

        // Only bounce off solid level geometry
        if (((1 << collision.gameObject.layer) & LayerMask.GetMask("Ground")) == 0)
            return;

        HandleBounce(collision);
    }

    private void HandleBounce(Collision2D collision)
    {
        Vector2 velocity = _rb.linearVelocity;

        // Get strongest contact normal
        Vector2 normal = collision.contacts[0].normal;

        // Reflect velocity
        Vector2 reflected = Vector2.Reflect(velocity, normal);

        // Apply damping
        reflected *= bounceDamping;

        // Prevent micro-bouncing
        if (reflected.magnitude < minBounceSpeed)
            reflected = Vector2.zero;

        _rb.linearVelocity = reflected;
    }


}

public struct InputState
{
    public Vector2 Move;
    public bool JumpPressed;
    public bool JumpReleased;
    public bool NormalPressed;
    public bool SpecialPressed;
}

public interface IInputProvider
{
    InputState GetInput();
}