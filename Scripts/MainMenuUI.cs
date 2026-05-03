using UnityEngine;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI coinText;

    void Start()
    {
        UpdateCoinText();
    }

    void Update()
    {
        // Optional: keep it live-updated if coins change dynamically
        UpdateCoinText();
    }

    void UpdateCoinText()
    {
        if (coinText != null)
        {
            coinText.text = "Coins: " + CurrencyManager.Coins;
        }
    }
}
