using System.Collections.Generic;
using JsonBin;
using NGameAsset.NAsyncLoad;
using NVespa;
using NVespa.NRenderProxy;
using UnityEngine;

namespace NGameAsset.NEffect;

public class ParticleData : ILoadJson, IParticleData
{
	public const int ReserveCount = 1;

	public string Name = string.Empty;

	public string BundleFileName = string.Empty;

	public string AssetName = string.Empty;

	public CommonData Common = new CommonData();

	private PoolingList<ParticleSequenceData> _sequenceList;

	public bool IsDistanceEmit { get; set; }

	public AssetDataAsync<GameObject, ParticleData> CreateAssetAsync()
	{
		AssetDataAsync<GameObject, ParticleData> assetDataAsync = new AssetDataAsync<GameObject, ParticleData>();
		assetDataAsync.Open(this, BundleFileName, AssetName);
		return assetDataAsync;
	}

	string IParticleData.GetName()
	{
		return Name;
	}

	CommonData IParticleData.GetCommonData()
	{
		return Common;
	}

	int IParticleData.GetSequenceCount()
	{
		return _sequenceList.COUNT();
	}

	ParticleSequenceData IParticleData.GetSequenceData(int index)
	{
		return _sequenceList.GetValid(index);
	}

	IMaterialFactory IParticleData.GetMaterialFactory()
	{
		return SingletonAuto<MaterialPoolingFactory2>.instance;
	}

	bool ILoadJson.Load(JsonRead json)
	{
		Name = json["Name"].GETSTRING(Name);
		BundleFileName = json["BundleFileName"].GETSTRING(BundleFileName);
		AssetName = json["AssetName"].GETSTRING(AssetName);
		Common.LOADFROMJSON(json["Common"]);
		IsDistanceEmit = json["IsDistanceEmit"].GETBOOLEAN(IsDistanceEmit);
		ParticleSequenceData.LoadFromJson(ref _sequenceList, json["SequenceList"]);
		return true;
	}

	public static void LoadList(JsonRead json, ref PoolingList<ParticleData> poolingList)
	{
		poolingList.Clear();
		if (json.IsArray() && json.Count != 0)
		{
			List<ParticleData> list = poolingList.PopList(forceCreate: true);
			LoadList(json, list);
			poolingList.PushList(list);
		}
	}

	public static void LoadList(JsonRead json, List<ParticleData> list)
	{
		list.Clear();
		if (!json.IsArray())
		{
			return;
		}
		for (int i = 0; i < json.Count; i++)
		{
			JsonRead fromArray = json.GetFromArray(i);
			ParticleData particleData = new ParticleData();
			if (particleData.LOADFROMJSON(fromArray))
			{
				list.Add(particleData);
			}
		}
	}
}
