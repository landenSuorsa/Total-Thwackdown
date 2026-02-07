using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class CursorVisual : MonoBehaviour
{
    public PlayerSlotBehaviour owner; // assigned on spawn
    public RectTransform cursorRect;  // your cursor image
    public Image highlightImage;      // optional: highlight / lock-in color
    public SelectScript selectScript;
    public Transform[] cursorSpots;
    private Color[] colors = {Color.red, Color.blue, Color.yellow, Color.gray};

    void Start()
    {
        selectScript = GameObject.FindFirstObjectByType<SelectScript>();
        cursorSpots = selectScript.GetSlots();
        cursorRect.gameObject.GetComponent<Image>().color = colors[owner.slot.playerIndex];
    }

    void Update()
    {
        if (owner == null || owner.slot == null) return;

        int index = owner.slot.cursorIndex;
        if (index < 0 || index >= cursorSpots.Length) return;

        // Move cursor to target character
        cursorRect.gameObject.transform.position = cursorSpots[index].position - new Vector3(owner.slot.playerIndex * 20, 0, 0);

        cursorRect.gameObject.GetComponent<Image>().color = owner.slot.lockedIn ? new Color(colors[owner.slot.playerIndex].r * 0.4f, colors[owner.slot.playerIndex].g * 0.4f, colors[owner.slot.playerIndex].b * 0.4f) : colors[owner.slot.playerIndex];
    }
}
