using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NUtil.NDataHelper;
using NShared;

namespace NGame2.NUI.NWindow;

public class BanRuleContainer
{
	private int[] _addtionalBanRuleIndices;

	private List<BanRuleData> _listBanRuleData = new List<BanRuleData>();

	private List<CreatureTagType> _listSelectClassType = new List<CreatureTagType>();

	private List<CreatureTagType> _listAcceptedClassType = new List<CreatureTagType>();

	private List<CreatureData> _listBanCreatureData = new List<CreatureData>();

	private RaidPartySetting.ClassEnterPermissionBanTypeData _classEnterPermissionBanTypeData;

	private BanRuleData _defaultBanRuleData;

	public BanRuleContainer()
	{
	}

	public BanRuleContainer(BanRuleData banRuleData)
	{
		SetDefaultBanRuleData(banRuleData);
	}

	public void SetDefaultBanRuleData(BanRuleData banRuleData)
	{
		if (_defaultBanRuleData != banRuleData)
		{
			_defaultBanRuleData = banRuleData;
			SetBanRuleData();
		}
	}

	public void SetAddtionalBanRuleData(int[] banRuleIndices)
	{
		if (!IsEquals(_addtionalBanRuleIndices, banRuleIndices))
		{
			_addtionalBanRuleIndices = banRuleIndices;
			SetBanRuleData();
		}
	}

	private void Reset()
	{
		_listBanRuleData.Clear();
		_listSelectClassType.Clear();
		_listAcceptedClassType.Clear();
		_listBanCreatureData.Clear();
		_classEnterPermissionBanTypeData = null;
	}

	private void SetBanRuleData()
	{
		Reset();
		if (_defaultBanRuleData != null)
		{
			AddBanRuleData(_defaultBanRuleData);
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_addtionalBanRuleIndices); i++)
		{
			int key = _addtionalBanRuleIndices[i];
			BanRuleData data = GameRoot.Instance.BanRuleDataContainer.GetData(key);
			AddBanRuleData(data);
		}
	}

	private void AddBanRuleData(BanRuleData banRuleData)
	{
		if (banRuleData == null)
		{
			return;
		}
		_listBanRuleData.Add(banRuleData);
		CreatureTagType[] selectClassType = banRuleData.SelectClassType;
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(selectClassType))
		{
			_listSelectClassType.AddRange(selectClassType);
		}
		CreatureTagType[] acceptedClassType = banRuleData.AcceptedClassType;
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(acceptedClassType))
		{
			_listAcceptedClassType.AddRange(acceptedClassType);
		}
		RaidBanRuleData[] banRuleDataAry = banRuleData.GetBanRuleDataAry();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(banRuleDataAry); i++)
		{
			RaidBanRuleData raidBanRuleData = banRuleDataAry[i];
			if (raidBanRuleData == null)
			{
				continue;
			}
			switch (raidBanRuleData.BanType)
			{
			case RaidBanType.CodeName:
			{
				List<CreatureData> collection = BanRuleDataHelper.ConvBanListStringToIntList(raidBanRuleData.BanValue);
				_listBanCreatureData.AddRange(collection);
				break;
			}
			case RaidBanType.ClassEnterPermission:
			{
				Tuple<List<CreatureTagType>, int> classEnterBanRuleData = RaidHelper.GetClassEnterBanRuleData(raidBanRuleData);
				if (classEnterBanRuleData != null)
				{
					_classEnterPermissionBanTypeData = new RaidPartySetting.ClassEnterPermissionBanTypeData();
					_classEnterPermissionBanTypeData.LimitEnterClassList = classEnterBanRuleData.Item1;
					_classEnterPermissionBanTypeData.PossibleEnterCount = classEnterBanRuleData.Item2;
					_classEnterPermissionBanTypeData.RuleTactic = banRuleData.RuleTactic;
				}
				break;
			}
			}
		}
	}

	private bool IsEquals(int[] lhs, int[] rhs)
	{
		int num = __Extension_Method_ICollection__.COUNT(lhs);
		int num2 = __Extension_Method_ICollection__.COUNT(rhs);
		if (num != num2)
		{
			return false;
		}
		if (num == 0)
		{
			return true;
		}
		Array.Sort(lhs, Compare);
		Array.Sort(rhs, Compare);
		return lhs.SequenceEqual(rhs);
	}

	private int Compare(int x, int y)
	{
		return x.CompareTo(y);
	}

	public List<CreatureTagType> GetSelectClassTagTypeList()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_listSelectClassType))
		{
			return null;
		}
		return _listSelectClassType;
	}

	public List<CreatureTagType> GetAccpetedClassTagTypeList()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_listAcceptedClassType))
		{
			return null;
		}
		return _listAcceptedClassType;
	}

	public List<CreatureData> GetBannedHeroDataList()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_listBanCreatureData))
		{
			return null;
		}
		return _listBanCreatureData;
	}

	public RaidPartySetting.ClassEnterPermissionBanTypeData GetClassEnterPermissionBanTypeData()
	{
		return _classEnterPermissionBanTypeData;
	}

	public List<BanRuleData> GetBanRuleDataList()
	{
		return _listBanRuleData;
	}
}
