using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    // �Ǐ]�Ώۂ̃^�[�Q�b�g
    public Transform target;

    // �I�t�Z�b�g (�^�[�Q�b�g�Ƃ̑��Έʒu)
    public Vector3 offset = Vector3.zero;

    // �Ǐ]���x
    public float followSpeed = 5f;

    void Update()
    {
        if (target == null)
        {
            Debug.LogWarning("�^�[�Q�b�g���ݒ肳��Ă��܂���I");
            return;
        }

        // �^�[�Q�b�g�̈ʒu + �I�t�Z�b�g
        Vector3 targetPosition = target.position + offset;

        // ���݈ʒu����^�[�Q�b�g�ʒu�ɃX���[�Y�Ɉړ�
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }
}
