using System.Collections.Generic;
using NShared;

namespace NGame2.NAccount;

public class UserAccumulateLoginManager : NShared.UserAccumulateLoginManager
{
	private readonly UserObserverManager<LoginDailyInfo> _observerManager = new UserObserverManager<LoginDailyInfo>();

	public IUserObserverManager ObserverManager => _observerManager;

	public void Apply(LoginDailyInfo info)
	{
		if (info != null)
		{
			Add(info);
			_observerManager.AnnounceToObservers(info);
		}
	}

	public void Apply(IList<LoginDailyInfo> info)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(info); i++)
		{
			Apply(info[i]);
		}
	}

	protected override void OnAddData(int key, LoginDailyInfo value)
	{
		base.OnAddData(key, value);
		_observerManager.AnnounceToObservers(value);
	}

	public bool IsRewarded(int accumulateLoginRewardDataIndex)
	{
		LoginDailyInfo loginDailyInfo = Get(accumulateLoginRewardDataIndex);
		if (loginDailyInfo == null)
		{
			return false;
		}
		if (String_Extension_Method.IsNullOrEmpty(loginDailyInfo.CompletedTime))
		{
			return false;
		}
		return true;
	}

	public bool ExistLoginReward(int accumlateLoginDay, IList<AccumulateLoginRewardData> allData)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(allData); i++)
		{
			if (accumlateLoginDay >= allData[i].Day && !IsRewarded(allData[i].Index))
			{
				return true;
			}
		}
		return false;
	}
}
