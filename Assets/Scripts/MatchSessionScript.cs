using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Windows;
using static UnityEditor.Experimental.GraphView.GraphView;

public class MatchSessionScript : MonoBehaviour
{
    public static MatchSessionScript Instance;
    public SelectScript script;

    private Color[] colors = { Color.red, Color.blue, Color.yellow, Color.gray };

    public List<PlayerSelection> Players = new();

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddPlayer(PlayerSelection player)
    {
        Players.Add(player);
    }

    public void ClearPlayers()
    {
        Players.Clear();
    }

    // loads for UI selection (cursors)
    public void LoadPlayers(GameObject playerControlledPrefab)
    {
        script = FindFirstObjectByType<SelectScript>();
        foreach (PlayerSelection player in Players)
        {
            if (player.inputDev == null) return;
            var input = PlayerInput.Instantiate(playerControlledPrefab, pairWithDevice: player.inputDev);
            var behaviour = input.GetComponent<PlayerSlotBehaviour>();

            // Initialize the PlayerSlotBehaviour
            behaviour.Init(script, input, player.playerIndex, player.playerIndex);

            // Add to the player list
            script.AddPlayer(behaviour.slot);
            
        }
    }

    public List<GameObject> LoadPlayersGame(List<Transform> spawnPoints) 
    {
        var counter = 0;
        var toReturn = new List<GameObject>();
        foreach (PlayerSelection player in Players)
        {
            GameObject newCharacter;
            if (player.inputDev != null)
            {
                newCharacter = PlayerInput.Instantiate(player.character.gameplayPrefab, pairWithDevice: player.inputDev).gameObject;
                newCharacter.AddComponent<HumanInput>();
                var characterScript = newCharacter.GetComponent<Character>();
                characterScript._inputProvider = newCharacter.GetComponent<HumanInput>();
                newCharacter.GetComponent<PlayerInput>().enabled = true;
                characterScript.UI.GetComponentInChildren<Image>().color = colors[counter];
                characterScript.UI.GetComponentInChildren<TextMeshProUGUI>().text = "P" + (counter + 1);
            }
            else
            {
                newCharacter = Instantiate(player.character.gameplayPrefab);
                newCharacter.AddComponent<AIInput>();
                var characterScript = newCharacter.GetComponent<Character>();
                characterScript._inputProvider = newCharacter.GetComponent<AIInput>();
                characterScript.UI.GetComponentInChildren<Image>().color = colors[counter];
                characterScript.UI.GetComponentInChildren<TextMeshProUGUI>().text = "C" + (counter + 1);
            }
            toReturn.Add(newCharacter);
            if (spawnPoints != null && spawnPoints.Count >= 4)
            {
                newCharacter.transform.position = spawnPoints[counter++].position;
            }
        }
        return toReturn;
    }
}

public class PlayerSelection
{
    public InputDevice inputDev;
    public int playerIndex;
    public CharacterData character;
}