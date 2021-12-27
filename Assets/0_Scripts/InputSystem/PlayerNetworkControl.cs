using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using StarterAssets;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using quaternion = Unity.Mathematics.quaternion;

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
        private PlayerState OldPlayerState = PlayerState.Idle;
        
        [Tooltip("Acceleration and deceleration")] 
        public float speedChangeRate = 10.0f;
        
        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)] public float rotationSmoothTime = 0.12f;
        
        [SerializeField] private float moveSpeed = 2.0f;
        [SerializeField] private float sprintSpeed = 5.335f;

        [SerializeField] private NetworkVariable<float> AnimationBlend = new NetworkVariable<float>(0.0f);
        
        [SerializeField] private NetworkVariable<PlayerState> networkPlayerState = new NetworkVariable<PlayerState>();
        
        [SerializeField] private NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>();

        [SerializeField] private NetworkVariable<Quaternion> netRotationQuaternion = new NetworkVariable<Quaternion>();
        
        [SerializeField] private NetworkVariable<Matrix4x4> netMatrix = new NetworkVariable<Matrix4x4>();
        private float TargetSpeed => Keyboard.current.leftShiftKey.isPressed ? sprintSpeed : moveSpeed;
        
        // animation IDs
        private int AnimIDSpeed;
        private int AnimIDGrounded;
        private int AnimIDJump;
        private int AnimIDFreeFall;
        private int AnimIDMotionSpeed;
        
        //Player
        private float Speed;
        //private float AnimationBlend;
        private float TargetRotation = 0.0f;
        private float RotationVelocity;
        private float VerticalVelocity = 0.0f;
        private float TerminalVelocity = 53.0f;

        private Animator PlayerAnimator;
        private PlayerInputController InputStarter;
        private CharacterController Controller;
        public GameObject Camera;

        private bool HasOwnership => NetworkManager.Singleton.IsHost ? IsHost && IsOwner : IsClient && IsOwner;
        private Vector2 InputMove => InputStarter.MoveValue;
        
        private void Awake()
        {
            PlayerAnimator = GetComponent<Animator>();
            InputStarter = GetComponent<PlayerInputController>();
            Controller = GetComponent<CharacterController>();
            
            AssignAnimationIDs();
        }

        public override void OnNetworkSpawn()
        {
            NetManager = NetworkManager.Singleton;
            networkPlayerState.OnValueChanged += UpdateVisualsTest;
            AnimationBlend.OnValueChanged += UpdateVisualsTest2;
            netMatrix.OnValueChanged += TestMatrixTransform;
            if (!HasOwnership) return;
            Camera = GetComponentInChildren<CinemachineBrain>().gameObject;
        }

        private void TestMatrixTransform(Matrix4x4 previousvalue, Matrix4x4 newvalue)
        {
            Debug.Log($"New Pos = {newvalue.GetPosition()}; new rot = {newvalue.rotation}");
            //Controller.transform.SetPositionAndRotation(newvalue.GetPosition(), newvalue.rotation);
            Controller.transform.rotation = newvalue.rotation;
            Controller.Move(newvalue.GetPosition());
            //transform.SetPositionAndRotation(newvalue.GetPosition(), newvalue.rotation);
        }

        public override void OnDestroy()
        {
            networkPlayerState.OnValueChanged -= UpdateVisualsTest;
            AnimationBlend.OnValueChanged -= UpdateVisualsTest2;
            netMatrix.OnValueChanged -= TestMatrixTransform;
        }

        public override void OnNetworkDespawn()
        {
            networkPlayerState.OnValueChanged -= UpdateVisualsTest;
            AnimationBlend.OnValueChanged -= UpdateVisualsTest2;
            netMatrix.OnValueChanged -= TestMatrixTransform;
        }

        private void UpdateVisualsTest2(float previousValue, float newValue)
        {
            
            if (newValue <= previousValue - float.Epsilon || newValue == 0.0f)
            {
                Debug.Log($"Stop Start {newValue}");
                if (newValue == 0)
                {
                    Debug.Log($"Stop Value == 0 {newValue}");
                    PlayerAnimator.SetFloat(AnimIDSpeed, newValue);
                    PlayerAnimator.SetFloat(AnimIDMotionSpeed, 1);
                    return;
                }
                Debug.Log($"Stop Value != 0 {newValue}");
                AnimationBlendServerRpc(0.0f);
                PlayerAnimator.SetFloat(AnimIDSpeed, newValue);
                PlayerAnimator.SetFloat(AnimIDMotionSpeed, 1);
                return;
            }
            PlayerAnimator.SetFloat(AnimIDSpeed, newValue);
            PlayerAnimator.SetFloat(AnimIDMotionSpeed, 1);

            float animationSpeed = (InputMove == Vector2.zero) ? 0.0f : TargetSpeed;
            //Debug.Log($"anim Speed = {animationSpeed}");
            float newAnimBlend = Mathf.Lerp(newValue, animationSpeed, Time.deltaTime * speedChangeRate);
            //we only have idle or move (may have to go back to switch case)
            
            AnimationBlendServerRpc(newAnimBlend);
        }
        
        private void UpdateVisualsTest(PlayerState previousValue, PlayerState newValue)
        {
            
            float animationSpeed = (InputMove == Vector2.zero) ? 0.0f : TargetSpeed;
            if (newValue == PlayerState.Idle) animationSpeed = 0.0f;
            //Debug.Log($"anim Speed = {animationSpeed}");
            float newAnimBlend = Mathf.Lerp(AnimationBlend.Value, animationSpeed, Time.deltaTime * speedChangeRate);
            //we only have idle or move (may have to go back to switch case)
            AnimationBlendServerRpc(newAnimBlend);
            //PlayerAnimator.SetFloat(AnimIDSpeed, AnimationBlend.Value);
            //PlayerAnimator.SetFloat(AnimIDMotionSpeed, 1);
        }


        private void Update()
        {
            if (IsClient && IsOwner)
            {
                ClientInput();
            }
            //ClientVisuals();
            //OtherClientsUpdateTransform();
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
            if (InputMove == Vector2.zero /*&& networkPosition.Value == Vector3.zero*/) return;

            if (InputMove == Vector2.zero /*&& networkPosition.Value != Vector3.zero*/)
            {
                UpdateClientTransformServerRpc(Vector3.zero, Quaternion.identity);
                UpdatePlayerState(0);
            }
            else
            {
                Move();
                UpdatePlayerState(1);
            }
                
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
            if (netRotationQuaternion.Value != Quaternion.identity)
                transform.rotation = netRotationQuaternion.Value;
            if (networkPosition.Value != Vector3.zero)
                Controller.Move(networkPosition.Value);
        }
/*
        private void ClientVisuals()
        {
            float animationSpeed = (InputMove == Vector2.zero) ? 0.0f : TargetSpeed;
            AnimationBlend = Mathf.Lerp(AnimationBlend, animationSpeed, Time.deltaTime * speedChangeRate);
            //we only have idle or move (may have to go back to switch case)
            PlayerAnimator.SetFloat(AnimIDSpeed, AnimationBlend);
            PlayerAnimator.SetFloat(AnimIDMotionSpeed, 1);
            
            switch (networkPlayerState.Value)
            {
                case PlayerState.Walk:
                    PlayerAnimator.SetFloat(AnimIDSpeed, AnimationBlend);
                    PlayerAnimator.SetFloat(AnimIDMotionSpeed, 1);
                    break;
                case PlayerState.ReverseWalk:
                    PlayerAnimator.SetFloat(AnimIDSpeed, AnimationBlend);
                    PlayerAnimator.SetFloat(AnimIDMotionSpeed, 1);
                    break;
                default:
                    PlayerAnimator.SetFloat(AnimIDSpeed, AnimationBlend);
                    PlayerAnimator.SetFloat(AnimIDMotionSpeed, 1);
                    break;
            }
            
        }
*/
        private void Move()
        {
            float targetSpeed = GetSpeed();
            Vector3 inputDirection = new Vector3(InputMove.x, 0.0f, InputMove.y).normalized;
            
            TargetRotation = degrees(Mathf.Atan2(inputDirection.x, inputDirection.z)) + Camera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, TargetRotation, ref RotationVelocity, rotationSmoothTime);
            
            // rotate to face input direction relative to camera position
            Vector3 targetDirection = Quaternion.Euler(0.0f, TargetRotation, 0.0f) * Vector3.forward;
            
            Quaternion rotInput = Quaternion.Euler(0.0f, rotation, 0.0f);
            Vector3 posInput = targetDirection.normalized * (targetSpeed * Time.deltaTime) /*+ new Vector3(0.0f, VerticalVelocity, 0.0f) * NetManager.LocalTime.FixedDeltaTime*/;
            //Matrix4x4 newmat = new Matrix4x4();
            //Debug.Log($"new mat pos = {posInput}");
            //newmat.SetTRS(posInput, rotInput, Vector3.one);
            Transform newTransform = Controller.transform;
            newTransform.SetPositionAndRotation(posInput, rotInput);
            //Debug.Log($"new mat pos = {newTransform.localToWorldMatrix.GetPosition()}");
            UpdateClientMatrixServerRpc(newTransform.localToWorldMatrix);
            //UpdateClientTransformServerRpc(posInput, rotInput);
        }

        private float GetSpeed()
        {
            float currentSpeed = (InputMove == Vector2.zero) ? 0.0f : TargetSpeed;
            Vector3 velocity = Controller.velocity;
            float currentHorizontalSpeed = new Vector3(velocity.x, 0.0f, velocity.z).magnitude;
            if (currentHorizontalSpeed < currentSpeed - 0.1f || currentHorizontalSpeed > currentSpeed + 0.1f)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                Speed = Mathf.Lerp(currentHorizontalSpeed, currentSpeed, Time.deltaTime * speedChangeRate);

                // round speed to 3 decimal places
                Speed = Mathf.Round(Speed * 1000f) / 1000f;
            }
            else
            {
                Speed = currentSpeed;
            }
            return Speed;
        }
        
        [ServerRpc]
        private void UpdateClientTransformServerRpc(Vector3 newPosition, Quaternion newRotation)
        {
            networkPosition.Value = newPosition;
            netRotationQuaternion.Value = newRotation;
        }
        
        [ServerRpc]
        private void UpdateClientMatrixServerRpc(Matrix4x4 newTransform)
        {
            netMatrix.Value = newTransform;
        }

        [ServerRpc]
        private void UpdatePlayerStateServerRpc(PlayerState newState) => networkPlayerState.Value = newState;
        
        [ServerRpc]
        private void AnimationBlendServerRpc(float newValue)
        {
            AnimationBlend.Value = newValue;
        }
    }
}
