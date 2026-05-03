using UnityEngine;

public static class CurrencyManager
{
    const string CoinsKey = "Coins";

    public static int Coins
    {
        get => PlayerPrefs.GetInt(CoinsKey, 0);
        private set { PlayerPrefs.SetInt(CoinsKey, value); PlayerPrefs.Save(); }
    }

    public static void AddCoins(int amount)
    {
        Coins = Mathf.Max(0, Coins + amount);
    }

    public static bool SpendCoins(int amount)
    {
        if (Coins < amount) return false;
        Coins -= amount;
        return true;
    }

    public static void ResetCoins()
    {
        Coins = 0;
    }
}
