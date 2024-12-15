using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Zenject.SpaceFighter;
using AbubuResouse.Editor;
using AbubuResouse.Singleton;
using UnityEngine.InputSystem;

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

    [SerializeField, Header("��]���x")]
    private float _rotationSpeed = 20f;

    private bool _isCanvasActive = false;

    private bool _isTalking = false;

    private Quaternion _originalRotation;

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _originalRotation = transform.rotation;
    }
    private void Update()
    {
        HandleNPCInteraction();
        HandleCanvasVisibility();
    }

    /// <summary>
    /// NPC�Ƃ̃C���^���N�V��������
    /// </summary>
    private void HandleNPCInteraction()
    {
        if (_isTalking)
        {
            RotateTowardsPlayer();
            HandleShopNPC();
        }
        else
        {
            ReturnToOriginalRotation();
        }
    }

    /// <summary>
    /// �v���C���[�̕����Ɍ�������
    /// </summary>
    private void RotateTowardsPlayer()
    {
        Vector3 direction = (_player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * _rotationSpeed);
    }

    /// <summary>
    /// ShopNPC�̏ꍇ�̏���
    /// �e�L�X�g���I��������V���b�v�L�����o�X��\��
    /// </summary>
    private void HandleShopNPC()
    {
        if (_npcType == NPCType.ShopNPC && TextManager.Instance.isTextEnd)
        {
            Cursor.visible = true;
            StopManager.Instance.IsStopped = true;
            _shopCanvas.SetActive(true);
        }
    }

    /// <summary>
    /// ���̉�]�ɖ߂鏈��
    /// </summary>
    private void ReturnToOriginalRotation()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, _originalRotation, Time.deltaTime * _rotationSpeed);
    }

    /// <summary>
    /// �L�����o�X�̕\���E��\��������
    /// </summary>
    private void HandleCanvasVisibility()
    {
        float distance = Vector3.Distance(transform.position, _player.position);

        if (distance <= _triggerDistance)
        {
            ShowCanvas();
            if (Input.GetKeyDown(_keyCode) || (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame))
            {
                _textTrigger.TriggerTextDisplay();
                _isTalking = true;
            }
        }
        else if (_isCanvasActive)
        {
            ResetText();
            _isCanvasActive = false;
        }
    }

    /// <summary>
    /// �L�����o�X��\�����鏈��
    /// </summary>
    private void ShowCanvas()
    {
        _speakCanvas.SetActive(true);
        Vector3 uiPosition = transform.position;
        uiPosition.y += 2;
        _speakCanvas.transform.position = uiPosition;
        _isCanvasActive = true;
    }

    /// <summary>
    /// �L�����o�X����鏈��
    /// </summary>
    public void CanvasClose()
    {
        Cursor.visible = false;
        StopManager.Instance.IsStopped = false;
        _shopCanvas.SetActive(false);
        TextManager.Instance.isTextEnd = false;
        _isTalking = false;
    }

    /// <summary>
    /// �e�L�X�g�����Z�b�g���鏈��
    /// </summary>
    public void ResetText()
    {
        _speakCanvas.SetActive(false);
        _textTrigger.ResetTextIndex();
        TextManager.Instance.isTextEnd = false;
        _isTalking = false;
    }
}
