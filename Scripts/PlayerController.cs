using UnityEngine;
using UnityEngine.InputSystem; // new Input System

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 18f;
    private Camera cam;
    private float screenHalfWidthWorld;

    void Start()
    {
        cam = Camera.main;

        // Calculate screen half-width in world units
        float halfPlayerWidth = GetComponent<SpriteRenderer>().bounds.extents.x;
        screenHalfWidthWorld = cam.orthographicSize * cam.aspect - halfPlayerWidth;
    }

    void Update()
    {
        Vector3 targetPos = transform.position; // stay in place by default

        // --- Keyboard controls (for testing in Editor) ---
        float keyboardMove = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
                keyboardMove = -1f;
            if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
                keyboardMove = 1f;
        }
        if (keyboardMove != 0)
            targetPos = new Vector3(transform.position.x + keyboardMove, transform.position.y, 0);

        // --- Touch controls (for mobile) ---
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            Vector2 touchPos = Touchscreen.current.primaryTouch.position.ReadValue();
            Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(touchPos.x, touchPos.y, 10f));

            // only follow x-axis
            targetPos = new Vector3(worldPos.x, transform.position.y, 0);
        }

        // --- Smooth movement ---
        transform.position = Vector3.Lerp(transform.position, targetPos, moveSpeed * Time.deltaTime);

        // --- Clamp inside screen bounds ---
        float clampedX = Mathf.Clamp(transform.position.x, -screenHalfWidthWorld, screenHalfWidthWorld);
        transform.position = new Vector3(clampedX, transform.position.y, 0);
    }

    private void OnCollisionEnter2D(Collision2D c)
    {
        if (!c.collider.CompareTag("Block")) return;

        if (BattleManager.Instance != null) BattleManager.Instance.PlayerDied();
        else GameManager.Instance?.GameOver();

        Destroy(gameObject);
    }
}
