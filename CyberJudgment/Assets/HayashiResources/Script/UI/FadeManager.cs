using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeManager : MonoBehaviour
{
    private static FadeManager instance;
    public static FadeManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<FadeManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = typeof(FadeManager).Name;
                    instance = obj.AddComponent<FadeManager>();
                }
            }
            return instance;
        }
    }

    [SerializeField, Header("フェード用の画像")]
    private Image m_FadeImage;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // フェード処理
    public void FadeIn(float fadeDuration)
    {
        //カラーの設定
        Color alfaColor = m_FadeImage.color;
        alfaColor.a = 0f;
        m_FadeImage.color = alfaColor;
        StartCoroutine(FadeImage(fadeDuration, 1));
    }

    public void FadeOut(float fadeDuration)
    {
        //カラーの設定
        Color alfaColor = m_FadeImage.color;
        alfaColor.a = 1f;
        m_FadeImage.color = alfaColor;
        StartCoroutine(FadeImage(fadeDuration, 0));
    }

    private IEnumerator FadeImage(float fadeDuration, float targetAlpha)
    {
        Color currentColor = m_FadeImage.color;
        Color targetColor = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);

        float currentTime = 0f;
        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            m_FadeImage.color = Color.Lerp(currentColor, targetColor, currentTime / fadeDuration);
            yield return null;
        }
        m_FadeImage.color = targetColor;
    }
}

