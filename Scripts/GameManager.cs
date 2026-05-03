using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] TextMeshProUGUI scoreText;       // live score (top-left)
    [SerializeField] GameObject gameOverPanel;        // hidden at start
    [SerializeField] TextMeshProUGUI finalScoreText;  // inside gameOverPanel
    [SerializeField] Button restartButton;            // inside gameOverPanel

    float score;
    bool isAlive = true;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (restartButton) restartButton.onClick.AddListener(BackToMenu);
        score = 0;
        isAlive = true;
        UpdateScoreUI();
    }

    void Update()
    {
        if (!isAlive) return;

        // Increase score by time survived
        score += Time.deltaTime;
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        if (scoreText)
            scoreText.text = "Score: " + Mathf.FloorToInt(score);
    }

    public void GameOver(bool playerWon = false)
    {
        if (!isAlive) return;
        isAlive = false;

        // Reward coins
        GiveCoins(playerWon);

        // Show Game Over panel
        if (gameOverPanel) gameOverPanel.SetActive(true);
        if (finalScoreText)
            finalScoreText.text = $"Your score: {Mathf.FloorToInt(score)}";
    }

    void GiveCoins(bool playerWon)
    {
        int coinsEarned = Mathf.FloorToInt(score / 10f);
        if (playerWon)
            coinsEarned += 10;

        CurrencyManager.AddCoins(coinsEarned);
        Debug.Log($"Coins earned: {coinsEarned}, Total coins: {CurrencyManager.Coins}");
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
