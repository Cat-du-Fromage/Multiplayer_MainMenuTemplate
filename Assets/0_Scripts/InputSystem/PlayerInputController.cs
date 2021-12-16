using System;
using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KaizerWaldCode
{
    public class PlayerInputController : MonoBehaviour
    {
        public PlayerController PlayerCtrl;
        [ReadOnlyInspector] public Vector2 MoveValue;
        [ReadOnlyInspector] public Vector2 Look;
        
        public bool cursorLocked = false;
        public float Vertical => MoveValue.y;
        public float Horizontal => MoveValue.x;
        private void Awake()
        {
            PlayerCtrl = new PlayerController();
            PlayerCtrl.Player.Enable();
            
            PlayerCtrl.Player.WASD.performed += OnMove;
            PlayerCtrl.Player.Look.performed += OnMouseMove;
        }

        private void OnMove(InputAction.CallbackContext ctx) => MoveValue = ctx.ReadValue<Vector2>();
        
        private void OnMouseMove(InputAction.CallbackContext ctx) => Look = ctx.ReadValue<Vector2>();

        private void OnDestroy()
        {
            PlayerCtrl.Disable();
            PlayerCtrl.Player.WASD.performed -= OnMove;
            PlayerCtrl.Player.Look.performed -= OnMouseMove;
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
}
