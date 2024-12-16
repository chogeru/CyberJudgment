using Cysharp.Threading.Tasks;
using UnityEngine;
using uPools;
using AbubuResouse.Log;

namespace AbubuResouse.Singleton
{
    public class EffectManager : SingletonMonoBehaviour<EffectManager>
    {

        /// <summary>
        /// 指定したエフェクトプレハブを再生
        /// </summary>
        /// <param name="effectPrefab">エフェクトプレハブ</param>
        /// <param name="position">再生位置</param>
        /// <param name="rotation">再生回転</param>
        /// <param name="delay">エフェクト返却までの遅延時間</param>
        public void PlayEffect(GameObject effectPrefab, Vector3 position, Quaternion rotation, float delay = 1.0f)
        {
            if (effectPrefab == null)
            {
                DebugUtility.LogError("エフェクトプレハブがnullです！");
                return;
            }

            GameObject effectInstance = SharedGameObjectPool.Rent(effectPrefab, position, rotation);

            if (effectInstance == null)
            {
                DebugUtility.LogError($"エフェクトの生成に失敗!:名前＝＞{effectPrefab.name}");
                return;
            }
         
            ReturnEffectAfterDelay(effectInstance, delay).Forget();
        }

        /// <summary>
        /// エフェクトを指定された時間後に返却する
        /// </summary>
        /// <param name="effect">返却するエフェクト</param>
        /// <param name="delay">返却までの遅延時間</param>
        private async UniTaskVoid ReturnEffectAfterDelay(GameObject effect, float delay)
        {
            await UniTask.Delay((int)(delay * 1000));
            SharedGameObjectPool.Return(effect);
        }
    }
}
