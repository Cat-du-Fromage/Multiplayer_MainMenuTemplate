using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using StarterAssets;
using static Unity.Mathematics.math;

namespace KaizerWaldCode.V2
{
    public class PlayerNetworkControl : NetworkBehaviour
    {
        private NetworkManager NetManager;
        
        private enum PlayerState
        {
            Idle,
            Walk,
            ReverseWalk
        }
        
        [Tooltip("Acceleration and deceleration")] 
        public float speedChangeRate = 10.0f;
        
        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)] public float rotationSmoothTime = 0.12f;
        
        [SerializeField] private float moveSpeed = 2.0f;
        [SerializeField] private float sprintSpeed = 5.335f;
        [SerializeField] private float rotationSpeed = 1.5f;

        [SerializeField] private NetworkVariable<PlayerState> networkPlayerState = new NetworkVariable<PlayerState>();
        
        [SerializeField] private NetworkVariable<Vector3> networkPosition, networkRotation = new NetworkVariable<Vector3>();

        //[SerializeField] private NetworkVariable<Quaternion> netRotationQuaternion = new NetworkVariable<Quaternion>();

        [SerializeField] private NetworkVariable<float> forwardBackPosition, leftRightPosition = new NetworkVariable<float>();

        // animation IDs
        
        private int AnimIDSpeed;
        private int AnimIDGrounded;
        private int AnimIDJump;
        private int AnimIDFreeFall;
        private int AnimIDMotionSpeed;
        
        //Player
        private float Speed;
        private float AnimationBlend;
        private float TargetRotation = 0.0f;
        private float RotationVelocity;
        private float VerticalVelocity;
        private float TerminalVelocity = 53.0f;
        
        //Client caching
        private Vector3 OldInputPosition;
        private Vector3 OldInputRotation;
        
        private Animator PlayerAnimator;
        private PlayerInputController InputStarter;
        private CharacterController Controller;
        
        public GameObject CinemachineCameraTarget;
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
            NetManager = NetworkManager.Singleton;
            CineCamera.SetActive(HasOwnership);
            Camera.SetActive(HasOwnership);
        }
/*
        private void Update()
        {
            if (IsClient && IsOwner)
            {
                ClientInput();
            }
            OtherClientsUpdateTransform();
            ClientVisuals();
        }
        */
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
            Test();
            /*
            //Player Position and Rotation
            Vector3 inputRotation = new Vector3(0, InputStarter.Horizontal, 0);

            Vector3 direction = transform.TransformDirection(Vector3.forward);
            float forwardInput = InputStarter.Vertical;
            
            Vector3 inputPosition = direction * forwardInput;
            
            float targetSpeed = Keyboard.current.leftShiftKey.isPressed ? sprintSpeed : moveSpeed;
            if (OldInputPosition != inputPosition || OldInputRotation != inputRotation)
            {
                OldInputPosition = inputPosition;
                OldInputRotation = inputRotation;
                UpdateClientTransformServerRpc(inputPosition * targetSpeed, inputRotation * targetSpeed);
            }
            UpdatePlayerState(forwardInput);
            */
        }
        

        private void Test()
        {
            Vector3 posInput = Vector3.zero;
            Vector3 rotInput = Vector3.zero;
            
            //POSITION
            float targetSpeed = Keyboard.current.leftShiftKey.isPressed ? sprintSpeed : moveSpeed;
            
            // if there is no input, set the target speed to 0
            if (InputStarter.MoveValue == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            Vector3 charCtrlVelocity = Controller.velocity;
            float currentHorizontalSpeed = new Vector3(charCtrlVelocity.x, 0.0f, charCtrlVelocity.z).magnitude;
            const float speedOffset = 0.1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                Speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed, NetManager.ServerTime.FixedDeltaTime * speedChangeRate);

                // round speed to 3 decimal places
                Speed = Mathf.Round(Speed * 1000f) / 1000f;
            }
            else
            {
                Speed = targetSpeed;
            }
            // normalise input direction
            Vector3 inputDirection = new Vector3(InputStarter.MoveValue.x, 0.0f, InputStarter.MoveValue.y).normalized;
            
            
            
            //ROTATION
            if (InputStarter.MoveValue != Vector2.zero)
            {
                //TargetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg;
                TargetRotation = atan2(inputDirection.x, inputDirection.z);
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, TargetRotation, ref RotationVelocity, rotationSmoothTime);

                // rotate to face input direction relative to camera position
                rotInput = new Vector3(0.0f, rotation, 0.0f);
            }
            
            //=============================================================================================
            Vector3 targetDirection = Quaternion.Euler(0.0f, TargetRotation, 0.0f) * Vector3.forward;
            //Debug.Log($"Networktime = {NetManager.ServerTime.FixedDeltaTime} normalTime = {Time.deltaTime}");
            posInput = targetDirection.normalized * (Speed * NetManager.ServerTime.FixedDeltaTime) + new Vector3(0.0f, VerticalVelocity, 0.0f) * (NetManager.ServerTime.FixedDeltaTime);
            
            UpdateClientTransformServerRpc(posInput,rotInput);
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

        private void OtherClientsUpdateTransform()
        {
            Controller.Move(networkPosition.Value);

            transform.rotation = Quaternion.Euler(networkRotation.Value);
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
            float targetSpeed = Keyboard.current.leftShiftKey.isPressed ? sprintSpeed : moveSpeed;
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
