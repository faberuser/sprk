using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NHeroPortraitSub;

public class CostumeNewTagSmallSubComponent : BaseSubComponent<HeroPortraitComponent>
{
	public static string PrefabPath = "Prefab2/Component/HeroPortraitSub/CostumeNewTagSmallSubComponent";

	public static PrefabAuto<CostumeNewTagSmallSubComponent> PrefabAuto = new PrefabAuto<CostumeNewTagSmallSubComponent>(PrefabPath);

	public override void Open(HeroPortraitComponent owner)
	{
		base.Open(owner);
		SetRootAnchor(base.Owner.GetRootGO(HeroPortraitComponent.ESubComponent.CostumeNewTagSmall));
	}

	public void Show(bool show)
	{
		UIHelper.EnableGameObject(Root, show);
	}
}
