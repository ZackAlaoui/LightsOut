using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenu : MonoBehaviour
{
    [Header("Assign the GameOver panel here")]
    public GameObject gameOverScreen;

    private void Start()
    {
        // Hide game over screen at the beginning
        if (gameOverScreen != null)
            gameOverScreen.SetActive(false);
    }

    /// <summary>
    /// Call this when the player dies to show the Game Over menu
    /// </summary>
    public void ShowGameOver()
    {
        Time.timeScale = 0f; // Pause the game
        gameOverScreen.SetActive(true);
    }

    /// <summary>
    /// Called by Restart button
    /// </summary>
    public void RestartGame()
    {
        Time.timeScale = 1f; // Unpause
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Called by Main Menu button
    /// </summary>
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f; // Unpause
        SceneManager.LoadScene("MainMenu"); // Replace with your main menu scene name
    }

    /// <summary>
    /// Called by Quit button
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}