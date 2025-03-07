using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
[RequireComponent(typeof(Light))]
public class PhysicalLight : MonoBehaviour
{
    // Point/Spotライト用の物理単位（ルーメン）
    [Tooltip("光源のルーメン値 (lm) - Point/Spot Lights")]
    public float lumens = 800f;

    // Directionalライト用の物理単位（照度：lux）
    [Tooltip("光源の照度値 (lux) - Directional Light")]
    public float lux = 120000f;

    // キャリブレーションファクター（シーン全体の露出やトーンマッピングに合わせた補正用）
    [Tooltip("キャリブレーションファクター（実際の露出との調整用）")]
    public float calibrationFactor = 1f;

    private Light lightComponent;

    void OnEnable()
    {
        lightComponent = GetComponent<Light>();
        UpdateLightIntensity();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (lightComponent == null)
            lightComponent = GetComponent<Light>();

        UpdateLightIntensity();

        // Inspectorでの変更があった際にシーンビューを即座に更新
        SceneView.RepaintAll();
    }
#endif

    void UpdateLightIntensity()
    {
        if (lightComponent == null)
            return;

        if (lightComponent.type == LightType.Point)
        {
            // ポイントライトの場合: candela = lumens / (4π)
            lightComponent.intensity = (lumens / (4f * Mathf.PI)) * calibrationFactor;
        }
        else if (lightComponent.type == LightType.Spot)
        {
            // スポットライトの場合: 半角をラジアンに変換して立体角を計算
            float halfAngleRad = lightComponent.spotAngle * 0.5f * Mathf.Deg2Rad;
            lightComponent.intensity = (lumens / (2f * Mathf.PI * (1f - Mathf.Cos(halfAngleRad)))) * calibrationFactor;
        }
        else if (lightComponent.type == LightType.Directional)
        {
            // ディレクショナルライトの場合: 一般的な晴天時の日光は約120,000 luxとして換算
            lightComponent.intensity = (lux / 120000f) * calibrationFactor;
        }
    }
}
