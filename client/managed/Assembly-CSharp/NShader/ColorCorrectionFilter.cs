using LitJson;
using UnityEngine;

namespace NShader;

public class ColorCorrectionFilter : FilterBase
{
	private static class FilterProcess
	{
		private static AnimationCurve _channelR;

		private static AnimationCurve _channelG;

		private static AnimationCurve _channelB;

		private static Material _filterMaterial;

		private static Texture2D _rgbChannelTex;

		static FilterProcess()
		{
			_filterMaterial = new Material(Shader.Find("Vespa/Filter/ColorCorrectionFilter"));
			_filterMaterial.hideFlags = HideFlags.DontSave;
			_rgbChannelTex = new Texture2D(256, 1, TextureFormat.ARGB32, mipChain: false, linear: true);
			_rgbChannelTex.hideFlags = HideFlags.DontSave;
			_rgbChannelTex.wrapMode = TextureWrapMode.Clamp;
		}

		public static void ImageProcess(ColorCorrectionFilter filter, RenderTexture source, RenderTexture dest)
		{
			UpdateChannelTex(filter);
			_filterMaterial.SetTexture("_RgbTex", _rgbChannelTex);
			_filterMaterial.SetFloat("_Saturation", filter.Saturation);
			FilterBase.Blit(source, dest, _filterMaterial);
		}

		private static void UpdateChannelTex(ColorCorrectionFilter filter)
		{
			if (_channelR != filter.ChannelR || _channelG != filter.ChannelG || _channelB != filter.ChannelB)
			{
				_channelR = filter.ChannelR;
				_channelG = filter.ChannelG;
				_channelB = filter.ChannelB;
				for (int i = 0; i < 256; i++)
				{
					float num = (float)i / 255f;
					float r = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(filter.ChannelR, num, num);
					float g = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(filter.ChannelG, num, num);
					float b = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(filter.ChannelB, num, num);
					_rgbChannelTex.SetPixel(i, 0, new Color(r, g, b));
				}
				_rgbChannelTex.Apply();
			}
		}
	}

	public AnimationCurve ChannelR = new AnimationCurve();

	public AnimationCurve ChannelG = new AnimationCurve();

	public AnimationCurve ChannelB = new AnimationCurve();

	public float Saturation = 1f;

	public static ColorCorrectionFilter CreateFilter()
	{
		return new ColorCorrectionFilter();
	}

	protected override void ImageProcessDerived(RenderTexture source, RenderTexture dest)
	{
		FilterProcess.ImageProcess(this, source, dest);
	}

	protected override bool LoadBodyFromJson(JsonData json)
	{
		ChannelR = json["ChannelR"].GetAnimationCurve(ChannelR);
		ChannelG = json["ChannelG"].GetAnimationCurve(ChannelG);
		ChannelB = json["ChannelB"].GetAnimationCurve(ChannelB);
		Saturation = json["Saturation"].GETFLOAT(Saturation);
		return true;
	}

	protected override JsonData MakeBodyJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldAnimationCurve("ChannelR", ChannelR);
		jsonData.AddFieldAnimationCurve("ChannelG", ChannelG);
		jsonData.AddFieldAnimationCurve("ChannelB", ChannelB);
		jsonData.AddFieldDouble("Saturation", Saturation);
		return jsonData;
	}
}
