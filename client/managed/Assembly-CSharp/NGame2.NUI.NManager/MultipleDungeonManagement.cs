using System;
using System.Collections;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NScene;
using NGame2.NScene.NLobby;
using NGame2.NTutorial;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI.NWindow.NMultipleDungeon;
using NGame2.NUtil;
using NShared;
using NShared.BeginCampaign;
using NShared.GetUnderPrisonInfo;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NTask;
using UnityEngine;

namespace NGame2.NUI.NManager;

public class MultipleDungeonManagement : BaseManagement<MultipleDungeonManagement>, MultipleDungeonWindow.Listener, DungeonDiffSelect.Listener, BattlePartySetting.Listener
{
	private int _SelectedDungeonIndex;

	private int _SelectedDiff;

	private bool _waitForBattle;

	private static DateTime _CheckDungeonInfoTime;

	public StaminaType MultipleDungeonStaminaType => StaminaType.PrisonKey;

	protected override void Awake()
	{
		base.Awake();
		UseBackgroundBlack(use: true);
		_CheckDungeonInfoTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime();
		AddInstance<MultipleDungeonWindow>("Resources/Prefab2/Window/MultipleDungeon/MultipleDungeonWindow");
		AddInstance<DungeonDiffSelect>("Resources/Prefab2/Window/EventDungeon/DungeonDiffSelect2");
		AddInstance<BattlePartySetting>("Resources/Prefab2/Window/BattlePartySetting/BattlePartySetting");
	}

	public void OpenPrisonList(Action onOpen = null)
	{
		if (SingletonUpdater<TutorialManager>.isValidInstance && SingletonUpdater<TutorialManager>.instance.IsProgressing)
		{
			MultipleDungeonWindow multipleDungeonWindow = GetInstance<MultipleDungeonWindow>();
			if (multipleDungeonWindow != null)
			{
				multipleDungeonWindow.Open(this);
			}
		}
		else
		{
			AddTask(OpenPrisonListAsync(onOpen));
		}
	}

	protected override void ChainUpdate()
	{
		base.ChainUpdate();
		UpdateDOWDungeonInfo();
	}

	private void UpdateDOWDungeonInfo()
	{
		if (SingletonUpdater<TutorialManager>.isValidInstance && SingletonUpdater<TutorialManager>.instance.IsProgressing)
		{
			return;
		}
		MultipleDungeonWindow window = GetInstance<MultipleDungeonWindow>();
		if (window == null || !window.IsOpen() || !NeedToCheckDungeonInfo())
		{
			return;
		}
		NShared.GetUnderPrisonInfo.Request request = new NShared.GetUnderPrisonInfo.Request();
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetUnderPrisonInfo.Response response)
		{
			if (response.Result == NShared.GetUnderPrisonInfo.ResultType.Success)
			{
				GlobalAutoBehaviour<AccountManager>.instance.SetUnderPrisonInfos(response.UnderPrisonInfos);
				UserManager userManager = GlobalAutoBehaviour<AccountManager>.instance.UserManager;
				StaminaType multipleDungeonStaminaType = MultipleDungeonStaminaType;
				int underPrisonKey = response.UnderPrisonKey;
				userManager.SetStamina(multipleDungeonStaminaType, underPrisonKey);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.SetStaminaRechargeTime(MultipleDungeonStaminaType, response.UnderPrisonKeyRechargeTime);
				window.SetDungeonList();
			}
		});
	}

	private bool NeedToCheckDungeonInfo()
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.UnderPrisonManager == null)
		{
			return true;
		}
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("UpdateDOWDungeonInfoMin");
		DateTime currentServerLocalTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime();
		if ((currentServerLocalTime - _CheckDungeonInfoTime).TotalMinutes >= (double)valueInt)
		{
			_CheckDungeonInfoTime = currentServerLocalTime;
			return true;
		}
		return false;
	}

	private ITask OpenPrisonListAsync(Action onOpen = null)
	{
		return base.TaskFactory.Branch(base.TaskFactory.Run(() => true), new WebServiceTask<NShared.GetUnderPrisonInfo.Request, NShared.GetUnderPrisonInfo.Response>(new NShared.GetUnderPrisonInfo.Request(), delegate(NShared.GetUnderPrisonInfo.Request request, NShared.GetUnderPrisonInfo.Response response)
		{
			if (response.Result != NShared.GetUnderPrisonInfo.ResultType.Success)
			{
				Vespa_System_Action_Extension_Method.fire(onOpen);
				return base.TaskFactory.Run(delegate
				{
					MessagePopup.OpenError(response.Result);
				});
			}
			return base.TaskFactory.Sequence(base.TaskFactory.Run(delegate
			{
				GlobalAutoBehaviour<AccountManager>.instance.SetUnderPrisonInfos(response.UnderPrisonInfos);
				UserManager userManager = GlobalAutoBehaviour<AccountManager>.instance.UserManager;
				StaminaType multipleDungeonStaminaType = MultipleDungeonStaminaType;
				int underPrisonKey = response.UnderPrisonKey;
				userManager.SetStamina(multipleDungeonStaminaType, underPrisonKey);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.SetStaminaRechargeTime(MultipleDungeonStaminaType, response.UnderPrisonKeyRechargeTime);
			}), base.TaskFactory.Branch(base.TaskFactory.IsOpen<MultipleDungeonWindow>(), base.TaskFactory.Run(delegate(MultipleDungeonWindow window)
			{
				window.SetDungeonList();
			}), OpenDungeonListAsync(onOpen)));
		}), base.TaskFactory.Branch(base.TaskFactory.IsOpen<MultipleDungeonWindow>(), base.TaskFactory.Run(delegate(MultipleDungeonWindow window)
		{
			window.SetDungeonList();
		}), OpenDungeonListAsync(onOpen)));
	}

	private ITask OpenDungeonListAsync(Action onOpen = null)
	{
		return base.TaskFactory.Run(delegate(MultipleDungeonWindow window)
		{
			window.Open(this);
			Vespa_System_Action_Extension_Method.fire(onOpen);
		});
	}

	public void SetDungeonInfo(int selectedDungeonIndex, int selectedDiff)
	{
		_SelectedDungeonIndex = selectedDungeonIndex;
		_SelectedDiff = selectedDiff;
		GlobalAutoBehaviour<AccountManager>.instance.UnderPrisonManager.SelectedDiff = selectedDiff;
	}

	private IEnumerator OpenDungeon(UnderPrisonData underPrisonData, int diff)
	{
		DungeonDiffSelect dungeonDiffSelect = GetInstance<DungeonDiffSelect>();
		if (dungeonDiffSelect != null)
		{
			dungeonDiffSelect.Open(this, underPrisonData, diff);
		}
		yield return null;
	}

	private IEnumerator OpenBattlePartySetting(int chapterIndex, int dungeonIndex, int diff)
	{
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
		_SelectedDiff = diff;
		GlobalAutoBehaviour<AccountManager>.instance.UnderPrisonManager.SelectedDiff = diff;
		EnterDungeonData enterDungeon = new EnterDungeonData(chapterIndex, dungeonIndex, dungeonIndex, NShared.DungeonDifficultyType.Easy);
		DefaultBattlePartySettingDataProdiver dataProvider = new DefaultBattlePartySettingDataProdiver(data.LocalStorageKey, data.ReqStaminaType, data.GetStaminaOfDifficulty(GlobalAutoBehaviour<BoosterManager>.instance.CheckSaveStamina()), Battle.ENTER_TRIAL_DUNGEON, enterDungeon);
		GetInstance<BattlePartySetting>().Open(this, dataProvider);
		yield return null;
	}

	void MultipleDungeonWindow.Listener.EnterDungeon(UnderPrisonData underPrisonData)
	{
		VespaCoroutine.Start(OpenDungeon(underPrisonData, -1));
	}

	void DungeonDiffSelect.Listener.EnterDungeon(int chapterIndex, int dungeonIndex, int diff)
	{
		VespaCoroutine.Start(OpenBattlePartySetting(chapterIndex, dungeonIndex, diff));
	}

	bool DungeonDiffSelect.Listener.OnCloseRequest(DungeonDiffSelect closeThis)
	{
		return true;
	}

	void DungeonDiffSelect.Listener.SelectDungeonDiff(int Diff)
	{
		_SelectedDiff = Diff;
		GlobalAutoBehaviour<AccountManager>.instance.UnderPrisonManager.SelectedDiff = Diff;
	}

	void BattlePartySetting.Listener.StartBattle(EnterDungeonData enterDungeon, BattlePartySetting.BattlePartyInfo battlePartyInfo, BattlePartySetting.ExtraOptionData extraOption)
	{
		if (enterDungeon.CheckBattleEnter() && !_waitForBattle)
		{
			RequestBeginDungeon(enterDungeon, battlePartyInfo, 0, extraOption);
		}
	}

	private void RequestBeginDungeon(EnterDungeonData enterDungeon, BattlePartySetting.BattlePartyInfo info, int buyPlayKey, BattlePartySetting.ExtraOptionData extraOption)
	{
		if (enterDungeon == null)
		{
			return;
		}
		NShared.BeginCampaign.Request obj = new NShared.BeginCampaign.Request
		{
			ChapterIndex = enterDungeon.Chapter,
			DungeonIndex = enterDungeon.LinkedDungeon,
			HeroIndices = info.heroIndices,
			LeaderHeroIndex = info.leaderHeroIndex,
			AccountIds = info.accountIds,
			BuyKey = buyPlayKey
		};
		int onetimeBoosterCount = info.OnetimeBoosterCount;
		obj.ItemBoosterCount = onetimeBoosterCount;
		obj.AffixIndices = info.AffixIndices;
		NShared.BeginCampaign.Request request = obj;
		if (GameRoot.Instance.SelectRewardDataContainer.GetData(request.ChapterIndex, request.DungeonIndex) != null)
		{
			request.SelectedRewardItemCodes = NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.GetSelectedRewardToArray();
		}
		SingletonAuto<BattleContext>.instance.BeginCampaign(request, delegate
		{
			if (extraOption != null && extraOption.AutoRepeat)
			{
				NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.OpenGlobal();
			}
			LobbyScene2.ListReturnState.Add(new PrisonReturnState(_SelectedDiff));
		});
	}

	public IEnumerator ReturnStateOpenBattlePartySetting(int chapterIndex, int dungeonIndex, int diff)
	{
		yield return new WaitForSeconds(1f);
		if (GlobalAutoBehaviour<AccountManager>.instance.UnderPrisonManager.GetRemainEnterCountByChapterIndex(chapterIndex) <= 0)
		{
			yield return null;
		}
		UnderPrisonData underPrisonData = GameRoot.Instance.UnderPrisonDataContainer.GetData(chapterIndex);
		if (underPrisonData == null)
		{
			yield return null;
		}
		yield return VespaCoroutine.Start(OpenDungeon(underPrisonData, diff));
		yield return VespaCoroutine.Start(OpenBattlePartySetting(chapterIndex, dungeonIndex, diff));
		yield return null;
	}

	bool BattlePartySetting.Listener.OnCloseRequest(BattlePartySetting closeThis)
	{
		return true;
	}

	void BattlePartySetting.Listener.OpenHeroDetailView(CachedHeroInfo heroInfo)
	{
	}

	bool BattlePartySetting.Listener.OpenAutoNextBattleSelect()
	{
		return false;
	}
}
