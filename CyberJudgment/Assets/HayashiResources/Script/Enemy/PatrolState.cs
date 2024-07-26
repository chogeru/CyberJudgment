/// <summary>
/// �G���ݒ肳�ꂽ�|�C���g�Ԃ����񂷂���
/// </summary>
public class PatrolState : IEnemyState
{

    /// <summary>
    /// �����Ԃɓ��鎞�ɌĂяo����郁�\�b�h
    /// </summary>
    /// <param name="enemy">���̏�Ԃɓ���G</param>
    public void EnterState(EnemyBase enemy)
    {
        enemy._animator.SetBool("isMoving", true);
    }

    /// <summary>
    /// �����Ԃ��X�V���邽�߂ɌĂяo����郁�\�b�h
    /// </summary>
    /// <param name="enemy">���̏�Ԃɂ���G</param>
    public void UpdateState(EnemyBase enemy)
    {
        Enemy specificEnemy = enemy as Enemy;
        if (specificEnemy != null)
        {
            specificEnemy.Patrol();
        }

        if (enemy.isPlayerInSight)
        {
            enemy.TransitionToState(new ChaseState());
        }
    }

    /// <summary>
    /// �����Ԃ�ޏo���鎞�ɌĂяo����郁�\�b�h
    /// </summary>
    /// <param name="enemy">���̏�Ԃ�ޏo����G</param>
    public void ExitState(EnemyBase enemy)
    {
    }
}
