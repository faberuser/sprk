using NGame2.NAccount;
using NGame2.NAchievement;
using NGame2.NUI.NManager.Mission;
using NGame2.NUI.NWindow;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

[RequireComponent(typeof(UIWidget))]
public sealed class MissionSubCategoryComponent : MonoBehaviour
{
	private enum ESubCategoryState
	{
		Normal,
		Disable,
		Selected,
		Lock
	}

	public interface IListener
	{
		void OnClick(MissionSubCategoryComponent comp);
	}

	public const string PrefabPath = "Prefab2/Component/QuestIntegration/MissionSubCategoryComponent";

	public static PrefabAuto<MissionSubCategoryComponent> PrefabAuto = new PrefabAuto<MissionSubCategoryComponent>("Prefab2/Component/QuestIntegration/MissionSubCategoryComponent");

	[Header("[CommonMissionCategory]")]
	public UILabel TitleLabel;

	public UIBundleLoader Loader_Category;

	public UISprite CategorySprite;

	public GameObject DisableObject;

	public GameObject SelectedObject;

	public GameObject LockObject;

	[Header("[MissionSubCategoryComponent]")]
	public UISprite SpriteNew;

	public UISprite SpriteReward;

	public Transform NoticeCountTransform;

	public UILabel NoticeCountLabel;

	private ESubCategoryState _subCategoryState;

	private IListener _listener;

	public MissionCategoryData MissionCategoryData { get; set; }

	public void OpenComponent(MissionCategoryData data, IListener listener)
	{
		_listener = listener;
		MissionCategoryData = data;
		UIHelper.EnableGameObject(base.gameObject, enable: true);
		SetDrawInfos();
	}

	private bool IsOpenOrForceUnlock()
	{
		if (MissionManagement.IsOpenSubCategory(MissionCategoryData))
		{
			return true;
		}
		return ContentsDefine.ReturnUserJumping.IsForceUnlockMission_in_MissionSubCategoryComponent(MissionCategoryData.MainCategoryIndex, MissionCategoryData.SubCategoryIndex);
	}

	private void SetDrawInfos()
	{
		if (MissionCategoryData != null)
		{
			UIHelper.SetLabelText(TitleLabel, LocalizationManager.GetStringDirect(MissionCategoryData.SubCategoryLocalKey));
			SetupSubCategoryImage();
			if (IsOpenOrForceUnlock())
			{
				SetState(ESubCategoryState.Normal);
			}
			else
			{
				SetState(ESubCategoryState.Lock);
			}
			SetupNoticeIcon();
		}
	}

	private void SetupSubCategoryImage()
	{
		if (MissionCategoryData != null && Loader_Category != null)
		{
			UIBundleLoader loader_Category = Loader_Category;
			string text = MissionCategoryData.SubCategoryButtonPath;
			if (!string.IsNullOrEmpty(text) && text.IndexOf("/") < 0)
			{
				text = "UI/Quest/SubCategory/" + text;
			}
			loader_Category.AssetPath = text;
		}
	}

	private void SetState(ESubCategoryState state)
	{
		if (_subCategoryState != state)
		{
			_subCategoryState = state;
			DoState();
		}
	}

	private void DoState()
	{
		switch (_subCategoryState)
		{
		case ESubCategoryState.Normal:
			NormalState();
			break;
		case ESubCategoryState.Disable:
			DisableState();
			break;
		case ESubCategoryState.Selected:
			SelectedState();
			break;
		case ESubCategoryState.Lock:
			LockedState();
			break;
		}
	}

	private void NormalState()
	{
		UIHelper.EnableGameObject(DisableObject, enable: false);
		UIHelper.EnableGameObject(SelectedObject, enable: false);
		UIHelper.EnableGameObject(LockObject, enable: false);
	}

	private void DisableState()
	{
		UIHelper.EnableGameObject(SelectedObject, enable: false);
		UIHelper.EnableGameObject(DisableObject, enable: true);
		UIHelper.EnableGameObject(LockObject, enable: false);
	}

	private void SelectedState()
	{
		UIHelper.EnableGameObject(DisableObject, enable: false);
		UIHelper.EnableGameObject(SelectedObject, enable: true);
		UIHelper.EnableGameObject(LockObject, enable: false);
	}

	private void LockedState()
	{
		UIHelper.EnableGameObject(SelectedObject, enable: false);
		UIHelper.EnableGameObject(DisableObject, enable: true);
		UIHelper.EnableGameObject(LockObject, enable: true);
	}

	public void Refresh()
	{
		SetDrawInfos();
	}

	public void OnClick()
	{
		if (_subCategoryState == ESubCategoryState.Lock)
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect(MissionCategoryData.OpenConditionLocalKey));
			return;
		}
		MissionManagement.CheckNewMark(MissionCategoryData.MainCategoryIndex, MissionCategoryData.SubCategoryIndex);
		if (_listener != null)
		{
			_listener.OnClick(this);
		}
	}

	public void Select()
	{
		if (IsOpenOrForceUnlock())
		{
			MissionManagement.CheckNewMark(MissionCategoryData.MainCategoryIndex, MissionCategoryData.SubCategoryIndex);
			SetupNoticeIcon();
			SetState(ESubCategoryState.Selected);
		}
	}

	private void SetupNoticeIcon()
	{
		if (MissionCategoryData == null)
		{
			return;
		}
		int mainCategoryIndex = MissionCategoryData.MainCategoryIndex;
		int subCategoryIndex = MissionCategoryData.SubCategoryIndex;
		UIHelper.EnableGameObject(SpriteNew, enable: false);
		UIHelper.EnableGameObject(SpriteReward, enable: false);
		UIHelper.EnableGameObject(NoticeCountTransform, enable: false);
		if (_subCategoryState == ESubCategoryState.Lock)
		{
			return;
		}
		if (MissionManagement.IsNewSubCategory(mainCategoryIndex, subCategoryIndex))
		{
			UIHelper.EnableGameObject(SpriteNew, enable: true);
			UIHelper.EnableGameObject(SpriteReward, enable: false);
			UIHelper.EnableGameObject(NoticeCountTransform, enable: false);
			return;
		}
		bool flag = false;
		int num = 0;
		foreach (MissionVisualData missionVisualData in GameRoot.Instance.MissionVisualDataContainer.GetMissionVisualDataList(mainCategoryIndex, subCategoryIndex))
		{
			string questType = missionVisualData.QuestType;
			int questIndex = missionVisualData.QuestIndex;
			if (String_Extension_Method.IsNullOrEmpty(questType) || questIndex == 0)
			{
				continue;
			}
			if (questType.CompareTo("Achievement") == 0)
			{
				bool flag2 = AchievementChecker.Completed(questIndex);
				if (!flag2)
				{
					num++;
				}
				flag |= !flag2 && AchievementChecker.Cleared(questIndex);
			}
			else if (questType.CompareTo("SubQuest") == 0)
			{
				if (!GlobalAutoBehaviour<AccountManager>.instance.QuestManager.IsGetCompleteSubQuest(questIndex))
				{
					num++;
				}
				flag |= GlobalAutoBehaviour<AccountManager>.instance.QuestManager.IsGetRewardableSubQuest(questIndex);
			}
		}
		if (!flag)
		{
			string rewardQuestType = MissionCategoryData.RewardQuestType;
			int rewardQuestIndex = MissionCategoryData.RewardQuestIndex;
			if (!String_Extension_Method.IsNullOrEmpty(rewardQuestType) && rewardQuestIndex != 0)
			{
				if (rewardQuestType.CompareTo("Achievement") == 0)
				{
					bool flag3 = AchievementChecker.Completed(rewardQuestIndex);
					flag |= !flag3 && AchievementChecker.Cleared(rewardQuestIndex);
				}
				else if (rewardQuestType.CompareTo("SubQuest") == 0)
				{
					GlobalAutoBehaviour<AccountManager>.instance.QuestManager.IsGetCompleteSubQuest(rewardQuestIndex);
					flag |= GlobalAutoBehaviour<AccountManager>.instance.QuestManager.IsGetRewardableSubQuest(rewardQuestIndex);
				}
			}
		}
		if (flag)
		{
			UIHelper.EnableGameObject(SpriteNew, enable: false);
			UIHelper.EnableGameObject(SpriteReward, enable: true);
			UIHelper.EnableGameObject(NoticeCountTransform, enable: false);
		}
		else
		{
			UIHelper.EnableGameObject(SpriteNew, enable: false);
			UIHelper.EnableGameObject(SpriteReward, enable: false);
			UIHelper.EnableGameObject(NoticeCountTransform, num > 0);
			UIHelper.SetLabelText(NoticeCountLabel, num);
		}
	}
}
