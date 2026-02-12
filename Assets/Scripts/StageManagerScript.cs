using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.InputSystem.Android;
using UnityEngine.UI;

public class StageManagerScript : MonoBehaviour
{
    private MatchSessionScript _sessionScript;
    [SerializeField] private List<Transform> spawnPoints;
    public List<GameObject> characters;
    [SerializeField] List<RectTransform> playerUIs;
    List<Image> playerPanels = new List<Image>();
    List<Image> playerPortraits = new List<Image>();
    List<TextMeshProUGUI> playerDamages = new List<TextMeshProUGUI>();
    List<List<Image>> playerStocks = new List<List<Image>>();

    private Color[] colors = { Color.red, Color.blue, Color.yellow, Color.gray };

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _sessionScript = FindFirstObjectByType<MatchSessionScript>();
        characters = _sessionScript.LoadPlayersGame(spawnPoints);
        var counter = 0;
        
        foreach (GameObject character in characters)
        {
            playerPanels.Add(playerUIs[counter].GetChild(0).gameObject.GetComponent<Image>());
            playerPortraits.Add(playerUIs[counter].GetChild(1).gameObject.GetComponent<Image>());
            playerDamages.Add(playerUIs[counter].GetChild(2).gameObject.GetComponent<TextMeshProUGUI>());

            List<Image> newList = new List<Image>();
            for (int i = 0; i < 3; i++)
            {
                newList.Add(playerUIs[counter].GetChild(3).GetChild(i).gameObject.GetComponent<Image>());
            }
            playerStocks.Add(newList);

            var portrait = _sessionScript.Players[counter].character.portrait;
            playerUIs[counter].gameObject.SetActive(true);
            playerPanels[counter].color = new Color(colors[counter].r, colors[counter].g, colors[counter].b, 0.3f);
            playerPortraits[counter].sprite = portrait;
            foreach (var stock in playerStocks[counter])
            {
                stock.sprite = portrait;
            }

            counter++;
            character.GetComponent<Character>().stage = this;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateHealth(float damageAmount, GameObject character)
    {
        for (int i = 0; i < characters.Count; i++)
        {
            if (characters[i] == character)
            {
                playerDamages[i].text = damageAmount.ToString("F1") + "%";
                break;
            }
        }
    }
}
