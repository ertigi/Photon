using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class PlayerWorldHpBarView : MonoBehaviour
{
    [SerializeField] private Vector3 _offset = new Vector3(0f, 1.85f, 0f);
    [SerializeField] private Vector2 _barSize = new Vector2(90f, 12f);
    [SerializeField] private float _worldScale = 0.01f;
    [SerializeField] private Color _backgroundColor = new Color(0f, 0f, 0f, 0.6f);
    [SerializeField] private Color _fillColor = new Color(0.95f, 0.15f, 0.15f, 1f);

    private LocalPlayerRegistry _localPlayerRegistry;
    private PlayerStatsNetwork _stats;

    private Transform _root;
    private Image _fillImage;
    private Camera _mainCamera;

    [Inject]
    public void Construct(LocalPlayerRegistry localPlayerRegistry)
    {
        _localPlayerRegistry = localPlayerRegistry;
    }

    private void Awake()
    {
        _stats = GetComponent<PlayerStatsNetwork>();
        BuildWorldBar();
    }

    private void LateUpdate()
    {
        if (_stats == null || _root == null || _fillImage == null)
            return;

        bool isLocalPlayer = IsLocalPlayer();
        bool shouldShow = !isLocalPlayer;

        if (_root.gameObject.activeSelf != shouldShow)
            _root.gameObject.SetActive(shouldShow);

        if (!shouldShow)
            return;

        _root.position = transform.position + _offset;

        if (_mainCamera == null)
            _mainCamera = Camera.main;

        if (_mainCamera != null)
            _root.forward = _mainCamera.transform.forward;

        float hpNormalized = _stats.MaxHP > 0 ? (float)_stats.HP / _stats.MaxHP : 0f;
        _fillImage.fillAmount = Mathf.Clamp01(hpNormalized);
    }

    private bool IsLocalPlayer()
    {
        if (_stats != null && _stats.Object != null && _stats.Object.IsValid && _stats.Object.HasInputAuthority)
            return true;

        if (_localPlayerRegistry == null)
            return false;

        var localTarget = _localPlayerRegistry.Local.Value;
        var player = localTarget as Player;
        if (player == null)
            return false;

        var localStats = player.GetComponent<PlayerStatsNetwork>();
        return localStats == _stats;
    }

    private void BuildWorldBar()
    {
        var rootGo = new GameObject("WorldHpBar");
        _root = rootGo.transform;
        _root.SetParent(transform, false);
        _root.localPosition = _offset;
        _root.localRotation = Quaternion.identity;
        _root.localScale = Vector3.one * _worldScale;

        var canvas = rootGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;

        var canvasRect = canvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = _barSize;

        var background = CreateImage("Background", _root, _backgroundColor);
        var fill = CreateImage("Fill", background.rectTransform, _fillColor);

        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = (int)Image.OriginHorizontal.Left;
        fill.fillAmount = 1f;

        _fillImage = fill;
    }

    private static Image CreateImage(string name, Transform parent, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        go.AddComponent<CanvasRenderer>();
        var image = go.AddComponent<Image>();
        image.color = color;

        return image;
    }
}
