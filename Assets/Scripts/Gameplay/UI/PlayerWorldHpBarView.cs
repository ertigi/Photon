using UnityEngine;
using UnityEngine.UI;

public class PlayerWorldHpBarView : MonoBehaviour
{
    [SerializeField] private Slider _hpBar;
    [SerializeField] private Transform _barParent;
    [SerializeField] private Vector3 _offset = new Vector3(0f, 1f, .5f);

    private PlayerStatsNetwork _stats;
    private Player _player;
    private int _lastHp = int.MinValue;
    private int _lastMaxHp = int.MinValue;
    private bool _isLocalPlayer;
    private bool _ownershipResolved;

    private void Awake()
    {
        _stats = GetComponent<PlayerStatsNetwork>();
        _player = GetComponent<Player>();

        UpdateSlider();

        if (_barParent != null)
            _barParent.parent = null;
    }

    private void UpdateSlider()
    {
        if (_hpBar == null || _stats == null)
            return;

        int maxHp = Mathf.Max(1, _stats.MaxHP);
        _hpBar.value = Mathf.Clamp01(_stats.HP / (float)maxHp);

        _lastHp = _stats.HP;
        _lastMaxHp = _stats.MaxHP;
    }

    private void LateUpdate()
    {
        ResolveOwnership();

        if (_isLocalPlayer)
            return;

        if (_stats != null && (_stats.HP != _lastHp || _stats.MaxHP != _lastMaxHp))
            UpdateSlider();

        if (_barParent != null)
            _barParent.position = transform.position + _offset;
    }

    private void ResolveOwnership()
    {
        if (_ownershipResolved || _player == null || _player.Object == null)
            return;

        _ownershipResolved = true;
        _isLocalPlayer = _player.Object.HasInputAuthority;

        if (_isLocalPlayer && _barParent != null)
            _barParent.gameObject.SetActive(false);
    }
}
