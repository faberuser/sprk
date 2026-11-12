using System.Collections.Generic;
using JsonBin;
using NGameAsset.NAsyncLoad;
using NVespa;
using UnityEngine;

namespace NGameAsset.NEffect;

public class TrailData : ILoadJson, ITrailData
{
	public const int ReserveCount = 1;

	public string Name = string.Empty;

	public string BundleFileName = string.Empty;

	public string AssetName = string.Empty;

	public CommonData Common = new CommonData();

	public List<TrailSequenceData> SequenceList = new List<TrailSequenceData>();

	public bool IsDistanceEmit { get; set; }

	public AssetDataAsync<GameObject, TrailData> CreateAssetAsync()
	{
		AssetDataAsync<GameObject, TrailData> assetDataAsync = new AssetDataAsync<GameObject, TrailData>();
		assetDataAsync.Open(this, BundleFileName, AssetName);
		return assetDataAsync;
	}

	string ITrailData.GetName()
	{
		return Name;
	}

	CommonData ITrailData.GetCommonData()
	{
		return Common;
	}

	int ITrailData.GetSequenceCount()
	{
		return __Extension_Method_ICollection__.COUNT(SequenceList);
	}

	TrailSequenceData ITrailData.GetSequenceData(int index)
	{
		return Vespa_Generic_IList_Extension_Method.GetValid(SequenceList, index);
	}

	bool ILoadJson.Load(JsonRead json)
	{
		Name = json["Name"].GETSTRING(Name);
		BundleFileName = json["BundleFileName"].GETSTRING(BundleFileName);
		AssetName = json["AssetName"].GETSTRING(AssetName);
		Common.LOADFROMJSON(json["Common"]);
		IsDistanceEmit = json["IsDistanceEmit"].GETBOOLEAN(IsDistanceEmit);
		SequenceList.LoadListFromJson(json["SequenceList"]);
		return true;
	}

	public static void LoadList(JsonRead json, ref PoolingList<TrailData> poolingList)
	{
		poolingList.Clear();
		if (json.IsArray() && json.Count != 0)
		{
			List<TrailData> list = poolingList.PopList(forceCreate: true);
			LoadList(json, list);
			poolingList.PushList(list);
		}
	}

	public static void LoadList(JsonRead json, List<TrailData> list)
	{
		list.Clear();
		if (!json.IsArray())
		{
			return;
		}
		for (int i = 0; i < json.Count; i++)
		{
			JsonRead fromArray = json.GetFromArray(i);
			TrailData trailData = new TrailData();
			if (trailData.LOADFROMJSON(fromArray))
			{
				list.Add(trailData);
			}
		}
	}
}
