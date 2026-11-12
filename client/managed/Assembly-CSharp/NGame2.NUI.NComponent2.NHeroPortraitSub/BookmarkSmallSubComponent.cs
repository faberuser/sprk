using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NHeroPortraitSub;

public class BookmarkSmallSubComponent : BaseSubComponent<HeroPortraitComponent>
{
	public static string PrefabPath = "Prefab2/Component/HeroPortraitSub/BookmarkSmallSubComponent";

	public static PrefabAuto<BookmarkSmallSubComponent> PrefabAuto = new PrefabAuto<BookmarkSmallSubComponent>(PrefabPath);

	public UIWidget SelectionMark;

	public override void Open(HeroPortraitComponent owner)
	{
		base.Open(owner);
		SetRootAnchor(base.Owner.GetRootGO(HeroPortraitComponent.ESubComponent.Bookmark));
	}

	public void Close()
	{
	}
}
