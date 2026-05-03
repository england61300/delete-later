using UnityEngine;

public class FallingBlock : MonoBehaviour
{
    void Update()
    {
        if (transform.position.y < -6f) // adjust if camera is bigger
        {
            Destroy(gameObject);
        }
    }
}
