using UnityEngine;
using UnityEngine.UI;

public class PointerController : MonoBehaviour
{
    [SerializeField] private RectTransform pointerImage;    // ポインター用 Image の RectTransform
    [SerializeField] private RectTransform canvasRectTransform; // Canvas の RectTransform
    [SerializeField] private Camera uiCamera;                 // Screen Space - Camera モードの Canvas にアサインしているカメラ

    void Start()
    {
        // OSのカーソルを非表示に
        Cursor.visible = false;
        // 必要に応じてカーソルのロック状態を設定
        Cursor.lockState = CursorLockMode.Confined;
    }

    void Update()
    {
        Vector2 localPoint;
        // Input.mousePosition (スクリーン座標) を Canvas 内のローカル座標に変換
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRectTransform,    // 対象の Canvas の RectTransform
            Input.mousePosition,      // マウスのスクリーン座標
            uiCamera,                 // Canvas に設定されているカメラ
            out localPoint            // 変換後のローカル座標
        );

        // 得られたローカル座標を、ポインターImage の anchoredPosition に設定する
        pointerImage.anchoredPosition = localPoint;
    }
}
