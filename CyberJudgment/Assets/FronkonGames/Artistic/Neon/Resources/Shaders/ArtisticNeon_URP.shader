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
Shader "Hidden/Fronkon Games/Artistic/Neon URP"
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
      Name "Fronkon Games Artistic Neon"

      HLSLPROGRAM
      #pragma vertex ArtisticVert
      #pragma fragment ArtisticFrag
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma exclude_renderers d3d9 d3d11_9x ps3 flash
      #pragma multi_compile_fog
      #pragma multi_compile _ _USE_DRAW_PROCEDURAL
      #pragma multi_compile ___ PROCESS_DEPTH
      //#pragma multi_compile ___ ARTISTIC_DEMO
      #define USE_FOG

      #include "Artistic.hlsl"
      #include "ColorBlend.hlsl"

      float _Strength;
      int _Radius;
      int _Blend;
      float _Speed;
      float _Fisheye;
#if PROCESS_DEPTH
      int _SampleSky;
      float _DepthPower;
#endif

      inline half3 HSV2RGBSmooth(half3 pixel)
      {
        half3 rgb = clamp(abs(mod(pixel.r * 6.0 + float3(0.0, 4.0, 2.0), 6.0) - 3.0) - 1.0, 0.0, 1.0);
        
        return pixel.b * lerp((float3)1.0, rgb, pixel.g);
      }

      inline float Kernel(int a, int b)
      {
        float radius = (float)_Radius;

        return float(a) * exp(-float(a * a + b * b) / (radius * radius)) / radius;
      }

      inline float2 Fisheye(float2 uv)
      {
        float ratio = _ScreenParams.x / _ScreenParams.y;
        float2 m = float2(0.5, 0.5 / ratio);
        float2 d = uv - m;
        float r = sqrt(dot(d, d));

        float power = (2.0 * PI / (2.0 * sqrt(dot(m, m)))) * _Fisheye;
        float bind;
        
        UNITY_BRANCH
        if (power > 0.0)
          bind = sqrt(dot(m, m));
        else
          bind = ratio < 1.0 ? m.x : m.y;

        UNITY_BRANCH
        if (power > 0.0)
          uv = m + normalize(d) * tan(r * power) * bind / tan(bind * power);
        else if (power < 0.0)
          uv = m + normalize(d) * atan(r * -power) * bind / atan(-power * bind);

        return uv;
      }

      inline half3 Neon(half3 pixel, float2 uv)
      {
        float3 colorX = 0.0, colorY = 0.0;

        UNITY_LOOP
        for (int i = -_Radius ; i <= _Radius; i++)
        {
          UNITY_LOOP
          for (int j = -_Radius ; j <= _Radius; j++)
          {
            pixel = SafePositivePow(max(SAMPLE_MAIN_LOD(uv + float2(i, j) * TEXEL_SIZE.xy).rgb, 0.0), (float3)2.2);
            colorX += Kernel(i, j) * pixel;
            colorY += Kernel(j, i) * pixel;
          }
        }

        const float radius = (float)_Radius;
        float3 derivative = sqrt((colorX * colorX + colorY * colorY)) / (radius * radius);

        float angle = atan2(Luminance601(colorY), Luminance601(colorX)) / (2.0 * PI) + _Time.y * _Speed / 2.0;
        pixel = HSV2RGBSmooth(float3(angle, 1.0, SafePositivePow_float(Luminance601(derivative) * 30.0 * _Strength, 3.0) * 5.0));
        pixel = SafePositivePow(max(pixel, (float3)0.0), (float3)(1.0 / 2.2));

        return pixel;
      }

      half4 ArtisticFrag(const Varyings input) : SV_Target
      {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord).xy;

        const half4 color = SAMPLE_MAIN(uv);
        half4 pixel = color;

        uv = Fisheye(uv);

#if PROCESS_DEPTH
        const float depth = SampleLinear01Depth(uv);
        float depthPower = 1.0 - log10(depth * _DepthPower);

        half3 neon = Neon(pixel.rgb, uv) * depthPower;
        UNITY_BRANCH
        if (_SampleSky == 1)
          pixel.rgb = neon;
        else
          pixel.rgb = depth < 0.98 ? neon : color.rgb;
#else
        pixel.rgb = Neon(pixel.rgb, uv);
#endif

        pixel.rgb = ColorBlend(_Blend,
                               ColorAdjust(color.rgb, _Contrast, _Brightness, _Hue, _Gamma, _Saturation),
                               pixel.rgb);

#if ARTISTIC_DEMO
        pixel.rgb = PixelDemo(color.rgb, pixel.rgb, uv);
#endif
        return lerp(color, pixel, _Intensity);
      }

      ENDHLSL
    }
  }
  
  FallBack "Diffuse"
}
