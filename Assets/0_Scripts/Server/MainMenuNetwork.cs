using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace KaizerWaldCode
{
    public class MainMenuNetwork : MonoBehaviour
    {
        private GameNetPortal gameNetPortal;

        private ClientGameNetPortal clientNetPortal;

        private void Start()
        {
            GameObject GamePortalGO = GameObject.Find("GameNetPortal");
            gameNetPortal = GamePortalGO.GetComponent<GameNetPortal>();
            clientNetPortal = GamePortalGO.GetComponent<ClientGameNetPortal>();
        }

        public void Host()
        {
            NetworkManager.Singleton.StartHost(); // => see OnNetworkReady in GameNetPortal.cs
        }
        
        public void Client()
        {
            NetworkManager.Singleton.StartClient();
        }
    }
}
