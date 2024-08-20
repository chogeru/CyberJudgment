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

    [SerializeField, Header("アニメーター")]
    private Animator m_Animator;

    [SerializeField, Header("攻撃待機時間")]
    private float m_AttackCoolDown = 0.5f;

    [SerializeField]
    private bool isAttack = true;

    [SerializeField, Header("攻撃ボイスクリップ名")]
    private string m_NomalAttackVoiceClipName;

    [SerializeField, Header("強攻撃ボイスクリップ名")]
    private string m_StringAttackVoiceClipName;

    [SerializeField, Header("通常攻撃時SE")]
    private string m_NomalAttackSE;

    [SerializeField, Header("強攻撃時SE")]
    private string m_StringAttackSE;

    [SerializeField, Header("音量")]
    private float m_Volume;
    private void Start()
    {
        BindAttackInput();
    }

    private void Update()
    {
        CheckAttackAnimationEnd();
    }

    /// <summary>
    /// 入力により攻撃アニメーションを再生する処理
    /// </summary>
    private void BindAttackInput()
    {
        // 左クリックによる通常攻撃
        this.UpdateAsObservable()
            .Where(_ => Input.GetMouseButtonDown(0))
            .Where(_ => isAttack)
            .Subscribe(_ => ExecuteAttack("NormalAttack", m_NomalAttackVoiceClipName, m_NomalAttackSE));

        // 右クリックによる強攻撃
        this.UpdateAsObservable()
            .Where(_ => Input.GetMouseButtonDown(1))
            .Where(_ => isAttack)
            .Subscribe(_ => ExecuteAttack("StrongAttack", m_StringAttackVoiceClipName, m_StringAttackSE));

        // マウスボタンのリリースを検知してIdleに移行
        this.UpdateAsObservable()
            .Where(_ => Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
            .Subscribe(_ => CancelAttack());
    }

    /// <summary>
    /// 攻撃アニメーションの再生
    /// </summary>
    /// <param name="animationTrigger">再生するアニメーションのトリガー名</param>
    private void ExecuteAttack(string animationTrigger, string voiceClipName, string attackSEClipName)
    {
        playerManager.SetAttacking(true);

        m_Animator.Play(animationTrigger);

        VoiceManager.Instance.PlaySound(voiceClipName, m_Volume);
        SEManager.Instance.PlaySound(attackSEClipName, m_Volume);

        isAttack = false;
        ResetAttackCooldown().Forget();
    }


    /// <summary>
    /// 攻撃アニメーションが終了したらIdle状態に移行
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
    /// 攻撃をキャンセルしてIdle状態に移行
    /// </summary>
    private void CancelAttack()
    {
        var currentState = m_Animator.GetCurrentAnimatorStateInfo(0);

        if (currentState.IsName("NormalAttack") || currentState.IsName("StrongAttack"))
        {
            // 攻撃をキャンセルしてIdleに遷移
            TransitionToIdle();
        }
    }


    private void TransitionToIdle()
    {
        m_Animator.ResetTrigger("NormalAttack");
        m_Animator.ResetTrigger("StrongAttack");
        m_Animator.CrossFade("Idle", 0.01f);
        isAttack = true;

        // プレイヤーの状態をIdleに更新
        playerManager.UpdatePlayerState(PlayerState.Idle);
        playerManager.SetAttacking(false);

        // 移動キーが押されているかを確認し、対応するアニメーションとステートを更新
        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        if (movement != Vector3.zero)
        {
            // 走りか歩きの状態に遷移
            playerManager.UpdatePlayerState(isRunning ? PlayerState.Run : PlayerState.Walk);
            m_Animator.SetBool("Run", isRunning);
            m_Animator.SetBool("Walk", !isRunning);
        }
    }

    /// <summary>
    /// クールダウンを経て再び攻撃を可能にする
    /// </summary>
    /// <returns></returns>
    private async UniTaskVoid ResetAttackCooldown()
    {
        await UniTask.Delay((int)(m_AttackCoolDown * 1000));
        isAttack = true;
    }
}

