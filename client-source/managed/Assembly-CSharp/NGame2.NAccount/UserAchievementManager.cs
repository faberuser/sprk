using System.Collections.Generic;
using NGame2.NAchievement;
using NShared;
using NVespa;

namespace NGame2.NAccount;

public class UserAchievementManager : NShared.UserAchievementManager
{
	public enum AchievementChangeType
	{
		UpdatedAchievement
	}

	public class ChangeAchievementInfo
	{
		public AchievementChangeType changeType;
	}

	private readonly UserObserverManager<ChangeAchievementInfo> _observerManager = new UserObserverManager<ChangeAchievementInfo>();

	public IUserObserverManager ObserverManager => _observerManager;

	public List<AchievementInfo> GetAchievementList(AchievementType AchievementType)
	{
		List<AchievementInfo> list = new List<AchievementInfo>();
		IEnumerator<AchievementInfo> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			AchievementData data = GameRoot.Instance.AchievementDataContainer.GetData(enumerator.Current.AchievementIndex, enumerator.Current.LastStep + 1);
			if (data != null && data.Type == AchievementType)
			{
				list.Add(enumerator.Current);
			}
		}
		return list;
	}

	public List<AchievementInfo> GetHeroAchievementList(int HeroIndex)
	{
		List<AchievementInfo> list = new List<AchievementInfo>();
		IEnumerator<AchievementInfo> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			AchievementData data = GameRoot.Instance.AchievementDataContainer.GetData(enumerator.Current.AchievementIndex, enumerator.Current.LastStep + 1);
			if (data != null && data.ReqType == AchievementReqType.HeroIndex && Vespa_String_Extension_Method.TOINT(data.ReqValue, 0) == HeroIndex)
			{
				list.Add(enumerator.Current);
			}
		}
		return list;
	}

	public List<AchievementInfo> GetCompletedAchievementList(AchievementReqType reqType)
	{
		List<AchievementInfo> list = new List<AchievementInfo>();
		IEnumerator<AchievementInfo> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current.LastStep > 0)
			{
				AchievementData data = GameRoot.Instance.AchievementDataContainer.GetData(enumerator.Current.AchievementIndex, 1);
				if (data != null && data.ReqType == reqType)
				{
					list.Add(enumerator.Current);
				}
			}
		}
		return list;
	}

	public bool HeroAchievementCompleted(int HeroIndex)
	{
		HashSet<int> heroAchievementIndexSet = GameRoot.Instance.AchievementDataContainer.GetHeroAchievementIndexSet(HeroIndex);
		if (heroAchievementIndexSet == null)
		{
			return false;
		}
		HashSet<int>.Enumerator enumerator = heroAchievementIndexSet.GetEnumerator();
		while (enumerator.MoveNext())
		{
			AchievementInfo achievementInfo = Get(enumerator.Current);
			if (achievementInfo == null)
			{
				return false;
			}
			if (GameRoot.Instance.AchievementDataContainer.GetData(enumerator.Current, achievementInfo.LastStep + 1) != null)
			{
				return false;
			}
		}
		return true;
	}

	public List<int> GetProgressAchievementIndices()
	{
		List<int> list = new List<int>();
		IEnumerator<AchievementInfo> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (!AchievementChecker.Cleared(enumerator.Current) && !AchievementChecker.Completed(enumerator.Current))
			{
				list.Add(enumerator.Current.AchievementIndex);
			}
		}
		return list;
	}

	public void Apply(AchievementInfo[] achievementInfos)
	{
		List<int> progressAchievementIndices = GetProgressAchievementIndices();
		Set(achievementInfos);
		List<int> list = new List<int>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(progressAchievementIndices); i++)
		{
			int num = progressAchievementIndices[i];
			if (AchievementChecker.Cleared(num))
			{
				list.Add(num);
			}
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(list); j++)
		{
		}
		SingletonAuto<AchievementNotiChecker>.instance.CheckNewlyUpdatedIndex();
		_observerManager.AnnounceToObservers(new ChangeAchievementInfo
		{
			changeType = AchievementChangeType.UpdatedAchievement
		});
	}
}
