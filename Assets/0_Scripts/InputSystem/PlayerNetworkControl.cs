using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using StarterAssets;

namespace KaizerWaldCode.V2
{
    public class PlayerNetworkControl : NetworkBehaviour
    {
        private enum PlayerState
        {
            Idle,
            Walk,
            ReverseWalk
        }
        
        [Tooltip("Acceleration and deceleration")] 
        public float speedChangeRate = 10.0f;
        
        [SerializeField] private float speed = 2.0f;
        [SerializeField] private float sprintSpeed = 5.335f;
        [SerializeField] private float rotationSpeed = 1.5f;
        
        private float AnimationBlend;
        
        [SerializeField] private NetworkVariable<PlayerState> networkPlayerState = new NetworkVariable<PlayerState>();
        
        [SerializeField] private NetworkVariable<Vector3> networkPosition, networkRotation = new NetworkVariable<Vector3>();

        [SerializeField] private NetworkVariable<float> forwardBackPosition, leftRightPosition = new NetworkVariable<float>();

        // animation IDs
        private int AnimIDSpeed;
        private int AnimIDGrounded;
        private int AnimIDJump;
        private int AnimIDFreeFall;
        private int AnimIDMotionSpeed;
        
        //Client caching
        private Vector3 OldInputPosition;
        private Vector3 OldInputRotation;
        
        private Animator PlayerAnimator;
        private PlayerInputController InputStarter;
        private CharacterController Controller;
        
        public GameObject CineCamera;
        public GameObject Camera;

        private bool HasOwnership => NetworkManager.Singleton.IsHost ? IsHost && IsOwner : IsClient && IsOwner;
        
        private void Awake()
        {
            PlayerAnimator = GetComponent<Animator>();
            InputStarter = GetComponent<PlayerInputController>();
            Controller = GetComponent<CharacterController>();
            
            CineCamera = GetComponentInChildren<CinemachineVirtualCamera>().gameObject;
            Camera = GetComponentInChildren<CinemachineBrain>().gameObject;

            AssignAnimationIDs();
        }

        public override void OnNetworkSpawn()
        {
            //enabled = HasOwnership;
            CineCamera.SetActive(HasOwnership);
            Camera.SetActive(HasOwnership);
        }

        private void Update()
        {
            if (IsClient && IsOwner)
            {
                ClientInput();
            }
            
            ClientMoveAndRotate();
            ClientVisuals();
        }
        
        private void AssignAnimationIDs()
        {
            AnimIDSpeed = Animator.StringToHash("Speed");
            AnimIDGrounded = Animator.StringToHash("Grounded");
            AnimIDJump = Animator.StringToHash("Jump");
            AnimIDFreeFall = Animator.StringToHash("FreeFall");
            AnimIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void ClientInput()
        {
            //Player Position and Rotation
            Vector3 inputRotation = new Vector3(0, InputStarter.Horizontal, 0);

            Vector3 direction = transform.TransformDirection(Vector3.forward);
            float forwardInput = InputStarter.Vertical;
            
            Vector3 inputPosition = direction * forwardInput;

            float targetSpeed = Keyboard.current.leftShiftKey.isPressed ? sprintSpeed : speed;
            
            if (OldInputPosition != inputPosition || OldInputRotation != inputRotation)
            {
                OldInputPosition = inputPosition;
                OldInputRotation = inputRotation;
                UpdateClientTransformServerRpc(inputPosition * targetSpeed, inputRotation * targetSpeed);
            }

            UpdatePlayerState(forwardInput);
        }

        private void UpdatePlayerState(float forwardInput)
        {
            switch (forwardInput)
            {
                case > 0:
                    UpdatePlayerStateServerRpc(PlayerState.Walk);
                    break;
                case < 0:
                    UpdatePlayerStateServerRpc(PlayerState.ReverseWalk);
                    break;
                default:
                    UpdatePlayerStateServerRpc(PlayerState.Idle);
                    break;
            }
        }

        private void ClientMoveAndRotate()
        {
            if (networkPosition.Value != Vector3.zero)
            {
                Controller.SimpleMove(networkPosition.Value);
            }
            if (networkRotation.Value != Vector3.zero)
            {
                transform.Rotate(networkRotation.Value);
            }
        }

        private void ClientVisuals()
        {
            float targetSpeed = Keyboard.current.leftShiftKey.isPressed ? sprintSpeed : speed;
            AnimationBlend = Mathf.Lerp(AnimationBlend, targetSpeed, Time.deltaTime * speedChangeRate);
            
            if (networkPlayerState.Value == PlayerState.Walk)
            {
                PlayerAnimator.SetFloat(AnimIDSpeed, AnimationBlend);
                PlayerAnimator.SetFloat(AnimIDMotionSpeed, 1);
            }
            else if (networkPlayerState.Value == PlayerState.ReverseWalk)
            {
                PlayerAnimator.SetFloat(AnimIDSpeed, AnimationBlend);
                PlayerAnimator.SetFloat(AnimIDMotionSpeed, 1);
            }
            else
            {
                PlayerAnimator.SetFloat(AnimIDSpeed, 0);
                PlayerAnimator.SetFloat(AnimIDMotionSpeed, 0);
            }
        }

        [ServerRpc]
        private void UpdateClientTransformServerRpc(Vector3 newPosition, Vector3 newRotation)
        {
            networkPosition.Value = newPosition;
            networkRotation.Value = newRotation;
        }

        [ServerRpc]
        private void UpdatePlayerStateServerRpc(PlayerState newState)
        {
            networkPlayerState.Value = newState;
        }
    }
}
