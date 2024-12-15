using UnityEngine;

/// <summary>
/// 敵がプレイヤーから距離を取る状態
/// </summary>
public class RetreatState : IEnemyState
{
    private float retreatDuration; // 後退時間
    private float timer; // 経過時間
    private Vector3 retreatDirection; // 後退方向
    private float originalAnimatorSpeed; // 元のアニメーション速度

    public void EnterState(EnemyBase enemy)
    {
        // 1.5～3秒のランダム時間を設定
        retreatDuration = Random.Range(5f, 10f);
        timer = 0f;

        // プレイヤーの反対方向に後退
        retreatDirection = (enemy.transform.position - enemy._player.position).normalized;

        // 移動アニメーションの再生
        enemy._animator.SetBool("isMoving", true);

        // アニメーションを逆再生するために速度を-1に設定
        originalAnimatorSpeed = enemy._animator.speed;
        enemy._animator.speed = -1f;

        Debug.Log($"{enemy.name}: Enter RetreatState");
    }

    public void UpdateState(EnemyBase enemy)
    {
        timer += Time.deltaTime;

        // 壁が後方にない場合は後退する
        if (!IsWallBehind(enemy))
        {
            Vector3 retreatPosition = enemy.transform.position + retreatDirection * enemy.enemyData.moveSpeed * Time.deltaTime;
            enemy.MoveTowards(retreatPosition);
            enemy.RotateTowards(retreatPosition);
        }
        else
        {
            // 壁が後方にある場合は攻撃状態に移行
            if (enemy.isPlayerInSight && !enemy.GetIsAttacking())
            {
                int attackChoice = Random.Range(0, 2); // 0 または 1
                if (attackChoice == 0)
                {
                    enemy._animator.SetBool("Attack", true);
                    enemy.TransitionToState(new AttackState());
                }
                else
                {
                    enemy._animator.SetBool("StrongAttack", true);
                    enemy.TransitionToState(new StrongAttackState());
                }
            }
            return;
        }

        // 後退時間が終了したら IdleState に遷移
        if (timer >= retreatDuration)
        {
            enemy.TransitionToState(new IdleState());
        }
    }

    public void ExitState(EnemyBase enemy)
    {
        // 移動アニメーションの停止
        enemy._animator.SetBool("isMoving", false);

        // アニメーション速度を元に戻す
        enemy._animator.speed = originalAnimatorSpeed;

        Debug.Log($"{enemy.name}: Exit RetreatState");
    }

    /// <summary>
    /// 敵の後方に壁があるかを判定するメソッド
    /// </summary>
    /// <param name="enemy">現在の敵</param>
    /// <returns>後方に壁がある場合はtrue</returns>
    private bool IsWallBehind(EnemyBase enemy)
    {
        Vector3 raycastOrigin = enemy.transform.position + Vector3.up * 1f;
        Vector3 raycastDirection = -enemy.transform.forward; // 敵の後方方向
        float raycastDistance = 1f; // レイキャストの距離

        RaycastHit hit;
        if (Physics.Raycast(raycastOrigin, raycastDirection, out hit, raycastDistance))
        {
            // 壁に当たった場合
            if (hit.collider.CompareTag("Default"))
            {
                return true;
            }
        }
        return false;
    }
}
