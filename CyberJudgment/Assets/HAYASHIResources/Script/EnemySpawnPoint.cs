using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using AbubuResouse.Singleton;
using AbubuResouse.Log;
using uPools;

namespace AbubuResouse.Spawn
{
    /// <summary>
    /// 敵のスポーンポイント管理クラス
    /// </summary>
    public class EnemySpawnPoint : MonoBehaviour
    {
        [Header("スポーンデータ")]
        [SerializeField] private EnemySpawnData spawnData;

        [Header("スポーン状態")]
        [SerializeField] private bool isActive = true;
        [SerializeField] private bool showGizmos = true;

        [Header("デバッグ情報")]
        [SerializeField] private bool showDebugInfo = false;

        // 内部管理用
        private Dictionary<EnemySpawnData.EnemySpawnInfo, int> currentSpawnCounts = new Dictionary<EnemySpawnData.EnemySpawnInfo, int>();
        private Dictionary<EnemySpawnData.EnemySpawnInfo, float> lastSpawnTimes = new Dictionary<EnemySpawnData.EnemySpawnInfo, float>();
        private Dictionary<GameObject, EnemySpawnData.EnemySpawnInfo> spawnedEnemyInfoMap = new Dictionary<GameObject, EnemySpawnData.EnemySpawnInfo>();
        private List<GameObject> spawnedEnemies = new List<GameObject>();
        private Coroutine spawnCoroutine;

        // イベント
        public System.Action<GameObject> OnEnemySpawned;
        public System.Action<GameObject> OnEnemyDestroyed;

        private void Start()
        {
            InitializeSpawnData();
            if (isActive)
            {
                StartSpawning();
            }
        }

        private void OnDestroy()
        {
            StopSpawning();
            CleanupSpawnedEnemies();
        }

        private void Update()
        {
            // 破壊された敵を検出してカウントを更新
            CheckForDestroyedEnemies();
        }

        /// <summary>
        /// スポーンデータの初期化
        /// </summary>
        private void InitializeSpawnData()
        {
            if (spawnData == null)
            {
                DebugUtility.LogError($"SpawnData が設定されていません: {gameObject.name}");
                return;
            }

            currentSpawnCounts.Clear();
            lastSpawnTimes.Clear();
            spawnedEnemyInfoMap.Clear();

            foreach (var enemyInfo in spawnData.enemySpawnInfos)
            {
                if (enemyInfo.enemyPrefab != null)
                {
                    currentSpawnCounts[enemyInfo] = 0;
                    lastSpawnTimes[enemyInfo] = -enemyInfo.spawnCoolTime; // 即座にスポーン可能にする
                }
            }

            DebugUtility.Log($"スポーンポイント初期化完了: {gameObject.name}");
        }

        /// <summary>
        /// スポーン開始
        /// </summary>
        public void StartSpawning()
        {
            if (!isActive || spawnData == null) return;

            StopSpawning();
            spawnCoroutine = StartCoroutine(SpawnRoutine());
            DebugUtility.Log($"スポーン開始: {gameObject.name}");
        }

        /// <summary>
        /// スポーン停止
        /// </summary>
        public void StopSpawning()
        {
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }
        }

        /// <summary>
        /// スポーンルーチン
        /// </summary>
        private IEnumerator SpawnRoutine()
        {
            while (isActive)
            {
                yield return new WaitForSeconds(spawnData.baseSpawnInterval);

                TrySpawnEnemy();
            }
        }

        /// <summary>
        /// 敵のスポーンを試行
        /// </summary>
        private void TrySpawnEnemy()
        {
            if (spawnData == null || spawnData.enemySpawnInfos.Count == 0) return;

            // スポーン可能な敵をフィルタリング
            var availableEnemies = spawnData.enemySpawnInfos.Where(CanSpawnEnemy).ToList();

            if (availableEnemies.Count == 0) return;

            // スポーンする敵を選択
            EnemySpawnData.EnemySpawnInfo selectedEnemy;
            if (spawnData.useRandomSpawnOrder)
            {
                selectedEnemy = availableEnemies[Random.Range(0, availableEnemies.Count)];
            }
            else
            {
                selectedEnemy = availableEnemies[0];
            }

            // 確率チェック
            if (Random.value > selectedEnemy.spawnProbability) return;

            SpawnEnemyAsync(selectedEnemy).Forget();
        }

        /// <summary>
        /// 指定された敵がスポーン可能かチェック
        /// </summary>
        private bool CanSpawnEnemy(EnemySpawnData.EnemySpawnInfo enemyInfo)
        {
            if (enemyInfo.enemyPrefab == null) return false;

            // 最大数チェック
            if (currentSpawnCounts[enemyInfo] >= enemyInfo.maxSpawnCount) return false;

            // クールタイムチェック
            if (Time.time - lastSpawnTimes[enemyInfo] < enemyInfo.spawnCoolTime) return false;

            return true;
        }

        /// <summary>
        /// 敵を非同期でスポーン
        /// </summary>
        private async UniTaskVoid SpawnEnemyAsync(EnemySpawnData.EnemySpawnInfo enemyInfo)
        {
            try
            {
                // スポーン位置を決定
                Vector3 spawnPosition = GetRandomSpawnPosition();

                if (spawnPosition == Vector3.zero)
                {
                    DebugUtility.LogWarning($"有効なスポーン位置が見つかりません: {gameObject.name}");
                    return;
                }

                // 敵をスポーン
                GameObject enemyInstance = SharedGameObjectPool.Rent(
                    enemyInfo.enemyPrefab,
                    spawnPosition + Vector3.up * spawnData.spawnHeight,
                    Quaternion.identity
                );

                if (enemyInstance == null)
                {
                    DebugUtility.LogError($"敵のスポーンに失敗: {enemyInfo.enemyName}");
                    return;
                }

                // スポーンカウントを更新
                currentSpawnCounts[enemyInfo]++;
                lastSpawnTimes[enemyInfo] = Time.time;
                spawnedEnemies.Add(enemyInstance);
                spawnedEnemyInfoMap[enemyInstance] = enemyInfo;

                // エフェクトを再生
                if (enemyInfo.spawnEffect != null)
                {
                    EffectManager.Instance.PlayEffect(
                        enemyInfo.spawnEffect,
                        spawnPosition,
                        Quaternion.identity,
                        2f
                    );
                }

                // SEを再生
                if (!string.IsNullOrEmpty(enemyInfo.spawnSEName))
                {
                    SEManager.Instance.PlaySound(enemyInfo.spawnSEName, 0.7f);
                }

                // 敵の初期化（既存のEnemyBaseに対応）
                var enemyComponent = enemyInstance.GetComponent<EnemyBase>();
                if (enemyComponent != null)
                {
                    // 既存のHP初期化処理と重複しないよう、必要に応じて値を設定
                    enemyComponent._currentHealth = enemyComponent._currentHealth; // 既存の初期化を保持
                }

                // スポーンアニメーションを実行
                await PlaySpawnAnimation(enemyInstance, enemyInfo);

                // イベント発火
                OnEnemySpawned?.Invoke(enemyInstance);

                if (showDebugInfo)
                {
                    DebugUtility.Log($"敵をスポーン: {enemyInfo.enemyName} at {spawnPosition}");
                }
            }
            catch (System.Exception e)
            {
                DebugUtility.LogError($"スポーン処理でエラーが発生: {e.Message}");
            }
        }

        /// <summary>
        /// ランダムなスポーン位置を取得
        /// </summary>
        private Vector3 GetRandomSpawnPosition()
        {
            const int maxAttempts = 10;

            for (int i = 0; i < maxAttempts; i++)
            {
                // 円形の範囲内でランダム位置を生成
                Vector2 randomPoint = Random.insideUnitCircle * spawnData.spawnRadius;
                Vector3 testPosition = transform.position + new Vector3(randomPoint.x, 0, randomPoint.y);

                // 地面との接地点を探す
                if (Physics.Raycast(testPosition + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f, spawnData.groundLayerMask))
                {
                    return hit.point;
                }
            }

            // 見つからない場合はスポーンポイントの位置を返す
            return transform.position;
        }

        /// <summary>
        /// スポーンアニメーションを再生
        /// </summary>
        private async UniTask PlaySpawnAnimation(GameObject enemyInstance, EnemySpawnData.EnemySpawnInfo enemyInfo)
        {
            if (enemyInstance == null) return;

            Transform enemyTransform = enemyInstance.transform;
            Vector3 originalScale = enemyTransform.localScale;

            // 初期スケールを設定
            enemyTransform.localScale = originalScale * enemyInfo.initialScale;

            float elapsedTime = 0f;

            while (elapsedTime < enemyInfo.spawnAnimationDuration)
            {
                if (enemyInstance == null) return; // 途中で破壊された場合

                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / enemyInfo.spawnAnimationDuration;
                float curveValue = enemyInfo.scaleCurve.Evaluate(progress);

                float currentScale = Mathf.Lerp(enemyInfo.initialScale, enemyInfo.finalScale, curveValue);
                enemyTransform.localScale = originalScale * currentScale;

                await UniTask.Yield();
            }

            // 最終スケールを確定
            if (enemyInstance != null)
            {
                enemyTransform.localScale = originalScale * enemyInfo.finalScale;
            }
        }

        /// <summary>
        /// 破壊された敵をチェックしてカウントを更新
        /// </summary>
        private void CheckForDestroyedEnemies()
        {
            var enemiesToRemove = new List<GameObject>();

            foreach (var enemy in spawnedEnemies.ToList())
            {
                if (enemy == null) // オブジェクトが破壊された
                {
                    enemiesToRemove.Add(enemy);
                }
            }

            foreach (var destroyedEnemy in enemiesToRemove)
            {
                OnEnemyDestroyedCallback(destroyedEnemy);
            }
        }

        /// <summary>
        /// 敵が破壊された時のコールバック
        /// </summary>
        private void OnEnemyDestroyedCallback(GameObject enemyInstance)
        {
            if (spawnedEnemyInfoMap.TryGetValue(enemyInstance, out var enemyInfo))
            {
                if (currentSpawnCounts.ContainsKey(enemyInfo))
                {
                    currentSpawnCounts[enemyInfo] = Mathf.Max(0, currentSpawnCounts[enemyInfo] - 1);
                }

                spawnedEnemyInfoMap.Remove(enemyInstance);

                if (showDebugInfo)
                {
                    DebugUtility.Log($"敵が破壊されました: {enemyInfo.enemyName}");
                }
            }

            spawnedEnemies.Remove(enemyInstance);
            OnEnemyDestroyed?.Invoke(enemyInstance);
        }

        /// <summary>
        /// すべてのスポーンされた敵をクリーンアップ
        /// </summary>
        private void CleanupSpawnedEnemies()
        {
            foreach (var enemy in spawnedEnemies.ToList())
            {
                if (enemy != null)
                {
                    SharedGameObjectPool.Return(enemy);
                }
            }
            spawnedEnemies.Clear();
            spawnedEnemyInfoMap.Clear();
        }

        /// <summary>
        /// 設定変更
        /// </summary>
        public void SetActive(bool active)
        {
            isActive = active;
            if (active)
            {
                StartSpawning();
            }
            else
            {
                StopSpawning();
            }
        }

        public void SetSpawnData(EnemySpawnData newSpawnData)
        {
            spawnData = newSpawnData;
            InitializeSpawnData();
        }

        /// <summary>
        /// 現在のスポーン状況を取得
        /// </summary>
        public Dictionary<string, int> GetCurrentSpawnStatus()
        {
            var status = new Dictionary<string, int>();
            foreach (var kvp in currentSpawnCounts)
            {
                status[kvp.Key.enemyName] = kvp.Value;
            }
            return status;
        }

        /// <summary>
        /// 手動で敵を破壊通知する（デバッグ用）
        /// </summary>
        public void ManuallyNotifyEnemyDestroyed(GameObject enemy)
        {
            OnEnemyDestroyedCallback(enemy);
        }

        // Gizmos描画
        private void OnDrawGizmos()
        {
            if (!showGizmos || spawnData == null) return;

            // スポーン範囲を描画
            Gizmos.color = isActive ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, spawnData.spawnRadius);

            // スポーン高さを描画
            Gizmos.color = Color.yellow;
            Vector3 heightPos = transform.position + Vector3.up * spawnData.spawnHeight;
            Gizmos.DrawWireCube(heightPos, Vector3.one * 0.2f);
        }

        private void OnDrawGizmosSelected()
        {
            if (spawnData == null) return;

            // より詳細な情報を表示
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, spawnData.spawnRadius * 0.5f);

            // スポーンポイント名を表示
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up, gameObject.name);
#endif
        }
    }
}