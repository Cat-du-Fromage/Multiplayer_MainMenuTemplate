using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWaldCode
{
    /// <summary>
    /// Client side logic for a GameNetPortal. Contains implementations for all of GameNetPortal's S2C RPCs.
    /// </summary>
    [RequireComponent(typeof(GameNetPortal))]
    public class ClientGameNetPortal : MonoBehaviour
    {
        public static ClientGameNetPortal Instance;
        private GameNetPortal portal;
        
        void Start()
        {
        
        }
        
        public void OnConnectFinished(ConnectStatus status)
        {
            //on success, there is nothing to do (the Netcode for GameObjects (Netcode) scene management system will take us to the next scene).
            //on failure, we must raise an event so that the UI layer can display something.
            Debug.Log("RecvConnectFinished Got status: " + status);
            /*
            if( status != ConnectStatus.Success )
            {
                //this indicates a game level failure, rather than a network failure. See note in ServerGameNetPortal.
                DisconnectReason.SetDisconnectReason(status);
            }
*/
            //ConnectFinished?.Invoke(status);
        }
    }
}
