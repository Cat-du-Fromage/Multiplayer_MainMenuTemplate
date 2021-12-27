using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;
using quaternion = Unity.Mathematics.quaternion;

namespace KaizerWaldCode.V2
{
    [Serializable]
    public enum MovementStatus
    {
        Idle,         // not trying to move
        Walking,      // character is walking (Walk)
        Running,      // character is running (Run)
    }
    
    public class ServerCharacterMouvement : NetworkBehaviour
    {
        /// Indicates how the character's movement should be depicted.
        [SerializeField] private NetworkVariable<MovementStatus> MovementStatus = new NetworkVariable<MovementStatus>();
        
        //[SerializeField] private NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>();
        [SerializeField] private NetworkVariable<float> TargetRotation = new NetworkVariable<float>(0.0f);
        
        //public CharacterController Controller;

        private NetworkCharacterState NetworkCharacter;
        private void Awake()
        {
            //Controller = GetComponent<CharacterController>();
            NetworkCharacter = GetComponent<NetworkCharacterState>();
        }

        

        public override void OnNetworkSpawn()
        {
            //networkTransform.Value = transform.localToWorldMatrix;
            if (IsServer)
            {
                //networkPosition.Value = transform.position;
                //networkRotation.Value = transform.rotation;
                
                NetworkCharacter.ReceivedClientInput += UpdateNetworkTransform;

                return;
            }
            // Disable server component on clients
            enabled = false;
        }

        private void UpdateNetworkTransform(Vector2 clientInput)
        {
            Vector3 inputDirection = new Vector3(clientInput.x, 0.0f, clientInput.y).normalized;
            transform.position += inputDirection * Time.deltaTime;
        }
        /*
        private Transform Move(Vector2 InputMove)
        {
            float targetSpeed = 1f;
            Vector3 inputDirection = new Vector3(InputMove.x, 0.0f, InputMove.y).normalized;
            
            TargetRotation.Value = degrees(Mathf.Atan2(inputDirection.x, inputDirection.z)) + Camera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, TargetRotation.Value, ref RotationVelocity, rotationSmoothTime);
            
            // rotate to face input direction relative to camera position
            Vector3 targetDirection = Quaternion.Euler(0.0f, TargetRotation, 0.0f) * Vector3.forward;
            
            Quaternion rotInput = Quaternion.Euler(0.0f, rotation, 0.0f);
            Vector3 posInput = targetDirection.normalized * (targetSpeed * Time.deltaTime);

            Transform newTransform = Controller.transform;
            newTransform.SetPositionAndRotation(posInput, rotInput);
            return newTransform;
        }
        */
/*
        private void Update()
        {
            if (transform.position == networkPosition.Value) return;
            //Debug.Log("update sent?");
            transform.position = networkPosition.Value;
        }
        */
    }
}
