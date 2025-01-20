using UnityEngine;

/// <summary>
/// �ʏ�U���X�e�[�g
/// </summary>
public class AttackState : IEnemyState
{
    private float originalAnimatorSpeed;

    public void EnterState(EnemyBase enemy)
    {
        // ���U���J�n���_�ŃN�[���_�E�������Z�b�g
        enemy.ResetAttackCooldown();

        originalAnimatorSpeed = enemy._animator.speed;
        enemy._animator.speed = 1f;

        // �U���t���OON
        enemy.SetIsAttacking(true);

        // �U���A�j���Đ��p�t���O
        enemy._animator.SetBool("Attack", true);

        Debug.Log($"{enemy.name}: Enter AttackState");
    }

    public void UpdateState(EnemyBase enemy)
    {
        if (enemy._player == null) return;

        float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy._player.position);

        // �˒��O or ���E�O�ɂȂ�����Chase�� (�U����߂�)
        if (distanceToPlayer > enemy.enemyData.attackRange || !enemy.isPlayerInSight)
        {
            enemy._animator.SetBool("Attack", false);
            enemy.TransitionToState(new ChaseState());
            return;
        }

        // �U���A�j�����Ō�܂ōĐ����ꂽ���ރX�e�[�g��
        AnimatorStateInfo stateInfo = enemy._animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Attack") && stateInfo.normalizedTime >= 1.0f && enemy.GetIsAttacking())
        {
            enemy._animator.SetBool("Attack", false);
            enemy.TransitionToState(new RetreatState());
        }
    }

    public void ExitState(EnemyBase enemy)
    {
        // �X�e�[�g�����鎞�ɍU���t���OOFF
        enemy.SetIsAttacking(false);
        enemy._animator.SetBool("Attack", false);

        // �A�j�����x�߂�
        enemy._animator.speed = originalAnimatorSpeed;

        Debug.Log($"{enemy.name}: Exit AttackState");
    }
}
