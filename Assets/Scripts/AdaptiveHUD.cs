using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class AdaptiveHUD : MonoBehaviour
{
    void OnEnable() { Refresh(); }
    void LateUpdate() { Refresh(); }
    public void Refresh()
    {
        var rect = (RectTransform)transform;
        if (!(rect.parent is RectTransform parent)) return;
        rect.anchorMin = rect.anchorMax = new Vector2(0,1);
        rect.pivot = new Vector2(0,1);
        rect.anchoredPosition = new Vector2(24,-24);
        rect.sizeDelta = new Vector2(Mathf.Clamp(parent.rect.width-168,1,1120),108);
    }
}
