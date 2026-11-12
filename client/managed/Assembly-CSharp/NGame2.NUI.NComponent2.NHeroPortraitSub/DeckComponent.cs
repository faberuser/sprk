using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NHeroPortraitSub;

public class DeckComponent : BaseSubComponent<HeroPortraitComponent>
{
	public static string PrefabPath = "Prefab2/Component/HeroPortraitSub/DeckComponent";

	public static PrefabAuto<DeckComponent> PrefabAuto = new PrefabAuto<DeckComponent>(PrefabPath);

	public override void Open(HeroPortraitComponent owner)
	{
		base.Open(owner);
		SetRootAnchor(base.Owner.GetRootGO(HeroPortraitComponent.ESubComponent.Deck));
	}

	public void Show(bool show)
	{
		UIHelper.EnableGameObject(Root, show);
	}
}
