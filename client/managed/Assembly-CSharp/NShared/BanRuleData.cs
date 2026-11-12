using System;
using System.Collections.Generic;
using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class BanRuleData : IDataKeySelector<int>
{
	[IgnoreMember]
	public const int MaxBanCount = 3;

	private RaidBanRuleData[] _raidBanRuleDataAry;

	int IDataKeySelector<int>.Key => Index;

	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public RaidBanType BanType1
	{
		get
		{
			return GetRaidBanRuleData(0).BanType;
		}
		set
		{
			GetRaidBanRuleData(0).BanType = value;
		}
	}

	[Key(2)]
	public string BanValue1
	{
		get
		{
			return GetRaidBanRuleData(0).BanValue;
		}
		set
		{
			GetRaidBanRuleData(0).BanValue = value;
		}
	}

	[Key(3)]
	public RaidBanType BanType2
	{
		get
		{
			return GetRaidBanRuleData(1).BanType;
		}
		set
		{
			GetRaidBanRuleData(1).BanType = value;
		}
	}

	[Key(4)]
	public string BanValue2
	{
		get
		{
			return GetRaidBanRuleData(1).BanValue;
		}
		set
		{
			GetRaidBanRuleData(1).BanValue = value;
		}
	}

	[Key(5)]
	public RaidBanType BanType3
	{
		get
		{
			return GetRaidBanRuleData(2).BanType;
		}
		set
		{
			GetRaidBanRuleData(2).BanType = value;
		}
	}

	[Key(6)]
	public string BanValue3
	{
		get
		{
			return GetRaidBanRuleData(2).BanValue;
		}
		set
		{
			GetRaidBanRuleData(2).BanValue = value;
		}
	}

	[Key(7)]
	public string RuleName { get; set; }

	[Key(8)]
	public string RuleTactic { get; set; }

	[Key(9)]
	public CreatureTagType[] SelectClassType { get; set; }

	[Key(10)]
	public CreatureTagType[] AcceptedClassType { get; set; }

	private bool IsVaildIndex(int index)
	{
		if (index < 0)
		{
			return false;
		}
		if (3 <= index)
		{
			return false;
		}
		return true;
	}

	public RaidBanRuleData[] GetBanRuleDataAry()
	{
		return _raidBanRuleDataAry;
	}

	public RaidBanRuleData GetRaidBanRuleData(int index)
	{
		if (!IsVaildIndex(index))
		{
			return null;
		}
		if (_raidBanRuleDataAry == null)
		{
			_raidBanRuleDataAry = new RaidBanRuleData[3];
			for (int i = 0; i < 3; i++)
			{
				_raidBanRuleDataAry[i] = new RaidBanRuleData();
			}
		}
		return _raidBanRuleDataAry[index];
	}

	public static bool GetCreatureTagType(string banValue, out CreatureTagType type)
	{
		type = CreatureTagType.Archer;
		try
		{
			type = (CreatureTagType)Enum.Parse(typeof(CreatureTagType), banValue);
		}
		catch (Exception)
		{
			return false;
		}
		return true;
	}

	public static bool GetSkillAttrType(string banValue, out SkillAttrType type)
	{
		type = SkillAttrType.Magical;
		try
		{
			type = (SkillAttrType)Enum.Parse(typeof(SkillAttrType), banValue);
		}
		catch (Exception)
		{
			return false;
		}
		return true;
	}

	public static string[] GetCodeNames(string banValue)
	{
		if (string.IsNullOrEmpty(banValue))
		{
			return null;
		}
		return banValue.Split(',');
	}

	public static bool GetClassEnterPermissionValue(string banValue, out List<CreatureTagType> heroTagTypeList, out int heroEnterCount, out bool isAll)
	{
		heroTagTypeList = null;
		heroEnterCount = 0;
		isAll = false;
		if (string.IsNullOrEmpty(banValue))
		{
			return false;
		}
		string[] array = banValue.Split(',');
		if (array.Length < 2)
		{
			return false;
		}
		if (!int.TryParse(array[0], out var result))
		{
			return false;
		}
		heroEnterCount = result;
		if (heroTagTypeList == null)
		{
			heroTagTypeList = new List<CreatureTagType>();
		}
		for (int i = 1; i < array.Length; i++)
		{
			if (array[i].Equals("All", StringComparison.OrdinalIgnoreCase))
			{
				heroTagTypeList.Clear();
				isAll = true;
				return true;
			}
			if (!GetCreatureTagType(array[i], out var type))
			{
				return false;
			}
			heroTagTypeList.Add(type);
		}
		return true;
	}
}
