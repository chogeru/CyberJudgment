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
    [SerializeField] private int _shakeAmount = 60; // �V�F�C�N�̗�
    [SerializeField] private float _shakeDuration = 3f; // �V�F�C�N�̌p������
    [SerializeField] private float _shakeInterval = 0.02f; // �V�F�C�N�̊Ԋu

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
        string filePath = Path.Combine(Application.dataPath, "ファンタジーコネク�.txt");

        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "バグ発�");
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

            // �E�C���h�E�����̈ʒu�ɖ߂�
            MoveWindow(hWnd, originalX, originalY, rect.right - rect.left, rect.bottom - rect.top, true);
        }
    }

    private struct RECT
    {
        public int left, top, right, bottom;
    }
}
