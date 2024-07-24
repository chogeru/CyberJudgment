using UnityEngine;

public class Enemy : EnemyBase
{
    [SerializeField] private Transform[] patrolPoints;
    private int currentPatrolIndex;
    private float patrolTimer;

    protected override void Start()
    {
        base.Start();
        currentPatrolIndex = 0;
        patrolTimer = 0f;
    }

    protected override void Update()
    {
        base.Update();

        if (!isPlayerInSight)
        {
            Patrol();
        }
    }

    protected override void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        Transform targetPatrolPoint = patrolPoints[currentPatrolIndex];
        MoveTowards(targetPatrolPoint.position);

        if (Vector3.Distance(transform.position, targetPatrolPoint.position) < 0.5f)
        {
            patrolTimer += Time.deltaTime;
            if (patrolTimer >= enemyData.patrolPointWaitTime)
            {
                patrolTimer = 0f;
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            }
        }
    }
}
