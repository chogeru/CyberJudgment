using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

public class LipSync : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
    public SkinnedMeshRenderer skinnedMeshRenderer;
    public float shapeKeyWeight = 100f;
    public float speakSpeed = 0.15f; // 各文字の合計アニメーション時間

    // ブレンドシェイプの名前（Fcl_MTH_形式）
    public string shapeKeyNameA = "Fcl_MTH_A";
    public string shapeKeyNameI = "Fcl_MTH_I";
    public string shapeKeyNameU = "Fcl_MTH_U";
    public string shapeKeyNameE = "Fcl_MTH_E";
    public string shapeKeyNameO = "Fcl_MTH_O";

    private int shapeKeyA, shapeKeyI, shapeKeyU, shapeKeyE, shapeKeyO;
    private string previousText = "";
    private int processedCharCount = 0; // すでに処理した文字数
    private CancellationTokenSource cts;
    private CancellationTokenSource lipSyncCTS;

    // 直近で文字が追加された時間
    private float lastCharacterTime = 0f;
    // 新たな文字追加がないと判断する閾値（秒）
    private readonly float typingTimeout = 0.3f;

    Dictionary<char, int> charToShapeKey;

    // TimelineTextManagerへの参照（必要ならInspectorに設定）
    public TimelineTextManager timelineTextManager;

    void Start()
    {
        if (textMeshPro == null || skinnedMeshRenderer == null)
        {
            Debug.LogError("必要なコンポーネントが設定されていません。");
            enabled = false;
            return;
        }

        // TimelineTextManagerの参照がなければシーン内から取得（任意）
        if (timelineTextManager == null)
        {
            timelineTextManager = FindObjectOfType<TimelineTextManager>();
        }

        // ブレンドシェイプ名からインデックスを取得
        shapeKeyA = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(shapeKeyNameA);
        shapeKeyI = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(shapeKeyNameI);
        shapeKeyU = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(shapeKeyNameU);
        shapeKeyE = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(shapeKeyNameE);
        shapeKeyO = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(shapeKeyNameO);

        Debug.Log($"BlendShape Indices: A={shapeKeyA}, I={shapeKeyI}, U={shapeKeyU}, E={shapeKeyE}, O={shapeKeyO}");

        // ひらがなとその派生音のマッピング例
        charToShapeKey = new Dictionary<char, int>
        {
            {'あ', shapeKeyA}, {'か', shapeKeyA}, {'さ', shapeKeyA}, {'た', shapeKeyA}, {'な', shapeKeyA},
            {'は', shapeKeyA}, {'ま', shapeKeyA}, {'や', shapeKeyA}, {'ら', shapeKeyA}, {'わ', shapeKeyA},
            {'が', shapeKeyA}, {'ざ', shapeKeyA}, {'だ', shapeKeyA}, {'ば', shapeKeyA}, {'ぱ', shapeKeyA}, {'ぁ', shapeKeyA},

            {'い', shapeKeyI}, {'き', shapeKeyI}, {'し', shapeKeyI}, {'ち', shapeKeyI}, {'に', shapeKeyI},
            {'ひ', shapeKeyI}, {'み', shapeKeyI}, {'り', shapeKeyI}, {'ぎ', shapeKeyI}, {'じ', shapeKeyI},
            {'ぢ', shapeKeyI}, {'び', shapeKeyI}, {'ぴ', shapeKeyI}, {'ぃ', shapeKeyI},

            {'う', shapeKeyU}, {'く', shapeKeyU}, {'す', shapeKeyU}, {'つ', shapeKeyU}, {'ぬ', shapeKeyU},
            {'ふ', shapeKeyU}, {'む', shapeKeyU}, {'ゆ', shapeKeyU}, {'る', shapeKeyU}, {'ぐ', shapeKeyU},
            {'ず', shapeKeyU}, {'づ', shapeKeyU}, {'ぶ', shapeKeyU}, {'ぷ', shapeKeyU}, {'ぅ', shapeKeyU},

            {'え', shapeKeyE}, {'け', shapeKeyE}, {'せ', shapeKeyE}, {'て', shapeKeyE}, {'ね', shapeKeyE},
            {'へ', shapeKeyE}, {'め', shapeKeyE}, {'れ', shapeKeyE}, {'げ', shapeKeyE}, {'ぜ', shapeKeyE},
            {'で', shapeKeyE}, {'べ', shapeKeyE}, {'ぺ', shapeKeyE}, {'ぇ', shapeKeyE},

            {'お', shapeKeyO}, {'こ', shapeKeyO}, {'そ', shapeKeyO}, {'と', shapeKeyO}, {'の', shapeKeyO},
            {'ほ', shapeKeyO}, {'も', shapeKeyO}, {'よ', shapeKeyO}, {'ろ', shapeKeyO}, {'を', shapeKeyO},
            {'ご', shapeKeyO}, {'ぞ', shapeKeyO}, {'ど', shapeKeyO}, {'ぼ', shapeKeyO}, {'ぽ', shapeKeyO},
            {'ぉ', shapeKeyO}, {'ん', shapeKeyO}, {'ー', shapeKeyO}
        };

        cts = new CancellationTokenSource();
        WatchTextChange(cts.Token).Forget();
    }

    async UniTaskVoid WatchTextChange(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            // テキストが更新されたかチェック
            if (previousText != textMeshPro.text)
            {
                // 新たな文字追加時刻を更新
                lastCharacterTime = Time.time;

                // もしテキストが前回の先頭から追加されたなら差分だけ処理、そうでなければリセット
                if (textMeshPro.text.StartsWith(previousText))
                {
                    // processedCharCountはそのまま
                }
                else
                {
                    processedCharCount = 0;
                }
                previousText = textMeshPro.text;

                if (lipSyncCTS != null)
                {
                    lipSyncCTS.Cancel();
                    lipSyncCTS.Dispose();
                }
                lipSyncCTS = new CancellationTokenSource();

                ResetBlendShapes();
                await LipSyncAsync(textMeshPro.text, lipSyncCTS.Token);
            }
            else
            {
                // 直近の文字追加から一定時間経過していれば、タイピング完了とみなす
                if (Time.time - lastCharacterTime > typingTimeout)
                {
                    if (lipSyncCTS != null && !lipSyncCTS.IsCancellationRequested)
                    {
                        lipSyncCTS.Cancel();
                    }
                    ResetBlendShapes();
                }
            }
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
    }

    async UniTask LipSyncAsync(string text, CancellationToken token)
    {
        // 新たに追加された部分のみ処理
        for (int i = processedCharCount; i < text.Length; i++)
        {
            if (token.IsCancellationRequested) break;

            // 文字追加時刻を更新
            lastCharacterTime = Time.time;

            char c = text[i];
            if (charToShapeKey.TryGetValue(c, out int shapeIndex))
            {
                Debug.Log($"文字: {c} -> シェイプインデックス: {shapeIndex}");
                if (shapeIndex >= 0 && shapeIndex < skinnedMeshRenderer.sharedMesh.blendShapeCount)
                {
                    await AnimateBlendShape(shapeIndex, shapeKeyWeight, speakSpeed, token);
                }
            }
            else
            {
                Debug.Log($"文字: {c} は未マッピング。デフォルトで処理します。");
                if (shapeKeyA >= 0 && shapeKeyA < skinnedMeshRenderer.sharedMesh.blendShapeCount)
                {
                    await AnimateBlendShape(shapeKeyA, shapeKeyWeight, speakSpeed, token);
                }
            }
            processedCharCount = i + 1;
        }

        // 新たな文字が追加されなかった場合は、一定時間後に口を閉じる
        await UniTask.Delay(500, cancellationToken: token);
        if (text == textMeshPro.text)
        {
            ResetBlendShapes();
        }
    }

    // 指定したブレンドシェイプを、duration時間で滑らかに上げ下げする（SmoothStepによる補間）
    async UniTask AnimateBlendShape(int index, float targetWeight, float duration, CancellationToken token)
    {
        float halfDuration = duration / 2f;
        float timer = 0f;
        // 開くアニメーション（0→targetWeight）
        while (timer < halfDuration)
        {
            if (token.IsCancellationRequested) return;
            float t = timer / halfDuration;
            float weight = Mathf.SmoothStep(0f, targetWeight, t);
            skinnedMeshRenderer.SetBlendShapeWeight(index, weight);
            timer += Time.deltaTime;
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
        skinnedMeshRenderer.SetBlendShapeWeight(index, targetWeight);

        // 閉じるアニメーション（targetWeight→0）
        timer = 0f;
        while (timer < halfDuration)
        {
            if (token.IsCancellationRequested) return;
            float t = timer / halfDuration;
            float weight = Mathf.SmoothStep(targetWeight, 0f, t);
            skinnedMeshRenderer.SetBlendShapeWeight(index, weight);
            timer += Time.deltaTime;
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
        skinnedMeshRenderer.SetBlendShapeWeight(index, 0f);
    }

    void ResetBlendShapes()
    {
        if (shapeKeyA >= 0 && shapeKeyA < skinnedMeshRenderer.sharedMesh.blendShapeCount)
            skinnedMeshRenderer.SetBlendShapeWeight(shapeKeyA, 0);
        if (shapeKeyI >= 0 && shapeKeyI < skinnedMeshRenderer.sharedMesh.blendShapeCount)
            skinnedMeshRenderer.SetBlendShapeWeight(shapeKeyI, 0);
        if (shapeKeyU >= 0 && shapeKeyU < skinnedMeshRenderer.sharedMesh.blendShapeCount)
            skinnedMeshRenderer.SetBlendShapeWeight(shapeKeyU, 0);
        if (shapeKeyE >= 0 && shapeKeyE < skinnedMeshRenderer.sharedMesh.blendShapeCount)
            skinnedMeshRenderer.SetBlendShapeWeight(shapeKeyE, 0);
        if (shapeKeyO >= 0 && shapeKeyO < skinnedMeshRenderer.sharedMesh.blendShapeCount)
            skinnedMeshRenderer.SetBlendShapeWeight(shapeKeyO, 0);
    }

    void OnDestroy()
    {
        cts.Cancel();
        cts.Dispose();
        if (lipSyncCTS != null)
        {
            lipSyncCTS.Cancel();
            lipSyncCTS.Dispose();
        }
    }
}
