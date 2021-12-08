using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KaizerWaldCode.V2
{
    public class GameNetPortalV2 : MonoBehaviour
    {
        public static GameNetPortalV2 Instance;
        private ClientNetPortalV2 clientPortal;
        private ServerNetPortalV2 serverPortal;
        
        public event Action OnNetworkReadied;
        
        public event Action OnUserDisconnectRequested;
        
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
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }

        private void OnNetworkReady()
        {
            OnNetworkReadied?.Invoke();
        }
        
        private void OnClientConnected(ulong clientId)
        {
            if (clientId != NetworkManager.Singleton.LocalClientId) return;
            Debug.Log("Connected as client");
            OnNetworkReady();
            //NetworkManager.Singleton.SceneManager.OnSceneEvent += HandleSceneEvent;
        }
        
        public void SaveClientData(string playerName)
        {
            string clientGuid = ClientPrefs.GetGuid();
            string payload = JsonUtility.ToJson(new ConnectionPayload()
            {
                clientGUID = clientGuid,
                clientScene = SceneManager.GetActiveScene().buildIndex,
                playerName = playerName
            });

            byte[] payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);

            NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
        }
        
        
        public void RequestDisconnect()
        {
            OnUserDisconnectRequested?.Invoke();
        }
    }
}
