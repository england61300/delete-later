using UnityEngine;

public class EquippedSkinApplier : MonoBehaviour
{
    [SerializeField] private bool applyPlayerSkin = true; // true = Player, false = Enemy
    [SerializeField] private SkinDatabase skinDatabase;
    [SerializeField] private SpriteRenderer targetRenderer;

    private const string PLAYER_KEY = "EquippedPlayerIndex";
    private const string ENEMY_KEY = "EquippedEnemyIndex";

    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<SpriteRenderer>();

        ApplyEquippedSkinSafe();
    }

    private void ApplyEquippedSkinSafe()
    {
        if (skinDatabase == null || targetRenderer == null)
            return;

        var list = applyPlayerSkin ? skinDatabase.playerSkins : skinDatabase.enemySkins;
        if (list == null || list.Count == 0)
            return;

        int index = PlayerPrefs.GetInt(applyPlayerSkin ? PLAYER_KEY : ENEMY_KEY, 0);
        index = Mathf.Clamp(index, 0, list.Count - 1);

        // ✅ Fallback search: if chosen sprite is missing, use first valid sprite
        Sprite desired = list[index].sprite;
        if (desired == null)
        {
            desired = GetFirstValidSprite(list);
            if (desired == null)
                return; // nothing valid exists, don't overwrite
        }

        // ✅ Only apply if valid
        targetRenderer.sprite = desired;
    }

    private Sprite GetFirstValidSprite(System.Collections.Generic.List<SkinData> list)
    {
        for (int i = 0; i < list.Count; i++)
            if (list[i].sprite != null)
                return list[i].sprite;

        return null;
    }
}
