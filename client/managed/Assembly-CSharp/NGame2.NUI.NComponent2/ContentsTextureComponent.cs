using NGameAsset;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class ContentsTextureComponent : LinkFactoryComponent
{
	public const string PrefabPath = "Resources/Prefab2/Window/Portal/PortalRenewalComponents/ContentsTextureComponent";

	public static PrefabAuto<ContentsTextureComponent> PrefabAuto = new PrefabAuto<ContentsTextureComponent>("Resources/Prefab2/Window/Portal/PortalRenewalComponents/ContentsTextureComponent");

	public UITexture ChapterTexture;

	public int ChapterIndex { get; private set; }

	public void Open(int chapterIndex, string path)
	{
		ChapterIndex = chapterIndex;
		AssetTexture assetTexture = new AssetTexture();
		assetTexture.Open(path);
		UIHelper.ResetPlaneAssetTextureAsync(this, ChapterTexture, assetTexture);
	}

	public void Clear()
	{
		UIHelper.ClearPlaneAssetTexture(ChapterTexture);
	}
}
