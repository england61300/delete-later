using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI statusText;       // "You Win!" / "You Lose!"
    [SerializeField] private GameObject gameOverPanel;         // hidden at start
    [SerializeField] private TextMeshProUGUI finalScoreText;   // final results text
    [SerializeField] private Button restartButton;             // main menu button

    [Header("Rewards")]
    [SerializeField] private int coinsPerSecond = 2;           // payout scaling
    [SerializeField] private int winBonusCoins = 20;           // extra coins for winning
    [SerializeField] private int minimumCoins = 5;             // always get at least this

    private bool gameOver;
    private float timer; // score = time survived

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        gameOver = false;
        timer = 0f;

        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (statusText) statusText.text = "";

        if (restartButton)
            restartButton.onClick.AddListener(BackToMenu);
    }

    void Update()
    {
        if (!gameOver)
            timer += Time.deltaTime;
    }

    public void PlayerDied()
    {
        if (gameOver) return;
        EndGame(playerWon: false);
    }

    public void AIDied()
    {
        if (gameOver) return;
        EndGame(playerWon: true);
    }

    private void EndGame(bool playerWon)
    {
        gameOver = true;

        if (statusText)
            statusText.text = playerWon ? "You Win!" : "You Lose!";

        int score = Mathf.FloorToInt(timer);
        int coinsEarned = CalculateCoins(playerWon);

        // ✅ Reward player
        CurrencyManager.AddCoins(coinsEarned);

        // ✅ Results text shows both score and coins
        ShowEnd($"Your score: {score}\nCoins earned: +{coinsEarned}\nTotal coins: {CurrencyManager.Coins}");

        // Ads (optional)
        var ads = AdsManager.Instance;
        if (ads != null) ads.ShowInterstitial();
    }

    private int CalculateCoins(bool playerWon)
    {
        // Example payout: time survived * coinsPerSecond, with a win bonus
        int coins = Mathf.FloorToInt(timer * coinsPerSecond);

        if (playerWon)
            coins += winBonusCoins;

        // Make sure they always get something
        coins = Mathf.Max(minimumCoins, coins);

        return coins;
    }

    private void ShowEnd(string final)
    {
        if (finalScoreText) finalScoreText.text = final;
        if (gameOverPanel) gameOverPanel.SetActive(true);
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
