using R3;

/// <summary>
/// UI���f���N���X�A���j���[�̊J��Ԃ��Ǘ�����
/// </summary>
public class UIModel
{
    // ���j���[���J���Ă��邩�ǂ�����ReactiveProperty�A�����l��false
    public ReactiveProperty<bool> IsMenuOpen { get; } = new ReactiveProperty<bool>(false);
    // ���ݕ\������Ă���UI�I�u�W�F�N�g�̖��O��ReactiveProperty�A�����l�͋󕶎���
    public ReactiveProperty<string> CurrentUIObject { get; } = new ReactiveProperty<string>("");
    // �J�[�\�����\������Ă��邩�ǂ�����\��ReactiveProperty�A�����l��false
    public ReactiveProperty<bool> IsCursorVisible { get; } = new ReactiveProperty<bool>(false);
}