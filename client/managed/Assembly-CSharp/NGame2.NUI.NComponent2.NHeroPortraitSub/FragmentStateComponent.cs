using NShared;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NHeroPortraitSub;

public class FragmentStateComponent : BaseSubComponent<HeroPortraitComponent>
{
	public static string PrefabPath = "Prefab2/Component/HeroPortraitSub/FragmentStateComponent";

	public static PrefabAuto<FragmentStateComponent> PrefabAuto = new PrefabAuto<FragmentStateComponent>(PrefabPath);

	public UILabel FragmentLabel;

	public UILabel EssenceLabel;

	public override void Open(HeroPortraitComponent owner)
	{
		base.Open(owner);
		SetRootAnchor(base.Owner.GetRootGO(HeroPortraitComponent.ESubComponent.FragmentState));
	}

	public void Show(bool show, HeroAwakeType toggle)
	{
		UIHelper.EnableGameObject(Root, show);
		UIHelper.EnableGameObject(FragmentLabel, toggle == HeroAwakeType.Awaken);
		UIHelper.EnableGameObject(EssenceLabel, toggle == HeroAwakeType.Transcend);
	}
}
