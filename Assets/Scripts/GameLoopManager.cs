using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;
using static UnityEditor.Experimental.GraphView.GraphView;

public class GameLoopManager : MonoBehaviour
{
    public static GameLoopManager Instance;

    [Header("Match Settings")]
    [SerializeField] private int stocksPerPlayer = 3;
    [SerializeField] private float respawnDelay = 2f;
    [SerializeField] private float matchStartDelay = 3f;
    [SerializeField] private float matchEndDelay = 3f;

    [Header("References")]
    [SerializeField] private List<Transform> spawnPoints;
    [SerializeField] private GameUIManager uiManager;

    private Dictionary<int, PlayerInfo> playerMatchData = new Dictionary<int, PlayerInfo>();
    private List<GameObject> activePlayers = new List<GameObject>();

    [SerializeField] private Color[] colors = { Color.red, Color.blue, Color.yellow, Color.gray };

    public enum MatchState { PreMatch, InProgress, PostMatch }
    public MatchState currentState { get; private set; }

    private MatchSessionScript matchSession;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        matchSession = FindFirstObjectByType<MatchSessionScript>();
        StartCoroutine(InitializeMatch());
    }

    IEnumerator InitializeMatch()
    {
        currentState = MatchState.PreMatch;

        // Show countdown
        if (uiManager != null)
        {
            uiManager.ShowCountdown(matchStartDelay);
        }

        SpawnAllPlayers();

        yield return new WaitForSeconds(matchStartDelay);

        StartMatch();
    }

    void SpawnAllPlayers()
    {
        if (matchSession == null || matchSession.Players == null) return;

        var counter = 0;
        foreach (PlayerInfo player in matchSession.Players)
        {
            player.stocksRemaining = stocksPerPlayer;
            player.isEliminated = false;
            playerMatchData.Add(counter, player);

            activePlayers.Add(SpawnPlayer(player, counter++));
        }

        // Update UI
        if (uiManager != null)
        {
            uiManager.InitializeStockDisplay(playerMatchData);
        }
    }

    GameObject SpawnPlayer(PlayerInfo player, int spawnIndex)
    {
        GameObject newCharacter;
        if (player.inputDev != null)
        {
            newCharacter = PlayerInput.Instantiate(player.character.gameplayPrefab, pairWithDevice: player.inputDev).gameObject;
            newCharacter.AddComponent<HumanInput>();
            var characterScript = newCharacter.GetComponent<Character>();
            characterScript._inputProvider = newCharacter.GetComponent<HumanInput>();
            newCharacter.GetComponent<PlayerInput>().enabled = true;
            characterScript.UI.GetComponentInChildren<Image>().color = colors[spawnIndex];
            characterScript.UI.GetComponentInChildren<TextMeshProUGUI>().text = "P" + (spawnIndex + 1);
        }
        else
        {
            newCharacter = Instantiate(player.character.gameplayPrefab);
            newCharacter.AddComponent<AIInput>();
            var characterScript = newCharacter.GetComponent<Character>();
            characterScript._inputProvider = newCharacter.GetComponent<AIInput>();
            characterScript.UI.GetComponentInChildren<Image>().color = colors[spawnIndex];
            characterScript.UI.GetComponentInChildren<TextMeshProUGUI>().text = "C" + (spawnIndex + 1);
        }

        if (spawnPoints != null && spawnIndex < spawnPoints.Count)
        {
            newCharacter.transform.position = spawnPoints[spawnIndex].position;
        }

        var playerController = newCharacter.GetComponent<Character>();
        if (playerController != null)
        {
            playerController.playerIndex = player.playerIndex;
            playerController.OnPlayerDeath += HandlePlayerDeath;
        }

        return newCharacter;
    }

    void StartMatch()
    {
        currentState = MatchState.InProgress;
        Debug.Log("Match Started!");

        // Enable player controls
        foreach (var player in activePlayers)
        {
            var controller = player.GetComponent<Character>();
            if (controller != null)
            {
                controller.EnableControls();
            }
        }

        if (uiManager != null)
        {
            uiManager.HideCountdown();
        }
    }

    public List<GameObject> GetActivePlayers () => activePlayers;

    void HandlePlayerDeath(int playerIndex)
    {
        if (!playerMatchData.ContainsKey(playerIndex)) return;
        if (currentState != MatchState.InProgress) return;

        PlayerInfo data = playerMatchData[playerIndex];
        data.stocksRemaining--;

        Debug.Log($"Player {playerIndex} died! Stocks remaining: {data.stocksRemaining}");

        // Update UI
        if (uiManager != null)
        {
            uiManager.UpdateStockDisplay(playerIndex, data.stocksRemaining);
        }

        if (data.stocksRemaining <= 0)
        {
            data.isEliminated = true;
            Debug.Log($"Player {playerIndex} eliminated!");

            CheckMatchEnd();
        }
        else
        {
            // Respawn 
            StartCoroutine(RespawnPlayer(playerIndex));
        }
    }

    IEnumerator RespawnPlayer(int playerIndex)
    {
        yield return new WaitForSeconds(respawnDelay);

        if (currentState != MatchState.InProgress) yield break;

        GameObject playerObj = activePlayers.Find(p =>
        {
            var controller = p.GetComponent<Character>();
            return controller != null && controller.playerIndex == playerIndex;
        });

        if (playerObj != null)
        {
            var controller = playerObj.GetComponent<Character>();
            if (controller != null)
            {
                int spawnIndex = matchSession.Players.FindIndex(p => p.playerIndex == playerIndex);
                if (spawnIndex >= 0 && spawnIndex < spawnPoints.Count)
                {
                    controller.Respawn(spawnPoints[spawnIndex].position);
                }
            }
        }
    }

    void CheckMatchEnd()
    {
        // Count remaining 
        int playersAlive = playerMatchData.Values.Count(p => !p.isEliminated);

        if (playersAlive <= 1)
        {
            EndMatch();
        }
    }

    void EndMatch()
    {
        currentState = MatchState.PostMatch;

        PlayerInfo winner = playerMatchData.Values
            .Where(p => !p.isEliminated)
            .OrderByDescending(p => p.stocksRemaining)
            .FirstOrDefault();

        if (winner != null)
        {
            Debug.Log($"Player {winner.playerIndex} wins!");

            if (uiManager != null)
            {
                uiManager.ShowWinner(winner.playerIndex);
            }
        }
        foreach (var player in activePlayers)
        {
            var controller = player.GetComponent<Character>();
            if (controller != null)
            {
                controller.DisableControls();
            }
        }

        StartCoroutine(ReturnToMenu());
    }

    IEnumerator ReturnToMenu()
    {
        yield return new WaitForSeconds(matchEndDelay);

        if (matchSession != null)
        {
            matchSession.ClearPlayers();
        }

        // Return to select
        SceneManager.LoadScene("Local CharSelect");
    }

    // Public method for death zones to call
    public void PlayerFellOffStage(int playerIndex)
    {
        HandlePlayerDeath(playerIndex);
    }

    public PlayerInfo GetPlayerMatchData(int playerIndex)
    {
        if (playerMatchData.ContainsKey(playerIndex))
            return playerMatchData[playerIndex];
        return null;
    }
}
