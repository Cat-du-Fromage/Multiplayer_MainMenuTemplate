using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace KaizerWaldCode.V2
{
    /// <summary>
    /// We need a networkBehaviour to Spawn NetworkObject
    /// With ServerNetPortal(Monobehaviour) we got an error(can't fin hash prefab)
    /// </summary>
    public class ServerGameState : NetworkBehaviour
    {
        [SerializeField]
        [Tooltip("Make sure this is included in the NetworkManager's list of prefabs!")]
        private NetworkObject playerPrefab;

        [SerializeField]
        [Tooltip("A collection of locations for spawning players")]
        private Transform[] playerSpawnPoints;
        
        private List<Transform> PlayerSpawnPointsList = null;
        
        private GameNetPortalV2 GameNetPortal;
        private ServerNetPortalV2 ServerNetPortal;
        
        /// <summary>
        /// Has the ServerBossRoomState already hit its initial spawn? (i.e. spawned players following load from character select).
        /// </summary>
        public bool InitialSpawnDone { get; private set; }
        
        public override void OnNetworkSpawn()
        {
            if (!IsServer)
            {
                enabled = false;
                NetworkLog.LogInfoServer("Not Server so enable = false");
            }
            else
            {
                GameNetPortal = GameObject.Find("GameNetPortalV2").GetComponent<GameNetPortalV2>();
                ServerNetPortal = GameNetPortal.GetComponent<ServerNetPortalV2>();

                NetworkManager.SceneManager.OnSceneEvent += OnClientSceneChanged;
                
                DoInitialSpawnIfPossible();
            }
        }
        
        private bool DoInitialSpawnIfPossible()
        {
            if (!InitialSpawnDone)
            {
                InitialSpawnDone = true;
                foreach (KeyValuePair<ulong, NetworkClient> kvp in NetworkManager.ConnectedClients)
                {
                    SpawnPlayer(kvp.Key, false);
                }
                return true;
            }
            return false;
        }

        private void OnClientSceneChanged(SceneEvent sceneEvent)
        {
            if (sceneEvent.SceneEventType != SceneEventType.LoadComplete) return;

            ulong clientId = sceneEvent.ClientId;
            int sceneIndex = SceneManager.GetSceneByName(sceneEvent.SceneName).buildIndex;
            int serverScene = SceneManager.GetActiveScene().buildIndex;
            
            if (sceneIndex == serverScene)
            {
                Debug.Log($"client={clientId} now in scene {sceneIndex}, server_scene={serverScene}");
                bool didSpawn = DoInitialSpawnIfPossible();
            }
        }
        
        private void SpawnPlayer(ulong clientId, bool lateJoin)
        {
            if (PlayerSpawnPointsList is null || PlayerSpawnPointsList.Count == 0)
            {
                PlayerSpawnPointsList = new List<Transform>(playerSpawnPoints);
            }

            Debug.Assert(PlayerSpawnPointsList.Count > 0, $"PlayerSpawnPoints array should have at least 1 spawn points.");

            //REMOVE TAKEN LOCATION
            int index = NetworkManager.Singleton.IsHost ? 0 : Random.Range(1, PlayerSpawnPointsList.Count);
            Transform spawnPoint = PlayerSpawnPointsList[index];
            PlayerSpawnPointsList.RemoveAt(index);
            
            NetworkObject newPlayer = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
            
            newPlayer.SpawnWithOwnership(clientId, true);
        }
    }
}
