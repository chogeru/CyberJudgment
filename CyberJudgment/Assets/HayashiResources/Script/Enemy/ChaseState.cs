using UnityEngine;

/// <summary>
/// “G‚ªƒvƒŒƒCƒ„[‚ğ’ÇÕ‚·‚éó‘Ô
/// </summary>
public class ChaseState : IEnemyState
{
    private float originalAnimatorSpeed;
    public void EnterState(EnemyBase enemy)
    {
        originalAnimatorSpeed = enemy._animator.speed;
        enemy._animator.speed = 1f;
        enemy._animator.SetBool("isMoving", true);
        Debug.Log($"{enemy.name}: Enter ChaseState");
    }

    public void UpdateState(EnemyBase enemy)
    {
        // UŒ‚’†‚Å‚È‚¯‚ê‚Î’ÇÕ
        if (!enemy.GetIsAttacking())
        {
            enemy.MoveTowards(enemy._player.position);
            enemy.RotateTowards(enemy._player.position);
        }

        float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy._player.position);

        // UŒ‚”ÍˆÍ“à‚ÅUŒ‚’†‚Å‚È‚¯‚ê‚ÎUŒ‚ƒXƒe[ƒg‚É‘JˆÚ
        if (distanceToPlayer <= enemy.enemyData.attackRange && !enemy.GetIsAttacking())
        {
            int attackChoice = Random.Range(0, 2); // 0 ‚Ü‚½‚Í 1
            if (attackChoice == 0)
            {
                enemy.TransitionToState(new AttackState());
            }
            else
            {
                enemy.TransitionToState(new StrongAttackState());
            }
            return;
        }

        // ƒvƒŒƒCƒ„[‚ª‹ŠE‚É“ü‚Á‚Ä‚¢‚È‚¯‚ê‚Î IdleState ‚É‘JˆÚ
        if (!enemy.isPlayerInSight)
        {
            enemy.TransitionToState(new IdleState());
            return;
        }
    }

    public void ExitState(EnemyBase enemy)
    {
        enemy._animator.SetBool("isMoving", false);
        Debug.Log($"{enemy.name}: Exit ChaseState");
    }
}
