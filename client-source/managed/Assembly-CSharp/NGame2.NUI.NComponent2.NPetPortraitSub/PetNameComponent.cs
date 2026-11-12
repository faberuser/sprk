using System;
using NGame2.NUI.NManager;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPetPortraitSub;

public class PetNameComponent : BaseSubComponent<PetPortraitComponent>
{
	[Serializable]
	public class RootInfo
	{
		public PetPortraitComponent.EPortraitMode Mode;

		public Transform Root;

		public UILabel NameLabel;

		public Transform Shadow;
	}

	public static string PrefabPath = "Prefab2/Component/PetPortraitSub/PetNameComponent";

	public static PrefabAuto<PetNameComponent> PrefabAuto = new PrefabAuto<PetNameComponent>(PrefabPath);

	public RootInfo[] Roots;

	public override void Open(PetPortraitComponent owner)
	{
		base.Open(owner);
		SetRootAnchor(base.Owner.GetRootGO(PetPortraitComponent.ESubComponent.Name));
	}

	public void SetShowName(bool ShowName, bool showShadow)
	{
		if (base.Owner.CreatureData == null)
		{
			return;
		}
		if (ShowName)
		{
			SetShowName(base.Owner.CreatureData.Name, showShadow);
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(Roots); i++)
		{
			UIHelper.EnableGameObject(Roots[i].Root, enable: false);
		}
	}

	public void SetShowName(string Name, bool showShadow)
	{
		if (base.Owner.CreatureData != null)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(Roots); i++)
			{
				UIHelper.EnableGameObject(Roots[i].Root, enable: false);
			}
			string text = Name;
			if (ClientValueManager.IsShowHeroIndex)
			{
				text = $"{text}({base.Owner.CreatureData.Index})";
			}
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(Roots); j++)
			{
				RootInfo rootInfo = Roots[j];
				UIHelper.EnableGameObject(rootInfo.Root, rootInfo.Mode == base.Owner.PortraitMode);
				UIHelper.EnableGameObject(rootInfo.Shadow, showShadow);
				UIHelper.SetLabelText(rootInfo.NameLabel, text);
			}
			Root.ForceUpdateAnchors(includeChildren: true);
		}
	}
}
