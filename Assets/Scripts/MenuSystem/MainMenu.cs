using UnityEngine;
using UnityEngine.UI;
using Game;

public class MainMenu : MonoBehaviour
{
    public Button startButton;
    public CanvasGroup canvasGroup;
    public void StartGame()
    {
        OnButtonClick();
        Debug.Log("Button Clicked and we are in the StartGame function.");
    }

    public void OnButtonClick()
    {
        Debug.Log("Button Clicked and we are in the OnButtonClick function.");
        GameManager.StartGame();
    }


    public void OpenHowToPlay()
    {
        Debug.Log("How to Play button clicked.");
        // Optional: Load another scene or show a panel
        //Maybe provide a tutorial on how to play the game.
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}
