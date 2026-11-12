using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NScene;
using NGame2.NScene.NLobby;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI.NWorldMap;
using NGame2.NUI.NWorldMap.NMapLayout;
using NGame2.NUI.NWorldMap.NMapLayout.NEventManager;
using NGame2.NUtil;
using NShared;
using NShared.BeginCampaign;
using NShared.GetPassiveInfo;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NManager;

internal class ShakmehDungeonManagement : BaseManagement<NGame2.NUI.NManager.ShakmehDungeonManagement>, ShakmehDungeonWindow.Listener, ShakmehPartySetting.Listener, IUserObserver<UserManager.UserStatInfo>, IUserObserver
{
	public enum ShakmehNpcActionState
	{
		None,
		Idle_Middle,
		Idle_Final,
		Appear_Middle,
		Appear_Final
	}

	private enum EffectState
	{
		Idle,
		Active
	}

	public class ShakmehDungeonEnterInfo
	{
		private int _maxPoint;

		private const string _localSaveKey = "SHAKMEH_MIDDLE_BOSS_POINT_BEFORE_VALUE";

		public int GetPrevBossPoint => GetLocalSaveShakmehPoint();

		public long GetCurrBossPoint => GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetUserStat(UserManager.UserStatType.ShakmehMiddleBossPoint);

		public int GetMaxPoint => _maxPoint;

		public ShakmehDungeonEnterInfo(int maxVal)
		{
			_maxPoint = maxVal;
		}

		public int GetLocalSaveShakmehPoint()
		{
			return new LocalAccountInt(0, "SHAKMEH_MIDDLE_BOSS_POINT_BEFORE_VALUE").Value;
		}

		public void SetLocalSaveShakmehPoint(int? val = null)
		{
			int num = 0;
			long num2 = num;
			if (!val.HasValue)
			{
				num2 = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetUserStat(UserManager.UserStatType.ShakmehMiddleBossPoint);
			}
			else
			{
				int value = val.Value;
				num2 = value;
			}
			new LocalAccountInt(0, "SHAKMEH_MIDDLE_BOSS_POINT_BEFORE_VALUE").Value = checked((int)num2);
		}
	}

	private EffectState _effectState;

	private float _battleRequestedTime;

	public float ShakmehNodeEffectDelay = 3.1f;

	public ShakmehBossData ShakmehBossData { get; private set; }

	public ShakmehDungeonEnterInfo ShakmehDungeonEnterData { get; private set; }

	public ShakmehNpcActionState CurrShakmehNpcActionState { get; private set; }

	public bool IsOpenShakmehDungeon
	{
		get
		{
			IList<CampaignNodeData> data_by_Function = GameRoot.Instance.CampaignNodeDataContainer.GetData_by_Function(NodeFunctionType.ShakmehDungeon);
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(data_by_Function); i++)
			{
				CampaignNodeData campaignNodeData = data_by_Function[i];
				if (campaignNodeData != null && !SingletonAuto<ConditionChecker>.instance.Check(campaignNodeData.ReqCondition))
				{
					return false;
				}
			}
			return true;
		}
	}

	public bool IsEnoughShakmehBossGauge
	{
		get
		{
			CurrencyTypeData data = GameRoot.Instance.CurrencyTypeDataContainer.GetData(CurrencyType.ShakmehMiddleBossPoint);
			if (data == null)
			{
				return false;
			}
			long userStat = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetUserStat(UserManager.UserStatType.ShakmehMiddleBossPoint);
			int maxValue = data.MaxValue;
			return userStat >= maxValue;
		}
	}

	private DefaultBattlePartySettingDataProdiver GetDataProdiver(int chapterIndex, int index, int level)
	{
		ShakmehDungeonData data = GameRoot.Instance.ShakmehDungeonDataContainer.GetData(index, level);
		if (data == null)
		{
			return null;
		}
		int dungeonIndex = data.DungeonIndex;
		CampaignDungeonData data2 = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
		_ = data2.ReqStaminaType;
		int staminaOfDifficulty = data2.GetStaminaOfDifficulty(GlobalAutoBehaviour<BoosterManager>.instance.CheckSaveStamina());
		long staminaCount = staminaOfDifficulty;
		return new DefaultBattlePartySettingDataProdiver(data2.LocalStorageKey, enterDungeon: new EnterDungeonData(chapterIndex, dungeonIndex, dungeonIndex), banRuleData: GameRoot.Instance.BanRuleDataContainer.GetData(data.BanRuleIndex), staminaType: data2.ReqStaminaType, staminaCount: staminaCount, startButtonTextKey: Battle.START_BATTLE);
	}

	protected override void Awake()
	{
		base.Awake();
		AddInstance<ShakmehDungeonWindow>("Resources/Prefab2/Window/ShakmehDungeon/ShakmehDungeonWindow");
		AddInstance<ShakmehBossInfoPopup>("Resources/Prefab2/Window/ShakmehDungeon/ShakmehBossInfoPopup");
		AddInstance<ShakmehPartySetting>("Resources/Prefab2/Window/ShakmehDungeon/ShakmehPartySetting");
		CurrencyTypeData data = GameRoot.Instance.CurrencyTypeDataContainer.GetData(CurrencyType.ShakmehMiddleBossPoint);
		if (data != null)
		{
			int maxValue = data.MaxValue;
			ShakmehDungeonEnterData = new ShakmehDungeonEnterInfo(maxValue);
			GlobalAutoBehaviour<AccountManager>.instance.UserManager.ObserverManager.AddObserver(this);
			SetShakmehNpcActionState();
		}
	}

	public void OpenShakmehDungeonWindow(bool needMove = true)
	{
		if (!RestoredPortal.ShakmehAccess())
		{
			return;
		}
		if (!IsEnoughShakmehBossGauge)
		{
			ShakmehBossData dataByBossType = GameRoot.Instance.ShakmehBossDataContainer.GetDataByBossType(ShakmehBossType.Middle);
			if (dataByBossType != null)
			{
				OpenShakmehDungeonWindow(dataByBossType.ChapterIndex, dataByBossType.DungeonIndex);
			}
			return;
		}
		ShakmehBossData dataByBossType2 = GameRoot.Instance.ShakmehBossDataContainer.GetDataByBossType(ShakmehBossType.Final);
		if (dataByBossType2 != null)
		{
			if (needMove)
			{
				NGame2.NAccount.UserCampaignManager.DungeonIndex = dataByBossType2.DungeonIndex;
			}
			OpenShakmehDungeonWindow(dataByBossType2.ChapterIndex, dataByBossType2.DungeonIndex);
		}
	}

	public void OpenShakmehDungeonWindow(ShakmehBossType shakmehBossType)
	{
		if (RestoredPortal.ShakmehAccess())
		{
			ShakmehBossData dataByBossType = GameRoot.Instance.ShakmehBossDataContainer.GetDataByBossType(ShakmehBossType.Middle);
			if (dataByBossType != null)
			{
				int chapterIndex = dataByBossType.ChapterIndex;
				int dungeonIndex = dataByBossType.DungeonIndex;
				OpenShakmehDungeonWindow(chapterIndex, dungeonIndex);
			}
		}
	}

	public void OpenShakmehDungeonWindow(int chapterIndex, int dungeonIndex)
	{
		if (!RestoredPortal.ShakmehBossAccess(chapterIndex, dungeonIndex) || !RestoredPortal.ShakmehAccess())
		{
			return;
		}
		ShakmehBossData = GameRoot.Instance.ShakmehBossDataContainer.GetData(chapterIndex, dungeonIndex);
		if (ShakmehBossData == null)
		{
			return;
		}
		switch (ShakmehBossData.BossType)
		{
		case ShakmehBossType.Middle:
			if (IsEnoughShakmehBossGauge)
			{
				return;
			}
			break;
		case ShakmehBossType.Final:
			if (!IsEnoughShakmehBossGauge)
			{
				return;
			}
			break;
		}
		RequestShakmehBuffList(delegate
		{
			OpenShakmehDungeonWindow(ShakmehBossData);
		});
	}

	public void SetShakmehNpcActionState()
	{
		int getPrevBossPoint = ShakmehDungeonEnterData.GetPrevBossPoint;
		long getCurrBossPoint = ShakmehDungeonEnterData.GetCurrBossPoint;
		int getMaxPoint = ShakmehDungeonEnterData.GetMaxPoint;
		ShakmehNpcActionState shakmehNpcActionState = ShakmehNpcActionState.None;
		long num = getCurrBossPoint;
		int num2 = getPrevBossPoint;
		if (num2 == num)
		{
			int num3 = getMaxPoint;
			shakmehNpcActionState = ((getCurrBossPoint != num3) ? ShakmehNpcActionState.Idle_Middle : ShakmehNpcActionState.Idle_Final);
		}
		else
		{
			int num4 = getMaxPoint;
			if (getCurrBossPoint == num4)
			{
				shakmehNpcActionState = ShakmehNpcActionState.Appear_Final;
			}
			else
			{
				long num5 = getCurrBossPoint;
				int num6 = getPrevBossPoint;
				shakmehNpcActionState = ((num6 <= num5 || getCurrBossPoint != 0L) ? ShakmehNpcActionState.Idle_Middle : ShakmehNpcActionState.Appear_Middle);
			}
		}
		CurrShakmehNpcActionState = shakmehNpcActionState;
	}

	public bool IsShakmehChatper(int chapterIndex)
	{
		return GameRoot.Instance.CampaignNodeDataContainer.GetData_by_Function(NodeFunctionType.ShakmehDungeon).Any((CampaignNodeData v) => v.ChapterIndex == chapterIndex && v.Function == NodeFunctionType.ShakmehDungeon);
	}

	public int GetExpectedBossPointByDungeon(int dungeonIndex)
	{
		ShakmehDungeonData dataByDungeonIndex = GameRoot.Instance.ShakmehDungeonDataContainer.GetDataByDungeonIndex(dungeonIndex);
		if (dataByDungeonIndex == null)
		{
			return 0;
		}
		return GameRoot.Instance.RewardDataContainer.GetData(dataByDungeonIndex.RewardIndex)?.GetItemMaxCount(UserManager.UserStatType.ShakmehMiddleBossPoint.ToString()) ?? 0;
	}

	public void RequestShakmehBuffList(Action callback)
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetProjectileGroupIndexFromServer(BattleType.Shakmeh)))
		{
			Vespa_System_Action_Extension_Method.fire(callback);
			return;
		}
		NShared.GetPassiveInfo.Request request = new NShared.GetPassiveInfo.Request();
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetPassiveInfo.Response response)
		{
			if (response != null && response.BaseResult == BaseResultType.Success && response.Result == NShared.GetPassiveInfo.ResultType.Success)
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.SetShakmehBuffInfo(response.PassiveInfos);
				Vespa_System_Action_Extension_Method.fire(callback);
			}
		});
	}

	private void OpenShakmehDungeonWindow(ShakmehBossData shakmehBossData)
	{
		GetInstance<ShakmehDungeonWindow>().Open(this, shakmehBossData);
	}

	void ShakmehDungeonWindow.Listener.OnClickEnter(int chapterIndex, int index, int level, ShakmehBossType shakmehBossType)
	{
		DefaultBattlePartySettingDataProdiver dataProdiver = GetDataProdiver(chapterIndex, index, level);
		if (dataProdiver != null)
		{
			GetInstance<ShakmehPartySetting>().Open(shakmehBossType, this, dataProdiver);
		}
	}

	void ShakmehDungeonWindow.Listener.OnClickInfo(int chapterIndex, int dungeonIndex, ShakmehBossData shakmehBossData)
	{
		GetInstance<ShakmehBossInfoPopup>().Open(chapterIndex, dungeonIndex, shakmehBossData);
	}

	void ShakmehPartySetting.Listener.StartBattle(ShakmehBossType shakmehBossType, EnterDungeonData enterDungeon, BattlePartySetting.BattlePartyInfo battlePartyInfo, BattlePartySetting.ExtraOptionData extraOption)
	{
		if (Time.time - _battleRequestedTime < 10f)
		{
			return;
		}
		_battleRequestedTime = Time.time;
		NShared.BeginCampaign.Request obj = new NShared.BeginCampaign.Request
		{
			ChapterIndex = enterDungeon.Chapter,
			DungeonIndex = enterDungeon.OriginalDungeon,
			LinkedDungeonIndex = enterDungeon.LinkedDungeon,
			DungeonDifficulty = (int)enterDungeon.Difficulty,
			ScenarioDungeon = enterDungeon.IsScenario,
			HeroIndices = battlePartyInfo.heroIndices,
			LeaderHeroIndex = battlePartyInfo.leaderHeroIndex,
			AccountIds = battlePartyInfo.accountIds,
			Repeat = (extraOption != null && extraOption.AutoRepeat)
		};
		int onetimeBoosterCount = battlePartyInfo.OnetimeBoosterCount;
		obj.ItemBoosterCount = onetimeBoosterCount;
		obj.AffixIndices = battlePartyInfo.AffixIndices;
		NShared.BeginCampaign.Request request = obj;
		if (GameRoot.Instance.SelectRewardDataContainer.GetData(request.ChapterIndex, request.DungeonIndex) != null)
		{
			request.SelectedRewardItemCodes = NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.GetSelectedRewardToArray();
		}
		if (shakmehBossType == ShakmehBossType.Final)
		{
			request.GroupHeroIndices = battlePartyInfo.subHeroIndices;
		}
		else
		{
			request.AiHeroIndices = battlePartyInfo.subHeroIndices;
		}
		SingletonAuto<BattleContext>.instance.BeginCampaign(request, delegate
		{
			if (extraOption != null && extraOption.AutoRepeat)
			{
				NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.OpenGlobal();
			}
			LobbyScene2.ListReturnState.Add(new ShakmehReturnState(enterDungeon));
			ShakmehBossData dataByBossType = GameRoot.Instance.ShakmehBossDataContainer.GetDataByBossType(shakmehBossType);
			GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.SaveLatelyBattleNodePos(dataByBossType.ChapterIndex, dataByBossType.DungeonIndex);
			ShakmehDungeonEnterData.SetLocalSaveShakmehPoint();
		}, delegate
		{
			_battleRequestedTime = 0f;
		});
	}

	void ShakmehPartySetting.Listener.OpenHeroDetailView(CachedHeroInfo heroInfo)
	{
		SceneAutoBehaviour<LobbyCampaignDungeon>.instance.OpenHeroDetailView(heroInfo);
	}

	bool ShakmehPartySetting.Listener.OpenAutoNextBattleSelect()
	{
		return false;
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<UserManager.UserStatInfo>.OnChanged(UserManager.UserStatInfo userStatInfo)
	{
		if (userStatInfo.UserStatType == UserManager.UserStatType.ShakmehMiddleBossPoint)
		{
			if (_effectState == EffectState.Idle)
			{
				long num = userStatInfo.BeforeValue;
				ShakmehDungeonEnterInfo shakmehDungeonEnterData = ShakmehDungeonEnterData;
				long num2 = num;
				shakmehDungeonEnterData.SetLocalSaveShakmehPoint(checked((int)num2));
			}
			StartShakmehEffect();
		}
	}

	public void StartShakmehEffect()
	{
		if (_effectState == EffectState.Active)
		{
			VespaDebug.LogError("Already Shakmeh Effect Running!!!");
		}
		else
		{
			VespaCoroutine.Start(coStartShakmehEffect(), null, null);
		}
	}

	private IEnumerator coStartShakmehEffect()
	{
		_effectState = EffectState.Active;
		while (GlobalAutoBehaviour<LoadingManager>.instance.IsSceneLoading)
		{
			yield return null;
		}
		yield return null;
		SetShakmehNpcActionState();
		EventManager eventManager = SceneAutoBehaviour<WorldMapManagement>.instance.GetInstance<MapLayout>().EventManager;
		eventManager.GetComponent<DungeonNpcManager>().UpdateShakmehNodeNpcAvatar();
		IncreaseDic<int, MapNode> nodeDictionary = eventManager.Mediator.NodeGraph.GetNodeDictionary();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(nodeDictionary); i++)
		{
			nodeDictionary.GetValueByIndex(i).UpdateShakmehNodeState();
		}
		LobbyShakmehBossGauge lobbyShakmehBossGauge = SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<LobbyShakmehBossGauge>(openedOnly: true);
		if (lobbyShakmehBossGauge != null && UIHelper.IsEnabledGameObjet(lobbyShakmehBossGauge))
		{
			int getPrevBossPoint = ShakmehDungeonEnterData.GetPrevBossPoint;
			long getCurrBossPoint = ShakmehDungeonEnterData.GetCurrBossPoint;
			lobbyShakmehBossGauge.UpdateGauge(getPrevBossPoint, getCurrBossPoint);
		}
		yield return null;
		ShakmehDungeonEnterData.SetLocalSaveShakmehPoint();
		_effectState = EffectState.Idle;
	}
}
