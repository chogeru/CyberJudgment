using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera targetCamera;

    /// <summary>
    /// カメラを設定
    /// </summary>
    /// <param name="camera">ターゲットカメラ</param>
    public void SetTargetCamera(Camera camera)
    {
        targetCamera = camera;
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera != null)
        {
            transform.LookAt(transform.position + targetCamera.transform.rotation * Vector3.forward,
                             targetCamera.transform.rotation * Vector3.up);
        }
    }
}
