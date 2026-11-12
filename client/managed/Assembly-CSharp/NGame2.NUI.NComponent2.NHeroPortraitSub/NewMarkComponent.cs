using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NHeroPortraitSub;

public class NewMarkComponent : BaseSubComponent<HeroPortraitComponent>
{
	public static string PrefabPath = "Prefab2/Component/HeroPortraitSub/NewMarkComponent";

	public static PrefabAuto<NewMarkComponent> PrefabAuto = new PrefabAuto<NewMarkComponent>(PrefabPath);

	public override void Open(HeroPortraitComponent owner)
	{
		base.Open(owner);
		SetRootAnchor(base.Owner.GetRootGO(HeroPortraitComponent.ESubComponent.NewMark));
	}
}
