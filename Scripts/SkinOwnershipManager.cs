using UnityEngine;
using System.Collections.Generic;

public class SkinOwnershipManager : MonoBehaviour
{
    public static SkinOwnershipManager Instance { get; private set; }

    [SerializeField] private SkinDatabase skinDatabase;

    private const string OWNED_PLAYER = "OwnedPlayerSkins";
    private const string OWNED_ENEMY = "OwnedEnemySkins";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (skinDatabase == null)
        {
            Debug.LogError("[SkinOwnershipManager] SkinDatabase is not assigned.");
            return;
        }

        LoadOwnedFlags(skinDatabase.playerSkins, OWNED_PLAYER);
        LoadOwnedFlags(skinDatabase.enemySkins, OWNED_ENEMY);
    }

    private void LoadOwnedFlags(List<SkinData> list, string key)
    {
        if (list == null || list.Count == 0) return;

        // Safety: starter skin always owned
        list[0].owned = true;

        string raw = PlayerPrefs.GetString(key, "");
        if (string.IsNullOrEmpty(raw)) return;

        var ownedNames = new HashSet<string>(raw.Split('|'));

        foreach (var skin in list)
            skin.owned = skin.owned || ownedNames.Contains(skin.skinName);
    }

    public bool IsOwned(bool playerSkin, string skinName)
    {
        if (skinDatabase == null) return false;

        var list = playerSkin ? skinDatabase.playerSkins : skinDatabase.enemySkins;
        foreach (var skin in list)
            if (skin.skinName == skinName)
                return skin.owned;

        return false;
    }

    public bool UnlockSkin(bool playerSkin, string skinName)
    {
        if (skinDatabase == null) return false;

        var list = playerSkin ? skinDatabase.playerSkins : skinDatabase.enemySkins;

        foreach (var skin in list)
        {
            if (skin.skinName == skinName)
            {
                if (skin.owned) return false; // already owned

                skin.owned = true;
                SaveOwnedFlags(list, playerSkin ? OWNED_PLAYER : OWNED_ENEMY);
                return true;
            }
        }

        Debug.LogWarning($"[SkinOwnershipManager] Skin not found: {skinName}");
        return false;
    }

    private void SaveOwnedFlags(List<SkinData> list, string key)
    {
        List<string> owned = new List<string>();

        foreach (var skin in list)
            if (skin.owned) owned.Add(skin.skinName);

        PlayerPrefs.SetString(key, string.Join("|", owned));
        PlayerPrefs.Save();
    }
}
