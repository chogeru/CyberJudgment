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
using UnityEngine;
using UnityEditor;
using static FronkonGames.Artistic.Comic.Inspector;

namespace FronkonGames.Artistic.Comic.Editor
{
  /// <summary> Artistic Comic inspector. </summary>
  [CustomPropertyDrawer(typeof(Comic.Settings))]
  public class ComicFeatureSettingsDrawer : Drawer
  {
    private Comic.Settings settings;

    protected override void ResetValues() => settings?.ResetDefaultValues();

    protected override void InspectorGUI()
    {
      settings ??= GetSettings<Comic.Settings>();

      /////////////////////////////////////////////////
      // Common.
      /////////////////////////////////////////////////
      settings.intensity = Slider("Intensity", "Controls the intensity of the effect [0, 1]. Default 0.", settings.intensity, 0.0f, 1.0f, 1.0f);

      /////////////////////////////////////////////////
      // Comic.
      /////////////////////////////////////////////////
      Separator();
      
      settings.scale = Slider("Scale", "Scale of dots [0, 1]. Default 0.2.", settings.scale, 0.01f, 2.0f, 0.2f);
      settings.colorBlend = (ColorBlends)EnumPopup("Color blend", "Color blend operation. Default Solid.", settings.colorBlend, ColorBlends.Solid);
      settings.edge = Slider("Edge", "Edge enhancement intensity [0, 10]. Default 3.", settings.edge, 0.0f, 10.0f, 3.0f);
      IndentLevel++;
      settings.edgeColorBlend = (ColorBlends)EnumPopup("Color blend", ".", settings.edgeColorBlend, ColorBlends.Solid);
      settings.edgeColor = ColorField("Tint", "Edge color tint. Default Black.", settings.edgeColor, Color.black);
      IndentLevel--;

      Label("CMYK pattern");
      IndentLevel++;
      settings.cmykPattern.x = Slider("Cyan", "Pattern angle.", settings.cmykPattern.x, 0.0f, 360.0f, Comic.Settings.DefaultPattern.x);
      settings.cmykPattern.y = Slider("Magenta", "Pattern angle.", settings.cmykPattern.y, 0.0f, 360.0f, Comic.Settings.DefaultPattern.y);
      settings.cmykPattern.z = Slider("Yellow", "Pattern angle.", settings.cmykPattern.z, 0.0f, 360.0f, Comic.Settings.DefaultPattern.z);
      settings.cmykPattern.w = Slider("Black", "Pattern angle.", settings.cmykPattern.w, 0.0f, 360.0f, Comic.Settings.DefaultPattern.w);
      IndentLevel--;

      /////////////////////////////////////////////////
      // Color.
      /////////////////////////////////////////////////
      Separator();

      if (Foldout("Color") == true)
      {
        IndentLevel++;

        settings.brightness = Slider("Brightness", "Brightness [-1.0, 1.0]. Default 0.", settings.brightness, -1.0f, 1.0f, 0.0f);
        settings.contrast = Slider("Contrast", "Contrast [0.0, 10.0]. Default 1.", settings.contrast, 0.0f, 10.0f, 1.0f);
        settings.gamma = Slider("Gamma", "Gamma [0.1, 10.0]. Default 1.", settings.gamma, 0.01f, 10.0f, 1.0f);
        settings.hue = Slider("Hue", "The color wheel [0.0, 1.0]. Default 0.", settings.hue, 0.0f, 1.0f, 0.0f);
        settings.saturation = Slider("Saturation", "Intensity of a colors [0.0, 2.0]. Default 1.", settings.saturation, 0.0f, 2.0f, 1.0f);

        IndentLevel--;
      }

      /////////////////////////////////////////////////
      // Advanced.
      /////////////////////////////////////////////////
      Separator();

      if (Foldout("Advanced") == true)
      {
        IndentLevel++;

#if !UNITY_6000_0_OR_NEWER
        settings.affectSceneView = Toggle("Affect the Scene View?", "Does it affect the Scene View?", settings.affectSceneView);
        settings.filterMode = (FilterMode)EnumPopup("Filter mode", "Filter mode. Default Bilinear.", settings.filterMode, FilterMode.Bilinear);
#endif
        settings.whenToInsert = (UnityEngine.Rendering.Universal.RenderPassEvent)EnumPopup("RenderPass event",
          "Render pass injection. Default BeforeRenderingPostProcessing.",
          settings.whenToInsert,
          UnityEngine.Rendering.Universal.RenderPassEvent.BeforeRenderingPostProcessing);
#if !UNITY_6000_0_OR_NEWER
        settings.enableProfiling = Toggle("Enable profiling", "Enable render pass profiling", settings.enableProfiling);
#endif

        IndentLevel--;
      }
    }
  }
}
