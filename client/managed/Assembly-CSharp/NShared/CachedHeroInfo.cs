using System;
using System.Collections.Generic;
using System.Linq;
using NVespa;

namespace NShared;

[JsonSerializable(Javascript = true)]
public class CachedHeroInfo : HeroInfo
{
	public enum EquipResult
	{
		Success,
		Fail,
		TagMismatch,
		LowLevel,
		CreatureMismatch,
		PartMismatch
	}

	private readonly HeroInfo _heroInfo;

	public EquipItemInfo EquipItemInfo1 { get; set; }

	public EquipItemInfo EquipItemInfo2 { get; set; }

	public EquipItemInfo EquipItemInfo3 { get; set; }

	public EquipItemInfo EquipItemInfo4 { get; set; }

	public EquipItemInfo EquipItemInfo5 { get; set; }

	public EquipItemInfo EquipItemInfo6 { get; set; }

	public EquipItemInfo EquipItemInfo7 { get; set; }

	public EquipItemInfo EquipItemInfo8 { get; set; }

	public EquipItemInfo EquipItemInfo9 { get; set; }

	public EquipItemInfo EquipItemInfo10 { get; set; }

	public byte FirstSkillSlotIndex { get; set; }

	public override int HeroIndex
	{
		get
		{
			return _heroInfo.HeroIndex;
		}
		set
		{
			_heroInfo.HeroIndex = value;
		}
	}

	public override short Level
	{
		get
		{
			return _heroInfo.Level;
		}
		set
		{
			_heroInfo.Level = value;
		}
	}

	public override int Exp
	{
		get
		{
			return _heroInfo.Exp;
		}
		set
		{
			_heroInfo.Exp = value;
		}
	}

	public override sbyte Star
	{
		get
		{
			return _heroInfo.Star;
		}
		set
		{
			_heroInfo.Star = value;
		}
	}

	public override string CreatedTime
	{
		get
		{
			return _heroInfo.CreatedTime;
		}
		set
		{
			_heroInfo.CreatedTime = value;
		}
	}

	public override short SkillLevel1
	{
		get
		{
			return _heroInfo.Level;
		}
		set
		{
			_heroInfo.SkillLevel1 = value;
		}
	}

	public override short SkillLevel2
	{
		get
		{
			return _heroInfo.Level;
		}
		set
		{
			_heroInfo.SkillLevel2 = value;
		}
	}

	public override short SkillLevel3
	{
		get
		{
			return _heroInfo.Level;
		}
		set
		{
			_heroInfo.SkillLevel3 = value;
		}
	}

	public override short SkillLevel4
	{
		get
		{
			return _heroInfo.Level;
		}
		set
		{
			_heroInfo.SkillLevel4 = value;
		}
	}

	public override short SkillLevel5
	{
		get
		{
			return _heroInfo.Level;
		}
		set
		{
			_heroInfo.SkillLevel5 = value;
		}
	}

	public override int EquipItemSlotIndex1
	{
		get
		{
			return _heroInfo.EquipItemSlotIndex1;
		}
		set
		{
			_heroInfo.EquipItemSlotIndex1 = value;
		}
	}

	public override int EquipItemSlotIndex2
	{
		get
		{
			return _heroInfo.EquipItemSlotIndex2;
		}
		set
		{
			_heroInfo.EquipItemSlotIndex2 = value;
		}
	}

	public override int EquipItemSlotIndex3
	{
		get
		{
			return _heroInfo.EquipItemSlotIndex3;
		}
		set
		{
			_heroInfo.EquipItemSlotIndex3 = value;
		}
	}

	public override int EquipItemSlotIndex4
	{
		get
		{
			return _heroInfo.EquipItemSlotIndex4;
		}
		set
		{
			_heroInfo.EquipItemSlotIndex4 = value;
		}
	}

	public override int EquipItemSlotIndex5
	{
		get
		{
			return _heroInfo.EquipItemSlotIndex5;
		}
		set
		{
			_heroInfo.EquipItemSlotIndex5 = value;
		}
	}

	public override int EquipItemSlotIndex6
	{
		get
		{
			return _heroInfo.EquipItemSlotIndex6;
		}
		set
		{
			_heroInfo.EquipItemSlotIndex6 = value;
		}
	}

	public override int EquipItemSlotIndex7
	{
		get
		{
			return _heroInfo.EquipItemSlotIndex7;
		}
		set
		{
			_heroInfo.EquipItemSlotIndex7 = value;
		}
	}

	public override int EquipItemSlotIndex8
	{
		get
		{
			return _heroInfo.EquipItemSlotIndex8;
		}
		set
		{
			_heroInfo.EquipItemSlotIndex8 = value;
		}
	}

	public override int EquipItemSlotIndex9
	{
		get
		{
			return _heroInfo.EquipItemSlotIndex9;
		}
		set
		{
			_heroInfo.EquipItemSlotIndex9 = value;
		}
	}

	public override int EquipItemSlotIndex10
	{
		get
		{
			return _heroInfo.EquipItemSlotIndex10;
		}
		set
		{
			_heroInfo.EquipItemSlotIndex10 = value;
		}
	}

	public override short SkillExtend1
	{
		get
		{
			return _heroInfo.SkillExtend1;
		}
		set
		{
			_heroInfo.SkillExtend1 = value;
		}
	}

	public override short SkillExtend2
	{
		get
		{
			return _heroInfo.SkillExtend2;
		}
		set
		{
			_heroInfo.SkillExtend2 = value;
		}
	}

	public override short SkillExtend3
	{
		get
		{
			return _heroInfo.SkillExtend3;
		}
		set
		{
			_heroInfo.SkillExtend3 = value;
		}
	}

	public override short SkillExtend4
	{
		get
		{
			return _heroInfo.SkillExtend4;
		}
		set
		{
			_heroInfo.SkillExtend4 = value;
		}
	}

	public override short SkillExtend5
	{
		get
		{
			return _heroInfo.SkillExtend5;
		}
		set
		{
			_heroInfo.SkillExtend5 = value;
		}
	}

	public override sbyte EnterDungeon
	{
		get
		{
			return _heroInfo.EnterDungeon;
		}
		set
		{
			_heroInfo.EnterDungeon = value;
		}
	}

	public override short Purified
	{
		get
		{
			return _heroInfo.Purified;
		}
		set
		{
			_heroInfo.Purified = value;
		}
	}

	public override sbyte Transcended
	{
		get
		{
			return _heroInfo.Transcended;
		}
		set
		{
			_heroInfo.Transcended = value;
		}
	}

	public override short TranscendPoint
	{
		get
		{
			return _heroInfo.TranscendPoint;
		}
		set
		{
			_heroInfo.TranscendPoint = value;
		}
	}

	public override sbyte LimitBreakLevel
	{
		get
		{
			return _heroInfo.LimitBreakLevel;
		}
		set
		{
			_heroInfo.LimitBreakLevel = value;
		}
	}

	public override long LimitBreakExp
	{
		get
		{
			return _heroInfo.LimitBreakExp;
		}
		set
		{
			_heroInfo.LimitBreakExp = value;
		}
	}

	public override int FlaskItemIndex
	{
		get
		{
			return _heroInfo.FlaskItemIndex;
		}
		set
		{
			_heroInfo.FlaskItemIndex = value;
		}
	}

	public override int FlaskExp
	{
		get
		{
			return _heroInfo.FlaskExp;
		}
		set
		{
			_heroInfo.FlaskExp = value;
		}
	}

	public override int CostumeIndex
	{
		get
		{
			return _heroInfo.CostumeIndex;
		}
		set
		{
			_heroInfo.CostumeIndex = value;
		}
	}

	public override byte HideUniqueWeapon
	{
		get
		{
			return _heroInfo.HideUniqueWeapon;
		}
		set
		{
			_heroInfo.HideUniqueWeapon = value;
		}
	}

	public override int WeaponCostumeIndex
	{
		get
		{
			return _heroInfo.WeaponCostumeIndex;
		}
		set
		{
			_heroInfo.WeaponCostumeIndex = value;
		}
	}

	public override int HairCostumeIndex
	{
		get
		{
			return _heroInfo.HairCostumeIndex;
		}
		set
		{
			_heroInfo.HairCostumeIndex = value;
		}
	}

	public override int AccessoryCostumeIndex1
	{
		get
		{
			return _heroInfo.AccessoryCostumeIndex1;
		}
		set
		{
			_heroInfo.AccessoryCostumeIndex1 = value;
		}
	}

	public override int AccessoryCostumeIndex2
	{
		get
		{
			return _heroInfo.AccessoryCostumeIndex2;
		}
		set
		{
			_heroInfo.AccessoryCostumeIndex2 = value;
		}
	}

	public override int AccessoryCostumeIndex3
	{
		get
		{
			return _heroInfo.AccessoryCostumeIndex3;
		}
		set
		{
			_heroInfo.AccessoryCostumeIndex3 = value;
		}
	}

	public override int AccessoryCostumeIndex4
	{
		get
		{
			return _heroInfo.AccessoryCostumeIndex4;
		}
		set
		{
			_heroInfo.AccessoryCostumeIndex4 = value;
		}
	}

	public override int AccessoryCostumeIndex5
	{
		get
		{
			return _heroInfo.AccessoryCostumeIndex5;
		}
		set
		{
			_heroInfo.AccessoryCostumeIndex5 = value;
		}
	}

	public override int AccessoryCostumeIndex6
	{
		get
		{
			return _heroInfo.AccessoryCostumeIndex6;
		}
		set
		{
			_heroInfo.AccessoryCostumeIndex6 = value;
		}
	}

	public virtual PositionInfo AccessoryPositionInfo1 { get; set; }

	public virtual PositionInfo AccessoryPositionInfo2 { get; set; }

	public virtual PositionInfo AccessoryPositionInfo3 { get; set; }

	public virtual PositionInfo AccessoryPositionInfo4 { get; set; }

	public virtual PositionInfo AccessoryPositionInfo5 { get; set; }

	public virtual PositionInfo AccessoryPositionInfo6 { get; set; }

	public override int ApplySkillPage
	{
		get
		{
			return _heroInfo.ApplySkillPage;
		}
		set
		{
			_heroInfo.ApplySkillPage = value;
		}
	}

	public override int SkillPage => _heroInfo.SkillPage;

	public override string TranscendSkillPage1
	{
		get
		{
			return _heroInfo.TranscendSkillPage1;
		}
		set
		{
			_heroInfo.TranscendSkillPage1 = value;
		}
	}

	public override string TranscendSkillPage2
	{
		get
		{
			return _heroInfo.TranscendSkillPage2;
		}
		set
		{
			_heroInfo.TranscendSkillPage2 = value;
		}
	}

	public override string TranscendSkillPage3
	{
		get
		{
			return _heroInfo.TranscendSkillPage3;
		}
		set
		{
			_heroInfo.TranscendSkillPage3 = value;
		}
	}

	public override string TranscendSkillPage4
	{
		get
		{
			return _heroInfo.TranscendSkillPage4;
		}
		set
		{
			_heroInfo.TranscendSkillPage4 = value;
		}
	}

	public override string TranscendSkillPage5
	{
		get
		{
			return _heroInfo.TranscendSkillPage5;
		}
		set
		{
			_heroInfo.TranscendSkillPage5 = value;
		}
	}

	public override string TranscendSkillPage6
	{
		get
		{
			return _heroInfo.TranscendSkillPage6;
		}
		set
		{
			_heroInfo.TranscendSkillPage6 = value;
		}
	}

	public override int ClosenessPoint
	{
		get
		{
			return _heroInfo.ClosenessPoint;
		}
		set
		{
			_heroInfo.ClosenessPoint = value;
		}
	}

	public override string LastGreetingTime
	{
		get
		{
			return _heroInfo.LastGreetingTime;
		}
		set
		{
			_heroInfo.LastGreetingTime = value;
		}
	}

	public override string LastConversationTime
	{
		get
		{
			return _heroInfo.LastConversationTime;
		}
		set
		{
			_heroInfo.LastConversationTime = value;
		}
	}

	public override string LastGiftTime
	{
		get
		{
			return _heroInfo.LastGiftTime;
		}
		set
		{
			_heroInfo.LastGiftTime = value;
		}
	}

	public override string MaxRewardedTime
	{
		get
		{
			return _heroInfo.MaxRewardedTime;
		}
		set
		{
			_heroInfo.MaxRewardedTime = value;
		}
	}

	public override string LastRouletteTime
	{
		get
		{
			return _heroInfo.LastRouletteTime;
		}
		set
		{
			_heroInfo.LastRouletteTime = value;
		}
	}

	public override int ApplyRunePage
	{
		get
		{
			return _heroInfo.ApplyRunePage;
		}
		set
		{
			_heroInfo.ApplyRunePage = value;
		}
	}

	public int BookmarkPriority { get; set; }

	public PunishmentRuneOptionInfo[] PunishmentRuneOptionInfos { get; set; }

	public CachedHeroInfo()
	{
		_heroInfo = new HeroInfo();
	}

	public CachedHeroInfo(HeroInfo heroInfo, IEnumerable<EquipItemInfo> equipItemInfos = null, Func<int, int, PositionInfo> accessoryPositionGetter = null, PunishmentRuneOptionInfo[] punishmentOptionInfos = null)
	{
		if (punishmentOptionInfos == null && heroInfo is CachedHeroInfo cachedHeroInfo)
		{
			punishmentOptionInfos = cachedHeroInfo.GetPunishmentRuneOptionInfos().ToArray();
		}
		_heroInfo = heroInfo;
		if (equipItemInfos != null)
		{
			foreach (EquipSlotData allEquipSlotDatum in EquipSlotHelper.GetAllEquipSlotData())
			{
				int equipSlotIndex = GetEquipItemSlotIndex(allEquipSlotDatum.Index);
				SetEquipItemInfo(allEquipSlotDatum.Index, equipItemInfos.FirstOrDefault((EquipItemInfo e) => e.SlotIndex == equipSlotIndex));
			}
		}
		if (accessoryPositionGetter != null)
		{
			foreach (AccessoryPartType item in AccessoryPartTypeHelper.PartTypes())
			{
				int accessoryIndex = GetAccessoryIndex(item);
				if (accessoryIndex != 0)
				{
					PositionInfo positionInfo = accessoryPositionGetter(HeroIndex, accessoryIndex);
					SetAccessoryCostumePositionInfo(item, positionInfo);
				}
			}
		}
		if (heroInfo.HeroRunePageInfos != null || punishmentOptionInfos != null)
		{
			base.HeroRunePageInfos = heroInfo.HeroRunePageInfos;
			PunishmentRuneOptionInfos = punishmentOptionInfos;
			InitRunePages();
		}
	}

	public CachedHeroInfo(CreatureData creatureData)
	{
		if (creatureData != null)
		{
			_heroInfo = new HeroInfo();
			HeroIndex = creatureData.Index;
			Star = (sbyte)((creatureData.StartHeroStar <= 0) ? 1 : creatureData.StartHeroStar);
			Level = (short)((creatureData.StartHeroLevel <= 0) ? 1 : creatureData.StartHeroLevel);
		}
	}

	protected override void InitRunePages()
	{
		RunePageInfoDic = new Dictionary<int, List<HeroRunePageInfo>>();
		if (base.HeroRunePageInfos == null || base.HeroRunePageInfos.Length == 0)
		{
			AddRunePage(HeroInfo.GetDefaultRunePage());
			return;
		}
		Dictionary<int, PunishmentRuneOptionInfo> dictionary = new Dictionary<int, PunishmentRuneOptionInfo>();
		if (PunishmentRuneOptionInfos != null)
		{
			PunishmentRuneOptionInfo[] punishmentRuneOptionInfos = PunishmentRuneOptionInfos;
			foreach (PunishmentRuneOptionInfo punishmentRuneOptionInfo in punishmentRuneOptionInfos)
			{
				if (!dictionary.ContainsKey(punishmentRuneOptionInfo.EquipItemSlotIndex))
				{
					dictionary.Add(punishmentRuneOptionInfo.EquipItemSlotIndex, punishmentRuneOptionInfo);
				}
			}
		}
		HeroRunePageInfo[] heroRunePageInfos = base.HeroRunePageInfos;
		foreach (HeroRunePageInfo heroRunePageInfo in heroRunePageInfos)
		{
			PunishmentRuneOptionInfo optionInfo = null;
			if (dictionary.ContainsKey(heroRunePageInfo.EquipItemSlotIndex))
			{
				optionInfo = dictionary[heroRunePageInfo.EquipItemSlotIndex];
			}
			SetRunePage(heroRunePageInfo, optionInfo);
		}
	}

	public void SetEquipItemInfo(int partIndex, EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo != null)
		{
			SetEquipItemSlotIndex(partIndex, equipItemInfo.SlotIndex);
			GetEquipItemInfoByPartIndex(partIndex)?.SetEquippedHeroIndex(-1);
			equipItemInfo.SetEquippedHeroIndex(HeroIndex);
		}
		else
		{
			SetEquipItemSlotIndex(partIndex, IsEquipSlotLocked(partIndex) ? (-1) : 0);
			GetEquipItemInfoByPartIndex(partIndex)?.SetEquippedHeroIndex(-1);
		}
		ChangeEquipItemInfo(partIndex, equipItemInfo);
	}

	public void SetEquipItemInfo_InMainThread(int partIndex, EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo != null)
		{
			SetEquipItemSlotIndex(partIndex, equipItemInfo.SlotIndex);
			GetEquipItemInfoByPartIndex(partIndex)?.SetEquippedHeroIndex_InMainThread(-1);
			equipItemInfo.SetEquippedHeroIndex_InMainThread(HeroIndex);
		}
		else
		{
			SetEquipItemSlotIndex(partIndex, IsEquipSlotLocked(partIndex) ? (-1) : 0);
			GetEquipItemInfoByPartIndex(partIndex)?.SetEquippedHeroIndex_InMainThread(-1);
		}
		ChangeEquipItemInfo(partIndex, equipItemInfo);
	}

	public void ChangeEquipItemInfo(int partIndex, EquipItemInfo equipItemInfo)
	{
		switch (partIndex)
		{
		case 1:
			EquipItemInfo1 = equipItemInfo;
			break;
		case 2:
			EquipItemInfo2 = equipItemInfo;
			break;
		case 3:
			EquipItemInfo3 = equipItemInfo;
			break;
		case 4:
			EquipItemInfo4 = equipItemInfo;
			break;
		case 5:
			EquipItemInfo5 = equipItemInfo;
			break;
		case 6:
			EquipItemInfo6 = equipItemInfo;
			break;
		case 7:
			EquipItemInfo7 = equipItemInfo;
			break;
		case 8:
			EquipItemInfo8 = equipItemInfo;
			break;
		case 9:
			EquipItemInfo9 = equipItemInfo;
			break;
		case 10:
			EquipItemInfo10 = equipItemInfo;
			break;
		default:
			VespaDebug.LogError("SetEquipItemInfo(info) failed : partIndex=" + partIndex);
			break;
		}
	}

	public EquipItemInfo GetEquipItemInfoByPartIndex(int partIndex)
	{
		return partIndex switch
		{
			1 => EquipItemInfo1, 
			2 => EquipItemInfo2, 
			3 => EquipItemInfo3, 
			4 => EquipItemInfo4, 
			5 => EquipItemInfo5, 
			6 => EquipItemInfo6, 
			7 => EquipItemInfo7, 
			8 => EquipItemInfo8, 
			9 => EquipItemInfo9, 
			10 => EquipItemInfo10, 
			_ => null, 
		};
	}

	public List<EquipItemInfo> GetEquipItemInfoList()
	{
		List<EquipItemInfo> list = new List<EquipItemInfo>();
		foreach (EquipSlotData allEquipSlotDatum in EquipSlotHelper.GetAllEquipSlotData())
		{
			EquipItemInfo equipItemInfoByPartIndex = GetEquipItemInfoByPartIndex(allEquipSlotDatum.Index);
			if (equipItemInfoByPartIndex != null)
			{
				list.Add(equipItemInfoByPartIndex);
			}
		}
		return list;
	}

	public Dictionary<int, EquipItemInfo> GetSlotIndexEquipItemPair()
	{
		Dictionary<int, EquipItemInfo> dictionary = new Dictionary<int, EquipItemInfo>();
		foreach (EquipSlotData allEquipSlotDatum in EquipSlotHelper.GetAllEquipSlotData())
		{
			EquipItemInfo equipItemInfoByPartIndex = GetEquipItemInfoByPartIndex(allEquipSlotDatum.Index);
			if (equipItemInfoByPartIndex != null)
			{
				dictionary[allEquipSlotDatum.Index] = equipItemInfoByPartIndex;
			}
		}
		return dictionary;
	}

	public EquipItemInfo[] GetEquipItemInfos()
	{
		return GetEquipItemInfoList().ToArray();
	}

	public EquipItemInfo GetEquipItemInfoByPartType(EquipPartType partType)
	{
		int equipSlotIndex = partType.GetEquipSlotIndex();
		return GetEquipItemInfoByPartIndex(equipSlotIndex);
	}

	public void GetEquipOptionDataList(List<EquipOptionCreatedData> openedOptionDataList, List<EquipOptionCreatedData> closedOptionDataList, List<EquipOptionCreatedData> fixedOptionDataList)
	{
		new List<EquipOptionData>();
		foreach (KeyValuePair<int, EquipItemInfo> item in GetSlotIndexEquipItemPair())
		{
			int key = item.Key;
			EquipItemInfo value = item.Value;
			int star = value.Star;
			EquipSlotData data = GameRoot.Instance.EquipSlotDataContainer.GetData(key);
			if (!data.IgnoreRandomOption)
			{
				int num = 1;
				while (true)
				{
					EquipItemOptionInfo optionInfo = value.GetOptionInfo(num);
					if (optionInfo == null)
					{
						break;
					}
					EquipOptionData data2 = GameRoot.Instance.EquipOptionDataContainer.GetData(optionInfo.OptionIndex);
					if (data2 != null)
					{
						(value.IsOptionOpened(num) ? openedOptionDataList : closedOptionDataList)?.Add(new EquipOptionCreatedData(data2, star, optionInfo.OptionStep));
					}
					num++;
				}
			}
			if (!data.IgnoreFixedOption)
			{
				int num2 = 1;
				while (true)
				{
					ItemData<EquipItemData> data3 = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(value.ItemIndex);
					if (data3 == null)
					{
						break;
					}
					int fixedOptionIndex = data3.SubData.GetFixedOptionIndex(num2);
					if (fixedOptionIndex == 0)
					{
						break;
					}
					EquipOptionData data4 = GameRoot.Instance.EquipOptionDataContainer.GetData(fixedOptionIndex);
					if (data4 != null)
					{
						fixedOptionDataList?.Add(new EquipOptionCreatedData(data4, star, 0));
					}
					num2++;
				}
			}
			if (!data.IgnoreEnchantOption)
			{
				int num3 = 1;
				while (true)
				{
					int enchantOptionIndex = value.GetEnchantOptionIndex(num3);
					if (enchantOptionIndex == 0)
					{
						break;
					}
					EquipOptionData data5 = GameRoot.Instance.EquipOptionDataContainer.GetData(enchantOptionIndex);
					if (data5 != null)
					{
						int enchantOptionStep = value.GetEnchantOptionStep(num3);
						openedOptionDataList?.Add(new EquipOptionCreatedData(data5, star, enchantOptionStep));
					}
					num3++;
				}
			}
			int num4 = 1;
			while (true)
			{
				int extraOptionIndex = value.GetExtraOptionIndex(num4);
				if (extraOptionIndex == 0)
				{
					break;
				}
				EquipOptionData data6 = GameRoot.Instance.EquipOptionDataContainer.GetData(extraOptionIndex);
				if (data6 != null)
				{
					int extraOptionStep = value.GetExtraOptionStep(num4);
					openedOptionDataList?.Add(new EquipOptionCreatedData(data6, star, extraOptionStep));
				}
				num4++;
			}
		}
	}

	public void GetEquipRuneDataList(List<ItemData<RuneItemData>> runeItemDataList)
	{
		new List<EquipOptionData>();
		EquipItemInfo[] equipItemInfos = GetEquipItemInfos();
		for (int i = 0; i < equipItemInfos.Length; i++)
		{
			List<int> runeItemIndexList = equipItemInfos[i].GetRuneItemIndexList();
			for (int j = 0; j < runeItemIndexList.Count; j++)
			{
				int itemIndex = runeItemIndexList[j];
				ItemData<RuneItemData> data = GameRoot.Instance.ItemDataContainer.GetData<RuneItemData>(itemIndex);
				if (data == null)
				{
					VespaDebug.LogError("RuneItemData not found : " + itemIndex);
				}
				else
				{
					runeItemDataList.Add(data);
				}
			}
		}
	}

	public void RefreshRuneInfo()
	{
		PunishmentRuneOptionInfos = GetPunishmentRuneOptionInfos().ToArray();
		base.HeroRunePageInfos = GetRunePageInfos().ToArray();
	}

	public EquipResult CheckEquipResult(EquipItemData equipItemData)
	{
		if (equipItemData == null)
		{
			return EquipResult.Fail;
		}
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(HeroIndex);
		if (data == null)
		{
			return EquipResult.Fail;
		}
		if (!data.HasTag(equipItemData.TagType))
		{
			return EquipResult.TagMismatch;
		}
		if (equipItemData.CreatureIndex != null && equipItemData.CreatureIndex.Length != 0 && !TypeIterator.Contains(equipItemData.CreatureIndex, HeroIndex))
		{
			return EquipResult.CreatureMismatch;
		}
		EquipPartType partType = equipItemData.PartType;
		if ((uint)(partType - 1) > 6u)
		{
			return EquipResult.PartMismatch;
		}
		short level = Level;
		short reqLevel = equipItemData.ReqLevel;
		if (level < reqLevel)
		{
			return EquipResult.LowLevel;
		}
		return EquipResult.Success;
	}

	public void Validate()
	{
		ValidateSkillLevel();
	}

	private void ValidateSkillLevel()
	{
		for (int i = 1; i <= 5; i++)
		{
			int num = GetSkillLevel(i, isUserHero: true);
			if (num > Level)
			{
				num = Level;
			}
			if (GameRoot.Instance.ConstantDataContainer.GetValueInt("ReqHeroLevelSkillSlot" + i) > Level)
			{
				num = 0;
			}
			SetSkillLevel(i, num);
			if (num <= 0)
			{
				SetSkillExtend(i, 0);
			}
		}
	}

	private void GetSetOptionList(Dictionary<EquipSetBonusData, int> setIndexCounter, out List<EquipOptionData> openedOptionDataList, out List<EquipOptionData> closedOptionDataList)
	{
		openedOptionDataList = null;
		closedOptionDataList = null;
		if (setIndexCounter == null)
		{
			return;
		}
		foreach (KeyValuePair<EquipSetBonusData, int> item in setIndexCounter)
		{
			EquipSetBonusData key = item.Key;
			int value = item.Value;
			key.GetOptionIndexList(value, out var openedOptionIndexList, out var closedOptionIndexList);
			if (openedOptionIndexList != null)
			{
				for (int i = 0; i < openedOptionIndexList.Count; i++)
				{
					int key2 = openedOptionIndexList[i];
					EquipOptionData data = GameRoot.Instance.EquipOptionDataContainer.GetData(key2);
					if (data != null)
					{
						openedOptionDataList = openedOptionDataList ?? new List<EquipOptionData>();
						openedOptionDataList.Add(data);
					}
				}
			}
			if (closedOptionIndexList == null)
			{
				continue;
			}
			for (int j = 0; j < closedOptionIndexList.Count; j++)
			{
				int key3 = closedOptionIndexList[j];
				EquipOptionData data2 = GameRoot.Instance.EquipOptionDataContainer.GetData(key3);
				if (data2 != null)
				{
					closedOptionDataList = closedOptionDataList ?? new List<EquipOptionData>();
					closedOptionDataList.Add(data2);
				}
			}
		}
	}

	public void GetSelectedSetOptionList(EquipItemInfo _selectedEquipItemInfo, out List<EquipOptionData> openedOptionDataList, out List<EquipOptionData> closedOptionDataList)
	{
		openedOptionDataList = null;
		closedOptionDataList = null;
		EquipSetBonusData equipSetBonusData = _selectedEquipItemInfo.GetEquipSetBonusData();
		if (equipSetBonusData == null)
		{
			return;
		}
		Dictionary<EquipSetBonusData, int> dictionary = new Dictionary<EquipSetBonusData, int>();
		dictionary[equipSetBonusData] = 0;
		List<EquipItemInfo> equipItemInfoList = GetEquipItemInfoList();
		int num = equipItemInfoList?.Count ?? 0;
		for (int i = 0; i < num; i++)
		{
			EquipItemInfo equipItemInfo = equipItemInfoList[i];
			if (equipItemInfo != null)
			{
				EquipSetBonusData equipSetBonusData2 = equipItemInfo.GetEquipSetBonusData();
				if (equipSetBonusData2 != null && equipSetBonusData.SetIndex == equipSetBonusData2.SetIndex)
				{
					int value = 0;
					dictionary.TryGetValue(equipSetBonusData2, out value);
					dictionary[equipSetBonusData2] = value + 1;
				}
			}
		}
		GetSetOptionList(dictionary, out openedOptionDataList, out closedOptionDataList);
	}

	public void GetSetOptionList(out List<EquipOptionData> openedOptionDataList, out List<EquipOptionData> closedOptionDataList)
	{
		openedOptionDataList = null;
		closedOptionDataList = null;
		List<EquipItemInfo> equipItemInfoList = GetEquipItemInfoList();
		if (equipItemInfoList == null || equipItemInfoList.Count == 0)
		{
			return;
		}
		Dictionary<EquipSetBonusData, int> dictionary = null;
		for (int i = 0; i < equipItemInfoList.Count; i++)
		{
			EquipItemInfo equipItemInfo = equipItemInfoList[i];
			if (equipItemInfo != null)
			{
				EquipSetBonusData equipSetBonusData = equipItemInfo.GetEquipSetBonusData();
				if (equipSetBonusData != null)
				{
					int value = 0;
					dictionary = dictionary ?? new Dictionary<EquipSetBonusData, int>();
					dictionary.TryGetValue(equipSetBonusData, out value);
					dictionary[equipSetBonusData] = value + 1;
				}
			}
		}
		GetSetOptionList(dictionary, out openedOptionDataList, out closedOptionDataList);
	}

	public void SetAccessoryCostume(AccessoryPartType partType, int accessoryIndex, PositionInfo positionInfo)
	{
		SetAccessorySlotIndex(partType, accessoryIndex);
		SetAccessoryCostumePositionInfo(partType, positionInfo);
	}

	public bool SetAccessoryCostumePositionInfo(AccessoryPartType partType, PositionInfo positionInfo)
	{
		switch ((int)partType)
		{
		case 1:
			AccessoryPositionInfo1 = positionInfo;
			return true;
		case 2:
			AccessoryPositionInfo2 = positionInfo;
			return true;
		case 3:
			AccessoryPositionInfo3 = positionInfo;
			return true;
		case 4:
			AccessoryPositionInfo4 = positionInfo;
			return true;
		case 5:
			AccessoryPositionInfo5 = positionInfo;
			return true;
		case 6:
			AccessoryPositionInfo6 = positionInfo;
			return true;
		default:
			VespaDebug.LogError("SetAccessoryCostumePositionInfo failed : partType=" + partType);
			return false;
		}
	}

	public PositionInfo GetAccessoryPositionInfo(AccessoryPartType partType)
	{
		switch ((int)partType)
		{
		case 1:
			return AccessoryPositionInfo1;
		case 2:
			return AccessoryPositionInfo2;
		case 3:
			return AccessoryPositionInfo3;
		case 4:
			return AccessoryPositionInfo4;
		case 5:
			return AccessoryPositionInfo5;
		case 6:
			return AccessoryPositionInfo6;
		default:
			VespaDebug.LogError("GetAccessoryPositionInfo failed : partType=" + partType);
			return null;
		}
	}

	public void SetCostumeInfo(CachedHeroCostumeInfo costumeInfo)
	{
		HeroIndex = costumeInfo.HeroIndex;
		CostumeIndex = costumeInfo.CostumeIndex;
		HairCostumeIndex = costumeInfo.HairCostumeIndex;
		HideUniqueWeapon = costumeInfo.HideUniqueWeapon;
		WeaponCostumeIndex = costumeInfo.WeaponCostumeIndex;
		AccessoryCostumeIndex1 = costumeInfo.AccessoryCostumeIndex1;
		AccessoryCostumeIndex2 = costumeInfo.AccessoryCostumeIndex2;
		AccessoryCostumeIndex3 = costumeInfo.AccessoryCostumeIndex3;
		AccessoryCostumeIndex4 = costumeInfo.AccessoryCostumeIndex4;
		AccessoryCostumeIndex5 = costumeInfo.AccessoryCostumeIndex5;
		AccessoryCostumeIndex6 = costumeInfo.AccessoryCostumeIndex6;
		AccessoryPositionInfo1 = costumeInfo.AccessoryPositionInfo1;
		AccessoryPositionInfo2 = costumeInfo.AccessoryPositionInfo2;
		AccessoryPositionInfo3 = costumeInfo.AccessoryPositionInfo3;
		AccessoryPositionInfo4 = costumeInfo.AccessoryPositionInfo4;
		AccessoryPositionInfo5 = costumeInfo.AccessoryPositionInfo5;
		AccessoryPositionInfo6 = costumeInfo.AccessoryPositionInfo6;
	}

	public CachedHeroCostumeInfo GetCostumeInfo()
	{
		return new CachedHeroCostumeInfo
		{
			HeroIndex = HeroIndex,
			CostumeIndex = CostumeIndex,
			HairCostumeIndex = HairCostumeIndex,
			HideUniqueWeapon = HideUniqueWeapon,
			WeaponCostumeIndex = WeaponCostumeIndex,
			AccessoryCostumeIndex1 = AccessoryCostumeIndex1,
			AccessoryCostumeIndex2 = AccessoryCostumeIndex2,
			AccessoryCostumeIndex3 = AccessoryCostumeIndex3,
			AccessoryCostumeIndex4 = AccessoryCostumeIndex4,
			AccessoryCostumeIndex5 = AccessoryCostumeIndex5,
			AccessoryCostumeIndex6 = AccessoryCostumeIndex6,
			AccessoryPositionInfo1 = AccessoryPositionInfo1,
			AccessoryPositionInfo2 = AccessoryPositionInfo2,
			AccessoryPositionInfo3 = AccessoryPositionInfo3,
			AccessoryPositionInfo4 = AccessoryPositionInfo4,
			AccessoryPositionInfo5 = AccessoryPositionInfo5,
			AccessoryPositionInfo6 = AccessoryPositionInfo6
		};
	}

	public void ClearAccessoryCostumes()
	{
		AccessoryCostumeIndex1 = 0;
		AccessoryCostumeIndex2 = 0;
		AccessoryCostumeIndex3 = 0;
		AccessoryCostumeIndex4 = 0;
		AccessoryCostumeIndex5 = 0;
		AccessoryCostumeIndex6 = 0;
		AccessoryPositionInfo1 = null;
		AccessoryPositionInfo2 = null;
		AccessoryPositionInfo3 = null;
		AccessoryPositionInfo4 = null;
		AccessoryPositionInfo5 = null;
		AccessoryPositionInfo6 = null;
	}

	public SoulWeaponInfo GetSoulWeaponInfo()
	{
		EquipItemInfo equipItemInfoByPartType = GetEquipItemInfoByPartType(EquipPartType.Weapon);
		if (equipItemInfoByPartType == null)
		{
			return null;
		}
		if (!equipItemInfoByPartType.IsSoulWeapon())
		{
			return null;
		}
		return equipItemInfoByPartType.SoulWeaponInfo;
	}

	public int[] GetSoulWeaponActiveConditionSkillIndices()
	{
		return GetSoulWeaponInfo()?.GetSoulWeaponSkillData()?.ActiveConditionSkillIndices;
	}

	public int GetSoulWeaponBasicSkillIndex()
	{
		SoulWeaponInfo soulWeaponInfo = GetSoulWeaponInfo();
		if (soulWeaponInfo == null)
		{
			return 0;
		}
		return soulWeaponInfo.GetSoulWeaponSkillData()?.BasicSkillIndex ?? 0;
	}

	public int[] GetSoulWeaponAddSkillIndices()
	{
		return GetSoulWeaponInfo()?.GetAddSkillIndices();
	}
}
