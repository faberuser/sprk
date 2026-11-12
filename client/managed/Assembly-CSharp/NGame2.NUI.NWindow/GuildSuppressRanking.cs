using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NContents;
using NGame2.NGuild;
using NGame2.NTutorial;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NHelper.NGuild;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGameAsset;
using NShared;
using NShared.AutoApplyGuildSuppress;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class GuildSuppressRanking : LobbyFullWindow, GuildSuppressRankerComponent.Listener
{
	public interface Listener
	{
		void GetGuildRankerList(Action<GuildSuppressGroupRankerInfo[]> onList);

		void GetGuildMemberRankerList(Action<GuildSuppressMemberScoreInfo[]> onList);

		void OnClickRankingRewardPopup();

		void OnClickInfoPopup();

		void OnClickApplyGuildSuppress();

		void OnClickReadyGuildSuppress();
	}

	private Listener _listener;

	public UITexture GuildLogoTexture;

	public UITexture GuildBackTexture;

	public UITexture GuildCountryFlagTexture;

	public UITexture MyRankerHeroTexture;

	public UILabel Label_MemberName;

	public UILabel Label_GuildName;

	public UILabel ScoreLabel;

	public UIToggle GuildRankToggle;

	public UIToggle GuildMemberRankToggle;

	public Transform EmptyRankerRoot;

	public UITexture BossImageTexture;

	public UILabel BossNameLabel;

	public Transform RemainTimeOpenedRoot;

	public UILabel RemainTimeLabel;

	public UILabel RemainTimeLabel2;

	public UILabel Label_DebugText;

	public UIButton RewardButton;

	public UIButton GuideButton;

	public UIButton ApplyButton;

	public UIButton EnterButton;

	public Transform UnopenedButton;

	public Transform ApplySuccessButton;

	public UIBaseGrid RankerGrid;

	public GameObject AutoApplySuppress;

	public bool CompleteApply = true;

	public bool CompleteChangeSession = true;

	private List<GuildSuppressRankerComponent> _rankerComponentList = new List<GuildSuppressRankerComponent>();

	private GuildSuppressRankingType _guildSuppressRankingType;

	private GuildSuppressChapterData _guildSuppressChapterData;

	private float _lastRefreshInfoTime;

	private float _serverSyncTime = 5f;

	public const string PrefabPath = "Resources/Prefab2/Window/GuildSuppress/GuildSuppressRanking";

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		RefreshInfo();
	}

	private void RefreshInfo()
	{
		if (SingletonUpdater<TutorialManager>.instance.IsProgressing || Time.time - _lastRefreshInfoTime < 1f)
		{
			return;
		}
		_lastRefreshInfoTime = Time.time;
		if (GetRemainTime() == 0)
		{
			InitRemainTimeLabel();
			if (_serverSyncTime >= 0f)
			{
				_serverSyncTime -= 1f;
				return;
			}
			_serverSyncTime = 5f;
			if (CompleteChangeSession)
			{
				CompleteChangeSession = false;
				SceneAutoBehaviour<GuildSuppressManagement>.instance.InternalRequestGuildSuppressSessionInfo(delegate
				{
					Init();
					CompleteChangeSession = true;
				});
			}
		}
		else
		{
			InitRemainTimeLabel();
			SetButtonState();
		}
	}

	public override void Reset()
	{
		base.Reset();
		ClearRanker();
	}

	private void ClearRanker()
	{
		FactoryBase.ReleaseAndClearList(_rankerComponentList);
	}

	private int GetRemainTime()
	{
		GuildSuppressSessionData guildSuppressSessionData = SceneAutoBehaviour<GuildSuppressManagement>.instance.GuildSuppressSessionData;
		GuildSuppressSessionInfo guildSuppressSessionInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildSuppressManager.GetGuildSuppressSessionInfo();
		return GuildSuppressHelper.GetRemainSeconds(guildSuppressSessionData, guildSuppressSessionInfo);
	}

	private bool IsContentStatusOn()
	{
		ContentsStatus contentsStatus = SingletonAuto<ContentsStatusManager>.instance.GetContentsStatus(ContentsStatusType.GuildSuppress);
		if (contentsStatus == ContentsStatus.Disable || contentsStatus == ContentsStatus.Hide)
		{
			string text = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string stringDirect = LocalizationManager.GetStringDirect("COMMON_CONTENT_NOT_ENTER");
			MessagePopup.Open(text, stringDirect);
			return false;
		}
		return true;
	}

	public void Open(Listener listener, int raidIndex, Action callback = null)
	{
		_listener = listener;
		_guildSuppressRankingType = GuildSuppressRankingType.GuildRank;
		_guildSuppressChapterData = GameRoot.Instance.GuildSuppressChapterDataContainer.GetData(raidIndex);
		Open(delegate
		{
			Init();
			Vespa_System_Action_Extension_Method.fire(callback);
		});
	}

	private void Init()
	{
		InitToggleButton();
		SetMyRankUi();
		RefreshRanker();
		InitBossDesc();
		InitRemainTimeLabel();
		InitAutoApply();
		SetButtonState();
		SetDebugLabel();
	}

	private void InitToggleButton()
	{
		GuildRankToggle.Set(state: true);
		GuildMemberRankToggle.Set(state: false);
	}

	private void SetMyRankUi()
	{
		int num = 1;
		string text = "VESPA";
		if (GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager == null || GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info == null)
		{
			return;
		}
		byte logo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Logo;
		int num2 = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Back;
		if (num2 <= 0 || num2 > GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxGuildBG"))
		{
			num2 = 1;
		}
		num = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AvatarHeroIndex;
		text = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.CountryCode;
		AssetTexture guildLogoIcon = UIHelper.GetGuildLogoIcon2(logo);
		UIHelper.ResetPlaneAssetTextureAsync(this, GuildLogoTexture, guildLogoIcon);
		AssetTexture guildBackGround2D = UIHelper.GetGuildBackGround2D(num2);
		UIHelper.ResetPlaneAssetTextureAsync(this, GuildBackTexture, guildBackGround2D);
		UIHelper.EnableGameObject(Label_MemberName, _guildSuppressRankingType == GuildSuppressRankingType.GuildMemberRank);
		UIHelper.EnableGameObject(Label_GuildName, _guildSuppressRankingType == GuildSuppressRankingType.GuildRank);
		if (_guildSuppressRankingType == GuildSuppressRankingType.GuildRank)
		{
			CountryCodeData dataByCode = GameRoot.Instance.CountryCodeDataContainer.GetDataByCode(text);
			if (dataByCode != null)
			{
				AssetTexture plainTexture = UIHelper.GetPlainTexture(dataByCode.IconPath);
				UIHelper.ResetPlaneAssetTextureAsync(this, GuildCountryFlagTexture, plainTexture);
			}
		}
		else
		{
			UIHelper.ClearPlaneAssetTexture(GuildCountryFlagTexture);
		}
		AssetTexture plainTexture2 = UIHelper.GetPlainTexture(UIAvatarHelper.GetBattlePortraitPath(num));
		UIHelper.EnableGameObject(MyRankerHeroTexture, enable: true);
		UIHelper.ResetPlaneAssetTextureAsync(this, MyRankerHeroTexture, plainTexture2);
	}

	private void RefreshRanker()
	{
		if (_listener != null)
		{
			if (_guildSuppressRankingType == GuildSuppressRankingType.GuildRank)
			{
				_listener.GetGuildRankerList(RefreshGuildRanker);
			}
			else
			{
				_listener.GetGuildMemberRankerList(RefreshMemberRanker);
			}
		}
	}

	private void RefreshGuildRanker(GuildSuppressGroupRankerInfo[] rankerInfos)
	{
		ClearRanker();
		if (rankerInfos == null || rankerInfos.Count() == 0)
		{
			UIHelper.SetLabelText(Label_GuildName, GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Name);
			UIHelper.SetLabelText(ScoreLabel, "0");
			UIHelper.EnableGameObject(EmptyRankerRoot, enable: false);
			return;
		}
		string countryServerName = GlobalAutoBehaviour<AccountManager>.instance.CountryServerName;
		foreach (GuildSuppressGroupRankerInfo guildSuppressGroupRankerInfo in rankerInfos)
		{
			GuildSuppressRankerComponent guildSuppressRankerComponent = UIHelper.AddGridItem(RankerGrid, GuildSuppressRankerComponent.PrefabAuto);
			if (guildSuppressRankerComponent == null || !guildSuppressRankerComponent.SetGuildRankingData(guildSuppressGroupRankerInfo, this))
			{
				FactoryBase.ReleaseComponent(guildSuppressRankerComponent);
				continue;
			}
			_rankerComponentList.Add(guildSuppressRankerComponent);
			if (guildSuppressGroupRankerInfo.GuildId == GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Id && guildSuppressGroupRankerInfo.ServerGroup.Equals(countryServerName))
			{
				UIHelper.SetLabelText(Label_GuildName, guildSuppressGroupRankerInfo.GuildName);
				UIHelper.SetLabelText(ScoreLabel, Vespa_Long_Extension_Method.ToCommaString(guildSuppressGroupRankerInfo.Score));
				string countryCode = guildSuppressGroupRankerInfo.CountryCode;
				CountryCodeData dataByCode = GameRoot.Instance.CountryCodeDataContainer.GetDataByCode(countryCode);
				if (dataByCode != null)
				{
					AssetTexture plainTexture = UIHelper.GetPlainTexture(dataByCode.IconPath);
					UIHelper.ResetPlaneAssetTextureAsync(this, GuildCountryFlagTexture, plainTexture);
				}
			}
		}
		UIHelper.RepositionGrid(RankerGrid);
	}

	private void RefreshMemberRanker(GuildSuppressMemberScoreInfo[] rankerInfos)
	{
		ClearRanker();
		if (rankerInfos == null)
		{
			UIHelper.EnableGameObject(EmptyRankerRoot, enable: false);
			return;
		}
		List<GuildSuppressMemberScoreInfo> list = new List<GuildSuppressMemberScoreInfo>();
		if (rankerInfos.Count() > 0)
		{
			list.AddRange(rankerInfos);
			list.Sort((GuildSuppressMemberScoreInfo l, GuildSuppressMemberScoreInfo r) => r.Score.CompareTo(l.Score));
		}
		int num = 0;
		foreach (GuildSuppressMemberScoreInfo item in list)
		{
			GuildSuppressRankerComponent guildSuppressRankerComponent = UIHelper.AddGridItem(RankerGrid, GuildSuppressRankerComponent.PrefabAuto);
			if (!guildSuppressRankerComponent.SetGuildMemberRankingData(item, num, this))
			{
				FactoryBase.ReleaseComponent(guildSuppressRankerComponent);
				continue;
			}
			num++;
			_rankerComponentList.Add(guildSuppressRankerComponent);
			if (item.AccountId == GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId)
			{
				UIHelper.SetLabelText(Label_MemberName, GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Nick);
				UIHelper.SetLabelText(ScoreLabel, Vespa_Long_Extension_Method.ToCommaString(item.Score));
			}
		}
		UIHelper.RepositionGrid(RankerGrid);
		UIHelper.EnableGameObject(EmptyRankerRoot, enable: false);
	}

	private void InitBossDesc()
	{
		if (_guildSuppressChapterData != null)
		{
			string bossInfoImage = _guildSuppressChapterData.BossInfoImage;
			if (!String_Extension_Method.IsNullOrEmpty(bossInfoImage))
			{
				AssetTexture assetTexture = new AssetTexture();
				assetTexture.Open(bossInfoImage);
				UIHelper.ResetPlaneAssetTextureAsync(this, BossImageTexture, assetTexture);
			}
			else
			{
				VespaDebug.LogError("bossinfoimage is null");
			}
			UIHelper.SetLabelText(BossNameLabel, LocalizationManager.GetStringDirect(_guildSuppressChapterData.Name));
		}
	}

	private void InitRemainTimeLabel()
	{
		GuildSuppressSessionInfo guildSuppressSessionInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildSuppressManager.GetGuildSuppressSessionInfo();
		if (guildSuppressSessionInfo != null)
		{
			GuildSuppressSessionState state = guildSuppressSessionInfo.State;
			int remainTime = GetRemainTime();
			long num = remainTime;
			int num3;
			if (state == GuildSuppressSessionState.Apply || state == GuildSuppressSessionState.ApplyEnded || state == GuildSuppressSessionState.Battle)
			{
				int num2 = 0;
				num3 = ((num > num2) ? 1 : 0);
			}
			else
			{
				num3 = 0;
			}
			bool flag = (byte)num3 != 0;
			UIHelper.EnableGameObject(RemainTimeOpenedRoot, flag);
			if (flag)
			{
				UIHelper.EnableGameObject(RemainTimeLabel, enable: true);
				UIHelper.EnableGameObject(RemainTimeLabel2, enable: false);
				string text = "";
				UIHelper.SetLabelText(RemainTimeLabel, LocalizationManager.GetStringDirect(state switch
				{
					GuildSuppressSessionState.Apply => "GUILD_SUPPRESS_REGISTER_REMAIN_DAY_HOUR", 
					GuildSuppressSessionState.ApplyEnded => "GUILD_SUPPRESS_START_REMAIN_DAY_HOUR", 
					GuildSuppressSessionState.Battle => "GUILD_SUPPRESS_END_REMAIN_DAY_HOUR", 
					_ => "GUILD_SUPPRESS_STATUS_BOARD_REFRESH_TIMER", 
				}, UIHelper.GetRemainTimeLabelText(num)));
			}
		}
	}

	public void SetButtonState()
	{
		GuildSuppressSessionInfo guildSuppressSessionInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildSuppressManager.GetGuildSuppressSessionInfo();
		if (guildSuppressSessionInfo != null)
		{
			GuildSuppressSessionState state = guildSuppressSessionInfo.State;
			bool applied = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildSuppressManager.Applied;
			switch (state)
			{
			case GuildSuppressSessionState.Apply:
				UIHelper.EnableGameObject(ApplyButton, !applied);
				UIHelper.EnableGameObject(ApplySuccessButton, applied);
				UIHelper.EnableGameObject(EnterButton, enable: false);
				UIHelper.EnableGameObject(UnopenedButton, enable: false);
				break;
			case GuildSuppressSessionState.ApplyEnded:
				UIHelper.EnableGameObject(ApplyButton, enable: false);
				UIHelper.EnableGameObject(ApplySuccessButton, enable: false);
				UIHelper.EnableGameObject(EnterButton, enable: true);
				UIHelper.EnableGameObject(UnopenedButton, enable: false);
				break;
			case GuildSuppressSessionState.NotHeld:
				UIHelper.EnableGameObject(ApplyButton, enable: false);
				UIHelper.EnableGameObject(ApplySuccessButton, enable: false);
				UIHelper.EnableGameObject(EnterButton, enable: false);
				UIHelper.EnableGameObject(UnopenedButton, enable: true);
				break;
			case GuildSuppressSessionState.Battle:
				UIHelper.EnableGameObject(ApplyButton, enable: false);
				UIHelper.EnableGameObject(ApplySuccessButton, enable: false);
				UIHelper.EnableGameObject(EnterButton, enable: true);
				UIHelper.EnableGameObject(UnopenedButton, enable: false);
				break;
			case GuildSuppressSessionState.Ended:
				UIHelper.EnableGameObject(ApplyButton, enable: false);
				UIHelper.EnableGameObject(ApplySuccessButton, enable: false);
				UIHelper.EnableGameObject(EnterButton, enable: false);
				UIHelper.EnableGameObject(UnopenedButton, enable: true);
				break;
			}
			if (ApplyButton.isActiveAndEnabled)
			{
				GuildMemberInfo myMemberInfo = GlobalAutoBehaviour<GuildManager>.instance.MemberManager.GetMyMemberInfo();
				bool enable = myMemberInfo != null && (myMemberInfo.Rank == 1 || myMemberInfo.Rank == 2);
				UIHelper.EnableButton(ApplyButton, enable);
			}
			if (EnterButton.isActiveAndEnabled)
			{
				UIHelper.EnableButton(EnterButton, state == GuildSuppressSessionState.Battle && applied);
			}
		}
	}

	public void InitAutoApply()
	{
		bool autoApplySuppress = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildSuppressManager.AutoApplySuppress;
		UIHelper.EnableGameObject(AutoApplySuppress, autoApplySuppress);
	}

	public void OnGuildRankingToggleChanged()
	{
		if ((bool)GuildRankToggle && GuildRankToggle.value)
		{
			_guildSuppressRankingType = GuildSuppressRankingType.GuildRank;
			SetMyRankUi();
			RefreshRanker();
		}
	}

	public void OnMemberRankingToggleChanged()
	{
		if ((bool)GuildMemberRankToggle && GuildMemberRankToggle.value)
		{
			_guildSuppressRankingType = GuildSuppressRankingType.GuildMemberRank;
			SetMyRankUi();
			RefreshRanker();
		}
	}

	public void OnClickRankingRewardPopup()
	{
		if (_listener != null)
		{
			_listener.OnClickRankingRewardPopup();
		}
	}

	public void OnClickSuppressInfo()
	{
		if (_listener != null)
		{
			_listener.OnClickInfoPopup();
		}
	}

	public void OnClickApply()
	{
		if (!IsTutorialProgressing() && IsContentStatusOn() && CompleteApply)
		{
			CompleteApply = false;
			if (_listener != null)
			{
				_listener.OnClickApplyGuildSuppress();
			}
		}
	}

	public void OnClickReady()
	{
		if (!IsTutorialProgressing() && IsContentStatusOn() && _listener != null)
		{
			_listener.OnClickReadyGuildSuppress();
		}
	}

	public void OnClickAutoApply(GameObject senderObject)
	{
		GuildMemberInfo myMemberInfo = GlobalAutoBehaviour<GuildManager>.instance.MemberManager.GetMyMemberInfo();
		if (myMemberInfo == null || myMemberInfo.Rank != 1)
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("GUILD_SUPPRESS_AUTO_PARTICIPATION_FAIL"));
			return;
		}
		if (UIHelper.IsEnabledGameObjet(senderObject))
		{
			RequestAutoApply(isApply: false, delegate
			{
				UIHelper.ToggleGameObject(senderObject);
			});
			return;
		}
		string text = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string stringDirect = LocalizationManager.GetStringDirect("GUILD_SUPPRESS_AUTO_PARTICIPATION_NOTICE");
		MessagePopup.Open(text, stringDirect, delegate
		{
			RequestAutoApply(isApply: true, delegate
			{
				UIHelper.ToggleGameObject(senderObject);
				GuildSuppressSessionInfo guildSuppressSessionInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildSuppressManager.GetGuildSuppressSessionInfo();
				if (guildSuppressSessionInfo != null && guildSuppressSessionInfo.State == GuildSuppressSessionState.Apply && !GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildSuppressManager.Applied)
				{
					OnClickApply();
				}
			});
		}, null);
	}

	private void RequestAutoApply(bool isApply, Action callback)
	{
		NShared.AutoApplyGuildSuppress.Request request = new NShared.AutoApplyGuildSuppress.Request
		{
			IsApply = isApply
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.AutoApplyGuildSuppress.Response response)
		{
			if (response.Result != ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else
			{
				GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildSuppressManager.AutoApplySuppress = ((response.AutoApplySuppress != 0) ? true : false);
				Vespa_System_Action_Extension_Method.fire(callback);
			}
		});
	}

	void GuildSuppressRankerComponent.Listener.OnClickComponent(long accountId)
	{
		NGame2.NGuild.GuildMemberManager memberManager = GlobalAutoBehaviour<GuildManager>.instance.MemberManager;
		if (memberManager != null)
		{
			GuildMemberInfo guildMemberInfo = memberManager.Get(accountId);
			if (guildMemberInfo != null && guildMemberInfo.AccountId != GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId)
			{
				InspectUserManagement.InspectUserData inspectUser = new InspectUserManagement.InspectUserData(guildMemberInfo, InspectUserCategory.WorldBoss);
				SceneAutoBehaviour<InspectUserManagement>.instance.OpenMenuPopup(inspectUser);
			}
		}
	}

	public void Tutorial_InitUi()
	{
		UIHelper.EnableGameObject(RemainTimeOpenedRoot, enable: true);
		UIHelper.EnableGameObject(RemainTimeLabel, enable: true);
		UIHelper.SetLabelText(RemainTimeLabel, "");
		UIHelper.EnableGameObject(ApplyButton, enable: true);
		UIHelper.EnableGameObject(ApplySuccessButton, enable: false);
		UIHelper.EnableGameObject(EnterButton, enable: false);
		UIHelper.EnableGameObject(UnopenedButton, enable: false);
		UIHelper.EnableGameObject(Label_MemberName, enable: false);
		UIHelper.EnableGameObject(Label_GuildName, enable: true);
		UIHelper.SetLabelText(Label_GuildName, GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Name);
		UIHelper.SetLabelText(ScoreLabel, "0");
		UIHelper.EnableButton(ApplyButton, enable: true);
	}

	public void Tutorial_ChangeUiButton()
	{
		UIHelper.EnableGameObject(ApplyButton, enable: false);
		UIHelper.EnableGameObject(ApplySuccessButton, enable: false);
		UIHelper.EnableGameObject(EnterButton, enable: true);
		UIHelper.EnableGameObject(UnopenedButton, enable: false);
	}

	private bool IsTutorialProgressing()
	{
		if (SingletonUpdater<TutorialManager>.isValidInstance)
		{
			if (SingletonUpdater<TutorialManager>.instance.IsProgressing)
			{
				return true;
			}
			if (SingletonUpdater<TutorialManager>.instance.HasNextTurorial)
			{
				return true;
			}
		}
		return false;
	}

	private void SetDebugLabel()
	{
		GuildSuppressInfo guildSuppressInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildSuppressManager.GetGuildSuppressInfo();
		GuildSuppressSessionInfo guildSuppressSessionInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildSuppressManager.GetGuildSuppressSessionInfo();
		if (guildSuppressInfo != null && guildSuppressSessionInfo != null)
		{
			if (GuiManager.IsShowDebugButton())
			{
				UIHelper.EnableGameObject(Label_DebugText, enable: true);
				UIHelper.SetLabelText(Label_DebugText, $"SI : {guildSuppressSessionInfo.SessionIndex}, GI : {guildSuppressInfo.GroupIndex}");
			}
			else
			{
				UIHelper.EnableGameObject(Label_DebugText, enable: false);
			}
		}
	}
}
