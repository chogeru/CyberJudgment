// BlendShapeLipSync.cs
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;

namespace TelePresent.AudioSyncPro
{
    public class BlendShapeLipSync : MonoBehaviour, ASP_IAudioReaction
    {
        [Title("Blend Shapes Settings")]
        [BoxGroup("BlendShapes")]
        [LabelText("Viseme A Shape")]
        public string blendA = "Viseme_A";
        [BoxGroup("BlendShapes")]
        [LabelText("Viseme I Shape")]
        public string blendI = "Viseme_I";
        [BoxGroup("BlendShapes")]
        [LabelText("Viseme U Shape")]
        public string blendU = "Viseme_U";

        [PropertySpace(10)]
        [Title("RMS Smoothing Settings")]
        [BoxGroup("RMS Smoothing")]
        [LabelText("Smooth Speed")]
        [Tooltip("平滑化の速さ（大きいほど素早く追従）")]
        [MinValue(0.1f)]
        public float rmsSmoothSpeed = 5f;

        [BoxGroup("RMS Smoothing")]
        [LabelText("RMS to Weight Multiplier")]
        [Tooltip("RMS を口開き用に増幅する倍率")]
        [MinValue(1f)]
        public float rmsToWeight = 500f;

        [PropertySpace(10)]
        [Title("Spectral Bands Settings")]
        [BoxGroup("Spectral Ranges")]
        [LabelText("Low Band Range")]
        [Tooltip("低域インデックス範囲 (例: 0–10)")]
        [MinMaxSlider(0, 1023, ShowFields = true)]
        public Vector2Int lowRange = new Vector2Int(0, 10);

        [BoxGroup("Spectral Ranges")]
        [LabelText("Mid Band Range")]
        [Tooltip("中域インデックス範囲 (例: 11–50)")]
        [MinMaxSlider(0, 1023, ShowFields = true)]
        public Vector2Int midRange = new Vector2Int(11, 50);

        [BoxGroup("Spectral Ranges")]
        [LabelText("High Band Range")]
        [Tooltip("高域インデックス範囲 (例: 51–)")]
        [MinMaxSlider(0, 1023, ShowFields = true)]
        public Vector2Int highRange = new Vector2Int(51, 1023);

        [PropertySpace(10)]
        [BoxGroup("Debug")]
        [ReadOnly]
        [LabelText("Smoothed RMS Value")]
        public float smoothedRMS;

        [SerializeField, Tooltip("口パクさせたい SkinnedMeshRenderer をここにセット（未設定時は自身のものを探します）")]
        private SkinnedMeshRenderer targetRenderer;

        private SkinnedMeshRenderer smrInstance;
        private int idxA, idxI, idxU;

        [HideInEditorMode]
        public bool IsActive { get; set; } = true;

        public void Initialize(Vector3 pos, Vector3 scale, Quaternion rot)
        {
            smrInstance = targetRenderer != null
                ? targetRenderer
                : GetComponent<SkinnedMeshRenderer>();
            if (smrInstance == null)
            {
                Debug.LogError($"[{nameof(BlendShapeLipSync)}] に SkinnedMeshRenderer がありません: {gameObject.name}", this);
                return;
            }

            var mesh = smrInstance.sharedMesh;
            idxA = mesh.GetBlendShapeIndex(blendA);
            idxI = mesh.GetBlendShapeIndex(blendI);
            idxU = mesh.GetBlendShapeIndex(blendU);
        }

        [Button("React Now")]
        public void ReactNow()
        {
            if (Application.isPlaying && smrInstance != null)
                React(null, transform, smoothedRMS, new float[1024]);
        }

        public void React(AudioSourcePlus src, Transform target, float rmsValue, float[] spectrum)
        {
            if (!IsActive || smrInstance == null) return;

            smoothedRMS = Mathf.Lerp(smoothedRMS, rmsValue, rmsSmoothSpeed * Time.deltaTime);
            float mouthWeight = Mathf.Clamp01(smoothedRMS * (rmsToWeight / 100f));

            float low  = spectrum.Skip(lowRange.x).Take(lowRange.y - lowRange.x + 1).Sum();
            float mid  = spectrum.Skip(midRange.x).Take(midRange.y - midRange.x + 1).Sum();
            float high = spectrum.Skip(highRange.x).Take(spectrum.Length - highRange.x).Sum();
            float total = low + mid + high + 1e-6f;
            low /= total; mid /= total; high /= total;

            float weightA = (low > mid && low > high)   ? 100f : 0f;
            float weightI = (high > low && high > mid) ? 100f : 0f;
            float weightU = (mid > low && mid > high)  ? 100f : 0f;

            if (idxA >= 0) smrInstance.SetBlendShapeWeight(idxA, weightA * mouthWeight);
            if (idxI >= 0) smrInstance.SetBlendShapeWeight(idxI, weightI * mouthWeight);
            if (idxU >= 0) smrInstance.SetBlendShapeWeight(idxU, weightU * mouthWeight);
        }

        public void ResetToOriginalState(Transform target)
        {
            if (smrInstance == null) return;
            if (idxA >= 0) smrInstance.SetBlendShapeWeight(idxA, 0f);
            if (idxI >= 0) smrInstance.SetBlendShapeWeight(idxI, 0f);
            if (idxU >= 0) smrInstance.SetBlendShapeWeight(idxU, 0f);
            smoothedRMS = 0f;
        }
    }
}
