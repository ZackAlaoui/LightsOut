using UnityEngine;
using UnityEngine.SceneManagement;
using Game;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        GameManager.StartGame(); // Change "GameScene" to your actual gameplay scene name
    }

    public void OpenHowToPlay()
    {
        Debug.Log("How to Play button clicked."); 
        // Optional: Load another scene or show a panel
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}
