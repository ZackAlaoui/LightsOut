using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenu : MonoBehaviour
{
    public GameObject gameOverUI;

    private void Start()
    {
        gameOverUI.SetActive(false); // Hide at start
    }

    public void ShowGameOver()
    {
        Time.timeScale = 0f;
        gameOverUI.SetActive(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu"); // Make sure you have a scene named this
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}