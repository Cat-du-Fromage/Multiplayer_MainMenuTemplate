using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KaizerWaldCode.V2
{
    public class ClientNetPortalV2 : MonoBehaviour
    {
        public static ClientNetPortalV2 Instance;
        private void Awake()
        {
            Instance = this;
        }
        
        private static void ConnectClient(string playerName)
        {
            string clientGuid = ClientPrefs.GetGuid();
            string payload = JsonUtility.ToJson(new ConnectionPayload()
            {
                clientGUID = clientGuid,
                clientScene = SceneManager.GetActiveScene().buildIndex,
                playerName = playerName
            });

            byte[] payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);

            NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

            NetworkManager.Singleton.StartClient();
        }
    }
}