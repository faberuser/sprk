using System.Collections.Generic;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NHeroPortraitSub;

public class StarSubComponent : BaseSubComponent<HeroPortraitComponent>
{
	public static string PrefabPath = "Prefab2/Component/HeroPortraitSub/StarSubComponent";

	public static PrefabAuto<StarSubComponent> PrefabAuto = new PrefabAuto<StarSubComponent>(PrefabPath);

	public List<UIWidget> StarList = new List<UIWidget>();

	public List<UIWidget> TranscendStarList;

	public UILabel TranscendLabel;

	public override void Open(HeroPortraitComponent owner)
	{
		base.Open(owner);
		SetRootAnchor(base.Owner.GetRootGO(HeroPortraitComponent.ESubComponent.HeroStar));
		UIWidget[] componentsInChildren = Root.GetComponentsInChildren<UIWidget>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].ResetAndUpdateAnchors();
		}
	}

	public void Show(bool show)
	{
		base.SetShow(show);
		ShowStar(base.Owner, StarList, TranscendStarList, show);
	}

	public static void ShowStar(HeroPortraitComponent Owner, List<UIWidget> StarList, List<UIWidget> TranscendStarList, bool show)
	{
		if (!show || Owner.CreatureData == null)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		if (Owner.VisualStar > 0)
		{
			num = Owner.VisualStar;
			num2 = Owner.VisualTranscend;
		}
		else if (Owner.CreatureData.StartHeroStar > 0)
		{
			if (Owner.HeroInfo == null)
			{
				num = Owner.CreatureData.StartHeroStar;
			}
			else
			{
				num = ((Owner.HeroInfo.Transcended <= 0) ? Owner.HeroInfo.Star : 0);
				num2 = Owner.HeroInfo.Transcended;
			}
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(StarList); i++)
		{
			int num3 = i + 1;
			UIHelper.EnableGameObject(StarList[i], num == num3 && num2 == 0);
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(TranscendStarList); j++)
		{
			int num4 = j + 1;
			UIHelper.EnableGameObject(TranscendStarList[j], num2 == num4);
		}
	}
}
