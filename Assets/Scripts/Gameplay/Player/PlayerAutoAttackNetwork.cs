using Fusion;
using UnityEngine;
using Zenject;

public class PlayerAutoAttackNetwork : NetworkBehaviour
{
    [SerializeField, Min(0.02f)] private float _scanInterval = 0.1f;
    [SerializeField, Min(0.0f)] private float _fxHeight = 0.5f;
    [SerializeField, Min(0.01f)] private float _fxDuration = 0.12f;
    [SerializeField, Min(0.005f)] private float _fxWidth = 0.04f;
    [SerializeField] private Color _fxColor = new Color(1f, 0.25f, 0.25f, 1f);

    private EnemyRuntimeRegistry _enemyRuntimeRegistry;
    private EnemyDamageService _enemyDamageService;
    private PlayerStatsNetwork _stats;

    private TickTimer _scanTimer;
    private TickTimer _attackTimer;

    private float _fxUntilTime;
    private Vector3 _fxStart;
    private Vector3 _fxEnd;
    private LineRenderer _fxLineRenderer;
    private Material _fxMaterial;

    [Inject]
    public void Construct(
        EnemyRuntimeRegistry enemyRuntimeRegistry,
        EnemyDamageService enemyDamageService)
    {
        _enemyRuntimeRegistry = enemyRuntimeRegistry;
        _enemyDamageService = enemyDamageService;
    }

    public override void Spawned()
    {
        _stats = GetComponent<PlayerStatsNetwork>();
        _scanTimer = TickTimer.None;
        _attackTimer = TickTimer.None;
        EnsureFxRenderer();
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
            return;

        if (_stats == null || _stats.IsDead)
            return;

        if (!_scanTimer.ExpiredOrNotRunning(Runner))
            return;

        _scanTimer = TickTimer.CreateFromSeconds(Runner, _scanInterval);

        if (!_attackTimer.ExpiredOrNotRunning(Runner))
            return;

        float attackRadius = Mathf.Max(0f, _stats.AttackRadius);
        if (!TryGetClosestEnemy(attackRadius, out var target))
            return;

        if (target.OwnerController == null || target.Object == null || !target.Object.IsValid)
            return;

        int damage = Mathf.Max(1, _stats.Damage);
        if (!_enemyDamageService.ApplyDamage(target.OwnerController, target.Object.Id, damage, Object.Id))
            return;

        float attacksPerSecond = Mathf.Max(0.01f, _stats.AttackRate);
        _attackTimer = TickTimer.CreateFromSeconds(Runner, 1f / attacksPerSecond);

        Vector3 from = transform.position + Vector3.up * _fxHeight;
        Vector3 to = target.transform.position + Vector3.up * _fxHeight;
        RpcPlayAttackFx(from, to);
    }

    public override void Render()
    {
        if (_fxLineRenderer == null)
            return;

        if (Time.time > _fxUntilTime)
        {
            if (_fxLineRenderer.enabled)
                _fxLineRenderer.enabled = false;

            return;
        }

        _fxLineRenderer.enabled = true;
        _fxLineRenderer.startColor = _fxColor;
        _fxLineRenderer.endColor = _fxColor;
        _fxLineRenderer.SetPosition(0, _fxStart);
        _fxLineRenderer.SetPosition(1, _fxEnd);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Unreliable)]
    private void RpcPlayAttackFx(Vector3 from, Vector3 to)
    {
        _fxStart = from;
        _fxEnd = to;
        _fxUntilTime = Time.time + _fxDuration;
    }

    private bool TryGetClosestEnemy(float maxDistance, out EnemyView closestEnemy)
    {
        closestEnemy = null;

        if (maxDistance <= 0f)
            return false;

        Vector3 playerPosition = transform.position;
        float bestDistanceSqr = maxDistance * maxDistance;
        bool found = false;

        foreach (var enemy in _enemyRuntimeRegistry.GetAll())
        {
            if (enemy == null || enemy.Object == null || !enemy.Object.IsValid)
                continue;

            if (enemy.OwnerController == null)
                continue;

            float distanceSqr = (enemy.transform.position - playerPosition).sqrMagnitude;
            if (distanceSqr > bestDistanceSqr)
                continue;

            bestDistanceSqr = distanceSqr;
            closestEnemy = enemy;
            found = true;
        }

        return found;
    }

    private void EnsureFxRenderer()
    {
        if (_fxLineRenderer != null || Application.isBatchMode)
            return;

        var fxObj = new GameObject("AutoAttackFx");
        fxObj.transform.SetParent(transform, false);

        _fxLineRenderer = fxObj.AddComponent<LineRenderer>();
        _fxLineRenderer.positionCount = 2;
        _fxLineRenderer.useWorldSpace = true;
        _fxLineRenderer.startWidth = _fxWidth;
        _fxLineRenderer.endWidth = _fxWidth;
        _fxLineRenderer.enabled = false;

        Shader fxShader = Shader.Find("Sprites/Default");
        if (fxShader == null)
            fxShader = Shader.Find("Universal Render Pipeline/Unlit");

        if (fxShader != null)
        {
            _fxMaterial = new Material(fxShader);
            _fxMaterial.color = _fxColor;
            _fxLineRenderer.material = _fxMaterial;
        }
    }

    private void OnDestroy()
    {
        if (_fxMaterial != null)
            Destroy(_fxMaterial);
    }
}
