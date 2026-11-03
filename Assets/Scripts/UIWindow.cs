
using DG.Tweening;
using UnityEngine;

public class UIWindow : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private string _id;

    [Header("UI Settings")]
    [SerializeField] private RectTransform _canvasRectTransform;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private bool _hideOnStart;

    [Header("Animation Settings")]
    [SerializeField] private float showDuration = 0.5f;
    [SerializeField] private float hideDuration = 0.5f;

    [SerializeField] private Ease showEase = Ease.OutBack;
    [SerializeField] private Ease hideEase = Ease.InBack;

    public CanvasGroup CanvasGroup => _canvasGroup;
    public RectTransform CanvasRectTransform => _canvasRectTransform;
    public string Id => _id;

    public virtual void Initialize()
    {
        if (_hideOnStart)
        {
            Hide(true);
        }
    }

    void Start()
    {
        Initialize();
    }

    public virtual void Show(bool instant = false)
    {
        RectTransform rectTransform =
            _canvasGroup.GetComponent<RectTransform>();

        // Detener animaciones anteriores
        rectTransform.DOKill();

        _canvasRectTransform.gameObject.SetActive(true);

        if (instant)
        {
            rectTransform.localScale = Vector3.one;
        }
        else
        {
            rectTransform.localScale = Vector3.zero;

            rectTransform.DOScale(Vector3.one, showDuration)
                .SetEase(showEase)
                .SetUpdate(true);
        }
    }

    public virtual void Hide(bool instant = false)
    {
        RectTransform rectTransform =
            _canvasGroup.GetComponent<RectTransform>();

        // Detener animaciones anteriores
        rectTransform.DOKill();

        if (instant)
        {
            _canvasRectTransform.gameObject.SetActive(false);
        }
        else
        {
            rectTransform.DOScale(Vector3.zero, hideDuration)
                .SetEase(hideEase)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    _canvasRectTransform.gameObject.SetActive(false);
                });
        }
    }
}
