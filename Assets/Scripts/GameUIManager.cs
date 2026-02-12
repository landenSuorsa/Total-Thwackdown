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

    public void InitializeStockDisplay(Dictionary<int, PlayerMatchData> playerData)
    {
        foreach (Transform child in stockDisplayContainer.transform)
        {
            Destroy(child.gameObject);
        }
        stockDisplays.Clear();

        // Create stock display
        int index = 0;
        foreach (var kvp in playerData)
        {
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

public class PlayerStockDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private Image playerPortrait;
    [SerializeField] private Transform stockIconContainer;
    [SerializeField] private GameObject stockIconPrefab;
    [SerializeField] private Image backgroundImage;

    private int playerIndex;
    private List<GameObject> stockIcons = new List<GameObject>();

    public void Initialize(int index, int stocks, Color playerColor, CharacterData character)
    {
        playerIndex = index;

        // player name
        if (playerNameText != null)
        {
            playerNameText.text = $"P{index + 1}";
            playerNameText.color = playerColor;
        }

        // portrait
        if (playerPortrait != null && character != null && character.portrait != null)
        {
            playerPortrait.sprite = character.portrait;
        }

        // background color
        if (backgroundImage != null)
        {
            Color bgColor = playerColor;
            bgColor.a = 0.3f;
            backgroundImage.color = bgColor;
        }

        // stock icons
        CreateStockIcons(stocks, playerColor);
    }

    void CreateStockIcons(int count, Color color)
    {
        foreach (GameObject icon in stockIcons)
        {
            Destroy(icon);
        }
        stockIcons.Clear();

        for (int i = 0; i < count; i++)
        {
            GameObject icon = Instantiate(stockIconPrefab, stockIconContainer);
            Image iconImage = icon.GetComponent<Image>();

            if (iconImage != null)
            {
                iconImage.color = color;
            }

            stockIcons.Add(icon);
        }
    }

    public void UpdateStocks(int stocksRemaining)
    {
        for (int i = 0; i < stockIcons.Count; i++)
        {
            stockIcons[i].SetActive(i < stocksRemaining);
        }

        // Gray out if eliminated!
        if (stocksRemaining <= 0 && backgroundImage != null)
        {
            Color grayedOut = Color.gray;
            grayedOut.a = 0.5f;
            backgroundImage.color = grayedOut;
        }
    }
}