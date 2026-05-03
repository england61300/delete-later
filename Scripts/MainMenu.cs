using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private void Start()
    {
        Time.timeScale = 1f;              // ✅ always unpause
        AudioListener.pause = false;      // optional safety
    }
    public void PlaySurvival()
    {
        SceneManager.LoadScene("SurvivalScene");
    }

    public void PlayBattle()
    {
        SceneManager.LoadScene("BattleScene");
    }

    public void OpenShop()
    {
        SceneManager.LoadScene("ShopScene");
    }

    public void OpenCustomize()
    {
        SceneManager.LoadScene("CustomizeScene");
    }

    public void OpenDailyRewards()
    {
        SceneManager.LoadScene("DailyRewardScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }


}
