/// <summary>
/// �G�̏�Ԃ��`����C���^�[�t�F�[�X
/// </summary>
public interface IEnemyState
{
    /// <summary>
    /// ���̏�Ԃɓ��鎞�ɌĂяo����郁�\�b�h
    /// </summary>
    /// <param name="enemy">���̏�Ԃɓ���G</param>
    void EnterState(EnemyBase enemy);
    /// <summary>
    /// ���̏�Ԃ��X�V���邽�߂ɌĂяo����郁�\�b�h
    /// </summary>
    /// <param name="enemy">���̏�Ԃɂ���G</param>
    void UpdateState(EnemyBase enemy);
    /// <summary>
    /// ���̏�Ԃ�ޏo���鎞�ɌĂяo����郁�\�b�h
    /// </summary>
    /// <param name="enemy">���̏�Ԃ�ޏo����G</param>
    void ExitState(EnemyBase enemy);
}
