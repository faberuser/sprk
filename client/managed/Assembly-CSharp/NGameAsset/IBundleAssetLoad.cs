using UnityEngine;

namespace NGameAsset;

public interface IBundleAssetLoad<TAsset> : IBundleAssetLoadBase, IAsyncLoadBase where TAsset : Object
{
	TAsset Asset { get; }

	IAssetFactory<TAsset> Factory { get; }
}
