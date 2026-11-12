using NGame2.NContentsInfo;
using NGame2.NUI;
using NGame2.NUI.NManager.Mission;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.Mission;
using NShared;
using NVespa;
using UnityEngine;

public sealed class SprkMissionClaimAll : MonoBehaviour
{
	private MissionNodeWindowTopView2 view;

	private CommonNodeWindow owner;

	private float nextRefresh;

	public static EMissionTopViewButtonGroup ButtonGroup(EMissionTopViewButtonGroup requested)
	{
		switch (requested)
		{
		case EMissionTopViewButtonGroup.None:
			return EMissionTopViewButtonGroup.GetAllRewardOnly;
		case EMissionTopViewButtonGroup.GetRewardOnly:
		case EMissionTopViewButtonGroup.MultipleSteps:
			return EMissionTopViewButtonGroup.Basic;
		default:
			return requested;
		}
	}

	public static void Attach(MissionNodeWindowTopView2 view, MissionNodeWindowTopView2.IListener listener)
	{
		SprkMissionClaimAll sprkMissionClaimAll = view.GetComponent<SprkMissionClaimAll>();
		if (sprkMissionClaimAll == null)
		{
			sprkMissionClaimAll = view.gameObject.AddComponent<SprkMissionClaimAll>();
		}
		sprkMissionClaimAll.view = view;
		sprkMissionClaimAll.owner = listener as CommonNodeWindow;
		sprkMissionClaimAll.nextRefresh = 0f;
	}

	private void LateUpdate()
	{
		if (view == null || owner == null || !owner.isActiveAndEnabled || !view.IsOpen() || Time.unscaledTime < nextRefresh)
		{
			return;
		}
		nextRefresh = Time.unscaledTime + 0.15f;
		MissionCategoryData selectedMissionCategoryData = owner.SelectedMissionCategoryData;
		if (selectedMissionCategoryData == null)
		{
			return;
		}
		bool flag = owner.HasAvailableCategoryReward();
		MissionNodeWindow missionNodeWindow = owner as MissionNodeWindow;
		MissionNodeWindowV3 missionNodeWindowV = owner as MissionNodeWindowV3;
		MissionNode2Window missionNode2Window = owner as MissionNode2Window;
		MissionNodeMap missionNodeMap = null;
		if (missionNodeWindow != null)
		{
			missionNodeMap = (missionNodeWindow.NodeMapTop.gameObject.activeInHierarchy ? missionNodeWindow.NodeMapTop : missionNodeWindow.NodeMapTop2);
		}
		if (missionNodeWindowV != null)
		{
			missionNodeMap = (missionNodeWindowV.NodeMapTop.gameObject.activeInHierarchy ? missionNodeWindowV.NodeMapTop : missionNodeWindowV.NodeMapTop2);
		}
		if (missionNodeMap != null)
		{
			flag |= missionNodeMap.GetAchievementRewardList().Count > 0 || missionNodeMap.GetSubQuestRewardList().Count > 0;
		}
		if (missionNode2Window != null && missionNode2Window.Node2Map != null)
		{
			flag |= missionNode2Window.Node2Map.GetAchievementRewardList().Count > 0 || missionNode2Window.Node2Map.GetSubQuestRewardList().Count > 0;
		}
		flag &= MissionManagement.IsOpenSubCategory(selectedMissionCategoryData);
		flag &= !SingletonAuto<MissionStatus>.instance.IsMissionTimeout(selectedMissionCategoryData);
		MissionNodeWindowTopView2.TopViewRewardButtonGroup[] rewardButtonGroupList = view.RewardButtonGroupList;
		foreach (MissionNodeWindowTopView2.TopViewRewardButtonGroup topViewRewardButtonGroup in rewardButtonGroupList)
		{
			if (topViewRewardButtonGroup != null && !(topViewRewardButtonGroup.Root == null) && topViewRewardButtonGroup.Root.gameObject.activeInHierarchy && !(topViewRewardButtonGroup.GetAllRewardButton == null))
			{
				topViewRewardButtonGroup.SetGetAllRewardButton(allGet: true);
				UIHelper.EnableButton(topViewRewardButtonGroup.GetAllRewardButton, flag, grayscaleIfDisable: true);
			}
		}
	}
}
