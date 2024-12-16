using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Cysharp.Threading.Tasks;

public class DeathEffect : MonoBehaviour
{
    [Header("グローバルボリューム")]
    [SerializeField]
    private Volume globalVolume;

    private DepthOfField depthOfField;

    [Header("エフェクト設定")]
    [SerializeField]
    private GameObject dieAnimationObj;

    private void Awake()
    {
        // グローバルボリュームからDepthOfFieldを取得
        if (globalVolume != null)
        {
            if (!globalVolume.profile.TryGet<DepthOfField>(out depthOfField))
            {
                Debug.LogError("DepthOfFieldコンポーネントが見つかりません。");
            }
        }
        else
        {
            Debug.LogError("グローバルボリュームが割り当てられていません。");
        }

        if (dieAnimationObj == null)
        {
            Debug.LogError("特定のオブジェクトが割り当てられていません。");
        }
    }

    /// <summary>
    /// 死亡エフェクトを開始します（焦点距離を調整）。
    /// </summary>
    public async UniTask StartDeathEffectAsync()
    {
        if (depthOfField != null)
        {
            // 被写界深度をBokehモードに設定
            depthOfField.active = true;
            depthOfField.mode.value = DepthOfFieldMode.Bokeh;

            float duration = 6f; // 5秒間かけて調整
            float elapsed = 0f;
            float startFocalLength = 1f;  // 初期の焦点距離
            float endFocalLength = 90f; // 最終の焦点距離

            // 焦点距離を徐々に変更
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                depthOfField.focalLength.value = Mathf.Lerp(startFocalLength, endFocalLength, t);
                await UniTask.Yield();
            }

            // 最終的な焦点距離に設定
            depthOfField.focalLength.value = endFocalLength;
        }

        // 特定のオブジェクトをアクティブ化
        if (dieAnimationObj != null)
        {
            dieAnimationObj.SetActive(true);
        }
    }

    /// <summary>
    /// エフェクトをリセットします。
    /// </summary>
    public void ResetEffect()
    {
        if (depthOfField != null)
        {
            depthOfField.focalLength.value = 1f; // 初期の焦点距離に戻す
            depthOfField.mode.value = DepthOfFieldMode.Off;
            depthOfField.active = false;
        }

        if (dieAnimationObj != null)
        {
            dieAnimationObj.SetActive(false);
        }
    }
}
