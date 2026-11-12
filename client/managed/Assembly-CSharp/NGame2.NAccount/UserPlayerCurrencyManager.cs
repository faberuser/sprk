using System;
using System.Collections.Generic;
using NShared;
using NShared.GetPlayerCurrencyInfo;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NWeb;

namespace NGame2.NAccount;

public class UserPlayerCurrencyManager : NShared.UserPlayerCurrencyManager
{
	public class ChangedCurrencyInfo
	{
		public CurrencyType currencyType;

		public long Amount;

		public long DailyAmount;

		public long NextResetTime;
	}

	private readonly UserObserverManager<ChangedCurrencyInfo> _observerManager = new UserObserverManager<ChangedCurrencyInfo>();

	private List<PlayerCurrencyInfo> _listNextResetTimeCurrency = new List<PlayerCurrencyInfo>();

	private float _updateInterval = 5f;

	private float _intervalTime;

	private bool _resquestWaiting;

	public IUserObserverManager ObserverManager => _observerManager;

	public void Apply(CurrencyResultInfo3 currencyChangedInfo)
	{
		if (currencyChangedInfo != null)
		{
			PlayerCurrencyInfo playerCurrencyInfo = Get(currencyChangedInfo.CurrencyType);
			if (playerCurrencyInfo == null)
			{
				playerCurrencyInfo = new PlayerCurrencyInfo
				{
					CurrencyType = currencyChangedInfo.CurrencyType,
					Amount = currencyChangedInfo.NewValue,
					DailyAmount = currencyChangedInfo.GetFieldValue("DailyAmount"),
					NextResetTime = currencyChangedInfo.GetFieldValue("NextResetTime")
				};
			}
			else
			{
				playerCurrencyInfo.CurrencyType = currencyChangedInfo.CurrencyType;
				playerCurrencyInfo.Amount = currencyChangedInfo.NewValue;
				playerCurrencyInfo.DailyAmount = currencyChangedInfo.GetFieldValue("DailyAmount");
				playerCurrencyInfo.NextResetTime = currencyChangedInfo.GetFieldValue("NextResetTime");
			}
			Apply(playerCurrencyInfo);
		}
	}

	public void Apply(CurrencyResultInfo3[] infos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
		{
			Apply(infos[i]);
		}
	}

	public void Apply(PlayerCurrencyInfo info)
	{
		Add(info);
		ChangedCurrencyInfo changedInfo = new ChangedCurrencyInfo
		{
			currencyType = info.CurrencyType,
			Amount = info.Amount,
			DailyAmount = info.DailyAmount,
			NextResetTime = info.NextResetTime
		};
		_observerManager.AnnounceToObservers(changedInfo);
	}

	protected override void OnAddData(CurrencyType key, PlayerCurrencyInfo value)
	{
		AddOrRemoveNextResetCurrency(value);
		base.OnAddData(key, value);
	}

	public override bool Remove(CurrencyType key)
	{
		bool result = base.Remove(key);
		RemoveNextResetCurrency(key);
		return result;
	}

	public void Apply(PlayerCurrencyInfo[] infos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
		{
			Apply(infos[i]);
		}
	}

	public long GetCurrencyValue(UserManager.UserStatType statType)
	{
		return GetCurrencyValue(statType.ToString());
	}

	public long GetCurrencyValue(string statName)
	{
		return Get((CurrencyType)Enum.Parse(typeof(CurrencyType), statName))?.Amount ?? 0;
	}

	private void AddOrRemoveNextResetCurrency(PlayerCurrencyInfo currencyInfo)
	{
		DateTime currentServerUtcTime = GetCurrentServerUtcTime();
		currencyInfo.SetNextResetTime(currentServerUtcTime);
		bool num = currencyInfo.NextResetTime > 0;
		int num2 = _listNextResetTimeCurrency.FindIndex((PlayerCurrencyInfo info) => info.CurrencyType == currencyInfo.CurrencyType);
		if (num2 != -1)
		{
			_listNextResetTimeCurrency.RemoveAt(num2);
		}
		if (num)
		{
			_listNextResetTimeCurrency.Add(currencyInfo);
		}
	}

	private void RemoveNextResetCurrency(CurrencyType currencyType)
	{
		for (int i = 0; i < _listNextResetTimeCurrency.Count; i++)
		{
			PlayerCurrencyInfo playerCurrencyInfo = _listNextResetTimeCurrency[i];
			if (playerCurrencyInfo != null && playerCurrencyInfo.CurrencyType == currencyType)
			{
				_listNextResetTimeCurrency.RemoveAt(i);
				break;
			}
		}
	}

	private DateTime GetCurrentServerUtcTime()
	{
		if (!AccountManager.IsLogin())
		{
			return DateTime.MaxValue;
		}
		return GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
	}

	public void Update()
	{
		if (!Helper.GameDataLoadedAndLogined() || _resquestWaiting || _listNextResetTimeCurrency.Count <= 0)
		{
			return;
		}
		_intervalTime += RealTime.deltaTime;
		if (_intervalTime < _updateInterval)
		{
			return;
		}
		_intervalTime = 0f;
		DateTime currentServerUtcTime = GetCurrentServerUtcTime();
		CurrencyType currencyType = CurrencyType.None;
		for (int i = 0; i < _listNextResetTimeCurrency.Count; i++)
		{
			PlayerCurrencyInfo playerCurrencyInfo = _listNextResetTimeCurrency[i];
			if (playerCurrencyInfo == null || playerCurrencyInfo.NextResetTime <= 0)
			{
				_listNextResetTimeCurrency.RemoveAt(i);
				i--;
			}
			else if (playerCurrencyInfo.GetNextResetTime().CompareTo(currentServerUtcTime) <= 0)
			{
				currencyType = playerCurrencyInfo.CurrencyType;
				break;
			}
		}
		if (currencyType != CurrencyType.None)
		{
			RequestCurrencyInfo(currencyType);
		}
	}

	public void RequestCurrencyInfo(CurrencyType currencyType)
	{
		NShared.GetPlayerCurrencyInfo.Request request = new NShared.GetPlayerCurrencyInfo.Request();
		request.CurrencyType = currencyType;
		_resquestWaiting = true;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, WebRequestFlag.DisableRetry, delegate(NShared.GetPlayerCurrencyInfo.Response response)
		{
			_resquestWaiting = false;
			if (response.BaseResult == BaseResultType.Success && response.Result == ResultType.Success)
			{
				Apply(response.PlayerCurrencyInfo);
			}
		});
	}
}
