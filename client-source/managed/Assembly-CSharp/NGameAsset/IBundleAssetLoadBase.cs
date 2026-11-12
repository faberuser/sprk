namespace NGameAsset;

public interface IBundleAssetLoadBase : IAsyncLoadBase
{
	BundleAssetKey Key { get; }

	ILockCounter Lock { get; }

	void DestroyUnusedInFactory();

	void Close(bool destroyImmediately);
}
