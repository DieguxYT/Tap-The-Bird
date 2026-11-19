using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public void OnPointerDown(PointerEventData e)
    {
        if (!GetComponent<Selectable>().IsInteractable()) return;
        transform.DOKill(); transform.DOScale(0.95f, 0.08f).SetUpdate(true);
    }
    public void OnPointerUp(PointerEventData e) { Release(); }
    public void OnPointerExit(PointerEventData e) { Release(); }
    void Release() { transform.DOKill(); transform.DOScale(1f, 0.15f).SetEase(Ease.OutBack).SetUpdate(true); }
    void OnDisable() { transform.DOKill(); transform.localScale = Vector3.one; }
}
