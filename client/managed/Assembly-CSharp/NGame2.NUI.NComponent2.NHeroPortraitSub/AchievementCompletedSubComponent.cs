using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NHeroPortraitSub;

public class AchievementCompletedSubComponent : BaseSubComponent<HeroPortraitComponent>
{
	public static string PrefabPath = "Prefab2/Component/HeroPortraitSub/AchievementCompletedSubComponent";

	public static PrefabAuto<AchievementCompletedSubComponent> PrefabAuto = new PrefabAuto<AchievementCompletedSubComponent>(PrefabPath);

	public override void Open(HeroPortraitComponent owner)
	{
		base.Open(owner);
		SetRootAnchor(base.Owner.GetRootGO(HeroPortraitComponent.ESubComponent.AchievementCompleted));
	}

	public void Show(bool show)
	{
		UIHelper.EnableGameObject(Root, show);
	}
}
