using System.Collections.Generic;
using JsonBin;
using NGameAsset.NAsyncLoad;
using UnityEngine;

namespace NGameAsset.NCreature;

public class HairData : ILoadJson, IHairData
{
	public string HairName = string.Empty;

	public string BundleFileName = string.Empty;

	public string AssetName = string.Empty;

	public string HairMaterialAssetName = string.Empty;

	string IHairData.HairName => HairName;

	public AssetDataAsync<GameObject, HairData> CreateAssetAsync()
	{
		AssetDataAsync<GameObject, HairData> assetDataAsync = new AssetDataAsync<GameObject, HairData>();
		assetDataAsync.Open(this, BundleFileName, AssetName);
		return assetDataAsync;
	}

	public AssetDataAsync<Material, HairData> CreateHairMaterialAssetAsync()
	{
		if (String_Extension_Method.IsNullOrEmpty(HairMaterialAssetName))
		{
			return null;
		}
		AssetDataAsync<Material, HairData> assetDataAsync = new AssetDataAsync<Material, HairData>();
		assetDataAsync.Open(this, BundleFileName, HairMaterialAssetName);
		return assetDataAsync;
	}

	GameObject IHairData.InstantiateModel(GameObject assetGo)
	{
		if (!assetGo)
		{
			return null;
		}
		GameObject gameObject = Object.Instantiate(assetGo);
		if (gameObject == null)
		{
			return null;
		}
		return gameObject;
	}

	bool ILoadJson.Load(JsonRead json)
	{
		HairName = json["HairName"].GETSTRING(HairName);
		BundleFileName = json["BundleFileName"].GETSTRING(BundleFileName);
		AssetName = json["AssetName"].GETSTRING(AssetName);
		HairMaterialAssetName = json["HairMaterialAssetName"].GETSTRING(HairMaterialAssetName);
		return true;
	}

	public static void LoadList(JsonRead json, List<HairData> hairList)
	{
		hairList.Clear();
		if (!json.IsArray())
		{
			return;
		}
		for (int i = 0; i < json.Count; i++)
		{
			JsonRead fromArray = json.GetFromArray(i);
			HairData hairData = new HairData();
			if (hairData.LOADFROMJSON(fromArray))
			{
				hairList.Add(hairData);
			}
		}
	}

	public static T SelectDataFromList<T>(string customHairName, string defaultHairName, List<T> hairList) where T : class, IHairData
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(hairList))
		{
			return null;
		}
		if (!String_Extension_Method.IsNullOrEmpty(customHairName))
		{
			for (int i = 0; i < hairList.Count; i++)
			{
				T val = hairList[i];
				if (val.HairName == customHairName)
				{
					return val;
				}
			}
		}
		if (!String_Extension_Method.IsNullOrEmpty(defaultHairName))
		{
			for (int j = 0; j < hairList.Count; j++)
			{
				T val2 = hairList[j];
				if (val2.HairName == defaultHairName)
				{
					return val2;
				}
			}
		}
		return null;
	}
}
