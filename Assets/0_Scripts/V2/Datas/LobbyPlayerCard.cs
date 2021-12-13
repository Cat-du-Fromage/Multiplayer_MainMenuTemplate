using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace KaizerWaldCode.V2
{
    public class LobbyPlayerCard : MonoBehaviour
    {

        [Header("Data Display")]
        [SerializeField] private TMP_Text playerDisplayNameText;
        [SerializeField] private Toggle isReadyToggle;

        public void UpdateDisplay(LobbyPlayerState lobbyPlayerState)
        {
            playerDisplayNameText.text = lobbyPlayerState.PlayerName.ToString();
            isReadyToggle.isOn = lobbyPlayerState.IsReady;
        }

        public void DisableDisplay()
        {
            playerDisplayNameText.text = "Available";
            isReadyToggle.isOn = false;
        }
    }
}