using System;
using NGame2.NAccount;
using NGame2.NAchievement;
using NGame2.NContentsInfo;
using NGame2.NQuest;
using NGame2.NUI.NManager.NBooster;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class MissionComponentData
{
	private AchievementComponent.ITextProvider AchievementTextProvider;

	private AchievementComponent.IDataProvider AchievementDataProvider;

	private static Type[] dataType = new Type[5]
	{
		typeof(AchievementData),
		typeof(SubQuestData),
		typeof(WebContentInfo),
		typeof(AttendanceData),
		typeof(BoosterInfo)
	};

	public Type DataType;

	public object Data { get; set; }

	public MissionVisualData VisualData { get; set; }

	public int Index
	{
		get
		{
			if (GetData<AchievementData>() != null)
			{
				return GetData<AchievementData>().Index;
			}
			if (GetData<SubQuestData>() != null)
			{
				return GetData<SubQuestData>().QuestIndex;
			}
			if (GetData<WebContentInfo>() != null)
			{
				return GetData<WebContentInfo>().Index;
			}
			if (GetData<AttendanceData>() != null)
			{
				return GetData<AttendanceData>().Index;
			}
			return 0;
		}
	}

	public string MissionComponentTitle
	{
		get
		{
			if (VisualData == null)
			{
				return Title;
			}
			MissionCategoryData data = GameRoot.Instance.MissionCategoryDataContainer.GetData(VisualData.MainCategoryIndex, VisualData.SubCategoryIndex);
			EMissionType missionType = data.MissionType;
			if ((uint)(missionType - 8) <= 2u)
			{
				string arg = LocalizationManager.GetStringDirect(data.MainCategoryLocalKey).Replace("\n", "");
				return $"{Title}\n{arg}";
			}
			return Title;
		}
	}

	public string Title
	{
		get
		{
			if (GetData<AchievementData>() != null)
			{
				AchievementData data = GetData<AchievementData>();
				return SingletonUpdater<QuestManager>.instance.GetLocalizeMissionText(data.Title, data.Variable1);
			}
			if (GetData<SubQuestData>() != null)
			{
				SubQuestData data2 = GetData<SubQuestData>();
				return SingletonUpdater<QuestManager>.instance.GetLocalizeMissionText(data2.Title, data2.Variable1);
			}
			if (GetData<WebContentInfo>() != null)
			{
				return GetData<WebContentInfo>().Title;
			}
			if (GetData<AttendanceData>() != null)
			{
				AttendanceInfoData data3 = GameRoot.Instance.AttendanceInfoDataContainer.GetData(Index);
				if (data3 == null)
				{
					return string.Empty;
				}
				return LocalizationManager.GetStringDirect(data3.TitleKey);
			}
			if (GetData<BoosterInfo>() != null)
			{
				BoosterInfo data4 = GetData<BoosterInfo>();
				if (data4 == null)
				{
					return string.Empty;
				}
				return data4.Name;
			}
			return string.Empty;
		}
	}

	public string Goal
	{
		get
		{
			if (GetData<AchievementData>() != null)
			{
				AchievementData data = GetData<AchievementData>();
				return SingletonUpdater<QuestManager>.instance.GetLocalizeMissionText(data.Goal, data.Variable3);
			}
			if (GetData<SubQuestData>() != null)
			{
				SubQuestData data2 = GetData<SubQuestData>();
				return SingletonUpdater<QuestManager>.instance.GetLocalizeMissionText(data2.Goal, data2.Variable3);
			}
			if (GetData<WebContentInfo>() != null)
			{
				Vespa_String_Extension_Method2.TryParseNetworkTime2(GetData<WebContentInfo>().EndTime, out var o_dateTime);
				int num = Mathf.Max((int)(o_dateTime - GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime()).TotalSeconds, 0);
				long num2 = num;
				int num3 = 0;
				if (num2 > num3)
				{
					return UIHelper.GetRemainTimeLabelText(num2);
				}
				return LocalizationManager.GetStringDirect("ADVENTURE_NOTICE_STAMINA_END");
			}
			if (GetData<AttendanceData>() != null)
			{
				if (AttendanceKeywordGroup.Helper.CreateByAnyAttendanceIndex(Index).TryGetRemainTimeSpan(out var remainTimeSpan))
				{
					int num4 = Mathf.Max((int)remainTimeSpan.TotalSeconds, 0);
					long num5 = num4;
					int num6 = 0;
					if (num5 > num6)
					{
						return UIHelper.GetRemainTimeLabelText(num5);
					}
					return LocalizationManager.GetStringDirect("ADVENTURE_NOTICE_STAMINA_END");
				}
			}
			else if (GetData<BoosterInfo>() != null)
			{
				BoosterInfo data3 = GetData<BoosterInfo>();
				if (data3 == null)
				{
					return string.Empty;
				}
				int num7 = Mathf.Max((int)(data3.EndTimeUTC - GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime()).TotalSeconds, 0);
				long num8 = num7;
				int num9 = 0;
				if (num8 > num9)
				{
					return UIHelper.GetRemainTimeLabelText(num8);
				}
				return LocalizationManager.GetStringDirect("ADVENTURE_NOTICE_STAMINA_END");
			}
			return string.Empty;
		}
	}

	public string Desc
	{
		get
		{
			if (GetData<AchievementData>() != null)
			{
				AchievementData data = GetData<AchievementData>();
				return SingletonUpdater<QuestManager>.instance.GetLocalizeMissionText(data.Desc, data.Variable2);
			}
			if (GetData<SubQuestData>() != null)
			{
				SubQuestData data2 = GetData<SubQuestData>();
				return SingletonUpdater<QuestManager>.instance.GetLocalizeMissionText(data2.Desc, data2.Variable2);
			}
			if (GetData<WebContentInfo>() != null)
			{
				Vespa_String_Extension_Method2.TryParseNetworkTime2(GetData<WebContentInfo>().EndTime, out var o_dateTime);
				int num = Mathf.Max((int)(o_dateTime - GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime()).TotalSeconds, 0);
				long num2 = num;
				int num3 = 0;
				if (num2 > num3)
				{
					return UIHelper.GetRemainTimeLabelText(num2);
				}
				return string.Empty;
			}
			return string.Empty;
		}
	}

	public string GoalRequirement
	{
		get
		{
			if (GetData<AchievementData>() != null)
			{
				AchievementDataProvider.GetAchievementInfo(Index);
				SingletonAuto<AchievementChecker>.instance.GetAchievementAndRequirement(Index, out var curProgress, out var requirement, out var _);
				if (AchievementChecker.Completed(Index))
				{
					return $"{Goal} ({curProgress}/{curProgress})";
				}
				if (requirement <= 1)
				{
					return Goal;
				}
				return $"{Goal} ({curProgress}/{requirement})";
			}
			if (GetData<SubQuestData>() != null)
			{
				SubQuestData data = GetData<SubQuestData>();
				SubQuestInfo subQuestInfoByQuestIndex = GlobalAutoBehaviour<AccountManager>.instance.QuestManager.GetSubQuestInfoByQuestIndex(Index);
				long num = 0L;
				int reqProgress = data.GetReqProgress();
				if (reqProgress <= 1)
				{
					return Goal;
				}
				if (subQuestInfoByQuestIndex != null)
				{
					if (GlobalAutoBehaviour<AccountManager>.instance.QuestManager.GetSubQuestStatus(data) == QuesetStatusType.Complete)
					{
						return $"{Goal} ({reqProgress}/{reqProgress})";
					}
					num = subQuestInfoByQuestIndex.Progress;
					return $"{Goal} ({num}/{reqProgress})";
				}
				return $"{Goal} ({num}/{reqProgress})";
			}
			return string.Empty;
		}
	}

	public string OpenConditionText
	{
		get
		{
			if (GetData<SubQuestData>() != null)
			{
				return LocalizationManager.GetStringDirect(GetData<SubQuestData>().OpenConditionLocalKey);
			}
			return string.Empty;
		}
	}

	public MissionComponentData(MissionVisualData data)
	{
		VisualData = data;
		if (VisualData == null)
		{
			VespaDebug.LogError("ElementData is Null. (VisualData)");
		}
		AchievementTextProvider = SingletonAuto<AchievementTextCombine>.instance;
		AchievementDataProvider = SingletonAuto<AchievementChecker>.instance;
	}

	public MissionComponentData(object data, MissionVisualData visualData = null)
	{
		Data = data;
		VisualData = visualData;
		if (Data == null)
		{
			if (VisualData == null)
			{
				VespaDebug.LogError("ElementData is Null. (Data, VisualData)");
				return;
			}
			VespaDebug.LogError($"ElementData is Null. [{VisualData.MainCategoryIndex},{VisualData.SubCategoryIndex},{VisualData.MainStep},{VisualData.SubStep}]");
		}
		else
		{
			DataType = Data.GetType();
			AchievementTextProvider = SingletonAuto<AchievementTextCombine>.instance;
			AchievementDataProvider = SingletonAuto<AchievementChecker>.instance;
		}
	}

	public MissionComponentData(int achievementIndex, MissionVisualData visualData = null)
	{
		AchievementTextProvider = SingletonAuto<AchievementTextCombine>.instance;
		AchievementDataProvider = SingletonAuto<AchievementChecker>.instance;
		Data = AchievementDataProvider.GetAchievementData(achievementIndex);
		if (Data == null)
		{
			Data = AchievementDataProvider.GetLastAchievementData(achievementIndex);
		}
		VisualData = visualData;
		if (Data == null)
		{
			if (VisualData == null)
			{
				VespaDebug.LogError("ElementData is Null. (Data, VisualData)");
				return;
			}
			VespaDebug.LogError($"ElementData is Null. [{VisualData.MainCategoryIndex},{VisualData.SubCategoryIndex},{VisualData.MainStep},{VisualData.SubStep}]");
		}
		else
		{
			DataType = Data.GetType();
		}
	}

	public T GetData<T>() where T : class
	{
		if (typeof(T).Equals(DataType))
		{
			return Data as T;
		}
		return null;
	}

	public AchievementComponent.EAchievementStatus GetAchievementStatus()
	{
		if (AchievementDataProvider != null)
		{
			return AchievementDataProvider.GetAchievementStatus(Index);
		}
		return AchievementComponent.EAchievementStatus.InProgress;
	}
}
