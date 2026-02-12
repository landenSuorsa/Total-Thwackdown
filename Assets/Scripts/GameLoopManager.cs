using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    private Dictionary<int, PlayerMatchData> playerMatchData = new Dictionary<int, PlayerMatchData>();
    private List<GameObject> activePlayers = new List<GameObject>();

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

        int counter = 0;
        foreach (var playerSelection in matchSession.Players)
        {
            PlayerMatchData data = new PlayerMatchData
            {
                playerIndex = playerSelection.playerIndex,
                stocksRemaining = stocksPerPlayer,
                isEliminated = false,
                character = playerSelection.character
            };
            playerMatchData[playerSelection.playerIndex] = data;

            GameObject player = SpawnPlayer(playerSelection, counter);
            activePlayers.Add(player);

            counter++;
        }

        // Update UI
        if (uiManager != null)
        {
            uiManager.InitializeStockDisplay(playerMatchData);
        }
    }

    GameObject SpawnPlayer(PlayerSelection playerSelection, int spawnIndex)
    {
        GameObject newCharacter = UnityEngine.InputSystem.PlayerInput.Instantiate(
            playerSelection.character.gameplayPrefab,
            pairWithDevice: playerSelection.inputDev
        ).gameObject;

        if (spawnPoints != null && spawnIndex < spawnPoints.Count)
        {
            newCharacter.transform.position = spawnPoints[spawnIndex].position;
        }

        var playerController = newCharacter.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.playerIndex = playerSelection.playerIndex;
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
            var controller = player.GetComponent<PlayerController>();
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

    void HandlePlayerDeath(int playerIndex)
    {
        if (!playerMatchData.ContainsKey(playerIndex)) return;
        if (currentState != MatchState.InProgress) return;

        PlayerMatchData data = playerMatchData[playerIndex];
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
            var controller = p.GetComponent<PlayerController>();
            return controller != null && controller.playerIndex == playerIndex;
        });

        if (playerObj != null)
        {
            var controller = playerObj.GetComponent<PlayerController>();
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

        PlayerMatchData winner = playerMatchData.Values
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
            var controller = player.GetComponent<PlayerController>();
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

    public PlayerMatchData GetPlayerMatchData(int playerIndex)
    {
        if (playerMatchData.ContainsKey(playerIndex))
            return playerMatchData[playerIndex];
        return null;
    }
}

[System.Serializable]
public class PlayerMatchData
{
    public int playerIndex;
    public int stocksRemaining;
    public bool isEliminated;
    public CharacterData character;
}