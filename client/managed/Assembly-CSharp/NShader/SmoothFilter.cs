using LitJson;
using UnityEngine;

namespace NShader;

public class SmoothFilter : FilterBase
{
	private static class FilterProcess
	{
		private static Material _filterMaterial;

		static FilterProcess()
		{
			_filterMaterial = new Material(Shader.Find("Vespa/Filter/SmoothFilter"));
			_filterMaterial.hideFlags = HideFlags.DontSave;
		}

		public static void ImageProcess(SmoothFilter filter, RenderTexture source, RenderTexture dest)
		{
			_filterMaterial.SetFloat("_SmoothWeight", filter.Weight);
			_filterMaterial.SetFloat("_SmoothOffsetX", 1f / (float)source.width);
			_filterMaterial.SetFloat("_SmoothOffsetY", 1f / (float)source.height);
			int width = Mathf.Min(1920, source.width);
			int height = Mathf.Min(1920, source.height);
			RenderTexture renderTexture = source;
			for (int i = 0; i < filter.Iterations; i++)
			{
				RenderTexture temporaryRT = FilterBase.GetTemporaryRT(width, height, 0, source.format);
				FilterBase.Blit(renderTexture, temporaryRT, _filterMaterial);
				if (renderTexture != source)
				{
					RenderTexture.ReleaseTemporary(renderTexture);
				}
				renderTexture = temporaryRT;
			}
			FilterBase.Blit(renderTexture, dest, FilterBase.BlitMaterial);
			if (renderTexture != source)
			{
				RenderTexture.ReleaseTemporary(renderTexture);
			}
		}
	}

	public int Iterations = 3;

	public float Weight = 0.2f;

	public static SmoothFilter CreateFilter()
	{
		return new SmoothFilter();
	}

	protected override void ImageProcessDerived(RenderTexture source, RenderTexture dest)
	{
		FilterProcess.ImageProcess(this, source, dest);
	}

	protected override bool LoadBodyFromJson(JsonData json)
	{
		Iterations = json["Iterations"].GETINT(Iterations);
		Weight = json["Weight"].GETFLOAT(Weight);
		return true;
	}

	protected override JsonData MakeBodyJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldLong("Iterations", Iterations);
		jsonData.AddFieldDouble("Weight", Weight);
		return jsonData;
	}
}
