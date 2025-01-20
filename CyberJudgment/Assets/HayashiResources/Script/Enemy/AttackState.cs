using UnityEngine;

/// <summary>
/// 通常攻撃ステート
/// </summary>
public class AttackState : IEnemyState
{
    private float originalAnimatorSpeed;

    public void EnterState(EnemyBase enemy)
    {
        // ■攻撃開始時点でクールダウンをリセット
        enemy.ResetAttackCooldown();

        originalAnimatorSpeed = enemy._animator.speed;
        enemy._animator.speed = 1f;

        // 攻撃フラグON
        enemy.SetIsAttacking(true);

        // 攻撃アニメ再生用フラグ
        enemy._animator.SetBool("Attack", true);

        Debug.Log($"{enemy.name}: Enter AttackState");
    }

    public void UpdateState(EnemyBase enemy)
    {
        if (enemy._player == null) return;

        float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy._player.position);

        // 射程外 or 視界外になったらChaseへ (攻撃やめる)
        if (distanceToPlayer > enemy.enemyData.attackRange || !enemy.isPlayerInSight)
        {
            enemy._animator.SetBool("Attack", false);
            enemy.TransitionToState(new ChaseState());
            return;
        }

        // 攻撃アニメが最後まで再生されたら後退ステートへ
        AnimatorStateInfo stateInfo = enemy._animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Attack") && stateInfo.normalizedTime >= 1.0f && enemy.GetIsAttacking())
        {
            enemy._animator.SetBool("Attack", false);
            enemy.TransitionToState(new RetreatState());
        }
    }

    public void ExitState(EnemyBase enemy)
    {
        // ステート抜ける時に攻撃フラグOFF
        enemy.SetIsAttacking(false);
        enemy._animator.SetBool("Attack", false);

        // アニメ速度戻す
        enemy._animator.speed = originalAnimatorSpeed;

        Debug.Log($"{enemy.name}: Exit AttackState");
    }
}
