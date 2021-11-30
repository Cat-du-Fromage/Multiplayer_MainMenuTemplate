using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KaizerWaldCode.V2
{
    public class StartupScene : MonoBehaviour
    {
        private void Start() => SceneManager.LoadScene("MainMenuSceneV2");
    }
}
