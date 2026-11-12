namespace NGameAsset;

public interface IAssetFactory<TAsset>
{
	TAsset Create();

	void Release(TAsset assetInstance);

	void Close();

	void DestroyUnused();
}
