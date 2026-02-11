using System.Collections.Generic;
using UnityEngine;

public class AttackScript : MonoBehaviour
{
    public int attackState;
    public AttackData attackData;

    private GameObject _visualRoot;
    private Collider2D _col;
    private HashSet<Character> _hitTargets = new();
    public Character character;

    private void Awake()
    {
        _col = GetComponent<Collider2D>();
        _col.enabled = false;
        _visualRoot = transform.parent.parent.gameObject;
    }

    private void OnEnable()
    {
        _hitTargets.Clear();
        _col.enabled = true;
    }

    private void OnDisable()
    {
        _col.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent(out Character target)) return;
        if (_hitTargets.Contains(target)) return;

        _hitTargets.Add(target);
        target.TakeDamage(attackData, _visualRoot.transform.localScale.x, character.attackModifier);
    }
}

[System.Serializable]
public struct AttackHitbox
{
    public int attackState;
    public AttackData attackData;
}

[System.Serializable]
public struct AttackData
{
    public float damageAmount;
    public Vector2 knockback_dir;
    public float stunAmount;
}