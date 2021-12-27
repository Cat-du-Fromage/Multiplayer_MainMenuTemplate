using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

using static Unity.Mathematics.math;
using quaternion = Unity.Mathematics.quaternion;

namespace KaizerWaldCode.V2
{
    public class ClientInputSender : NetworkBehaviour
    {

        public PlayerController PlayerCtrl;
        [ReadOnlyInspector] public Vector2 MoveValue;

        private NetworkCharacterState NetworkCharacter;
        private GameObject MainCamera;
        public GameObject CineCamera;
        public CharacterController Controller;
        public ServerCharacterMouvement ServerMouvement;
        
        private bool HasOwnership => NetworkManager.Singleton.IsHost ? IsHost && IsOwner : IsClient && IsOwner;
        
        private void Awake()
        {
            NetworkCharacter = GetComponent<NetworkCharacterState>();
            Controller = GetComponent<CharacterController>();
            MainCamera = GetComponentInChildren<CinemachineBrain>().gameObject;
            CineCamera = GetComponentInChildren<CinemachineVirtualCamera>().gameObject;
            ServerMouvement = GetComponent<ServerCharacterMouvement>();
            PlayerCtrl = new PlayerController();
            PlayerCtrl.Player.Enable();
        }
        
        public override void OnNetworkSpawn()
        {
            
            if (!IsClient || !IsOwner)
            {
                enabled = false;
                //CAMERA MUST NOT BE HERE => go to CAMERA TPS CONTROLLER
                MainCamera.SetActive(HasOwnership);
                CineCamera.SetActive(HasOwnership);
                return;
            }

            MovementEvents(true);

            PlayerCtrl.Player.Run.performed += OnCharacterRun;
            
            //Debug.Log($"(!IsClient || !IsOwner) && !IsHost : {(!IsClient || !IsOwner) && !IsHost}");
            //Debug.Log($"enable = IsClient && IsOwner : {IsClient && IsOwner}");
        }

        private void OnCharacterRun(InputAction.CallbackContext ctx)
        {
            throw new System.NotImplementedException();
        }

        public override void OnNetworkDespawn()
        {
            MovementEvents(false);
            PlayerCtrl.Disable();
        }
        
        public override void OnDestroy()
        {
            MovementEvents(false);
            PlayerCtrl.Disable();
        }

        
        //WASD MOVES
        private void OnCharacterMove(InputAction.CallbackContext ctx) => MoveValue = ctx.ReadValue<Vector2>();
        private void OnCharacterStop(InputAction.CallbackContext ctx) => MoveValue = Vector2.zero;
        
        private void Update()
        {
            if (MoveValue == Vector2.zero) return;
            //Vector3 inputDirection = new Vector3(MoveValue.x, 0.0f, MoveValue.y).normalized;
            NetworkCharacter.SendCharacterInputServerRpc(MoveValue);
        }

        private void MovementEvents(bool enable)
        {
            if (enable)
            {
                PlayerCtrl.Player.WASD.performed += OnCharacterMove;
                PlayerCtrl.Player.WASD.canceled += OnCharacterStop;
            }
            else
            {
                PlayerCtrl.Player.WASD.performed -= OnCharacterMove;
                PlayerCtrl.Player.Look.canceled -= OnCharacterStop;
            }
        }
    }
}
