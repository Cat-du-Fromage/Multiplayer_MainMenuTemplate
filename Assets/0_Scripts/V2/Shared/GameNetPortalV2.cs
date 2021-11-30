using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWaldCode.V2
{
    public class GameNetPortalV2 : MonoBehaviour
    {
        public static GameNetPortalV2 Instance;

        private void Awake()
        {
            Instance = this;
        }
        
        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
