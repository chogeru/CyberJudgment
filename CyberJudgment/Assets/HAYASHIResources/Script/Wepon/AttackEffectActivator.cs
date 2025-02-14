using System.Collections.Generic;
using UnityEngine;

public class AttackEffectActivator : MonoBehaviour
{
    [SerializeField, Header("�G�t�F�N�g���X�g")]
    private List<GameObject> attackEffects;
    [SerializeField, Header("�v���C���[��Animator")]
    private Animator playerAnimator;

    // �G�t�F�N�g�ɑΉ�����ParticleSystem�̃��X�g��ێ�
    private List<ParticleSystem> particleSystems = new List<ParticleSystem>();

    private void Start()
    {
        // ParticleSystem���L���b�V��
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
                    Debug.LogWarning($"�G�t�F�N�g {effect.name} ��ParticleSystem���A�^�b�`����Ă��܂���B");
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

            // ParticleSystem���Đ�
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
            // ParticleSystem���~
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
                // ParticleSystem���~
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
