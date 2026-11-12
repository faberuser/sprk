using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NHeroPortraitSub;

public class AchievementClearedSubComponent : BaseSubComponent<HeroPortraitComponent>
{
	public static string PrefabPath = "Prefab2/Component/HeroPortraitSub/AchievementClearedSubComponent";

	public static PrefabAuto<AchievementClearedSubComponent> PrefabAuto = new PrefabAuto<AchievementClearedSubComponent>(PrefabPath);

	public override void Open(HeroPortraitComponent owner)
	{
		base.Open(owner);
		SetRootAnchor(base.Owner.GetRootGO(HeroPortraitComponent.ESubComponent.AchievementCleared));
	}

	public void Show(bool show)
	{
		UIHelper.EnableGameObject(Root, show);
	}
}
