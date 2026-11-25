using System.Collections.Generic;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPetPortraitSub;

public class StarSubComponent : BaseSubComponent<PetPortraitComponent>
{
	public static string PrefabPath = "Prefab2/Component/PetPortraitSub/StarSubComponent";

	public static PrefabAuto<StarSubComponent> PrefabAuto = new PrefabAuto<StarSubComponent>(PrefabPath);

	public List<UIWidget> StarList = new List<UIWidget>();

	private const float BottomStripHeight = 42f;

	private const float BottomStripInset = 5f;

	public override void Open(PetPortraitComponent owner)
	{
		base.Open(owner);
		GameObject rootGO = base.Owner.GetRootGO(PetPortraitComponent.ESubComponent.PetStar);
		SetRootAnchor(rootGO);
		if (base.Owner.WidgetThis != null && rootGO == base.Owner.WidgetThis.gameObject)
		{
			// No PetStar root (PetPortraitSmall): the stars, anchored 2 units inside
			// the root's top and bottom, would stretch over the whole icon. Keep the
			// root to a strip one star high along the bottom edge instead, as wide
			// as the 5-star sprite so the 3-5 star rows keep their native width.
			Root.leftAnchor.Set(0f, BottomStripInset);
			Root.rightAnchor.Set(1f, 0f - BottomStripInset);
			Root.bottomAnchor.Set(0f, 0f);
			Root.topAnchor.Set(0f, BottomStripHeight);
			Root.ResetAndUpdateAnchors();
		}
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
