using R3;
using UnityEngine;

/// <summary>
/// UI���f���N���X�B���j���[�̊J��Ԃ��Ǘ�����B
/// </summary>
public class UIModel
{
    public ReactiveProperty<bool> IsMenuOpen { get; } = new ReactiveProperty<bool>(false);
}