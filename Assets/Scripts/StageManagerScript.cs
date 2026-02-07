using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class StageManagerScript : MonoBehaviour
{
    private MatchSessionScript _sessionScript;
    [SerializeField] private List<Transform> spawnPoints;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _sessionScript = FindFirstObjectByType<MatchSessionScript>();
        _sessionScript.LoadPlayersGame(spawnPoints);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
