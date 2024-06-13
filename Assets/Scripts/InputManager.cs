using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class InputManager : MonoBehaviour
{
    public Vector2 move;
    public Vector2 look;
    public bool jump;
    public bool sprint;
    public bool aiming;
    public bool fire;
    public bool roll;

    public bool cursorLocked = true;
    public bool cursorInputForLook = true;

    public void OnMove(InputValue value)
    {
        move = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        if (cursorInputForLook)
        {
            look = value.Get<Vector2>() * 2;
        }
    }

    public void OnJump(InputValue value)
    {
        jump = value.isPressed;
    }

    public void OnSprint(InputValue value)
    {
        sprint = value.isPressed;
    }

    public void OnAim(InputValue value)
    {
        aiming = value.isPressed;
    }
    public void OnFire(InputValue value)
    {
        fire = value.isPressed;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
            roll = true;
        else
            roll = false;

    }

    private void OnApplicationFocus(bool hasFocus)
    {
        SetCursorState(cursorLocked);
    }

    private void SetCursorState(bool newState)
    {
        Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
    }
}
