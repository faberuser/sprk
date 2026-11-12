using System;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NHeroPortraitSub;

public class HeroNameComponent : BaseSubComponent<HeroPortraitComponent>
{
	[Serializable]
	public class RootInfo
	{
		public HeroPortraitComponent.EPortraitMode Mode;

		public Transform Root;

		public UILabel NameLabel;

		public Transform Shadow;
	}

	public static string PrefabPath = "Prefab2/Component/HeroPortraitSub/HeroNameComponent";

	public static PrefabAuto<HeroNameComponent> PrefabAuto = new PrefabAuto<HeroNameComponent>(PrefabPath);

	public RootInfo[] Roots;

	public override void Open(HeroPortraitComponent owner)
	{
		base.Open(owner);
		SetRootAnchor(base.Owner.GetRootGO(HeroPortraitComponent.ESubComponent.HeroName));
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

	public void SetShowName(bool ShowName, bool showShadow)
	{
		if (base.Owner.CreatureData == null)
		{
			return;
		}
		if (ShowName)
		{
			HeroProfileData data = GameRoot.Instance.HeroProfileDataContainer.GetData(base.Owner.CreatureData.Index);
			if (data != null && !String_Extension_Method.IsNullOrEmpty(data.HeroName))
			{
				SetShowName(LocalizationManager.GetStringDirect(data.HeroName), showShadow);
			}
			else
			{
				SetShowName(base.Owner.CreatureData.Name, showShadow);
			}
		}
		else
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(Roots); i++)
			{
				UIHelper.EnableGameObject(Roots[i].Root, enable: false);
			}
		}
	}
}
