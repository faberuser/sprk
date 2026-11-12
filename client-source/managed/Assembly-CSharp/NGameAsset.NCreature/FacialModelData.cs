using System.Collections.Generic;
using JsonBin;
using NGameAsset.NAsyncLoad;
using UnityEngine;

namespace NGameAsset.NCreature;

public class FacialModelData : ILoadJson, IFacialModelData
{
	public string BundleFileName = string.Empty;

	public string AssetName = string.Empty;

	public List<string> BlendShapeNameList = new List<string>();

	string IFacialModelData.ModelName => AssetName;

	public AssetDataAsync<Mesh, FacialModelData> CreateAssetAsync()
	{
		if (String_Extension_Method.IsNullOrEmpty(BundleFileName) || String_Extension_Method.IsNullOrEmpty(AssetName))
		{
			return null;
		}
		AssetDataAsync<Mesh, FacialModelData> assetDataAsync = new AssetDataAsync<Mesh, FacialModelData>();
		assetDataAsync.Open(this, BundleFileName, AssetName);
		return assetDataAsync;
	}

	int IFacialModelData.GetBlendShapeCount()
	{
		return BlendShapeNameList.Count;
	}

	string IFacialModelData.GetBlendShapeName(int index)
	{
		return BlendShapeNameList[index];
	}

	bool ILoadJson.Load(JsonRead json)
	{
		BundleFileName = json["BundleFileName"].GETSTRING(BundleFileName);
		AssetName = json["AssetName"].GETSTRING(AssetName);
		if (json.Contains("FacialMappingDic"))
		{
			JsonRead jsonRead = json["FacialMappingDic"];
			int i = 0;
			for (int count = jsonRead.Count; i < count; i++)
			{
				string sTRING = jsonRead.GetFromArray(i)["Key"].GETSTRING(string.Empty);
				if (!String_Extension_Method.IsNullOrEmpty(sTRING))
				{
					BlendShapeNameList.Add(sTRING);
				}
			}
		}
		else
		{
			BlendShapeNameList.LoadListFromJson(json["BlendShapeNameList"]);
		}
		return true;
	}

	public static void LoadList(JsonRead json, List<FacialModelData> list)
	{
		list.Clear();
		if (!json.IsArray())
		{
			return;
		}
		for (int i = 0; i < json.Count; i++)
		{
			JsonRead fromArray = json.GetFromArray(i);
			FacialModelData facialModelData = new FacialModelData();
			if (facialModelData.LOADFROMJSON(fromArray))
			{
				list.Add(facialModelData);
			}
		}
	}
}
