using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Audio;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using System.IO;
using AbubuResouse.Log;

public class AudioVolumeSetting : MonoBehaviour
{
    [SerializeField] 
    AudioMixer m_AudioMixer;
    [SerializeField,Header("音量設定用スライダー")]
    Slider m_BGMSlider;
    [SerializeField,Header("効果音設定用スライダー")] 
    Slider m_SESlider;
    [SerializeField, Header("BGM音量表示")]
    TextMeshProUGUI m_BGMVolumeText;
    [SerializeField, Header("SE音量表示")]
    TextMeshProUGUI m_SEVolumeText;

    private string m_SavePath;

    private void Start()
    {
        m_SavePath = Path.Combine(Application.persistentDataPath, "AudioSettings.json");

        LoadAudioSettings();

        // スライダーのイベントリスナーを設定
        m_BGMSlider.onValueChanged.AddListener(SetBGM);
        m_SESlider.onValueChanged.AddListener(SetSE);
        // 初期値を設定
        SetBGM(m_BGMSlider.value);
        SetSE(m_SESlider.value);
    }

    // BGMの音量を設定するためのメソッド
    public void SetBGM(float volume)
    {
        SetVolume("BGM", volume);
        UpdateVolumeText(m_BGMVolumeText, volume);
        SaveAudioSettings();
    }

    // SEの音量を設定するためのメソッド
    public void SetSE(float volume)
    {
        SetVolume("SE", volume);
        UpdateVolumeText(m_SEVolumeText, volume);
        SaveAudioSettings();
    }

    private void SetVolume(string parameterName, float volume)
    {

        // スライダーの値が0の場合、最小値を設定
        if (volume <= 0)
        {
            m_AudioMixer.SetFloat(parameterName, -80f);
            DebugUtility.Log($"{parameterName}: -80dB (Minimum)");
        }
        else
        {
            float dB = Mathf.Log10(volume) * 20;
            m_AudioMixer.SetFloat(parameterName, dB);
            DebugUtility.Log($"{parameterName}: {dB}dB");
        }
    }
    private void LoadAudioSettings()
    {
        if (File.Exists(m_SavePath))
        {
            string json = File.ReadAllText(m_SavePath);
            AudioSettings settings = JsonUtility.FromJson<AudioSettings>(json);
            m_BGMSlider.value = settings.BGMVolume;
            m_SESlider.value = settings.SEVolume;
        }
        else
        {
            // 初期音量を最大に設定
            m_BGMSlider.value = 1f;
            m_SESlider.value = 1f;
        }
    }
    private void SaveAudioSettings()
    {
        AudioSettings settings = new AudioSettings
        {
            BGMVolume = m_BGMSlider.value,
            SEVolume = m_SESlider.value
        };
        string json = JsonUtility.ToJson(settings);
        File.WriteAllText(m_SavePath, json);
    }
    private void UpdateVolumeText(TextMeshProUGUI text, float volume)
    {
        text.text = volume.ToString("F1");
    }
    [System.Serializable]
    private class AudioSettings
    {
        public float BGMVolume;
        public float SEVolume;
    }
}
