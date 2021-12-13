using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Netcode;
using UnityEngine;

namespace KaizerWaldCode
{
    public class CameraNetwork : MonoBehaviour
    {
        public CinemachineVirtualCamera Cinemachine;
        private void Awake() => Cinemachine = GetComponent<CinemachineVirtualCamera>();
    }
}
