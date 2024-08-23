using AbubuResouse.Singleton;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uPools;
using static JBooth.MicroVerseCore.Ambient;

public class EnemyAttackCol : MonoBehaviour
{
    enum AttackColType
    {
        Nomal,
        Strong,
    }

    [SerializeField, Header("�U���R���C�_�[�̃^�C�v")]
    private AttackColType _type;

    [SerializeField, Header("����̃X�e�[�^�X�f�[�^")]
    private EnemyData _enemyData;

    [SerializeField, Header("�U�����̃G�t�F�N�g")]
    private GameObject _effect;

    [SerializeField, Header("�U���T�E���h")]
    private List<string> _attackSounds;
    [SerializeField, Header("����")]
    private float _volume;

    [SerializeField, Header("�U����")]
    private int _attackPower;

    private void Start()
    {
        switch (_type)
        {
            case AttackColType.Nomal:
                _attackPower = _enemyData.meleeAttackPower;
                break;
            case AttackColType.Strong:
                _attackPower *= _enemyData.meleeStringAttackpower;
                break;
        }
        SharedGameObjectPool.Prewarm(_effect, 10);
    }

    void GenerateAttackEffect()
    {
        // uPools���g�p���ăG�t�F�N�g�𐶐�
        GameObject effect = SharedGameObjectPool.Rent(
            _effect,
            transform.position,
            Quaternion.identity);
        // ��莞�Ԍ�ɃG�t�F�N�g��ԋp
        ReturnEffectAfterDelay(effect, 0.5f).Forget();

    }

    private void OnTriggerEnter(Collider other)
    {
        // ���蔲�����Ώۂ�Enemy�^�O�������m�F
        if (other.CompareTag("Player"))
        {
            // EnemyBase �N���X�������Ă��邩�m�F���ă_���[�W��^����
            PlayerController enemy = other.GetComponent<PlayerController>();
            if (enemy != null)
            {
                string randomSound = _attackSounds[Random.Range(0, _attackSounds.Count)];
                GenerateAttackEffect();
                SEManager.Instance.PlaySound(randomSound, _volume);
                //enemy.TakeDamage(_attackPower);
            }
        }
        if(other.CompareTag("Ground"))
        {
            string randomSound = _attackSounds[Random.Range(0, _attackSounds.Count)];
            GenerateAttackEffect();
            SEManager.Instance.PlaySound(randomSound, _volume);
        }
    }

    /// <summary>
    /// �G�t�F�N�g���w�肳�ꂽ���Ԍ�ɕԋp����
    /// </summary>
    /// <param name="effect">�ԋp����G�t�F�N�g</param>
    /// <param name="delay">�ԋp�܂ł̒x������</param>
    private async UniTaskVoid ReturnEffectAfterDelay(GameObject effect, float delay)
    {
        await UniTask.Delay((int)(delay * 1000));
        SharedGameObjectPool.Return(effect);
    }
}
