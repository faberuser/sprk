using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;
using MiniJSON;
using NVespa;

namespace NShared;

[JsonSerializable(Javascript = true)]
public class HeroInfo : IAccountAttrKey<int>
{
	protected Dictionary<int, TranscendSkillPageInfo> TranscendSkillPages;

	protected Dictionary<int, List<HeroRunePageInfo>> RunePageInfoDic;

	protected Dictionary<int, PunishmentRuneOptionInfo> EquippiedPunishmentRuneInfoDic = new Dictionary<int, PunishmentRuneOptionInfo>();

	public const int MaxSkillSlotIndex = 5;

	public const int MaxTranscendSkillSlotIndex = 5;

	[IgnoreMember]
	public CreatureData CreatureData => GameRoot.Instance.CreatureDataContainer.GetData(HeroIndex);

	int IAccountAttrKey<int>.Key => HeroIndex;

	public virtual int HeroIndex { get; set; }

	public virtual short Level { get; set; }

	public virtual int Exp { get; set; }

	public virtual sbyte Star { get; set; }

	public virtual string CreatedTime { get; set; }

	public virtual short SkillLevel1 { get; set; }

	public virtual short SkillLevel2 { get; set; }

	public virtual short SkillLevel3 { get; set; }

	public virtual short SkillLevel4 { get; set; }

	public virtual short SkillLevel5 { get; set; }

	public virtual int EquipItemSlotIndex1 { get; set; }

	public virtual int EquipItemSlotIndex2 { get; set; }

	public virtual int EquipItemSlotIndex3 { get; set; }

	public virtual int EquipItemSlotIndex4 { get; set; }

	public virtual int EquipItemSlotIndex5 { get; set; }

	public virtual int EquipItemSlotIndex6 { get; set; }

	public virtual int EquipItemSlotIndex7 { get; set; }

	public virtual int EquipItemSlotIndex8 { get; set; }

	public virtual int EquipItemSlotIndex9 { get; set; }

	public virtual int EquipItemSlotIndex10 { get; set; }

	public virtual short SkillExtend1 { get; set; }

	public virtual short SkillExtend2 { get; set; }

	public virtual short SkillExtend3 { get; set; }

	public virtual short SkillExtend4 { get; set; }

	public virtual short SkillExtend5 { get; set; }

	public virtual sbyte EnterDungeon { get; set; }

	public virtual short Purified { get; set; }

	public virtual sbyte Transcended { get; set; }

	public virtual sbyte LimitBreakLevel { get; set; }

	public virtual long LimitBreakExp { get; set; }

	public virtual int FlaskItemIndex { get; set; }

	public virtual int FlaskExp { get; set; }

	public virtual int CostumeIndex { get; set; }

	public virtual int WeaponCostumeIndex { get; set; }

	public virtual int HairCostumeIndex { get; set; }

	public virtual byte HideUniqueWeapon { get; set; }

	public virtual short TranscendPoint { get; set; }

	public virtual string TranscendSkillPage1 { get; set; }

	public virtual string TranscendSkillPage2 { get; set; }

	public virtual string TranscendSkillPage3 { get; set; }

	public virtual string TranscendSkillPage4 { get; set; }

	public virtual string TranscendSkillPage5 { get; set; }

	public virtual string TranscendSkillPage6 { get; set; }

	[Obsolete("ApplySkillPage를 사용하지 말고 SkillPage를 사용하자. 서버에서 데이터를 받기위해 필요한 변수")]
	public virtual int ApplySkillPage { get; set; }

	public virtual int ClosenessPoint { get; set; }

	public virtual string LastGreetingTime { get; set; }

	public virtual string LastConversationTime { get; set; }

	public virtual string LastGiftTime { get; set; }

	public virtual string MaxRewardedTime { get; set; }

	public virtual string LastRouletteTime { get; set; }

	public virtual int AccessoryCostumeIndex1 { get; set; }

	public virtual int AccessoryCostumeIndex2 { get; set; }

	public virtual int AccessoryCostumeIndex3 { get; set; }

	public virtual int AccessoryCostumeIndex4 { get; set; }

	public virtual int AccessoryCostumeIndex5 { get; set; }

	public virtual int AccessoryCostumeIndex6 { get; set; }

	public virtual int ApplyRunePage { get; set; }

	[NonDbSchema]
	public HeroRunePageInfo[] HeroRunePageInfos { get; set; }

	[NonDbSchema]
	public virtual int SkillPage
	{
		get
		{
			if (ApplySkillPage != 0)
			{
				return ApplySkillPage;
			}
			return 1;
		}
	}

	protected virtual void InitRunePages()
	{
		RunePageInfoDic = new Dictionary<int, List<HeroRunePageInfo>>();
		if (HeroRunePageInfos == null || HeroRunePageInfos.Length == 0)
		{
			AddRunePage(GetDefaultRunePage());
			return;
		}
		HeroRunePageInfo[] heroRunePageInfos = HeroRunePageInfos;
		foreach (HeroRunePageInfo info in heroRunePageInfos)
		{
			SetRunePage(info, null);
		}
	}

	public void AddRunePage(int runePage)
	{
		if (runePage <= 0)
		{
			runePage = GetDefaultRunePage();
		}
		if (RunePageInfoDic == null)
		{
			InitRunePages();
		}
		if (!RunePageInfoDic.ContainsKey(runePage))
		{
			RunePageInfoDic.Add(runePage, new List<HeroRunePageInfo>());
			int maxRuneSlot = GetMaxRuneSlot();
			for (int i = 1; i <= maxRuneSlot; i++)
			{
				HeroRunePageInfo heroRunePageInfo = new HeroRunePageInfo();
				heroRunePageInfo.HeroIndex = HeroIndex;
				heroRunePageInfo.RunePage = runePage;
				heroRunePageInfo.SlotNum = i;
				RunePageInfoDic[runePage].Add(heroRunePageInfo);
			}
		}
	}

	public List<HeroRunePageInfo> GetRunePageInfos(int runePage)
	{
		if (runePage <= 0)
		{
			runePage = GetDefaultRunePage();
		}
		if (RunePageInfoDic == null)
		{
			InitRunePages();
		}
		if (!RunePageInfoDic.ContainsKey(runePage))
		{
			return null;
		}
		return RunePageInfoDic[runePage];
	}

	public List<HeroRunePageInfo> GetRunePageInfos()
	{
		if (RunePageInfoDic == null)
		{
			InitRunePages();
		}
		List<HeroRunePageInfo> list = new List<HeroRunePageInfo>();
		foreach (KeyValuePair<int, List<HeroRunePageInfo>> item in RunePageInfoDic)
		{
			list.AddRange(item.Value);
		}
		return list;
	}

	public PunishmentRuneOptionInfo GetPunishmentRuneOptionInfo(HeroRunePageInfo info)
	{
		if (info == null)
		{
			return null;
		}
		if (RunePageInfoDic == null)
		{
			InitRunePages();
		}
		if (!EquippiedPunishmentRuneInfoDic.ContainsKey(info.EquipItemSlotIndex))
		{
			return null;
		}
		return EquippiedPunishmentRuneInfoDic[info.EquipItemSlotIndex];
	}

	public List<PunishmentRuneOptionInfo> GetPunishmentRuneOptionInfos()
	{
		return EquippiedPunishmentRuneInfoDic.Values.ToList();
	}

	public void SetRunePage(HeroRunePageInfo info, PunishmentRuneOptionInfo optionInfo)
	{
		if (RunePageInfoDic == null)
		{
			InitRunePages();
		}
		if (info == null)
		{
			return;
		}
		List<HeroRunePageInfo> runePageInfos = GetRunePageInfos(info.RunePage);
		if (runePageInfos == null)
		{
			AddRunePage(info.RunePage);
			runePageInfos = GetRunePageInfos(info.RunePage);
		}
		for (int i = 0; i < runePageInfos.Count; i++)
		{
			if (runePageInfos[i].RunePage != info.RunePage || runePageInfos[i].SlotNum != info.SlotNum)
			{
				continue;
			}
			int equipItemSlotIndex = runePageInfos[i].EquipItemSlotIndex;
			int num = -1;
			if (optionInfo != null)
			{
				num = optionInfo.EquipItemSlotIndex;
				if (!EquippiedPunishmentRuneInfoDic.ContainsKey(num))
				{
					EquippiedPunishmentRuneInfoDic.Add(num, optionInfo);
				}
			}
			if (equipItemSlotIndex != num)
			{
				EquippiedPunishmentRuneInfoDic.Remove(equipItemSlotIndex);
			}
			runePageInfos[i] = info;
		}
	}

	public static int GetMaxRunePage()
	{
		return GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxRunePage", 3);
	}

	public static int GetDefaultRunePage()
	{
		return GameRoot.Instance.ConstantDataContainer.GetValueInt("DefaultHeroRunePage", 1);
	}

	public static int GetMaxRuneSlot()
	{
		return GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxRuneSlotNum", 5);
	}

	private void InitTranscendSkillPages()
	{
		TranscendSkillPages = new Dictionary<int, TranscendSkillPageInfo>();
		AddTranscendSkillPage(1, TranscendSkillPage1, forceAdd: true);
		AddTranscendSkillPage(2, TranscendSkillPage2);
		AddTranscendSkillPage(3, TranscendSkillPage3);
		AddTranscendSkillPage(4, TranscendSkillPage4);
		AddTranscendSkillPage(5, TranscendSkillPage5);
		AddTranscendSkillPage(6, TranscendSkillPage6);
	}

	public void AddTranscendSkillPage(int pageNum, string pageString, bool forceAdd = false)
	{
		if (pageString == null || pageString.Length == 0)
		{
			if (forceAdd)
			{
				TranscendSkillPageInfo value = new TranscendSkillPageInfo();
				TranscendSkillPages.Add(pageNum, value);
			}
			return;
		}
		List<int> list = (Json.Deserialize(pageString) as List<object>).ConvertAll(Convert.ToInt32);
		if (list == null)
		{
			return;
		}
		TranscendSkillPageInfo transcendSkillPageInfo = new TranscendSkillPageInfo();
		foreach (int item in list)
		{
			int transcended = item / 10;
			int index = item % 10;
			CreatureData.TranscendSkill transcendSkillData = CreatureData.GetTranscendSkillData(transcended, index);
			if (transcendSkillData != null)
			{
				transcendSkillPageInfo.TranscendSkillDatas.Add(transcendSkillData);
			}
		}
		TranscendSkillPages.Add(pageNum, transcendSkillPageInfo);
	}

	public int GetMaxLevel()
	{
		return GameRoot.Instance.CreatureStarDataContainer.GetHeroMaxLevel(Star, Transcended);
	}

	public bool IsMaxLevel()
	{
		return Level >= GetMaxLevel();
	}

	public int GetEquipItemSlotIndex(int partIndex)
	{
		switch (partIndex)
		{
		case 1:
			return EquipItemSlotIndex1;
		case 2:
			return EquipItemSlotIndex2;
		case 3:
			return EquipItemSlotIndex3;
		case 4:
			return EquipItemSlotIndex4;
		case 5:
			return EquipItemSlotIndex5;
		case 6:
			return EquipItemSlotIndex6;
		case 7:
			return EquipItemSlotIndex7;
		case 8:
			return EquipItemSlotIndex8;
		case 9:
			return EquipItemSlotIndex9;
		case 10:
			return EquipItemSlotIndex10;
		default:
			VespaDebug.LogError("GetEquipItemSlotIndex failed : partIndex=" + partIndex);
			return 0;
		}
	}

	public bool SetEquipItemSlotIndex(int partIndex, int equipItemSlotIndex)
	{
		switch (partIndex)
		{
		case 1:
			EquipItemSlotIndex1 = equipItemSlotIndex;
			return true;
		case 2:
			EquipItemSlotIndex2 = equipItemSlotIndex;
			return true;
		case 3:
			EquipItemSlotIndex3 = equipItemSlotIndex;
			return true;
		case 4:
			EquipItemSlotIndex4 = equipItemSlotIndex;
			return true;
		case 5:
			EquipItemSlotIndex5 = equipItemSlotIndex;
			return true;
		case 6:
			EquipItemSlotIndex6 = equipItemSlotIndex;
			return true;
		case 7:
			EquipItemSlotIndex7 = equipItemSlotIndex;
			return true;
		case 8:
			EquipItemSlotIndex8 = equipItemSlotIndex;
			return true;
		case 9:
			EquipItemSlotIndex9 = equipItemSlotIndex;
			return true;
		case 10:
			EquipItemSlotIndex10 = equipItemSlotIndex;
			return true;
		default:
			VespaDebug.LogError("SetEquipItemSlotIndex failed : partIndex=" + partIndex);
			return false;
		}
	}

	public List<int> GetEquipItemSlotIndexList()
	{
		List<int> list = new List<int>(11);
		foreach (EquipSlotData allEquipSlotDatum in EquipSlotHelper.GetAllEquipSlotData())
		{
			int equipItemSlotIndex = GetEquipItemSlotIndex(allEquipSlotDatum.Index);
			if (equipItemSlotIndex != 0)
			{
				list.Add(equipItemSlotIndex);
			}
		}
		foreach (HeroRunePageInfo runePageInfo in GetRunePageInfos())
		{
			if (runePageInfo.EquipItemSlotIndex > 0)
			{
				list.Add(runePageInfo.EquipItemSlotIndex);
			}
		}
		return list;
	}

	public int GetPartIndexOfEquipItem(int equipItemSlotIndex)
	{
		foreach (EquipSlotData allEquipSlotDatum in EquipSlotHelper.GetAllEquipSlotData())
		{
			int equipItemSlotIndex2 = GetEquipItemSlotIndex(allEquipSlotDatum.Index);
			if (equipItemSlotIndex2 != 0 && equipItemSlotIndex2 == equipItemSlotIndex)
			{
				return allEquipSlotDatum.Index;
			}
		}
		return 0;
	}

	public bool IsEquipSlotLocked(int partIndex)
	{
		return GetEquipItemSlotIndex(partIndex) == -1;
	}

	public void SetEquipSlotOpened(int partIndex)
	{
		SetEquipItemSlotIndex(partIndex, 0);
	}

	public static EquipPartType GetEquipPartTypeFor(int equipSlotIndex)
	{
		return GameRoot.Instance.EquipSlotDataContainer.GetEquipPartTypeFor(equipSlotIndex);
	}

	public int GetSkillLevel(int slotIndex, bool isUserHero)
	{
		int num = 0;
		switch (slotIndex)
		{
		case 1:
			num = SkillLevel1;
			break;
		case 2:
			num = SkillLevel2;
			break;
		case 3:
			num = SkillLevel3;
			break;
		case 4:
			num = SkillLevel4;
			break;
		case 5:
			num = SkillLevel5;
			break;
		default:
			return 0;
		}
		if (isUserHero && num <= 0)
		{
			int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("ReqHeroLevelSkillSlot" + slotIndex);
			num = ((Level >= valueInt) ? 1 : 0);
		}
		return num;
	}

	public void SetSkillLevel(int slotIndex, int level)
	{
		switch (slotIndex)
		{
		case 1:
			SkillLevel1 = (short)level;
			break;
		case 2:
			SkillLevel2 = (short)level;
			break;
		case 3:
			SkillLevel3 = (short)level;
			break;
		case 4:
			SkillLevel4 = (short)level;
			break;
		case 5:
			SkillLevel5 = (short)level;
			break;
		}
	}

	public void MakeFullSkillLevel()
	{
		for (int i = 1; i <= 5; i++)
		{
			SetSkillLevel(i, Level);
		}
	}

	public int GetSkillLevelBySkillIndex(CreatureData creatureData, int skillIndex, bool learnByLevel = true, bool isRecurse = false)
	{
		if (creatureData == null)
		{
			return 0;
		}
		if (skillIndex == creatureData.BaseSkillIndex)
		{
			return Level;
		}
		if (creatureData.ExtraSkill != null && TypeIterator.Contains(creatureData.ExtraSkill, skillIndex))
		{
			return Level;
		}
		int skillSlotByIndex = creatureData.GetSkillSlotByIndex(skillIndex);
		if (skillSlotByIndex > 0)
		{
			return GetSkillLevel(skillSlotByIndex, learnByLevel);
		}
		int num = 1;
		while (true)
		{
			int[] skillExtend = creatureData.GetSkillExtend(num);
			if (skillExtend == null || skillExtend.Length == 0)
			{
				break;
			}
			int num2 = -1;
			for (int i = 0; i < skillExtend.Length; i++)
			{
				if (skillExtend[i] == skillIndex)
				{
					num2 = i;
					break;
				}
			}
			if (num2 != -1)
			{
				int skillExtend2 = GetSkillExtend(num);
				if (skillExtend2 > num2)
				{
					return skillExtend2;
				}
			}
			num++;
		}
		if (IsLearnTranscendSkill(SkillPage, skillIndex))
		{
			return 1;
		}
		if (!isRecurse)
		{
			SkillData data = GameRoot.Instance.SkillDataContainer.GetData(skillIndex);
			if (data != null && data.FollowLevelIndex != 0)
			{
				return GetSkillLevelBySkillIndex(creatureData, data.FollowLevelIndex, learnByLevel, isRecurse: true);
			}
		}
		return 0;
	}

	public int GetSkillExtend(int slotIndex)
	{
		return slotIndex switch
		{
			1 => SkillExtend1, 
			2 => SkillExtend2, 
			3 => SkillExtend3, 
			4 => SkillExtend4, 
			5 => SkillExtend5, 
			_ => 0, 
		};
	}

	public void SetSkillExtend(int slotIndex, int extend)
	{
		switch (slotIndex)
		{
		case 1:
			SkillExtend1 = (short)extend;
			break;
		case 2:
			SkillExtend2 = (short)extend;
			break;
		case 3:
			SkillExtend3 = (short)extend;
			break;
		case 4:
			SkillExtend4 = (short)extend;
			break;
		case 5:
			SkillExtend5 = (short)extend;
			break;
		}
	}

	public bool IsMaxStar()
	{
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("HeroMaxStar");
		return Star >= valueInt;
	}

	public string GetTranscendSkillPageStringArray(int page)
	{
		return page switch
		{
			1 => TranscendSkillPage1, 
			2 => TranscendSkillPage2, 
			3 => TranscendSkillPage3, 
			4 => TranscendSkillPage4, 
			5 => TranscendSkillPage5, 
			6 => TranscendSkillPage6, 
			_ => null, 
		};
	}

	public void SetTranscendSkillPage(int page)
	{
		ApplySkillPage = page;
	}

	public void SetTranscendSkillPageStringArray(int page, string skillPage)
	{
		switch (page)
		{
		case 1:
			TranscendSkillPage1 = skillPage;
			break;
		case 2:
			TranscendSkillPage2 = skillPage;
			break;
		case 3:
			TranscendSkillPage3 = skillPage;
			break;
		case 4:
			TranscendSkillPage4 = skillPage;
			break;
		case 5:
			TranscendSkillPage5 = skillPage;
			break;
		case 6:
			TranscendSkillPage6 = skillPage;
			break;
		}
	}

	public void SetTranscendSkillSlotIndex(int page, int transcended, int skillSlotIndex, string skillPage = "")
	{
		CreatureData.TranscendSkill transcendSkillData = CreatureData.GetTranscendSkillData(transcended, skillSlotIndex);
		if (transcendSkillData != null)
		{
			TranscendSkillPageInfo transcendSkillPageInfo = GetTranscendSkillPageInfo(page);
			if (transcendSkillPageInfo == null)
			{
				transcendSkillPageInfo = new TranscendSkillPageInfo
				{
					Page = page
				};
				TranscendSkillPages.Add(page, transcendSkillPageInfo);
			}
			transcendSkillPageInfo.TranscendSkillDatas.Add(transcendSkillData);
			SetTranscendSkillPageStringArray(page, skillPage);
		}
	}

	public void ResetAllTranscendSkillSlotIndex(int page)
	{
		TranscendSkillPageInfo transcendSkillPageInfo = GetTranscendSkillPageInfo(page);
		if (transcendSkillPageInfo != null)
		{
			SetTranscendSkillPageStringArray(page, "[]");
			transcendSkillPageInfo.TranscendSkillDatas.Clear();
		}
	}

	public bool AnyTranscendSkillLearned(int page)
	{
		TranscendSkillPageInfo transcendSkillPageInfo = GetTranscendSkillPageInfo(page);
		if (transcendSkillPageInfo == null)
		{
			return false;
		}
		return transcendSkillPageInfo.TranscendSkillDatas.Count > 0;
	}

	public bool IsLearnTranscendSkill(int page, int skillIndex)
	{
		TranscendSkillPageInfo transcendSkillPageInfo = GetTranscendSkillPageInfo(page);
		if (transcendSkillPageInfo == null)
		{
			return false;
		}
		if (transcendSkillPageInfo.TranscendSkillDatas.Find((CreatureData.TranscendSkill skill) => skill.TranscendSkillIndex == skillIndex) == null)
		{
			return false;
		}
		return true;
	}

	public TranscendSkillPageInfo GetTranscendSkillPageInfo(int page = 0)
	{
		if (page == 0)
		{
			page = SkillPage;
		}
		if (TranscendSkillPages == null)
		{
			InitTranscendSkillPages();
		}
		if (TranscendSkillPages.ContainsKey(page))
		{
			return TranscendSkillPages[page];
		}
		return null;
	}

	public int GetUsableTranscendSkillPoint(int page)
	{
		int num = GetCurrentTotalTrancendPoint();
		TranscendSkillPageInfo transcendSkillPageInfo = GetTranscendSkillPageInfo(page);
		if (transcendSkillPageInfo == null)
		{
			return num;
		}
		foreach (CreatureData.TranscendSkill transcendSkillData in transcendSkillPageInfo.TranscendSkillDatas)
		{
			if (transcendSkillData != null)
			{
				num -= transcendSkillData.ReqSKillPoint;
			}
		}
		return num;
	}

	public int GetOpenedTranscendSkillPageCount()
	{
		if (TranscendSkillPages == null)
		{
			InitTranscendSkillPages();
		}
		if (TranscendSkillPages == null)
		{
			return 0;
		}
		return TranscendSkillPages.Count;
	}

	public bool IsRemainLearnableTranscendSkill(int page)
	{
		CreatureData creatureData = CreatureData;
		int usableTranscendSkillPoint = GetUsableTranscendSkillPoint(page);
		for (int i = 1; i <= 5; i++)
		{
			int[] transcendSkill = creatureData.GetTranscendSkill(i);
			int[] reqSkillPoint = creatureData.GetReqSkillPoint(i);
			if (transcendSkill == null)
			{
				continue;
			}
			for (int j = 0; j < transcendSkill.Length; j++)
			{
				if (!IsLearnTranscendSkill(page, j))
				{
					int num = reqSkillPoint[j];
					if (usableTranscendSkillPoint > num)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public int GetDefaultTranscendPoint()
	{
		return CreatureData.GetSumTranscendSkillPoint(Transcended, LimitBreakLevel);
	}

	public int GetCurrentTotalTrancendPoint()
	{
		int defaultTranscendPoint = GetDefaultTranscendPoint();
		int extendTranscendPoint = GetExtendTranscendPoint();
		return defaultTranscendPoint + extendTranscendPoint;
	}

	public int GetExtendTranscendPoint()
	{
		if (TranscendPoint <= 0)
		{
			return 0;
		}
		return TranscendPoint;
	}

	public void SetExtendTranscendPoint(int extnedTranscendPoint)
	{
		TranscendPoint = (short)extnedTranscendPoint;
	}

	public static bool IntegrityTranscendSkill(HeroInfo lhs, HeroInfo rhs, out string error)
	{
		error = "";
		if (lhs.SkillPage != rhs.SkillPage)
		{
			error = "SkillPage_" + lhs.SkillPage + "_" + rhs.SkillPage;
			return false;
		}
		TranscendSkillPageInfo transcendSkillPageInfo = lhs.GetTranscendSkillPageInfo();
		TranscendSkillPageInfo transcendSkillPageInfo2 = rhs.GetTranscendSkillPageInfo();
		if (transcendSkillPageInfo.TranscendSkillDatas.Count != transcendSkillPageInfo2.TranscendSkillDatas.Count)
		{
			error = "SkillCount_" + transcendSkillPageInfo.TranscendSkillDatas.Count + "_" + transcendSkillPageInfo2.TranscendSkillDatas.Count;
			return false;
		}
		foreach (CreatureData.TranscendSkill lSkillData in transcendSkillPageInfo.TranscendSkillDatas)
		{
			if (transcendSkillPageInfo2.TranscendSkillDatas.Find((CreatureData.TranscendSkill skill) => skill.TranscendSkillIndex == lSkillData.TranscendSkillIndex) == null)
			{
				error = "TranscendSkillIndex_" + lSkillData.TranscendSkillIndex + "_NULL";
				return false;
			}
		}
		return true;
	}

	public static Tuple<string, int, int> GetIntegrityCheckResult(HeroInfo l, HeroInfo r)
	{
		if (l.HeroIndex != r.HeroIndex)
		{
			return new Tuple<string, int, int>("HeroIndex", l.HeroIndex, r.HeroIndex);
		}
		if (l.Level != r.Level)
		{
			return new Tuple<string, int, int>("Level", l.Level, r.Level);
		}
		if (l.Star != r.Star)
		{
			return new Tuple<string, int, int>("Star", l.Star, r.Star);
		}
		if (l.SkillLevel1 != r.SkillLevel1)
		{
			return new Tuple<string, int, int>("SkillLevel1", l.SkillLevel1, r.SkillLevel1);
		}
		if (l.SkillLevel2 != r.SkillLevel2)
		{
			return new Tuple<string, int, int>("SkillLevel2", l.SkillLevel2, r.SkillLevel2);
		}
		if (l.SkillLevel3 != r.SkillLevel3)
		{
			return new Tuple<string, int, int>("SkillLevel3", l.SkillLevel3, r.SkillLevel3);
		}
		if (l.SkillLevel4 != r.SkillLevel4)
		{
			return new Tuple<string, int, int>("SkillLevel4", l.SkillLevel4, r.SkillLevel4);
		}
		if (l.SkillLevel5 != r.SkillLevel5)
		{
			return new Tuple<string, int, int>("SkillLevel5", l.SkillLevel5, r.SkillLevel5);
		}
		return null;
	}

	public int GetAccessoryIndex(AccessoryPartType partType)
	{
		switch ((int)partType)
		{
		case 1:
			return AccessoryCostumeIndex1;
		case 2:
			return AccessoryCostumeIndex2;
		case 3:
			return AccessoryCostumeIndex3;
		case 4:
			return AccessoryCostumeIndex4;
		case 5:
			return AccessoryCostumeIndex5;
		case 6:
			return AccessoryCostumeIndex6;
		default:
			VespaDebug.LogError("GetAccessorySlotIndex failed : partType=" + partType);
			return 0;
		}
	}

	protected bool SetAccessorySlotIndex(AccessoryPartType partType, int accessoryIndex)
	{
		switch ((int)partType)
		{
		case 1:
			AccessoryCostumeIndex1 = accessoryIndex;
			return true;
		case 2:
			AccessoryCostumeIndex2 = accessoryIndex;
			return true;
		case 3:
			AccessoryCostumeIndex3 = accessoryIndex;
			return true;
		case 4:
			AccessoryCostumeIndex4 = accessoryIndex;
			return true;
		case 5:
			AccessoryCostumeIndex5 = accessoryIndex;
			return true;
		case 6:
			AccessoryCostumeIndex6 = accessoryIndex;
			return true;
		default:
			VespaDebug.LogError("SetAccessorySlotIndex failed : partType=" + partType);
			return false;
		}
	}

	public void SetDefaultCostume()
	{
		CostumeIndex = 0;
		HideUniqueWeapon = 1;
		WeaponCostumeIndex = 0;
		HairCostumeIndex = 0;
		AccessoryCostumeIndex1 = 0;
		AccessoryCostumeIndex2 = 0;
		AccessoryCostumeIndex3 = 0;
		AccessoryCostumeIndex4 = 0;
		AccessoryCostumeIndex5 = 0;
		AccessoryCostumeIndex6 = 0;
	}

	public bool IsMaxLimitBreakLevel()
	{
		return true;
	}

	public bool CanIncLimitBreakExp()
	{
		return false;
	}

	public bool CanIncLimitBreakLevel()
	{
		return false;
	}
}
