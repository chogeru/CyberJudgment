using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using VInspector;
using BU = Beautify.Universal.Beautify;

[ExecuteAlways]
public class BeautifyExternalController : MonoBehaviour
{

    [Tab("Volume設定")]
    [Header("対象の Volume")]
    public Volume postProcessVolume;


    #region Frame Settings
    [Tab("Frame Settings")]
    [Header("Frame Settings")]
    public bool frame;
    public BU.FrameStyle frameStyle;
    [Range(0f, 10f)]
    public float frameBandHorizontalSize;
    [Range(0f, 10f)]
    public float frameBandHorizontalSmoothness;
    [Range(0f, 10f)]
    public float frameBandVerticalSize;
    [Range(0f, 10f)]
    public float frameBandVerticalSmoothness;
    public Color frameColor;
    public Texture frameMask;
    #endregion
    #region LUT Settings
    [Tab("LUT設定")]
    [Header("LUT Settings")]
    public bool enableLUT;
    [Range(0f, 1f)]
    public float lutIntensity;
    public Texture lutTexture;
    #endregion
    [SerializeField]
    private BU beautify;

    void OnEnable()
    {
        UpdateBeautifyReference();
        ApplySettings();
    }

    void OnValidate()
    {
        UpdateBeautifyReference();
        ApplySettings();
#if UNITY_EDITOR
        UnityEditor.SceneView.RepaintAll();
#endif
    }
    private void Update()
    {
        UpdateBeautifyReference();
        ApplySettings();
    }
    /// <summary>
    /// 指定された Volume Profile から Beautify コンポーネントを取得
    /// </summary>
    void UpdateBeautifyReference()
    {
        if (postProcessVolume != null && postProcessVolume.profile != null)
        {
            if (!postProcessVolume.profile.TryGet<BU>(out beautify))
            {
                Debug.LogWarning("指定された Volume Profile に Beautify エフェクトが見つかりません。");
            }
        }
        else
        {
            beautify = null;
        }
    }

    /// <summary>
    /// インスペクター上の各値を Beautify のパラメータに反映
    /// </summary>
    void ApplySettings()
    {
        if (beautify == null)
        {
            return;
        }
        // 各パラメータの override 状態を有効にする
        beautify.frame.overrideState = true;
        beautify.frameStyle.overrideState = true;
        beautify.frameBandHorizontalSize.overrideState = true;
        beautify.frameBandHorizontalSmoothness.overrideState = true;
        beautify.frameBandVerticalSize.overrideState = true;
        beautify.frameBandVerticalSmoothness.overrideState = true;
        beautify.frameColor.overrideState = true;
        beautify.frameMask.overrideState = true;

        beautify.frame.value = frame;
        beautify.frameStyle.value = frameStyle;
        beautify.frameBandHorizontalSize.value = frameBandHorizontalSize;
        beautify.frameBandHorizontalSmoothness.value = frameBandHorizontalSmoothness;
        beautify.frameBandVerticalSize.value = frameBandVerticalSize;
        beautify.frameBandVerticalSmoothness.value = frameBandVerticalSmoothness;
        beautify.frameColor.value = frameColor;
        beautify.frameMask.value = frameMask;


        beautify.lut.overrideState = true;
        beautify.lutIntensity.overrideState = true;
        beautify.lutTexture.overrideState = true;

        beautify.lut.value = enableLUT;
        beautify.lutIntensity.value = lutIntensity;
        beautify.lutTexture.value = lutTexture;
    }
}
