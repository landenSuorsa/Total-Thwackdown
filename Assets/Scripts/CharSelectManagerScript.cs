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
using UnityEditor;

public class CharSelectManagerScript : SelectScript
{
    [SerializeField] private Image[] characterImages; // 4
    [SerializeField] private Image[] slotImages; // 4 images
    [SerializeField] private TextMeshProUGUI[] slotTexts;   // 4 texts
    [SerializeField] public List<CharacterData> dataList;
    private MatchSessionScript matchSession;

    public void Awake()
    {
        matchSession = FindFirstObjectByType<MatchSessionScript>();
        int count = 0;
        if (dataList != null && characterImages != null)
        {
            foreach (CharacterData character in dataList)
            {
                characterImages[count++].sprite = character.portrait;
            }
        }
    }

    public void OnPlayerJoined(PlayerInput input)
    {
        if (!(input.devices[0] is Gamepad) && !(input.devices[0] is Keyboard))
        {
            Debug.Log("Ignoring non-gamepad device: " + input.devices[0]);
            Destroy(input.gameObject);
            return;
        }
        // Get the PlayerSlotBehaviour from the prefab instance spawned by PlayerInputManager
        var behaviour = input.GetComponent<PlayerSlotBehaviour>();

        // Initialize the PlayerSlotBehaviour
        behaviour.Init(this, input, players.Count, players.Count);

        // Update UI text for this player
        if (players.Count < slotTexts.Length)
            slotTexts[players.Count].text = "Choosing...";

        // Add to the player list
        players.Add(behaviour.slot);
    }

    public override void MoveCursor(PlayerSlot player, int direction)
    {
        print("move cursor ");
        if (player.lockedIn) return;

        player.cursorIndex =
            (player.cursorIndex + direction + dataList.Count) % dataList.Count;
    }

    public override void LockIn(PlayerSlot player)
    {
        if (!player.lockedIn)
        {
            player.lockedIn = true;
            player.selected = (CharacterData)dataList[player.cursorIndex];
            slotTexts[player.playerIndex].text = "";
            slotImages[player.playerIndex].sprite = ((CharacterData)player.selected).portrait;

            if (players.All(p => p.lockedIn))
            {
                StartMatch();
            }
        } else
        {
            player.lockedIn = false;
            player.selected = null;
            slotTexts[player.playerIndex].text = "Choosing...";
            slotImages[player.playerIndex].sprite = null;
        }
    }

    void StartMatch()
    {
        foreach (PlayerSlot player in players)
        {
            matchSession.AddPlayer(new PlayerSelection { inputDev = player.input.devices[0], character = (CharacterData)player.selected, playerIndex = player.playerIndex });
        }
        // TODO: Transitiony stuff (fade out or something)
        SceneManager.LoadScene("Stage Select");

    }
}


public class PlayerSlot
{
    public PlayerInput input;
    public int cursorIndex;
    public int playerIndex;
    public Data selected;
    public bool lockedIn;
}

[System.Serializable]
public class CharacterData : Data
{
    public string characterName;
    public Sprite portrait;
    public GameObject gameplayPrefab;
}

[System.Serializable]
public class Data
{

}