using UnityEngine;

/// <summary>
/// 敵の実装クラス
/// </summary>
public class Enemy : EnemyBase
{
    [SerializeField,Header("巡回ポイントの配列")]
    private Transform[] _patrolPoints;
    //現在の巡回ポイント
    private int _currentPatrolIndex;
    //巡回ポイントで待機時間
    private float _patrolTimer;

    /// <summary>
    /// 敵の初期設定
    /// </summary>
    protected override void Start()
    {
        base.Start();
        _currentPatrolIndex = 0;
        _patrolTimer = 0f;
        TransitionToState(new IdleState());
    }

    protected override void Update()
    {
        base.Update();
    }

    /// <summary>
    /// 巡回動作の実装メソッド
    /// </summary>
    public override void Patrol()
    {
        if (_patrolPoints.Length == 0) return;

        Transform targetPatrolPoint = _patrolPoints[_currentPatrolIndex];
        MoveTowards(targetPatrolPoint.position);

        if (Vector3.Distance(transform.position, targetPatrolPoint.position) < 0.5f)
        {
            _patrolTimer += Time.deltaTime;
            if (_patrolTimer >= enemyData.patrolPointWaitTime)
            {
                _patrolTimer = 0f;
                _currentPatrolIndex = (_currentPatrolIndex + 1) % _patrolPoints.Length;
            }
        }
    }
}
