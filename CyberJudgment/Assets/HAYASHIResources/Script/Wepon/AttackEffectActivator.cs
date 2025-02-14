using System.Collections.Generic;
using UnityEngine;

public class AttackEffectActivator : MonoBehaviour
{
    [SerializeField, Header("エフェクトリスト")]
    private List<GameObject> attackEffects;
    [SerializeField, Header("プレイヤーのAnimator")]
    private Animator playerAnimator;

    // エフェクトに対応するParticleSystemのリストを保持
    private List<ParticleSystem> particleSystems = new List<ParticleSystem>();

    private void Start()
    {
        // ParticleSystemをキャッシュ
        foreach (var effect in attackEffects)
        {
            if (effect != null)
            {
                ParticleSystem ps = effect.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    particleSystems.Add(ps);
                }
                else
                {
                    Debug.LogWarning($"エフェクト {effect.name} にParticleSystemがアタッチされていません。");
                    particleSystems.Add(null);
                }
            }
            else
            {
                particleSystems.Add(null);
            }
        }

        AttackEventManager.OnEffectEvent += ActivateEffect;
        AttackEventManager.OffEffectEvent += DeactivateEffect;
    }

    private void Update()
    {
        if (!IsInAttackAnimation())
        {
            DeactivateAllEffects();
        }
    }

    private void ActivateEffect(int index)
    {
        if (index >= 0 && index < attackEffects.Count && attackEffects[index] != null)
        {
            attackEffects[index].SetActive(true);

            // ParticleSystemを再生
            ParticleSystem ps = particleSystems[index];
            if (ps != null)
            {
                ps.Play();
            }
        }
    }

    private void DeactivateEffect(int index)
    {
        if (index >= 0 && index < attackEffects.Count && attackEffects[index] != null)
        {
            // ParticleSystemを停止
            ParticleSystem ps = particleSystems[index];
            if (ps != null)
            {
                ps.Stop();
            }

            attackEffects[index].SetActive(false);
        }
    }

    private void DeactivateAllEffects()
    {
        for (int i = 0; i < attackEffects.Count; i++)
        {
            if (attackEffects[i] != null)
            {
                // ParticleSystemを停止
                ParticleSystem ps = particleSystems[i];
                if (ps != null)
                {
                    ps.Stop();
                }

                attackEffects[i].SetActive(false);
            }
        }
    }

    private bool IsInAttackAnimation()
    {
        if (playerAnimator == null) return false;

        AnimatorStateInfo stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName("NormalAttack") || stateInfo.IsName("StrongAttack");
    }

    private void OnDestroy()
    {
        AttackEventManager.OnEffectEvent -= ActivateEffect;
        AttackEventManager.OffEffectEvent -= DeactivateEffect;
    }
}
