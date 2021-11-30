using UnityEngine;

namespace KaizerWaldCode.V2
{
    public class ClientNetPortalV2 : MonoBehaviour
    {
        public static ClientNetPortalV2 Instance;
        private void Awake()
        {
            Instance = this;
        }
    }
}