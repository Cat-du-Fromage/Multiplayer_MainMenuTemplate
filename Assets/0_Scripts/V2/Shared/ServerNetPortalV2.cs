using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
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
        private Dictionary<string, PlayerData> clientData;
        
        /// <summary>
        /// Map to allow us to cheaply map from guid to player data.
        /// </summary>
        private Dictionary<ulong, string> clientIDToGuid;
        
        /// <summary>
        /// Keeps a list of what clients are in what scenes.
        /// </summary>
        private Dictionary<ulong, int> clientSceneMap = new Dictionary<ulong, int>();
        
        /// <summary>
        /// The active server scene index.
        /// </summary>
        public int ServerScene => SceneManager.GetActiveScene().buildIndex;

        public static ServerNetPortalV2 Instance;

        public void DebugPlayerData()
        {
            foreach (KeyValuePair<string, PlayerData> v in clientData)
            {
                Debug.Log($"Player connected : Key = {v.Key} ClientID = {v.Value.ClientID} PlayerName = {v.Value.PlayerName}");
            }
        }
        
        private void Awake()
        {
            Instance = this;
        }
        
        private void Start()
        {
            GameNetPortalV2.Instance.OnNetworkReadied += OnNetworkReady;
            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
            clientData = new Dictionary<string, PlayerData>();
            clientIDToGuid = new Dictionary<ulong, string>();
        }
        
        private void OnDestroy()
        {
            if (GameNetPortalV2.Instance is null) return;
            GameNetPortalV2.Instance.OnNetworkReadied -= OnNetworkReady;
            if (NetworkManager.Singleton is null) return;
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
        }

        private void OnNetworkReady()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                enabled = false;
            }
            else
            {
                SetDisconnectEvents(true);
                
                NetworkManager.Singleton.SceneManager.LoadScene("LobbySceneV2", LoadSceneMode.Single);

                if(NetworkManager.Singleton.IsHost)
                {
                    clientSceneMap[NetworkManager.Singleton.LocalClientId] = ServerScene;
                }
            }
        }
        
        public PlayerData? GetPlayerData(ulong clientId)
        {
            //First see if we have a guid matching the clientID given.

            if (clientIDToGuid.TryGetValue(clientId, out string clientguid))
            {
                if (clientData.TryGetValue(clientguid, out PlayerData data))
                {
                    return data;
                }
                else
                {
                    Debug.Log("No PlayerData of matching guid found");
                }
            }
            else
            {
                Debug.Log("No client guid found mapped to the given client ID");
            }
            return null;
        }

        private void ApprovalCheck(byte[] connectionData, ulong clientId, NetworkManager.ConnectionApprovedDelegate connectionApprovedCallback)
        {
            DebugPlayerData();
            string payload = System.Text.Encoding.UTF8.GetString(connectionData);
            ConnectionPayload connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload); 
            int clientScene = connectionPayload.clientScene;
            // Approval check happens for Host too, but obviously we want it to be approved
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                connectionApprovedCallback(false, null, true, null, null);
                RegisterClientData(clientId, clientScene, connectionPayload);
                return;
            }
            
            
            
            // 1) MUST check if player Guid not already taken
            RegisterClientData(clientId, clientScene, connectionPayload);
            //Populate our dictionaries with the playerData
            
            
            connectionApprovedCallback(false, null, true, null, null);
            DebugPlayerData();
        }

        private void RegisterClientData(ulong clientId, int clientScene, ConnectionPayload connectionPayload)
        {
            clientSceneMap[clientId] = clientScene;
            clientIDToGuid[clientId] = connectionPayload.clientGUID;
            clientData[connectionPayload.clientGUID] = new PlayerData(connectionPayload.playerName, clientId);
        }
        
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
            clientData.Clear();
            clientIDToGuid.Clear();
            clientSceneMap.Clear();
        }
        
        private void OnClientDisconnected(ulong clientId)
        {
            clientSceneMap.Remove(clientId);

            if (clientIDToGuid.TryGetValue(clientId, out string guid))
            {
                clientIDToGuid.Remove(clientId);

                if (clientData[guid].ClientID == clientId)
                {
                    clientData.Remove(guid);
                }
            }

            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                SetDisconnectEvents(false);
                //gameNetPortal.OnClientSceneChanged -= HandleClientSceneChanged;
            }
        }
        
        private void OnUserDisconnectRequested()
        {
            OnClientDisconnected(NetworkManager.Singleton.LocalClientId);

            NetworkManager.Singleton.Shutdown();

            ClearData();

            SceneManager.LoadScene("MainMenuSceneV2");
        }
    }
}
