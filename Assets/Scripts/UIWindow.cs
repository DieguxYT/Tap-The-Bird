
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
    private bool initialized;

    void EnsureInitialized()
    {
        if (initialized || !Application.isPlaying) return;
        initialized = true;
        Initialize();
    }

    public virtual void Initialize()
    {
        if (_hideOnStart)
        {
            Hide(true);
        }
    }

    void Start()
    {
        EnsureInitialized();
    }

    public virtual void Show(bool instant = false)
    {
        EnsureInitialized();
        RectTransform rectTransform =
            _canvasGroup.GetComponent<RectTransform>();

        // Detener animaciones anteriores
        rectTransform.DOKill();
        _canvasGroup.DOKill();
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;

        _canvasRectTransform.gameObject.SetActive(true);

        if (instant)
        {
            rectTransform.localScale = Vector3.one;
            _canvasGroup.alpha = 1;
        }
        else
        {
            rectTransform.localScale = Vector3.one * 0.94f;
            _canvasGroup.alpha = 0;
            _canvasGroup.DOFade(1, showDuration * 0.6f).SetUpdate(true);

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
        _canvasGroup.DOKill();
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        if (instant)
        {
            _canvasRectTransform.gameObject.SetActive(false);
        }
        else
        {
            _canvasGroup.DOFade(0, hideDuration).SetUpdate(true);
            rectTransform.DOScale(Vector3.one * 0.94f, hideDuration)
                .SetEase(hideEase)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    _canvasRectTransform.gameObject.SetActive(false);
                });
        }
    }
}
