using System;
using System.Collections.Generic;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NAccount;

public class UserMonthlyHeroManager : NShared.UserMonthlyHeroManager
{
	private readonly UserObserverManager<MonthlyHeroInfo> _observerManager = new UserObserverManager<MonthlyHeroInfo>();

	public IUserObserverManager ObserverManager => _observerManager;

	public bool MothlyHeroWindowVisited { get; set; }

	public DateTime GetCurrentLocalTime()
	{
		return GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime();
	}

	public bool ExistReceiableReward()
	{
		DateTime currentLocalTime = GetCurrentLocalTime();
		IEnumerator<MonthlyHeroData> enumerator = GameRoot.Instance.MonthlyHeroDataContainer.GetAllData().GetEnumerator();
		while (enumerator.MoveNext())
		{
			int index = enumerator.Current.Index;
			if (Get(index) == null)
			{
				DateTime openDateTime = enumerator.Current.GetOpenDateTime();
				if (currentLocalTime > openDateTime)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsExistCurrentMonthlyHero(int creatureIndex)
	{
		IList<MonthlyHeroData> allData = GameRoot.Instance.MonthlyHeroDataContainer.GetAllData();
		DateTime currentLocalTime = GetCurrentLocalTime();
		MonthlyHeroData monthlyHeroData = null;
		for (int i = 0; i < allData.Count; i++)
		{
			monthlyHeroData = allData[i];
			if (monthlyHeroData != null && monthlyHeroData.HeroIndex == creatureIndex)
			{
				DateTime openDateTime = monthlyHeroData.GetOpenDateTime();
				if (currentLocalTime.Year == openDateTime.Year && currentLocalTime.Month >= openDateTime.Month && (currentLocalTime.Month != openDateTime.Month || currentLocalTime.Day >= openDateTime.Day))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void Apply(MonthlyHeroInfo info)
	{
		if (info != null)
		{
			Add(info);
			_observerManager.AnnounceToObservers(info);
		}
	}

	public void Apply(IList<MonthlyHeroInfo> info)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(info); i++)
		{
			Apply(info[i]);
		}
	}

	protected override void OnAddData(int key, MonthlyHeroInfo value)
	{
		base.OnAddData(key, value);
		_observerManager.AnnounceToObservers(value);
	}
}
