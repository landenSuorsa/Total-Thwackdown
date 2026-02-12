using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUIManager : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private GameObject stockDisplayContainer;
    [SerializeField] private GameObject stockDisplayPrefab;

    [Header("Match Flow UI")]
    [SerializeField] private GameObject countdownPanel;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private GameObject winnerPanel;
    [SerializeField] private TextMeshProUGUI winnerText;

    [Header("Stock Icon Settings")]
    [SerializeField] private Sprite stockIcon;
    [SerializeField] private Color[] playerColors = { Color.red, Color.blue, Color.yellow, Color.gray };

    private Dictionary<int, PlayerStockDisplay> stockDisplays = new Dictionary<int, PlayerStockDisplay>();

    public void InitializeStockDisplay(Dictionary<int, PlayerInfo> playerData)
    {
        foreach (Transform child in stockDisplayContainer.transform)
        {
            Destroy(child.gameObject);
        }
        stockDisplays.Clear();

        // Create stock display
        int index = 0;
        Debug.Log($"PlayerData count: {playerData.Count}");
        foreach (var kvp in playerData)
        {
            Debug.Log($"Creating stock display for player {kvp.Key}");
            GameObject stockDisplay = Instantiate(stockDisplayPrefab, stockDisplayContainer.transform);
            PlayerStockDisplay display = stockDisplay.GetComponent<PlayerStockDisplay>();

            if (display != null)
            {
                Color playerColor = index < playerColors.Length ? playerColors[index] : Color.white;
                display.Initialize(kvp.Key, kvp.Value.stocksRemaining, playerColor, kvp.Value.character);
                stockDisplays[kvp.Key] = display;
            }

            index++;
        }
    }

    public void UpdateStockDisplay(int playerIndex, int stocksRemaining)
    {
        if (stockDisplays.ContainsKey(playerIndex))
        {
            stockDisplays[playerIndex].UpdateStocks(stocksRemaining);
        }
    }

    public void ShowCountdown(float duration)
    {
        if (countdownPanel != null)
        {
            countdownPanel.SetActive(true);
            StartCoroutine(CountdownRoutine(duration));
        }
    }

    public void HideCountdown()
    {
        if (countdownPanel != null)
        {
            countdownPanel.SetActive(false);
        }
    }

    System.Collections.IEnumerator CountdownRoutine(float duration)
    {
        int count = Mathf.CeilToInt(duration);

        while (count > 0)
        {
            if (countdownText != null)
            {
                countdownText.text = count.ToString();
            }

            yield return new WaitForSeconds(1f);
            count--;
        }

        if (countdownText != null)
        {
            countdownText.text = "GO!";
        }

        yield return new WaitForSeconds(0.5f);
    }

    public void ShowWinner(int playerIndex)
    {
        if (winnerPanel != null)
        {
            winnerPanel.SetActive(true);

            if (winnerText != null)
            {
                winnerText.text = $"Player {playerIndex + 1} Wins!";

                if (playerIndex < playerColors.Length)
                {
                    winnerText.color = playerColors[playerIndex];
                }
            }
        }
    }

    public void HideWinner()
    {
        if (winnerPanel != null)
        {
            winnerPanel.SetActive(false);
        }
    }
}

