using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using TMPro;
using UnityEngine;

namespace KaizerWaldCode
{
    public class LobbyNetworkManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text hostField;
        [SerializeField] private TMP_Text clientField;
        
        private Dictionary<ulong, string> clientPseudo;
        
        private void Start()
        {
            if (NetworkManager.Singleton.IsHost)
            {
                //hostField.text = MainMenuButtons.GetPlayerName(NetworkManager.Singleton.ServerClientId);
                NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
            }
        }
        
        private void HandleClientDisconnect(ulong obj)
        {
            clientField.text = "Client Identity";
        }

        private void HandleClientConnected(ulong obj)
        {
            //clientField.text = MainMenuButtons.GetPlayerName(obj);
        }
    }
}
