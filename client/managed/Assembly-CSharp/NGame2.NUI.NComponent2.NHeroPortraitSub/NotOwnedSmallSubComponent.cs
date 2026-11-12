using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NHeroPortraitSub;

public class NotOwnedSmallSubComponent : BaseSubComponent<HeroPortraitComponent>
{
	public static string PrefabPath = "Prefab2/Component/HeroPortraitSub/NotOwnedSmallSubComponent";

	public static PrefabAuto<NotOwnedSmallSubComponent> PrefabAuto = new PrefabAuto<NotOwnedSmallSubComponent>(PrefabPath);

	public override void Open(HeroPortraitComponent owner)
	{
		base.Open(owner);
		SetRootAnchor(base.Owner.GetRootGO(HeroPortraitComponent.ESubComponent.NotOwnedSmall));
	}

	public void Close()
	{
	}
}
