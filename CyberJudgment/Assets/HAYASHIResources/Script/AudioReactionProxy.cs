using UnityEngine;

namespace TelePresent.AudioSyncPro
{
    public class AudioReactionProxy : MonoBehaviour, ASP_IAudioReaction
    {
        [Tooltip("実際に反応させたい BlendShapeLipSync コンポーネント")]
        public BlendShapeLipSync externalReaction;

        public bool IsActive
        {
            get => externalReaction != null && externalReaction.IsActive;
            set { if (externalReaction != null) externalReaction.IsActive = value; }
        }

        public void Initialize(Vector3 pos, Vector3 scale, Quaternion rot)
        {
            externalReaction?.Initialize(pos, scale, rot);
        }

        public void React(AudioSourcePlus src, Transform target, float rmsValue, float[] spectrum)
        {
            externalReaction?.React(src, target, rmsValue, spectrum);
        }

        public void ResetToOriginalState(Transform target)
        {
            externalReaction?.ResetToOriginalState(target);
        }
    }
}
