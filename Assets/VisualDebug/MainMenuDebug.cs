using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace KaizerWaldCode
{
    public class MainMenuDebug : MonoBehaviour
    {
        [SerializeField]private Image image;
        // Start is called before the first frame update
        void Start()
        {
            NetworkManager net = FindObjectOfType<NetworkManager>();
            image.color = net is not null ? Color.green : Color.red;
        }
        
    }
}
