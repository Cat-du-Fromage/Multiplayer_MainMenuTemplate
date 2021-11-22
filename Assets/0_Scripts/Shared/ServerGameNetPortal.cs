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
            callback(true, null, true, basePosition, null);
            
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
