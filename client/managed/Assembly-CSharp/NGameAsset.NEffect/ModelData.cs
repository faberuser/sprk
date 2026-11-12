using System.Collections.Generic;
using JsonBin;
using NGameAsset.NAsyncLoad;
using NVespa;
using UnityEngine;

namespace NGameAsset.NEffect;

public class ModelData : ILoadJson, IModelData
{
	public const int ReserveCount = 1;

	public string Name = string.Empty;

	public string BundleFileName = string.Empty;

	public string AssetName = string.Empty;

	public CommonData Common = new CommonData();

	public List<PlaySequenceData> PlaySequenceList = new List<PlaySequenceData>();

	public EndingDisappearData EndingDisappear = new EndingDisappearData();

	public List<RendererExtraData> RendererExtraList = new List<RendererExtraData>();

	public AssetDataAsync<GameObject, ModelData> CreateAssetAsync()
	{
		AssetDataAsync<GameObject, ModelData> assetDataAsync = new AssetDataAsync<GameObject, ModelData>();
		assetDataAsync.Open(this, BundleFileName, AssetName);
		return assetDataAsync;
	}

	string IModelData.GetName()
	{
		return Name;
	}

	CommonData IModelData.GetCommonData()
	{
		return Common;
	}

	List<PlaySequenceData> IModelData.GetPlaySequenceList()
	{
		return PlaySequenceList;
	}

	EndingDisappearData IModelData.GetEndingDisappear()
	{
		return EndingDisappear;
	}

	List<RendererExtraData> IModelData.GetRenderExtraList()
	{
		return RendererExtraList;
	}

	bool ILoadJson.Load(JsonRead json)
	{
		Name = json["Name"].GETSTRING(Name);
		BundleFileName = json["BundleFileName"].GETSTRING(BundleFileName);
		AssetName = json["AssetName"].GETSTRING(AssetName);
		Common.LOADFROMJSON(json["Common"]);
		PlaySequenceData.LoadList(json["PlaySequenceList"], PlaySequenceList);
		EndingDisappear.LOADFROMJSON(json["EndingDisappear"]);
		RendererExtraData.LoadList(json["RenderExtraList"], RendererExtraList);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(PlaySequenceList))
		{
			PlaySequenceData playSequenceData = new PlaySequenceData();
			playSequenceData.PlayType = PlaySequenceData.EPlayType.Clamp;
			playSequenceData.TimeLength = Common.TimeLength;
			playSequenceData.ClipStartFrame = 0f;
			playSequenceData.ClipEndFrame = 1f;
			playSequenceData.LoopCount = 1;
			PlaySequenceList.Add(playSequenceData);
		}
		return true;
	}

	public static void LoadList(JsonRead json, ref PoolingList<ModelData> poolingList)
	{
		poolingList.Clear();
		if (json.IsArray() && json.Count != 0)
		{
			List<ModelData> list = poolingList.PopList(forceCreate: true);
			LoadList(json, list);
			poolingList.PushList(list);
		}
	}

	public static void LoadList(JsonRead json, List<ModelData> modelList)
	{
		modelList.Clear();
		if (!json.IsArray())
		{
			return;
		}
		for (int i = 0; i < json.Count; i++)
		{
			JsonRead fromArray = json.GetFromArray(i);
			ModelData modelData = new ModelData();
			if (modelData.LOADFROMJSON(fromArray))
			{
				modelList.Add(modelData);
			}
		}
	}
}
