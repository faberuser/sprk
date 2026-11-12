using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGame2.NAccount;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2;

public class TeamLevelBuffManager : GlobalAutoBehaviour<TeamLevelBuffManager>, IUserObserver<TeamLevelBuff>, IUserObserver
{
	public enum EBuffType
	{
		None,
		CommonBuff,
		ContentsBuff,
		Bonus
	}

	private List<int> _openAbleBuffTeamLevelList = new List<int>();

	private Dictionary<int, TeamLevelBuffOptionData> _teamLevelBuffOptionDic = new Dictionary<int, TeamLevelBuffOptionData>();

	private Dictionary<int, TeamLevelBonusBuffOptionData> _teamLevelBonusBuffOptionDic = new Dictionary<int, TeamLevelBonusBuffOptionData>();

	private SortedDictionary<int, DefaultExtraStatData> _cachedTeamLevelBuffStatDataDic = new SortedDictionary<int, DefaultExtraStatData>();

	private SortedDictionary<int, PetStatDataBase> _cachedTeamLevelBuffContentsStatDataDic = new SortedDictionary<int, PetStatDataBase>();

	private SortedDictionary<int, PetBonusDataBase> _cachedTeamLevelBonusDataDic = new SortedDictionary<int, PetBonusDataBase>();

	private HashSet<int> _changedTeamLevelStatTypes = new HashSet<int>();

	private HashSet<int> _changedTeamLevelContentsStatIndices = new HashSet<int>();

	private HashSet<int> _changedTeamLevelBonusIndices = new HashSet<int>();

	private LocalAccountString _changedTeamLevelStatLocalInfos = new LocalAccountString("", "ChangedTeamLevelStatLocalInfos");

	private LocalAccountString _changedTeamLevelContentsStatLocalInfos = new LocalAccountString("", "ChangedTeamLevelContentsStatLocalInfos");

	private LocalAccountString _changedTeamLevelBonusLocalInfos = new LocalAccountString("", "ChangedTeamLevelBonusLocalInfos");

	public void Init()
	{
		_cachedTeamLevelBuffStatDataDic.Clear();
		_cachedTeamLevelBuffContentsStatDataDic.Clear();
		_cachedTeamLevelBonusDataDic.Clear();
		if (GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			GlobalAutoBehaviour<AccountManager>.instance.TeamLevelBuffManager.ObserverManager.AddObserver(this);
		}
		_openAbleBuffTeamLevelList.Clear();
		foreach (TeamLevelData allDatum in GameRoot.Instance.TeamLevelDataContainer.GetAllData())
		{
			int level = allDatum.Level;
			if (allDatum.TeamLevelBuffOptionIndex != 0)
			{
				TeamLevelBuffOptionData data = GameRoot.Instance.TeamLevelBuffOptionDataContainer.GetData(allDatum.TeamLevelBuffOptionIndex);
				_teamLevelBuffOptionDic[level] = data;
				_openAbleBuffTeamLevelList.Add(level);
			}
			else if (allDatum.TeamLevelBuffBonusOptionIndex != 0)
			{
				TeamLevelBonusBuffOptionData data2 = GameRoot.Instance.TeamLevelBonusBuffOptionDataContainer.GetData(allDatum.TeamLevelBuffBonusOptionIndex);
				_teamLevelBonusBuffOptionDic[level] = data2;
				_openAbleBuffTeamLevelList.Add(level);
			}
		}
		_openAbleBuffTeamLevelList.Sort();
		TeamLevelBuff[] values = GlobalAutoBehaviour<AccountManager>.instance.TeamLevelBuffManager.GetValues();
		foreach (TeamLevelBuff buffInfo in values)
		{
			AddOrUpdateTeamLevelBuff(buffInfo);
			AddOrUpdateTeamLevelBonusBuff(buffInfo);
		}
		InitChangedTeamLevelBuffInfo();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			GlobalAutoBehaviour<AccountManager>.instance.TeamLevelBuffManager.ObserverManager.RemoveObserver(this);
		}
	}

	private void AddOrUpdateTeamLevelBuff(TeamLevelBuff buffInfo)
	{
		if (buffInfo != null)
		{
			DefaultExtraStatData statData = null;
			PetStatDataBase contentsStatData = null;
			GetTeamLevelBattleOption(buffInfo, out statData, out contentsStatData);
			if (statData != null)
			{
				_cachedTeamLevelBuffStatDataDic[buffInfo.OpenBuffLevel] = statData;
			}
			if (contentsStatData != null)
			{
				_cachedTeamLevelBuffContentsStatDataDic[buffInfo.OpenBuffLevel] = contentsStatData;
			}
		}
	}

	private void AddOrUpdateTeamLevelBonusBuff(TeamLevelBuff buffInfo)
	{
		if (buffInfo != null)
		{
			PetBonusDataBase bonusData = null;
			GetTeamLevelBonusOption(buffInfo, out bonusData);
			if (bonusData != null)
			{
				_cachedTeamLevelBonusDataDic[buffInfo.OpenBuffLevel] = bonusData;
			}
		}
	}

	private void GetTeamLevelBattleOption(TeamLevelBuff buffInfo, out DefaultExtraStatData statData, out PetStatDataBase contentsStatData)
	{
		statData = null;
		contentsStatData = null;
		if (buffInfo == null)
		{
			return;
		}
		int openBuffLevel = buffInfo.OpenBuffLevel;
		int buffReinforce = buffInfo.BuffReinforce;
		TeamLevelBuffOptionData buffOptionData = GetBuffOptionData(openBuffLevel);
		if (buffOptionData != null)
		{
			if (buffOptionData.ConditionType == TeamLevelBuffOptionConditionType.None)
			{
				statData = new DefaultExtraStatData
				{
					OptionType = buffOptionData.OptionType,
					OptionValue = buffOptionData.GetOptionValue(buffReinforce)
				};
			}
			else
			{
				contentsStatData = new PetStatDataBase
				{
					Index = buffOptionData.Index,
					OptionType = buffOptionData.OptionType,
					OptionValue = buffOptionData.GetOptionValue(buffReinforce)
				};
			}
		}
	}

	private void GetTeamLevelBonusOption(TeamLevelBuff buffInfo, out PetBonusDataBase bonusData)
	{
		bonusData = null;
		if (buffInfo != null)
		{
			TeamLevelBonusBuffOptionData bonusBuffOptionData = GetBonusBuffOptionData(buffInfo.OpenBuffLevel);
			if (bonusBuffOptionData != null)
			{
				bonusData = new PetBonusDataBase();
				bonusData.BonusIndex = bonusBuffOptionData.Index;
				bonusData.BonusValue = bonusBuffOptionData.GetBonusValue(buffInfo.BuffReinforce);
			}
		}
	}

	public List<DefaultExtraStatData> GetTeamLevelBuffStatDataBaseList(bool sort = false)
	{
		if (__Extension_Method_ICollection__.COUNT(_cachedTeamLevelBuffStatDataDic) == 0)
		{
			return null;
		}
		List<DefaultExtraStatData> list = null;
		foreach (KeyValuePair<int, DefaultExtraStatData> item2 in _cachedTeamLevelBuffStatDataDic)
		{
			DefaultExtraStatData statDataBase = item2.Value;
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
				TeamLevelCommonOptionOrderData data = GameRoot.Instance.TeamLevelCommonOptionOrderDataContainer.GetData(lhs.OptionType);
				TeamLevelCommonOptionOrderData data2 = GameRoot.Instance.TeamLevelCommonOptionOrderDataContainer.GetData(rhs.OptionType);
				int num2 = data?.Priority ?? 0;
				int value = data2?.Priority ?? 0;
				return num2.CompareTo(value);
			});
		}
		return list;
	}

	public List<PetStatDataBase> GetTeamLevelBuffContentsStatDataBaseList(CampaignDungeonData targetDungeonData = null, bool sumEqualConditionOption = false, bool sort = false)
	{
		if (__Extension_Method_ICollection__.COUNT(_cachedTeamLevelBuffContentsStatDataDic) == 0)
		{
			return null;
		}
		List<PetStatDataBase> list = null;
		foreach (KeyValuePair<int, PetStatDataBase> item2 in _cachedTeamLevelBuffContentsStatDataDic)
		{
			PetStatDataBase statDataBase = item2.Value;
			if (statDataBase == null || (targetDungeonData != null && !statDataBase.CheckActiveCondition(targetDungeonData)))
			{
				continue;
			}
			if (list == null)
			{
				list = new List<PetStatDataBase>();
			}
			TeamLevelBuffOptionData newStatOptionData = (sumEqualConditionOption ? GameRoot.Instance.TeamLevelBuffOptionDataContainer.GetData(statDataBase.Index) : null);
			int num = list.FindIndex(delegate(PetStatDataBase statData)
			{
				if (statData.Index == statDataBase.Index)
				{
					return true;
				}
				if (sumEqualConditionOption && statData.OptionType == statDataBase.OptionType)
				{
					TeamLevelBuffOptionData data = GameRoot.Instance.TeamLevelBuffOptionDataContainer.GetData(statData.Index);
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

	public List<PetBonusDataBase> GetTeamLevelBonusDataBaseList(bool sort = false)
	{
		if (__Extension_Method_ICollection__.COUNT(_cachedTeamLevelBonusDataDic) == 0)
		{
			return null;
		}
		List<PetBonusDataBase> list = null;
		foreach (KeyValuePair<int, PetBonusDataBase> item2 in _cachedTeamLevelBonusDataDic)
		{
			PetBonusDataBase bonusDataBase = item2.Value;
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
		if (sort)
		{
			list.Sort((PetBonusDataBase lhs, PetBonusDataBase rhs) => lhs.BonusIndex.CompareTo(rhs.BonusIndex));
		}
		return list;
	}

	public int GetTeamLevelBuffBonusValue(EventBonusType eventBonusType)
	{
		int num = 0;
		foreach (KeyValuePair<int, PetBonusDataBase> item in _cachedTeamLevelBonusDataDic)
		{
			PetBonusDataBase value = item.Value;
			if (value != null)
			{
				TeamLevelBonusBuffOptionData data = GameRoot.Instance.TeamLevelBonusBuffOptionDataContainer.GetData(value.BonusIndex);
				if (data != null && data.BonusType == eventBonusType)
				{
					num += value.BonusValue;
				}
			}
		}
		return num;
	}

	private bool IsEqualConditionOption(TeamLevelBuffOptionData l, TeamLevelBuffOptionData r)
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

	public List<int> GetOpenAbleBuffTeamLevelList()
	{
		return _openAbleBuffTeamLevelList;
	}

	public TeamLevelBuffOptionData GetBuffOptionData(int level)
	{
		if (_teamLevelBuffOptionDic.ContainsKey(level))
		{
			return _teamLevelBuffOptionDic[level];
		}
		return null;
	}

	public TeamLevelBonusBuffOptionData GetBonusBuffOptionData(int level)
	{
		if (_teamLevelBonusBuffOptionDic.ContainsKey(level))
		{
			return _teamLevelBonusBuffOptionDic[level];
		}
		return null;
	}

	public void ClearChangedBuffInfo(EBuffType buffType = EBuffType.None)
	{
		switch (buffType)
		{
		case EBuffType.CommonBuff:
			_changedTeamLevelStatTypes.Clear();
			_changedTeamLevelStatLocalInfos.Value = string.Empty;
			return;
		case EBuffType.ContentsBuff:
			_changedTeamLevelContentsStatIndices.Clear();
			_changedTeamLevelContentsStatLocalInfos.Value = string.Empty;
			return;
		case EBuffType.Bonus:
			_changedTeamLevelBonusIndices.Clear();
			_changedTeamLevelBonusLocalInfos.Value = string.Empty;
			return;
		}
		_changedTeamLevelStatTypes.Clear();
		_changedTeamLevelContentsStatIndices.Clear();
		_changedTeamLevelBonusIndices.Clear();
		_changedTeamLevelStatLocalInfos.Value = string.Empty;
		_changedTeamLevelContentsStatLocalInfos.Value = string.Empty;
		_changedTeamLevelBonusLocalInfos.Value = string.Empty;
	}

	private void InitChangedTeamLevelBuffInfo()
	{
		_changedTeamLevelStatTypes.Clear();
		_changedTeamLevelContentsStatIndices.Clear();
		_changedTeamLevelBonusIndices.Clear();
		LoadChangedBuffInfos(_changedTeamLevelStatLocalInfos.Value, _changedTeamLevelStatTypes);
		LoadChangedBuffInfos(_changedTeamLevelContentsStatLocalInfos.Value, _changedTeamLevelContentsStatIndices);
		LoadChangedBuffInfos(_changedTeamLevelBonusLocalInfos.Value, _changedTeamLevelBonusIndices);
	}

	private void LoadChangedBuffInfos(string localSaveValue, HashSet<int> buffInfos)
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

	public bool IsChangedTeamLevelStat(OperationType operationType)
	{
		return _changedTeamLevelStatTypes.Contains((int)operationType);
	}

	public bool IsChangedTeamLevelContentsStat(int targetOptionIndex)
	{
		bool flag = _changedTeamLevelContentsStatIndices.Contains(targetOptionIndex);
		if (flag)
		{
			return true;
		}
		TeamLevelBuffOptionData data = GameRoot.Instance.TeamLevelBuffOptionDataContainer.GetData(targetOptionIndex);
		foreach (int changedTeamLevelContentsStatIndex in _changedTeamLevelContentsStatIndices)
		{
			TeamLevelBuffOptionData data2 = GameRoot.Instance.TeamLevelBuffOptionDataContainer.GetData(changedTeamLevelContentsStatIndex);
			if (data.OptionType == data2.OptionType && IsEqualConditionOption(data, data2))
			{
				flag = true;
				break;
			}
		}
		return flag;
	}

	public bool IsChangedTeamLevelBonus(int bonusIndex)
	{
		return _changedTeamLevelBonusIndices.Contains(bonusIndex);
	}

	public bool IsChangedBuffType(EBuffType buffType = EBuffType.None)
	{
		bool flag = false;
		return buffType switch
		{
			EBuffType.CommonBuff => __Extension_Method_ICollection__.COUNT(_changedTeamLevelStatTypes) > 0, 
			EBuffType.ContentsBuff => __Extension_Method_ICollection__.COUNT(_changedTeamLevelContentsStatIndices) > 0, 
			EBuffType.Bonus => __Extension_Method_ICollection__.COUNT(_changedTeamLevelBonusIndices) > 0, 
			_ => __Extension_Method_ICollection__.COUNT(_changedTeamLevelStatTypes) > 0 || __Extension_Method_ICollection__.COUNT(_changedTeamLevelContentsStatIndices) > 0 || __Extension_Method_ICollection__.COUNT(_changedTeamLevelBonusIndices) > 0, 
		};
	}

	void IUserObserver<TeamLevelBuff>.OnChanged(TeamLevelBuff changedInfo)
	{
		if (changedInfo == null)
		{
			return;
		}
		AddOrUpdateTeamLevelBuff(changedInfo);
		AddOrUpdateTeamLevelBonusBuff(changedInfo);
		TeamLevelBuffOptionData buffOptionData = GetBuffOptionData(changedInfo.OpenBuffLevel);
		TeamLevelBonusBuffOptionData bonusBuffOptionData = GetBonusBuffOptionData(changedInfo.OpenBuffLevel);
		if (buffOptionData != null)
		{
			GetTeamLevelBattleOption(changedInfo, out var statData, out var contentsStatData);
			if (statData != null)
			{
				_changedTeamLevelStatTypes.Add((int)statData.OptionType);
				SaveChangedBuffInfo(_changedTeamLevelStatTypes, _changedTeamLevelStatLocalInfos);
			}
			else if (contentsStatData != null)
			{
				_changedTeamLevelContentsStatIndices.Add(contentsStatData.Index);
				SaveChangedBuffInfo(_changedTeamLevelContentsStatIndices, _changedTeamLevelContentsStatLocalInfos);
			}
		}
		else if (bonusBuffOptionData != null)
		{
			GetTeamLevelBonusOption(changedInfo, out var bonusData);
			if (bonusData != null)
			{
				_changedTeamLevelBonusIndices.Add(bonusData.BonusIndex);
				SaveChangedBuffInfo(_changedTeamLevelBonusIndices, _changedTeamLevelBonusLocalInfos);
			}
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return true;
	}
}
