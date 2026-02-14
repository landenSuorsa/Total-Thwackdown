using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.Android;

public class CursorScript : MonoBehaviour
{
    bool _stageActivated = true;
    [SerializeField] List<GameObject> windows;
    [SerializeField] List<Transform> openTransforms;
    [SerializeField] List<Transform> closeTransforms;
    [SerializeField] float changeTime = 15f;
    private float lastChanged = -1f;
    private Transform target;
    private int targetIndex;
    private int numOpen = 1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        numOpen = 0;
        foreach (var window in windows) if (window.activeSelf) numOpen++;
        if (!_stageActivated || Time.time - lastChanged < changeTime) return;
        if (target)
        {
            transform.position = Vector2.MoveTowards(transform.position, target.position, Time.deltaTime * 3);
            if ((transform.position - target.position).magnitude < 0.01)
            {
                lastChanged = Time.time;
                windows[targetIndex].SetActive(!windows[targetIndex].activeSelf);
                target = null;
            }
        }
        else
        {
            SelectTarget();
        }
    }

    private void SelectTarget()
    {
        if (target) return;

        var rand = Random.Range(0, 2);
        if (numOpen < 2 || (numOpen == 2 && rand == 0))
        {
            var counter = 0;
            Dictionary<int, GameObject> targets = new Dictionary<int, GameObject>();
            foreach (var window in windows)
            {
                if (!window.activeSelf)
                {
                    targets.Add(counter, window);
                }
                counter++;
            }

            if (targets.Count < 1) return;

            var index = Random.Range(0, targets.Count);
            targetIndex = targets.ElementAt(index).Key;
            target = openTransforms[targetIndex];
        }
        else
        {
            var counter = 0;
            Dictionary<int, GameObject> targets = new Dictionary<int, GameObject>();
            foreach (var window in windows)
            {
                if (window.activeSelf)
                {
                    targets.Add(counter, window);
                }
                counter++;
            }

            if (targets.Count < 1) return;

            var index = Random.Range(0, targets.Count);
            targetIndex = targets.ElementAt(index).Key;
            target = closeTransforms[targetIndex];
        }
    }

    public void Activate()
    {
        _stageActivated = true;
    }

    public void Deactivate()
    {
        _stageActivated = false;
    }
}
