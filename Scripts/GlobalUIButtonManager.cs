using UnityEngine;
using UnityEngine.UI;

public class GlobalUIButtonManager : MonoBehaviour
{
    void Awake()
    {
        var allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (var button in allButtons)
        {
            if (button.GetComponent<UIButtonBounce>() == null)
                button.gameObject.AddComponent<UIButtonBounce>();
        }
    }
}
