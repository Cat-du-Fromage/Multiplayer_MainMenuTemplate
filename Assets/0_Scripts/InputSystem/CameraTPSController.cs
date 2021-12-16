using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KaizerWaldCode
{
    public class CameraTPSController : NetworkBehaviour
    {
        private NetworkManager NetManager;
        private PlayerInputController InputStarter;
        public GameObject CinemachineCameraTarget;
        public GameObject CineCamera;
        public GameObject Camera;
        
        // cinemachine
        private float CinemachineTargetYaw;
        private float CinemachineTargetPitch;
        
        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;
        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;
        
        private InputAction PlayerCtrlLook;
        
        private bool HasOwnership => NetworkManager.Singleton.IsHost ? IsHost && IsOwner : IsClient && IsOwner;

        private void Awake()
        {
            InputStarter = GetComponent<PlayerInputController>();
            CineCamera = GetComponentInChildren<CinemachineVirtualCamera>().gameObject;
            Camera = GetComponentInChildren<CinemachineBrain>().gameObject;
            //LastCursorPosition = Mouse.current.position.ReadValue();
            PlayerCtrlLook = InputStarter.PlayerCtrl.Player.Look;
        }
        
        public override void OnNetworkSpawn()
        {
            enabled = HasOwnership;
            NetManager = NetworkManager.Singleton;
            CineCamera.SetActive(HasOwnership);
            Camera.SetActive(HasOwnership);
            if(HasOwnership) PlayerCtrlLook.performed += CameraRotation;
        }

        public override void OnNetworkDespawn() => PlayerCtrlLook.performed -= CameraRotation;

        public override void OnDestroy() => PlayerCtrlLook.performed -= CameraRotation;

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }
        
        private void CameraRotation(InputAction.CallbackContext ctx)
        {
            Vector2 inputLook = ctx.ReadValue<Vector2>();
            //Debug.Log($"Input look = {InputStarter.Look}");

            //Debug.Log($"current magn = {InputStarter.Look.sqrMagnitude}");
            CinemachineTargetYaw += inputLook.x * NetManager.ServerTime.FixedDeltaTime;
            CinemachineTargetPitch += inputLook.y * NetManager.ServerTime.FixedDeltaTime;

            // clamp our rotations so our values are limited 360 degrees
            CinemachineTargetYaw = ClampAngle(CinemachineTargetYaw, float.MinValue, float.MaxValue);
            CinemachineTargetPitch = ClampAngle(CinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(CinemachineTargetPitch, CinemachineTargetYaw, 0.0f);
        }
    }
}
