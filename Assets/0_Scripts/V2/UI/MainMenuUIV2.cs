using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace KaizerWaldCode.V2
{
    public class MainMenuUIV2 : MonoBehaviour
    {
        //UI INTERACTIONS
        //=======================================
        [SerializeField] private TMP_InputField nameInputField;
        [SerializeField] private Button HostButton;
        [SerializeField] private Button ClientButton;
        
        //NET PORTALS
        //=======================================
        private GameNetPortalV2 GameNetPortal;
        private ClientNetPortalV2 ClientNetPortal;
        
        private void Start()
        {
            GameNetPortal = GameNetPortalV2.Instance;
            ClientNetPortal = ClientNetPortalV2.Instance;
            
            HostButton.onClick.AddListener(OnHostClicked);
            ClientButton.onClick.AddListener(OnClientClicked);
        }

        private void OnHostClicked()
        {
            if (!ValidNameInput()) return;
            GameNetPortal.SaveClientData(nameInputField.text);
            NetworkManager.Singleton.StartHost();
        }

        private void OnClientClicked()
        {
            if (!ValidNameInput()) return;
            ClientNetPortal.StartClient(nameInputField.text);
        }

        private bool ValidNameInput()
        {
            if (!string.IsNullOrEmpty(nameInputField.text) && !string.IsNullOrWhiteSpace(nameInputField.text)) return true;
            nameInputField.image.color = Color.red;
            return false;
        }
    }
}
