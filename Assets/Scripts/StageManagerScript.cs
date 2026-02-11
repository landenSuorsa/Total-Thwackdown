using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Android;

public class StageManagerScript : MonoBehaviour
{
    private MatchSessionScript _sessionScript;
    [SerializeField] private List<Transform> spawnPoints;
    List<GameObject> characters;
    [SerializeField] GameObject playerUI;
    [SerializeField] Canvas canvas;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _sessionScript = FindFirstObjectByType<MatchSessionScript>();
        characters = _sessionScript.LoadPlayersGame(spawnPoints);
        var counter = 0;
        foreach (GameObject character in characters)
        {
            GameObject ui = Instantiate(playerUI);
            ui.transform.parent = canvas.transform;
            ui.transform.position = new Vector2(-200 + 100 * counter++, -50);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
