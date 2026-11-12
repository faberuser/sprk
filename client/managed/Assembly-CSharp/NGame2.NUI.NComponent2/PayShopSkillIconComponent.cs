using System.Collections.Generic;
using NGame2.NAccount;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class PayShopSkillIconComponent : MonoBehaviour, InitOnCreate
{
	public const string PrefabPath = "Resources/Prefab2/Component/PayShop2/PayShopSkillIconComponent";

	public static PrefabAuto<PayShopSkillIconComponent> PrefabAuto = new PrefabAuto<PayShopSkillIconComponent>("Resources/Prefab2/Component/PayShop2/PayShopSkillIconComponent");

	public List<HeroSkillIconComponent> HeroSkillIconComponents = new List<HeroSkillIconComponent>();

	public HeroInfo HeroInfo { get; private set; }

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
	}

	public void Open(HeroInfo heroInfo, int skillSlotIndex)
	{
		Helper.SkillBasicInfo[] skillBasicInfos = Helper.GetSkillBasicInfos(heroInfo, skillSlotIndex);
		Open(heroInfo, skillBasicInfos);
	}

	public void Open(HeroInfo heroInfo, Helper.SkillBasicInfo[] skillInfos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(HeroSkillIconComponents); i++)
		{
			HeroSkillIconComponent heroSkillIconComponent = HeroSkillIconComponents[i];
			if (__Extension_Method_ICollection__.COUNT(skillInfos) <= i)
			{
				UIHelper.EnableGameObject(heroSkillIconComponent, enable: false);
				continue;
			}
			UIHelper.EnableGameObject(heroSkillIconComponent, this);
			heroSkillIconComponent.Open(heroInfo, skillInfos[i], null);
		}
	}
}
