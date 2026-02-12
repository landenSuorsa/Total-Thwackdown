using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ComputerScript : Character
{
    [SerializeField] float teleUpDistance = 7f;
    [SerializeField] GameObject firePrefab;
    [SerializeField] GameObject error101;
    [SerializeField] GameObject error200;
    [SerializeField] GameObject error301;
    [SerializeField] ParticleSystem particles;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void StartAttack(int attackState)
    {
        _inAttack = true;
        _currentAttackState = attackState;

        _anim.SetInteger("AttackState", attackState);
        DisableAllHitboxes();

        if (_hitboxMap.TryGetValue(attackState, out var hitbox))
            hitbox.enabled = true;

        switch (attackState)
        {
            case 11: // forward
                ErrorAttack();
                break;
            case 12: // up
                if (canUpSpecial)
                {
                    canUpSpecial = false;
                    TeleportUp();
                }
                break;
            case 13: // down
                FireAttack();
                break;
            case 14: // neut
                PowerUp();
                break;
        }
    }

    private void TeleportUp()
    {
        RaycastHit2D hit = Physics2D.Raycast(_rb.position, Vector2.up, teleUpDistance, 1 << 6);
        if (hit.collider)
        {
            _rb.position = hit.point - new Vector2(0, _bc.size.y);
        } else
        {
            _rb.position = new Vector2(_rb.position.x, _rb.position.y + teleUpDistance - _bc.size.y);
        }
        _rb.linearVelocityY = 0;
        _rb.gravityScale = 0;
        Invoke("ResetGravity", 1f);
    }

    private void ErrorAttack()
    {
        int num = Random.Range(0, 3);
        switch (num) {
            case 0:
                Error101Attack();
                break;
            case 1:
                Error200Attack();
                break;
            case 2:
                Error301Attack();
                break;
        }
    }

    private void Error101Attack()
    {
        GameObject error = Instantiate(error101, _rb.position + new Vector2(isFacingRight ? 1 : -1, 0), Quaternion.identity);
        error.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(isFacingRight ? 3 : -3, 0);
        error.GetComponent<Error101Script>().hitTargets.Add(this);
        error.GetComponent<Error101Script>().attackData.knockback_dir *= new Vector2(isFacingRight ? 1 : -1, 0);
    }

    private void Error200Attack()
    {
        GameObject error = Instantiate(error200, _rb.position + new Vector2(isFacingRight ? 1 : -1, 0), Quaternion.identity);
        error.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(isFacingRight ? 6 : -6, 0);
        error.GetComponent<Error200Script>().hitTargets.Add(this);
        error.GetComponent<Error200Script>().attackData.knockback_dir *= new Vector2(isFacingRight ? 1 : -1, 0);
    }

    private void Error301Attack()
    {
        GameObject error = Instantiate(error301, _rb.position + new Vector2(isFacingRight ? 1 : -1, 0), Quaternion.identity);
        error.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(isFacingRight ? 6 : -6, 0);
        error.GetComponent<Error301Script>().hitTargets.Add(this);
        error.GetComponent<Error301Script>().attackData.knockback_dir *= new Vector2(isFacingRight ? 1 : -1, 0);
    }

    private void FireAttack()
    {
        GameObject error = Instantiate(firePrefab, _rb.position + new Vector2(isFacingRight ? 1 : -1, -_bc.size.y / 2), Quaternion.identity);
        error.GetComponent<FireScript>().hitTargets.Add(this);
        error.GetComponent<FireScript>().attackData.knockback_dir *= new Vector2(isFacingRight ? 1 : -1, 0);
    }

    private void PowerUp()
    {
        attackModifier = 1.35f;
        StartCoroutine("DamageTicks");
        particles.Play();
    }

    private IEnumerator DamageTicks()
    {
        AttackData attackData = new AttackData();
        attackData.damageAmount = 1f;
        attackData.stunAmount = 0;
        attackData.knockback_dir = Vector2.zero;
        for (int i = 0; i < 5; i++)
        {
            TakeDamage(attackData, 1, 1);
            yield return new WaitForSeconds(2.5f);
        }
        particles.Stop();
        attackModifier = 1f;
    }

    private void ResetGravity()
    {
        _rb.gravityScale = gravity;
    }
}
