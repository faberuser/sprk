using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NHeroPortraitSub;

public class BanSubComponent : BaseSubComponent<HeroPortraitComponent>
{
	public static string PrefabPath = "Prefab2/Component/HeroPortraitSub/BanSubComponent";

	public static PrefabAuto<BanSubComponent> PrefabAuto = new PrefabAuto<BanSubComponent>(PrefabPath);

	public UIWidget SelectionMark;

	public override void Open(HeroPortraitComponent owner)
	{
		base.Open(owner);
		SetRootAnchor(base.Owner.GetRootGO(HeroPortraitComponent.ESubComponent.Ban));
	}
}
