using UnityEngine;
using UnityEngine.InputSystem;

public class HumanInput : MonoBehaviour, IInputProvider
{
    private InputState _input;
    private bool _isActive = true;

    void Update()
    {

    }

    public InputState GetInput()
    {
        return _isActive ? _input : new InputState();
    }

    public void OnMove(InputValue val)
    {
        _input.Move = val.Get<Vector2>();
    }

    public void OnJump(InputValue val)
    {
        _input.JumpPressed = val.isPressed;
        _input.JumpReleased = !val.isPressed;
    }

    public void OnNormal(InputValue val)
    {
        _input.NormalPressed = val.isPressed;
    }

    public void OnSpecial(InputValue val)
    {
        _input.SpecialPressed = val.isPressed;
    }

    public void Deactivate() => _isActive = false;

    public void Activate() => _isActive = true;
}
