using System;
using JetBrains.Annotations;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KaizerWaldCode.V2
{
    public class ClientNetPortalV2 : MonoBehaviour
    {
        public static ClientNetPortalV2 Instance;

        private NetworkManager NetworkManager;
        private GameNetPortalV2 GameNetPortal;
        private ServerNetPortalV2 ServerPortal;
        
        private void Awake() => Instance = this;

        private void Start()
        {
            //Initialization
            NetworkManager = NetworkManager.Singleton;
            GameNetPortal = GetComponent<GameNetPortalV2>();
            ServerPortal = GetComponent<ServerNetPortalV2>();
            //Events
            GameNetPortal.OnNetworkReadied += OnNetworkReady;
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnect;
        }
        
        private void OnDestroy()
        {
            GameNetPortal.OnNetworkReadied -= OnNetworkReady;
            NetworkManager.OnClientDisconnectCallback -= OnClientDisconnect;
        }

        private void OnNetworkReady()
        {
            enabled = NetworkManager.IsClient;
            
            if (NetworkManager.IsHost) return;
            GameNetPortal.OnUserDisconnectRequested += OnUserDisconnectRequested;
        }
        

        public void StartClient(string playerName)
        {
            GameNetPortal.SaveClientData(playerName);
            NetworkManager.StartClient();
        }
        
//DISCONNECT RELATED CAREFUL USER(GameNetPortal => Client ask to quit(leave button)) != CLIENT(NetworkManager)
//======================================================================================================================

        private void OnUserDisconnectRequested()
        {
            NetworkManager.Shutdown();
            OnClientDisconnect(NetworkManager.LocalClientId);
            SceneManager.LoadScene(GameScene.MainMenu);
        }
        
        private void OnClientDisconnect(ulong clientId)
        {
            if (NetworkManager.IsConnectedClient || NetworkManager.IsHost) return;
            GameNetPortal.OnUserDisconnectRequested -= OnUserDisconnectRequested;
            //Use to disconnect clients when Host/server is out
            if (SceneManager.GetActiveScene().name == GameScene.MainMenu) return;
            SceneManager.LoadScene(GameScene.MainMenu);
        }
    }
}