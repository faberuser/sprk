using LitJson;
using UnityEngine;

namespace NShader;

public class BloomFilter : FilterBase
{
	private static class FilterProcess
	{
		private static Material _filterMaterial;

		static FilterProcess()
		{
			_filterMaterial = new Material(Shader.Find("Vespa/Filter/BloomFilter"));
			_filterMaterial.hideFlags = HideFlags.DontSave;
		}

		public static void ImageProcess(BloomFilter filter, RenderTexture source, RenderTexture dest)
		{
			int num = 1 << filter._usingScaleDown;
			float num2 = Mathf.Min(1f, 2f / (float)num);
			_filterMaterial.SetVector("_Parameter", new Vector4(filter.BlurSize * num2, filter.OriginalIntensity, filter.Threshhold, filter.BloomIntensity));
			source.filterMode = FilterMode.Bilinear;
			int width = Mathf.Min(1920, source.width) / num;
			int height = Mathf.Min(1920, source.height) / num;
			RenderTexture renderTexture = FilterBase.GetTemporaryRT(width, height, 0, source.format);
			renderTexture.filterMode = FilterMode.Bilinear;
			FilterBase.Blit(source, renderTexture, _filterMaterial, 1);
			for (int i = 0; i < Mathf.Min(5, filter._usingBlurIterations); i++)
			{
				_filterMaterial.SetVector("_Parameter", new Vector4(filter.BlurSize * num2 + (float)i, filter.OriginalIntensity, filter.Threshhold, filter.BloomIntensity));
				RenderTexture temporaryRT = FilterBase.GetTemporaryRT(width, height, 0, source.format);
				temporaryRT.filterMode = FilterMode.Bilinear;
				FilterBase.Blit(renderTexture, temporaryRT, _filterMaterial, 2);
				RenderTexture.ReleaseTemporary(renderTexture);
				renderTexture = temporaryRT;
				temporaryRT = FilterBase.GetTemporaryRT(width, height, 0, source.format);
				temporaryRT.filterMode = FilterMode.Bilinear;
				FilterBase.Blit(renderTexture, temporaryRT, _filterMaterial, 3);
				RenderTexture.ReleaseTemporary(renderTexture);
				renderTexture = temporaryRT;
			}
			if (source != dest)
			{
				_filterMaterial.SetTexture("_Bloom", renderTexture);
				FilterBase.Blit(source, dest, _filterMaterial, 0);
			}
			else
			{
				FilterBase.Blit(renderTexture, dest, _filterMaterial, 4);
			}
			RenderTexture.ReleaseTemporary(renderTexture);
		}
	}

	public static bool UseGlobalVariable = false;

	public static int GlobalBlurIterations = 1;

	public static int GlobalScaleDown = 1;

	public float Threshhold = 0.8f;

	public float OriginalIntensity = 1f;

	public float BloomIntensity = 3f;

	public float BlurSize = 2f;

	public int ScaleDown = 2;

	public int BlurIterations = 2;

	private int _usingBlurIterations
	{
		get
		{
			if (!UseGlobalVariable)
			{
				return BlurIterations;
			}
			return GlobalBlurIterations;
		}
	}

	private int _usingScaleDown
	{
		get
		{
			if (!UseGlobalVariable)
			{
				return ScaleDown;
			}
			return GlobalScaleDown;
		}
	}

	public static BloomFilter CreateFilter()
	{
		return new BloomFilter();
	}

	protected override void ImageProcessDerived(RenderTexture source, RenderTexture dest)
	{
		FilterProcess.ImageProcess(this, source, dest);
	}

	protected override bool LoadBodyFromJson(JsonData json)
	{
		Threshhold = json["Threshhold"].GETFLOAT(Threshhold);
		OriginalIntensity = json["OriginalIntensity"].GETFLOAT(OriginalIntensity);
		BloomIntensity = json["BloomIntensity"].GETFLOAT(BloomIntensity);
		BlurSize = json["BlurSize"].GETFLOAT(BlurSize);
		ScaleDown = json["ScaleDown"].GETINT(ScaleDown);
		BlurIterations = json["BlurIterations"].GETINT(BlurIterations);
		return true;
	}

	protected override JsonData MakeBodyJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldDouble("Threshhold", Threshhold);
		jsonData.AddFieldDouble("OriginalIntensity", OriginalIntensity);
		jsonData.AddFieldDouble("BloomIntensity", BloomIntensity);
		jsonData.AddFieldDouble("BlurSize", BlurSize);
		jsonData.AddFieldLong("ScaleDown", ScaleDown);
		jsonData.AddFieldLong("BlurIterations", BlurIterations);
		return jsonData;
	}
}
