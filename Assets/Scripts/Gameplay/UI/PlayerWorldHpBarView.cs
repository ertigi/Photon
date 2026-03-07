using UnityEngine;
using UnityEngine.UI;

public class PlayerWorldHpBarView : MonoBehaviour, PlayerStatsNetwork.IRenderChangeListener
{
    [SerializeField] private Slider _hpBar;
    [SerializeField] private Transform _barParent;
    [SerializeField] private Vector3 _offset = new Vector3(0f, 1f, .5f);

    private PlayerStatsNetwork _stats;
    private Player _player;
    private bool _isLocalPlayer;
    private bool _ownershipResolved;
    private bool _isBound;

    private void Awake()
    {
        _stats = GetComponent<PlayerStatsNetwork>();
        _player = GetComponent<Player>();

        if (_barParent != null)
            _barParent.parent = null;
    }

    private void OnEnable()
    {
        TryBind();
    }

    private void OnDisable()
    {
        Unbind();
    }

    private void OnDestroy()
    {
        Unbind();
    }

    private void LateUpdate()
    {
        if (_isBound && (_stats == null || _stats.Object == null || !_stats.Object.IsValid))
            Unbind();

        ResolveOwnership();

        if (_barParent != null)
            _barParent.position = transform.position + _offset;
    }

    private void ResolveOwnership()
    {
        if (_ownershipResolved || _player == null || _player.Object == null || !_player.Object.IsValid)
            return;

        _ownershipResolved = true;
        _isLocalPlayer = _player.Object.HasInputAuthority;

        if (_isLocalPlayer && _barParent != null)
            _barParent.gameObject.SetActive(false);
    }

    public void HandleHealthChanged(int hp, int maxHp, bool isDead)
    {
        ResolveOwnership();

        if (_hpBar == null || _isLocalPlayer)
            return;

        int safeMaxHp = Mathf.Max(1, maxHp);
        _hpBar.value = Mathf.Clamp01(hp / (float)safeMaxHp);
    }

    public void HandleProgressChanged(int xp, int level)
    {
    }

    public void HandleDeathChanged(bool isDead)
    {
    }

    private void TryBind()
    {
        if (_isBound)
            return;

        if (_stats == null)
            _stats = GetComponent<PlayerStatsNetwork>();

        if (_stats == null)
            return;

        _stats.RegisterRenderListener(this, pushInitialState: true);
        _isBound = true;
    }

    private void Unbind()
    {
        if (!_isBound)
            return;

        if (_stats != null)
            _stats.UnregisterRenderListener(this);

        _isBound = false;
    }
}
