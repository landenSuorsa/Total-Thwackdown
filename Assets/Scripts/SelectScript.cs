using System.Collections.Generic;
using UnityEditor.U2D.Animation;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Linq;
using UnityEngine.WSA;
using Unity.VisualScripting;
using TMPro;
using UnityEngine.SceneManagement;

public abstract class SelectScript : MonoBehaviour
{
    [SerializeField] public GameObject cursorPrefab;
    [SerializeField] public Transform[] cursorSlots;   // 4 texts

    public List<PlayerSlot> players = new();

    public abstract void MoveCursor(PlayerSlot player, int direction);

    public abstract void LockIn(PlayerSlot player);

    public Transform[] GetSlots()
    {
        return cursorSlots;
    }

    public void AddPlayer(PlayerSlot player)
    {
        players.Add(player);
    }
}
