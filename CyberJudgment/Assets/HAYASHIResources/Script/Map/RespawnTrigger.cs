using UnityEngine;

public class RespawnTrigger : MonoBehaviour
{
    [Header("RespawnManagerの参照")]
    [SerializeField]
    private RespawnManager respawnManager;

    private void OnTriggerEnter(Collider other)
    {
        // プレイヤーと衝突した場合にリスポーンシーケンスを開始
        if (respawnManager != null && other.CompareTag("Player"))
        {
            Debug.Log("リスポーントリガーが発動しました。");
            respawnManager.StartRespawnSequence();
        }
    }
}
