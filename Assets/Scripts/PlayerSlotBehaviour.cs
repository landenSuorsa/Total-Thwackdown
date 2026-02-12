using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSlotBehaviour : MonoBehaviour
{
    public PlayerSlot slot;
    SelectScript manager;
    public bool canConfirm = false;

    public void Init(SelectScript mgr, PlayerInput input, int playerIndex, int index)
    {
        manager = mgr;

        slot = new PlayerSlot
        {
            input = input,
            playerIndex = playerIndex,
            cursorIndex = index
        };

        Invoke("SetCanConfirm", 0.3f);
    }

    // INPUT CALLBACKS (called by PlayerInput)
    public void OnMove(InputValue value)
    {
        print("ON MOVE");
        float x = value.Get<Vector2>().x;
        if (Mathf.Abs(x) < 0.5f) return;

        manager.MoveCursor(slot, x > 0 ? 1 : -1);
    }

    public void OnConfirm()
    {
        if (!canConfirm) return;
        manager.LockIn(slot);
    }

    public void OnSelect()
    {
        if (!canConfirm) return;
        manager.Select(slot);
    }

    public void OnBack()
    {
        if (!canConfirm) return;
        manager.Back(slot);
    }

    void SetCanConfirm()
    {
        canConfirm = true;
    }

    public void OnStart()
    {
        manager.Start();
    }
}
