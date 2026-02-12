using UnityEngine;

[CreateAssetMenu(menuName = "Character/CharacterData")]
public class CharacterData : Data
{
    [Header("Character Info")]
    public string characterName;
    public Sprite portrait;

    [Header("Gameplay")]
    public GameObject gameplayPrefab;

    [Header("Stats")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public int maxJumps = 2;
    public float weight = 1f; // Affects knockback
}

// Base class for all data scriptable objects
public abstract class Data : ScriptableObject
{
    // Base class that both CharacterData and StageData inherit from
}