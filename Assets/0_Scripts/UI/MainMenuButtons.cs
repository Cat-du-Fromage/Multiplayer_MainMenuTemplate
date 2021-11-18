using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace KaizerWaldCode
{
    public class MainMenuButtons : MonoBehaviour
    {
        [SerializeField]private Button host;
        [SerializeField]private Button client;
        [SerializeField]private Button quit;
        private void Start()
        {
            host.onClick.AddListener(() =>
            {
                NetworkManager.Singleton.StartHost();
            });
            client.onClick.AddListener(() =>
            {
                NetworkManager.Singleton.StartClient();
            });
            
            quit.onClick.AddListener(() => Application.Quit());
        }

        
    }
}
