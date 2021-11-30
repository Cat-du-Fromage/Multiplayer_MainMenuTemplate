using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

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

        private void ApprovalCheck(byte[] connectionData, ulong clientId, NetworkManager.ConnectionApprovedDelegate connectionApprovedCallback)
        {
            
        }
    }
}
