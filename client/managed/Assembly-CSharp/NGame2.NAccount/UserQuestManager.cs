using System.Collections.Generic;
using NShared;

namespace NGame2.NAccount;

public class UserQuestManager : NShared.UserQuestManager
{
	private Dictionary<int, SubQuestInfo> _subQuestInfoDic = new Dictionary<int, SubQuestInfo>();

	private readonly List<int> _clearMainQuestSetpList = new List<int>();

	private readonly List<int> _clearMainQuestByOpenContentsList = new List<int>();

	private readonly List<SubQuestInfo> _clearSubQuestInfoList = new List<SubQuestInfo>();

	private readonly List<SubQuestInfo> _clearMainQuestInfoList = new List<SubQuestInfo>();

	private HashSet<int> _hashMainIndex;

	private readonly UserObserverManager<MainQuestInfo> _mainQuestObserverManager;

	private readonly UserObserverManager<SubQuestInfo> _subQuestObserverManager;

	private Dictionary<string, bool> _openedSpecialMissionPopup = new Dictionary<string, bool>();

	public IUserObserverManager MainQuestObserverManager => _mainQuestObserverManager;

	public IUserObserverManager SubQuestObserverManager => _subQuestObserverManager;

	public MainQuestInfo MainQuestInfo { get; private set; }

	public List<int> ClearMainQuestStepList => _clearMainQuestSetpList;

	public List<int> ClearMainQuestByOpenContentsList => _clearMainQuestByOpenContentsList;

	public List<SubQuestInfo> ClearSubQuestInfoList => _clearSubQuestInfoList;

	public List<SubQuestInfo> ClearMainQuestInfoList => _clearMainQuestInfoList;

	public bool IsGetRewardable
	{
		get
		{
			foreach (SubQuestInfo value in _subQuestInfoDic.Values)
			{
				if (value != null && IsGetRewardableSubQuest(value.SubQuestIndex))
				{
					return true;
				}
			}
			return false;
		}
	}

	public UserQuestManager()
	{
		_mainQuestObserverManager = new UserObserverManager<MainQuestInfo>();
		_subQuestObserverManager = new UserObserverManager<SubQuestInfo>();
	}

	public void Apply(MainQuestInfo info)
	{
		if (info == null)
		{
			return;
		}
		MainQuestInfo mainQuestInfo = MainQuestInfo;
		MainQuestInfo = info;
		if (mainQuestInfo != null)
		{
			for (int i = mainQuestInfo.Step; i < info.Step; i++)
			{
				_clearMainQuestSetpList.Add(i);
				_clearMainQuestByOpenContentsList.Add(i);
			}
		}
		_clearMainQuestSetpList.Sort();
		_clearMainQuestByOpenContentsList.Sort();
		AnnounceMainQuest(info);
	}

	private void AnnounceMainQuest(MainQuestInfo info)
	{
		_mainQuestObserverManager.AnnounceToObservers(info);
	}

	public void InitSubQuestInfos(SubQuestInfo[] infos)
	{
		_subQuestInfoDic.Clear();
		_clearSubQuestInfoList.Clear();
		_clearMainQuestInfoList.Clear();
		SubQuestInfo subQuestInfo = null;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
		{
			subQuestInfo = infos[i];
			if (subQuestInfo != null && !_subQuestInfoDic.ContainsKey(subQuestInfo.SubQuestIndex))
			{
				_subQuestInfoDic.Add(subQuestInfo.SubQuestIndex, subQuestInfo);
			}
		}
	}

	public void Apply(SubQuestInfo[] infos, bool isComplete)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(infos))
		{
			return;
		}
		_clearSubQuestInfoList.Clear();
		_clearMainQuestInfoList.Clear();
		SubQuestInfo subQuestInfo = null;
		SubQuestData subQuestData = null;
		SubQuestInfo subQuestInfo2 = null;
		HashSet<int> missionVisualMainIndexHash = GetMissionVisualMainIndexHash();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
		{
			subQuestInfo = infos[i];
			if (subQuestInfo == null)
			{
				continue;
			}
			if (!_subQuestInfoDic.ContainsKey(subQuestInfo.SubQuestIndex))
			{
				_subQuestInfoDic.Add(subQuestInfo.SubQuestIndex, subQuestInfo);
			}
			else
			{
				_ = _subQuestInfoDic[subQuestInfo.SubQuestIndex].LastStep;
				_subQuestInfoDic[subQuestInfo.SubQuestIndex].LastStep = subQuestInfo.LastStep;
				_subQuestInfoDic[subQuestInfo.SubQuestIndex].Progress = subQuestInfo.Progress;
			}
			if (isComplete)
			{
				continue;
			}
			subQuestData = GameRoot.Instance.SubQuestDataContainer.GetSubQuestData(subQuestInfo.SubQuestIndex);
			if (subQuestData == null)
			{
				continue;
			}
			if (subQuestData.StateType == QuestActionType.StepMissionClear)
			{
				foreach (SubQuestData item in GameRoot.Instance.SubQuestDataContainer.GetListByQuestIndex(subQuestInfo.SubQuestIndex))
				{
					if (subQuestInfo.Progress == item.GetReqProgress())
					{
						SubQuestInfo subQuestInfo3 = new SubQuestInfo
						{
							SubQuestIndex = subQuestInfo.SubQuestIndex,
							Progress = subQuestInfo.Progress,
							LastStep = item.Step - 1
						};
						_clearSubQuestInfoList.Add(subQuestInfo3);
						subQuestInfo2 = subQuestInfo3;
						break;
					}
				}
			}
			else
			{
				if (subQuestInfo.LastStep != 0)
				{
					continue;
				}
				if (subQuestInfo.Progress == subQuestData.GetReqProgress())
				{
					_clearSubQuestInfoList.Add(subQuestInfo);
					subQuestInfo2 = subQuestInfo;
					if (missionVisualMainIndexHash.Contains(subQuestInfo.SubQuestIndex))
					{
						_clearMainQuestInfoList.Add(subQuestInfo);
					}
				}
				else
				{
					if (subQuestInfo.Progress <= subQuestData.GetReqProgress())
					{
						continue;
					}
					int achievedNextStep = GetAchievedNextStep(subQuestInfo.Progress, subQuestData);
					if (achievedNextStep > subQuestData.Step)
					{
						int lastStep = achievedNextStep - 1;
						if (!IsExistClearSubQuestInfo(subQuestInfo.SubQuestIndex, lastStep))
						{
							SubQuestInfo subQuestInfo4 = new SubQuestInfo
							{
								SubQuestIndex = subQuestInfo.SubQuestIndex,
								Progress = subQuestInfo.Progress,
								LastStep = lastStep
							};
							_clearSubQuestInfoList.Add(subQuestInfo4);
							subQuestInfo2 = subQuestInfo4;
						}
					}
				}
			}
		}
		if (subQuestInfo2 != null)
		{
			AnnounceToSubQuestObservers(subQuestInfo2);
		}
	}

	private int GetAchievedNextStep(long achievedProgress, SubQuestData data)
	{
		int step = data.Step;
		for (SubQuestData nextData = data.GetNextData(); nextData != null; nextData = nextData.GetNextData())
		{
			int reqProgress = nextData.GetReqProgress();
			if (achievedProgress < reqProgress)
			{
				break;
			}
			step = nextData.Step;
		}
		return step;
	}

	private bool IsExistClearSubQuestInfo(int subQuestIndex, int lastStep)
	{
		bool result = false;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_clearSubQuestInfoList); i++)
		{
			SubQuestInfo subQuestInfo = _clearSubQuestInfoList[i];
			if (subQuestInfo != null && subQuestInfo.SubQuestIndex == subQuestIndex && subQuestInfo.LastStep == lastStep)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public SubQuestInfo GetSubQuestInfoByQuestIndex(int index)
	{
		_subQuestInfoDic.TryGetValue(index, out var value);
		return value;
	}

	public bool IsGetRewardableSubQuest(int subQuestIndex)
	{
		SubQuestInfo subQuestInfoByQuestIndex = GetSubQuestInfoByQuestIndex(subQuestIndex);
		if (subQuestInfoByQuestIndex == null)
		{
			return false;
		}
		int lastStepByQuestIndex = GameRoot.Instance.SubQuestDataContainer.GetLastStepByQuestIndex(subQuestIndex);
		if (lastStepByQuestIndex <= -1)
		{
			return false;
		}
		if (lastStepByQuestIndex == subQuestInfoByQuestIndex.LastStep)
		{
			return false;
		}
		SubQuestData subQuestData = GameRoot.Instance.SubQuestDataContainer.GetSubQuestData(subQuestIndex, subQuestInfoByQuestIndex.LastStep + 1);
		if (subQuestData == null)
		{
			return false;
		}
		if (subQuestInfoByQuestIndex.Progress >= subQuestData.GetReqProgress())
		{
			return true;
		}
		return false;
	}

	public bool IsGetCompleteSubQuest(int subQuestIndex)
	{
		SubQuestInfo subQuestInfoByQuestIndex = GetSubQuestInfoByQuestIndex(subQuestIndex);
		if (subQuestInfoByQuestIndex == null)
		{
			return false;
		}
		int lastStepByQuestIndex = GameRoot.Instance.SubQuestDataContainer.GetLastStepByQuestIndex(subQuestIndex);
		if (lastStepByQuestIndex <= -1)
		{
			return false;
		}
		if (lastStepByQuestIndex == subQuestInfoByQuestIndex.LastStep)
		{
			return true;
		}
		return false;
	}

	private HashSet<int> GetMissionVisualMainIndexHash()
	{
		if (_hashMainIndex == null)
		{
			_hashMainIndex = new HashSet<int>();
			List<MissionCategoryData> mainMissionList = GameRoot.Instance.MissionCategoryDataContainer.GetMainMissionList();
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(mainMissionList); i++)
			{
				MissionCategoryData missionCategoryData = mainMissionList[i];
				for (MissionVisualData missionVisualData = GameRoot.Instance.MissionVisualDataContainer.GetMissionVisualDataHead(missionCategoryData.MainCategoryIndex, missionCategoryData.SubCategoryIndex); missionVisualData != null; missionVisualData = missionVisualData.GetNextData())
				{
					if (!_hashMainIndex.Contains(missionVisualData.QuestIndex))
					{
						_hashMainIndex.Add(missionVisualData.QuestIndex);
					}
				}
			}
		}
		return _hashMainIndex;
	}

	public QuesetStatusType GetSubQuestStatus(SubQuestData data)
	{
		QuesetStatusType result = QuesetStatusType.Lock;
		if (data == null)
		{
			return result;
		}
		SubQuestInfo subQuestInfoByQuestIndex = GetSubQuestInfoByQuestIndex(data.QuestIndex);
		if (subQuestInfoByQuestIndex == null)
		{
			return result;
		}
		result = QuesetStatusType.Progress;
		int lastStepByQuestIndex = GameRoot.Instance.SubQuestDataContainer.GetLastStepByQuestIndex(data.QuestIndex);
		if (lastStepByQuestIndex <= -1)
		{
			return result;
		}
		if (lastStepByQuestIndex == subQuestInfoByQuestIndex.LastStep)
		{
			result = QuesetStatusType.Complete;
		}
		else
		{
			int step = subQuestInfoByQuestIndex.LastStep + 1;
			SubQuestData subQuestData = GameRoot.Instance.SubQuestDataContainer.GetSubQuestData(subQuestInfoByQuestIndex.SubQuestIndex, step);
			if (subQuestData == null)
			{
				return result;
			}
			int reqProgress = subQuestData.GetReqProgress();
			if (subQuestInfoByQuestIndex.Progress >= reqProgress)
			{
				result = QuesetStatusType.Reward;
			}
		}
		return result;
	}

	private void AnnounceToSubQuestObservers(SubQuestInfo info)
	{
		if (_subQuestObserverManager != null)
		{
			_subQuestObserverManager.AnnounceToObservers(info);
		}
	}

	public void SetSpecialMissionPopupOpen(int mainCategoryIndex, int subCategoryIndex, bool open)
	{
		string key = mainCategoryIndex + "_" + subCategoryIndex;
		if (!_openedSpecialMissionPopup.ContainsKey(key))
		{
			_openedSpecialMissionPopup.Add(key, open);
		}
		else
		{
			_openedSpecialMissionPopup[key] = open;
		}
	}

	public bool GetSpecialMissionPopupOpen(int mainCategoryIndex, int subCategoryIndex)
	{
		string key = mainCategoryIndex + "_" + subCategoryIndex;
		bool value = false;
		_openedSpecialMissionPopup.TryGetValue(key, out value);
		return value;
	}

	public bool CanOpenMainQuestNotiPopup()
	{
		bool result = false;
		if (Helper.CheckRestriction("OldMainQuestConfirmNotiPopupOpenCondition", cacheResult: true) || Helper.CheckRestriction("MainQuestConfirmNotiPopupOpenCondition"))
		{
			result = true;
		}
		return result;
	}

	public bool CanOpenSubQuestNotiPopup()
	{
		bool result = false;
		if (Helper.CheckRestriction("OldSubQuestConfirmNotiPopupOpenCondition", cacheResult: true) || Helper.CheckRestriction("SubQuestConfirmNotiPopupOpenCondition"))
		{
			result = true;
		}
		return result;
	}

	public bool CanOpenAchievementNotiPopup()
	{
		bool result = false;
		if (Helper.CheckRestriction("OldAchievementClearedPopupOpenCondition", cacheResult: true) || Helper.CheckRestriction("AchievementClearedPopupOpenCondition"))
		{
			result = true;
		}
		return result;
	}
}
