using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace TelePresent.AudioSyncPro
{
    [AddComponentMenu("GameObject/")]
    [ASP_ReactorCategory("Post Processing")]
    public class ASPR_PostProcessingWeightWithVolume : MonoBehaviour, ASP_IAudioReaction
    {
        public new string name = "Post Processing Weight On Volume!";
        public string info = "This Component modifies the weight of a Post Processing effect based on Audio Volume.";

        [SerializeField]
        private PostProcessVolume targetVolume;

        [SerializeField] private float weightMultiplier = 2f;

        [ASP_FloatSlider(0.0f, 1f)]
        [SerializeField] private float smoothness = .25f; // Displayed as 0 to 1

        [ASP_FloatSlider(0.0f, 15f)]
        [SerializeField] public float sensitivity = 1.0f;
        [ASP_MinMaxSlider(0f, 1f)]
        [SerializeField] private Vector3 volumeRange = new Vector3(0f, 0.3f, 0.7f); // Volume range with min and max limits

        private float initialWeight;
        private bool isInitialized = false;

        [HideInInspector]
        [SerializeField] private bool isActive = true;

        public bool IsActive
        {
            get => isActive;
            set => isActive = value;
        }
        public void Initialize(Vector3 initialPosition, Vector3 initialScale, Quaternion initialRotation)
        {
            if (targetVolume != null)
            {
                initialWeight = targetVolume.weight;

                if (initialWeight == 0f)
                {
                    initialWeight = 0.1f;
                    targetVolume.weight = initialWeight;
                }

                isInitialized = true;
            }
        }

        public void React(AudioSourcePlus audioSourcePlus, Transform targetTransform, float rmsValue, float[] spectrumData)
        {
            if (!isInitialized || !IsActive || targetVolume == null) return;

            float volume = rmsValue * sensitivity;

            volumeRange.x = Mathf.Lerp(volumeRange.x, volume, Time.deltaTime * (1.0f / Mathf.Clamp(smoothness, 0.01f, 10.0f)));

            if (volumeRange.x < volumeRange.y)
            {
                return;
            }

            float mappedWeight = Mathf.Lerp(initialWeight, initialWeight * weightMultiplier, Mathf.InverseLerp(volumeRange.y, volumeRange.z, volumeRange.x));
            mappedWeight = Mathf.Clamp(mappedWeight, 0f, 1f);

            targetVolume.weight = mappedWeight;
        }

        public void ResetToOriginalState(Transform targetTransform)
        {
            if (!isInitialized || targetVolume == null) return;

            targetVolume.weight = initialWeight;
            volumeRange.x = 0f;
        }
    }
}
