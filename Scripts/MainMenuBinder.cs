using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuBinder : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button survivalButton;
    [SerializeField] private Button battleButton;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button customizeButton;
    [SerializeField] private Button dailyRewardsButton;
    [SerializeField] private Button quitButton;

    private void Awake()
    {
        // Safety: always restore interaction state
        Time.timeScale = 1f;

        Bind(survivalButton, "SurvivalScene");
        Bind(battleButton, "BattleScene");
        Bind(shopButton, "ShopScene");
        Bind(customizeButton, "CustomizeScene");
        Bind(dailyRewardsButton, "DailyRewardScene");

        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(Application.Quit);
        }
    }

    private void Bind(Button btn, string sceneName)
    {
        if (btn == null) return;

        btn.onClick.RemoveAllListeners(); // removes broken persistent calls too
        btn.onClick.AddListener(() =>
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName);
        });
    }
}
