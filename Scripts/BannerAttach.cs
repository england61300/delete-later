using GoogleMobileAds.Api;
using UnityEngine;

public class BannerAttach : MonoBehaviour
{
    void Start()
    {
        var rect = GetComponent<RectTransform>();
        float x = rect.position.x;
        float y = rect.position.y;
        AdsManager.Instance.ShowBanner(x, y);
    }
}
