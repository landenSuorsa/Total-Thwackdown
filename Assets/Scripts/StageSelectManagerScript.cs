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

public class StageSelectManagerScript : SelectScript
{
    [SerializeField] private Image[] stageImages; // 4
    public List<StageData> dataList;
    private MatchSessionScript matchSession;

    public void Awake()
    {
        matchSession = FindFirstObjectByType<MatchSessionScript>();
        matchSession.LoadPlayers(cursorPrefab);
        var counter = 0;
        foreach (Image stageImage in stageImages)
        {
            stageImage.sprite = dataList[counter++].portrait;
        }
    }

    public override void MoveCursor(PlayerSlot player, int direction)
    {
        if (player.lockedIn) return;

        player.cursorIndex =
            (player.cursorIndex + direction + dataList.Count) % dataList.Count;
    }

    public override void LockIn(PlayerSlot player)
    {
        if (!player.lockedIn)
        {
            player.lockedIn = true;
            player.selected = dataList[player.cursorIndex];

            if (players.All(p => p.lockedIn))
            {
                StartMatch();
            }
        }
        else
        {
            player.lockedIn = false;
            player.selected = null;
        }
    }

    public override void Back(PlayerSlot player)
    {
        if (!player.lockedIn)
        {
            matchSession.ClearPlayers();
            SceneManager.LoadScene("Local CharSelect");
        } else
        {
            player.lockedIn = false;
            player.selected = null;
        }
    }

    void StartMatch()
    {
        Dictionary<string, int> dict = new Dictionary<string, int>(); 
        foreach (PlayerSlot player in players)
        {
            dict.TryGetValue(((StageData)player.selected).scene_name, out int value);
            dict[((StageData)player.selected).scene_name] = value + 1;
        }

        int maxVal = dict.Values.Max();

        List<string> maxKeys = dict
            .Where(kvp => kvp.Value == maxVal)
            .Select(kvp => kvp.Key)
            .ToList();
        // TODO: Transitiony stuff (fade out or something)
        if (maxKeys.Count > 0) SceneManager.LoadScene(maxKeys[Random.Range(0, maxKeys.Count)]);
        else SceneManager.LoadScene("Local CharSelect");
    }
}

[System.Serializable]
public class StageData : Data
{
    public string stage_name;
    public Sprite portrait;
    public string scene_name;
}

