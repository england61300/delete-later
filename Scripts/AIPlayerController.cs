using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AIPlayerController : MonoBehaviour
{
    [Header("Avoid What")]
    [SerializeField] LayerMask hazardLayers;      // set to your "Hazard"/"Block" layer(s)

    [Header("Sense")]
    [SerializeField] float senseRadius = 12f;     // how far we scan
    [SerializeField] float dangerVerticalWindow = 6f; // only hazards roughly above count

    [Header("Movement")]
    [SerializeField] float moveSpeed = 8f;
    [SerializeField] float edgePadding = 0.1f;    // keep a tiny gap from the edges
    [SerializeField] bool computeBoundsFromCamera = true;
    [SerializeField] float xMin = -3f, xMax = 3f; // used if computeBoundsFromCamera = false
    [SerializeField] float returnToCenterSpeed = 0.5f; // how fast we drift back when safe

    Rigidbody2D rb;
    Camera cam;
    float halfWidth;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;

        var sr = GetComponent<SpriteRenderer>();
        halfWidth = sr ? sr.bounds.extents.x : 0.5f;

        if (computeBoundsFromCamera && cam && cam.orthographic)
        {
            float halfScreen = cam.orthographicSize * cam.aspect;
            xMin = -halfScreen + halfWidth + edgePadding;
            xMax = halfScreen - halfWidth - edgePadding;
        }
    }

    void FixedUpdate()
    {
        float desiredX = transform.position.x;

        // Find closest "threat" (hazard within radius & roughly above)
        Transform threat = ClosestThreat();
        if (threat)
        {
            // Move AWAY from the threat horizontally
            float dir = (threat.position.x >= transform.position.x) ? -1f : 1f;

            // If already at an edge and the chosen dir pushes outward, cancel that push
            if ((dir < 0f && transform.position.x <= xMin + 0.01f) ||
                (dir > 0f && transform.position.x >= xMax - 0.01f))
            {
                dir = 0f;
            }

            desiredX += dir * moveSpeed * Time.fixedDeltaTime;
        }
        else
        {
            // No immediate threats → drift back toward center for natural behavior
            float center = Mathf.Clamp(0f, xMin, xMax);
            desiredX = Mathf.MoveTowards(transform.position.x, center,
                                         moveSpeed * returnToCenterSpeed * Time.fixedDeltaTime);
        }

        // Hard clamp to screen bounds
        desiredX = Mathf.Clamp(desiredX, xMin, xMax);

        if (rb) rb.MovePosition(new Vector2(desiredX, rb.position.y));
        else transform.position = new Vector3(desiredX, transform.position.y, 0f);
    }

    Transform ClosestThreat()
    {
        if (hazardLayers.value == 0) return null;

        var hits = Physics2D.OverlapCircleAll(transform.position, senseRadius, hazardLayers);
        Transform best = null;
        float bestScore = float.PositiveInfinity;

        for (int i = 0; i < hits.Length; i++)
        {
            var h = hits[i];
            if (!h) continue;

            // Only care about hazards roughly above us (or slightly below)
            float dy = h.transform.position.y - transform.position.y;
            if (dy < -0.5f || dy > dangerVerticalWindow) continue;

            // Score by horizontal closeness (primary) + a little vertical bias
            float dx = Mathf.Abs(h.transform.position.x - transform.position.x);
            float score = dx + Mathf.Max(0f, dy) * 0.25f;

            if (score < bestScore) { bestScore = score; best = h.transform; }
        }
        return best;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, senseRadius);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // AI died -> player wins (flip if you want the opposite)
        if ((hazardLayers.value & (1 << collision.gameObject.layer)) != 0)
        {
            BattleManager.Instance?.AIDied();
            Destroy(gameObject);
        }
    }
}
