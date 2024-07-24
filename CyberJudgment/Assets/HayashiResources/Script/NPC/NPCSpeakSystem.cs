using AbubuResouse.UI;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NPCSpeakSystem : MonoBehaviour
{
    public enum NPCType
    {
        NPC,
        ShopNPC
    }
    [SerializeField, Header("NPC�̃^�C�v")]
    public NPCType _npcType;

    [SerializeField, ReadOnly, Header("�v���C���[�̃g�����X�t�H�[��")]
    private Transform _player;

    [SerializeField, Header("�V���b�v�p��Canvas")]
    private GameObject _shopCanvas;

    [SerializeField, Header("�����ŕ\������UI")]
    public GameObject _speakCanvas;


    [SerializeField, Header("UI�\���p�̋���")]
    public float _triggerDistance = 10f;

    [SerializeField, Header("��b���̃{�^��")]
    private KeyCode _keyCode;

    [SerializeField, Header("�e�L�X�g�g���K�[")]
    public TextTrigger _textTrigger;


    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
    }
    private void Update()
    {
        float distance = Vector3.Distance(transform.position, _player.position);

        if (distance <= _triggerDistance)
        {
            // �v���C���[���w��̋����ȓ��ɂ���ꍇ�AUI��\������
            _speakCanvas.SetActive(true);
            Vector3 UIPos = transform.position;
            UIPos.y += 2;
            _speakCanvas.transform.position = UIPos;
            if (Input.GetKeyDown(_keyCode))
            {
                _textTrigger.TriggerTextDisplay();

            }
            if (_npcType == NPCType.ShopNPC && TextManager.Instance.isTextEnd)
            {
                Cursor.visible = true;
                StopManager.Instance.IsStopped = true;
                _shopCanvas.SetActive(true);
            }
        }
        else
        {
            ResetText();
        }

    }
    public void CanvasClose()
    {
        Cursor.visible = false;
        StopManager.Instance.IsStopped = false;
        _shopCanvas.SetActive(false);
        TextManager.Instance.isTextEnd = false;
    }
    public void ResetText()
    {
        // �v���C���[���w��̋����O�ɂ���ꍇ�AUI���\���ɂ���
        _speakCanvas.SetActive(false);
        _textTrigger.ResetTextIndex();
        TextManager.Instance.isTextEnd = false;
    }
}
