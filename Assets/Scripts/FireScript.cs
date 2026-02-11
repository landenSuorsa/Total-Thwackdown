using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class FireScript : MonoBehaviour
{
    public List<Character> hitTargets = new List<Character> ();
    [SerializeField] AttackData attackData;
    [SerializeField] float lifetime = 3f;
    public float attackModifier = 1f;
    float timeCreated = -1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timeCreated = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - timeCreated > lifetime)
        {
            Destroy(gameObject);
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.TryGetComponent(out Character target)) return;
        if (hitTargets.Contains(target)) return;

        hitTargets.Add(target);
        target.TakeDamage(attackData, 1, attackModifier);
    }
}
