using LitJson;
using UnityEngine;

namespace NShader;

public class SepiaToneFilter : FilterBase
{
	private static class FilterProcess
	{
		private static Material _filterMaterial;

		static FilterProcess()
		{
			_filterMaterial = new Material(Shader.Find("Vespa/Filter/SepiaToneFilter"));
			_filterMaterial.hideFlags = HideFlags.DontSave;
		}

		public static void ImageProcess(RenderTexture source, RenderTexture dest)
		{
			FilterBase.Blit(source, dest, _filterMaterial);
		}
	}

	private static SepiaToneFilter _default = new SepiaToneFilter();

	public static SepiaToneFilter CreateFilter()
	{
		return _default;
	}

	protected override void ImageProcessDerived(RenderTexture source, RenderTexture dest)
	{
		FilterProcess.ImageProcess(source, dest);
	}

	protected override bool LoadBodyFromJson(JsonData json)
	{
		return true;
	}

	protected override JsonData MakeBodyJson()
	{
		return new JsonData();
	}
}
