using UnityEngine;

public class EnemyController : BaseController
{
    private EnemyManager enemyManager;
    private Transform target; // 공격할 대상

    [SerializeField] private float followRange = 15f; // 타겟을 쫓아갈 최대 거리

    // 적이 생성될 때 초기화
    public void Init(EnemyManager enemyManager, Transform target)
    {
        this.enemyManager = enemyManager;
        this.target = target;
    }

    // 타겟과의 거리 계산
    protected float DistanceToTarget()
    {
        return Vector3.Distance(transform.position, target.position);
    }

    // 매 프레임마다 적의 동작 판단 및 처리
    protected override void HandleAction()
    {
        // 부모에서 정의한 기본 동작 처리
        base.HandleAction();

        // 무기나 타겟이 없으면 아무 행동도 하지 않음
        if (weaponHandler == null || target == null)
        {
            // 정지 처리
            if (!movementDirection.Equals(Vector2.zero)) movementDirection = Vector2.zero;
            return;
        }

        float distance = DistanceToTarget(); // 타겟까지 거리
        Vector2 direction = DirectionToTarget(); // 타겟을 향한 방향

        isAttacking = false; // 기본적으로 공격 중 아님

        // 타겟이 일정 거리 안에 있을 때만 추적 시작
        if (distance <= followRange)
        {
            lookDirection = direction; // 방향 전환

            // 공격 사거리 안으로 들어왔을 경우
            if (distance <= weaponHandler.AttackRange)
            {
                int layerMaskTarget = weaponHandler.target;

                // 앞에 장애물이 없는지 확인하며, 대상이 맞는지 체크
                RaycastHit2D hit = Physics2D.Raycast(
                    transform.position,
                    direction,
                    weaponHandler.AttackRange * 1.5f, // 사거리보다 약간 여유 있게
                    (1 << LayerMask.NameToLayer("Level")) | layerMaskTarget
                );

                // 공격 가능한 대상이 정확히 맞았을 때만 공격
                if (hit.collider != null && layerMaskTarget == (layerMaskTarget | (1 << hit.collider.gameObject.layer)))
                {
                    isAttacking = true;
                }

                // 공격 범위 안이므로 정지
                movementDirection = Vector2.zero;
                return;
            }

            // 공격 범위는 아니므로 타겟을 향해 이동
            movementDirection = direction;
        }

    }

    // 타겟을 향한 방향 벡터 계산
    protected Vector2 DirectionToTarget()
    {
        return (target.position - transform.position).normalized;
    }

    public override void Death()
    {
        base.Death();
        enemyManager.RemoveEnemyOnDeath(this);
    }
}