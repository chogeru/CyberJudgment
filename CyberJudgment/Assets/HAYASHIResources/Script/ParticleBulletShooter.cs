using UnityEngine;
using Cysharp.Threading.Tasks;
using uPools;

public class ParticleBulletShooter : MonoBehaviour
{
    [Header("エフェクト設定")]
    public GameObject effectPrefab;

    [Header("発射位置")]
    public Transform firePoint;

    [Header("ターゲット設定")]
    // ターゲットのTransformをInspectorから設定してください
    public Transform target;

    [Header("発射設定")]
    // 初速の大きさ（forceの大きさ）
    public float bulletSpeed = 5f;
    public float cooldownTime = 0.2f;
    private float nextFireTime = 0f;

    void Update()
    {
        // マウス左ボタンが押されている場合
        if (Input.GetMouseButton(0))
        {
            if (Time.time >= nextFireTime)
            {
                FireEffectWithForce();
                nextFireTime = Time.time + cooldownTime;
            }
        }
    }

    /// <summary>
    /// エフェクトを生成し、ターゲット方向にRigidbodyへ力を加える
    /// </summary>
    void FireEffectWithForce()
    {
        if (effectPrefab == null)
        {
            Debug.LogError("effectPrefab が設定されていません");
            return;
        }
        if (firePoint == null)
        {
            Debug.LogError("firePoint が設定されていません");
            return;
        }
        if (target == null)
        {
            Debug.LogError("ターゲットが設定されていません");
            return;
        }

        // firePointからtargetへの方向ベクトルを計算（単位ベクトル）
        Vector3 direction = (target.position - firePoint.position).normalized;
        // エフェクトの向きをターゲット方向に合わせる
        Quaternion rotation = Quaternion.LookRotation(direction);

        // SharedGameObjectPoolからエフェクトインスタンスを借りる（位置はfirePoint、回転はターゲット方向）
        GameObject effectInstance = SharedGameObjectPool.Rent(effectPrefab, firePoint.position, rotation);
        if (effectInstance == null)
        {
            Debug.LogError($"エフェクトの生成に失敗!:名前＝＞{effectPrefab.name}");
            return;
        }

        // Rigidbodyコンポーネントを取得して、状態をリセット後にターゲット方向へ力を加える
        Rigidbody rb = effectInstance.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // 前回の発射時の速度や回転速度をリセット
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.AddForce(direction * bulletSpeed, ForceMode.Impulse);
        }
        else
        {
            Debug.LogWarning("生成されたエフェクトに Rigidbody コンポーネントがありません");
        }

        // 2秒後にエフェクトをプールに返却する
        ReturnEffectAfterDelay(effectInstance, 2f).Forget();
    }

    /// <summary>
    /// 指定時間後にエフェクトをプールへ返却する（UniTaskを使用）
    /// </summary>
    /// <param name="effect">返却するエフェクト</param>
    /// <param name="delay">返却までの秒数</param>
    private async UniTaskVoid ReturnEffectAfterDelay(GameObject effect, float delay)
    {
        await UniTask.Delay((int)(delay * 1000));
        SharedGameObjectPool.Return(effect);
    }
}
