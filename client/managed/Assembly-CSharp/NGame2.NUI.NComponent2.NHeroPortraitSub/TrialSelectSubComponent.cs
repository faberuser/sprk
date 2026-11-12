using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NHeroPortraitSub;

public class TrialSelectSubComponent : BaseSubComponent<HeroPortraitComponent>
{
	public static string PrefabPath = "Prefab2/Component/HeroPortraitSub/TrialSelectSubComponent";

	public static PrefabAuto<TrialSelectSubComponent> PrefabAuto = new PrefabAuto<TrialSelectSubComponent>(PrefabPath);

	public override void Open(HeroPortraitComponent owner)
	{
		base.Open(owner);
		SetRootAnchor(base.Owner.GetRootGO(HeroPortraitComponent.ESubComponent.TrialSelect));
	}
}
