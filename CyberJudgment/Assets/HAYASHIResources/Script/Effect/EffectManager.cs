using Cysharp.Threading.Tasks;
using UnityEngine;
using uPools;
using AbubuResouse.Log;

namespace AbubuResouse.Singleton
{
    public class EffectManager : SingletonMonoBehaviour<EffectManager>
    {

        /// <summary>
        /// �w�肵���G�t�F�N�g�v���n�u���Đ�
        /// </summary>
        /// <param name="effectPrefab">�G�t�F�N�g�v���n�u</param>
        /// <param name="position">�Đ��ʒu</param>
        /// <param name="rotation">�Đ���]</param>
        /// <param name="delay">�G�t�F�N�g�ԋp�܂ł̒x������</param>
        public void PlayEffect(GameObject effectPrefab, Vector3 position, Quaternion rotation, float delay = 1.0f)
        {
            if (effectPrefab == null)
            {
                DebugUtility.LogError("�G�t�F�N�g�v���n�u��null�ł��I");
                return;
            }

            GameObject effectInstance = SharedGameObjectPool.Rent(effectPrefab, position, rotation);

            if (effectInstance == null)
            {
                DebugUtility.LogError($"�G�t�F�N�g�̐����Ɏ��s!:���O����{effectPrefab.name}");
                return;
            }
         
            ReturnEffectAfterDelay(effectInstance, delay).Forget();
        }

        /// <summary>
        /// �G�t�F�N�g���w�肳�ꂽ���Ԍ�ɕԋp����
        /// </summary>
        /// <param name="effect">�ԋp����G�t�F�N�g</param>
        /// <param name="delay">�ԋp�܂ł̒x������</param>
        private async UniTaskVoid ReturnEffectAfterDelay(GameObject effect, float delay)
        {
            await UniTask.Delay((int)(delay * 1000));
            SharedGameObjectPool.Return(effect);
        }
    }
}
