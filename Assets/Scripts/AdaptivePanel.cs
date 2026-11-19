using UnityEngine;

[ExecuteAlways]
public class AdaptivePanel : MonoBehaviour
{
    public Vector2 preferredSize = new Vector2(740, 980);
    void LateUpdate() { Refresh(); }
    public void Refresh()
    {
        var parent = transform.parent as RectTransform;
        if (parent == null) return;
        var rect = (RectTransform)transform;
        rect.sizeDelta = new Vector2(Mathf.Min(preferredSize.x, Mathf.Max(1, parent.rect.width - 48)),
            Mathf.Min(preferredSize.y, Mathf.Max(1, parent.rect.height - 48)));
    }
}
