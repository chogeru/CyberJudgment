using UnityEngine;

/// <summary>
/// 攻撃後、プレイヤーから離れる (後退) 状態
/// </summary>
public class RetreatState : IEnemyState
{
    private float retreatDuration;   // 後退時間
    private float timer;             // 経過時間
    private Vector3 retreatDirection;
    private float originalAnimatorSpeed;

    public void EnterState(EnemyBase enemy)
    {
        // 1.5～3秒のランダム時間
        retreatDuration = Random.Range(1.5f, 3f);
        timer = 0f;

        // プレイヤーの反対方向へ
        if (enemy._player != null)
        {
            retreatDirection = (enemy.transform.position - enemy._player.position).normalized;
        }
        else
        {
            retreatDirection = -enemy.transform.forward;
        }

        // 後退アニメっぽく isMoving = true
        enemy._animator.SetBool("isMoving", true);

        // アニメ速度を通常に
        originalAnimatorSpeed = enemy._animator.speed;
        enemy._animator.speed = 1f;

        Debug.Log($"{enemy.name}: Enter RetreatState");
    }

    public void UpdateState(EnemyBase enemy)
    {
        timer += Time.deltaTime;

        // 後方に壁がなければ後退
        if (!IsWallBehind(enemy))
        {
            Vector3 retreatPosition = enemy.transform.position + retreatDirection * enemy.enemyData.moveSpeed * Time.deltaTime;
            enemy.MoveTowards(retreatPosition);
            enemy.RotateTowards(retreatPosition);
        }
        else
        {
            // 壁がある場合 → 攻撃 or Idleへ戻るなど
            if (enemy.isPlayerInSight && !enemy.GetIsAttacking() && enemy.CanAttack())
            {
                int attackChoice = Random.Range(0, 2);
                if (attackChoice == 0)
                    enemy.TransitionToState(new AttackState());
                else
                    enemy.TransitionToState(new StrongAttackState());
            }
            else
            {
                enemy.TransitionToState(new IdleState());
            }
            return;
        }

        // 指定時間後退したらIdleへ
        if (timer >= retreatDuration)
        {
            enemy.TransitionToState(new IdleState());
        }
    }

    public void ExitState(EnemyBase enemy)
    {
        enemy._animator.SetBool("isMoving", false);
        enemy._animator.speed = originalAnimatorSpeed;

        Debug.Log($"{enemy.name}: Exit RetreatState");
    }

    /// <summary>
    /// 後方に壁があるかチェック
    /// </summary>
    private bool IsWallBehind(EnemyBase enemy)
    {
        Vector3 raycastOrigin = enemy.transform.position + Vector3.up * 1f;
        Vector3 raycastDirection = -enemy.transform.forward;
        float raycastDistance = 1f;

        if (Physics.Raycast(raycastOrigin, raycastDirection, out RaycastHit hit, raycastDistance))
        {
            // Tag は環境に合わせる
            if (hit.collider.CompareTag("Wall") || hit.collider.CompareTag("Default"))
            {
                return true;
            }
        }
        return false;
    }
}
