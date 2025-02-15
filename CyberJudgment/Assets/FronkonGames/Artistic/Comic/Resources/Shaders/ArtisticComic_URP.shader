////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Martin Bustos @FronkonGames <fronkongames@gmail.com>. All rights reserved.
//
// THIS FILE CAN NOT BE HOSTED IN PUBLIC REPOSITORIES.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
Shader "Hidden/Fronkon Games/Artistic/Comic URP"
{
  Properties
  {
    _MainTex("Main Texture", 2D) = "white" {}
  }

  SubShader
  {
    Tags
    {
      "RenderType" = "Opaque"
      "RenderPipeline" = "UniversalPipeline"
    }
    LOD 100
    ZTest Always ZWrite Off Cull Off

    Pass
    {
      Name "Fronkon Games Artistic Comic"

      HLSLPROGRAM
      #include "Artistic.hlsl"
      #include "ColorBlend.hlsl"

      #pragma vertex ArtisticVert
      #pragma fragment ArtisticFrag
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma exclude_renderers d3d9 d3d11_9x ps3 flash
      #pragma multi_compile _ _USE_DRAW_PROCEDURAL

      float _Scale;
      float4 _CMYKPattern;
      int _ColorBlend;
      float _Edge;
      float3 _EdgeColor;
      int _EdgeBlendOp;
      float _Posterize;

      inline half4 rgb2cmyki(half4 pixel)
      {
        float k = max(max(pixel.r, pixel.g), pixel.b);

        return min(half4(pixel.rgb / k, k), 1.0);
      }

      inline half4 cmyki2rgb(half4 pixel)
      {
        return half4(pixel.rgb * pixel.a, pixel.a);
      }

      inline float2 Grid(float2 uv)
      {
        return floor(uv / _Scale) * _Scale;
      }

      inline half4 Smoothstep(half4 v)
      {
        const float SST = 0.888;
        const float SSQ = 0.288;

        return smoothstep(SST - SSQ, SST + SSQ, v);
      }

      inline half4 Halftone(float2 fc, float2x2 m)
      {
        float2 smp = mul(Grid(mul(m, fc)) + 0.5 * _Scale, m);
        float s = min(length(fc - smp) / (0.74 * _Scale), 1.0);
        float4 c = rgb2cmyki(SAMPLE_MAIN((smp / _ScreenParams.xy) + float2(0.5, 0.5)));

        return c + s;
      }

      inline float2x2 Rotate2D(float theta)
      {
        float st = sin(theta);
        float ct = cos(theta);
        
        return float2x2(ct, st, -st, ct);
      }

      inline float Edgeness(float2 uv)
      {
        float f = dot(SAMPLE_MAIN(uv).rgb, (float3)1.0);
        float2 grad = float2(dot(SAMPLE_MAIN(uv + _Edge * float2(TEXEL_SIZE.x, 0.0)).rgb, (float3)1.0),
                             dot(SAMPLE_MAIN(uv + _Edge * float2(0.0, TEXEL_SIZE.y)).rgb, (float3)1.0)) - f;

        return length(grad);
      }

      half4 ArtisticFrag(VertexOutput input) : SV_Target0
      {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord).xy;

        const half4 color = SAMPLE_MAIN(uv);
        half4 pixel = color;

        pixel = cmyki2rgb(Smoothstep(half4(Halftone(input.screenPos, Rotate2D(_CMYKPattern.x)).r,
                                            Halftone(input.screenPos, Rotate2D(_CMYKPattern.y)).g,
                                            Halftone(input.screenPos, Rotate2D(_CMYKPattern.z)).b,
                                            Halftone(input.screenPos, Rotate2D(_CMYKPattern.w)).a)));

        pixel.rgb = ColorBlend(_ColorBlend, color.rgb, pixel.rgb);

        // Color adjust.
        pixel.rgb = ColorAdjust(pixel.rgb, _Contrast, _Brightness, _Hue, _Gamma, _Saturation);

        float edge = smoothstep(0.75, 0.0, Edgeness(uv));
        half3 pixeledge = ColorBlend(_EdgeBlendOp, color.rgb, _EdgeColor.rgb * (1.0 - edge));

        pixel.rgb = lerp(pixel.rgb, pixeledge, 1.0 - edge);

        return lerp(color, pixel, _Intensity);
      }

      ENDHLSL
    }
  }
  
  FallBack "Diffuse"
}
