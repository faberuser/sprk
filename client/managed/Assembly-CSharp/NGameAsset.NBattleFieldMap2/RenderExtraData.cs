using System.Collections.Generic;
using JsonBin;
using UnityEngine;

namespace NGameAsset.NBattleFieldMap2;

public class RenderExtraData : ILoadJson
{
	public string RendererPath = string.Empty;

	public bool UseManualScale;

	public float ManualScale = 1f;

	public bool IsBillboard;

	public bool IsBackground;

	public float BackgroundDepth;

	public bool IsLoop;

	public float LoopLength = 10f;

	private GameObject _ownerAsset;

	public Renderer RenderComponent { get; private set; }

	public Material SharedMaterial
	{
		get
		{
			if (RenderComponent == null)
			{
				return null;
			}
			return RenderComponent.sharedMaterial;
		}
	}

	public RenderExtraData(GameObject asset)
	{
		_ownerAsset = asset;
	}

	public RenderExtraData(GameObject asset, string rendererPath)
	{
		_ownerAsset = asset;
		RendererPath = rendererPath;
		Transform transform = asset.transform.Find(RendererPath);
		if (!(transform == null))
		{
			RenderComponent = transform.gameObject.GetComponent<Renderer>();
		}
	}

	public float GetFinalScale()
	{
		if (UseManualScale)
		{
			return Mathf.Max(ManualScale, 0.01f);
		}
		return CalcScaleByDepth();
	}

	private float CalcScaleByDepth()
	{
		if (!IsBackground)
		{
			return 1f;
		}
		float num = 25f;
		return (num + BackgroundDepth) / num;
	}

	bool ILoadJson.Load(JsonRead json)
	{
		RendererPath = json["RendererPath"].GETSTRING(RendererPath);
		UseManualScale = json["UseManualScale"].GETBOOLEAN(UseManualScale);
		ManualScale = json["ManualScale"].GETFLOAT(ManualScale);
		IsBillboard = json["IsBillboard"].GETBOOLEAN(IsBillboard);
		IsBackground = json["IsBackground"].GETBOOLEAN(IsBackground);
		BackgroundDepth = json["BackgroundDepth"].GETFLOAT(BackgroundDepth);
		IsLoop = json["IsLoop"].GETBOOLEAN(IsLoop);
		LoopLength = json["LoopLength"].GETFLOAT(LoopLength);
		if (_ownerAsset != null)
		{
			Transform transform = _ownerAsset.transform.Find(RendererPath);
			if (transform != null)
			{
				RenderComponent = transform.gameObject.GetComponent<Renderer>();
			}
		}
		return true;
	}

	public static void LoadList(JsonRead json, List<RenderExtraData> list)
	{
		list.Clear();
		if (!json.IsArray())
		{
			return;
		}
		for (int i = 0; i < json.Count; i++)
		{
			JsonRead fromArray = json.GetFromArray(i);
			RenderExtraData renderExtraData = new RenderExtraData(null);
			if (renderExtraData.LOADFROMJSON(fromArray))
			{
				list.Add(renderExtraData);
			}
		}
	}
}
