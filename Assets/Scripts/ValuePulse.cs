using DG.Tweening;
using TMPro;
using UnityEngine;

public class ValuePulse : MonoBehaviour
{
    private TMP_Text label;
    private string last;
    void Awake() { label = GetComponent<TMP_Text>(); last = label.text; }
    void Update()
    {
        if (label.text == last) return;
        last = label.text;
        transform.DOKill(); transform.localScale = Vector3.one;
        transform.DOPunchScale(Vector3.one * 0.1f, 0.22f, 1, 0.4f).SetUpdate(true);
    }
    void OnDisable() { transform.DOKill(); transform.localScale = Vector3.one; }
}
