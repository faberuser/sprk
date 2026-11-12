using System.Collections.Generic;
using JsonBin;
using NGameAsset.NAsyncLoad;
using UnityEngine;

namespace NGameAsset.NBattleFieldMap2;

public class ModelData : ILoadJson, IModelData
{
	public const int ReserveCount = 1;

	public string BundleFileName = string.Empty;

	public string AssetName = string.Empty;

	public List<RenderExtraData> RenderExtraList = new List<RenderExtraData>();

	string IModelData.AssetName => AssetName;

	public AssetDataAsync<GameObject, ModelData> CreateAssetAsync()
	{
		AssetDataAsync<GameObject, ModelData> assetDataAsync = new AssetDataAsync<GameObject, ModelData>();
		assetDataAsync.Open(this, BundleFileName, AssetName);
		return assetDataAsync;
	}

	public GameObject InstantiateModel(GameObject assetGo)
	{
		GameObject gameObject = Object.Instantiate(assetGo);
		if (gameObject == null)
		{
			return null;
		}
		return gameObject;
	}

	public List<RenderExtraData> GetRenderExtraList()
	{
		return RenderExtraList;
	}

	bool ILoadJson.Load(JsonRead json)
	{
		BundleFileName = json["BundleFileName"].GETSTRING(BundleFileName);
		AssetName = json["AssetName"].GETSTRING(AssetName);
		RenderExtraData.LoadList(json["RenderExtraList"], RenderExtraList);
		return true;
	}
}
