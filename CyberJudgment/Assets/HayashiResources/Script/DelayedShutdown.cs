using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Runtime.InteropServices;
using System.IO;

public class DelayedShutdown : MonoBehaviour
{
    [SerializeField] private float _delaySeconds = 3f;
    [SerializeField] private int _shakeAmount = 60; // シェイクの量
    [SerializeField] private float _shakeDuration = 3f; // シェイクの継続時間
    [SerializeField] private float _shakeInterval = 0.02f; // シェイクの間隔

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    private void Start()
    {
        ShutdownAfterDelayAsync().Forget();
    }

    private async UniTaskVoid ShutdownAfterDelayAsync()
    {
        string filePath = Path.Combine(Application.dataPath, "繝輔ぃ繝ｳ繧ｿ繧ｸ繝ｼ繧ｳ繝阪け繝.txt");

        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "繝舌げ逋ｺ逕");
        }

        if (!Screen.fullScreen)
        {
            Screen.fullScreen = false;
            await ShakeWindow();
        }

        await UniTask.Delay(TimeSpan.FromSeconds(_delaySeconds));
        if (!Screen.fullScreen)
        {
            await ShakeWindow();
        }
        Application.Quit();
    }

    private async UniTask ShakeWindow()
    {
        IntPtr hWnd = GetActiveWindow();
        if (hWnd == IntPtr.Zero)
        {
            return;
        }

        if (GetWindowRect(hWnd, out RECT rect))
        {
            int originalX = rect.left;
            int originalY = rect.top;

            float elapsed = 0f;
            while (elapsed < _shakeDuration)
            {
                int offsetX = UnityEngine.Random.Range(-_shakeAmount, _shakeAmount);
                int offsetY = UnityEngine.Random.Range(-_shakeAmount, _shakeAmount);
                MoveWindow(hWnd, originalX + offsetX, originalY + offsetY, rect.right - rect.left, rect.bottom - rect.top, true);
                await UniTask.Delay(TimeSpan.FromSeconds(_shakeInterval));

                elapsed += _shakeInterval;
            }

            // ウインドウを元の位置に戻す
            MoveWindow(hWnd, originalX, originalY, rect.right - rect.left, rect.bottom - rect.top, true);
        }
    }

    private struct RECT
    {
        public int left, top, right, bottom;
    }
}
