using System.Collections.Generic;
using LitJson;
using UnityEngine;

namespace NShader;

public class ChainFilter : IBuilder, ILoadJson, IJsonInterface, IMakeJson, IFilter
{
	private List<FilterBase> _filterList = new List<FilterBase>();

	public ChainFilter()
	{
	}

	public ChainFilter(FilterBase initFilter)
	{
		if (initFilter != null)
		{
			_filterList.Add(initFilter);
		}
	}

	public void Reset(FilterBase filter)
	{
		_filterList.Clear();
		_filterList.Add(filter);
	}

	void IFilter.ImageProcess(RenderTexture source, RenderTexture dest, bool discardDest)
	{
		ImageProcess(source, dest, discardDest);
	}

	public void ImageProcess(RenderTexture source, RenderTexture dest, bool discardDest)
	{
		if (dest != null && discardDest)
		{
			dest.DiscardContents();
		}
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_filterList))
		{
			FilterBase.Blit(source, dest, FilterBase.BlitMaterial);
			return;
		}
		RenderTexture renderTexture = source;
		for (int i = 0; i < _filterList.Count; i++)
		{
			RenderTexture renderTexture2 = ((i >= _filterList.Count - 1) ? dest : FilterBase.GetTemporaryRT(source.width, source.height, 0, source.format));
			_filterList[i].ImageProcess(renderTexture, renderTexture2, discardDest: false);
			if (renderTexture != source)
			{
				RenderTexture.ReleaseTemporary(renderTexture);
			}
			renderTexture = renderTexture2;
		}
		if (renderTexture != source && renderTexture != dest)
		{
			RenderTexture.ReleaseTemporary(renderTexture);
		}
	}

	bool ILoadJson.Load(JsonData json)
	{
		if (!json.IsArray())
		{
			return false;
		}
		_filterList.Clear();
		for (int i = 0; i < json.Count; i++)
		{
			FilterBase filterBase = FilterBase.CreateFromJson(json.GetFromArray(i));
			if (filterBase != null)
			{
				_filterList.Add(filterBase);
			}
		}
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		for (int i = 0; i < _filterList.Count; i++)
		{
			JsonData data = _filterList[i].MakeJson();
			jsonData.Add(data);
		}
		return jsonData;
	}
}
