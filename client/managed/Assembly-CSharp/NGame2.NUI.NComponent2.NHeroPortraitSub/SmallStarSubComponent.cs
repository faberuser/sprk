using System.Collections.Generic;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NHeroPortraitSub;

public class SmallStarSubComponent : BaseSubComponent<HeroPortraitComponent>
{
	public static string PrefabPath = "Prefab2/Component/HeroPortraitSub/SmallStarSubComponent";

	public static PrefabAuto<SmallStarSubComponent> PrefabAuto = new PrefabAuto<SmallStarSubComponent>(PrefabPath);

	public List<UIWidget> StarList = new List<UIWidget>();

	public List<UIWidget> TranscendStarList;

	public UILabel TranscendLabel;

	public override void Open(HeroPortraitComponent owner)
	{
		base.Open(owner);
		SetRootAnchor(base.Owner.GetRootGO(HeroPortraitComponent.ESubComponent.HeroSmallStar));
		UIWidget[] componentsInChildren = Root.GetComponentsInChildren<UIWidget>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].ResetAndUpdateAnchors();
		}
	}

	public void Show(bool show)
	{
		base.SetShow(show);
		StarSubComponent.ShowStar(base.Owner, StarList, TranscendStarList, show);
	}
}
