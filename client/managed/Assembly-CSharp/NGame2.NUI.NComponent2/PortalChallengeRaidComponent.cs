using System;
using System.Collections;
using NGame2.NAccount;
using NGame2.NUI.NHelper.NPortal;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGameAsset;
using NShared;
using NShared.NRaid;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class PortalChallengeRaidComponent : PortalContentsComponent
{
	public const string PrefabPath = "Resources/Prefab2/Window/Portal/PortalRenewalComponents/PortalChallengeraidComponent";

	public static PrefabAuto<PortalChallengeRaidComponent> PrefabAuto = new PrefabAuto<PortalChallengeRaidComponent>("Resources/Prefab2/Window/Portal/PortalRenewalComponents/PortalChallengeraidComponent");

	[Header("[PortalChallengeRaidComponent]")]
	public UIScrollView DescScrollView;

	public UILabel ContentsSubTitleLabel;

	public UILabel ContentsSeasonTypeLabel;

	public UILabel ContentsConditionLabel;

	[Header("[ChallengeRaid Lock Group]")]
	public Transform LockedRoot;

	public string DisableRaidBackgroundName = "Portal_ChallengeRaid_NotProgress_Image";

	private string BackgroundCode
	{
		get
		{
			ChallengeRaidInfo challengeRaidSeasonInfo = SceneAutoBehaviour<RaidManagement>.instance.GetChallengeRaidSeasonInfo();
			if (challengeRaidSeasonInfo == null)
			{
				return DisableRaidBackgroundName;
			}
			ChallengeRaidData data = GameRoot.Instance.ChallengeRaidDataContainer.GetData(challengeRaidSeasonInfo.ChallengeRaidIndex);
			if (data == null)
			{
				return DisableRaidBackgroundName;
			}
			return data.PortalImagePath;
		}
	}

	protected override void Init()
	{
		base.Init();
	}

	public override void ResolutionComp(float contentsWidth)
	{
		base.ResolutionComp(contentsWidth);
		VespaCoroutine.Start(coDescLabelBugFix());
	}

	private IEnumerator coDescLabelBugFix()
	{
		UIHelper.EnableGameObject(_basicData.ContentsDescriptionLabel, enable: false);
		yield return null;
		if (!String_Extension_Method.IsNullOrEmpty(base._portalRenewalData.DescLocalKey))
		{
			UIHelper.EnableGameObject(_basicData.ContentsDescriptionLabel, enable: true);
			DescScrollView.ResetPosition();
		}
	}

	private void InitBackGround()
	{
		if (_basicData != null && !(_basicData.ContentsTexture == null))
		{
			AssetTexture newPortalBackground = PortalHelper.GetNewPortalBackground(BackgroundCode);
			UIHelper.ResetPlaneAssetTextureAsync(this, _basicData.ContentsTexture, newPortalBackground);
		}
	}

	private void InitChallengeRaid()
	{
		UIHelper.SetLabelText(ContentsSubTitleLabel, string.Empty);
		ChallengeRaidInfo seasonInfo = SceneAutoBehaviour<RaidManagement>.instance.GetChallengeRaidWarpper().GetSeasonInfo();
		if (seasonInfo == null)
		{
			return;
		}
		RaidData data = GameRoot.Instance.RaidDataContainer.GetData(seasonInfo.RaidIndex, 1);
		if (data != null)
		{
			CampaignWaveData bossWaveData;
			CreatureData bossCreatureData = GameRoot.Instance.CampaignDungeonDataContainer.GetBossCreatureData(data.ChapterIndex, data.DungeonIndex, DungeonDifficultyType.Easy, scenario: false, out bossWaveData);
			if (bossCreatureData != null)
			{
				string visualInfo = bossWaveData?.BossVisualInfo;
				string creatureName = string.Empty;
				string creatureNamePrefix = string.Empty;
				GameRoot.Instance.CreatureVisualDataContainer.GetCreatureName(bossCreatureData, visualInfo, LocalizationManager.GetStringDirect, out creatureName, out creatureNamePrefix);
				UIHelper.SetLabelText(ContentsSubTitleLabel, creatureName);
			}
		}
	}

	protected override void UpdateResetTime()
	{
		SceneAutoBehaviour<RaidManagement>.instance.RefreshChallengeRaidInfo();
		InitBackGround();
		InitChallengeRaid();
		CheckLockedState();
		EnableContentsCondition(isEnable: true);
		UIHelper.EnableGameObject(GetResetTimeGroup(0), enable: true);
		UIHelper.SetLabelText(GetResetTimeLabel(0), LocalizationManager.GetStringDirect("RESULT_TYPE.PORTAL_NOTGOAHEAD_CONTENTS"));
		UIHelper.EnableGameObject(ContentsSeasonTypeLabel, enable: false);
		SceneAutoBehaviour<RaidManagement>.instance.RefreshChallengeRaidInfo();
		ChallengeRaidWrapper challengeRaidWarpper = SceneAutoBehaviour<RaidManagement>.instance.GetChallengeRaidWarpper();
		if (challengeRaidWarpper == null)
		{
			return;
		}
		ChallengeRaidInfo seasonInfo = challengeRaidWarpper.GetSeasonInfo();
		if (seasonInfo == null || GameRoot.Instance.ChallengeRaidDataContainer.GetData(seasonInfo.ChallengeRaidIndex) == null)
		{
			return;
		}
		bool flag = challengeRaidWarpper.IsAbleEnter();
		bool flag2 = challengeRaidWarpper.IsAbleSeason();
		if (flag)
		{
			if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(challengeRaidWarpper.GetEnterInfo().EndTime, out var o_dateTime))
			{
				return;
			}
			DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
			double totalSeconds = (o_dateTime - currentServerUTCTime).TotalSeconds;
			if (totalSeconds < 0.0)
			{
				VespaDebug.LogError($"Portal {base._portalRenewalData.SubCategory} subCategory Time Wrong!!!");
				return;
			}
			DateTime nextResetTime = currentServerUTCTime.AddSeconds(totalSeconds);
			_contentsResetDatas.Add(new ContentsResetData(this, "PORTAL_UI_TEXT_FINISH_TIME", nextResetTime));
			int raidIndex = seasonInfo.RaidIndex;
			int raidMaxLevel = RaidHelper.GetRaidMaxLevel(raidIndex);
			RaidData data = GameRoot.Instance.RaidDataContainer.GetData(raidIndex, raidMaxLevel);
			if (data == null)
			{
				VespaDebug.LogError($"Portal {base._portalRenewalData.SubCategory} raidData is Null");
				UIHelper.EnableGameObject(ContentsSeasonTypeLabel, enable: false);
				return;
			}
			string text = ((data.Type == RaidType.Challenge) ? LocalizationManager.GetStringDirect("CHALLENGE_RAID_SEASON_MULTI") : LocalizationManager.GetStringDirect("CHALLENGE_RAID_SEASON_SINGLE"));
			UIHelper.EnableGameObject(ContentsSeasonTypeLabel, enable: true);
			UIHelper.SetLabelText(ContentsSeasonTypeLabel, text);
		}
		else
		{
			UIHelper.EnableGameObject(GetResetTimeGroup(0), enable: true);
			UIHelper.SetLabelText(GetResetTimeLabel(0), LocalizationManager.GetStringDirect("RESULT_TYPE.PORTAL_NOTGOAHEAD_CONTENTS"));
			UIHelper.EnableGameObject(ContentsSeasonTypeLabel, enable: false);
		}
		EnableContentsCondition(!flag && !flag2);
		PortalRenewal.LockedState lockedState = ((!(flag || flag2)) ? PortalRenewal.LockedState.CONDITION_LOCKED : PortalRenewal.LockedState.NONE);
		ChangeLockState(lockedState);
	}

	private void EnableContentsCondition(bool isEnable)
	{
		UIHelper.EnableGameObject(ContentsConditionLabel, isEnable);
	}
}
