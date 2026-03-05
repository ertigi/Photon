using UnityEngine;
using UnityEngine.UI;

public class GameplayHudView : MonoBehaviour
{
    [SerializeField] private Image _hpBarFill;
    [SerializeField] private Image _xpBarFill;
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void SetVisible(bool isVisible)
    {
        if (_canvasGroup == null)
            return;

        _canvasGroup.alpha = isVisible ? 1f : 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }

    public void SetHpNormalized(float normalizedValue)
    {
        if (_hpBarFill == null)
            return;

        _hpBarFill.fillAmount = Mathf.Clamp01(normalizedValue);
    }

    public void SetXpNormalized(float normalizedValue)
    {
        if (_xpBarFill == null)
            return;

        _xpBarFill.fillAmount = Mathf.Clamp01(normalizedValue);
    }
}
