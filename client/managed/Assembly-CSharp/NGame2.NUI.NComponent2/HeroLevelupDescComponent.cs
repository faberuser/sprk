using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NDescription;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NComponent2;

public class HeroLevelupDescComponent : LinkFactoryComponent, NVespa.NPrefab2.InitOnCreate
{
	[Serializable]
	public class StatChangeLabelInfo
	{
		public StatType heroStatType;

		public UILabel Label_StatChange;
	}

	public const string PrefabPath = "Prefab2/Component/Hero/HeroLevelupDescComponent";

	public static PrefabAuto<HeroLevelupDescComponent> PrefabAuto = new PrefabAuto<HeroLevelupDescComponent>("Prefab2/Component/Hero/HeroLevelupDescComponent");

	public UISimpleGrid HeroPortraitSmallGrid;

	public UISimpleGrid HeroClassTagIconGrid;

	public UILabel Label_HeroName;

	public UILabel Label_PrevHeroLevel;

	public UILabel Label_CurHeroLevel;

	public StatChangeLabelInfo[] StatChangeLabels;

	public UIWidget WidgetLearnedSkill;

	public UISimpleGrid LearnedHeroSkillIconGrid;

	public UILabel Label_SkillName;

	private CachedHeroInfo _HeroInfo;

	private CreatureData _creatureData;

	private int _PrevHeroLevel;

	private HeroPortraitComponent _HeroPortrait;

	private ClassTagIconComponent _HeroClassTagIcon;

	private HeroSkillIconComponent _LearnedSkillIcon;

	private static int[] _LearnSkillLevels;

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		FactoryBase.SafeReleaseComponent(ref _HeroPortrait);
		FactoryBase.SafeReleaseComponent(ref _HeroClassTagIcon);
		FactoryBase.SafeReleaseComponent(ref _LearnedSkillIcon);
	}

	public void Open(CachedHeroInfo heroInfo, int PrevHeroLevel)
	{
		_HeroInfo = heroInfo;
		_PrevHeroLevel = PrevHeroLevel;
		_creatureData = GameRoot.Instance.CreatureDataContainer.GetData(_HeroInfo.HeroIndex);
		SetupHeroIconLevel();
		SetupHeroStatChange();
		SetupLearnSkillDesc();
	}

	private void SetupHeroIconLevel()
	{
		_HeroPortrait = UIHelper.AddGridItem(HeroPortraitSmallGrid, HeroPortraitComponent.SmallPrefabAuto);
		_HeroPortrait.Open(_HeroInfo, null, null, HeroPortraitComponent.EPortraitMode.Icon);
		string text = ((_creatureData == null) ? "(NoCreatureData)" : _creatureData.Name);
		UIHelper.SetLabelText(Label_HeroName, text);
		UIHelper.SetLabelText(Label_PrevHeroLevel, _PrevHeroLevel);
		UIHelper.SetLabelText(Label_CurHeroLevel, _HeroInfo.Level);
		_HeroClassTagIcon = UIHelper.AddGridItem(HeroClassTagIconGrid, ClassTagIconComponent.PrefabAuto);
		_HeroClassTagIcon.Open(_creatureData.TagType, null);
	}

	private void SetupHeroStatChange()
	{
		StatType[] heroMainStats = StatData.GetHeroMainStats((_creatureData != null && _creatureData.AttrType != null) ? _creatureData.AttrType[0] : SkillAttrType.None);
		StatData cachedHeroStat = SceneAutoBehaviour<StatDataCache>.instance.GetCachedHeroStat(_HeroInfo);
		short level = _HeroInfo.Level;
		_HeroInfo.Level = (short)_PrevHeroLevel;
		StatData cachedHeroStat2 = SceneAutoBehaviour<StatDataCache>.instance.GetCachedHeroStat(_HeroInfo);
		_HeroInfo.Level = level;
		StatType[] array = heroMainStats;
		foreach (StatType statType in array)
		{
			UILabel statChangeLabelInfo = GetStatChangeLabelInfo(statType);
			long stat = cachedHeroStat2.GetStat(statType);
			UIHelper.SetLabelText(statChangeLabelInfo, string.Concat(str2: (cachedHeroStat.GetStat(statType) - stat).ToString(), str0: stat.ToString(), str1: " +[00FF00]", str3: "[-]"));
		}
	}

	private void SetupLearnSkillDesc()
	{
		List<int> learnedSkillSlotIndices = GetLearnedSkillSlotIndices();
		if (learnedSkillSlotIndices.Count == 0)
		{
			UIHelper.EnableGameObject(WidgetLearnedSkill, enable: false);
			return;
		}
		int num = learnedSkillSlotIndices[0];
		int skillIndexBySlot = _creatureData.GetSkillIndexBySlot(num);
		Helper.SkillDetailInfo skillInfo = new Helper.SkillDetailInfo
		{
			BasicInfo = 
			{
				TableIndex = skillIndexBySlot,
				TableData = GameRoot.Instance.SkillDataContainer.GetData(skillIndexBySlot),
				Level = 1,
				IsLocked = false
			}
		};
		_LearnedSkillIcon = UIHelper.AddGridItem(LearnedHeroSkillIconGrid, HeroSkillIconComponent.PrefabAuto);
		_LearnedSkillIcon.Open(_HeroInfo, skillInfo, num, null);
		string skillName = SkillDescContainer.GetSkillName(_HeroInfo, skillInfo.BasicInfo.TableData);
		UIHelper.SetLabelText(Label_SkillName, (skillInfo.BasicInfo.TableData == null) ? "(skill data is null)" : skillName);
	}

	private UILabel GetStatChangeLabelInfo(StatType statType)
	{
		for (int i = 0; i < StatChangeLabels.Length; i++)
		{
			if (StatChangeLabels[i].heroStatType == statType)
			{
				return StatChangeLabels[i].Label_StatChange;
			}
		}
		return null;
	}

	private List<int> GetLearnedSkillSlotIndices()
	{
		List<int> list = new List<int>();
		if (_LearnSkillLevels == null)
		{
			_LearnSkillLevels = new int[3]
			{
				GameRoot.Instance.ConstantDataContainer.GetValueInt("ReqHeroLevelSkillSlot2"),
				GameRoot.Instance.ConstantDataContainer.GetValueInt("ReqHeroLevelSkillSlot3"),
				GameRoot.Instance.ConstantDataContainer.GetValueInt("ReqHeroLevelSkillSlot4")
			};
		}
		short level = _HeroInfo.Level;
		for (int i = 0; i < _LearnSkillLevels.Length; i++)
		{
			if (_PrevHeroLevel < _LearnSkillLevels[i] && level >= _LearnSkillLevels[i])
			{
				list.Add(i + 2);
			}
		}
		return list;
	}
}
