using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NDescription;
using NGame2.NUtil.NStringProxy.NTypeText;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class RaidSkillComponent : MonoBehaviour, InitOnCreate
{
	public const string PrefabPath = "Resources/Prefab2/Component/Raid/RaidSkillComponent";

	public const string GuildRaidPrefabPath = "Resources/Prefab2/Component/GuildRaid/GuildRaidSkillComponent";

	public const string WorldBossPrefabpath = "Resources/Prefab2/Component/WorldBoss/WorldSkillComponent";

	public static PrefabAuto<RaidSkillComponent> PrefabAuto = new PrefabAuto<RaidSkillComponent>("Resources/Prefab2/Component/Raid/RaidSkillComponent");

	public static PrefabAuto<RaidSkillComponent> GuildRaidPrefabAuto = new PrefabAuto<RaidSkillComponent>("Resources/Prefab2/Component/GuildRaid/GuildRaidSkillComponent");

	public static PrefabAuto<RaidSkillComponent> WorldBossPrefabAuto = new PrefabAuto<RaidSkillComponent>("Resources/Prefab2/Component/WorldBoss/WorldSkillComponent");

	public Transform[] BgRoot;

	public UITexture IconTexture;

	public UILabel NameLabel;

	public UILabel DescLabel;

	public UILabel CooltimeLabel;

	public UILabel LevelLabel;

	public UILabel TypeLabel;

	public UIWidgetResizer Resizer;

	private UIWidget _wg;

	public void Open(SkillData skillData, int heroIndex, int heroStar, int heroLevel)
	{
		if (skillData != null)
		{
			int index = skillData.Index;
			AssetTexture skillIcon = UIHelper.GetSkillIcon2(index, null);
			UIHelper.ResetPlaneAssetTextureAsync(this, IconTexture, skillIcon);
			string skillName = SkillDescContainer.GetSkillName(skillData);
			UIHelper.SetLabelText(NameLabel, skillName);
			int num = 0;
			StatData statData = GameRoot.Instance.CreatureStatDataContainer.GetStatData(heroIndex, heroStar, heroLevel, num);
			string desc = SingletonAuto<SkillDescContainer>.instance.GetDesc(heroIndex, heroStar, num, 0, index, heroLevel, statData);
			UIHelper.SetLabelText(DescLabel, desc);
			int coolTimeMs = skillData.CoolTimeMs;
			if (coolTimeMs > 0)
			{
				UIHelper.EnableGameObject(CooltimeLabel, enable: true);
				UIHelper.SetLabelText(CooltimeLabel, SkillBuilder.GetCooltimeDisplayText(coolTimeMs));
			}
			else
			{
				UIHelper.EnableGameObject(CooltimeLabel, enable: false);
			}
			UIHelper.SetLabelText(LevelLabel, heroLevel);
			string text = SkillBuilder.GetText(new Helper.SkillBasicInfo
			{
				TableData = skillData,
				TableIndex = index
			});
			UIHelper.SetLabelText(TypeLabel, text);
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(BgRoot); i++)
			{
				UIHelper.EnableGameObject(BgRoot[i], enable: false);
			}
		}
	}

	public void SetBgIndex(int index)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(BgRoot); i++)
		{
			UIHelper.EnableGameObject(BgRoot[i], i == index % __Extension_Method_ICollection__.COUNT(BgRoot));
		}
	}

	public void Resize()
	{
		Resizer.Resize();
	}

	private void Start()
	{
		_wg = GetComponent<UIWidget>();
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
	}

	public static List<T> InitRaidSkillList<T>(PrefabAuto<T> prefab, UIBaseGrid grid, CreatureData monsterData, int star, int level, int[] showInfoSkills) where T : RaidSkillComponent
	{
		List<T> list = new List<T>();
		List<int> list2 = new List<int>();
		int num = 1;
		while (true)
		{
			int skillIndexBySlot = monsterData.GetSkillIndexBySlot(num);
			if (skillIndexBySlot == 0)
			{
				break;
			}
			list2.Add(skillIndexBySlot);
			num++;
		}
		if (__Extension_Method_ICollection__.COUNT(monsterData.ShowInfoSkill) > 0)
		{
			list2.AddRange(monsterData.ShowInfoSkill);
		}
		if (showInfoSkills != null && showInfoSkills.Length != 0)
		{
			list2.AddRange(showInfoSkills);
		}
		foreach (int item in list2)
		{
			SkillData data = GameRoot.Instance.SkillDataContainer.GetData(item);
			if (data != null)
			{
				T val = UIHelper.AddGridItem(grid, prefab);
				val.Open(data, monsterData.Index, star, level);
				val.SetBgIndex(__Extension_Method_ICollection__.COUNT(list));
				list.Add(val);
			}
		}
		UIHelper.RepositionGrid(grid);
		return list;
	}
}
