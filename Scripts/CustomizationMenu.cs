using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class CustomizationMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button playerTabButton;
    [SerializeField] private Button enemyTabButton;
    [SerializeField] private Button backButton;
    [SerializeField] private TextMeshProUGUI equippedText;

    [Header("2D Preview Sprites")]
    [SerializeField] private Image playerPreview;
    [SerializeField] private Image enemyPreview;

    [Header("Grid Catalog")]
    [SerializeField] private Transform gridParent;
    [SerializeField] private GameObject skinButtonPrefab;

    [Header("Database")]
    [SerializeField] private SkinDatabase skinDatabase;

    [Header("Text Punch Animation")]
    [SerializeField] private float punchScaleMultiplier = 1.15f;
    [SerializeField] private float punchUpDuration = 0.06f;
    [SerializeField] private float punchDownDuration = 0.10f;

    private bool showingPlayer = true;
    private int equippedPlayerIndex;
    private int equippedEnemyIndex;
    private List<SkinData> currentList;

    private Coroutine punchRoutine;
    private Vector3 equippedTextBaseScale;

    private const string PLAYER_KEY = "EquippedPlayerIndex";
    private const string ENEMY_KEY = "EquippedEnemyIndex";

    private void Start()
    {
        // Basic safety checks (prevents silent failures)
        if (skinDatabase == null)
        {
            Debug.LogError("[CustomizationMenu] SkinDatabase is not assigned.");
            enabled = false;
            return;
        }

        if (equippedText != null)
            equippedTextBaseScale = equippedText.transform.localScale;

        playerTabButton?.onClick.AddListener(ShowPlayer);
        enemyTabButton?.onClick.AddListener(ShowEnemy);
        backButton?.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));

        equippedPlayerIndex = PlayerPrefs.GetInt(PLAYER_KEY, 0);
        equippedEnemyIndex = PlayerPrefs.GetInt(ENEMY_KEY, 0);

        // Clamp saved values so they can't crash if database changed
        equippedPlayerIndex = Mathf.Clamp(equippedPlayerIndex, 0, Mathf.Max(0, skinDatabase.playerSkins.Count - 1));
        equippedEnemyIndex = Mathf.Clamp(equippedEnemyIndex, 0, Mathf.Max(0, skinDatabase.enemySkins.Count - 1));

        ShowPlayer();
    }

    private void ShowPlayer()
    {
        showingPlayer = true;

        if (playerPreview != null) playerPreview.gameObject.SetActive(true);
        if (enemyPreview != null) enemyPreview.gameObject.SetActive(false);

        RefreshGrid(skinDatabase.playerSkins);
        ApplySkin(true, equippedPlayerIndex, animate: false);
    }

    private void ShowEnemy()
    {
        showingPlayer = false;

        if (playerPreview != null) playerPreview.gameObject.SetActive(false);
        if (enemyPreview != null) enemyPreview.gameObject.SetActive(true);

        RefreshGrid(skinDatabase.enemySkins);
        ApplySkin(false, equippedEnemyIndex, animate: false);
    }

    private void RefreshGrid(List<SkinData> list)
    {
        if (gridParent == null || skinButtonPrefab == null)
            return;

        // Clear old buttons
        for (int i = gridParent.childCount - 1; i >= 0; i--)
            Destroy(gridParent.GetChild(i).gameObject);

        currentList = list;

        // Build new buttons
        for (int i = 0; i < list.Count; i++)
        {
            int index = i;
            SkinData skin = list[i];

            GameObject btnObj = Instantiate(skinButtonPrefab, gridParent);

            // Image
            Image img = btnObj.GetComponent<Image>();
            if (img != null) img.sprite = skin.sprite;

            // Name text
            TextMeshProUGUI label = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = skin.skinName;

            // Button
            Button button = btnObj.GetComponent<Button>();
            if (button != null)
            {
                button.interactable = skin.owned;
                button.onClick.AddListener(() => OnSelectSkin(index));
            }
        }
    }

    private void OnSelectSkin(int index)
    {
        if (showingPlayer)
        {
            equippedPlayerIndex = index;
            PlayerPrefs.SetInt(PLAYER_KEY, index);
            ApplySkin(true, index, animate: true);
        }
        else
        {
            equippedEnemyIndex = index;
            PlayerPrefs.SetInt(ENEMY_KEY, index);
            ApplySkin(false, index, animate: true);
        }

        PlayerPrefs.Save();
    }

    private void ApplySkin(bool player, int index, bool animate)
    {
        List<SkinData> list = player ? skinDatabase.playerSkins : skinDatabase.enemySkins;
        if (list == null || list.Count == 0) return;
        if (index < 0 || index >= list.Count) return;

        Sprite sprite = list[index].sprite;

        if (player)
        {
            if (playerPreview != null)
                playerPreview.sprite = sprite;
        }
        else
        {
            if (enemyPreview != null)
                enemyPreview.sprite = sprite;
        }

        UpdateUI(animate);
    }

    private void UpdateUI(bool animate)
    {
        if (equippedText == null)
            return;

        // Extra clamps in case database changed mid-dev
        equippedPlayerIndex = Mathf.Clamp(equippedPlayerIndex, 0, Mathf.Max(0, skinDatabase.playerSkins.Count - 1));
        equippedEnemyIndex = Mathf.Clamp(equippedEnemyIndex, 0, Mathf.Max(0, skinDatabase.enemySkins.Count - 1));

        string p = skinDatabase.playerSkins.Count > 0 ? skinDatabase.playerSkins[equippedPlayerIndex].skinName : "None";
        string e = skinDatabase.enemySkins.Count > 0 ? skinDatabase.enemySkins[equippedEnemyIndex].skinName : "None";

        equippedText.text = $"<b>Player:</b> {p}\n<b>Enemy:</b> {e}";

        if (animate)
            PunchEquippedText();
    }

    private void PunchEquippedText()
    {
        if (equippedText == null)
            return;

        // Ensure base scale is captured
        if (equippedTextBaseScale == Vector3.zero)
            equippedTextBaseScale = equippedText.transform.localScale;

        // Stop previous punch if spamming clicks
        if (punchRoutine != null)
            StopCoroutine(punchRoutine);

        punchRoutine = StartCoroutine(PunchScaleRoutine(equippedText.transform));
    }

    void AddButton()
    {
        Instantiate(skinButtonPrefab, gridParent);
    }

    private IEnumerator PunchScaleRoutine(Transform target)
    {
        Vector3 baseScale = equippedTextBaseScale;
        Vector3 peakScale = baseScale * punchScaleMultiplier;

        // Scale up (fast)
        float t = 0f;
        while (t < punchUpDuration)
        {
            t += Time.unscaledDeltaTime;
            float a = punchUpDuration <= 0f ? 1f : Mathf.Clamp01(t / punchUpDuration);
            target.localScale = Vector3.Lerp(baseScale, peakScale, a);
            yield return null;
        }

        // Scale down (slightly slower)
        t = 0f;
        while (t < punchDownDuration)
        {
            t += Time.unscaledDeltaTime;
            float a = punchDownDuration <= 0f ? 1f : Mathf.Clamp01(t / punchDownDuration);
            // Smooth step so it feels nicer
            a = a * a * (3f - 2f * a);
            target.localScale = Vector3.Lerp(peakScale, baseScale, a);
            yield return null;
        }

        target.localScale = baseScale;
        punchRoutine = null;
    }
}
