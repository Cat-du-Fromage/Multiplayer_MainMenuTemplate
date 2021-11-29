using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace KaizerWaldCode
{
    public class LobbyButtons : MonoBehaviour
    {
        public GameObject netPortalObj;
        public GameNetPortal netPortal;
        [SerializeField] private Button leave;
        // Start is called before the first frame update
        void Start()
        {
            netPortalObj ??= FindObjectOfType<GameNetPortal>().gameObject;
            netPortal = FindObjectOfType<GameNetPortal>().GetComponent<GameNetPortal>();
            ButtonsEvent();
        }

        private void ButtonsEvent()
        {
            leave.onClick.AddListener(() =>
            {
                netPortal.RequestDisconnect();
                SceneManager.LoadScene("MainMenuScene");
            });
        }
    }
}
