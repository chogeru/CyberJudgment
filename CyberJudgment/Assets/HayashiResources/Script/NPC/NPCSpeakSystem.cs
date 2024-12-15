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
    [SerializeField, Header("NPCのタイプ")]
    public NPCType _npcType;

    [SerializeField, ReadOnly, Header("プレイヤーのトランスフォーム")]
    private Transform _player;

    [SerializeField, Header("ショップ用のCanvas")]
    private GameObject _shopCanvas;

    [SerializeField, Header("距離で表示するUI")]
    public GameObject _speakCanvas;


    [SerializeField, Header("UI表示用の距離")]
    public float _triggerDistance = 10f;

    [SerializeField, Header("会話時のボタン")]
    private KeyCode _keyCode;

    [SerializeField, Header("テキストトリガー")]
    public TextTrigger _textTrigger;

    [SerializeField, Header("回転速度")]
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
    /// NPCとのインタラクション処理
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
    /// プレイヤーの方向に向く処理
    /// </summary>
    private void RotateTowardsPlayer()
    {
        Vector3 direction = (_player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * _rotationSpeed);
    }

    /// <summary>
    /// ShopNPCの場合の処理
    /// テキストが終了したらショップキャンバスを表示
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
    /// 元の回転に戻る処理
    /// </summary>
    private void ReturnToOriginalRotation()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, _originalRotation, Time.deltaTime * _rotationSpeed);
    }

    /// <summary>
    /// キャンバスの表示・非表示を処理
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
    /// キャンバスを表示する処理
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
    /// キャンバスを閉じる処理
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
    /// テキストをリセットする処理
    /// </summary>
    public void ResetText()
    {
        _speakCanvas.SetActive(false);
        _textTrigger.ResetTextIndex();
        TextManager.Instance.isTextEnd = false;
        _isTalking = false;
    }
}
