using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace KaizerWaldCode.V2
{
    //NEED to be a network object to work with client! (network variable won't work otherwise)
    [RequireComponent(typeof(NetworkObject))]
    public class LobbyUIV2 : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private LobbyPlayerCard[] lobbyPlayerCards;

        [SerializeField] private Button leaveBtn;
        [SerializeField] private Button readyBtn;
        [SerializeField] private Button startBtn;
        private NetworkList<LobbyPlayerState> LobbyPlayers;
        
        private void Awake() => LobbyPlayers = new NetworkList<LobbyPlayerState>();

        private void Start()
        {
            startBtn.gameObject.SetActive(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer);
            leaveBtn.onClick.AddListener(OnLeaveClicked);
            readyBtn.onClick.AddListener(OnReadyClicked);

            // CLIENT/HOST ONLY
            if (NetworkManager.Singleton.IsClient)
                LobbyPlayers.OnListChanged += OnLobbyPlayersStateChanged;

            // HOST ONLY
            if (NetworkManager.Singleton.IsHost)
            {
                startBtn.onClick.AddListener(OnStartClicked);
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
                
                foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
                {
                    OnClientConnected(client.ClientId);
                }
            }
        }
        
        public override void OnDestroy()
        {
            base.OnDestroy();
            LobbyPlayers.OnListChanged -= OnLobbyPlayersStateChanged;
            if (NetworkManager.Singleton is not null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
            }
        }
        
        private void OnClientConnected(ulong clientId)
        {
            PlayerData? playerData = ServerNetPortalV2.Instance.GetPlayerData(clientId);
            
            if (playerData.HasValue)
            {
                LobbyPlayers.Add(new LobbyPlayerState
                (
                    clientId,
                    playerData.Value.PlayerName,
                    false
                ));
            }
        }
        
        private void OnClientDisconnect(ulong clientId)
        {
            for (int i = 0; i < LobbyPlayers.Count; i++)
            {
                if (LobbyPlayers[i].ClientId != clientId) continue;
                LobbyPlayers.RemoveAt(i);
                break;
            }
        }
        
        private void OnLobbyPlayersStateChanged(NetworkListEvent<LobbyPlayerState> lobbyState)
        {
            for (int i = 0; i < lobbyPlayerCards.Length; i++)
            {
                if (LobbyPlayers.Count > i)
                    lobbyPlayerCards[i].UpdateDisplay(LobbyPlayers[i]);
                else
                    lobbyPlayerCards[i].DisableDisplay();
            }
        }
        
        private bool IsEveryoneReady()
        {
            if (LobbyPlayers.Count < 1) return false;

            foreach (LobbyPlayerState player in LobbyPlayers)
            {
                if (!player.IsReady) return false;
            }
            return true;
        }
        
        private void OnLeaveClicked() => GameNetPortalV2.Instance.RequestDisconnect();
        
        private void OnReadyClicked() => ToggleReadyServerRpc();

        private void OnStartClicked() => StartGameServerRpc();

//RPC
//======================================================================================================================

        [ServerRpc(RequireOwnership = false)]
        private void StartGameServerRpc(ServerRpcParams serverRpcParams = default)
        {
            if (serverRpcParams.Receive.SenderClientId != NetworkManager.Singleton.LocalClientId) { return; }

            if (!IsEveryoneReady()) return;

            NetworkLog.LogInfoServer($"NETWORK Game Can Start");
            ServerNetPortalV2.Instance.StartGame();
        }


        [ServerRpc(RequireOwnership = false)]
        private void ToggleReadyServerRpc(ServerRpcParams serverRpcParams = default)
        {
            for (int i = 0; i < LobbyPlayers.Count; i++)
            {
                if (LobbyPlayers[i].ClientId == serverRpcParams.Receive.SenderClientId)
                {
                    LobbyPlayers[i] = new LobbyPlayerState
                    (
                        LobbyPlayers[i].ClientId,
                        LobbyPlayers[i].PlayerName,
                        !LobbyPlayers[i].IsReady //get the inverse state
                    );
                }
            }
        }
    }
}