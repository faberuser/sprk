using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NHeroPortraitSub;

public class SelectSubComponent : BaseSubComponent<HeroPortraitComponent>
{
	public static string PrefabPath = "Prefab2/Component/HeroPortraitSub/SelectSubComponent";

	public static PrefabAuto<SelectSubComponent> PrefabAuto = new PrefabAuto<SelectSubComponent>(PrefabPath);

	public UIWidget SelectionMark;

	public override void Open(HeroPortraitComponent owner)
	{
		base.Open(owner);
		SetRootAnchor(base.Owner.GetRootGO(HeroPortraitComponent.ESubComponent.Select));
	}
}
