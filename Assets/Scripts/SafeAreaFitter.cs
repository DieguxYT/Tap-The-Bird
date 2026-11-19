using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class SafeAreaFitter : MonoBehaviour
{
    private Rect previous;
    private Vector2 screen;
    void OnEnable() { Refresh(); }
    void Update()
    {
        if (previous != Screen.safeArea || screen != new Vector2(Screen.width, Screen.height)) Refresh();
    }
    public void Refresh()
    {
        if (Screen.width < 1 || Screen.height < 1) return;
        previous = Screen.safeArea; screen = new Vector2(Screen.width, Screen.height);
        var rect = (RectTransform)transform;
        rect.anchorMin = previous.min / screen;
        rect.anchorMax = previous.max / screen;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }
}
