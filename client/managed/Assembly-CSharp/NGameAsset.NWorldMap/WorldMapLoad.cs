using System.Collections.Generic;
using NGameAsset.NAsyncLoad;
using UnityEngine;

namespace NGameAsset.NWorldMap;

public class WorldMapLoad : IWorldMapLoad, IAsyncLoadBase
{
	private WorldMapData _worldMapData;

	private List<AssetDataAsync<Material, MapImageData>> _assetDataList = new List<AssetDataAsync<Material, MapImageData>>();

	int IWorldMapLoad.MapImageCount => _assetDataList.Count;

	string IAsyncLoadBase.KeyDesc => _worldMapData.Code;

	bool IAsyncLoadBase.IsDone => GameAsset_AsyncLoad_ExtensionMethod.AsyncIsDone(_assetDataList);

	bool IAsyncLoadBase.IsValid => GameAsset_AsyncLoad_ExtensionMethod.AsyncIsValid(_assetDataList);

	string IAsyncLoadBase.LoadMessage => string.Empty;

	public void Open(WorldMapData data)
	{
		_worldMapData = data;
		for (int i = 0; i < data.MapImageList.Count; i++)
		{
			MapImageData mapImageData = data.MapImageList[i];
			AssetDataAsync<Material, MapImageData> assetDataAsync = new AssetDataAsync<Material, MapImageData>();
			assetDataAsync.Open(mapImageData, mapImageData.BundleFileName, mapImageData.AssetName);
			_assetDataList.Add(assetDataAsync);
		}
	}

	MapImageData IWorldMapLoad.GetImageData(int index)
	{
		if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(_assetDataList, index))
		{
			return null;
		}
		return _assetDataList[index].Data;
	}

	Material IWorldMapLoad.GetImageAsset(int index)
	{
		if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(_assetDataList, index))
		{
			return null;
		}
		AssetDataAsync<Material, MapImageData> assetDataAsync = _assetDataList[index];
		if (!assetDataAsync.ISVALID())
		{
			return null;
		}
		return assetDataAsync.Asset;
	}

	void IWorldMapLoad.Close()
	{
		for (int i = 0; i < _assetDataList.Count; i++)
		{
			_assetDataList[i].Close();
		}
		_assetDataList.Clear();
	}
}
