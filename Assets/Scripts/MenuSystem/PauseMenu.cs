using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardData;
using TMPro;
using Game.Player;

public class PauseMenu : MonoBehaviour
{
    public static bool IsPausingEnabled = false;
    public static bool GameIsPaused = false;

    public GameObject pauseMenuUI;
    public GameObject cardInfoPanel;
    public TMP_Text cardInfoText;
    public GameObject resumeButton;
    public GameObject currentCardsButton;
    public GameObject quitButton;

    private void Update()
    {
        if (IsPausingEnabled && Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        PlayerController.Instance.IsGunEnabled = true;
        pauseMenuUI.SetActive(false);
        cardInfoPanel.SetActive(false); // Hide extra info
        Time.timeScale = 1f;
        GameIsPaused = false;
    }

    void Pause()
    {
        PlayerController.Instance.IsGunEnabled = false;
        pauseMenuUI.SetActive(true);
        cardInfoPanel.SetActive(false); // Hide card info if it was left open
        resumeButton.SetActive(true);
        currentCardsButton.SetActive(true);
        quitButton.SetActive(true);

        Time.timeScale = 0f;
        GameIsPaused = true;
    }

    public void CurrentCardInformation()
    {
        Debug.Log("Current Card Information loading...");

        List<CardInformation> cards = HandManager.GetCurrentHand();

        if (cards == null || cards.Count == 0)
        {
            cardInfoText.text = "You are not holding any cards.";
        }
        else
        {
            cardInfoText.text = "";
            foreach (CardInformation card in cards)
            {
                cardInfoText.text += $"<b>{card.cardName}</b>\n{card.abilityDesc}\n\n";
            }
        }
        resumeButton.SetActive(false);
        currentCardsButton.SetActive(false);
        quitButton.SetActive(false);
        cardInfoPanel.SetActive(true);
        
    }


    public void Quit()
    {
        Debug.Log("Quitting Game");
        Application.Quit();
    }
    
    public void BackToPauseMenu()
    {
        cardInfoPanel.SetActive(false);
        resumeButton.SetActive(true);
        currentCardsButton.SetActive(true);
        quitButton.SetActive(true);
    }

}