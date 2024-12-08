using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera targetCamera;

    /// <summary>
    /// �J������ݒ�
    /// </summary>
    /// <param name="camera">�^�[�Q�b�g�J����</param>
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
