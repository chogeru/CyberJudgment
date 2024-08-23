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

    [SerializeField, Header("攻撃コライダーのタイプ")]
    private AttackColType _type;

    [SerializeField, Header("武器のステータスデータ")]
    private EnemyData _enemyData;

    [SerializeField, Header("攻撃時のエフェクト")]
    private GameObject _effect;

    [SerializeField, Header("攻撃サウンド")]
    private List<string> _attackSounds;
    [SerializeField, Header("音量")]
    private float _volume;

    [SerializeField, Header("攻撃力")]
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
        // uPoolsを使用してエフェクトを生成
        GameObject effect = SharedGameObjectPool.Rent(
            _effect,
            transform.position,
            Quaternion.identity);
        // 一定時間後にエフェクトを返却
        ReturnEffectAfterDelay(effect, 0.5f).Forget();

    }

    private void OnTriggerEnter(Collider other)
    {
        // すり抜けた対象がEnemyタグを持つか確認
        if (other.CompareTag("Player"))
        {
            // EnemyBase クラスを持っているか確認してダメージを与える
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
    /// エフェクトを指定された時間後に返却する
    /// </summary>
    /// <param name="effect">返却するエフェクト</param>
    /// <param name="delay">返却までの遅延時間</param>
    private async UniTaskVoid ReturnEffectAfterDelay(GameObject effect, float delay)
    {
        await UniTask.Delay((int)(delay * 1000));
        SharedGameObjectPool.Return(effect);
    }
}
