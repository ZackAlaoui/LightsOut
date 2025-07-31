using UnityEngine;
using UnityEngine.UI;
using Game;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public Button startButton;
    public CanvasGroup canvasGroup;

    public GameObject mainMenuPanel;
    public GameObject howToPlayPanel;
    public TMP_Text howToPlayText; // 👈 Add this


    public void StartGame()
    {
        OnButtonClick();
        Debug.Log("Start Game clicked.");
    }

    public void OnButtonClick()
    {
        Debug.Log("Game starting...");
        GameManager.StartGame();
    }

    public void OpenHowToPlay()
    {
        Debug.Log("How to Play button clicked.");

        mainMenuPanel.SetActive(false);
        howToPlayPanel.SetActive(true);

        howToPlayText.text = @"<b>🎯 Objective</b>
Survive waves of enemies in a haunted amusement park.
Use your flashlight and ability cards to survive.

<b>🎮 Controls</b>
WASD ............... Move  
Mouse .............. Aim  
Left Click ......... Shoot  
Shift .............. Sprint (Drains flashlight battery)  
1–5 ................ Use ability cards  
Esc ................ Pause menu

<b>🔦 Flashlight</b>
Your flashlight is your life source.  
If the battery runs out, you take damage.  
Recharge with batteries or abilities.

<b>🃏 Cards & Abilities</b>
- Cards grant active and passive powers.  
- Use keys 1–5 to activate.  
- Passives auto-trigger based on conditions.  
- Cooldowns apply after use.

<b>♠️ Poker Buffs</b>
Matching suits in hand give bonuses:
• One Pair → +50% Move Speed  
• Two Pair → +50% Max Health  
• Three of a Kind → +50% Damage  
• Full House → +100% Max Health  
• Four of a Kind → +100% Damage

<b>🃘 Dealer's Den</b>
Every 3 rounds, choose 1 of 5 new cards.  
Unchosen cards will be used against you by the Dealer!

<b>✅ Tips</b>
- Watch your battery!  
- Use sprint carefully.  
- Save strong cards for boss fights.";
    }


    public void BackToMainMenu()
    {
        howToPlayPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}