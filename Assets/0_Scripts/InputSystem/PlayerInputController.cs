using System;
using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.Utils;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KaizerWaldCode
{
    public class PlayerInputController : NetworkBehaviour
    {
        public PlayerController PlayerCtrl;
        [ReadOnlyInspector] public Vector2 MoveValue;
        [ReadOnlyInspector] public Vector2 Look;
        
        public bool cursorLocked = false;
        public float Vertical => MoveValue.y;
        public float Horizontal => MoveValue.x;
        
        private bool HasOwnership => NetworkManager.Singleton.IsHost ? IsHost && IsOwner : IsClient && IsOwner;
        
        private void Awake()
        {
            PlayerCtrl = new PlayerController();
            PlayerCtrl.Player.Enable();
        }

        public override void OnNetworkSpawn()
        {
            if (HasOwnership)
            {
                PlayerCtrl.Player.WASD.performed += OnCharacterMove;
                PlayerCtrl.Player.WASD.canceled += OnCharacterStop;
            
                PlayerCtrl.Player.Look.performed += OnMouseMove;
                PlayerCtrl.Player.Look.canceled += OnMouseStop;
            }
        }


        //CAMERA LOOK
        private void OnMouseStop(InputAction.CallbackContext obj) => Look = Vector2.zero;
        private void OnMouseMove(InputAction.CallbackContext ctx) => Look = ctx.ReadValue<Vector2>();

        //WASD MOVES
        private void OnCharacterMove(InputAction.CallbackContext ctx) => MoveValue = ctx.ReadValue<Vector2>();
        private void OnCharacterStop(InputAction.CallbackContext obj) => MoveValue = Vector2.zero;


        public override void OnDestroy()
        {
            PlayerCtrl.Disable();
            PlayerCtrl.Player.WASD.performed -= OnCharacterMove;
            PlayerCtrl.Player.Look.canceled -= OnCharacterStop;
            
            PlayerCtrl.Player.Look.performed -= OnMouseMove;
            PlayerCtrl.Player.Look.canceled -= OnMouseStop;
        }

        public override void OnNetworkDespawn()
        {
            PlayerCtrl.Disable();
            PlayerCtrl.Player.WASD.performed -= OnCharacterMove;
            PlayerCtrl.Player.Look.canceled -= OnCharacterStop;
            
            PlayerCtrl.Player.Look.performed -= OnMouseMove;
            PlayerCtrl.Player.Look.canceled -= OnMouseStop;
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
