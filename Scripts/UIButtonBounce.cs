using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Button))]
public class UIButtonBounce : MonoBehaviour
{
    [SerializeField] private float bounceScale = 1.1f;
    [SerializeField] private float bounceDuration = 0.15f;

    private Vector3 originalScale;
    private bool isAnimating;

    void Awake()
    {
        originalScale = transform.localScale;
        GetComponent<Button>().onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        if (!isAnimating)
            StartCoroutine(Bounce());
    }

    private IEnumerator Bounce()
    {
        isAnimating = true;
        Vector3 target = originalScale * bounceScale;

        float halfTime = bounceDuration / 2f;
        float t = 0;

        // Scale up
        while (t < halfTime)
        {
            t += Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(originalScale, target, t / halfTime);
            yield return null;
        }

        // Scale back down
        t = 0;
        while (t < halfTime)
        {
            t += Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(target, originalScale, t / halfTime);
            yield return null;
        }

        transform.localScale = originalScale;
        isAnimating = false;
    }
}
