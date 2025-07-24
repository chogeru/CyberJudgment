using UnityEngine;
using Cysharp.Threading.Tasks;
using uPools;
using AbubuResouse.Singleton;

public class ParticleBulletShooter : MonoBehaviour
{
    [Header("エフェクト設定")]
    public GameObject effectPrefab;

    [Header("発射位置")]
    public Transform firePoint;

    [Header("親オブジェクト設定")]
    public Transform bulletParent; // 弾の親にするオブジェクト

    [Header("ターゲット設定")]
    public Transform target;

    [Header("発射SE設定")]
    public string[] fireSEs;
    [SerializeField]
    private float volume;

    [Header("発射設定")]
    public float bulletSpeed = 5f;
    public float cooldownTime = 0.2f;

    [Header("位置調整")]
    public Vector3 firePointOffset = Vector3.zero; // 発射位置の微調整用

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

        // 発射位置にオフセットを適用
        Vector3 actualFirePosition = firePoint.position + firePoint.TransformDirection(firePointOffset);

        // actualFirePositionからtargetへの方向ベクトルを計算（単位ベクトル）
        Vector3 direction = (target.position - actualFirePosition).normalized;

        // エフェクトの向きをターゲット方向に合わせる
        Quaternion rotation = Quaternion.LookRotation(direction);

        // SharedGameObjectPoolからエフェクトインスタンスを借りる（位置は調整後のfirePoint、回転はターゲット方向）
        GameObject effectInstance = SharedGameObjectPool.Rent(effectPrefab, actualFirePosition, rotation);
        if (effectInstance == null)
        {
            Debug.LogError($"エフェクトの生成に失敗!:名前＝＞{effectPrefab.name}");
            return;
        }

        // 弾を指定の親オブジェクトの子にする
        if (bulletParent != null)
        {
            effectInstance.transform.SetParent(bulletParent, true);
        }

        // オブジェクト内のすべての TrailRenderer を取得して状態リセット
        TrailRenderer[] trails = effectInstance.GetComponentsInChildren<TrailRenderer>();
        foreach (TrailRenderer trail in trails)
        {
            trail.Clear();
            // 一度無効化してから再有効化することで、内部状態をリセット
            trail.enabled = false;
            trail.enabled = true;
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

        // SEを再生
        if (fireSEs == null || fireSEs.Length == 0)
        {
            Debug.LogError("fireSEs にSEが設定されていません");
        }
        else
        {
            string selectedSE = fireSEs[Random.Range(0, fireSEs.Length)];
            SEManager.Instance.PlaySound(selectedSE, volume);
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

        // 返却時に親子関係を解除
        if (effect != null)
        {
            effect.transform.SetParent(null);
            SharedGameObjectPool.Return(effect);
        }
    }
}