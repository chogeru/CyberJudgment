using UnityEngine;

public class AerialMovement : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 5f;         // 移動速度
    public float rollAngle = 15f;        // 左右入力での最大ロール角度
    public float pitchAngle = 15f;       // 上下入力での最大ピッチ角度

    [Header("視界制限")]
    public Vector2 viewportPadding = new Vector2(0.1f, 0.1f);  // カメラ視界内に収めるためのパディング

    [Header("回転スムーズ化")]
    public float rotationSmoothTime = 0.1f; // 回転をスムーズに戻す時間（大きいほどゆったり）

    [Header("参照")]
    public Camera mainCamera;          // メインカメラの参照（Inspectorでセット）
    public Transform childTransform;   // 動かしたい子オブジェクト（Inspectorでセット）

    // ロール・ピッチをスムーズに変化させるための補助変数
    private float rollVelocity = 0f;   // Mathf.SmoothDampAngle用
    private float pitchVelocity = 0f;  // Mathf.SmoothDampAngle用

    void Update()
    {
        // ----------------------------------
        // 1) 入力取得 (左右: Horizontal, 上下: Vertical)
        // ----------------------------------
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // ----------------------------------
        // 2) ローカル座標ベースでの移動
        //    （親オブジェクトの回転に影響されにくくし、Z=0を固定）
        // ----------------------------------
        Vector3 localPos = childTransform.localPosition;

        // ローカルでの移動方向(XY平面のみ)
        Vector3 localMoveDir = new Vector3(h, v, 0f);

        // 入力があるときだけ正規化
        if (localMoveDir.sqrMagnitude > 0.001f)
        {
            localMoveDir.Normalize();
        }

        // 移動処理
        localPos += localMoveDir * moveSpeed * Time.deltaTime;
        childTransform.localPosition = localPos;

        // ----------------------------------
        // 3) カメラのViewport内に収める
        //    World→Viewport→World に変換後、親のローカル座標に戻す
        // ----------------------------------
        Vector3 worldPos = childTransform.position;
        Vector3 vpPos = mainCamera.WorldToViewportPoint(worldPos);

        // ビューポート座標でClamp
        vpPos.x = Mathf.Clamp(vpPos.x, viewportPadding.x, 1f - viewportPadding.x);
        vpPos.y = Mathf.Clamp(vpPos.y, viewportPadding.y, 1f - viewportPadding.y);

        // ビューポート→ワールドに戻す
        worldPos = mainCamera.ViewportToWorldPoint(vpPos);

        // 親のローカル座標に戻してZ=0固定
        Vector3 parentLocalPos = transform.InverseTransformPoint(worldPos);
        parentLocalPos.z = 0f;

        childTransform.localPosition = parentLocalPos;

        // ----------------------------------
        // 4) 回転(ロール＆ピッチ)のスムーズ補間
        // ----------------------------------
        // ■ ロール(左右入力) = -rollAngle * h
        //   (右入力 → ロールを負角度方向、左入力 → 正角度方向)
        // ■ ピッチ(上下入力) = -pitchAngle * v
        //   (上入力 → 機首が上がる, 下入力 → 機首が下がる)
        //   ※符号は好みで調整してください
        float targetRoll = -rollAngle * h;
        float targetPitch = -pitchAngle * v;

        // 現在のローカル回転を -180~+180 で取得しつつ、
        // roll(Z軸)・pitch(X軸) それぞれをSmoothDampAngle
        Vector3 currentEuler = childTransform.localEulerAngles;

        // Z軸(ロール)
        float currentRoll = (currentEuler.z > 180f) ? currentEuler.z - 360f : currentEuler.z;
        float newRoll = Mathf.SmoothDampAngle(
            currentRoll,
            targetRoll,
            ref rollVelocity,
            rotationSmoothTime
        );

        // X軸(ピッチ)
        float currentPitch = (currentEuler.x > 180f) ? currentEuler.x - 360f : currentEuler.x;
        float newPitch = Mathf.SmoothDampAngle(
            currentPitch,
            targetPitch,
            ref pitchVelocity,
            rotationSmoothTime
        );

        // Y軸回転はそのまま使わない(不要なら0固定でもOK)
        float newYaw = currentEuler.y;

        // ローカル座標で回転を適用
        childTransform.localRotation = Quaternion.Euler(newPitch, newYaw, newRoll);
    }
}
