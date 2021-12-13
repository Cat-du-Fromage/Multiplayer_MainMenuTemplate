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
        public static GameNetPortalV2 Instance { get; private set; }

        public event Action OnNetworkReadied;
        public event Action OnUserDisconnectRequested;

        private void Awake() => Instance = this;

        private void Start()
        {
            //Initialization Part
            DontDestroyOnLoad(gameObject);

            //Events
            NetworkManager.Singleton.OnServerStarted += OnNetworkReady;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }

        private void OnClientConnected(ulong clientId)
        {
            //We go through ALL clients so this check is necessary!
            if (clientId != NetworkManager.Singleton.LocalClientId) return;
            Debug.Log($"clientId = {clientId} ; NetworkManager.LocalClientId = {NetworkManager.Singleton.LocalClientId}" );
            OnNetworkReady();
            NetworkLog.LogInfoServer($"OnClientConnected {clientId}");
        }


        public void SaveClientData(string playerName)
        {
            string payload = JsonUtility.ToJson(new ConnectionPayload()
            {
                clientGUID = ClientPrefs.GetGuid(),
                clientScene = SceneManager.GetActiveScene().buildIndex,
                playerName = playerName
            });

            byte[] payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);

            NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
        }
//EVENTS
//======================================================================================================================
        
        public void RequestDisconnect() => OnUserDisconnectRequested?.Invoke();
        
        private void OnNetworkReady() => OnNetworkReadied?.Invoke();
    }
}
