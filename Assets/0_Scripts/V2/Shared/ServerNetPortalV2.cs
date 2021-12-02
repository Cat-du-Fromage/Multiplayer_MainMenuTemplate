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
        
        private void Awake()
        {
            Instance = this;
        }
        
        private void Start()
        {
            clientData = new Dictionary<string, PlayerData>();
            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        }
        
        public void OnNetworkReady()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                enabled = false;
            }
            else
            {
                NetworkManager.Singleton.SceneManager.LoadScene("LobbySceneV2", LoadSceneMode.Single);

                if(NetworkManager.Singleton.IsHost)
                {
                    clientSceneMap[NetworkManager.Singleton.LocalClientId] = ServerScene;
                }
            }
        }

        private void ApprovalCheck(byte[] connectionData, ulong clientId, NetworkManager.ConnectionApprovedDelegate connectionApprovedCallback)
        {
            string payload = System.Text.Encoding.UTF8.GetString(connectionData);
            ConnectionPayload connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload); 
            int clientScene = connectionPayload.clientScene;
            
            // Approval check happens for Host too, but obviously we want it to be approved
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                connectionApprovedCallback(true, null, true, null, null);
                return;
            }
            
            // 1) MUST check if player Guid not already taken
            
            //Populate our dictionaries with the playerData
            clientSceneMap[clientId] = clientScene;
            clientIDToGuid[clientId] = connectionPayload.clientGUID;
            clientData[connectionPayload.clientGUID] = new PlayerData(connectionPayload.playerName, clientId);
            
            connectionApprovedCallback(true, null, true, Vector3.zero, Quaternion.identity);
        }
    }
}
