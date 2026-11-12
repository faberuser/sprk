using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NHeroPortraitSub;

public class SelectSmallSubComponent : BaseSubComponent<HeroPortraitComponent>
{
	public static string PrefabPath = "Prefab2/Component/HeroPortraitSub/SelectSmallSubComponent";

	public static PrefabAuto<SelectSmallSubComponent> PrefabAuto = new PrefabAuto<SelectSmallSubComponent>(PrefabPath);

	public UIWidget SelectionMark;

	public override void Open(HeroPortraitComponent owner)
	{
		base.Open(owner);
		SetRootAnchor(base.Owner.GetRootGO(HeroPortraitComponent.ESubComponent.SelectSmall));
	}

	public void Close()
	{
	}
}
