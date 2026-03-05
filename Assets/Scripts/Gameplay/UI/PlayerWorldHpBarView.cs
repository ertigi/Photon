using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class PlayerWorldHpBarView : MonoBehaviour
{
    [SerializeField] private Slider _hpBar;
    [SerializeField] private Transform _barParent;
    [SerializeField] private Vector3 _offset = new Vector3(0f, 1f, .5f);
    private PlayerStatsNetwork _stats;

    private void Start()
    {
        _stats = GetComponent<PlayerStatsNetwork>();
        _stats.OnChangeHP += UpdateSlider;
        UpdateSlider();
        _barParent.parent = null;
    }

    private void OnDestroy()
    {
        _stats.OnChangeHP -= UpdateSlider;
    }

    private void UpdateSlider()
    {
        _hpBar.value = _stats.HP / (float)_stats.MaxHP;
    }

    private void LateUpdate()
    {
        _barParent.position = transform.position + _offset;
    }
}
