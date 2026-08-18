using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ShaderController : MonoBehaviour
{
    public Material _targetMaterial;

    [Range(0, 1)]
    public float _mainColorPower = 0.8f;

    [Range(0, 1)]
    public float _highlightColorPower = 1.0f;

    [Range(0, 1)]
    public float _shadowThreshold = 0.93f;

    [Range(0, 1)]
    public float _shadowHardness = 0.0f;

    [Range(0, 1)]
    public float _glossIntensity = 1.0f;

    [Range(0, 1)]
    public float _glossiness = 0.6f;

    [Range(0, 1)]
    public float _selfLitIntensity = 0.0f;

    [Range(0, 1)]
    public float _outlineWidth = 0.5f;

    public Color _emissionColor = Color.white;
    [Range(0, 10)]
    public float _emissionIntensity = 1.0f;

    void Update()
    {
        if (_targetMaterial != null)
        {
            _targetMaterial.SetFloat("_MaiColPo", _mainColorPower);
            _targetMaterial.SetFloat("_HighlightColorPower", _highlightColorPower);
            _targetMaterial.SetFloat("_SelfShadowThreshold", _shadowThreshold);
            _targetMaterial.SetFloat("_ShadowHardness", _shadowHardness);
            _targetMaterial.SetFloat("_GlossIntensity", _glossIntensity);
            _targetMaterial.SetFloat("_Glossiness", _glossiness);
            _targetMaterial.SetFloat("_SelfLitIntensity", _selfLitIntensity);
            _targetMaterial.SetFloat("_OutlineWidth", _outlineWidth);
            _targetMaterial.SetColor("_EmissionColor", _emissionColor * _emissionIntensity);
            _targetMaterial.EnableKeyword("_EMISSION"); 
        }
    }

    void OnValidate()
    {
        Update();
    }
}
