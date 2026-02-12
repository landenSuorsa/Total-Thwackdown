using System.Collections.Generic;
using TMPro;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.InputSystem.Android;
using UnityEngine.UI;

public class StageManagerScript : MonoBehaviour
{
    [Header("Spawn Points")]
    [SerializeField] private List<Transform> spawnPoints;

    [Header("Blast Zones")]
    [SerializeField] private GameObject blastZoneTop;
    [SerializeField] private GameObject blastZoneBottom;
    [SerializeField] private GameObject blastZoneLeft;
    [SerializeField] private GameObject blastZoneRight;

    [Header("Stage Bounds")]
    [SerializeField] private Vector2 stageBoundsMin = new Vector2(-20, -15);
    [SerializeField] private Vector2 stageBoundsMax = new Vector2(20, 15);

    private GameLoopManager gameLoopManager;

    void Awake()
    {
        //SetupBlastZones();
    }

    void Start()
    {
        gameLoopManager = FindFirstObjectByType<GameLoopManager>();

        if (gameLoopManager == null)
        {
            Debug.LogError("GameLoopManager not found :(");
        }
    }

    //void SetupBlastZones()
    //{
    //    if (blastZoneBottom == null)
    //    {
    //        blastZoneBottom = CreateBlastZone("BlastZone_Bottom",
    //            new Vector2(0, stageBoundsMin.y - 2),
    //            new Vector2(Mathf.Abs(stageBoundsMax.x - stageBoundsMin.x) + 10, 2),
    //           BlastZone.BlastZoneType.Bottom);
    //    }

    //    // Top blast zone
    //    if (blastZoneTop == null)
    //    {
    //        blastZoneTop = CreateBlastZone("BlastZone_Top",
    //            new Vector2(0, stageBoundsMax.y + 2),
    //            new Vector2(Mathf.Abs(stageBoundsMax.x - stageBoundsMin.x) + 10, 2),
    //            BlastZone.BlastZoneType.Top);
    //    }

    //    // Left blast zone
    //    if (blastZoneLeft == null)
    //    {
    //        blastZoneLeft = CreateBlastZone("BlastZone_Left",
    //            new Vector2(stageBoundsMin.x - 2, 0),
    //            new Vector2(2, Mathf.Abs(stageBoundsMax.y - stageBoundsMin.y) + 10),
    //            BlastZone.BlastZoneType.Left);
    //    }

    //    // Right blast zone
    //    if (blastZoneRight == null)
    //    {
    //        blastZoneRight = CreateBlastZone("BlastZone_Right",
    //            new Vector2(stageBoundsMax.x + 2, 0),
    //            new Vector2(2, Mathf.Abs(stageBoundsMax.y - stageBoundsMin.y) + 10),
    //            BlastZone.BlastZoneType.Right);
    //    }
    //}

    //GameObject CreateBlastZone(string name, Vector2 position, Vector2 size, BlastZone.BlastZoneType type)
    //{
    //    GameObject zone = new GameObject(name);
    //    zone.transform.parent = transform;
    //    zone.transform.position = position;

    //    BoxCollider2D collider = zone.AddComponent<BoxCollider2D>();
    //    collider.size = size;
    //    collider.isTrigger = true;

    //    BlastZone blastZone = zone.AddComponent<BlastZone>();
    //    // You'll need to set the zone type via serialized field or public property

    //    return zone;
    //}

    void OnDrawGizmos()
    {
        // Draw stage bounds
        Gizmos.color = Color.green;
        Vector3 bottomLeft = new Vector3(stageBoundsMin.x, stageBoundsMin.y, 0);
        Vector3 bottomRight = new Vector3(stageBoundsMax.x, stageBoundsMin.y, 0);
        Vector3 topLeft = new Vector3(stageBoundsMin.x, stageBoundsMax.y, 0);
        Vector3 topRight = new Vector3(stageBoundsMax.x, stageBoundsMax.y, 0);

        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, bottomLeft);

        // Draw spawn points
        if (spawnPoints != null)
        {
            Gizmos.color = Color.blue;
            foreach (Transform spawn in spawnPoints)
            {
                if (spawn != null)
                {
                    Gizmos.DrawWireSphere(spawn.position, 0.5f);
                }
            }
        }
    }

    public List<Transform> GetSpawnPoints()
    {
        return spawnPoints;
    }
}
