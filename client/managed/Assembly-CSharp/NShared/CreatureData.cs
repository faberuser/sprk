using System;
using System.Collections.Generic;
using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable]
public class CreatureData : IDataKeySelector<int>, IPayShopEventSearchInfo, IValanceCreatureFilterInfo, IValanceFilterInfo
{
	[JsonSerializable]
	public class TranscendSkill
	{
		public int Transcended { get; set; }

		public int Index { get; set; }

		public int TranscendSkillIndex { get; set; }

		public int ReqSKillPoint { get; set; }

		public int Level { get; set; }

		public int MaxLevel { get; set; }

		public bool IsHeroSkillTranscend()
		{
			if (Transcended != 3)
			{
				return Transcended == 4;
			}
			return true;
		}
	}

	private const string PARAM_CREATURE = "[creature]";

	private static int[][] _TanscendSkillTable = new int[5][]
	{
		new int[4] { 60000001, 60000002, 60000003, 60000004 },
		new int[3] { 60000009, 60000010, 60000011 },
		new int[4] { 60000012, 60000035, 60000029, 60000026 },
		new int[3] { 60000027, 60000028, 60000039 },
		new int[4] { 60000013, 60000014, 60000015, 60000016 }
	};

	private static int[][] _TanscendReqSkillPointTable = new int[5][]
	{
		new int[4] { 5, 5, 5, 5 },
		new int[4] { 5, 5, 5, 5 },
		new int[4] { 5, 5, 5, 5 },
		new int[4] { 5, 5, 5, 5 },
		new int[4] { 5, 5, 5, 5 }
	};

	[Key(0)]
	public int Index { get; set; }

	[Key(2)]
	public string ReplaceNameKey { get; set; }

	[Key(3)]
	public string[] NameParam { get; set; }

	[Key(4)]
	public string CodeName { get; set; }

	[Key(5)]
	public string ModelCode { get; set; }

	[Key(6)]
	public string ModelPrefix { get; set; }

	[Key(7)]
	public CreatureOpenType OpenType { get; set; }

	[Key(8)]
	public CreatureOpenStatusType OpenStatus { get; set; }

	[Key(9)]
	public int OpenDate { get; set; }

	[Key(10)]
	public CreatureRaceType Race { get; set; }

	[Key(11)]
	public CreatureTagType TagType { get; set; }

	[Key(12)]
	public SkillAttrType[] AttrType { get; set; }

	[Key(13)]
	public SkillAttrType[] WeakType { get; set; }

	[Key(14)]
	public string ArchiveStory { get; set; }

	[Key(15)]
	public ArchiveType ArchiveType { get; set; }

	[Key(16)]
	public AttackAttrType AttackType { get; set; }

	[Key(17)]
	public AttackAttrType[] AttackWeakType { get; set; }

	[Key(18)]
	public CreaturePositionType PositionType { get; set; }

	[Key(19)]
	public int PositionWeight { get; set; }

	[Key(20)]
	public int SizeMm { get; set; }

	[Key(21)]
	public bool BlockDash { get; set; }

	[Key(22)]
	public int HeightMm { get; set; }

	[Key(23)]
	public string[] IgnoreCc { get; set; }

	[Key(24)]
	public CreatureTransformType TransformType { get; set; }

	[Key(25)]
	public SkillTargetType[] AvoidSkillTargetType { get; set; }

	[Key(26)]
	public int OffsetMx { get; set; }

	[Key(27)]
	public int OffsetMz { get; set; }

	[Key(28)]
	public string GradeStoneItemCode { get; set; }

	[Key(29)]
	public string StarStoneItemCode { get; set; }

	[Key(30)]
	public int DropItemWeight { get; set; }

	[Key(31)]
	public int DropGoldWeight { get; set; }

	[Key(32)]
	public int BaseSkillIndex { get; set; }

	[Key(33)]
	public int SkillIndex1 { get; set; }

	[Key(34)]
	public int SkillIndex2 { get; set; }

	[Key(35)]
	public int SkillIndex3 { get; set; }

	[Key(36)]
	public int SkillIndex4 { get; set; }

	[Key(37)]
	public int SkillIndex5 { get; set; }

	[Key(38)]
	public int LeaderSkillIndex { get; set; }

	[Key(39)]
	public int[] SkillExtend1 { get; set; }

	[Key(40)]
	public int[] SkillExtend2 { get; set; }

	[Key(41)]
	public int[] SkillExtend3 { get; set; }

	[Key(42)]
	public int[] SkillExtend4 { get; set; }

	[Key(43)]
	public int[] SkillExtend5 { get; set; }

	[Key(44)]
	public int[] LinkedSkillIndex1 { get; set; }

	[Key(45)]
	public int[] LinkedSkillIndex2 { get; set; }

	[Key(46)]
	public int[] LinkedSkillIndex3 { get; set; }

	[Key(47)]
	public int[] LinkedSkillIndex4 { get; set; }

	[Key(48)]
	public int[] LinkedSkillIndex5 { get; set; }

	[Key(49)]
	public int[] ExtraSkill { get; set; }

	[Key(50)]
	public int[] ExtraActiveSkill { get; set; }

	[Key(51)]
	public int[] ShowInfoSkill { get; set; }

	[Key(52)]
	public int[] GetTranscendSkillPoint { get; set; }

	[Key(53)]
	public int[] TranscendSkill1 { get; set; }

	[Key(54)]
	public int[] ReqSkillPoint1 { get; set; }

	[Key(55)]
	public int[] TranscendSkill2 { get; set; }

	[Key(56)]
	public int[] ReqSkillPoint2 { get; set; }

	[Key(57)]
	public int[] TranscendSkill3 { get; set; }

	[Key(58)]
	public int[] ReqSkillPoint3 { get; set; }

	[Key(59)]
	public int[] TranscendSkill4 { get; set; }

	[Key(60)]
	public int[] ReqSkillPoint4 { get; set; }

	[Key(61)]
	public int[] TranscendSkill5 { get; set; }

	[Key(62)]
	public int[] ReqSkillPoint5 { get; set; }

	[Key(63)]
	public sbyte StartHeroStar { get; set; }

	[Key(64)]
	public int StartHeroLevel { get; set; }

	[Key(66)]
	public string ReplaceDescKey { get; set; }

	[Key(67)]
	public string[] DescParam { get; set; }

	[Key(69)]
	public string ReplaceTitleKey { get; set; }

	[Key(70)]
	public string[] TitleParam { get; set; }

	[Key(72)]
	public string ClearTip { get; set; }

	[Key(73)]
	public int AddStamina { get; set; }

	[Key(74)]
	public bool IsDestroyOnDied { get; set; }

	[Key(75)]
	public string HeroPositionIcon1 { get; set; }

	[Key(76)]
	public string HeroPositionText1 { get; set; }

	[Key(77)]
	public string HeroPositionIcon2 { get; set; }

	[Key(78)]
	public string HeroPositionText2 { get; set; }

	[Key(79)]
	public string HeroPositionIcon3 { get; set; }

	[Key(80)]
	public string HeroPositionText3 { get; set; }

	[Key(97)]
	public string HeroPositionIcon4 { get; set; }

	[Key(98)]
	public string HeroPositionText4 { get; set; }

	[Key(81)]
	public int[] TranscendRecommend1 { get; set; }

	[Key(82)]
	public int[] TranscendRecommend2 { get; set; }

	[Key(83)]
	public int[] TranscendRecommend3 { get; set; }

	[Key(84)]
	public string[] EquipoptionRecommend1 { get; set; }

	[Key(85)]
	public string[] EquipoptionRecommend2 { get; set; }

	[Key(86)]
	public string[] EquipoptionRecommend3 { get; set; }

	[Key(87)]
	public int SoulWeaponBasicSkillIndex { get; set; }

	[Key(93)]
	public byte Grade { get; set; }

	[Key(94)]
	public int[] PetOptionIndices { get; set; }

	[Key(95)]
	public int[] PetBonusIndices { get; set; }

	[Key(96)]
	public bool PlayableCharacter { get; set; }

	[Key(99)]
	public int MaxFood { get; set; }

	[Key(100)]
	public bool IsLinePet { get; set; }

	private string NAME_KEY => "CREATURE_NAME_" + Index;

	private string DESC_KEY => "CREATURE_DESC_" + Index;

	private string TITLE_KEY => "CREATURE_TITLE_" + Index;

	private string TRIBE_KEY => "CREATURE_RACE_" + Race.ToString().ToUpper();

	[IgnoreMember]
	public string Name => GameRoot.LocalizeProxy.GetCache_or_Build(this, "Name", BuildLocalize_Name);

	[IgnoreMember]
	public string Description => GameRoot.LocalizeProxy.GetCache_or_Build(this, "Description", BuildLocalize_Description);

	[IgnoreMember]
	public string Title => GameRoot.LocalizeProxy.GetCache_or_Build(this, "Title", BuildLocalize_Title);

	[IgnoreMember]
	public string Tribe => GameRoot.LocalizeProxy.GetCache_or_Build(this, "Tribe", BuildLocalize_Tribe);

	int IDataKeySelector<int>.Key => Index;

	PayShopCategory IPayShopEventSearchInfo.PayShopCategory => PayShopCategory.Hero;

	int IPayShopEventSearchInfo.Index => Index;

	string IPayShopEventSearchInfo.Group => PositionType.ToString();

	SkillAttrType IValanceCreatureFilterInfo.AttrType => HeroAttrType;

	CreatureTagType IValanceCreatureFilterInfo.CreatureTagType => TagType;

	[IgnoreMember]
	public SkillAttrType HeroAttrType
	{
		get
		{
			if (Race != CreatureRaceType.Hero)
			{
				return SkillAttrType.None;
			}
			if (AttrType == null || AttrType.Length == 0)
			{
				return SkillAttrType.None;
			}
			return AttrType[0];
		}
	}

	[IgnoreMember]
	public CreatureGrade CreatureGrade => (CreatureGrade)Grade;

	private string BuildLocalize_Name()
	{
		string[] array = ConvertLocalizeParam(NameParam);
		ILocalizeProxy localizeProxy = GameRoot.LocalizeProxy;
		string nAME_KEY = NAME_KEY;
		string replaceNameKey = ReplaceNameKey;
		object[] values = array;
		return localizeProxy.GetStringWithReplaceKey(nAME_KEY, replaceNameKey, values);
	}

	private string BuildLocalize_Description()
	{
		string[] array = ConvertLocalizeParam(DescParam);
		ILocalizeProxy localizeProxy = GameRoot.LocalizeProxy;
		string dESC_KEY = DESC_KEY;
		string replaceDescKey = ReplaceDescKey;
		object[] values = array;
		return localizeProxy.GetStringWithReplaceKey(dESC_KEY, replaceDescKey, values);
	}

	private string BuildLocalize_Title()
	{
		string[] array = ConvertLocalizeParam(TitleParam);
		ILocalizeProxy localizeProxy = GameRoot.LocalizeProxy;
		string tITLE_KEY = TITLE_KEY;
		string replaceTitleKey = ReplaceTitleKey;
		object[] values = array;
		return localizeProxy.GetStringWithReplaceKey(tITLE_KEY, replaceTitleKey, values);
	}

	private string BuildLocalize_Tribe()
	{
		return GameRoot.LocalizeProxy.GetString(TRIBE_KEY);
	}

	private string[] ConvertLocalizeParam(string[] paramArr)
	{
		if (paramArr == null || paramArr.Length == 0)
		{
			return paramArr;
		}
		string[] array = null;
		string[] array2 = paramArr;
		for (int i = 0; i < paramArr.Length; i++)
		{
			if (array2[i].Equals("[creature]", StringComparison.OrdinalIgnoreCase))
			{
				if (array == null)
				{
					array = new string[paramArr.Length];
					Array.Copy(paramArr, array, paramArr.Length);
					array2 = array;
				}
				array2[i] = GetOriginalCreatureName();
			}
		}
		return array2;
	}

	private string GetOriginalCreatureName()
	{
		if (string.IsNullOrEmpty(ModelCode))
		{
			return string.Empty;
		}
		if (CodeName == ModelCode)
		{
			return string.Empty;
		}
		CreatureData dataByCode = GameRoot.Instance.CreatureDataContainer.GetDataByCode(ModelCode);
		if (dataByCode != null)
		{
			return dataByCode.Name;
		}
		return string.Empty;
	}

	public override string ToString()
	{
		return "Index=" + Index + " (" + Name + ")";
	}

	public bool IsStaticPosition()
	{
		return (TransformType & CreatureTransformType.StaticPosition) != 0;
	}

	public CreaturePositionType GetPositionType()
	{
		return PositionType;
	}

	public CreatureDPSType GetDPSType()
	{
		if (PositionType == CreaturePositionType.Forward)
		{
			return CreatureDPSType.Melee;
		}
		return CreatureDPSType.Range;
	}

	public bool HasTag(CreatureTagType tagType)
	{
		return TagType == tagType;
	}

	public bool HasTag(CreatureTagType[] tagTypes)
	{
		if (tagTypes == null)
		{
			return true;
		}
		return tagTypes?.Contains(TagType) ?? false;
	}

	public List<int> GetSkillIndexList(bool includeExtra)
	{
		List<int> list = new List<int>();
		list.Add(SkillIndex1);
		list.Add(SkillIndex2);
		list.Add(SkillIndex3);
		list.Add(SkillIndex4);
		list.Add(SkillIndex5);
		if (includeExtra && ExtraSkill != null)
		{
			list.AddRange(ExtraSkill);
		}
		return list;
	}

	public bool IsActiveSkill(int skillIndex)
	{
		if (skillIndex == 0)
		{
			return false;
		}
		for (int i = 1; i <= 5; i++)
		{
			if (GetSkillIndexBySlot(i) == skillIndex)
			{
				return true;
			}
		}
		return false;
	}

	public int GetSkillIndexBySlot(int slotNo, int linkedIndex = 0)
	{
		if (linkedIndex == 0)
		{
			switch (slotNo)
			{
			case 1:
				return SkillIndex1;
			case 2:
				return SkillIndex2;
			case 3:
				return SkillIndex3;
			case 4:
				return SkillIndex4;
			case 5:
				return SkillIndex5;
			case 10:
				return SoulWeaponBasicSkillIndex;
			}
		}
		else
		{
			int[] linkedSkillIndexBySlot = GetLinkedSkillIndexBySlot(slotNo);
			if (linkedSkillIndexBySlot != null && linkedSkillIndexBySlot.Length >= linkedIndex)
			{
				return linkedSkillIndexBySlot[linkedIndex - 1];
			}
		}
		return 0;
	}

	public int GetSkillSlotByIndex(int skillIndex, bool includeLinkedSkill = false)
	{
		if (SkillIndex1 == skillIndex)
		{
			return 1;
		}
		if (SkillIndex2 == skillIndex)
		{
			return 2;
		}
		if (SkillIndex3 == skillIndex)
		{
			return 3;
		}
		if (SkillIndex4 == skillIndex)
		{
			return 4;
		}
		if (SkillIndex5 == skillIndex)
		{
			return 5;
		}
		if (SoulWeaponBasicSkillIndex == skillIndex)
		{
			return 10;
		}
		if (includeLinkedSkill)
		{
			if (IsExistLinkedSkill(skillIndex, LinkedSkillIndex1))
			{
				return 1;
			}
			if (IsExistLinkedSkill(skillIndex, LinkedSkillIndex2))
			{
				return 2;
			}
			if (IsExistLinkedSkill(skillIndex, LinkedSkillIndex3))
			{
				return 3;
			}
			if (IsExistLinkedSkill(skillIndex, LinkedSkillIndex4))
			{
				return 4;
			}
			if (IsExistLinkedSkill(skillIndex, LinkedSkillIndex5))
			{
				return 5;
			}
		}
		return 0;
	}

	private bool IsExistLinkedSkill(int skillIndex, int[] linkedSkillIndices)
	{
		if (!HasLinkedSkill(linkedSkillIndices))
		{
			return false;
		}
		if (Array.Exists(linkedSkillIndices, (int linkedSkillIndex) => linkedSkillIndex == skillIndex))
		{
			return true;
		}
		return false;
	}

	public int[] GetSkillExtend(int skillslotIndex)
	{
		return skillslotIndex switch
		{
			1 => SkillExtend1, 
			2 => SkillExtend2, 
			3 => SkillExtend3, 
			4 => SkillExtend4, 
			5 => SkillExtend5, 
			_ => null, 
		};
	}

	public int[] GetLinkedSkillIndexBySlot(int slotNo)
	{
		return slotNo switch
		{
			1 => LinkedSkillIndex1, 
			2 => LinkedSkillIndex2, 
			3 => LinkedSkillIndex3, 
			4 => LinkedSkillIndex4, 
			5 => LinkedSkillIndex5, 
			_ => null, 
		};
	}

	public bool HasLinkedSkill(int slotNo)
	{
		return slotNo switch
		{
			1 => HasLinkedSkill(LinkedSkillIndex1), 
			2 => HasLinkedSkill(LinkedSkillIndex2), 
			3 => HasLinkedSkill(LinkedSkillIndex3), 
			4 => HasLinkedSkill(LinkedSkillIndex4), 
			5 => HasLinkedSkill(LinkedSkillIndex5), 
			_ => false, 
		};
	}

	private static bool HasLinkedSkill(int[] linkedSkillIndex)
	{
		if (linkedSkillIndex != null && linkedSkillIndex.Length != 0)
		{
			return true;
		}
		return false;
	}

	public int[] GetTranscendSkill(int transcended)
	{
		return transcended switch
		{
			1 => TranscendSkill1, 
			2 => TranscendSkill2, 
			3 => TranscendSkill3, 
			4 => TranscendSkill4, 
			5 => TranscendSkill5, 
			_ => null, 
		};
	}

	public int[] GetReqSkillPoint(int transcended)
	{
		return transcended switch
		{
			1 => ReqSkillPoint1, 
			2 => ReqSkillPoint2, 
			3 => ReqSkillPoint3, 
			4 => ReqSkillPoint4, 
			5 => ReqSkillPoint5, 
			_ => null, 
		};
	}

	public TranscendSkill GetTranscendSkillData(int transcended, int index)
	{
		int[] transcendSkill = GetTranscendSkill(transcended);
		if (transcendSkill == null || transcendSkill.Length <= index)
		{
			return null;
		}
		int[] reqSkillPoint = GetReqSkillPoint(transcended);
		if (reqSkillPoint == null)
		{
			return null;
		}
		return new TranscendSkill
		{
			Transcended = transcended,
			Index = index,
			TranscendSkillIndex = transcendSkill[index],
			ReqSKillPoint = reqSkillPoint[index]
		};
	}

	public TranscendSkill GetTranscendSkillData(int skillIndex)
	{
		for (int i = 1; i <= 5; i++)
		{
			int[] transcendSkill = GetTranscendSkill(i);
			if (transcendSkill == null)
			{
				continue;
			}
			for (int j = 0; j < transcendSkill.Length; j++)
			{
				if (transcendSkill[j] == skillIndex)
				{
					int[] reqSkillPoint = GetReqSkillPoint(i);
					return new TranscendSkill
					{
						Transcended = i,
						Index = j,
						TranscendSkillIndex = transcendSkill[j],
						ReqSKillPoint = reqSkillPoint[j]
					};
				}
			}
		}
		return null;
	}

	public int GetSumTranscendSkillPoint(int transcended, int limitBreakLevel)
	{
		int num = 0;
		for (int i = 0; i < transcended; i++)
		{
			num += GetTranscendSkillPoint[i];
		}
		if (limitBreakLevel > 0)
		{
			int addTotalTranscendSkillPoint = GameRoot.Instance.HeroLimitBreakDataContainer.GetAddTotalTranscendSkillPoint(Index, limitBreakLevel);
			num += addTotalTranscendSkillPoint;
		}
		return num;
	}

	public object GetFieldData(CreatureDataFieldType fieldType, int valueIndex)
	{
		switch (fieldType)
		{
		case CreatureDataFieldType.IgnoreCc:
			if (IgnoreCc == null || valueIndex >= IgnoreCc.Length)
			{
				return null;
			}
			return IgnoreCc[valueIndex];
		case CreatureDataFieldType.AvoidSkillTargetType:
			if (AvoidSkillTargetType == null || valueIndex >= AvoidSkillTargetType.Length)
			{
				return null;
			}
			return AvoidSkillTargetType[valueIndex].GetString();
		default:
			return null;
		}
	}

	public List<Tuple<string, string>> GetHeroPositionList()
	{
		return new List<Tuple<string, string>>
		{
			new Tuple<string, string>(HeroPositionIcon1, HeroPositionText1),
			new Tuple<string, string>(HeroPositionIcon2, HeroPositionText2),
			new Tuple<string, string>(HeroPositionIcon3, HeroPositionText3),
			new Tuple<string, string>(HeroPositionIcon4, HeroPositionText4)
		};
	}

	public Tuple<string, string> GetHeroPositionData(int num)
	{
		return num switch
		{
			1 => new Tuple<string, string>(HeroPositionIcon1, HeroPositionText1), 
			2 => new Tuple<string, string>(HeroPositionIcon2, HeroPositionText2), 
			3 => new Tuple<string, string>(HeroPositionIcon3, HeroPositionText3), 
			4 => new Tuple<string, string>(HeroPositionIcon4, HeroPositionText4), 
			_ => null, 
		};
	}

	public bool IsValidRecommendTranscendData()
	{
		if ((TranscendRecommend1 == null || TranscendRecommend1.Length == 0) && (TranscendRecommend2 == null || TranscendRecommend2.Length == 0) && (TranscendRecommend3 == null || TranscendRecommend3.Length == 0))
		{
			return false;
		}
		return true;
	}
}
