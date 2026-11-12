using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NHeroPortraitSub;

public class EquipableStateComponent : BaseSubComponent<HeroPortraitComponent>
{
	public static string PrefabPath = "Prefab2/Component/HeroPortraitSub/EquipableStateComponent";

	public static PrefabAuto<EquipableStateComponent> PrefabAuto = new PrefabAuto<EquipableStateComponent>(PrefabPath);

	public override void Open(HeroPortraitComponent owner)
	{
		base.Open(owner);
		SetRootAnchor(base.Owner.GetRootGO(HeroPortraitComponent.ESubComponent.EquipableState));
	}

	public void Show(bool show)
	{
		UIHelper.EnableGameObject(Root, show);
	}
}
