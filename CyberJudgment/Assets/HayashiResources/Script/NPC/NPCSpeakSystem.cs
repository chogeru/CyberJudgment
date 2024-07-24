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


    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
    }
    private void Update()
    {
        float distance = Vector3.Distance(transform.position, _player.position);

        if (distance <= _triggerDistance)
        {
            // プレイヤーが指定の距離以内にいる場合、UIを表示する
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
        // プレイヤーが指定の距離外にいる場合、UIを非表示にする
        _speakCanvas.SetActive(false);
        _textTrigger.ResetTextIndex();
        TextManager.Instance.isTextEnd = false;
    }
}
