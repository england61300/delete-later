using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class DailyRewardSystem : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private DailyRewardsConfig config; // <-- assign your 7-day asset

    [Header("UI References")]
    [SerializeField] private TMP_Text rewardInfoText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button claimButton;
    [SerializeField] private Button backButton;

    // Existing: 7 boxes used for coloring/highlight
    [SerializeField] private Image[] dayBoxes = new Image[7];

    // Optional convenience: drag the root GameObject of Day1..Day7 here.
    // If you leave this empty, we’ll fallback to dayBoxes[i].transform.
    [SerializeField] private Transform[] dayItemRoots = new Transform[7];

    [Header("Colors")]
    [SerializeField] private Color activeColor = new Color(1f, 0.85f, 0.2f); // gold-ish
    [SerializeField] private Color inactiveColor = Color.gray;

    // PlayerPrefs keys
    private const string LAST_DATE_KEY = "DR_LastClaimDateUTC";
    private const string STREAK_KEY = "DR_Streak";
    private const string NEXT_INDEX_KEY = "DR_NextIndex";

    private int streak;
    private int nextIndex;   // 0..6 (0=Day1)
    private bool canClaim;

    private void Start()
    {
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(() =>
            {
                // Replace with SceneTransition if you’re using it:
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            });
        }

        if (claimButton != null)
        {
            claimButton.onClick.RemoveAllListeners();
            claimButton.onClick.AddListener(ClaimReward);
        }

        LoadState();
        PopulateFromConfig();   // <-- set icons/amount labels from config
        UpdateCalendarUI();
        UpdateHeaderAndButtons();
    }

    // ----------------------
    // CONFIG → UI POPULATION
    // ----------------------
    private void PopulateFromConfig()
    {
        if (config == null || config.rewards == null || config.rewards.Count < 7)
        {
            Debug.LogWarning("DailyRewardSystem: Config missing or < 7 entries. UI will still function but won’t show icons/amounts.");
            // Still write a generic header so the scene isn’t blank
            if (rewardInfoText) rewardInfoText.text = $"Day {nextIndex + 1} – Reward: coins";
            return;
        }

        for (int i = 0; i < 7; i++)
        {
            Transform root = GetDayRoot(i);
            if (root == null) continue;

            // Try to find children by these names in your RewardItemPrefab:
            var icon = root.Find("Icon")?.GetComponent<Image>();
            var amountLabel = root.Find("AmountLabel")?.GetComponent<TMP_Text>();
            // If you used a different name for the amount, this fallback will try "Name"
            if (amountLabel == null)
                amountLabel = root.Find("Name")?.GetComponent<TMP_Text>();

            var entry = config.rewards[Mathf.Clamp(i, 0, config.rewards.Count - 1)];

            if (icon != null) icon.sprite = entry.icon;
            if (amountLabel != null) amountLabel.text = $"+{entry.amount}";
        }

        // Header text for the “today” reward
        if (rewardInfoText != null)
        {
            var today = config.rewards[Mathf.Clamp(nextIndex, 0, config.rewards.Count - 1)];
            rewardInfoText.text = $"Day {nextIndex + 1} – Reward: +{today.amount}";
        }
    }

    private Transform GetDayRoot(int i)
    {
        if (dayItemRoots != null && i < dayItemRoots.Length && dayItemRoots[i] != null)
            return dayItemRoots[i];

        if (dayBoxes != null && i < dayBoxes.Length && dayBoxes[i] != null)
            return dayBoxes[i].transform;

        return null;
    }

    // ----------------------
    // STATE
    // ----------------------
    private void LoadState()
    {
        nextIndex = PlayerPrefs.GetInt(NEXT_INDEX_KEY, 0);
        streak = PlayerPrefs.GetInt(STREAK_KEY, 0);

        string last = PlayerPrefs.GetString(LAST_DATE_KEY, "");
        var todayUtc = DateTime.UtcNow.Date;

        if (string.IsNullOrEmpty(last))
        {
            canClaim = true; // first time ever
            return;
        }

        if (DateTime.TryParse(last, out var lastDate))
        {
            var lastDay = lastDate.Date;

            if (todayUtc > lastDay)
            {
                canClaim = true;
                // If missed days, reset streak; if consecutive day, keep
                if (todayUtc != lastDay.AddDays(1))
                    streak = 0;
            }
            else
            {
                // same UTC day → already claimed
                canClaim = false;
            }
        }
        else
        {
            canClaim = true;
            streak = 0;
            nextIndex = 0;
        }
    }

    private void SaveStateAfterClaim()
    {
        PlayerPrefs.SetString(LAST_DATE_KEY, DateTime.UtcNow.Date.ToString("yyyy-MM-dd"));
        PlayerPrefs.SetInt(STREAK_KEY, streak);
        PlayerPrefs.SetInt(NEXT_INDEX_KEY, nextIndex);
        PlayerPrefs.Save();
    }

    // ----------------------
    // UI
    // ----------------------
    private void UpdateCalendarUI()
    {
        if (dayBoxes == null || dayBoxes.Length < 7) return;

        for (int i = 0; i < dayBoxes.Length; i++)
        {
            var img = dayBoxes[i];
            if (img == null) continue;

            // Get the root to find child visuals
            Transform root = GetDayRoot(i);
            var glow = root?.Find("State_TodayGlow")?.gameObject;
            var claimed = root?.Find("State_Claimed")?.gameObject;
            var locked = root?.Find("State_Locked")?.gameObject;

            bool isToday = (i == nextIndex);
            bool isFuture = (i > nextIndex);
            bool isPast = (i < nextIndex);

            // Base coloring
            if (isToday) img.color = activeColor;
            else if (isFuture) img.color = new Color(inactiveColor.r, inactiveColor.g, inactiveColor.b, 0.6f);
            else img.color = inactiveColor;

            // Visual states
            if (glow) glow.SetActive(isToday && canClaim);       // glow only on claimable tile
            if (locked) locked.SetActive(isFuture);                 // future tiles look locked

            // Claimed badge on past days (optional)
            if (claimed) claimed.SetActive(isPast);
        }

        // Optional: you can also toggle child visuals like “State_TodayGlow”/“State_Locked”/“State_Claimed”
        // by finding them under GetDayRoot(i) and SetActive based on i vs nextIndex.
    }

    private void UpdateHeaderAndButtons()
    {
        // Top info
        if (config != null && config.rewards != null && config.rewards.Count >= 7 && rewardInfoText != null)
        {
            var today = config.rewards[Mathf.Clamp(nextIndex, 0, config.rewards.Count - 1)];
            rewardInfoText.text = canClaim
                ? $"Day {nextIndex + 1} – Reward: +{today.amount}"
                : $"Come back tomorrow";
        }

        if (messageText != null)
        {
            messageText.text = canClaim
                ? $"Streak: {streak}"
                : $"Streak: {streak}  •  Next reset at 00:00 UTC";
        }

        if (claimButton != null)
        {
            claimButton.interactable = canClaim;
            var label = claimButton.GetComponentInChildren<TMP_Text>();
            if (label) label.text = canClaim ? "Claim" : "Claimed";
        }
    }

    // ----------------------
    // CLAIM
    // ----------------------
    private void ClaimReward()
    {
        if (!canClaim) return;

        int amount = 0;
        if (config != null && config.rewards != null && config.rewards.Count >= 7)
        {
            var entry = config.rewards[Mathf.Clamp(nextIndex, 0, config.rewards.Count - 1)];
            amount = Mathf.Max(0, entry.amount);
        }
        else
        {
            // fallback if config missing
            amount = 100;
        }

        // Grant coins
        if (amount > 0)
        {
                CurrencyManager.AddCoins(amount);

        }

        // Streak + day advance
        streak = Mathf.Max(0, streak) + 1;
        nextIndex = (nextIndex + 1) % 7;

        canClaim = false;
        SaveStateAfterClaim();

        // Refresh visuals
        PopulateFromConfig();  // header for next day amount
        UpdateCalendarUI();
        UpdateHeaderAndButtons();
    }
}
