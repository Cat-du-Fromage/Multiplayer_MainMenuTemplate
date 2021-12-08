using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KaizerWaldCode.V2
{
    public class ClientNetPortalV2 : MonoBehaviour
    {
        public static ClientNetPortalV2 Instance;
        
        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            GameNetPortalV2.Instance.OnNetworkReadied += OnNetworkReady;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
        }
        
        private void OnDestroy()
        {
            if (GameNetPortalV2.Instance is null) return;
            GameNetPortalV2.Instance.OnNetworkReadied -= OnNetworkReady;
            
            if (NetworkManager.Singleton is null) return;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
        }

        public void OnNetworkReady()
        {
            //enabled = NetworkManager.Singleton.IsClient;
            if (!NetworkManager.Singleton.IsClient) return;

            if (NetworkManager.Singleton.IsHost) return;
            GameNetPortalV2.Instance.OnUserDisconnectRequested += OnUserDisconnectRequested;
        }

        public void StartClient(string playerName)
        {
            GameNetPortalV2.Instance.SaveClientData(playerName);
            NetworkManager.Singleton.StartClient();
        }
        
//DISCONNECT RELATED CAREFUL USER(GameNetPortal => Client ask to quit(leave button)) != CLIENT(NetworkManager)
//======================================================================================================================

        private void OnUserDisconnectRequested()
        {
            NetworkManager.Singleton.Shutdown();

            HandleClientDisconnect(NetworkManager.Singleton.LocalClientId);

            SceneManager.LoadScene("MainMenuSceneV2");
        }
        
        private void HandleClientDisconnect(ulong clientId)
        {
            Debug.Log($"HandleClientDisconnect IN");
            if (NetworkManager.Singleton.IsConnectedClient || NetworkManager.Singleton.IsHost) return;
            Debug.Log($"HandleClientDisconnect IsConnectedClient and IsHost");
            GameNetPortalV2.Instance.OnUserDisconnectRequested -= OnUserDisconnectRequested;
            if (SceneManager.GetActiveScene().name == "MainMenuSceneV2") return;
            SceneManager.LoadScene("MainMenuSceneV2");
        }
    }
}