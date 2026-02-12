using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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