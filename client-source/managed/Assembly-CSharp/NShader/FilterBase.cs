using System;
using System.Collections.Generic;
using LitJson;
using NVespa;
using UnityEngine;

namespace NShader;

public abstract class FilterBase : IFilter
{
	public static LocalStorageBool CalcByHeight;

	private static Material _blitMaterial;

	private static List<string> _filterNameList;

	private static Dictionary<string, Func<FilterBase>> _createDic;

	public static Material BlitMaterial
	{
		get
		{
			if (_blitMaterial == null)
			{
				_blitMaterial = new Material(Shader.Find("Vespa/Filter/NoFilter"));
				_blitMaterial.hideFlags = HideFlags.DontSave;
			}
			return _blitMaterial;
		}
	}

	public static RenderTexture CreateScreenRenderTarget()
	{
		SingletonAuto<ResolutionInfo>.instance.GetScreenRenderTargetSize(out var width, out var height);
		return CreateRenderTarget(width, height);
	}

	public static RenderTexture CreateRenderTarget(int width, int height, int depthBuffer = 24)
	{
		return SprkGraphicsOptions.CreateNativeTarget(width, height, depthBuffer);
	}

	public static RenderTexture CreateRenderTargetWithAntiAliasing(int width, int height, int depthBuffer, int antiAliasing)
	{
		return new RenderTexture(width, height, depthBuffer)
		{
			filterMode = FilterMode.Bilinear,
			antiAliasing = antiAliasing
		};
	}

	public static RenderTexture GetTemporaryRT(int width, int height, int depthBuffer, RenderTextureFormat format)
	{
		RenderTexture temporary = RenderTexture.GetTemporary(width, height, depthBuffer, format);
		temporary.DiscardContents();
		temporary.filterMode = FilterMode.Bilinear;
		return temporary;
	}

	public static void Blit(RenderTexture source, RenderTexture dest, Material mat)
	{
		if (source == dest)
		{
			RenderTexture temporaryRT = GetTemporaryRT(source.width, source.height, 0, source.format);
			Graphics.Blit(source, temporaryRT, mat);
			Graphics.Blit(temporaryRT, dest, BlitMaterial);
			RenderTexture.ReleaseTemporary(temporaryRT);
		}
		else
		{
			Graphics.Blit(source, dest, mat);
		}
	}

	public static void Blit(RenderTexture source, RenderTexture dest, Material mat, int pass)
	{
		if (source == dest)
		{
			RenderTexture temporaryRT = GetTemporaryRT(source.width, source.height, 0, source.format);
			Graphics.Blit(source, temporaryRT, mat, pass);
			Graphics.Blit(temporaryRT, dest, BlitMaterial);
			RenderTexture.ReleaseTemporary(temporaryRT);
		}
		else
		{
			Graphics.Blit(source, dest, mat, pass);
		}
	}

	void IFilter.ImageProcess(RenderTexture source, RenderTexture dest, bool discardDest)
	{
		ImageProcess(source, dest, discardDest);
	}

	public void ImageProcess(RenderTexture source, RenderTexture dest, bool discardDest)
	{
		if (discardDest && dest != null)
		{
			dest.DiscardContents();
		}
		ImageProcessDerived(source, dest);
	}

	protected abstract void ImageProcessDerived(RenderTexture source, RenderTexture dest);

	protected abstract bool LoadBodyFromJson(JsonData json);

	protected abstract JsonData MakeBodyJson();

	public JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("TypeName", GetType().Name);
		jsonData.AddField("Body", MakeBodyJson());
		return jsonData;
	}

	static FilterBase()
	{
		CalcByHeight = new LocalStorageBool(i_default: true, typeof(FilterBase).FullName + "CalcByHeight");
		_filterNameList = new List<string>();
		_createDic = new Dictionary<string, Func<FilterBase>>();
		AddCreateFunc<BlurFilter>(BlurFilter.CreateFilter);
		AddCreateFunc<ColorCorrectionFilter>(ColorCorrectionFilter.CreateFilter);
		AddCreateFunc<GrayscaleFilter>(GrayscaleFilter.CreateFilter);
		AddCreateFunc<SepiaToneFilter>(SepiaToneFilter.CreateFilter);
		AddCreateFunc<SmoothFilter>(SmoothFilter.CreateFilter);
		AddCreateFunc<BloomFilter>(BloomFilter.CreateFilter);
	}

	private static void AddCreateFunc<T>(Func<FilterBase> createFunc) where T : FilterBase, new()
	{
		_createDic.Add(typeof(T).Name, createFunc);
		_filterNameList.Add(typeof(T).Name);
	}

	public static FilterBase CreateFromJson(JsonData json)
	{
		return CreateFromJson(json, null);
	}

	public static FilterBase CreateFromJson(JsonData json, FilterBase defaultFilter)
	{
		if (!json.IsObject())
		{
			return defaultFilter;
		}
		FilterBase filterBase = CreateFromTypeName(json["TypeName"].GETSTRING(string.Empty));
		if (filterBase == null)
		{
			return defaultFilter;
		}
		if (!filterBase.LoadBodyFromJson(json["Body"]))
		{
			return defaultFilter;
		}
		return filterBase;
	}

	public static FilterBase CreateFromTypeName(string typeName)
	{
		if (String_Extension_Method.IsNullOrEmpty(typeName))
		{
			return null;
		}
		Func<FilterBase> vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_createDic, typeName, null);
		if (vALUE == null)
		{
			return null;
		}
		return Vespa_System_Action_Extension_Method.fire(vALUE);
	}
}
