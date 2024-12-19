using UnityEngine;
using Cysharp.Threading.Tasks;
public class RespawnManager : MonoBehaviour
{
    [Header("プレイヤー参照")]
    [SerializeField]
    private PlayerHealth playerHealth;

    [Header("リスポーン設定")]
    [SerializeField]
    private Transform respawnPoint;

    [Header("DeathEffect参照")]
    [SerializeField]
    private DeathEffect deathEffect;

    [Header("リスポーン待機時間")]
    [SerializeField]
    private float respawnDelay = 6f;

    private void Awake()
    {
        if (playerHealth != null)
        {
            playerHealth.OnPlayerDeath += StartRespawnSequence;
        }
        else
        {
            Debug.LogError("RespawnManagerにPlayerHealthの参照が割り当てられていません。");
        }

        if (deathEffect == null)
        {
            Debug.LogError("RespawnManagerにDeathEffectの参照が割り当てられていません。");
        }

        if (respawnPoint == null)
        {
            Debug.LogError("RespawnManagerにRespawnPointが割り当てられていません。");
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnPlayerDeath -= StartRespawnSequence;
        }
    }

    /// <summary>
    /// リスポーンシーケンスを開始します。
    /// </summary>
    public void StartRespawnSequence()
    {
        RespawnSequence().Forget();
    }

    /// <summary>
    /// リスポーンのシーケンス処理
    /// </summary>
    private async UniTaskVoid RespawnSequence()
    {
        // DeathEffectのエフェクトを開始
        if (deathEffect != null)
        {
            await deathEffect.StartDeathEffectAsync();
        }

        // リスポーン待機時間を待つ
        await UniTask.Delay(System.TimeSpan.FromSeconds(respawnDelay));

        // リスポーン処理
        RespawnPlayer();
    }

    /// <summary>
    /// プレイヤーをリスポーンさせます。
    /// </summary>
    private void RespawnPlayer()
    {
        if (playerHealth != null && respawnPoint != null)
        {
            // プレイヤーの位置をリスポーン地点に設定
            playerHealth.transform.position = respawnPoint.position;

            // プレイヤーの状態をリセット
            playerHealth.ResetState();

            // DeathEffectのエフェクトをリセット
            if (deathEffect != null)
            {
                deathEffect.ResetEffect();
            }
        }
        else
        {
            Debug.LogError("RespawnPlayer処理に必要な参照が割り当てられていません。");
        }
    }
}
