using UnityEngine;

namespace NGameAsset;

public class InvalidBundleAssetLoad<TAsset> : IBundleAssetLoad<TAsset>, IBundleAssetLoadBase, IAsyncLoadBase where TAsset : Object
{
	private string _keyDesc;

	private string _loadMessage;

	private BundleAssetKey _key = new BundleAssetKey(string.Empty, string.Empty, typeof(TAsset));

	private ConstantLockCounter _lockCounter = new ConstantLockCounter(1);

	string IAsyncLoadBase.KeyDesc => _keyDesc;

	bool IAsyncLoadBase.IsDone => true;

	bool IAsyncLoadBase.IsValid => false;

	string IAsyncLoadBase.LoadMessage => _loadMessage;

	BundleAssetKey IBundleAssetLoadBase.Key => _key;

	ILockCounter IBundleAssetLoadBase.Lock => _lockCounter;

	TAsset IBundleAssetLoad<TAsset>.Asset => null;

	IAssetFactory<TAsset> IBundleAssetLoad<TAsset>.Factory => null;

	public InvalidBundleAssetLoad(string keyDesc, string loadMessage)
	{
		_keyDesc = keyDesc;
		_loadMessage = loadMessage;
	}

	void IBundleAssetLoadBase.DestroyUnusedInFactory()
	{
	}

	void IBundleAssetLoadBase.Close(bool destroyImmediately)
	{
	}
}
