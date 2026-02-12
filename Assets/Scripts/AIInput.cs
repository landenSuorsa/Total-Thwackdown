using System.Collections.Generic;
using UnityEngine;

public class AIInput : MonoBehaviour, IInputProvider
{
    private InputState _input;

    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private LayerMask groundLayer = 1 << 6;

    private Character _character;

    // Cooldowns
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float specialCooldown = 2f;
    [SerializeField] private float jumpCooldown = 1.5f;

    private float nextAttackTime = 0f;
    private float nextSpecialTime = 0f;
    private float nextJumpTime = 0f;

    // Movement pause after attack
    [SerializeField] private float postAttackPause = 0.5f;
    private float movePauseEndTime = 0f;

    private float stageBottomY = -5f; // adjust to your stage's bottom
    [SerializeField] private float edgeFearDistance = 1f; // how far from edge to start fearing

    // ---------------- Target switching ----------------
    [SerializeField] private float switchTargetInterval = 3f;
    private float nextSwitchTime = 0f;
    private GameObject currentTarget = null;

    void Awake()
    {
        _character = GetComponent<Character>();
    }

    void Update()
    {
        if (_character == null) return;

        _input = new InputState();

        // ---------------- Target selection ----------------
        if (Time.time >= nextSwitchTime || currentTarget == null)
        {
            currentTarget = PickRandomTarget();
            if (currentTarget != null)
            {
                nextSwitchTime = Time.time + switchTargetInterval;
            }
        }

        if (currentTarget == null) return;

        Vector2 toTarget = (Vector2)(currentTarget.transform.position - transform.position);

        // ---------------- MOVEMENT ----------------
        float moveDir = Mathf.Sign(toTarget.x);

        // Apply edge fear: move away from edges if too close
        moveDir = AdjustForEdges(moveDir);

        // Stop movement if paused
        if (Time.time < movePauseEndTime)
            moveDir = 0;

        // Stop movement if ground is not ahead
        if (!GroundAhead(moveDir))
            moveDir = 0;

        _input.Move.x = moveDir;

        // Vertical movement for attack direction
        _input.Move.y = 0;
        if (Mathf.Abs(toTarget.y) > 1f)
            _input.Move.y = Mathf.Sign(toTarget.y);

        // ---------------- JUMP ----------------
        if (Time.time >= nextJumpTime && ShouldJump(currentTarget, moveDir))
        {
            _input.JumpPressed = true;
            nextJumpTime = Time.time + jumpCooldown;
        }

        // ---------------- NORMAL ATTACK ----------------
        if (Time.time >= nextAttackTime &&
            (Mathf.Abs(toTarget.x) <= attackRange || Mathf.Abs(toTarget.y) > 1f))
        {
            _input.NormalPressed = true;
            nextAttackTime = Time.time + attackCooldown;

            movePauseEndTime = Time.time + postAttackPause;
        }

        // ---------------- SPECIAL ATTACK ----------------
        if (Time.time >= nextSpecialTime)
        {
            if (_character.transform.position.y < stageBottomY + 1f && !GroundAhead(0))
            {
                _input.SpecialPressed = true;
                _input.Move.y = 1; // up special
                nextSpecialTime = Time.time + specialCooldown;

                movePauseEndTime = Time.time + postAttackPause;
            }
            else if (_character._isGrounded && Random.value < 0.02f)
            {
                _input.SpecialPressed = true;
                _input.Move.y = 0; // neutral special
                nextSpecialTime = Time.time + specialCooldown;

                movePauseEndTime = Time.time + postAttackPause;
            }
        }
    }

    public InputState GetInput() => _input;

    // ---------------- HELPER FUNCTIONS ----------------
    private GameObject PickRandomTarget()
    {
        var allChars = _character.stage.characters;
        List<GameObject> validTargets = new List<GameObject>();

        foreach (var c in allChars)
        {
            if (c != gameObject && c.GetComponent<Character>().inMatch) validTargets.Add(c);
        }

        if (validTargets.Count == 0) return null;

        int index = Random.Range(0, validTargets.Count);
        return validTargets[index];
    }

    // Move away from edges if no ground ahead
    private float AdjustForEdges(float desiredMove)
    {
        float moveDir = desiredMove;

        // Check 0.5–1 unit ahead for ground
        bool groundLeft = GroundAhead(-1);
        bool groundRight = GroundAhead(1);

        // If moving left but no ground, move right
        if (moveDir < 0 && !groundLeft)
            moveDir = 1f;

        // If moving right but no ground, move left
        if (moveDir > 0 && !groundRight)
            moveDir = -1f;

        // Optional: If both sides are cliffs (cornered), stop
        if (!groundLeft && !groundRight)
            moveDir = 0f;

        return moveDir;
    }


    private bool GroundAhead(float moveDir)
    {
        if (moveDir == 0) return true;

        Vector2 origin = (Vector2)transform.position + new Vector2(moveDir * 0.5f, 0);
        float rayLength = 1.5f; // farther for early warning
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, rayLength, groundLayer);
        return hit.collider != null;
    }


    private bool ShouldJump(GameObject target, float moveDir)
    {
        if (moveDir != 0)
        {
            Vector2 origin = transform.position;
            Vector2 dir = new Vector2(moveDir, 0);
            if (Physics2D.Raycast(origin, dir, 0.5f, groundLayer))
                return true;
        }

        float yDiff = target.transform.position.y - transform.position.y;
        if (yDiff > 1f)
            return true;

        return false;
    }
}
