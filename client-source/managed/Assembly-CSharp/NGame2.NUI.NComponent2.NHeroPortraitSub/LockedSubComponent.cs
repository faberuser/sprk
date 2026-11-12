using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NHeroPortraitSub;

public class LockedSubComponent : BaseSubComponent<HeroPortraitComponent>
{
	public static string PrefabPath = "Prefab2/Component/HeroPortraitSub/LockedSubComponent";

	public static PrefabAuto<LockedSubComponent> PrefabAuto = new PrefabAuto<LockedSubComponent>(PrefabPath);

	public override void Open(HeroPortraitComponent owner)
	{
		base.Open(owner);
		SetRootAnchor(base.Owner.GetRootGO(HeroPortraitComponent.ESubComponent.Locked));
	}
}
