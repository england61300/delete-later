using GoogleMobileAds.Api;
using UnityEngine;

public class AdsManager : MonoBehaviour
{
    private static AdsManager _instance;
    public static AdsManager Instance => _instance;

    private BannerView bannerView;
    private InterstitialAd interstitial;

    private string bannerId;
    private string interstitialId;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        // If not already in the scene, create a persistent AdsManager now
        if (FindFirstObjectByType<AdsManager>() == null)
        {
            var go = new GameObject("AdsManager");
            go.AddComponent<AdsManager>();
            DontDestroyOnLoad(go);
        }
    }

    void Awake()
    {
#if UNITY_ANDROID
        bannerId = "ca-app-pub-2593431830550121/4920786939";
        interstitialId = "ca-app-pub-2593431830550121/6151211775";
#elif UNITY_IOS
        bannerId = "ca-app-pub-3940256099942544/2934735716";
        interstitialId = "ca-app-pub-3940256099942544/4411468910";
#else
        bannerId = "unused";
        interstitialId = "unused";
#endif

        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        MobileAds.Initialize(_ => { });
        RequestBanner();
        RequestInterstitial();
    }

    private void RequestBanner()
    {
        bannerView = new BannerView(bannerId, AdSize.Banner, AdPosition.Bottom);
        var request = new AdRequest();
        bannerView.LoadAd(request);
    }

    private void RequestInterstitial()
    {
        var request = new AdRequest();
        InterstitialAd.Load(interstitialId, request, (InterstitialAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("Interstitial failed to load: " + error);
                return;
            }
            interstitial = ad;
        });
    }

    public void ShowInterstitial()
    {
        if (interstitial != null && interstitial.CanShowAd())
        {
            interstitial.Show();
            RequestInterstitial(); // preload next
        }
        else
        {
            Debug.Log("Interstitial not ready yet!");
        }
    }

    public void ShowBanner(float x, float y)
    {
        if (bannerView == null)
        {
            RequestBanner();
        }
    }
}
