using System.Collections.Generic;
using NGame2.NDropBonusEvent;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NAchievement;

public class AchievementNotiChecker : SingletonAuto<AchievementNotiChecker>
{
	private class queueEntry
	{
		public AchievementData data;

		public float enqueueTime { get; private set; }

		public bool fromReward { get; private set; }

		public queueEntry(AchievementData _data, bool _fromReward = false)
		{
			data = _data;
			enqueueTime = Time.time;
			fromReward = _fromReward;
		}
	}

	private const float _holdDuration = 0.5f;

	private List<queueEntry> _newlyAddedAchievement = new List<queueEntry>();

	private List<int> _nonClearedAchievementSet = new List<int>();

	public AchievementNotiChecker()
	{
		InitAchievementList();
	}

	public void InitAchievementList()
	{
		_nonClearedAchievementSet.Clear();
		InitAchievementList(AchievementType.General);
		InitAchievementList(AchievementType.Hero);
		InitAchievementList(AchievementType.Daily);
		InitAchievementList(AchievementType.Weekly);
		InitLimitedEventMissionList();
	}

	private void InitAchievementList(AchievementType achievementType)
	{
		HashSet<int> achievementIndexSet = GameRoot.Instance.AchievementDataContainer.GetAchievementIndexSet(achievementType);
		if (achievementIndexSet == null)
		{
			return;
		}
		HashSet<int>.Enumerator enumerator = achievementIndexSet.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (!AchievementChecker.Cleared(enumerator.Current) && !AchievementChecker.Completed(enumerator.Current))
			{
				_nonClearedAchievementSet.Add(enumerator.Current);
			}
		}
	}

	private void InitLimitedEventMissionList()
	{
		HashSet<int> achievementIndexSet = GameRoot.Instance.AchievementDataContainer.GetAchievementIndexSet(AchievementType.Limited);
		if (achievementIndexSet == null)
		{
			return;
		}
		HashSet<int>.Enumerator enumerator = achievementIndexSet.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (!AchievementChecker.Cleared(enumerator.Current) && !AchievementChecker.Completed(enumerator.Current))
			{
				AchievementData data = GameRoot.Instance.AchievementDataContainer.GetData(enumerator.Current, 1);
				if (data != null && GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvent(data.LimitedEventMissionType, data.EventName) != null)
				{
					_nonClearedAchievementSet.Add(enumerator.Current);
				}
			}
		}
	}

	public void RewardAchievement(int achievementIndex)
	{
		if (GameRoot.Instance.AchievementDataContainer.CheckAchievementType(achievementIndex, AchievementType.General) || GameRoot.Instance.AchievementDataContainer.CheckAchievementType(achievementIndex, AchievementType.Hero))
		{
			RefreshAchievementSet(achievementIndex);
		}
	}

	public void RewardLimitedEventMission(int achievementIndex)
	{
		if (GameRoot.Instance.AchievementDataContainer.CheckAchievementType(achievementIndex, AchievementType.Limited))
		{
			AchievementData data = GameRoot.Instance.AchievementDataContainer.GetData(achievementIndex, 1);
			if (data != null && GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvent(data.LimitedEventMissionType, data.EventName) != null)
			{
				RefreshAchievementSet(achievementIndex);
			}
		}
	}

	private void RefreshAchievementSet(int achievementIndex)
	{
		if (AchievementChecker.Completed(achievementIndex))
		{
			_nonClearedAchievementSet.Remove(achievementIndex);
		}
		else if (AchievementChecker.Cleared(achievementIndex))
		{
			_nonClearedAchievementSet.Remove(achievementIndex);
			AchievementData achievementData = SingletonAuto<AchievementChecker>.instance.GetAchievementData(achievementIndex);
			_newlyAddedAchievement.Add(new queueEntry(achievementData, _fromReward: true));
		}
		else if (!_nonClearedAchievementSet.Contains(achievementIndex))
		{
			_nonClearedAchievementSet.Add(achievementIndex);
		}
	}

	public void CheckNewlyUpdatedIndex()
	{
		if (_nonClearedAchievementSet == null)
		{
			return;
		}
		List<int>.Enumerator enumerator = _nonClearedAchievementSet.GetEnumerator();
		List<int> list = new List<int>();
		while (enumerator.MoveNext())
		{
			if (AchievementChecker.Cleared(enumerator.Current))
			{
				list.Add(enumerator.Current);
				AchievementData achievementData = SingletonAuto<AchievementChecker>.instance.GetAchievementData(enumerator.Current);
				_newlyAddedAchievement.Add(new queueEntry(achievementData));
			}
		}
		foreach (int item in list)
		{
			_nonClearedAchievementSet.Remove(item);
		}
	}

	public AchievementData GetFirstComponent(out bool isReward)
	{
		isReward = false;
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_newlyAddedAchievement))
		{
			return null;
		}
		queueEntry queueEntry = _newlyAddedAchievement[0];
		if (Time.time - queueEntry.enqueueTime > 0.5f)
		{
			_newlyAddedAchievement.RemoveAt(0);
			isReward = queueEntry.fromReward;
			return queueEntry.data;
		}
		return null;
	}

	public void Clear()
	{
		_newlyAddedAchievement.Clear();
	}
}
