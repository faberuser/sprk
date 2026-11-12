using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NAchievement;
using NGame2.NContentsInfo;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager.Mission;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class LeftMenuMissionProgress : MonoBehaviour
{
	public interface Listner
	{
		void OnClickMission(MissionComponentData data);

		void OnClick();
	}

	public UISprite SpriteReward;

	public UISprite SpriteNew;

	public Transform RewardEffectTransform;

	public Transform MissionListTransform;

	public UILabel LabelShowMission01;

	public UILabel LabelShowMission02;

	public Animator MissionClearAnimator01;

	public Animator MissionClearAnimator02;

	public Animator MissionButtonAnimator;

	private List<MissionComponentData> listElementData = new List<MissionComponentData>();

	private MissionComponentData TopData;

	private MissionComponentData BottomData;

	private Coroutine coAnimator;

	private Listner iListner;

	public void Init(Listner listner)
	{
		TopData = null;
		BottomData = null;
		iListner = listner;
		LoadMissionData();
		SetupMission();
	}

	public void Clear()
	{
		listElementData.Clear();
	}

	public void Refresh()
	{
		LoadMissionData();
		SetupMission();
	}

	public void LoadMissionData()
	{
		listElementData.Clear();
		List<MissionVisualData> list = new List<MissionVisualData>(GameRoot.Instance.MissionVisualDataContainer.GetAllData());
		list.Sort(delegate(MissionVisualData a, MissionVisualData b)
		{
			if (a.MissionGradeValue > b.MissionGradeValue)
			{
				return -1;
			}
			if (a.MissionGradeValue < b.MissionGradeValue)
			{
				return 1;
			}
			if (a.QuestIndex > b.QuestIndex)
			{
				return 1;
			}
			return (a.QuestIndex < b.QuestIndex) ? (-1) : 0;
		});
		foreach (MissionVisualData item3 in list)
		{
			string questType = item3.QuestType;
			int questIndex = item3.QuestIndex;
			if (String_Extension_Method.IsNullOrEmpty(questType) || questIndex == 0)
			{
				continue;
			}
			MissionCategoryData missionCategoryDataHead = GameRoot.Instance.MissionCategoryDataContainer.GetMissionCategoryDataHead(item3.MainCategoryIndex);
			_ = missionCategoryDataHead?.MissionComponentValue;
			if (SingletonAuto<MissionStatus>.instance.IsMissionTimeout(missionCategoryDataHead))
			{
				continue;
			}
			if (questType.CompareTo("Achievement") == 0)
			{
				AchievementData data = GameRoot.Instance.AchievementDataContainer.GetData(questIndex, 1);
				if (data != null && data.Type != AchievementType.Hero && GlobalAutoBehaviour<AccountManager>.instance.AchievementManager.Get(questIndex) != null && !AchievementChecker.Completed(questIndex) && !AchievementChecker.Cleared(questIndex) && SingletonAuto<AchievementChecker>.instance.Available(questIndex))
				{
					MissionComponentData item = new MissionComponentData(questIndex, item3);
					listElementData.Add(item);
				}
			}
			else if (questType.CompareTo("SubQuest") == 0)
			{
				SubQuestData subQuestData = GameRoot.Instance.SubQuestDataContainer.GetSubQuestData(questIndex);
				if (GlobalAutoBehaviour<AccountManager>.instance.QuestManager.GetSubQuestStatus(subQuestData) == QuesetStatusType.Progress)
				{
					MissionComponentData item2 = new MissionComponentData(subQuestData, item3);
					listElementData.Add(item2);
				}
			}
		}
		if (MissionManagement.IsNewMissionMain())
		{
			UIHelper.EnableGameObject(SpriteReward, enable: false);
			UIHelper.EnableGameObject(RewardEffectTransform, enable: false);
			UIHelper.EnableGameObject(SpriteNew, enable: true);
		}
		else
		{
			bool enable = MissionManagement.IsMissionMainRewardCheck();
			SetupStatusReward(enable);
		}
	}

	private void SetupMission(bool forceChange = false)
	{
		if (listElementData == null)
		{
			return;
		}
		if (__Extension_Method_ICollection__.COUNT(listElementData) == 0)
		{
			TopData = null;
			BottomData = null;
		}
		else if (__Extension_Method_ICollection__.COUNT(listElementData) == 1)
		{
			TopData = listElementData[0];
			BottomData = null;
		}
		else
		{
			listElementData.Sort(delegate(MissionComponentData a, MissionComponentData b)
			{
				if (a.VisualData == null || b.VisualData == null)
				{
					if (a.Index > b.Index)
					{
						return 1;
					}
					if (a.Index < b.Index)
					{
						return -1;
					}
					return 0;
				}
				if (a.VisualData.MissionGradeValue > b.VisualData.MissionGradeValue)
				{
					return -1;
				}
				if (a.VisualData.MissionGradeValue < b.VisualData.MissionGradeValue)
				{
					return 1;
				}
				if (a.VisualData.QuestIndex > b.VisualData.QuestIndex)
				{
					return 1;
				}
				return (a.VisualData.QuestIndex < b.VisualData.QuestIndex) ? (-1) : 0;
			});
			TopData = listElementData[0];
			BottomData = listElementData[1];
		}
		UIHelper.EnableGameObject(MissionListTransform, __Extension_Method_ICollection__.COUNT(listElementData) > 0);
		UIHelper.SetLabelText(LabelShowMission01, (TopData == null) ? "" : TopData.Title);
		UIHelper.SetLabelText(LabelShowMission02, (BottomData == null) ? "" : BottomData.Title);
	}

	private void SetupStatusReward(bool enable)
	{
		if (MissionButtonAnimator != null && enable)
		{
			MissionButtonAnimator.Play("Anim_Button_Quest_Reward");
		}
		UIHelper.EnableGameObject(SpriteReward, enable);
		UIHelper.EnableGameObject(RewardEffectTransform, enable);
		UIHelper.EnableGameObject(SpriteNew, enable: false);
	}

	private IEnumerator FinishedAnimator()
	{
		while (MissionClearAnimator01.GetCurrentAnimatorStateInfo(0).IsName("MissionClear_Animation"))
		{
			yield return null;
		}
		while (MissionClearAnimator02.GetCurrentAnimatorStateInfo(0).IsName("MissionClear_Animation"))
		{
			yield return null;
		}
		LoadMissionData();
		SetupMission();
	}

	public void OnChanged(SubQuestInfo changedInfo)
	{
		bool flag = false;
		if (TopData != null && TopData.GetData<SubQuestData>() != null)
		{
			SubQuestData data = TopData.GetData<SubQuestData>();
			QuesetStatusType subQuestStatus = GlobalAutoBehaviour<AccountManager>.instance.QuestManager.GetSubQuestStatus(data);
			if (subQuestStatus == QuesetStatusType.Reward || subQuestStatus == QuesetStatusType.Complete)
			{
				MissionClearAnimator01.Play("");
				flag = true;
			}
		}
		if (BottomData != null && BottomData.GetData<SubQuestData>() != null)
		{
			SubQuestData data2 = BottomData.GetData<SubQuestData>();
			QuesetStatusType subQuestStatus2 = GlobalAutoBehaviour<AccountManager>.instance.QuestManager.GetSubQuestStatus(data2);
			if (subQuestStatus2 == QuesetStatusType.Reward || subQuestStatus2 == QuesetStatusType.Complete)
			{
				MissionClearAnimator02.Play("");
				flag = true;
			}
		}
		if (flag)
		{
			if (coAnimator != null)
			{
				VespaCoroutine.Stop(this, coAnimator);
			}
			coAnimator = VespaCoroutine.StartThis(this, FinishedAnimator());
		}
	}

	public void OnChanged(NGame2.NAccount.UserAchievementManager.ChangeAchievementInfo changedInfo)
	{
		bool flag = false;
		if (TopData != null && TopData.GetData<AchievementData>() != null && (AchievementChecker.Completed(TopData.Index) || AchievementChecker.Cleared(TopData.Index)))
		{
			MissionClearAnimator01.Play("MissionClear_Animation");
			flag = true;
		}
		if (BottomData != null && BottomData.GetData<AchievementData>() != null && (AchievementChecker.Completed(BottomData.Index) || AchievementChecker.Cleared(BottomData.Index)))
		{
			MissionClearAnimator01.Play("MissionClear_Animation");
			flag = true;
		}
		if (flag)
		{
			if (coAnimator != null)
			{
				VespaCoroutine.Stop(this, coAnimator);
			}
			coAnimator = VespaCoroutine.StartThis(this, FinishedAnimator());
		}
	}

	public void OnClickFirst()
	{
		if (iListner != null)
		{
			iListner.OnClickMission(TopData);
		}
	}

	public void OnClickSecond()
	{
		if (iListner != null)
		{
			iListner.OnClickMission(BottomData);
		}
	}

	public void OnClick()
	{
		if (iListner != null)
		{
			iListner.OnClick();
		}
	}
}
