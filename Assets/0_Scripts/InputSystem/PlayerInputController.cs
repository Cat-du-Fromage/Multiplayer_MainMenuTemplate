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
        private PlayerController PlayerCtrl;
        [ReadOnlyInspector] public Vector2 MoveValue;
        public float Vertical => MoveValue.y;
        public float Horizontal => MoveValue.x;
        private void Awake()
        {
            PlayerCtrl = new PlayerController();
            PlayerCtrl.Player.Enable();
            
            PlayerCtrl.Player.WASD.performed += OnMove;
        }

        private void OnMove(InputAction.CallbackContext ctx)
        {
            MoveValue = ctx.ReadValue<Vector2>();
        }

        private void OnDestroy()
        {
            PlayerCtrl.Disable();
            PlayerCtrl.Player.WASD.performed -= OnMove;
        }
    }
}
