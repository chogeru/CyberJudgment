using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    // 追従対象のターゲット
    public Transform target;

    // オフセット (ターゲットとの相対位置)
    public Vector3 offset = Vector3.zero;

    // 追従速度
    public float followSpeed = 5f;

    void Update()
    {
        if (target == null)
        {
            Debug.LogWarning("ターゲットが設定されていません！");
            return;
        }

        // ターゲットの位置 + オフセット
        Vector3 targetPosition = target.position + offset;

        // 現在位置からターゲット位置にスムーズに移動
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }
}
