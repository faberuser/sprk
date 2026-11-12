using System.Collections.Generic;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NPetPortraitSub;

public class StarSubComponent : BaseSubComponent<PetPortraitComponent>
{
	public static string PrefabPath = "Prefab2/Component/PetPortraitSub/StarSubComponent";

	public static PrefabAuto<StarSubComponent> PrefabAuto = new PrefabAuto<StarSubComponent>(PrefabPath);

	public List<UIWidget> StarList = new List<UIWidget>();

	public override void Open(PetPortraitComponent owner)
	{
		base.Open(owner);
		SetRootAnchor(base.Owner.GetRootGO(PetPortraitComponent.ESubComponent.PetStar));
		UIWidget[] componentsInChildren = Root.GetComponentsInChildren<UIWidget>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].ResetAndUpdateAnchors();
		}
	}

	public void Show(bool show)
	{
		base.SetShow(show);
		ShowStar(base.Owner, StarList, show);
	}

	private static int GetStar(PetPortraitComponent owner)
	{
		if (owner == null)
		{
			return 0;
		}
		return owner.VisualStar;
	}

	public static void ShowStar(PetPortraitComponent owner, List<UIWidget> startList, bool show)
	{
		if (!show)
		{
			return;
		}
		GetStar(owner);
		if (owner.CreatureData != null)
		{
			int star = GetStar(owner);
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(startList); i++)
			{
				int num = i + 1;
				UIHelper.EnableGameObject(startList[i], star == num);
			}
		}
	}
}
