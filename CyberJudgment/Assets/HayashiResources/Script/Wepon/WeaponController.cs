using AbubuResouse.Singleton;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uPools;

public class WeaponController : MonoBehaviour
{
    [SerializeField,Header("武器のステータスデータ")]
    private WeaponData _weaponData;

    [SerializeField,Header("攻撃時のエフェクト")]
    private GameObject _effect;

    private void Start()
    {
        SharedGameObjectPool.Prewarm(_effect, 10);
    }

    void GenerateFootstep()
    {
        SEManager.Instance.PlaySound("FootStep", 0.1f);

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
        if (other.CompareTag("Enemy"))
        {
            // EnemyBase クラスを持っているか確認してダメージを与える
            EnemyBase enemy = other.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                // 武器データの攻撃力でダメージを与える
                enemy.TakeDamage(_weaponData._attackPower);
            }
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
