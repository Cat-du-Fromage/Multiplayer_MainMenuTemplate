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
        private GameNetPortalV2 gameNetPortal;
        private ClientNetPortalV2 clientNetPortal;
        
        private void Start()
        {
            gameNetPortal = GameNetPortalV2.Instance;
            clientNetPortal = ClientNetPortalV2.Instance;
            
            HostButton.onClick.AddListener(OnHostClicked);
            ClientButton.onClick.AddListener(OnClientClicked);
        }
        
        //RETRIEVE HOST NAME
        //private static string clientInputName = string.Empty;
        //public static string GetInputName() => clientInputName;

        private void OnHostClicked()
        {
            if (!ValidNameInput()) return;
            //clientInputName = nameInputField.text;
            gameNetPortal.SaveClientData(nameInputField.text);
            NetworkManager.Singleton.StartHost();
        }

        private void OnClientClicked()
        {
            if (!ValidNameInput()) return;
            clientNetPortal.StartClient(nameInputField.text);
        }

        private bool ValidNameInput()
        {
            if (!string.IsNullOrEmpty(nameInputField.text) && !string.IsNullOrWhiteSpace(nameInputField.text)) return true;
            nameInputField.image.color = Color.red;
            return false;
        }
    }
}
