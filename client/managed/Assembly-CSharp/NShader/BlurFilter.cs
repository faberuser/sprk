using LitJson;
using UnityEngine;

namespace NShader;

public class BlurFilter : FilterBase
{
	private static class FilterProcess
	{
		private static Material _filterMaterial;

		static FilterProcess()
		{
			_filterMaterial = new Material(Shader.Find("Vespa/Filter/BlurFilter"));
			_filterMaterial.hideFlags = HideFlags.DontSave;
		}

		public static void ImageProcess(BlurFilter filter, RenderTexture source, RenderTexture dest)
		{
			int num = 1 << filter.ScaleDown;
			float num2 = Mathf.Min(1f, 2f / (float)num);
			_filterMaterial.SetFloat("_Parameter", filter.BlurSize * num2);
			source.filterMode = FilterMode.Bilinear;
			int width = Mathf.Min(1920, source.width) / num;
			int height = Mathf.Min(1920, source.height) / num;
			RenderTexture renderTexture = FilterBase.GetTemporaryRT(width, height, 0, source.format);
			renderTexture.filterMode = FilterMode.Bilinear;
			FilterBase.Blit(source, renderTexture, _filterMaterial, 1);
			for (int i = 0; i < Mathf.Min(5, filter.BlurIterations); i++)
			{
				_filterMaterial.SetFloat("_Parameter", filter.BlurSize * num2 + (float)i);
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
			FilterBase.Blit(renderTexture, dest, _filterMaterial, 0);
			RenderTexture.ReleaseTemporary(renderTexture);
		}
	}

	public float BlurSize = 2f;

	public int ScaleDown = 2;

	public int BlurIterations = 3;

	public static BlurFilter CreateFilter()
	{
		return new BlurFilter();
	}

	protected override void ImageProcessDerived(RenderTexture source, RenderTexture dest)
	{
		FilterProcess.ImageProcess(this, source, dest);
	}

	protected override bool LoadBodyFromJson(JsonData json)
	{
		BlurSize = json["BlurSize"].GETFLOAT(BlurSize);
		ScaleDown = json["ScaleDown"].GETINT(ScaleDown);
		BlurIterations = json["BlurIterations"].GETINT(BlurIterations);
		return true;
	}

	protected override JsonData MakeBodyJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldDouble("BlurSize", BlurSize);
		jsonData.AddFieldLong("ScaleDown", ScaleDown);
		jsonData.AddFieldLong("BlurIterations", BlurIterations);
		return jsonData;
	}
}
