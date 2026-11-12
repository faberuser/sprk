using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NUI.NHelper.NPet;
using NGame2.NUI.NManager;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NPet;

public class PetManager : GlobalAutoBehaviour<PetManager>, IUserObserver<NGame2.NAccount.UserPetManager.ChangedPetInfo>, IUserObserver
{
	public enum PetBuffType
	{
		None,
		CommonBuff,
		ContentsBuff,
		Bonus
	}

	public enum EPetCompareType
	{
		Defualt,
		PetManagement
	}

	public class PetBuffDesc
	{
		public string OptionName;

		public string OptionValue;
	}

	private SortedDictionary<int, DefaultExtraStatData[]> _dicCachedPetStatData = new SortedDictionary<int, DefaultExtraStatData[]>();

	private SortedDictionary<int, PetStatDataBase[]> _dicCachedPetContentsStatData = new SortedDictionary<int, PetStatDataBase[]>();

	private SortedDictionary<int, PetBonusDataBase[]> _dicCachedPetBonusData = new SortedDictionary<int, PetBonusDataBase[]>();

	private List<DefaultExtraStatData> _tempStatList = new List<DefaultExtraStatData>();

	private List<PetStatDataBase> _tempContentsStatList = new List<PetStatDataBase>();

	private List<PetBonusDataBase> _tempBonusList = new List<PetBonusDataBase>();

	private HashSet<int> _changedPetStatTypes = new HashSet<int>();

	private HashSet<int> _changedPetContentsStatIndices = new HashSet<int>();

	private HashSet<int> _changedPetBonusIndices = new HashSet<int>();

	private LocalAccountString _changedPetStatLocalInfos = new LocalAccountString("", "ChangedPetStatLocalInfos");

	private LocalAccountString _changedPetContentsStatLocalInfos = new LocalAccountString("", "ChangedPetContentsStatLocalInfos");

	private LocalAccountString _changedPetBonusLocalInfos = new LocalAccountString("", "ChangedPetBonusLocalInfos");

	public void Init()
	{
		if (IsValidPetManager())
		{
			GlobalAutoBehaviour<AccountManager>.instance.PetManager.ObserverManager.AddObserver(this);
			InitChangedPetBuffInfo();
			RefreshAllPetBuff();
		}
	}

	private void RefreshAllPetBuff()
	{
		ClearCachedDic();
		PetInfo[] values = GlobalAutoBehaviour<AccountManager>.instance.PetManager.GetValues();
		if (__Extension_Method_ICollection__.COUNT(values) == 0)
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(values); i++)
		{
			PetInfo petInfo = values[i];
			if (petInfo != null)
			{
				AddOrUpdatePetBuff(petInfo);
			}
		}
	}

	private void ApplyChangedPetInfo(int petIndex)
	{
		PetInfo petInfo = GlobalAutoBehaviour<AccountManager>.instance.PetManager.Get(petIndex);
		if (petInfo != null)
		{
			AddOrUpdatePetBuff(petInfo);
		}
	}

	private void AddOrUpdatePetBuff(PetInfo petInfo)
	{
		if (petInfo == null)
		{
			return;
		}
		CreatureData creatureData = petInfo.CreatureData;
		if (creatureData != null)
		{
			ClearTempPetBuffList();
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(creatureData.PetOptionIndices))
			{
				GetPetAllBattleOption(creatureData.PetOptionIndices, petInfo.Star, _tempStatList, _tempContentsStatList);
				AddOrUpdatePetBattleOptionCachedData(petInfo.PetIndex, _tempStatList, _tempContentsStatList);
			}
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(creatureData.PetBonusIndices))
			{
				GetPetAllBonusOption(creatureData.PetBonusIndices, petInfo.Star, _tempBonusList);
				AddOrUpdatePetBonusCachedData(petInfo.PetIndex, _tempBonusList);
			}
		}
	}

	public DefaultExtraStatData[] GetPetStatDataBases(int petIndex)
	{
		DefaultExtraStatData[] value = null;
		_dicCachedPetStatData.TryGetValue(petIndex, out value);
		return value;
	}

	public List<DefaultExtraStatData> GetPetStatDataBaseList(bool sort = false)
	{
		if (__Extension_Method_ICollection__.COUNT(_dicCachedPetStatData) == 0)
		{
			return null;
		}
		List<DefaultExtraStatData> list = null;
		DefaultExtraStatData[] petStatDataBases = GetPetStatDataBases(GlobalAutoBehaviour<AccountManager>.instance.PetManager.MainPetAvatarIndex);
		if (petStatDataBases == null)
		{
			return null;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(petStatDataBases); i++)
		{
			DefaultExtraStatData statDataBase = petStatDataBases[i];
			if (statDataBase != null)
			{
				if (list == null)
				{
					list = new List<DefaultExtraStatData>();
				}
				int num = list.FindIndex((DefaultExtraStatData statData) => statData.OptionType == statDataBase.OptionType);
				if (num == -1)
				{
					DefaultExtraStatData item = new DefaultExtraStatData
					{
						OptionType = statDataBase.OptionType,
						OptionValue = statDataBase.OptionValue
					};
					list.Add(item);
				}
				else
				{
					list[num].OptionValue += statDataBase.OptionValue;
				}
			}
		}
		if (sort)
		{
			list.Sort(delegate(DefaultExtraStatData lhs, DefaultExtraStatData rhs)
			{
				PetCommonOptionOrderData data = GameRoot.Instance.PetCommonOptionOrderDataContainer.GetData(lhs.OptionType);
				PetCommonOptionOrderData data2 = GameRoot.Instance.PetCommonOptionOrderDataContainer.GetData(rhs.OptionType);
				int num2 = data?.Priority ?? 0;
				int value = data2?.Priority ?? 0;
				return num2.CompareTo(value);
			});
		}
		return list;
	}

	public PetStatDataBase[] GetPetContentsStatDataBases(int petIndex)
	{
		PetStatDataBase[] value = null;
		_dicCachedPetContentsStatData.TryGetValue(petIndex, out value);
		return value;
	}

	public List<PetStatDataBase> GetPetContentsStatDataBaseList(CampaignDungeonData targetDungeonData = null, bool sumEqualConditionOption = false, bool sort = false)
	{
		if (__Extension_Method_ICollection__.COUNT(_dicCachedPetContentsStatData) == 0)
		{
			return null;
		}
		List<PetStatDataBase> list = null;
		PetStatDataBase[] petContentsStatDataBases = GetPetContentsStatDataBases(GlobalAutoBehaviour<AccountManager>.instance.PetManager.MainPetAvatarIndex);
		if (petContentsStatDataBases == null)
		{
			return null;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(petContentsStatDataBases); i++)
		{
			PetStatDataBase statDataBase = petContentsStatDataBases[i];
			if (statDataBase == null || (targetDungeonData != null && !statDataBase.CheckActiveCondition(targetDungeonData)))
			{
				continue;
			}
			if (list == null)
			{
				list = new List<PetStatDataBase>();
			}
			PetOptionData newStatOptionData = (sumEqualConditionOption ? GameRoot.Instance.PetOptionDataContainer.GetData(statDataBase.Index) : null);
			int num = list.FindIndex(delegate(PetStatDataBase statData)
			{
				if (statData.Index == statDataBase.Index)
				{
					return true;
				}
				if (sumEqualConditionOption && statData.OptionType == statDataBase.OptionType)
				{
					PetOptionData data = GameRoot.Instance.PetOptionDataContainer.GetData(statData.Index);
					return IsEqualConditionOption(newStatOptionData, data);
				}
				return false;
			});
			if (num == -1)
			{
				PetStatDataBase item = new PetStatDataBase
				{
					Index = statDataBase.Index,
					OptionType = statDataBase.OptionType,
					OptionValue = statDataBase.OptionValue
				};
				list.Add(item);
			}
			else
			{
				list[num].OptionValue += statDataBase.OptionValue;
			}
		}
		if (sort)
		{
			list.Sort((PetStatDataBase lhs, PetStatDataBase rhs) => lhs.Index.CompareTo(rhs.Index));
		}
		return list;
	}

	public PetBonusDataBase[] GetPetBonusDataBases(int petIndex)
	{
		PetBonusDataBase[] value = null;
		_dicCachedPetBonusData.TryGetValue(petIndex, out value);
		return value;
	}

	public List<PetBonusDataBase> GetPetBonusDataBaseList(bool sort = false)
	{
		if (__Extension_Method_ICollection__.COUNT(_dicCachedPetBonusData) == 0)
		{
			return null;
		}
		List<PetBonusDataBase> list = null;
		foreach (KeyValuePair<int, PetBonusDataBase[]> dicCachedPetBonusDatum in _dicCachedPetBonusData)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(dicCachedPetBonusDatum.Value); i++)
			{
				PetBonusDataBase bonusDataBase = dicCachedPetBonusDatum.Value[i];
				if (bonusDataBase != null)
				{
					if (list == null)
					{
						list = new List<PetBonusDataBase>();
					}
					int num = list.FindIndex((PetBonusDataBase bonusData) => bonusData.BonusIndex == bonusDataBase.BonusIndex);
					if (num == -1)
					{
						PetBonusDataBase item = new PetBonusDataBase
						{
							BonusIndex = bonusDataBase.BonusIndex,
							BonusValue = bonusDataBase.BonusValue
						};
						list.Add(item);
					}
					else
					{
						list[num].BonusValue += bonusDataBase.BonusValue;
					}
				}
			}
		}
		if (sort)
		{
			list.Sort((PetBonusDataBase lhs, PetBonusDataBase rhs) => lhs.BonusIndex.CompareTo(rhs.BonusIndex));
		}
		return list;
	}

	public int GetPetBonusValue(EventBonusType eventBonusType)
	{
		int num = 0;
		foreach (KeyValuePair<int, PetBonusDataBase[]> dicCachedPetBonusDatum in _dicCachedPetBonusData)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(dicCachedPetBonusDatum.Value); i++)
			{
				PetBonusDataBase petBonusDataBase = dicCachedPetBonusDatum.Value[i];
				if (petBonusDataBase != null)
				{
					PetBonusData data = GameRoot.Instance.PetBonusDataContainer.GetData(petBonusDataBase.BonusIndex);
					if (data != null && data.BonusType == eventBonusType)
					{
						num += petBonusDataBase.BonusValue;
					}
				}
			}
		}
		return num;
	}

	public string GetPetBattleOptionBuffDesc()
	{
		if (!SingletonAuto<BattleContext>.isValidInstance || SingletonAuto<BattleContext>.instance.DataProvider == null)
		{
			return string.Empty;
		}
		EnterDungeonData enterDungeon = SingletonAuto<BattleContext>.instance.DataProvider.GetEnterDungeon();
		if (enterDungeon == null)
		{
			return string.Empty;
		}
		CampaignDungeonData linkedDungeonData = enterDungeon.LinkedDungeonData;
		if (linkedDungeonData == null)
		{
			return string.Empty;
		}
		return GetPetContentsStatDesc(linkedDungeonData);
	}

	private string GetPetContentsStatDesc(CampaignDungeonData targetDungeonData = null)
	{
		string stringDirect = LocalizationManager.GetStringDirect("PET_BUFF_SKILL_DESC_APPLY");
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(stringDirect);
		List<PetStatDataBase> petContentsStatDataBaseList = GetPetContentsStatDataBaseList(targetDungeonData, sumEqualConditionOption: true);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(petContentsStatDataBaseList); i++)
		{
			PetStatDataBase petStatDataBase = petContentsStatDataBaseList[i];
			if (petStatDataBase != null)
			{
				PetOptionData data = GameRoot.Instance.PetOptionDataContainer.GetData(petStatDataBase.Index);
				if (data != null)
				{
					string text = LocalizationManager.GetString(petStatDataBase.OptionType);
					string stringDirect2 = LocalizationManager.GetStringDirect(data.ContentsOptionDescKey, text);
					string statDisplayValue = StatData.GetStatDisplayValue(petStatDataBase.OptionType, petStatDataBase.OptionValue, showSign: true, isCommaString: true);
					stringBuilder.AppendFormat("{0}{1} {2}", Environment.NewLine, stringDirect2, statDisplayValue);
				}
			}
		}
		return stringBuilder.ToString();
	}

	public List<PetBuffDesc> GetPetBuffDesc(int petIndex)
	{
		PetInfo petInfo = GlobalAutoBehaviour<AccountManager>.instance.PetManager.Get(petIndex);
		if (petInfo == null)
		{
			return null;
		}
		CreatureData creatureData = petInfo.CreatureData;
		if (creatureData == null)
		{
			return null;
		}
		List<PetBuffDesc> list = new List<PetBuffDesc>();
		string optionName = string.Empty;
		string optionValue = string.Empty;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(creatureData.PetOptionIndices); i++)
		{
			if (PetHelper.TryGetPetOptionLabelText(creatureData.PetOptionIndices[i], petInfo.Star, ref optionName, ref optionValue))
			{
				PetBuffDesc item = new PetBuffDesc
				{
					OptionName = optionName,
					OptionValue = optionValue
				};
				list.Add(item);
			}
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(creatureData.PetBonusIndices); j++)
		{
			if (PetHelper.TryGetPetBonusOptionLabelText(creatureData.PetBonusIndices[j], petInfo.Star, ref optionName, ref optionValue))
			{
				PetBuffDesc item2 = new PetBuffDesc
				{
					OptionName = optionName,
					OptionValue = optionValue
				};
				list.Add(item2);
			}
		}
		return list;
	}

	private void GetPetAllBattleOption(int[] petOptionIndices, int petStar, List<DefaultExtraStatData> statDataList, List<PetStatDataBase> contentsStatDataList)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(petOptionIndices); i++)
		{
			int num = petOptionIndices[i];
			if (num == 0)
			{
				continue;
			}
			PetOptionData data = GameRoot.Instance.PetOptionDataContainer.GetData(num);
			if (data != null)
			{
				if (data.ConditionType == PetOptionConditionType.None)
				{
					DefaultExtraStatData item = new DefaultExtraStatData
					{
						OptionType = data.OptionType,
						OptionValue = data.GetOptionValue(petStar)
					};
					statDataList.Add(item);
				}
				else
				{
					PetStatDataBase item2 = new PetStatDataBase
					{
						Index = num,
						OptionType = data.OptionType,
						OptionValue = data.GetOptionValue(petStar)
					};
					contentsStatDataList.Add(item2);
				}
			}
		}
	}

	private void AddOrUpdatePetBattleOptionCachedData(int petIndex, List<DefaultExtraStatData> statDataList, List<PetStatDataBase> contentsStatDataList)
	{
		if (__Extension_Method_ICollection__.COUNT(statDataList) > 0)
		{
			if (_dicCachedPetStatData.ContainsKey(petIndex))
			{
				_dicCachedPetStatData.Remove(petIndex);
			}
			_dicCachedPetStatData.Add(petIndex, statDataList.ToArray());
		}
		if (__Extension_Method_ICollection__.COUNT(contentsStatDataList) > 0)
		{
			if (_dicCachedPetContentsStatData.ContainsKey(petIndex))
			{
				_dicCachedPetContentsStatData.Remove(petIndex);
			}
			_dicCachedPetContentsStatData.Add(petIndex, contentsStatDataList.ToArray());
		}
	}

	private bool IsEqualConditionOption(PetOptionData l, PetOptionData r)
	{
		if (l == null || r == null)
		{
			return false;
		}
		if (l.ConditionType != r.ConditionType)
		{
			return false;
		}
		if (__Extension_Method_ICollection__.COUNT(l.ConditionValue) != __Extension_Method_ICollection__.COUNT(r.ConditionValue))
		{
			return false;
		}
		if (!l.ConditionValue.SequenceEqual(r.ConditionValue))
		{
			return false;
		}
		return true;
	}

	private void GetPetAllBonusOption(int[] petBonusIndices, int star, List<PetBonusDataBase> petBonusDataBases)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(petBonusIndices); i++)
		{
			int num = petBonusIndices[i];
			PetBonusData data = GameRoot.Instance.PetBonusDataContainer.GetData(num);
			if (data != null)
			{
				PetBonusDataBase item = new PetBonusDataBase
				{
					BonusIndex = num,
					BonusValue = data.GetBonusValue(star)
				};
				petBonusDataBases.Add(item);
			}
		}
	}

	private void AddOrUpdatePetBonusCachedData(int petIndex, List<PetBonusDataBase> petBonusDataBases)
	{
		if (__Extension_Method_ICollection__.COUNT(petBonusDataBases) != 0)
		{
			if (_dicCachedPetBonusData.ContainsKey(petIndex))
			{
				_dicCachedPetBonusData.Remove(petIndex);
			}
			_dicCachedPetBonusData.Add(petIndex, petBonusDataBases.ToArray());
		}
	}

	void IUserObserver<NGame2.NAccount.UserPetManager.ChangedPetInfo>.OnChanged(NGame2.NAccount.UserPetManager.ChangedPetInfo changedPetInfo)
	{
		if (changedPetInfo == null || (changedPetInfo.EChangedPetInfoType != NGame2.NAccount.UserPetManager.EChangedPetInfoType.PetInfoChanged && changedPetInfo.EChangedPetInfoType != NGame2.NAccount.UserPetManager.EChangedPetInfoType.AddedPet))
		{
			return;
		}
		ApplyChangedPetInfo(changedPetInfo.PetIndex);
		GlobalAutoBehaviour<BoosterManager>.instance.InitAllEffectiveBooster(BattleType.None, includeCash: true);
		GlobalAutoBehaviour<BoosterManager>.instance.AnnounceChangedBoosterInfoList();
		DefaultExtraStatData[] petStatDataBases = GetPetStatDataBases(changedPetInfo.PetIndex);
		if (__Extension_Method_ICollection__.COUNT(petStatDataBases) > 0)
		{
			Array.ForEach(petStatDataBases, delegate(DefaultExtraStatData statData)
			{
				_changedPetStatTypes.Add((int)statData.OptionType);
			});
			SaveChangedBuffInfo(_changedPetStatTypes, _changedPetStatLocalInfos);
		}
		PetStatDataBase[] petContentsStatDataBases = GetPetContentsStatDataBases(changedPetInfo.PetIndex);
		if (__Extension_Method_ICollection__.COUNT(petContentsStatDataBases) > 0)
		{
			Array.ForEach(petContentsStatDataBases, delegate(PetStatDataBase contentsStatData)
			{
				_changedPetContentsStatIndices.Add(contentsStatData.Index);
			});
			SaveChangedBuffInfo(_changedPetContentsStatIndices, _changedPetContentsStatLocalInfos);
		}
		PetBonusDataBase[] petBonusDataBases = GetPetBonusDataBases(changedPetInfo.PetIndex);
		if (__Extension_Method_ICollection__.COUNT(petBonusDataBases) > 0)
		{
			Array.ForEach(petBonusDataBases, delegate(PetBonusDataBase bonusData)
			{
				_changedPetBonusIndices.Add(bonusData.BonusIndex);
			});
			SaveChangedBuffInfo(_changedPetBonusIndices, _changedPetBonusLocalInfos);
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return true;
	}

	public void ClearChangedBuffInfo(PetBuffType buffType = PetBuffType.None)
	{
		switch (buffType)
		{
		case PetBuffType.CommonBuff:
			_changedPetStatTypes.Clear();
			_changedPetStatLocalInfos.Value = string.Empty;
			return;
		case PetBuffType.ContentsBuff:
			_changedPetContentsStatIndices.Clear();
			_changedPetContentsStatLocalInfos.Value = string.Empty;
			return;
		case PetBuffType.Bonus:
			_changedPetBonusIndices.Clear();
			_changedPetBonusLocalInfos.Value = string.Empty;
			return;
		}
		_changedPetStatTypes.Clear();
		_changedPetContentsStatIndices.Clear();
		_changedPetBonusIndices.Clear();
		_changedPetStatLocalInfos.Value = string.Empty;
		_changedPetContentsStatLocalInfos.Value = string.Empty;
		_changedPetBonusLocalInfos.Value = string.Empty;
	}

	public bool IsChangedPetStat(OperationType operationType)
	{
		return _changedPetStatTypes.Contains((int)operationType);
	}

	public bool IsChangedPetContentsStat(int targetOptionIndex)
	{
		bool flag = _changedPetContentsStatIndices.Contains(targetOptionIndex);
		if (flag)
		{
			return true;
		}
		PetOptionData data = GameRoot.Instance.PetOptionDataContainer.GetData(targetOptionIndex);
		foreach (int changedPetContentsStatIndex in _changedPetContentsStatIndices)
		{
			PetOptionData data2 = GameRoot.Instance.PetOptionDataContainer.GetData(changedPetContentsStatIndex);
			if (data.OptionType == data2.OptionType && IsEqualConditionOption(data, data2))
			{
				flag = true;
				break;
			}
		}
		return flag;
	}

	public bool IsChangedPetBonus(int bonusIndex)
	{
		return _changedPetBonusIndices.Contains(bonusIndex);
	}

	public bool IsChangedBuffType(PetBuffType buffType = PetBuffType.None)
	{
		bool flag = false;
		return buffType switch
		{
			PetBuffType.CommonBuff => __Extension_Method_ICollection__.COUNT(_changedPetStatTypes) > 0, 
			PetBuffType.ContentsBuff => __Extension_Method_ICollection__.COUNT(_changedPetContentsStatIndices) > 0, 
			PetBuffType.Bonus => __Extension_Method_ICollection__.COUNT(_changedPetBonusIndices) > 0, 
			_ => __Extension_Method_ICollection__.COUNT(_changedPetStatTypes) > 0 || __Extension_Method_ICollection__.COUNT(_changedPetContentsStatIndices) > 0 || __Extension_Method_ICollection__.COUNT(_changedPetBonusIndices) > 0, 
		};
	}

	public bool IsValidPetManager()
	{
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance || GlobalAutoBehaviour<AccountManager>.instance.PetManager == null)
		{
			return false;
		}
		return true;
	}

	protected override void OnDestroy()
	{
		if (!SystemStatus.IsApplicationQuit)
		{
			if (GlobalAutoBehaviour<AccountManager>.isValidInstance && GlobalAutoBehaviour<AccountManager>.instance.PetManager != null)
			{
				GlobalAutoBehaviour<AccountManager>.instance.PetManager.ObserverManager.RemoveObserver(this);
			}
			base.OnDestroy();
		}
	}

	private void ClearCachedDic()
	{
		_dicCachedPetBonusData.Clear();
		_dicCachedPetStatData.Clear();
		_dicCachedPetContentsStatData.Clear();
	}

	private void ClearTempPetBuffList()
	{
		_tempBonusList.Clear();
		_tempStatList.Clear();
		_tempContentsStatList.Clear();
	}

	private void InitChangedPetBuffInfo()
	{
		_changedPetStatTypes.Clear();
		_changedPetContentsStatIndices.Clear();
		_changedPetBonusIndices.Clear();
		SetChangedBuffInfos(_changedPetStatLocalInfos.Value, _changedPetStatTypes);
		SetChangedBuffInfos(_changedPetContentsStatLocalInfos.Value, _changedPetContentsStatIndices);
		SetChangedBuffInfos(_changedPetBonusLocalInfos.Value, _changedPetBonusIndices);
	}

	private void SetChangedBuffInfos(string localSaveValue, HashSet<int> buffInfos)
	{
		if (String_Extension_Method.IsNullOrEmpty(localSaveValue))
		{
			return;
		}
		string[] array = localSaveValue.Split(',');
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(array); i++)
		{
			string text = array[i];
			if (!String_Extension_Method.IsNullOrEmpty(text))
			{
				int num = Vespa_String_Extension_Method.TOINT(text, 0);
				if (num != 0)
				{
					buffInfos.Add(num);
				}
			}
		}
	}

	private void SaveChangedBuffInfo(HashSet<int> buffInfos, LocalAccountString localSave)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (int buffInfo in buffInfos)
		{
			if (stringBuilder.Length == 0)
			{
				stringBuilder.AppendFormat("{0}", buffInfo);
			}
			else
			{
				stringBuilder.AppendFormat(",{0}", buffInfo);
			}
		}
		localSave.Value = stringBuilder.ToString();
	}
}
