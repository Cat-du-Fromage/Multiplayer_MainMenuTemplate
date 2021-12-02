using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace KaizerWaldCode.V2
{
    public class GameNetPortalV2 : MonoBehaviour
    {
        public static GameNetPortalV2 Instance;
        private ClientNetPortalV2 clientPortal;
        private ServerNetPortalV2 serverPortal;
        
        private void Awake()
        {
            Instance = this;
        }
        
        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            clientPortal = ClientNetPortalV2.Instance;
            serverPortal = ServerNetPortalV2.Instance;
            
            NetworkManager.Singleton.OnServerStarted += OnNetworkReady;
        }
        
        private void OnNetworkReady()
        {
            clientPortal.OnNetworkReady();
            serverPortal.OnNetworkReady();
        }
    }
}
