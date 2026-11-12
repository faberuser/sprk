using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NGuild;
using NGame2.NTutorial;
using NGame2.NUI.NHelper.NPortal;
using NGame2.NUI.NManager;
using NGameAsset;
using NNguiExtension;
using NShared;
using NShared.AutoApplyGuildArena;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class GuildArena : LobbyFullWindow, LobbyTopmostMenu.IMiniGuideButtonObserver
{
	private enum LayoutType
	{
		APPLY,
		BATTLE,
		NOTMATCHED
	}

	public interface Listener
	{
		void OnClickApplyGuildArena();

		void OpenGuildArenaSetting();

		void OpenGuildArenaTotalInfo();

		void OpenGuildArenaRankingInfo();

		void OpenGuildArenaMatchList();
	}

	[Serializable]
	public class GuildInfo
	{
		public GameObject Root;

		public GameObject FlagRoot;

		public UIFlagWidget UIFlagWidget;

		public GameObject InfoRoot;

		public UITexture GuildCountryFlagTexture;

		public UILabel Label_GuildName;

		public UILabel Label_Rank;

		public UILabel Label_Record;

		public UILabel Label_Arena_Record;

		public UILabel Label_Member;

		public UISpriteSelector Sprite_TierRank;

		public AvatarRoot AvatarRoot;
	}

	[Serializable]
	public class AvatarRoot
	{
		public UICreatureWidget Left;

		public UICreatureWidget Center;

		public UICreatureWidget Right;
	}

	public GameObject VersusRoot;

	public UIGame3DWidget VSEffect;

	public GuildInfo MyGuildInfo;

	public GuildInfo EnemyGuildInfo;

	public GameObject MyGuild_OriginRoot;

	public GameObject MyGuild_CenterRoot;

	private Listener _listener;

	private GuildArenaSessionState _sessionState = GuildArenaSessionState.Apply;

	private float _lastRefreshInfoTime;

	private float _serverSyncTime = 5f;

	public const string PrefabPath = "Resources/Prefab2/Window/GuildArena/GuildArena";

	public GameObject AutoApplyChecked;

	public UIButton Button_ArenaGuide;

	public UIButton Button_DefenceSetting;

	public UIButton Button_RankInfo;

	public UIButton Button_Apply;

	public UIButton Button_Start;

	public UIButton Button_ApplyComplete;

	public Transform RemainTimeRoot;

	public UILabel RemainTimeLabel;

	public UIGame3DWidget UiEffect_DefenceSettingBtn;

	public const string UI_GuildArena_BGM = "BGM2.Guild.GuildEntryMenu";

	private string _entryMenuBGM = string.Empty;

	private LayoutType _layoutType;

	public GameObject FlagOriginPos;

	public GameObject FlagCenterPos;

	private bool _isLoaded;

	private bool _isMatched = true;

	private bool _playEffect = true;

	private int _applyMemberCount;

	private List<UICreatureWidget> CreatureWidgetList = new List<UICreatureWidget>();

	private GuildArenaRankerInfo _myGuildInfo;

	private GuildArenaRankerInfo _enemyGuildInfo;

	int LobbyTopmostMenu.IMiniGuideButtonObserver.WindowGuideIndex => 0;

	private void Update()
	{
		if (!IsOpeningWindowAnimating())
		{
			int num = 0;
			if (!_isLoaded && CreatureWidgetList.Count > 0)
			{
				foreach (UICreatureWidget creatureWidget in CreatureWidgetList)
				{
					if (creatureWidget != null && !_isLoaded && creatureWidget.RenderFacade.AssetCreatureAsync.ISDONE())
					{
						num++;
					}
				}
				if (num == CreatureWidgetList.Count)
				{
					_isLoaded = true;
					InitLayout();
					if (_layoutType == LayoutType.APPLY || _layoutType == LayoutType.NOTMATCHED)
					{
						FadeInAvatars();
						FadeInLayouts();
					}
					if (_layoutType == LayoutType.BATTLE)
					{
						TweenPositionLayouts();
						VespaCoroutine.Start(TweenPositionAvatarts());
					}
					VespaCoroutine.Start(coPlayGuildFlagWidgets());
					VespaCoroutine.Start(PlayVSEffect());
				}
			}
		}
		RefreshArenaInfo();
	}

	private IEnumerator PlayVSEffect()
	{
		yield return new WaitForSeconds(0.3f);
		if (VersusRoot.gameObject.activeSelf)
		{
			VSEffect.Play(null);
			if (_playEffect)
			{
				SoundStock stock = new SoundStock
				{
					Code = "UI2.Guild.GuildVSEffect"
				};
				SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 2f);
			}
		}
	}

	private void RefreshArenaInfo()
	{
		if (!(Time.time - _lastRefreshInfoTime < 1f))
		{
			_lastRefreshInfoTime = Time.time;
			InitRemainTimeInfo();
		}
	}

	public void FadeInAvatars()
	{
		foreach (UICreatureWidget creatureWidget in CreatureWidgetList)
		{
			if (creatureWidget != null)
			{
				creatureWidget.FadeIn(0.3f);
			}
		}
	}

	public void FadeInLayouts()
	{
		VespaCoroutine.Start(coFadeInLayouts());
	}

	private IEnumerator coFadeInLayouts()
	{
		yield return new WaitForSeconds(0.3f);
		TweenAlpha.Begin(MyGuildInfo.InfoRoot, 0.3f, 1f);
	}

	private IEnumerator TweenPositionAvatarts()
	{
		TweenPos(MyGuildInfo.AvatarRoot.Left.gameObject, 0.2f, -1000);
		TweenPos(EnemyGuildInfo.AvatarRoot.Left.gameObject, 0.2f, 1000);
		MyGuildInfo.AvatarRoot.Left.FadeIn(0.2f);
		EnemyGuildInfo.AvatarRoot.Left.FadeIn(0.2f);
		yield return new WaitForSeconds(0.1f);
		TweenPos(MyGuildInfo.AvatarRoot.Center.gameObject, 0.2f, -1000);
		TweenPos(EnemyGuildInfo.AvatarRoot.Center.gameObject, 0.2f, 1000);
		MyGuildInfo.AvatarRoot.Center.FadeIn(0.2f);
		EnemyGuildInfo.AvatarRoot.Center.FadeIn(0.2f);
		yield return new WaitForSeconds(0.1f);
		TweenPos(MyGuildInfo.AvatarRoot.Right.gameObject, 0.2f, -1000);
		TweenPos(EnemyGuildInfo.AvatarRoot.Right.gameObject, 0.2f, 1000);
		MyGuildInfo.AvatarRoot.Right.FadeIn(0.1f);
		EnemyGuildInfo.AvatarRoot.Right.FadeIn(0.1f);
		yield return null;
	}

	private void TweenPos(GameObject gameObject, float duration, int startDiffX)
	{
		Vector3 localPosition = gameObject.transform.localPosition;
		gameObject.transform.localPosition = new Vector3(localPosition.x + (float)startDiffX, localPosition.y, localPosition.z);
		TweenPosition.Begin(gameObject, duration, localPosition);
		UIHelper.EnableGameObject(gameObject, enable: true);
	}

	public void TweenPositionLayouts()
	{
		VespaCoroutine.Start(coTweenPositionLayouts());
	}

	private IEnumerator coTweenPositionLayouts()
	{
		yield return new WaitForSeconds(0.8f);
		TweenAlpha.Begin(MyGuildInfo.InfoRoot, 0.2f, 1f);
		TweenAlpha.Begin(EnemyGuildInfo.InfoRoot, 0.2f, 1f);
		TweenPos(MyGuildInfo.InfoRoot, 0.2f, -1000);
		TweenPos(EnemyGuildInfo.InfoRoot, 0.2f, 1000);
	}

	public override void Reset()
	{
		base.Reset();
		Clear();
	}

	public void Clear()
	{
		for (int i = 0; i < CreatureWidgetList.Count; i++)
		{
			if (CreatureWidgetList[i] != null)
			{
				CreatureWidgetList[i].Close();
			}
		}
		CreatureWidgetList.Clear();
	}

	public override void Close()
	{
		base.Close();
	}

	public override void OnClosing()
	{
		base.OnClosing();
		HideLayout();
		VSEffect.Stop(immediatelyClear: true);
		MyGuildInfo.UIFlagWidget.Close();
		EnemyGuildInfo.UIFlagWidget.Close();
		SingletonAuto<BgmManager2>.instance.StopEvent("BGM2.Guild.GuildEntryMenu");
		if (_entryMenuBGM != "BGM2.Guild.GuildEntryMenu")
		{
			SingletonAuto<BgmManager2>.instance.Request(_entryMenuBGM, isEvent: true);
		}
	}

	public void Open(Listener listener)
	{
		_listener = listener;
		if (GlobalAutoBehaviour<GuildManager>.isValidInstance && GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager != null && GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager != null)
		{
			_sessionState = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetGuildArenaSessionInfo().State;
			_myGuildInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetMyGuildInfo();
			_enemyGuildInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetEnemyGuildInfo();
			_isMatched = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.IsMatched;
			_applyMemberCount = GameRoot.Instance.ConstantDataContainer.GetValueInt("MinGuildArenaMemberCount", 20);
			_playEffect = true;
			Open();
			Init();
			PortalHelper.UpdateGuildArenaEnterInfo();
		}
	}

	public void SetPlayEffectSound(bool enable)
	{
		_playEffect = enable;
	}

	public void Init()
	{
		_isLoaded = false;
		_lastRefreshInfoTime = Time.time;
		Clear();
		SetLayoutType();
		InitLayout();
		InitGuildInfo(TeamType.Left, _myGuildInfo);
		InitGuildInfo(TeamType.Right, _enemyGuildInfo);
		InitGuildFlagWidgets();
		InitButtons();
		InitAutoApply();
		PlayBGM();
		OpenGuildArenaSessionResult();
	}

	public void HideLayout()
	{
		MyGuildInfo.InfoRoot.GetComponent<UIWidget>().alpha = 0f;
		EnemyGuildInfo.InfoRoot.GetComponent<UIWidget>().alpha = 0f;
	}

	public void SetLayoutType()
	{
		GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.IsGuildArenaApplied();
		switch (_sessionState)
		{
		case GuildArenaSessionState.Apply:
		case GuildArenaSessionState.ApplyEnded:
		case GuildArenaSessionState.Ended:
			_layoutType = LayoutType.APPLY;
			break;
		case GuildArenaSessionState.Battle:
			if (_isMatched)
			{
				_layoutType = LayoutType.BATTLE;
			}
			else
			{
				_layoutType = LayoutType.NOTMATCHED;
			}
			break;
		}
	}

	public void InitLayout()
	{
		switch (_layoutType)
		{
		case LayoutType.APPLY:
		case LayoutType.NOTMATCHED:
			UIHelper.EnableGameObject(EnemyGuildInfo.Root, enable: false);
			UIHelper.EnableGameObject(VersusRoot, enable: false);
			UIHelper.EnableGameObject(EnemyGuildInfo.FlagRoot, enable: false);
			ResetPosGameObject(MyGuildInfo.Root, MyGuild_CenterRoot);
			ResetPosGameObject(MyGuildInfo.FlagRoot, FlagCenterPos);
			break;
		case LayoutType.BATTLE:
			UIHelper.EnableGameObject(EnemyGuildInfo.Root, enable: true);
			UIHelper.EnableGameObject(VersusRoot, enable: true);
			UIHelper.EnableGameObject(EnemyGuildInfo.FlagRoot, enable: true);
			ResetPosGameObject(MyGuildInfo.Root, MyGuild_OriginRoot);
			ResetPosGameObject(MyGuildInfo.FlagRoot, FlagOriginPos);
			break;
		}
	}

	private void ResetPosGameObject(GameObject target, GameObject pos)
	{
		if (!(target == null) && !(pos == null))
		{
			target.transform.localPosition = pos.transform.localPosition;
		}
	}

	private void InitGuildFlagWidgets()
	{
		if (_myGuildInfo != null)
		{
			MyGuildInfo.UIFlagWidget.Open(_myGuildInfo.Flag, _myGuildInfo.Logo, _myGuildInfo.Back);
			if (_enemyGuildInfo != null)
			{
				EnemyGuildInfo.UIFlagWidget.Open(_enemyGuildInfo.Flag, _enemyGuildInfo.Logo, _enemyGuildInfo.Back);
			}
		}
	}

	private IEnumerator coPlayGuildFlagWidgets()
	{
		yield return new WaitForSeconds(0.2f);
		MyGuildInfo.UIFlagWidget.Play();
		yield return new WaitForSeconds(0.2f);
		EnemyGuildInfo.UIFlagWidget.Play();
	}

	public void InitButtons()
	{
		bool flag = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.IsGuildArenaApplied();
		switch (_sessionState)
		{
		case GuildArenaSessionState.Apply:
			UIHelper.EnableButton(Button_Apply, enable: true, grayscaleIfDisable: true);
			UIHelper.EnableButton(Button_DefenceSetting, enable: true, grayscaleIfDisable: true);
			UIHelper.EnableGameObject(Button_Apply, !flag);
			UIHelper.EnableGameObject(Button_ApplyComplete, flag);
			UIHelper.EnableGameObject(Button_Start, enable: false);
			UIHelper.EnableGameObject(Button_DefenceSetting, enable: true);
			UIHelper.EnableGameObject(Button_ArenaGuide, enable: true);
			UIHelper.EnableGameObject(Button_RankInfo, enable: true);
			VespaCoroutine.Start(coRefreshButtonObjects(new GameObject[3] { Button_Apply.gameObject, Button_ApplyComplete.gameObject, Button_DefenceSetting.gameObject }, new bool[3]
			{
				!flag,
				flag,
				true
			}));
			break;
		case GuildArenaSessionState.ApplyEnded:
			UIHelper.EnableButton(Button_Apply, enable: false, grayscaleIfDisable: true);
			UIHelper.EnableButton(Button_DefenceSetting, enable: false, grayscaleIfDisable: true);
			UIHelper.EnableGameObject(Button_Apply, enable: true);
			UIHelper.EnableGameObject(Button_ApplyComplete, enable: false);
			UIHelper.EnableGameObject(Button_Start, enable: false);
			UIHelper.EnableGameObject(Button_DefenceSetting, enable: true);
			UIHelper.EnableGameObject(Button_ArenaGuide, enable: true);
			UIHelper.EnableGameObject(Button_RankInfo, enable: true);
			VespaCoroutine.Start(coRefreshButtonObjects(new GameObject[2] { Button_Apply.gameObject, Button_DefenceSetting.gameObject }, new bool[2] { true, true }));
			break;
		case GuildArenaSessionState.Battle:
			UIHelper.EnableButton(Button_DefenceSetting, enable: false, grayscaleIfDisable: true);
			UIHelper.EnableButton(Button_Start, _isMatched);
			UIHelper.EnableGameObject(Button_Apply, enable: false);
			UIHelper.EnableGameObject(Button_ApplyComplete, enable: false);
			UIHelper.EnableGameObject(Button_Start, enable: true);
			UIHelper.EnableGameObject(Button_DefenceSetting, enable: true);
			UIHelper.EnableGameObject(Button_ArenaGuide, enable: true);
			UIHelper.EnableGameObject(Button_RankInfo, enable: true);
			VespaCoroutine.Start(coRefreshButtonObjects(new GameObject[2] { Button_DefenceSetting.gameObject, Button_Start.gameObject }, new bool[2] { true, true }));
			break;
		case GuildArenaSessionState.Ended:
			UIHelper.EnableButton(Button_DefenceSetting, enable: false, grayscaleIfDisable: true);
			UIHelper.EnableButton(Button_Apply, enable: false, grayscaleIfDisable: true);
			UIHelper.EnableGameObject(Button_Apply, enable: true);
			UIHelper.EnableGameObject(Button_ApplyComplete, enable: false);
			UIHelper.EnableGameObject(Button_Start, enable: false);
			UIHelper.EnableGameObject(Button_DefenceSetting, enable: true);
			UIHelper.EnableGameObject(Button_ArenaGuide, enable: true);
			UIHelper.EnableGameObject(Button_RankInfo, enable: true);
			UIHelper.EnableButton(Button_Apply, enable: false);
			UIHelper.EnableButton(Button_DefenceSetting, enable: false);
			VespaCoroutine.Start(coRefreshButtonObjects(new GameObject[2] { Button_Apply.gameObject, Button_DefenceSetting.gameObject }, new bool[2] { true, true }));
			break;
		}
	}

	private IEnumerator coRefreshButtonObjects(GameObject[] objects, bool[] enables)
	{
		if (objects.Length != enables.Length)
		{
			yield break;
		}
		for (int i = 0; i < 5; i++)
		{
			for (int objIndex = 0; objIndex < objects.Length; objIndex++)
			{
				UIHelper.EnableGameObject(objects[objIndex], enable: false);
				UIHelper.EnableGameObject(objects[objIndex], enables[objIndex]);
				yield return null;
			}
		}
		SetButtonEffect();
	}

	public void InitAutoApply()
	{
		bool enable = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.IsGuildArenaAutoApplied();
		UIHelper.EnableGameObject(AutoApplyChecked, enable);
	}

	private void PlayBGM()
	{
		SingletonAuto<BgmManager2>.instance.PendingRequest = false;
		_entryMenuBGM = SingletonAuto<BgmManager2>.instance.GetCurrentEventAssetBgmCode();
		if (_entryMenuBGM != "BGM2.Guild.GuildEntryMenu")
		{
			SingletonAuto<BgmManager2>.instance.Request("BGM2.Guild.GuildEntryMenu", isEvent: true);
		}
	}

	public void InitGuildInfo(TeamType teamType, GuildArenaRankerInfo guildInfo)
	{
		GuildInfo guildInfo2 = MyGuildInfo;
		if (teamType == TeamType.Right)
		{
			guildInfo2 = EnemyGuildInfo;
		}
		if (guildInfo != null)
		{
			string countryCode = guildInfo.CountryCode;
			CountryCodeData dataByCode = GameRoot.Instance.CountryCodeDataContainer.GetDataByCode(countryCode);
			if (dataByCode != null)
			{
				AssetTexture plainTexture = UIHelper.GetPlainTexture(dataByCode.IconPath);
				UIHelper.ResetPlaneAssetTextureAsync(this, guildInfo2.GuildCountryFlagTexture, plainTexture);
			}
			guildInfo2.Sprite_TierRank.Select(GetTierRank(guildInfo.TierIndex, guildInfo.Rank));
			UIHelper.SetLabelText(guildInfo2.Label_GuildName, guildInfo.Name);
			string stringDirect = LocalizationManager.GetStringDirect("HONOR_RANK_NUM", guildInfo.Rank);
			UIHelper.SetLabelText(guildInfo2.Label_Rank, (guildInfo.Rank > 0) ? stringDirect : "-");
			string text = $"{guildInfo.SeasonScore}{LocalizationManager.GetString(Match.SCORE_POSTFIX)}";
			UIHelper.SetLabelText(guildInfo2.Label_Record, (guildInfo.SeasonScore > 0) ? text : "-");
			string stringDirect2 = LocalizationManager.GetStringDirect("GUILD_ARENA_RECORD_INFO", guildInfo.SeasonWin, guildInfo.SeasonDraw, guildInfo.SeasonLose);
			bool flag = guildInfo.SeasonWin == 0 && guildInfo.SeasonDraw == 0 && guildInfo.SeasonLose == 0;
			UIHelper.SetLabelText(guildInfo2.Label_Arena_Record, (!flag) ? stringDirect2 : "-");
			int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxRecvJoinGuildRequest");
			UIHelper.SetLabelText(guildInfo2.Label_Member, $"{guildInfo.MemberCount}/{valueInt}");
			InitGuildAvatars(teamType, guildInfo);
		}
	}

	private void InitRemainTimeInfo()
	{
		int num = 0;
		long num2 = num;
		bool flag = false;
		GuildArenaSessionInfo guildArenaSessionInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetGuildArenaSessionInfo();
		if (guildArenaSessionInfo != null && (guildArenaSessionInfo.State == GuildArenaSessionState.Apply || guildArenaSessionInfo.State == GuildArenaSessionState.ApplyEnded))
		{
			flag = true;
			if (Vespa_String_Extension_Method2.TryParseNetworkTime2(guildArenaSessionInfo.BattleBeginTime, out var o_dateTime))
			{
				DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
				int num3 = Mathf.Max((int)(o_dateTime - currentServerUTCTime).TotalSeconds, 0);
				num2 = num3;
			}
		}
		int num6;
		if (!flag)
		{
			long num4 = num2;
			int num5 = 0;
			num6 = ((num4 > num5) ? 1 : 0);
		}
		else
		{
			num6 = 1;
		}
		bool flag2 = (byte)num6 != 0;
		UIHelper.EnableGameObject(RemainTimeRoot, flag2);
		if (flag2)
		{
			string stringDirect = LocalizationManager.GetStringDirect("GUILD_ARENA_REMAIN_DAY_HOUR", UIHelper.GetRemainTimeLabelText(num2));
			UIHelper.EnableGameObject(RemainTimeLabel, enable: true);
			UIHelper.SetLabelText(RemainTimeLabel, stringDirect);
		}
	}

	public void SetButtonEffect()
	{
		if (_sessionState != GuildArenaSessionState.Apply)
		{
			PlayButtonEffect(null);
			return;
		}
		GuildArenaDeckInfo[] savedDeckInfos = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetSavedDeckInfos();
		if (savedDeckInfos == null)
		{
			SceneAutoBehaviour<GuildArenaManagement>.instance.RequestGetMyGuildArenaDeck(delegate(GuildArenaDeckInfo[] deckInfos)
			{
				GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.SetSavedDeckInfos(deckInfos);
				PlayButtonEffect(deckInfos);
			});
		}
		else
		{
			PlayButtonEffect(savedDeckInfos);
		}
	}

	private void PlayButtonEffect(GuildArenaDeckInfo[] savedDeckInfos)
	{
		if (UiEffect_DefenceSettingBtn != null)
		{
			UiEffect_DefenceSettingBtn.Stop(immediatelyClear: true);
		}
		bool flag = false;
		if (savedDeckInfos != null)
		{
			int num = 3;
			int tierIndex = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetMyGuildInfo().TierIndex;
			GuildArenaTierData data = GameRoot.Instance.GuildArenaTierDataContainer.GetData(tierIndex);
			if (data != null)
			{
				num = data.DefenseDeckCount;
			}
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(savedDeckInfos); i++)
			{
				GuildArenaDeckInfo guildArenaDeckInfo = savedDeckInfos[i];
				if (guildArenaDeckInfo.DeckIndex > num)
				{
					if (guildArenaDeckInfo.CachedHeroInfos != null)
					{
					}
				}
				else if (__Extension_Method_ICollection__.COUNT(guildArenaDeckInfo.CachedHeroInfos) < 3)
				{
					flag = true;
					break;
				}
			}
		}
		if (flag && UiEffect_DefenceSettingBtn != null)
		{
			UiEffect_DefenceSettingBtn.Play(null);
		}
	}

	private string GetTierRank(int tierIndex, int rank)
	{
		if (tierIndex >= 80 && rank == 1)
		{
			return "Champion";
		}
		if (tierIndex >= 80)
		{
			return "Challenger";
		}
		if (tierIndex >= 70)
		{
			return "RoyalMaster";
		}
		if (tierIndex >= 60)
		{
			return "Master";
		}
		if (tierIndex >= 50)
		{
			return "Diamond";
		}
		if (tierIndex >= 40)
		{
			return "Platinum";
		}
		if (tierIndex >= 30)
		{
			return "Gold";
		}
		if (tierIndex >= 20)
		{
			return "Silver";
		}
		return "Bronze";
	}

	public void InitGuildAvatars(TeamType teamType, GuildArenaRankerInfo guildInfo)
	{
		GuildInfo guildInfo2 = MyGuildInfo;
		if (teamType == TeamType.Right)
		{
			guildInfo2 = EnemyGuildInfo;
		}
		if (guildInfo != null && guildInfo2.AvatarRoot != null)
		{
			if (guildInfo.GuildMembers.Length != 0 && guildInfo2.AvatarRoot != null && guildInfo2.AvatarRoot.Center != null)
			{
				OpenAvatar(guildInfo.GuildMembers[0], guildInfo2.AvatarRoot.Center);
			}
			if (guildInfo.GuildMembers.Length > 1 && guildInfo2.AvatarRoot != null && guildInfo2.AvatarRoot.Left != null)
			{
				OpenAvatar(guildInfo.GuildMembers[1], guildInfo2.AvatarRoot.Left);
			}
			if (guildInfo.GuildMembers.Length > 2 && guildInfo2.AvatarRoot != null && guildInfo2.AvatarRoot.Right != null)
			{
				OpenAvatar(guildInfo.GuildMembers[2], guildInfo2.AvatarRoot.Right);
			}
		}
	}

	private void OpenAvatar(GuildMemberServerInfo rankerInfo, UICreatureWidget creatureWidget)
	{
		UIAvatarHelper.AvatarInfo avatarInfo = new UIAvatarHelper.AvatarInfo(rankerInfo.AvatarHeroIndex);
		CreatureData creatureData = avatarInfo.GetCreatureData();
		int star = avatarInfo.Star;
		int costumeIndex = avatarInfo.CostumeIndex;
		if (creatureData != null)
		{
			creatureWidget.Open(creatureData, star, costumeIndex);
			if (creatureWidget.RenderFacade != null)
			{
				creatureWidget.RenderFacade.AssetCreature.SetShaderTransform(new ShaderTransformData(-5f, depthShift: false), null);
			}
			creatureWidget.SetAlpha(0f);
			CreatureWidgetList.Add(creatureWidget);
		}
	}

	public void OpenGuildArenaSessionResult()
	{
		GuildArenaSessionInfo guildArenaSessionInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetGuildArenaSessionInfo();
		if (guildArenaSessionInfo == null)
		{
			return;
		}
		LocalAccountBool localAccountBool = new LocalAccountBool(i_default: false, $"IsShownGuildArenaResult_SessionIndex{guildArenaSessionInfo.SessionIndex})");
		if (localAccountBool.Value || _sessionState != GuildArenaSessionState.Ended)
		{
			return;
		}
		GuildArenaRankerInfo myGuildInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetMyGuildInfo();
		if (myGuildInfo != null && guildArenaSessionInfo.CompletedRewardedTime != null && !String_Extension_Method.IsNullOrEmpty(guildArenaSessionInfo.CompletedRewardedTime))
		{
			if (!_isMatched && myGuildInfo.Rank != 0)
			{
				VespaCoroutine.Start(coOpenGuildArenaSeasonResult());
				localAccountBool.Value = true;
			}
			else if (_isMatched)
			{
				VespaCoroutine.Start(coOpenGuildArenaSessionResult());
				localAccountBool.Value = true;
			}
		}
	}

	private IEnumerator coOpenGuildArenaSessionResult()
	{
		while (!_isLoaded)
		{
			yield return null;
		}
		yield return new WaitForSeconds(0.2f);
		SceneAutoBehaviour<GuildArenaManagement>.instance.OpenGuildArenaSessionResult();
	}

	private IEnumerator coOpenGuildArenaSeasonResult()
	{
		while (!_isLoaded)
		{
			yield return null;
		}
		yield return new WaitForSeconds(0.2f);
		SceneAutoBehaviour<GuildArenaManagement>.instance.OpenGuildArenaSeasonResult();
	}

	public void OnClickGuildDefenceSetting()
	{
		if (_listener != null)
		{
			_listener.OpenGuildArenaSetting();
		}
	}

	public void OnClickGuildArenaTotalInfo()
	{
		if (_listener != null)
		{
			_listener.OpenGuildArenaTotalInfo();
		}
	}

	public void OnClickGuildArenaRankingInfo()
	{
		if (_listener != null)
		{
			_listener.OpenGuildArenaRankingInfo();
		}
	}

	public void OnClickGuildArenaMatchList()
	{
		if (_listener != null)
		{
			_listener.OpenGuildArenaMatchList();
		}
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

	private bool IsMasterOrAdmin()
	{
		GuildMemberInfo myMemberInfo = GlobalAutoBehaviour<GuildManager>.instance.MemberManager.GetMyMemberInfo();
		if (myMemberInfo == null || (myMemberInfo.Rank != 1 && myMemberInfo.Rank != 2))
		{
			return false;
		}
		return true;
	}

	private bool IsMaster()
	{
		GuildMemberInfo myMemberInfo = GlobalAutoBehaviour<GuildManager>.instance.MemberManager.GetMyMemberInfo();
		if (myMemberInfo == null || myMemberInfo.Rank != 1)
		{
			return false;
		}
		return true;
	}

	private bool IsEnoughMember()
	{
		if (_myGuildInfo.MemberCount >= _applyMemberCount)
		{
			return true;
		}
		return false;
	}

	public void OnClickApply()
	{
		if (!IsMasterOrAdmin())
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("GUILD_ARENA_PARTICIPATION_FAIL"));
		}
		else if (!IsEnoughMember())
		{
			string text = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string stringDirect = LocalizationManager.GetStringDirect("GUILD_ARENA_PARTICIPATION_NOT_CONFIRM", _applyMemberCount);
			MessagePopup.Open(text, stringDirect);
		}
		else
		{
			bool flag = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.IsGuildArenaApplied();
			if (!IsTutorialProgressing() && !flag && _listener != null)
			{
				_listener.OnClickApplyGuildArena();
			}
		}
	}

	public void OnClickAutoApply(GameObject senderObject)
	{
		if (UIHelper.IsEnabledGameObjet(senderObject))
		{
			if (!IsMaster())
			{
				CenterMessage.Open(LocalizationManager.GetStringDirect("GUILD_MASTER_ONLY_CANCEL"));
			}
			else if (!IsEnoughMember())
			{
				string text = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
				string stringDirect = LocalizationManager.GetStringDirect("GUILD_ARENA_PARTICIPATION_NOT_CONFIRM", _applyMemberCount);
				MessagePopup.Open(text, stringDirect);
			}
			else
			{
				RequestAutoApply(isApply: false, delegate
				{
					UIHelper.ToggleGameObject(senderObject);
				});
			}
			return;
		}
		if (!IsMasterOrAdmin())
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("GUILD_ARENA_AUTO_PARTICIPATION_FAIL"));
			return;
		}
		string text2 = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string stringDirect2 = LocalizationManager.GetStringDirect("GUILD_ARENA_AUTO_PARTICIPATION_NOTICE");
		if (!IsEnoughMember())
		{
			stringDirect2 = LocalizationManager.GetStringDirect("GUILD_ARENA_PARTICIPATION_NOT_CONFIRM", _applyMemberCount);
			MessagePopup.Open(text2, stringDirect2);
			return;
		}
		MessagePopup.Open(text2, stringDirect2, delegate
		{
			RequestAutoApply(isApply: true, delegate
			{
				UIHelper.ToggleGameObject(senderObject);
				GuildArenaSessionInfo guildArenaSessionInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetGuildArenaSessionInfo();
				if (guildArenaSessionInfo != null && guildArenaSessionInfo.State == GuildArenaSessionState.Apply && !GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.IsGuildArenaApplied())
				{
					OnClickApply();
				}
			});
		}, null);
	}

	private void RequestAutoApply(bool isApply, Action callback)
	{
		NShared.AutoApplyGuildArena.Request request = new NShared.AutoApplyGuildArena.Request
		{
			IsApply = isApply
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.AutoApplyGuildArena.Response response)
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
				GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.SetGuildArenaAutoApplied(response.AutoApply);
				Vespa_System_Action_Extension_Method.fire(callback);
			}
		});
	}
}
