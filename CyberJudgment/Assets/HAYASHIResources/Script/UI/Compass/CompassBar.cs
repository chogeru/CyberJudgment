using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompassBar : MonoBehaviour
{
    public RectTransform compassBar1;
    public RectTransform compassBar2;
    public Transform player;
    private float compassWidth;
    private Vector2 startPosition1;
    private Vector2 startPosition2;

    void Start()
    {
        if (compassBar1 != null && compassBar2 != null)
        {
            compassWidth = compassBar1.sizeDelta.x;
            startPosition1 = compassBar1.anchoredPosition;
            startPosition2 = compassBar2.anchoredPosition = new Vector2(compassWidth, 0); // 2つ目を右側に配置
        }
    }

    void Update()
    {
        if (compassBar1 != null && compassBar2 != null && player != null)
        {
            float playerRotation = player.eulerAngles.y;
            float normalizedRotation = playerRotation / 360f;
            float newPosition = -normalizedRotation * compassWidth;

            compassBar1.anchoredPosition = startPosition1 + new Vector2(newPosition, 0);
            compassBar2.anchoredPosition = startPosition2 + new Vector2(newPosition, 0);

            // ループ処理
            if (compassBar1.anchoredPosition.x <= -compassWidth)
            {
                compassBar1.anchoredPosition += new Vector2(compassWidth * 2, 0);
            }
            else if (compassBar1.anchoredPosition.x >= compassWidth)
            {
                compassBar1.anchoredPosition -= new Vector2(compassWidth * 2, 0);
            }

            if (compassBar2.anchoredPosition.x <= -compassWidth)
            {
                compassBar2.anchoredPosition += new Vector2(compassWidth * 2, 0);
            }
            else if (compassBar2.anchoredPosition.x >= compassWidth)
            {
                compassBar2.anchoredPosition -= new Vector2(compassWidth * 2, 0);
            }
        }
    }
}
