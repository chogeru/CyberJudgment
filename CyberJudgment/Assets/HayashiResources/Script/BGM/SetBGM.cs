using AbubuResouse.Singleton;
using UnityEngine;
using VInspector;  // [Tab] 属性用

namespace AbubuResouse
{
    /// <summary>
    /// BGMセット用クラス
    /// インスペクタからBGMやフィルタ設定を指定し、
    /// シーン開始時やランダムでBGMを再生する
    /// </summary>
    public class SetBGM : MonoBehaviour
    {
        //======================================================================
        // BGM 設定
        //======================================================================
        [Tab("BGM設定")]
        [Header("開始時に再生するBGM名")]
        [SerializeField] private string m_BGMName;

        [Header("BGM音量 (0.0~1.0)")]
        [SerializeField] private float m_Volume = 1.0f;

        //======================================================================
        // ランダムBGM 設定
        //======================================================================
        [Tab("ランダムBGM設定")]
        [Header("ランダムBGMをオンにするか")]
        [SerializeField] private bool isRandomBGM = false;

        [Header("ランダム再生候補のBGM名リスト")]
        [SerializeField] private string[] m_RandomBGMNames;

        //======================================================================
        // Chorus フィルタ設定
        //======================================================================
        [Tab("Chorusフィルタ設定")]
        [Header("Chorusフィルタを有効にするか")]
        [SerializeField] private bool useChorus = false;

        [Header("原音ドライミックス(0~1)")]
        [Range(0f, 1f)]
        [SerializeField] private float chorusDryMix = 0.5f;

        [Header("コーラス音1の音量(0~1)")]
        [Range(0f, 1f)]
        [SerializeField] private float chorusWetMix1 = 0.5f;

        [Header("コーラス音2の音量(0~1)")]
        [Range(0f, 1f)]
        [SerializeField] private float chorusWetMix2 = 0.5f;

        [Header("遅延時間(ミリ秒)")]
        [Range(0f, 100f)]
        [SerializeField] private float chorusDelay = 40f;

        [Header("モジュレーション速度(例: 0.1~2.0)")]
        [SerializeField] private float chorusRate = 0.8f;

        [Header("モジュレーションの深さ(0~1推奨)")]
        [SerializeField] private float chorusDepth = 0.03f;

        [Header("フィードバック量(0~1推奨)")]
        [SerializeField] private float chorusFeedback = 0.0f;

        //======================================================================
        // Distortion フィルタ設定
        //======================================================================
        [Tab("Distortionフィルタ設定")]
        [Header("Distortionフィルタを有効にするか")]
        [SerializeField] private bool useDistortion = false;

        [Header("歪みの強さ(0~1)")]
        [Range(0f, 1f)]
        [SerializeField] private float distortionLevel = 0.5f;

        //======================================================================
        // Echo フィルタ設定
        //======================================================================
        [Tab("Echoフィルタ設定")]
        [Header("Echoフィルタを有効にするか")]
        [SerializeField] private bool useEcho = false;

        [Header("ディレイ(ミリ秒) [10~5000]")]
        [Range(10f, 5000f)]
        [SerializeField] private float echoDelay = 500f;

        [Header("減衰率(0~1)")]
        [Range(0f, 1f)]
        [SerializeField] private float echoDecayRatio = 0.5f;

        [Header("原音レベル(0~1)")]
        [Range(0f, 1f)]
        [SerializeField] private float echoDryMix = 1.0f;

        [Header("エコーレベル(0~1)")]
        [Range(0f, 1f)]
        [SerializeField] private float echoWetMix = 1.0f;

        //======================================================================
        // Reverb フィルタ設定
        //======================================================================
        [Tab("Reverbフィルタ設定")]
        [Header("Reverbフィルタを有効にするか")]
        [SerializeField] private bool useReverb = false;

        [Header("リバーブのプリセット")]
        [SerializeField] private AudioReverbPreset reverbPreset = AudioReverbPreset.Cave;

        //======================================================================
        // LowPass フィルタ設定
        //======================================================================
        [Tab("LowPassフィルタ設定")]
        [Header("LowPassフィルタを有効にするか")]
        [SerializeField] private bool useLowPass = false;

        [Header("カットオフ周波数(10~22000)")]
        [Range(10f, 22000f)]
        [SerializeField] private float lowPassCutoff = 5000f;

        [Header("共鳴Q値(1~10推奨)")]
        [SerializeField] private float lowPassResonanceQ = 1.0f;

        //======================================================================
        // HighPass フィルタ設定
        //======================================================================
        [Tab("HighPassフィルタ設定")]
        [Header("HighPassフィルタを有効にするか")]
        [SerializeField] private bool useHighPass = false;

        [Header("カットオフ周波数(10~22000)")]
        [Range(10f, 22000f)]
        [SerializeField] private float highPassCutoff = 500f;

        [Header("共鳴Q値(1~10推奨)")]
        [SerializeField] private float highPassResonanceQ = 1.0f;


        //======================================================================
        // 実行時処理
        //======================================================================
        private void Start()
        {
            Initialization();
            SetupFilters();
        }

        private void Update()
        {
            if (isRandomBGM)
            {
                RandomBGM();
            }
        }

        [ContextMenu("ResetBGM")]
        private void TestBGM()
        {
            Initialization();
            SetupFilters();
        }

        /// <summary>初期設定 (BGM再生開始など)</summary>
        private void Initialization()
        {
            if (BGMManager.Instance != null)
            {
                // いったん再生中クリップをクリア
                var audioSrc = BGMManager.Instance.GetComponent<AudioSource>();
                if (audioSrc != null) audioSrc.clip = null;

                // 指定のBGM名があれば再生
                if (!string.IsNullOrEmpty(m_BGMName))
                {
                    BGMManager.Instance.PlaySound(m_BGMName, m_Volume);
                }
            }
        }

        /// <summary>ランダムBGMの再生処理</summary>
        private void RandomBGM()
        {
            if (BGMManager.Instance == null) return;

            var audioSource = BGMManager.Instance.GetComponent<AudioSource>();
            if (audioSource == null) return;

            audioSource.loop = false; // 曲切り替えのためループオフ

            // 再生が終わったタイミングでランダムなBGMを再生
            if (!audioSource.isPlaying && m_RandomBGMNames.Length > 0)
            {
                var index = Random.Range(0, m_RandomBGMNames.Length);
                BGMManager.Instance.PlaySound(m_RandomBGMNames[index], m_Volume);
            }
        }

        /// <summary>
        /// インスペクタで設定したフィルタパラメータを BGMManager (AudioFilterManager) に適用
        /// </summary>
        private void SetupFilters()
        {
            if (BGMManager.Instance == null) return;
            var filterManager = BGMManager.Instance.FilterManager;
            if (filterManager == null) return;

            //--- 1) Chorus ---//
            var chorus = filterManager.GetFilter<ChorusFilterController>();
            if (chorus != null)
            {
                chorus.SetEnable(useChorus);
                if (useChorus)
                {
                    chorus.SetParameters(
                        dryMix: chorusDryMix,
                        wetMix1: chorusWetMix1,
                        wetMix2: chorusWetMix2,
                        delay: chorusDelay,
                        rate: chorusRate,
                        depth: chorusDepth,
                        feedback: chorusFeedback
                    );
                }
            }

            //--- 2) Distortion ---//
            var distortion = filterManager.GetFilter<DistortionFilterController>();
            if (distortion != null)
            {
                distortion.SetEnable(useDistortion);
                if (useDistortion)
                {
                    distortion.SetParameters(distortionLevel);
                }
            }

            //--- 3) Echo ---//
            var echo = filterManager.GetFilter<EchoFilterController>();
            if (echo != null)
            {
                echo.SetEnable(useEcho);
                if (useEcho)
                {
                    echo.SetParameters(
                        delay: echoDelay,
                        decayRatio: echoDecayRatio,
                        dryMix: echoDryMix,
                        wetMix: echoWetMix
                    );
                }
            }

            //--- 4) Reverb ---//
            var reverb = filterManager.GetFilter<ReverbFilterController>();
            if (reverb != null)
            {
                reverb.SetEnable(useReverb);
                if (useReverb)
                {
                    reverb.SetPreset(reverbPreset);
                }
            }

            //--- 5) LowPass ---//
            var lowPass = filterManager.GetFilter<LowPassFilterController>();
            if (lowPass != null)
            {
                lowPass.SetEnable(useLowPass);
                if (useLowPass)
                {
                    lowPass.SetParameters(
                        cutoffFrequency: lowPassCutoff,
                        resonanceQ: lowPassResonanceQ
                    );
                }
            }

            //--- 6) HighPass ---//
            var highPass = filterManager.GetFilter<HighPassFilterController>();
            if (highPass != null)
            {
                highPass.SetEnable(useHighPass);
                if (useHighPass)
                {
                    highPass.SetParameters(
                        cutoffFrequency: highPassCutoff,
                        resonanceQ: highPassResonanceQ
                    );
                }
            }
        }
    }
}
