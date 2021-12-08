using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace KaizerWaldCode.V2
{
    [RequireComponent(typeof(NetworkObject))]
    public class LobbyUIV2 : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private LobbyPlayerCard[] lobbyPlayerCards;

        [SerializeField] private Button LeaveBtn;

        private NetworkList<LobbyPlayerState> lobbyPlayers;
        
        private void Awake()
        {
            Debug.Log($"Get Called Awake");
            lobbyPlayers = new NetworkList<LobbyPlayerState>();
            Debug.Log($"Awake current List {lobbyPlayers.Count}");
        }
        
        private void Start()
        {
            LeaveBtn.onClick.AddListener(OnLeaveClicked);
            //NEED to be a network object to work with client! (network variable won't work otherwise)
            if (NetworkManager.Singleton.IsClient)
            {
                lobbyPlayers.OnListChanged += HandleLobbyPlayersStateChanged;
            }

            if (NetworkManager.Singleton.IsHost)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
                
                foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
                {
                    HandleClientConnected(client.ClientId);
                }
                
            }
            
            
        }
        
        public override void OnDestroy()
        {
            base.OnDestroy();
            lobbyPlayers.OnListChanged -= HandleLobbyPlayersStateChanged;
            if (NetworkManager.Singleton is null) return;
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
        }
        
        private void HandleClientConnected(ulong clientId)
        {
            PlayerData? playerData = ServerNetPortalV2.Instance.GetPlayerData(clientId);
            
            if (!playerData.HasValue) return;

            lobbyPlayers.Add(new LobbyPlayerState
            (
                clientId,
                playerData.Value.PlayerName,
                false
            ));
        }
        
        private void HandleClientDisconnect(ulong clientId)
        {
            for (int i = 0; i < lobbyPlayers.Count; i++)
            {
                if (lobbyPlayers[i].ClientId == clientId)
                {
                    lobbyPlayers.RemoveAt(i);
                    break;
                }
            }
        }
        
        private void HandleLobbyPlayersStateChanged(NetworkListEvent<LobbyPlayerState> lobbyState)
        {
            for (int i = 0; i < lobbyPlayerCards.Length; i++)
            {
                if (lobbyPlayers.Count > i)
                {
                    lobbyPlayerCards[i].UpdateDisplay(lobbyPlayers[i]);
                }
                else
                {
                    lobbyPlayerCards[i].DisableDisplay();
                }
            }
        }
        
        private void OnLeaveClicked()
        {
            Debug.Log("OnLeaveClicked");
            GameNetPortalV2.Instance.RequestDisconnect();
        }
    }
}