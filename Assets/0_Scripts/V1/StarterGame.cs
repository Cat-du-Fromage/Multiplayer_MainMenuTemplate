using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KaizerWaldCode
{
    public class StarterGame : MonoBehaviour
    {
        // Only one Purpose Load the Very firstScene
        // why?
        // avoid the duplication of "DontDestroyOnLoad" Gameobjects when going back to main Menu
        private void Start() => SceneManager.LoadScene("MainMenuScene");
    }
}
