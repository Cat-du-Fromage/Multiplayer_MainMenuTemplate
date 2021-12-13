using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KaizerWaldCode.V2
{
    public struct PlayerData
    {
        public string PlayerName;
        public ulong ClientID;

        public PlayerData(string playerName, ulong clientID)
        {
            PlayerName = playerName;
            ClientID = clientID;
        }
    }
    
    public class ServerNetPortalV2 : MonoBehaviour
    {
        [SerializeField]
        private NetworkObject GameState;
        
        private NetworkManager NetworkManager;
        private GameNetPortalV2 GameNetPortal;
        private ClientNetPortalV2 ClientPortal;
        
        private Dictionary<string, PlayerData> ClientData;
        
        /// <summary>
        /// Map to allow us to cheaply map from guid to player data.
        /// </summary>
        private Dictionary<ulong, string> ClientIDToGuid;
        
        /// <summary>
        /// Keeps a list of what clients are in what scenes.
        /// </summary>
        private Dictionary<ulong, int> ClientSceneMap = new Dictionary<ulong, int>();
        
        /// <summary>
        /// The active server scene index.
        /// </summary>
        private int ServerScene => SceneManager.GetActiveScene().buildIndex;

        public static ServerNetPortalV2 Instance;

        private void DebugPlayerData()
        {
            foreach (KeyValuePair<string, PlayerData> v in ClientData)
                NetworkLog.LogInfoServer($"Player connected : Key = {v.Key} ClientID = {v.Value.ClientID} PlayerName = {v.Value.PlayerName}");
        }
        
        private void Awake() => Instance = this;

        private void Start()
        {
            //Initialization Part
            NetworkManager = NetworkManager.Singleton;
            GameNetPortal = GetComponent<GameNetPortalV2>();
            ClientPortal = GetComponent<ClientNetPortalV2>();
            //Events
            GameNetPortal.OnNetworkReadied += OnNetworkReady;
            NetworkManager.ConnectionApprovalCallback += ApprovalCheck;
            //Init Dictionaries
            ClientData = new Dictionary<string, PlayerData>();
            ClientIDToGuid = new Dictionary<ulong, string>();
        }
        
        private void OnDestroy()
        {
            if (GameNetPortal is null) return;
            GameNetPortal.OnNetworkReadied -= OnNetworkReady;
            if (NetworkManager is null) return;
            NetworkManager.ConnectionApprovalCallback -= ApprovalCheck;
        }

        private void OnNetworkReady()
        {
            enabled = NetworkManager.IsServer;

            if(NetworkManager.IsHost)
                ClientSceneMap[NetworkManager.LocalClientId] = ServerScene;

            if (!NetworkManager.IsServer) return;
            
            SetDisconnectEvents(true);
                
            NetworkManager.SceneManager.LoadScene(GameScene.Lobby, LoadSceneMode.Single);
        }
        
        public PlayerData? GetPlayerData(ulong clientId)
        {
            //First see if we have a guid matching the clientID given.

            if (ClientIDToGuid.TryGetValue(clientId, out string clientGuid))
            {
                if (ClientData.TryGetValue(clientGuid, out PlayerData data)) return data;
                NetworkLog.LogInfoServer("No PlayerData of matching guid found");
            }
            else
            {
                NetworkLog.LogInfoServer("No client guid found mapped to the given client ID");
            }
            return null;
        }

        private void ApprovalCheck(byte[] connectionData, ulong clientId, NetworkManager.ConnectionApprovedDelegate connectionApprovedCallback)
        {
            string payload = System.Text.Encoding.UTF8.GetString(connectionData);
            ConnectionPayload connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload); 
            int clientScene = connectionPayload.clientScene;
            
            // Approval check happens for Host too, but obviously we want it to be approved
            if (clientId == NetworkManager.LocalClientId)
            {
                connectionApprovedCallback(false, null, true, null, null);
                NetworkLog.LogInfoServer($"ApprovalCheck Host : {clientId} {NetworkManager.LocalClientId}");
                RegisterClientData(clientId, clientScene, connectionPayload);
                return;
            }
            
            //Test for Duplicate Login(with GUID).
            if (ClientData.ContainsKey(connectionPayload.clientGUID))
            {
                if (Debug.isDebugBuild)
                {
                    NetworkLog.LogInfoServer($"Client GUID {connectionPayload.clientGUID} already exists. Because this is a debug build, we will still accept the connection");
                    while (ClientData.ContainsKey(connectionPayload.clientGUID)) { connectionPayload.clientGUID += "_Secondary"; }
                }
                else
                {
                    NetworkLog.LogInfoServer($"We got a duplicate!");
                    ulong oldClientId = ClientData[connectionPayload.clientGUID].ClientID;
                    // kicking old client to leave only current
                    StartCoroutine(WaitToDisconnect(clientId));
                    return;
                }
            }

            DebugPlayerData();
            RegisterClientData(clientId, clientScene, connectionPayload);
            connectionApprovedCallback(false, null, true, null, null);
        }
        
        private IEnumerator WaitToDisconnect(ulong clientId)
        {
            yield return new WaitForSeconds(0.5f);
            NetworkManager.DisconnectClient(clientId);
        }

        private void RegisterClientData(ulong clientId, int clientScene, ConnectionPayload connectionPayload)
        {
            ClientSceneMap[clientId] = clientScene;
            ClientIDToGuid[clientId] = connectionPayload.clientGUID;
            ClientData[connectionPayload.clientGUID] = new PlayerData(connectionPayload.playerName, clientId);
        }
        
        public void StartGame() => NetworkManager.Singleton.SceneManager.LoadScene(GameScene.Game, LoadSceneMode.Single);

//DISCONNECT RELATED
//======================================================================================================================

        /// <summary>
        /// Enable/Disable Disconnection events related
        /// </summary>
        /// <param name="state">true to enable / false to disable</param>
        private void SetDisconnectEvents(bool state)
        {
            if (state)
            {
                GameNetPortalV2.Instance.OnUserDisconnectRequested += OnUserDisconnectRequested;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            }
            else
            {
                GameNetPortalV2.Instance.OnUserDisconnectRequested -= OnUserDisconnectRequested;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }
        }

        private void ClearData()
        {
            ClientData.Clear();
            ClientIDToGuid.Clear();
            ClientSceneMap.Clear();
        }
        
        private void OnClientDisconnected(ulong clientId)
        {
            ClientSceneMap.Remove(clientId);

            if (ClientIDToGuid.TryGetValue(clientId, out string guid))
            {
                ClientIDToGuid.Remove(clientId);

                if (ClientData[guid].ClientID == clientId)
                {
                    ClientData.Remove(guid);
                }
            }

            if (clientId != NetworkManager.LocalClientId) return; 
            SetDisconnectEvents(false);
        }
        
        private void OnUserDisconnectRequested()
        {
            OnClientDisconnected(NetworkManager.LocalClientId);

            NetworkManager.Shutdown();

            ClearData();

            SceneManager.LoadScene(GameScene.MainMenu);
        }
    }
}
