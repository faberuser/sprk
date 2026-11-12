using UnityEngine;

namespace NGameAsset.NBundleManager;

public static class AssetFactoryCreator
{
	public static IAssetFactory<TAsset> CreateFactory<TAsset>(IBundleAssetLoad<TAsset> assetLoad) where TAsset : Object
	{
		IAssetFactory<TAsset> assetFactory = null;
		if (typeof(TAsset) == typeof(GameObject) && assetLoad is IBundleAssetLoad<GameObject> assetLoad2 && new AssetFactoryGameObject(assetLoad2) is IAssetFactory<TAsset> result)
		{
			return result;
		}
		return new AssetFactory<TAsset>(assetLoad);
	}
}
