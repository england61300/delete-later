using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DailyRewardsConfig", menuName = "Game/Daily Rewards Config")]
public class DailyRewardsConfig : ScriptableObject
{
    [System.Serializable]
    public class RewardEntry
    {
        public string name = "Coins";
        public Sprite icon;
        public int amount = 100; // coins by default
    }

    public List<RewardEntry> rewards; // e.g., 7 entries
}
