using UnityEngine;

public class BlockSpawner : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject blockPrefab;

    [Header("Spawn area")]
    public float spawnY = 6f;           // where blocks appear
    public float xRange = 3f;           // horizontal half-width (–xRange..+xRange)
    public bool computeXRangeFromCamera = true;
    public float edgePadding = 0.25f;   // only used if computeXRangeFromCamera = true

    [Header("Difficulty: spawn interval (sec)")]
    public float startInterval = 1.0f;  // slow at start
    public float minInterval = 0.25f; // fastest
    public float intervalRampSeconds = 90f;

    [Header("Difficulty: fall speed via gravityScale")]
    public float startGravity = 2f;
    public float maxGravity = 6f;
    public float gravityRampSeconds = 120f;

    [Header("Difficulty: chance to spawn a 2nd block (0..1)")]
    public float startDoubleChance = 0f;
    public float maxDoubleChance = 0.35f;
    public float doubleChanceRampSeconds = 120f;

    float elapsed;
    float timer;

    void Start()
    {
        if (computeXRangeFromCamera)
        {
            var cam = Camera.main;
            if (cam && cam.orthographic)
            {
                float halfScreen = cam.orthographicSize * cam.aspect;
                xRange = Mathf.Max(0.1f, halfScreen - edgePadding);
            }
        }
    }

    void Update()
    {
        if (!blockPrefab) return;

        elapsed += Time.deltaTime;

        float interval = LerpOverTime(startInterval, minInterval, intervalRampSeconds, elapsed);
        timer += Time.deltaTime;

        if (timer >= interval)
        {
            timer = 0f;
            SpawnOne();

            float doubleChance = LerpOverTime(startDoubleChance, maxDoubleChance, doubleChanceRampSeconds, elapsed);
            if (Random.value < doubleChance) SpawnOne();
        }
    }

    void SpawnOne()
    {
        float x = Random.Range(-xRange, xRange);
        var go = Instantiate(blockPrefab, new Vector3(x, spawnY, 0f), Quaternion.identity);

        var rb = go.GetComponent<Rigidbody2D>();
        if (rb)
        {
            float g = LerpOverTime(startGravity, maxGravity, gravityRampSeconds, elapsed);
            rb.gravityScale = g;
        }
    }

    static float LerpOverTime(float from, float to, float seconds, float t)
    {
        float u = Mathf.Clamp01(t / Mathf.Max(0.01f, seconds));
        return Mathf.Lerp(from, to, u);
    }
}
