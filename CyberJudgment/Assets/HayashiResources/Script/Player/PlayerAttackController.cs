using AbubuResouse.Singleton;
using Cysharp.Threading.Tasks;
using R3;
using R3.Triggers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerAttackController : MonoBehaviour
{
    [SerializeField]
    private PlayerManager playerManager;

    [SerializeField, Header("�A�j���[�^�[")]
    private Animator m_Animator;

    [SerializeField, Header("�U���ҋ@����")]
    private float m_AttackCoolDown = 0.5f;

    [SerializeField]
    private bool isAttack = true;

    [SerializeField, Header("�U���{�C�X�N���b�v��")]
    private string m_NomalAttackVoiceClipName;

    [SerializeField, Header("���U���{�C�X�N���b�v��")]
    private string m_StringAttackVoiceClipName;

    [SerializeField, Header("�ʏ�U����SE")]
    private string m_NomalAttackSE;

    [SerializeField, Header("���U����SE")]
    private string m_StringAttackSE;

    [SerializeField, Header("����")]
    private float m_Volume;

    [SerializeField, Header("�G��T�����a")]
    private float searchRadius = 5f;

    private bool enableRootMotion = false;
    private Transform nearestEnemy = null;

    private void Start()
    {
        BindAttackInput();
    }

    private void Update()
    {
        if (StopManager.Instance.IsStopped)
        {
            CancelAttack();
            return;
        }
        CheckAttackAnimationEnd();
    }

    private void FixedUpdate()
    {
        if (enableRootMotion)
        {
            TrackNearestEnemy();
        }
    }

    /// <summary>
    /// ���͂ɂ��U���A�j���[�V�������Đ����鏈��
    /// </summary>
    private void BindAttackInput()
    {
        // ���N���b�N�ɂ��ʏ�U��
        this.UpdateAsObservable()
            .Where(_ => !StopManager.Instance.IsStopped)
            .Where(_ => Input.GetMouseButtonDown(0))
            .Where(_ => isAttack)
            .Subscribe(_ => ExecuteAttack("NormalAttack", m_NomalAttackVoiceClipName, m_NomalAttackSE));

        // �E�N���b�N�ɂ�鋭�U��
        this.UpdateAsObservable()
            .Where(_ => !StopManager.Instance.IsStopped)
            .Where(_ => Input.GetMouseButtonDown(1))
            .Where(_ => isAttack)
            .Subscribe(_ => ExecuteAttack("StrongAttack", m_StringAttackVoiceClipName, m_StringAttackSE));

        // �}�E�X�{�^���̃����[�X�����m����Idle�Ɉڍs
        this.UpdateAsObservable()
            .Where(_ => Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
            .Subscribe(_ => CancelAttack());
    }

    /// <summary>
    /// �U���A�j���[�V�����̍Đ�
    /// </summary>
    /// <param name="animationTrigger">�Đ�����A�j���[�V�����̃g���K�[��</param>
    private void ExecuteAttack(string animationTrigger, string voiceClipName, string attackSEClipName)
    {
        playerManager.SetAttacking(true);
        enableRootMotion = true;
        nearestEnemy = FindNearestEnemy();
        m_Animator.Play(animationTrigger);

        VoiceManager.Instance.PlaySound(voiceClipName, m_Volume);
        SEManager.Instance.PlaySound(attackSEClipName, m_Volume);

        isAttack = false;
        ResetAttackCooldown().Forget();
    }



    /// <summary>
    /// �U���A�j���[�V�������I��������Idle��ԂɈڍs
    /// </summary>
    private void CheckAttackAnimationEnd()
    {
        var currentState = m_Animator.GetCurrentAnimatorStateInfo(0);

        if ((currentState.IsName("NormalAttack") || currentState.IsName("StrongAttack")) && currentState.normalizedTime >= 1.0f)
        {
            TransitionToIdle();
        }
    }

    /// <summary>
    /// �U�����L�����Z������Idle��ԂɈڍs
    /// </summary>
    private void CancelAttack()
    {
        var currentState = m_Animator.GetCurrentAnimatorStateInfo(0);

        if (currentState.IsName("NormalAttack") || currentState.IsName("StrongAttack"))
        {
            // �U�����L�����Z������Idle�ɑJ��
            TransitionToIdle();
        }
    }

    private void TransitionToIdle()
    {
        enableRootMotion = false;
        nearestEnemy = null;

        m_Animator.ResetTrigger("NormalAttack");
        m_Animator.ResetTrigger("StrongAttack");
        m_Animator.CrossFade("Idle", 0.01f);
        isAttack = true;

        // �v���C���[�̏�Ԃ�Idle�ɍX�V
        playerManager.UpdatePlayerState(PlayerState.Idle);
        playerManager.SetAttacking(false);

        // �ړ��L�[��������Ă��邩���m�F���A�Ή�����A�j���[�V�����ƃX�e�[�g���X�V
        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        if (movement != Vector3.zero)
        {
            // ���肩�����̏�ԂɑJ��
            playerManager.UpdatePlayerState(isRunning ? PlayerState.Run : PlayerState.Walk);
            m_Animator.SetBool("Run", isRunning);
            m_Animator.SetBool("Walk", !isRunning);
        }
    }

    /// <summary>
    /// �N�[���_�E�����o�čĂэU�����\�ɂ���
    /// </summary>
    /// <returns></returns>
    private async UniTaskVoid ResetAttackCooldown()
    {
        await UniTask.Delay((int)(m_AttackCoolDown * 1000));
        isAttack = true;
    }

    private void OnAnimatorMove()
    {
        if (enableRootMotion)
        {
            transform.position += m_Animator.deltaPosition;
        }
    }

    /// <summary>
    /// 5m�ȓ��̍ł��߂��G��T��
    /// </summary>
    /// <returns></returns>
    private Transform FindNearestEnemy()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, searchRadius, LayerMask.GetMask("Enemy"));
        Transform nearest = null;
        float minDistance = float.MaxValue;

        foreach (var hitCollider in hitColliders)
        {
            float distance = Vector3.Distance(transform.position, hitCollider.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = hitCollider.transform;
            }
        }

        return nearest;
    }

    /// <summary>
    /// �ł��߂��G�̕����ɉ�]���A�ڐG���Ă���ꍇ�̓��[�g���[�V�����𖳌��ɂ���
    /// </summary>
    private void TrackNearestEnemy()
    {
        if (nearestEnemy != null)
        {
            // �G�̕����ɉ�]
            Vector3 direction = (nearestEnemy.position - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);

            // �G�ƐڐG���Ă��邩�m�F
            float distanceToEnemy = Vector3.Distance(transform.position, nearestEnemy.position);
            if (distanceToEnemy <= 1.0f)
            {
                enableRootMotion = false;
            }
            else
            {
                enableRootMotion = true;
            }
        }
    }
}
