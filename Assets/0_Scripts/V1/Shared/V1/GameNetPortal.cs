using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KaizerWaldCode
{
    public enum ConnectStatus
    {
        Undefined,
        Success,                  //client successfully connected. This may also be a successful reconnect.
        ServerFull,               //can't join, server is already at capacity.
        LoggedInAgain,            //logged in on a separate client, causing this one to be kicked out.
        UserRequestedDisconnect,  //Intentional Disconnect triggered by the user.
        GenericDisconnect,        //server disconnected, but no specific reason given.
    }
    
    [Serializable]
    public class ConnectionPayload
    {
        public string clientGUID;
        public int clientScene = -1;
        public string playerName;
    }
    
    /// <remarks>
    /// Why is there a C2S_ConnectFinished event here? How is that different from the "ApprovalCheck" logic that MLAPI optionally runs
    /// when establishing a new client connection?
    /// MLAPI's ApprovalCheck logic doesn't offer a way to return rich data. We need to know certain things directly upon logging in, such as
    /// whether the game-layer even wants us to join (we could fail because the server is full, or some other non network related reason), and also
    /// what BossRoomState to transition to. We do this with a Custom Named Message, which fires on the server immediately after the approval check delegate
    /// has run.
    ///
    /// Why do we need to send a client GUID? What is it? Don't we already have a clientID?
    /// ClientIDs are assigned on login. If you connect to a server, then your connection drops, and you reconnect, you get a new ClientID. This
    /// makes it awkward to get back your old character, which the server is going to hold onto for a fixed timeout. To properly reconnect and recover
    /// your character, you need a persistent identifier for your own client install. We solve that by generating a random GUID and storing it
    /// in player prefs, so it persists across sessions of the game.
    /// </remarks>
    public class GameNetPortal : MonoBehaviour
    {
        public NetworkManager networkManager;
        
        public static GameNetPortal Instance;
        
        public ClientGameNetPortal clientPortal;
        public ServerGameNetPortal serverPortal;

        private void Awake()
        {
            Instance = this;
            clientPortal = GetComponent<ClientGameNetPortal>();
            serverPortal = GetComponent<ServerGameNetPortal>();
        }

        void Start()
        {
            DontDestroyOnLoad(gameObject);
            networkManager ??= FindObjectOfType<NetworkManager>();
            networkManager.OnServerStarted += OnNetworkReady; //coming from startHost from MainMenuNetWork.cs
            networkManager.OnClientConnectedCallback += ClientNetworkReadyWrapper;
        }

        private void ClientNetworkReadyWrapper(ulong clientId)
        {
            //================ FLORIAN NOTE ======================================================
            //Client never pass the condition here in theory
            //CAREFUL networkManager.LocalClientId should be in general == 0
            //(in some case in a host system it may be different(host disconnect => change host))
            if (clientId != networkManager.LocalClientId) return; 
            OnNetworkReady();
            //================ FLORIAN NOTE ======================================================
            //BIG OVERHEAD ON THIS : see https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/blob/develop/com.unity.netcode.gameobjects/Runtime/SceneManagement/SceneEventData.cs
            //basically it will sync all client with the HOST
            networkManager.SceneManager.OnSceneEvent += OnSceneEvent; //Client will use this first before recalling ClientNetworkReadyWrapper
        }
        
        /// <summary>
        /// Callback of the NetCode SceneEvent
        /// Called when a client connect => sync the client scene with the host scene
        /// </summary>
        /// <param name="sceneEvent">track the progression of the scene transition</param>
        private void OnSceneEvent(SceneEvent sceneEvent)
        {
            // only processing single player finishing loading events
            if (sceneEvent.SceneEventType != SceneEventType.LoadComplete) return;
            serverPortal.OnClientSceneChanged(sceneEvent.ClientId, SceneManager.GetSceneByName(sceneEvent.SceneName).buildIndex);
        }

        /// <summary>
        /// This method runs when NetworkManager has started up
        /// (following a succesful connect on the client, or directly after StartHost is invoked on the host).
        /// It is named to match NetworkBehaviour.OnNetworkSpawn, and serves the same role,
        /// even though GameNetPortal itself isn't a NetworkBehaviour.
        /// </summary>
        private void OnNetworkReady()
        {
            if (networkManager.IsHost)
            {
                //special host code. This is what kicks off the flow that happens on a regular client
                //when it has finished connecting successfully. A dedicated server would remove this.
                //clientPortal.OnConnectFinished(ConnectStatus.Success);
                serverPortal.OnNetworkReady(); // => Go to ServerNetPortal.cs
            }
        }
        
        
        /// <summary>
        /// This will disconnect (on the client) or shutdown the server (on the host).
        /// It's a local signal (not from the network), indicating that the user has requested a disconnect.
        /// </summary>
        public void RequestDisconnect()
        {
            //clientPortal.OnUserDisconnectRequest(); => no need for disconnect reason yet
            serverPortal.OnUserDisconnectRequest();
            networkManager.Shutdown();
        }
    }
}
