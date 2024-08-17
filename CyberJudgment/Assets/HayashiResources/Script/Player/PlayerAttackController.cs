using Cysharp.Threading.Tasks;
using R3;
using R3.Triggers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackController : MonoBehaviour
{
    [SerializeField, Header("アニメーター")]
    private Animator m_Animator;

    [SerializeField, Header("攻撃待機時間")]
    private float m_AttackCoolDown = 0.5f;

    private bool isAttack = true;

    private void Start()
    {
        BindAttackInput();
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
            .Subscribe(_ => ExecuteAttack("NormalAttack"));

        // 右クリックによる強攻撃
        this.UpdateAsObservable()
            .Where(_ => Input.GetMouseButtonDown(1))
            .Where(_ => isAttack)
            .Subscribe(_ => ExecuteAttack("StrongAttack"));

        // マウスボタンのリリースを検知してIdleに移行
        this.UpdateAsObservable()
            .Where(_ => Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
            .Subscribe(_ => CancelAttack());
    }

    /// <summary>
    /// 攻撃アニメーションの再生
    /// </summary>
    /// <param name="animationTrigger">再生するアニメーションのトリガー名</param>
    private void ExecuteAttack(string animationTrigger)
    {
        var playerManager = GetComponent<PlayerManager>();
        playerManager.SetAttacking(true);

        m_Animator.Play(animationTrigger);
        isAttack = false;
        ResetAttackCooldown().Forget();
    }

    /// <summary>
    /// 攻撃をキャンセルしてIdle状態に移行
    /// </summary>
    private void CancelAttack()
    {
        // 現在のアニメーションステートが攻撃であるかを確認
        if (m_Animator.GetCurrentAnimatorStateInfo(0).IsName("NormalAttack") ||
            m_Animator.GetCurrentAnimatorStateInfo(0).IsName("StrongAttack"))
        {
            m_Animator.Play("Idle");
            isAttack = true;

            // プレイヤーの状態をIdleに更新
            var playerManager = GetComponent<PlayerManager>();
            playerManager.UpdatePlayerState(PlayerState.Idle);
            playerManager.SetAttacking(false);
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

