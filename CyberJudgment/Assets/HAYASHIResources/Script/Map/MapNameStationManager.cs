using UnityEngine;
using TMPro;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using AbubuResouse.Singleton;
using AbubuResouse.Log;
public class MapNameStationManager : MonoBehaviour
{
    [Header("UI関連")]
    [SerializeField] private CanvasGroup mapNameCanvasGroup;
    [SerializeField] private TextMeshProUGUI mapNameText;

    [Header("サウンド関連")]
    [SerializeField] private string uiActiveOnSE = "ShowUI_SE";
    [SerializeField, Range(0f, 1f)] private float volume = 1.0f;

    [Header("表示関連")]
    [SerializeField, Tooltip("フェードイン・アウトの時間(秒)")]
    private float fadeDuration = 0.5f;
    [SerializeField, Tooltip("表示を維持する時間(秒)")]
    private float displayTime = 3.0f;
    [SerializeField, Tooltip("Startで表示させたいマップ名。空の場合は表示なし")]
    private string initialMapName = "Default Map Name";

    private bool hasCollider = false;
    private bool hasShownThisSession = false;  // 一度表示したらリセットまで表示しないためのフラグ

    private async void Start()
    {
        // オブジェクトにColliderがあるかどうかチェック
        hasCollider = TryGetComponent<Collider>(out var col);

        if (!hasCollider)
        {
            await UniTask.Delay(2000);
            // コライダーが無ければStartで表示
            if (!string.IsNullOrEmpty(initialMapName))
            {
                ShowMapNameAsync(initialMapName).Forget();
                hasShownThisSession = true;
            }
        }
        else
        {
            DebugUtility.Log("判定セット");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasCollider && !hasShownThisSession && other.CompareTag("Player"))
        {
            if (!string.IsNullOrEmpty(initialMapName))
            {
                ShowMapNameAsync(initialMapName).Forget();
                hasShownThisSession = true;
            }
        }
    }

    /// <summary>
    /// マップ名を表示する公開メソッド
    /// 必要であれば外部から任意の表示時間やフェード時間を指定して呼び出せるようにオーバーロード
    /// </summary>
    public UniTaskVoid ShowMapNameAsync(string mapName)
    {
        ShowMapNameAsync(mapName, displayTime, fadeDuration).Forget();
        return default;
    }

    public async UniTask ShowMapNameAsync(string mapName, float displayDuration, float fadeTime)
    {
        if (!ValidateUIComponents())
        {
            Debug.LogWarning("CanvasGroupまたはTextMeshProUGUIが設定されていません。処理を中断します。");
            return;
        }

        SetupUIForDisplay(mapName);

        await FadeUI(mapNameCanvasGroup, 0f, 1f, fadeTime);

        PlayUIActivationSound();

        await UniTask.Delay((int)(displayDuration * 1000));

        await FadeUI(mapNameCanvasGroup, 1f, 0f, fadeTime);

        mapNameCanvasGroup.gameObject.SetActive(false);
    }

    /// <summary>
    /// UIコンポーネントが正しく設定されているか確認
    /// </summary>
    private bool ValidateUIComponents()
    {
        return mapNameCanvasGroup != null && mapNameText != null;
    }

    /// <summary>
    /// UI表示前の初期化処理
    /// </summary>
    private void SetupUIForDisplay(string mapName)
    {
        mapNameCanvasGroup.gameObject.SetActive(true);
        mapNameCanvasGroup.alpha = 0f;
        mapNameText.text = mapName;
    }

    /// <summary>
    /// CanvasGroupをフェードさせる共通処理
    /// </summary>
    private async UniTask FadeUI(CanvasGroup targetCanvasGroup, float fromAlpha, float toAlpha, float duration)
    {
        if (targetCanvasGroup == null)
        {
            Debug.LogWarning("フェード対象のCanvasGroupが未設定です。");
            return;
        }

        targetCanvasGroup.alpha = fromAlpha;
        await targetCanvasGroup
            .DOFade(toAlpha, duration)
            .SetUpdate(true)
            .AsyncWaitForCompletion()
            .AsUniTask();
    }

    /// <summary>
    /// UI表示時に再生するサウンドを再生する共通処理
    /// </summary>
    private void PlayUIActivationSound()
    {
        if (SEManager.Instance != null && !string.IsNullOrEmpty(uiActiveOnSE))
        {
            SEManager.Instance.PlaySound(uiActiveOnSE, volume);
        }
    }
}
