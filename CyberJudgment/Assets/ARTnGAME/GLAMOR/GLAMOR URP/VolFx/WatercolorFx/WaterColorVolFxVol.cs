using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Artngame.GLAMOR.VolFx
{
    [Serializable, VolumeComponentMenu("VolFx/WaterColorVolFx")]
    public sealed class WaterColorVolFxVol : VolumeComponent, IPostProcessComponent
    {

		//[Tooltip("Gradient Threshold")]
		//public NoInterpClampedFloatParameter gradThreshold = new NoInterpClampedFloatParameter(0.01f, 0.00001f, 0.01f);
		//[Tooltip("Color Threshold")]
		//public NoInterpClampedFloatParameter colorThreshold = new NoInterpClampedFloatParameter(0.8f, 0f, 1f);
		//public NoInterpClampedFloatParameter blendThreshold = new NoInterpClampedFloatParameter(0.8f, -0.3f, 1f);
		//public NoInterpClampedFloatParameter blendScreenThreshold = new NoInterpClampedFloatParameter(0.8f, -0.3f, 1f);
		//public NoInterpClampedFloatParameter sensivity = new NoInterpClampedFloatParameter(10f, 0f, 100f);

		public NoInterpColorParameter fillColor = new NoInterpColorParameter(new Color(0.9f, 0.8f, 1f, 0.8f));
		public NoInterpColorParameter edgeColor = new NoInterpColorParameter(new Color(0.1f, 0.1f, 0.3f));
		public NoInterpFloatParameter edgeContrast = new NoInterpFloatParameter(1f);
		public NoInterpFloatParameter blurWidth = new NoInterpFloatParameter(0.1f);
		public NoInterpFloatParameter blurFrequency = new NoInterpFloatParameter(0.2f);
		public NoInterpFloatParameter hueShift = new NoInterpFloatParameter(0.2f);
		public NoInterpFloatParameter interval = new NoInterpFloatParameter(0.5f);
		public NoInterpIntParameter iteration = new NoInterpIntParameter(20);

		public Texture2DParameter _NoiseTex = new Texture2DParameter(null);//


		//[Range(0, 10)]
		//public NoInterpFloatParameter intensity = new NoInterpFloatParameter(1f);

		//public NoInterpColorParameter bloomTint = new NoInterpColorParameter(Color.white);

		//[Range(1, 25)]
		//public NoInterpIntParameter blurIterations = new NoInterpIntParameter(1);

		//[Range(0, 1)]
		//public NoInterpFloatParameter blendFac = new NoInterpFloatParameter(0.5f);

		//[Range(0, 0.999f)]
		//public NoInterpFloatParameter ghostingAmount = new NoInterpFloatParameter(0.95f);
		//[Tooltip("Just play with this lol.")]

		//[Range(0, 100f)]
		//public NoInterpFloatParameter distanceMultiplier = new NoInterpFloatParameter(1f);

		////[Tooltip("Higher value means lower resolution buffer and therefore better performance. If not using ghosting it causes flickering.")]

		//[Range(1, 16)]
		//public NoInterpIntParameter downSampleFactor = new NoInterpIntParameter(16);

		//[SerializeField]
		//Shader _shader;
		//Material _material;

		public ClampedFloatParameter m_Intencity = new ClampedFloatParameter(0, 0, 1);
       
        public bool IsActive() => active && m_Intencity.value > 0f;

        public bool IsTileCompatible() => false;
    }
}