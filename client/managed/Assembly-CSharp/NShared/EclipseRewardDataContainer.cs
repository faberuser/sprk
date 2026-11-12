using System;
using System.Collections.Generic;
using System.Linq;
using NShared.NJit;

namespace NShared;

public class EclipseRewardDataContainer : JitDataContainerDefault<int, EclipseRewardData>
{
	public const string TABLENAME = "EclipseRewardTable";

	public override TablePathDefine PathDefine => new TablePathDefine("EclipseRewardTable");

	private int LastWaveIndex => GetAllData().LastOrDefault()?.Wave ?? (-1);

	public List<int> GetBaseRewardList(int currentWave, int repeatFactor = 5)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(GetAllData()))
		{
			return null;
		}
		List<int> list = new List<int>();
		list.AddRange(from v in GetAllData()
			where v.Wave < currentWave
			select v.NormalReward);
		repeatFactor = Math.Min(repeatFactor, GetAllData().Count);
		int wave = GetAllData().Last().Wave;
		if (currentWave <= wave)
		{
			return list;
		}
		List<EclipseRewardData> list2 = new List<EclipseRewardData>();
		int count = GetAllData().Count - repeatFactor;
		list2.AddRange(GetAllData().Skip(count));
		int num = (currentWave - wave) / repeatFactor;
		int num2 = (currentWave - wave) % repeatFactor;
		for (int num3 = 0; num3 < num; num3++)
		{
			foreach (EclipseRewardData item in list2)
			{
				list.Add(item.NormalReward);
			}
		}
		int num4 = 0;
		foreach (EclipseRewardData item2 in list2)
		{
			if (num4 == num2)
			{
				break;
			}
			list.Add(item2.NormalReward);
			num4++;
		}
		return list;
	}

	public List<RewardElementData> GetWaveRangeRewardList(int endWaveIndex, int startWaveIndex = 1, bool isBestTeam = false)
	{
		Dictionary<string, RewardElementData> itemRewardDic = new Dictionary<string, RewardElementData>();
		itemRewardDic.Clear();
		Dictionary<CurrencyType, RewardElementData> currencyRewardDic = new Dictionary<CurrencyType, RewardElementData>();
		currencyRewardDic.Clear();
		AddRewardDic(startWaveIndex, endWaveIndex, ref itemRewardDic, ref currencyRewardDic, isBestTeam);
		if (CheckOverWave(endWaveIndex))
		{
			int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("EclipseRewardRepeatFactor", 10);
			int lastWaveIndex = LastWaveIndex;
			int num = lastWaveIndex - valueInt + 1;
			for (int num2 = endWaveIndex - lastWaveIndex; num2 > 0; num2 -= valueInt)
			{
				int num3 = ((num2 < valueInt) ? num2 : valueInt);
				int endWaveIndex2 = num + num3 - 1;
				AddRewardDic(num, endWaveIndex2, ref itemRewardDic, ref currencyRewardDic, isBestTeam);
			}
		}
		if (itemRewardDic.Count <= 0 && currencyRewardDic.Count <= 0)
		{
			return null;
		}
		List<RewardElementData> list = new List<RewardElementData>();
		list.Clear();
		AddRewardElementList(ref list, currencyRewardDic);
		AddRewardElementList(ref list, itemRewardDic);
		return list;
	}

	private bool CheckOverWave(int endWaveIndex)
	{
		int lastWaveIndex = LastWaveIndex;
		if (lastWaveIndex <= -1)
		{
			return false;
		}
		return endWaveIndex > lastWaveIndex;
	}

	private void AddRewardDic(int startWaveIndex, int endWaveIndex, ref Dictionary<string, RewardElementData> itemRewardDic, ref Dictionary<CurrencyType, RewardElementData> currencyRewardDic, bool isBestTeam = false)
	{
		List<RewardElementData> list = new List<RewardElementData>();
		for (int i = startWaveIndex; i <= endWaveIndex; i++)
		{
			EclipseRewardData data = GameRoot.Instance.EclipseRewardDataContainer.GetData(i);
			if (data == null)
			{
				continue;
			}
			int key = (isBestTeam ? data.BestTeamReward : data.NormalReward);
			RewardData data2 = GameRoot.Instance.RewardDataContainer.GetData(key);
			if (data2 == null)
			{
				continue;
			}
			list.Clear();
			list = data2.GetRewardItemList();
			for (int j = 0; j < list.Count; j++)
			{
				RewardElementData rewardElementData = list[j];
				if (rewardElementData != null)
				{
					AddItemRewardElementData(ref itemRewardDic, rewardElementData);
					AddCurrencyRewardElementData(ref currencyRewardDic, rewardElementData);
				}
			}
		}
	}

	private void AddRewardElementList<T>(ref List<RewardElementData> list, Dictionary<T, RewardElementData> dic)
	{
		foreach (KeyValuePair<T, RewardElementData> item in dic)
		{
			if (item.Value != null)
			{
				list.Add(item.Value);
			}
		}
	}

	private void AddItemRewardElementData(ref Dictionary<string, RewardElementData> dic, RewardElementData data)
	{
		if (data != null && !data.ItemCode.IsNullOrEmpty())
		{
			HashString itemCode = data.ItemCode;
			if (dic.ContainsKey(itemCode))
			{
				dic[itemCode].ItemMin += data.ItemMin;
				dic[itemCode].ItemMax += data.ItemMax;
			}
			else
			{
				dic.Add(itemCode, data);
			}
		}
	}

	private void AddCurrencyRewardElementData(ref Dictionary<CurrencyType, RewardElementData> dic, RewardElementData data)
	{
		if (data != null && data.GetCurrencyType() != CurrencyType.None)
		{
			CurrencyType currencyType = data.GetCurrencyType();
			if (dic.ContainsKey(currencyType))
			{
				dic[currencyType].ItemMin += data.ItemMin;
				dic[currencyType].ItemMax += data.ItemMax;
			}
			else
			{
				dic.Add(currencyType, data);
			}
		}
	}

	public List<RewardElementData> GetBestTeamRewardElementDataList(int waveIndex)
	{
		EclipseRewardData data = GameRoot.Instance.EclipseRewardDataContainer.GetData(waveIndex);
		if (data == null)
		{
			return null;
		}
		return GameRoot.Instance.RewardDataContainer.GetData(data.BestTeamReward)?.GetElementList();
	}
}
