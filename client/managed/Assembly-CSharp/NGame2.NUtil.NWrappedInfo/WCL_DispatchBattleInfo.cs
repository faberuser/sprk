using System;
using System.Collections.Generic;
using NGame2.NUI;
using NGame2.NUtil.NHelper.NInfoHelper;
using NShared;
using UnityEngine;

namespace NGame2.NUtil.NWrappedInfo;

public class WCL_DispatchBattleInfo : DispatchBattleInfo
{
	private List<int> _heroIndexList = new List<int>();

	private DateTime _convBeginTime;

	private DateTime _convCompleteTime;

	private List<int> _clientResultList = new List<int>();

	private List<int> _serverResultList = new List<int>();

	private int _lastWinCount;

	private int _lastLoseCount;

	private CurrencyResultInfo3[] _currencyResults;

	private ItemResultInfo[] _itemResults;

	private StaminaResultInfo[] _staminaResult;

	public override string HeroIndices
	{
		get
		{
			return base.HeroIndices;
		}
		set
		{
			base.HeroIndices = value;
			_heroIndexList = InfoHelper.ConvToIntListFromServerString(value);
		}
	}

	public override string ClientResult
	{
		get
		{
			return base.ClientResult;
		}
		set
		{
			base.ClientResult = value;
			_clientResultList = InfoHelper.ConvToIntListFromServerString(value);
		}
	}

	public override string ServerResult
	{
		get
		{
			return base.ServerResult;
		}
		set
		{
			base.ServerResult = value;
			_serverResultList = InfoHelper.ConvToIntListFromServerString(value);
			UpdateWinLoseCount();
		}
	}

	public override string BeginTime
	{
		get
		{
			return base.BeginTime;
		}
		set
		{
			base.BeginTime = value;
			Vespa_String_Extension_Method2.TryParseNetworkTime2(base.BeginTime, out _convBeginTime);
		}
	}

	public override string CompleteTime
	{
		get
		{
			return base.CompleteTime;
		}
		set
		{
			base.CompleteTime = value;
			Vespa_String_Extension_Method2.TryParseNetworkTime2(base.CompleteTime, out _convCompleteTime);
		}
	}

	public List<int> HeroIndexList => _heroIndexList;

	public DateTime ConvBeginTime => _convBeginTime;

	public DateTime ConvCompleteTime => _convCompleteTime;

	public List<int> ServerResultList => _serverResultList;

	public int LastWinCount => _lastWinCount;

	public int LastLoseCount => _lastLoseCount;

	public CurrencyResultInfo3[] CurrencyResults => _currencyResults;

	public ItemResultInfo[] ItemResults => _itemResults;

	public StaminaResultInfo[] StaminaResult => _staminaResult;

	public TimeSpan RemainTime => NGame2.NUI.Common.GetRemainTime(ConvCompleteTime);

	public TimeSpan TotalTime => ConvCompleteTime - ConvBeginTime;

	public TimeSpan TimeForEach => new TimeSpan(TotalTime.Ticks / RepeatCount);

	public int LastCalculatedResultCount
	{
		get
		{
			if (RemainTime.Ticks == 0L)
			{
				return RepeatCount;
			}
			if (TimeForEach.Ticks == 0L)
			{
				return RepeatCount;
			}
			return RepeatCount - Mathf.Clamp((int)(RemainTime.Ticks / TimeForEach.Ticks), 0, RepeatCount);
		}
	}

	public WCL_DispatchBattleInfo Copy(DispatchBattleInfo info)
	{
		if (info == null)
		{
			return this;
		}
		SlotIndex = info.SlotIndex;
		ChapterIndex = info.ChapterIndex;
		DungeonIndex = info.DungeonIndex;
		Difficulty = info.Difficulty;
		DeckIndex = info.DeckIndex;
		RepeatCount = info.RepeatCount;
		HeroIndices = info.HeroIndices;
		BeginTime = info.BeginTime;
		CompleteTime = info.CompleteTime;
		ClientResult = info.ClientResult;
		ClientResultTimeMs = info.ClientResultTimeMs;
		ServerResult = info.ServerResult;
		UpdateDispatchState(info);
		WinCount = info.WinCount;
		LoseCount = info.LoseCount;
		_currencyResults = null;
		_itemResults = null;
		_staminaResult = null;
		return this;
	}

	public void SetRewardInfo(CurrencyResultInfo3[] currencyResults, ItemResultInfo[] itemResults, StaminaResultInfo[] staminaResult)
	{
		_currencyResults = currencyResults;
		_itemResults = itemResults;
		_staminaResult = staminaResult;
	}

	public void SetStaminaResult(StaminaResultInfo[] staminaResult)
	{
		_staminaResult = staminaResult;
	}

	public bool IsTimeEnd()
	{
		if (NGame2.NUI.Common.GetRemainTime(ConvCompleteTime).Ticks > 0)
		{
			return false;
		}
		if (__Extension_Method_ICollection__.COUNT(ServerResultList) < RepeatCount)
		{
			return false;
		}
		return true;
	}

	public bool IsTimeForEach()
	{
		if (RemainTime.TotalSeconds % TimeForEach.TotalSeconds > 1.0)
		{
			return false;
		}
		return true;
	}

	private void UpdateDispatchState(DispatchBattleInfo info)
	{
		State = info.State;
		if (State == DispatchState.Battle && IsTimeEnd())
		{
			State = DispatchState.ReadyToComplete;
		}
	}

	private void UpdateWinLoseCount()
	{
		_lastWinCount = 0;
		_lastLoseCount = 0;
		int i = 0;
		for (int count = _serverResultList.Count; i < count; i++)
		{
			if (_serverResultList[i] == 1)
			{
				_lastWinCount++;
			}
			else
			{
				_lastLoseCount++;
			}
		}
	}
}
