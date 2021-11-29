using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace KaizerWaldCode
{
    public class MainMenuButtons : MonoBehaviour
    {
        private MainMenuNetwork mainMenuNetwork;
        [SerializeField] private TMP_InputField nameInputField;
        [SerializeField] private Button host;
        [SerializeField] private Button client;
        [SerializeField] private Button quit;

        private void Start()
        {
            mainMenuNetwork ??= GetComponent<MainMenuNetwork>();
            ButtonsEvent();
        }
        
        private void ButtonsEvent()
        {
            host.onClick.AddListener(() =>
            {
                mainMenuNetwork.Host();
            });
            client.onClick.AddListener(() =>
            {
                mainMenuNetwork.Client();
            });
            
            quit.onClick.AddListener(() => Application.Quit());
        }
/*
        private void LoadGame()
        {
            NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
        }
        */
    }
}
