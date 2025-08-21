using UnityEngine;

public class PlayerController : BaseController
{
    private Camera playerCamera;

    private GameManager gameManager;

    [Header("Auto Attack")]
    [SerializeField] private bool useAutoAttack = true;
    [SerializeField] private float autoAttackRange = 8f;
    [SerializeField] private LayerMask enemyLayer;
    [Tooltip("시야를 가리는 레벨/벽 레이어. 비우면 시야 검사 생략")]
    [SerializeField] private LayerMask lineOfSightMask;
    [SerializeField] private float scanInterval = 0.15f;
    [SerializeField] private float keepTargetDuration = 0.5f;

    [Header("Movement & Attack Behavior")]
    [Tooltip("공격 사거리 안이어도 이동을 멈추지 않음")]
    [SerializeField] private bool allowMoveWhileAttacking = true;

    private float scanTimer;
    private Transform currentTarget;
    private float targetLostTimer;

    public void Init(GameManager gameManager)
    {
        this.gameManager = gameManager;
        playerCamera = Camera.main;
    }

    protected override void HandleAction()
    {
        // 이동 입력은 항상 먼저 처리
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        movementDirection = new Vector2(horizontal, vertical).normalized;

        // 공격 입력만 분기 (수동/자동)
        if (!useAutoAttack)
        {
            Vector2 mousePosition = Input.mousePosition;
            Vector2 worldPos = playerCamera.ScreenToWorldPoint(mousePosition);
            Vector2 dir = (worldPos - (Vector2)transform.position);

            lookDirection = (dir.magnitude < 0.9f) ? Vector2.zero : dir.normalized;
            isAttacking = Input.GetMouseButton(0);
            return;
        }

        // 주기적으로 주변 적 스캔
        scanTimer -= Time.deltaTime;
        if (scanTimer <= 0f)
        {
            scanTimer = scanInterval;
            ReacquireTargetIfNeeded();
        }

        if (currentTarget != null)
        {
            Vector2 dir = (currentTarget.position - transform.position);
            lookDirection = (dir.sqrMagnitude > 0.0001f) ? dir.normalized : Vector2.zero;
            isAttacking = true;

            if (!allowMoveWhileAttacking)
            {
                // 사거리 안에 있으면 멈추고 쏘는 스타일
                if (InRange(currentTarget))
                    movementDirection = Vector2.zero;
            }
        }
        else
        {
            lookDirection = Vector2.zero;
            isAttacking = false;
        }
    }

    private void ReacquireTargetIfNeeded()
    {
        // 현재 타겟이 여전히 유효한지 검증
        if (IsValidTarget(currentTarget))
        {
            targetLostTimer = 0f; // 여전히 유효하면 타이머 리셋
            return;
        }

        // 유예 시간 동안 기존 타겟 유지 시도 (잠깐 가려진 경우 등)
        if (currentTarget != null)
        {
            targetLostTimer += scanInterval;
            if (targetLostTimer < keepTargetDuration && InRange(currentTarget))
            {
                // 잠깐 가려졌을 수 있음 → 조금 더 유지
                return;
            }
        }

        // 완전히 상실 → 재탐색
        currentTarget = AcquireNearestVisibleTarget();
        targetLostTimer = 0f;
    }

    private bool InRange(Transform t)
    {
        if (t == null) return false;
        float distSqr = (t.position - transform.position).sqrMagnitude;
        return distSqr <= (autoAttackRange * autoAttackRange);
    }

    private bool HasLineOfSight(Transform t)
    {
        if (t == null) return false;
        if (lineOfSightMask.value == 0) return true; // 마스크 비어있으면 시야 검사 생략

        Vector2 origin = transform.position;
        Vector2 dir = (t.position - transform.position).normalized;
        float dist = Vector2.Distance(origin, t.position);

        // 라인캐스트로 레벨/벽 레이어에 막히는지 확인
        var hit = Physics2D.Raycast(origin, dir, dist, lineOfSightMask);
        return hit.collider == null;
    }

    private bool IsValidTarget(Transform t)
    {
        return t != null && InRange(t) && HasLineOfSight(t);
    }

    private Transform AcquireNearestVisibleTarget()
    {
        Collider2D[] results = Physics2D.OverlapCircleAll(transform.position, autoAttackRange, enemyLayer);

        Transform best = null;
        float bestDistSqr = float.PositiveInfinity;

        foreach (var c in results)
        {
            if (c == null) continue;

            var t = c.attachedRigidbody ? c.attachedRigidbody.transform : c.transform;
            if (t == transform) continue;

            if (!HasLineOfSight(t)) continue;

            float d = (t.position - transform.position).sqrMagnitude;
            if (d < bestDistSqr)
            {
                bestDistSqr = d;
                best = t;
            }
        }
        return best;
    }

    private void OnDrawGizmosSelected()
    {
        if (!useAutoAttack) return;
        Gizmos.color = new Color(0f, 0.6f, 1f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, autoAttackRange);
    }

    public override void Death()
    {
        base.Death();
        gameManager.GameOver(); // 게임 오버 처리
    }
}
