using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KaizerWaldCode
{
    
    
    [RequireComponent(typeof(GameNetPortal))]
    public class ServerGameNetPortal : MonoBehaviour
    {
        private GameNetPortal portal;
        
        /// <summary>
        /// Maps a given client guid to the data for a given client player.
        /// </summary>
        private Dictionary<string, PlayerData> clientData;
        
        /// <summary>
        /// Keeps a list of what clients are in what scenes.
        /// </summary>
        private Dictionary<ulong, int> clientSceneMap = new Dictionary<ulong, int>();
        
        /// <summary>
        /// GUID = Globally Unique Identifier DATA
        /// Map to allow us to cheaply map from guid to player data.
        /// </summary>
        private Dictionary<ulong, string> clientIDToGuid;
        
        /// <summary>
        /// The active server scene index.
        /// </summary>
        public int ServerScene => SceneManager.GetActiveScene().buildIndex;

        void Start()
        {
            portal = GetComponent<GameNetPortal>();
            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCallback;
            NetworkManager.Singleton.OnServerStarted += ServerStartedHandler;
            clientData = new Dictionary<string, PlayerData>();
            clientIDToGuid = new Dictionary<ulong, string>();
        }
        
        /// <summary>
        /// called In GameNetPortal.cs (function OnNetworkReady())
        /// </summary>
        public void OnNetworkReady()
        {
            if (!portal.networkManager.IsServer)
            {
                enabled = false;
            }
            else
            {
                
                //O__O if adding any event registrations here, please add an unregistration in OnClientDisconnect.
                portal.networkManager.OnClientDisconnectCallback += OnClientDisconnect;
                
                NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);

                if( portal.networkManager.IsHost)
                {
                    clientSceneMap[portal.networkManager.LocalClientId] = ServerScene;
                }
            }
        }

        private void OnClientDisconnect(ulong clientId)
        {
            clientSceneMap.Remove(clientId); //scene clean
            if( clientIDToGuid.TryGetValue(clientId, out string guid ) )
            {
                clientIDToGuid.Remove(clientId);

                if( clientData[guid].ClientID == clientId )
                {
                    //be careful to only remove the ClientData if it is associated with THIS clientId; in a case where a new connection
                    //for the same GUID kicks the old connection, this could get complicated. In a game that fully supported the reconnect flow,
                    //we would NOT remove ClientData here, but instead time it out after a certain period, since the whole point of it is
                    //to remember client information on a per-guid basis after the connection has been lost.
                    clientData.Remove(guid);
                }
            }
            //================ FLORIAN NOTE ======================================================
            //Why don't we use ServerClientId? => ServerClientId only viable in a server hosting
            //LocalClientId allow to be viable on Host AND server
            //ServerClientId always 0 , LocalClientId in general == 0 but in certain case(host change) may be not
            if( clientId == portal.networkManager.LocalClientId )
            {
                //the ServerGameNetPortal may be initialized again, which will cause its OnNetworkSpawn to be called again.
                //Consequently we need to unregister anything we registered, when the NetworkManager is shutting down.
                portal.networkManager.OnClientDisconnectCallback -= OnClientDisconnect;
            }
        }
        
        /// <summary>
        /// Handles the flow when a user has requested a disconnect via UI (which can be invoked on the Host, and thus must be
        /// handled in server code).
        /// </summary>
        public void OnUserDisconnectRequest() => Clear();

        private void Clear()
        {
            //resets all our runtime state.
            clientData.Clear();
            clientIDToGuid.Clear();
            clientSceneMap.Clear();
        }
        
        /// <summary>
        /// CAREFUL use only this if you want to restrict connection!
        /// for exemple :Password or number of player
        /// </summary>
        /// <param name="connectionData"></param>
        /// <param name="clientId"></param>
        /// <param name="callback"></param>
        private void ApprovalCallback(byte[] connectionData, ulong clientId, NetworkManager.ConnectionApprovedDelegate callback)
        {
            // Approval check happens for Host too, but obviously we want it to be approved
            Vector3 basePosition = new Vector3(-2.5f, 1, -10.5f);
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                callback(true, null, true, new Vector3(-2.5f,1,-10.5f), null);
                return;
            }

            basePosition.x += clientId * 1.5f;
            
            
            //Test for Duplicate Login.
            string payload = System.Text.Encoding.UTF8.GetString(connectionData);
            ConnectionPayload connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload); // https://docs.unity3d.com/2020.2/Documentation/Manual/JSONSerialization.html
//clientScene : {connectionPayload.clientScene.ToString()}; 
            Debug.Log($"playerName : {connectionPayload.playerName.ToString()}; clientGUID : {connectionPayload.clientGUID.ToString()};");
            
            int clientScene = connectionPayload.clientScene;

            Dictionary<string, PlayerData>.KeyCollection.Enumerator test = clientData.Keys.GetEnumerator();
            while (test.MoveNext())
                Debug.Log($"Same Guid : {test.Current}");
            
            if (clientData.ContainsKey(connectionPayload.clientGUID))
            {
                if (Debug.isDebugBuild)
                {
                    Debug.Log($"Client GUID {connectionPayload.clientGUID} already exists. Because this is a debug build, we will still accept the connection");
                    while (clientData.ContainsKey(connectionPayload.clientGUID)) { connectionPayload.clientGUID += "_Secondary"; }
                }
                else
                {
                    Debug.Log($"Same Guid : {clientData.ContainsKey(connectionPayload.clientGUID)}");
                    ulong oldClientId = clientData[connectionPayload.clientGUID].ClientID;
                    // kicking old client to leave only current
                    //SendServerToClientSetDisconnectReason(oldClientId, ConnectStatus.LoggedInAgain);
                    StartCoroutine(WaitToDisconnect(clientId));
                    return;
                }
            }
            //Populate our dictionaries with the playerData
            clientSceneMap[clientId] = clientScene;
            clientIDToGuid[clientId] = connectionPayload.clientGUID;
            clientData[connectionPayload.clientGUID] = new PlayerData(connectionPayload.playerName, clientId);
            
            callback(true, null, true, basePosition, null);
        }
        
        private IEnumerator WaitToDisconnect(ulong clientId)
        {
            yield return new WaitForSeconds(0.5f);
            portal.networkManager.DisconnectClient(clientId);
        }
        
        private void ServerStartedHandler()
        {
            clientData.Add("host_guid", new PlayerData("HOST", NetworkManager.Singleton.LocalClientId));
            clientIDToGuid.Add(NetworkManager.Singleton.LocalClientId, "host_guid");

            //AssignPlayerName(NetworkManager.Singleton.LocalClientId, "HOST");

            // server spawns game state
            //var gameState = Instantiate(m_GameState);

            //gameState.Spawn();
        }

        /// <summary>
        /// Register the scene the player belongs to
        /// call from GameNetPortal : OnSceneEvent
        /// </summary>
        /// <param name="clientId">client Id</param>
        /// <param name="sceneIndex">scene the player in sync with(host/server scene)</param>
        public void OnClientSceneChanged(ulong clientId, int sceneIndex) => clientSceneMap[clientId] = sceneIndex;

    }
}
