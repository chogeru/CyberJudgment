using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCullingSystem : MonoBehaviour
{
    public LayerMask playerLayerMask; // プレイヤーレイヤーのマスク

    private Camera mainCamera;
    private int defaultCullingMask; // 初期のカリングマスク

    private void Start()
    {
        mainCamera = GetComponent<Camera>();
        defaultCullingMask = mainCamera.cullingMask;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // プレイヤーオブジェクトがコライダーに入った時の処理
            mainCamera.cullingMask &= ~playerLayerMask; // プレイヤーレイヤーをカメラのカリングマスクから除外
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // プレイヤーオブジェクトがコライダーから出た時の処理
            mainCamera.cullingMask = defaultCullingMask; // カリングマスクを初期値に戻す
        }
    }
}
