using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 各種AudioFilterControllerを一括管理するクラス。
/// AudioSourceに必要なフィルタコンポーネントをアタッチして、
/// コントローラを生成・保持する。
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AudioFilterManager : MonoBehaviour
{
    private AudioSource audioSource;

    // フィルタコントローラを Type をキーとする辞書で保持 (拡張しやすい)
    private Dictionary<Type, IAudioFilterController> filterControllers = new Dictionary<Type, IAudioFilterController>();

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // フィルタコンポーネントをアタッチ＆無効化
        // ---------------------------------------
        var chorus = gameObject.AddComponent<AudioChorusFilter>();
        chorus.enabled = false;

        var distortion = gameObject.AddComponent<AudioDistortionFilter>();
        distortion.enabled = false;

        var echo = gameObject.AddComponent<AudioEchoFilter>();
        echo.enabled = false;

        var reverb = gameObject.AddComponent<AudioReverbFilter>();
        reverb.enabled = false;

        var lowPass = gameObject.AddComponent<AudioLowPassFilter>();
        lowPass.enabled = false;

        var highPass = gameObject.AddComponent<AudioHighPassFilter>();
        highPass.enabled = false;

        // それぞれのコントローラを生成
        // ---------------------------------------
        var chorusController = new ChorusFilterController(chorus);
        var distortionController = new DistortionFilterController(distortion);
        var echoController = new EchoFilterController(echo);
        var reverbController = new ReverbFilterController(reverb);
        var lowPassController = new LowPassFilterController(lowPass);
        var highPassController = new HighPassFilterController(highPass);

        // 辞書へ登録
        // ---------------------------------------
        filterControllers.Add(typeof(ChorusFilterController), chorusController);
        filterControllers.Add(typeof(DistortionFilterController), distortionController);
        filterControllers.Add(typeof(EchoFilterController), echoController);
        filterControllers.Add(typeof(ReverbFilterController), reverbController);
        filterControllers.Add(typeof(LowPassFilterController), lowPassController);
        filterControllers.Add(typeof(HighPassFilterController), highPassController);
    }

    /// <summary>
    /// 指定したフィルタコントローラクラスを取り出す
    /// 例: GetFilter<ChorusFilterController>() で ChorusFilterController を取得
    /// </summary>
    public T GetFilter<T>() where T : class, IAudioFilterController
    {
        var type = typeof(T);
        if (filterControllers.ContainsKey(type))
        {
            return filterControllers[type] as T;
        }
        return null;
    }
}
