using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class GachaShop : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SkinDatabase skinDatabase;

    [Header("UI")]
    [SerializeField] private Button rollPlayerButton;
    [SerializeField] private Button rollEnemyButton;
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUI resultText;

    [Header("Prices")]
    [SerializeField] private int playerRollCost = 25;
    [SerializeField] private int enemyRollCost = 25;

    [Header("Duplicate Handling")]
    [SerializeField] private int duplicateRefundCoins = 10;

    private void Start()
    {
        rollPlayerButton?.onClick.AddListener(() => Roll(true));
        rollEnemyButton?.onClick.AddListener(() => Roll(false));

        RefreshUI();
    }

    private void RefreshUI()
    {
        if (coinText != null)
            coinText.text = CurrencyManager.Coins.ToString();
    }

    private void Roll(bool playerSkin)
    {
        if (skinDatabase == null)
        {
            if (resultText) resultText.text = "SkinDatabase not set.";
            return;
        }

        if (SkinOwnershipManager.Instance == null)
        {
            if (resultText) resultText.text = "OwnershipManager missing in first scene.";
            return;
        }

        int cost = playerSkin ? playerRollCost : enemyRollCost;

        if (CurrencyManager.Coins < cost)
        {
            if (resultText) resultText.text = "Not enough coins.";
            return;
        }

        // Spend coins
        CurrencyManager.SpendCoins(cost);

        // Get list of skins you *can* unlock
        List<SkinData> list = playerSkin ? skinDatabase.playerSkins : skinDatabase.enemySkins;
        List<SkinData> available = new List<SkinData>();

        for (int i = 0; i < list.Count; i++)
        {
            // don't include starter skin in gacha (optional)
            if (i == 0) continue;

            if (!list[i].owned)
                available.Add(list[i]);
        }

        if (available.Count == 0)
        {
            // Nothing left to unlock
            CurrencyManager.AddCoins(duplicateRefundCoins); // optional consolation
            if (resultText) resultText.text = "All skins unlocked! (refund given)";
            RefreshUI();
            return;
        }

        // Pick random skin from available
        SkinData rolled = available[Random.Range(0, available.Count)];

        // Unlock it
        bool unlocked = SkinOwnershipManager.Instance.UnlockSkin(playerSkin, rolled.skinName);

        if (unlocked)
        {
            if (resultText) resultText.text = $"Unlocked: {rolled.skinName}";
        }
        else
        {
            // Should be rare if available list is correct, but safe fallback
            CurrencyManager.AddCoins(duplicateRefundCoins);
            if (resultText) resultText.text = $"Duplicate! Refund +{duplicateRefundCoins}";
        }

        RefreshUI();
    }
}
