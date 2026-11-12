using System;
using System.Collections.Generic;
using NGame2.N3D;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NBattleField;
using NGame2.NBattleThread.NThreadAction;
using NGame2.NBattleView;
using NGame2.NChatting;
using NGame2.NContents;
using NGame2.NCutScene;
using NGame2.NEventTrigger;
using NGame2.NMatch;
using NGame2.NMessageLog;
using NGame2.NNotify;
using NGame2.NParty;
using NGame2.NScene;
using NGame2.NScene.NLobby;
using NGame2.NSocket;
using NGame2.NTutorial;
using NGame2.NUI.NComponent2.NBattle;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI.NWindow.NChatting;
using NGameAsset;
using NShared;
using NShared.NCampaign;
using NShared.NEvent;
using NShared.NLocalization;
using NShared.NMessage.NProtocol;
using NShared.NParty;
using NShared.NWorldMap.NEventSubData;
using NVespa;
using NVespa.NGlobal;
using NVespa.NGui;
using NVespa.NScene;
using NVespa.NSingleton;
using NVespa.NSocket;
using UnityEngine;

namespace NGame2.NUI.NManager.NBattle;

public class BattleManagement : BaseManagement<BattleManagement>, SystemButtonAuto.Listener, SystemButtonAutoSimple.Listener, SystemButtonAutoFastContinue.Listener, SystemButtonPvpGiveUp.Listener, SystemButtonPause.Listener, SystemButtonExit.Listener, SystemButtonExitSmall.Listener, SystemButtonPartyExit.Listener, ReplayExitButton.Listener, PvpGiveUpMenu.Listener, PausedMenu.Listener, QuestStatus.Listener, HeroParty.Listener, PartySlotComponent2.Listener, GuildRaidScoreStatus.Listener, GuildRaidScoreStatus.IDataProvider, BattleEmoti.Listener, BattleLeave.Listener, IUserObserver<UserManager.UserStatInfo>, IUserObserver, PartyManager.IPartyListener, PartyManager.INewPartyListener, ChallengeRaidScoreStatus.Listener, ChallengeRaidScoreStatus.IDataProvider, IChatListener, EclipsePausedMenu.Listener, BattleCameraSwitch.IListener, DungeonSkillWindow.IListener, DamageMeter.Listener, KarmaCardSelectWindow.IListener
{
	[Flags]
	public enum EMenuHideFlag
	{
		InitCampaign = 1,
		BossIntro = 2,
		LastKill = 4,
		Tutorial = 8
	}

	private class SkillAutoReserveData
	{
		public float ElapsedTime;

		public List<Tuple<int, int>> SkillList = new List<Tuple<int, int>>();

		public SkillAutoReserveData(BattleFacade.CreatureCacheData creatureCache)
		{
			int baseSkillIndex = creatureCache.CreatureData.BaseSkillIndex;
			for (int i = 1; i <= 5; i++)
			{
				Helper.SkillBasicInfo skillBasicInfo = Helper.GetSkillBasicInfo(creatureCache.HeroInfo, i);
				if (skillBasicInfo.TableIndex != 0 && skillBasicInfo.TableIndex != baseSkillIndex && skillBasicInfo.TableData != null && skillBasicInfo.TableData.TriggerType == SkillTriggerType.None && !skillBasicInfo.IsLocked)
				{
					SkillList.Add(Tuple.Create(i, skillBasicInfo.TableData.Mp));
				}
			}
		}

		public int RandomSkillSlotIndex()
		{
			int index = UnityEngine.Random.Range(0, SkillList.Count);
			if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(SkillList, index))
			{
				return -1;
			}
			return SkillList[index].Item1;
		}
	}

	public delegate void ReserveSkillCallback(int creatureUid, int skillSlotIndex);

	public const float DefaultGameSpeedRatio = 1f;

	public bool IsMenuHidden;

	[NonSerialized]
	public EBattleMenu BattleMenu;

	private long _playerId;

	private int _chapterIndex;

	private int _dungeonIndex;

	private bool _isScenario;

	private NShared.DungeonDifficultyType _difficulty;

	private BattleType _battleType;

	private EMenuHideFlag _menuHideFlag;

	private int _worldMapEventIndex;

	private bool _isCanRecvPauseEvent = true;

	private DateTime _pausedTime;

	[NonSerialized]
	private EBattleMenu _menuType;

	private float _battleSpeed;

	private float _showTooltipGameSpeedRatio = 0.3f;

	private float _autoFastGameSpeedRatio = 1.5f;

	private float _basicGameSpeedRatio = 1f;

	private int _gameSpeedRatio = 1;

	private object _autoGameTimeScaler = new object();

	private BattleInstanceExtraData _extraData;

	private CampaignDungeonData _dungeonData;

	private CampaignTacticData _tacticData;

	private BattleDefineData _battleDefineData;

	private bool _usePunishmentBossStatus;

	private bool _lastSkillReserveAuto;

	private IncreaseDic<int, SkillAutoReserveData> _skillAutoReserveDic = new IncreaseDic<int, SkillAutoReserveData>();

	private int _highestTpsUid;

	private int _selectedCreaureUID;

	private GetCreatureStat _getSelectedCreatureStat;

	private GetCreatureState _getSelectedCreatureState;

	private GetCreatureOperation _getSelectedCreatureOperation;

	public bool CampaignEnd { get; set; }

	public DateTime PausedTime => _pausedTime;

	private bool CheckShowPunishmentRaidStateBattlePanel
	{
		get
		{
			if (_battleType != BattleType.PunishmentRaid)
			{
				return false;
			}
			RaidData raidDataByDungeon = GameRoot.Instance.RaidDataContainer.GetRaidDataByDungeon(_chapterIndex, _dungeonIndex);
			if (raidDataByDungeon == null)
			{
				return false;
			}
			return GameRoot.Instance.PunishmentRaidDataContainer.GetData(raidDataByDungeon.Index, raidDataByDungeon.Level)?.IsBoss ?? false;
		}
	}

	public static Creature GetEmoticonCreature(long accountId)
	{
		if (!SingletonAuto<BattleFacade>.isValidInstance)
		{
			return null;
		}
		Creature creature = null;
		for (int i = 0; i < SingletonAuto<BattleFacade>.instance.GetCreatureCount(); i++)
		{
			BattleFacade.CreatureCacheData creatureCacheByIndex = SingletonAuto<BattleFacade>.instance.GetCreatureCacheByIndex(i);
			if (creatureCacheByIndex.Uid != 0 && creatureCacheByIndex.Player.Id == accountId && !creatureCacheByIndex.IsDead)
			{
				Creature creature2 = BattleViewGenericSingleton<CreatureManager>.instance.GetCreature(creatureCacheByIndex.Uid);
				if (creature == null)
				{
					creature = creature2;
				}
				else if (creature.CreatureTableData.PositionType < creature2.CreatureTableData.PositionType)
				{
					creature = creature2;
				}
				else if (creature.CreatureTableData.PositionType == creature2.CreatureTableData.PositionType && creature.CreatureTableData.PositionWeight < creature.CreatureTableData.PositionWeight)
				{
					creature = creature2;
				}
				else if (creature.CreatureTableData.PositionType == creature2.CreatureTableData.PositionType && creature.CreatureTableData.PositionWeight == creature.CreatureTableData.PositionWeight && creature.CreatureTableData.Index > creature2.CreatureTableData.Index)
				{
					creature = creature2;
				}
			}
		}
		return creature;
	}

	private void OnApplicationPause(bool pauseStatus)
	{
		if ((!GlobalAutoBehaviour<LoadingManager>.isValidInstance || !GlobalAutoBehaviour<LoadingManager>.instance.IsSceneLoading) && !SingletonUpdater<TutorialManager>.instance.IsProgressing && (!SceneAutoBehaviour<CutSceneManager>.isValidInstance || !SceneAutoBehaviour<CutSceneManager>.instance.IsPlaying) && pauseStatus)
		{
			OpenPausedMenu();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		AddInstance<BossIntroWindow>("Resources/Prefab2/Window/Battle/BossIntroWindow");
		AddInstance<LootingStatus>("Resources/Prefab2/Window/Battle/LootingStatus");
		AddInstance<TimeStatus>("Resources/Prefab2/Window/Battle/TimeStatus");
		AddInstance<HeroParty>("Resources/Prefab2/Window/Battle/HeroParty");
		AddInstance<DamageMeter>("Resources/Prefab2/Window/Battle/DamageMeter");
		AddInstance<BossStatus>("Resources/Prefab2/Window/Battle/BossStatus");
		AddInstance<PunishmentRaidBossStatus>("Resources/Prefab2/Window/Battle/PunishmentRaidBossStatus");
		AddInstance<PatternAlarm>("Resources/Prefab2/Window/Battle/PatternAlarm");
		AddInstance<SystemButtonAuto>("Resources/Prefab2/Window/Battle/SystemButtonAuto");
		AddInstance<SystemButtonPause>("Resources/Prefab2/Window/Battle/SystemButtonPause");
		AddInstance<SystemButtonExit>("Resources/Prefab2/Window/Battle/SystemButtonExit");
		AddInstance<SystemButtonExitSmall>("Resources/Prefab2/Window/Battle/SystemButtonExitSmall");
		AddInstance<SystemButtonPartyExit>("Resources/Prefab2/Window/Battle/SystemButtonPartyExit");
		AddInstance<ReplayExitButton>("Resources/Prefab2/Window/Battle/ReplayExitButton");
		AddInstance<PausedMenu>("Resources/Prefab2/Window/Battle/PausedMenu");
		AddInstance<QuestStatus>("Resources/Prefab2/Window/Battle/QuestStatus");
		AddInstance<WaveStatus>("Resources/Prefab2/Window/Battle/WaveStatus");
		AddInstance<DeckStatus>("Resources/Prefab2/Window/Battle/DeckStatus");
		AddInstance<BossNameTag>("Resources/Prefab2/Window/Battle/BossNameTag");
		AddInstance<ScreenFilterWindow>("Resources/Prefab2/Window/ScreenFilterWindow");
		AddInstance<BoosterStatus>("Resources/Prefab2/Window/Battle/BoosterStatus");
		AddInstance<TacticStatus>("Resources/Prefab2/Window/Battle/TacticStatus");
		AddInstance<BattleEmoti>("Resources/Prefab2/Window/Battle/BattleEmoti");
		AddInstance<GuildRaidScoreStatus>("Resources/Prefab2/Window/Battle/GuildRaidScoreStatus");
		AddInstance<BossBuffStatus>("Resources/Prefab2/Window/Battle/BossBuffStatus");
		AddInstance<WorldBossStatus>("Resources/Prefab2/Window/Battle/WorldBossStatus");
		AddInstance<BattleChat>("Resources/Prefab2/Window/Battle/BattleChat");
		AddInstance<SystemButtonAutoSimple>("Resources/Prefab2/Window/Battle/SystemButtonAutoSimple");
		AddInstance<SystemButtonAutoFastContinue>("Resources/Prefab2/Window/Battle/SystemButtonAutoFastContinue");
		AddInstance<BattleLeave>("Resources/Prefab2/Window/Battle/BattleLeave");
		AddInstance<ChallengeRaidScoreStatus>("Resources/Prefab2/Window/Battle/ChallengeRaidScoreStatus");
		AddInstance<EmoticonPopupWindow>("Resources/Prefab2/Window/Battle/EmoticonPopupWindow");
		AddInstance<PvpGiveUpMenu>("Resources/Prefab2/Window/Battle/PvpGiveUpMenu");
		AddInstance<SystemButtonPvpGiveUp>("Resources/Prefab2/Window/Battle/SystemButtonPvpGiveUp");
		AddInstance<EclipsePausedMenu>("Resources/Prefab2/Window/Battle/EclipsePausedMenu");
		AddInstance<BattleCameraSwitch>("Resources/Prefab2/Window/Battle/BattleCameraSwitch");
		AddInstance<DungeonSkillWindow>("Resources/Prefab2/Window/Battle/DungeonSkillWindow");
		AddInstance<SystemButtonSpeed>("Resources/Prefab2/Window/Battle/SystemButtonSpeed");
		AddInstance<PunishmentRaidStateBattlePanel>("Resources/Prefab2/Window/Battle/PunishmentRaidStateBattlePanel");
		AddInstance<KillMeter>("Resources/Prefab2/Window/Battle/KillMeter");
		AddInstance<PunishmentShardGaugeWindow>(PunishmentShardGaugeWindow.PrefabPath);
		AddInstance<KarmaCardSelectWindow>(KarmaCardSelectWindow.PrefabPath);
		if (AccountManager.IsLogined())
		{
			GlobalAutoBehaviour<AccountManager>.instance.UserManager.ObserverManager.AddObserver(this);
		}
		GlobalAutoBehaviour<PartyManager>.instance.AddNewPartyListener(this);
		if (GlobalAutoBehaviour<PartyManager>.isValidInstance && GlobalAutoBehaviour<PartyManager>.instance.IsValidParty())
		{
			GlobalAutoBehaviour<PartyManager>.instance.AddPartyListener(this);
		}
		SceneAutoBehaviour<ChattingManagement>.CreateInstance();
		GlobalAutoBehaviour<ChattingManager>.instance.AddChatListener(this);
	}

	protected override void OnDestroy()
	{
		if (!SystemStatus.IsApplicationQuit)
		{
			if (AccountManager.IsLogined())
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.ObserverManager.RemoveObserver(this);
			}
			if (GlobalAutoBehaviour<PartyManager>.isValidInstance)
			{
				GlobalAutoBehaviour<PartyManager>.instance.RemovePartyListener(this);
			}
			GlobalAutoBehaviour<PartyManager>.instance.RemoveNewPartyListener(this);
			GlobalAutoBehaviour<ChattingManager>.instance.RemoveChatListener(this);
			base.OnDestroy();
		}
	}

	private void Start()
	{
		SceneAutoBehaviour<NVespa.NScene.GuiManager>.instance.Add("BattleManagement", this, OnDrawGUI);
	}

	protected override void ChainUpdate()
	{
		base.ChainUpdate();
		UpdateAutoSkill();
		UpdateSlide();
		UpdateAutoGameSpeed();
		UpdateHighestTps();
	}

	private void UpdateHighestTps()
	{
	}

	private void UpdateAutoSkill()
	{
		if (IsBattleEnabled())
		{
			if (IsOpened<SystemButtonAuto>())
			{
				UpdateAutoSkill(GetInstance<SystemButtonAuto>());
			}
			if (IsOpened<SystemButtonAutoSimple>())
			{
				UpdateAutoSkill(GetInstance<SystemButtonAutoSimple>());
			}
			if (IsOpened<SystemButtonAutoFastContinue>())
			{
				UpdateAutoSkill(GetInstance<SystemButtonAutoFastContinue>());
			}
		}
	}

	private void UpdateAutoSkillEach()
	{
		HeroParty heroParty = GetInstance<HeroParty>();
		if (heroParty == null)
		{
			return;
		}
		IncreaseDic<int, PartySlotComponent2> partySlotDic = heroParty.PartySlotDic;
		int num = partySlotDic?.Count ?? 0;
		for (int i = 0; i < num; i++)
		{
			PartySlotComponent2 valueByIndex = partySlotDic.GetValueByIndex(i);
			if (!(valueByIndex == null) && valueByIndex.IsAuto)
			{
				BattleFacade.CreatureCacheData creatureCache = SingletonAuto<BattleFacade>.instance.GetCreatureCache(valueByIndex.CreatureUid);
				UpdateSkillAutoReserve(creatureCache);
			}
		}
		if (!heroParty.EnabledSwapParty)
		{
			return;
		}
		IncreaseDic<int, PartySlotComponent2> subPartySlotDic = heroParty.SubPartySlotDic;
		int num2 = subPartySlotDic?.Count ?? 0;
		for (int j = 0; j < num2; j++)
		{
			PartySlotComponent2 valueByIndex2 = subPartySlotDic.GetValueByIndex(j);
			if (!(valueByIndex2 == null) && valueByIndex2.IsAuto)
			{
				BattleFacade.CreatureCacheData creatureCache2 = SingletonAuto<BattleFacade>.instance.GetCreatureCache(valueByIndex2.CreatureUid);
				UpdateSkillAutoReserve(creatureCache2);
			}
		}
	}

	private void UpdateAutoSkill(IAutoBattle autoBattle)
	{
		if (autoBattle.SkillReserveAuto)
		{
			UpdateSkillAutoReserve();
		}
		else
		{
			UpdateAutoSkillEach();
		}
		if (_lastSkillReserveAuto != autoBattle.SkillReserveAuto)
		{
			_lastSkillReserveAuto = autoBattle.SkillReserveAuto;
			if (!_lastSkillReserveAuto)
			{
				ClearAllReserveOnSkill();
			}
		}
	}

	public void OnKillCreature(Creature attacker, Creature deadCreature)
	{
	}

	private void UpdateSlide()
	{
	}

	public void OnEventCamera(EventCamera.EventValue current)
	{
		float uiAlpha = current.UiAlpha;
		if (IsOpened<BossIntroWindow>())
		{
			GetInstance<BossIntroWindow>().SetEventHide(uiAlpha);
		}
		if (IsOpened<LootingStatus>())
		{
			GetInstance<LootingStatus>().SetEventHide(uiAlpha);
		}
		if (IsOpened<TimeStatus>())
		{
			GetInstance<TimeStatus>().SetEventHide(uiAlpha);
		}
		if (IsOpened<HeroParty>())
		{
			GetInstance<HeroParty>().SetEventHide(uiAlpha);
		}
		if (IsOpened<DamageMeter>())
		{
			GetInstance<DamageMeter>().SetEventHide(uiAlpha);
		}
		if (IsOpened<KillMeter>())
		{
			GetInstance<KillMeter>().SetEventHide(uiAlpha);
		}
		if (IsOpened<BossStatus>())
		{
			GetInstance<BossStatus>().SetEventHide(uiAlpha);
		}
		if (IsOpened<PunishmentRaidBossStatus>())
		{
			GetInstance<PunishmentRaidBossStatus>().SetEventHide(uiAlpha);
		}
		if (IsOpened<PatternAlarm>())
		{
			GetInstance<PatternAlarm>().SetEventHide(uiAlpha);
		}
		if (IsOpened<SystemButtonAuto>())
		{
			GetInstance<SystemButtonAuto>().SetEventHide(uiAlpha);
		}
		if (IsOpened<SystemButtonPause>())
		{
			GetInstance<SystemButtonPause>().SetEventHide(uiAlpha);
		}
		if (IsOpened<QuestStatus>())
		{
			GetInstance<QuestStatus>().SetEventHide(uiAlpha);
		}
		if (IsOpened<WaveStatus>())
		{
			GetInstance<WaveStatus>().SetEventHide(uiAlpha);
		}
		if (IsOpened<DeckStatus>())
		{
			GetInstance<DeckStatus>().SetEventHide(uiAlpha);
		}
		if (IsOpened<BoosterStatus>())
		{
			GetInstance<BoosterStatus>().SetEventHide(uiAlpha);
		}
		if (IsOpened<TacticStatus>())
		{
			GetInstance<TacticStatus>().SetEventHide(uiAlpha);
		}
		if (IsOpened<BattleEmoti>())
		{
			GetInstance<BattleEmoti>().SetEventHide(uiAlpha);
		}
		if (IsOpened<GuildRaidScoreStatus>())
		{
			GetInstance<GuildRaidScoreStatus>().SetEventHide(uiAlpha);
		}
		if (IsOpened<BattleChat>())
		{
			GetInstance<BattleChat>().SetEventHide(uiAlpha);
		}
		if (IsOpened<SystemButtonAutoSimple>())
		{
			GetInstance<SystemButtonAutoSimple>().SetEventHide(uiAlpha);
		}
		if (IsOpened<SystemButtonAutoFastContinue>())
		{
			GetInstance<SystemButtonAutoFastContinue>().SetEventHide(uiAlpha);
		}
		if (IsOpened<BattleLeave>())
		{
			GetInstance<BattleLeave>().SetEventHide(uiAlpha);
		}
		if (IsOpened<SystemButtonPartyExit>())
		{
			GetInstance<SystemButtonPartyExit>().SetEventHide(uiAlpha);
		}
		if (IsOpened<ChallengeRaidScoreStatus>())
		{
			GetInstance<ChallengeRaidScoreStatus>().SetEventHide(uiAlpha);
		}
		if (IsOpened<EmoticonPopupWindow>())
		{
			GetInstance<EmoticonPopupWindow>().SetEventHide(uiAlpha);
			if (!SceneAutoBehaviour<ChattingManagement>.isValidInstance || !SceneAutoBehaviour<ChattingManagement>.instance.IsOpened<ChattingWindow>())
			{
				ChattingEmoticonSelectPopup.SetEventAlpha(uiAlpha);
			}
		}
		if (IsOpened<SystemButtonPvpGiveUp>())
		{
			GetInstance<SystemButtonPvpGiveUp>().SetEventHide(uiAlpha);
		}
		if (SceneAutoBehaviour<ChattingManagement>.isValidInstance)
		{
			ChattingWindow chattingWindow = SceneAutoBehaviour<ChattingManagement>.instance.GetInstance<ChattingWindow>(openedOnly: true);
			if (chattingWindow != null && chattingWindow.IsOpen())
			{
				chattingWindow.MakeTopmostDepth();
			}
		}
		if (IsOpened<SystemButtonExitSmall>())
		{
			GetInstance<SystemButtonExitSmall>().SetEventHide(uiAlpha);
		}
		if (IsOpened<EclipsePausedMenu>())
		{
			GetInstance<EclipsePausedMenu>().SetEventHide(uiAlpha);
		}
		if (IsOpened<BattleCameraSwitch>())
		{
			GetInstance<BattleCameraSwitch>().SetEventHide(uiAlpha);
		}
		if (IsOpened<DungeonSkillWindow>())
		{
			GetInstance<DungeonSkillWindow>().SetEventHide(uiAlpha);
		}
		if (IsOpened<PunishmentRaidStateBattlePanel>())
		{
			GetInstance<PunishmentRaidStateBattlePanel>().SetEventHide(uiAlpha);
		}
		if (IsOpened<PunishmentShardGaugeWindow>())
		{
			GetInstance<PunishmentShardGaugeWindow>().SetEventHide(uiAlpha);
		}
		if (IsOpened<KarmaCardSelectWindow>())
		{
			GetInstance<KarmaCardSelectWindow>().SetEventHide(uiAlpha);
		}
	}

	public void ClearAllReserveOnSkill()
	{
		for (int i = 0; i < SingletonAuto<BattleFacade>.instance.GetCreatureCount(); i++)
		{
			BattleFacade.CreatureCacheData creatureCacheByIndex = SingletonAuto<BattleFacade>.instance.GetCreatureCacheByIndex(i);
			ClearReserveOnSkill(creatureCacheByIndex);
		}
	}

	private void ClearReserveOnSkill(BattleFacade.CreatureCacheData creatureCache)
	{
		if (creatureCache.Uid != 0 && creatureCache.Player.Id == _playerId)
		{
			if (creatureCache.ReservedSkillIndex > 0)
			{
				SingletonAuto<BattleFacade>.instance.BattleLogRaiser.ReserveSkill(creatureCache.Uid, -1, 0, autoReserved: false);
			}
			SkillAutoReserveData vALUE = _skillAutoReserveDic.GETVALUE(creatureCache.Uid, null);
			if (vALUE != null)
			{
				vALUE.ElapsedTime = 0f;
			}
		}
	}

	public void UpdateSkillAutoReserve(BattleFacade.CreatureCacheData creatureCache)
	{
		if (creatureCache.Uid == 0 || creatureCache.Player.Id != _playerId || creatureCache.CreateInfo.IsDummy)
		{
			return;
		}
		SkillAutoReserveData skillAutoReserveData = _skillAutoReserveDic.GETVALUE(creatureCache.Uid, null);
		if (skillAutoReserveData == null)
		{
			skillAutoReserveData = new SkillAutoReserveData(creatureCache);
			_skillAutoReserveDic.Add(creatureCache.Uid, skillAutoReserveData);
		}
		if (__Extension_Method_ICollection__.IsNullOrEmpty(creatureCache.SuppressedUsingCheckSkillIndices))
		{
			int usingFiredSkillIndex = creatureCache.UsingFiredSkillIndex;
			if (creatureCache.CreatureData.IsActiveSkill(usingFiredSkillIndex))
			{
				skillAutoReserveData.ElapsedTime = 0f;
				return;
			}
		}
		skillAutoReserveData.ElapsedTime += GlobalTimer.deltaTime;
		if (skillAutoReserveData.ElapsedTime < 0.5f)
		{
			return;
		}
		int[] cooltimeSkillIndices = creatureCache.CooltimeSkillIndices;
		int[] disabledAutoUseSkillIndices = creatureCache.DisabledAutoUseSkillIndices;
		int[] suppressedUsingCheckSkillIndices = creatureCache.SuppressedUsingCheckSkillIndices;
		PoolingList<int> poolingList = default(PoolingList<int>);
		for (int i = 0; i < skillAutoReserveData.SkillList.Count; i++)
		{
			int item = skillAutoReserveData.SkillList[i].Item1;
			int skillIndexBySlot = creatureCache.CreatureData.GetSkillIndexBySlot(item);
			if ((disabledAutoUseSkillIndices != null && TypeIterator.Contains(disabledAutoUseSkillIndices, skillIndexBySlot)) || (suppressedUsingCheckSkillIndices != null && !TypeIterator.Contains(suppressedUsingCheckSkillIndices, skillIndexBySlot)))
			{
				continue;
			}
			if (cooltimeSkillIndices != null)
			{
				bool flag = false;
				for (int j = 0; j < cooltimeSkillIndices.Length; j++)
				{
					if (cooltimeSkillIndices[j] == creatureCache.GetChangedSkillndex(skillIndexBySlot))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					continue;
				}
			}
			poolingList.Add(item);
		}
		if (creatureCache.IsEquippedSoulWeapon && creatureCache.IsUseableSoulWeaponSkill)
		{
			poolingList.Add(10);
		}
		int reservedSkillIndex = creatureCache.ReservedSkillIndex;
		if (reservedSkillIndex > 0)
		{
			if (creatureCache.AutoReservedSkill)
			{
				int changedSkillndex = creatureCache.GetChangedSkillndex(reservedSkillIndex);
				bool num = cooltimeSkillIndices != null && TypeIterator.Contains(cooltimeSkillIndices, changedSkillndex);
				bool flag2 = disabledAutoUseSkillIndices != null && TypeIterator.Contains(disabledAutoUseSkillIndices, changedSkillndex);
				bool flag3 = creatureCache.IsReservedSoulWeaponSkill && !creatureCache.IsUseableSoulWeaponSkill;
				if ((num || flag2 || flag3) && !poolingList.IsNullOrEmpty())
				{
					SingletonAuto<BattleFacade>.instance.BattleLogRaiser.ReserveSkill(creatureCache.Uid, -1, 0, autoReserved: false);
					skillAutoReserveData.ElapsedTime = 0f;
				}
			}
			poolingList.Clear();
		}
		else
		{
			if (!poolingList.IsNullOrEmpty())
			{
				int skillSlotIndex = poolingList[UnityEngine.Random.Range(0, poolingList.Count)];
				SingletonAuto<BattleFacade>.instance.BattleLogRaiser.ReserveSkill(creatureCache.Uid, skillSlotIndex, 0, autoReserved: true);
			}
			poolingList.Clear();
			skillAutoReserveData.ElapsedTime = 0f;
		}
	}

	public void UpdateSkillAutoReserve()
	{
		for (int i = 0; i < SingletonAuto<BattleFacade>.instance.GetCreatureCount(); i++)
		{
			BattleFacade.CreatureCacheData creatureCacheByIndex = SingletonAuto<BattleFacade>.instance.GetCreatureCacheByIndex(i);
			if (creatureCacheByIndex.Uid != 0 && creatureCacheByIndex.Player.Id == _playerId && !creatureCacheByIndex.CreateInfo.IsDummy && !creatureCacheByIndex.CreateInfo.IsAi)
			{
				UpdateSkillAutoReserve(creatureCacheByIndex);
			}
		}
	}

	private SkillData GetNextSkillData(BattleFacade.CreatureCacheData creatureCache, int skillSlotIndex)
	{
		if (creatureCache == null || creatureCache.Uid == 0)
		{
			return null;
		}
		int usingSkillIndex = creatureCache.UsingSkillIndex;
		if (usingSkillIndex == 0)
		{
			return null;
		}
		SkillData data = GameRoot.Instance.SkillDataContainer.GetData(usingSkillIndex);
		if (data == null || data.NextIndex == 0)
		{
			return null;
		}
		int usingFiredSkillIndex = creatureCache.UsingFiredSkillIndex;
		int skillIndexBySlot = creatureCache.CreatureData.GetSkillIndexBySlot(skillSlotIndex);
		if (skillIndexBySlot != usingFiredSkillIndex)
		{
			SkillData data2 = GameRoot.Instance.SkillDataContainer.GetData(skillIndexBySlot);
			if (data2 == null)
			{
				return null;
			}
			if (!data2.HasTriggerFlag(SkillTriggerFlag.NoNextActiveSkillCheck))
			{
				return null;
			}
		}
		SkillData data3 = GameRoot.Instance.SkillDataContainer.GetData(data.NextIndex);
		if (data3 == null || data3.TriggerType != SkillTriggerType.NextActiveSkill)
		{
			return null;
		}
		return data3;
	}

	public void OnClickSkillSlot(int creatureUid, int skillSlotIndex)
	{
		BattleFacade.CreatureCacheData creatureCache = SingletonAuto<BattleFacade>.instance.GetCreatureCache(creatureUid);
		if (creatureCache.Uid == 0)
		{
			return;
		}
		int skillIndexBySlot = creatureCache.CreatureData.GetSkillIndexBySlot(skillSlotIndex);
		if (skillIndexBySlot == 0)
		{
			return;
		}
		if (GetNextSkillData(creatureCache, skillSlotIndex) != null)
		{
			SingletonAuto<BattleFacade>.instance.BattleLogRaiser.FireNextSkill(creatureUid);
			return;
		}
		if (skillSlotIndex != 10)
		{
			if (creatureCache.ReservedSkillIndex > 0 && creatureCache.ReservedSkillIndex == skillIndexBySlot)
			{
				SingletonAuto<BattleFacade>.instance.BattleLogRaiser.ReserveSkill(creatureUid, -1, 0, autoReserved: false);
				return;
			}
		}
		else if (creatureCache.IsReservedSoulWeaponSkill)
		{
			SingletonAuto<BattleFacade>.instance.BattleLogRaiser.ReserveSkill(creatureUid, -10, 0, autoReserved: false);
			return;
		}
		if (!creatureCache.IsDead)
		{
			SoundStock stock = new SoundStock
			{
				Code = "UI.Battle.UsingSkill",
				LocalTimer = GlobalTimer.DefaultLocal
			};
			SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 10f);
			SingletonAuto<BattleFacade>.instance.BattleLogRaiser.ReserveSkill(creatureUid, skillSlotIndex, 0, autoReserved: false);
		}
	}

	public void ClosePauseMenu()
	{
		CloseInstance<PausedMenu>();
	}

	public void CloseEclipsePausedMenu()
	{
		CloseInstance<EclipsePausedMenu>();
	}

	public void CloseMenu(EBattleMenu menu)
	{
		BattleMenu &= ~menu;
		switch (menu)
		{
		case EBattleMenu.SystemButton:
			CloseInstance<SystemButtonPause>();
			break;
		case EBattleMenu.SystemButtonAuto:
			CloseInstance<SystemButtonAuto>();
			break;
		case EBattleMenu.SystemButtonAutoSimple:
			CloseInstance<SystemButtonAutoSimple>();
			break;
		case EBattleMenu.SystemButtonAutoFastContinue:
			CloseInstance<SystemButtonAutoFastContinue>();
			break;
		case EBattleMenu.SystemButtonSpeed:
			CloseInstance<SystemButtonSpeed>();
			break;
		case EBattleMenu.TimeStatus:
			CloseInstance<TimeStatus>();
			break;
		case EBattleMenu.LootingStatus:
			CloseInstance<LootingStatus>();
			break;
		case EBattleMenu.WaveStatus:
			CloseInstance<WaveStatus>();
			break;
		case EBattleMenu.BoosterStatus:
			CloseInstance<BoosterStatus>();
			break;
		case EBattleMenu.QuestStatus:
			CloseInstance<QuestStatus>();
			break;
		case EBattleMenu.Chat:
			CloseInstance<BattleChat>();
			break;
		case EBattleMenu.Leave:
			CloseInstance<BattleLeave>();
			break;
		case EBattleMenu.GuildRaidScoreStatus:
			CloseInstance<GuildRaidScoreStatus>();
			break;
		case EBattleMenu.ChallengeRaidScorestatus:
			CloseInstance<ChallengeRaidScoreStatus>();
			break;
		case EBattleMenu.PvpGiveUp:
			CloseInstance<SystemButtonPvpGiveUp>();
			break;
		case EBattleMenu.ExitSmallButton:
			CloseInstance<SystemButtonExitSmall>();
			break;
		case EBattleMenu.ShardGauge:
			CloseInstance<PunishmentShardGaugeWindow>();
			break;
		case EBattleMenu.CardPick:
			CloseInstance<KarmaCardSelectWindow>();
			break;
		}
		if (CheckShowPunishmentRaidStateBattlePanel)
		{
			CloseInstance<PunishmentRaidStateBattlePanel>();
		}
	}

	private void CheckAutoPlayable(int chapterIndex, int dungeonIndex, NShared.DungeonDifficultyType difficulty, BattleType battleType, bool isScenario, out bool autoPlayable, out bool fastSpeedAppliable)
	{
		autoPlayable = fastSpeedAppliable = false;
		if (SprkConquest.ArenaAutoPlayable(battleType, ref autoPlayable, ref fastSpeedAppliable))
		{
			return;
		}
		autoPlayable = (fastSpeedAppliable = false);
		switch (battleType)
		{
		case BattleType.UnderPrison:
			if (GlobalAutoBehaviour<AccountManager>.isValidInstance && GlobalAutoBehaviour<AccountManager>.instance.UnderPrisonManager != null)
			{
				GlobalAutoBehaviour<AccountManager>.instance.UnderPrisonManager.Get(chapterIndex);
				autoPlayable = true;
				fastSpeedAppliable = true;
			}
			break;
		case BattleType.Arena:
		case BattleType.Raid:
		case BattleType.HardRaid:
		case BattleType.ChallengeRaid:
		case BattleType.GuildArena:
		case BattleType.PunishmentRaid:
			autoPlayable = true;
			fastSpeedAppliable = false;
			break;
		default:
			if (_battleDefineData != null)
			{
				int completeStar = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.GetCompleteStar(chapterIndex, dungeonIndex, difficulty);
				autoPlayable = completeStar >= _battleDefineData.AutoPlayStarCount;
				fastSpeedAppliable = completeStar >= _battleDefineData.FastSpeedStarCount;
			}
			break;
		}
	}

	private bool GetSpeedEnable()
	{
		if (IsOpened<SystemButtonAuto>())
		{
			return GetInstance<SystemButtonAuto>().FastSpeed;
		}
		if (IsOpened<SystemButtonSpeed>())
		{
			return GetInstance<SystemButtonSpeed>().FastSpeed;
		}
		if (IsOpened<SystemButtonAutoFastContinue>())
		{
			return GetInstance<SystemButtonAutoFastContinue>().FastSpeed;
		}
		return false;
	}

	private bool IsEnableAutoSpeedFast()
	{
		if (!GetSpeedEnable())
		{
			return false;
		}
		if (SceneAutoBehaviour<CutSceneManager>.isValidInstance && SceneAutoBehaviour<CutSceneManager>.instance.IsPlaying)
		{
			return false;
		}
		if (SingletonAuto<BattleFacade>.isValidInstance && SingletonAuto<BattleFacade>.instance.GetCampaignCache().IsBattleEnd)
		{
			return false;
		}
		return true;
	}

	private bool IsEnableShowTooltipSpeedSlow()
	{
		if (_battleDefineData == null)
		{
			return false;
		}
		if (_battleDefineData.FastSkillTooltip)
		{
			return false;
		}
		HeroParty heroParty = GetInstance<HeroParty>();
		DungeonSkillWindow dungeonSkillWindow = GetInstance<DungeonSkillWindow>(openedOnly: true);
		if (!heroParty.IsShowingTooltip())
		{
			if (dungeonSkillWindow != null)
			{
				return dungeonSkillWindow.IsShowingTooltip();
			}
			return false;
		}
		return true;
	}

	private float GetGameSpeedRatio()
	{
		if (IsOnline() || _battleType == BattleType.Eclipse)
		{
			return SprkArenaPractice.Speed(_gameSpeedRatio);
		}
		if (IsEnableShowTooltipSpeedSlow())
		{
			return SprkArenaPractice.Speed(_showTooltipGameSpeedRatio);
		}
		if (IsEnableAutoSpeedFast())
		{
			return SprkArenaPractice.Speed(_autoFastGameSpeedRatio);
		}
		return SprkArenaPractice.Speed(_basicGameSpeedRatio);
	}

	private void UpdateAutoGameSpeed()
	{
		float gameSpeedRatio = GetGameSpeedRatio();
		UnityTimeScaler.SetScale(_autoGameTimeScaler, gameSpeedRatio);
		SetBattleSpeedChangeTrigger(gameSpeedRatio);
	}

	public void UpdateGameSpeed(float speedRate)
	{
		UnityTimeScaler.SetScale(_autoGameTimeScaler, speedRate);
	}

	private void SetBattleSpeedChangeTrigger(float speed)
	{
		if (!Mathf.Approximately(_battleSpeed, speed))
		{
			BattleFacade.BattleCacheData battleCache = SingletonAuto<BattleFacade>.instance.GetBattleCache();
			if (battleCache != null)
			{
				IEvent triggerEvent = new BattleSpeedChange
				{
					BattleGlobalTime = battleCache.GlobalCurrentTimeMs,
					SpeedRate = speed
				};
				_battleSpeed = speed;
				SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent);
			}
		}
	}

	private List<BaseWindow> GetOpenedMenuList()
	{
		List<BaseWindow> list = new List<BaseWindow>();
		if (IsOpened<TimeStatus>())
		{
			list.Add(GetInstance<TimeStatus>());
		}
		if (IsOpened<SystemButtonAuto>())
		{
			list.Add(GetInstance<SystemButtonAuto>());
		}
		if (IsOpened<SystemButtonAutoSimple>())
		{
			list.Add(GetInstance<SystemButtonAutoSimple>());
		}
		if (IsOpened<SystemButtonAutoFastContinue>())
		{
			list.Add(GetInstance<SystemButtonAutoFastContinue>());
		}
		if (IsOpened<SystemButtonPause>())
		{
			list.Add(GetInstance<SystemButtonPause>());
		}
		if (IsOpened<LootingStatus>())
		{
			list.Add(GetInstance<LootingStatus>());
		}
		if (IsOpened<WaveStatus>())
		{
			list.Add(GetInstance<WaveStatus>());
		}
		if (IsOpened<DeckStatus>())
		{
			list.Add(GetInstance<DeckStatus>());
		}
		if (IsOpened<BoosterStatus>())
		{
			list.Add(GetInstance<BoosterStatus>());
		}
		if (IsOpened<QuestStatus>())
		{
			list.Add(GetInstance<QuestStatus>());
		}
		if (IsOpened<HeroParty>())
		{
			list.Add(GetInstance<HeroParty>());
		}
		if (IsOpened<BattleEmoti>())
		{
			list.Add(GetInstance<BattleEmoti>());
		}
		if (IsOpened<BattleChat>())
		{
			list.Add(GetInstance<BattleChat>());
		}
		if (IsOpened<BattleLeave>())
		{
			list.Add(GetInstance<BattleLeave>());
		}
		if (IsOpened<GuildRaidScoreStatus>())
		{
			list.Add(GetInstance<GuildRaidScoreStatus>());
		}
		if (IsOpened<SystemButtonPartyExit>())
		{
			list.Add(GetInstance<SystemButtonPartyExit>());
		}
		if (IsOpened<ChallengeRaidScoreStatus>())
		{
			list.Add(GetInstance<ChallengeRaidScoreStatus>());
		}
		if (IsOpened<EmoticonPopupWindow>())
		{
			list.Add(GetInstance<EmoticonPopupWindow>());
		}
		if (IsOpened<SystemButtonPvpGiveUp>())
		{
			list.Add(GetInstance<SystemButtonPvpGiveUp>());
		}
		if (IsOpened<SystemButtonExitSmall>())
		{
			list.Add(GetInstance<SystemButtonExitSmall>());
		}
		if (IsOpened<BattleCameraSwitch>())
		{
			list.Add(GetInstance<BattleCameraSwitch>());
		}
		if (IsOpened<DungeonSkillWindow>())
		{
			list.Add(GetInstance<DungeonSkillWindow>());
		}
		if (IsOpened<PunishmentRaidStateBattlePanel>())
		{
			list.Add(GetInstance<PunishmentRaidStateBattlePanel>());
		}
		if (IsOpened<PunishmentShardGaugeWindow>())
		{
			list.Add(GetInstance<PunishmentShardGaugeWindow>());
		}
		if (IsOpened<KarmaCardSelectWindow>())
		{
			list.Add(GetInstance<KarmaCardSelectWindow>());
		}
		if (_battleType == BattleType.Eclipse)
		{
			if (IsOpened<BossStatus>())
			{
				list.Add(GetInstance<BossStatus>());
			}
			if ((bool)GetInstance<DamageMeter>())
			{
				list.Add(GetInstance<DamageMeter>());
			}
		}
		return list;
	}

	public void OpenMenu(long playerId, int chapterIndex, int dungeonIndex, int requestDungeonIndex, bool isScenario, NShared.DungeonDifficultyType difficulty, EBattleMenu menuType, BattleInstanceExtraData extraData, int worldMapEventIndex = 0, int gameSpeedRatio = 1)
	{
		_playerId = playerId;
		_chapterIndex = chapterIndex;
		_dungeonIndex = dungeonIndex;
		_difficulty = difficulty;
		_isScenario = isScenario;
		BattleMenu = menuType;
		_worldMapEventIndex = worldMapEventIndex;
		_gameSpeedRatio = gameSpeedRatio;
		_battleSpeed = 0f;
		_menuType = menuType;
		_dungeonData = GetCampaignDungeonData(chapterIndex, dungeonIndex);
		_tacticData = GetCampaignTacticData(chapterIndex, dungeonIndex, difficulty);
		_battleDefineData = GetBattleDefineData(_dungeonData, isScenario);
		_battleType = ((_dungeonData != null) ? _dungeonData.BattleType : BattleType.None);
		_extraData = extraData;
		_usePunishmentBossStatus = GetUsePunishmentRaidBossStatus();
		OpenMenu(isFirstOpen: true);
		SetPostProcessing(_battleType);
		SetGameSpeedRatio();
		SetBattleDefineValue();
	}

	private CampaignDungeonData GetCampaignDungeonData(int chapterIndex, int dungeonIndex)
	{
		return SprkArenaPractice.CombatDungeon(GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex));
	}

	private CampaignTacticData GetCampaignTacticData(int chapterIndex, int dungeonIndex, NShared.DungeonDifficultyType difficulty)
	{
		return GameRoot.Instance.CampaignTacticDataContainer.GetData(chapterIndex, dungeonIndex, difficulty);
	}

	private BattleDefineData GetBattleDefineData(CampaignDungeonData dungeonData, bool isScenario)
	{
		return dungeonData?.GetBattleDefine(isScenario);
	}

	private void SetPostProcessing(BattleType battleType)
	{
		Game3DRoot.instance.GameCamera.SetupPostPorcessing(battleType);
	}

	private void SetGameSpeedRatio()
	{
		_showTooltipGameSpeedRatio = (float)GameRoot.Instance.ConstantDataContainer.GetValueInt("ShowTooltipGameSpeedPercentage", 30) / 100f;
		_autoFastGameSpeedRatio = (float)GameRoot.Instance.ConstantDataContainer.GetValueInt("AutoGameSpeedPercentage", 150) / 100f;
		SetBasicGameSpeedRatio(1f);
	}

	private void SetBattleDefineValue()
	{
		if (_battleDefineData != null)
		{
			SetCameraFarClipPlane(_battleDefineData.CameraFarClipPlane);
		}
	}

	private void SetCameraFarClipPlane(int cameraFarClipPlane)
	{
		Game3DRoot.instance.GameCamera.SetCameraFarClipPlane(cameraFarClipPlane);
	}

	private void SetBasicGameSpeedRatio(float gameSpeedRatio)
	{
		_basicGameSpeedRatio = gameSpeedRatio;
	}

	public void OpenMenu(bool isFirstOpen)
	{
		if (((_menuType & EBattleMenu.DamageMeter) != 0 || (_menuType & EBattleMenu.TPSDamageMeter) != 0 || (_menuType & EBattleMenu.MPSDamageMeter) != 0) && isFirstOpen)
		{
			GetInstance<DamageMeter>().Open(this, _playerId, _battleType, _battleDefineData);
		}
		if ((_menuType & EBattleMenu.TimeStatus) != 0)
		{
			GetInstance<TimeStatus>().Open(_chapterIndex, _dungeonIndex, _isScenario);
		}
		if ((_menuType & EBattleMenu.SystemButton) != 0 && !IsDisableWorldBossRestart(_chapterIndex, _dungeonIndex))
		{
			GetInstance<SystemButtonPause>().Open(this);
		}
		if ((_menuType & EBattleMenu.SystemButtonSpeed) != 0)
		{
			CheckAutoPlayable(_chapterIndex, _dungeonIndex, _difficulty, _battleType, _isScenario, out var _, out var _);
			GetInstance<SystemButtonSpeed>().Open();
		}
		bool autoPlayerable = false;
		if ((_menuType & EBattleMenu.SystemButtonAuto) != 0)
		{
			GetInstance<SystemButtonAuto>();
			CheckAutoPlayable(_chapterIndex, _dungeonIndex, _difficulty, _battleType, _isScenario, out var autoPlayable2, out var _);
			bool num = _battleType != BattleType.KarmaRaid;
			bool flag = _battleType == BattleType.KarmaRaid && isFirstOpen;
			if (num || flag)
			{
				GetInstance<SystemButtonAuto>().Open(this, _chapterIndex, _dungeonIndex, _difficulty, autoPlayable2);
			}
			autoPlayerable = autoPlayable2;
		}
		if ((_menuType & EBattleMenu.SystemButtonAutoSimple) != 0 && isFirstOpen)
		{
			CheckAutoPlayable(_chapterIndex, _dungeonIndex, _difficulty, _battleType, _isScenario, out var autoPlayable3, out var _);
			GetInstance<SystemButtonAutoSimple>().Open(this, _chapterIndex, _dungeonIndex, _difficulty, autoPlayable3);
			autoPlayerable = autoPlayable3;
		}
		if ((_menuType & EBattleMenu.SystemButtonAutoFastContinue) != 0)
		{
			SystemButtonAutoFastContinue systemButtonAutoFastContinue = GetInstance<SystemButtonAutoFastContinue>();
			if (systemButtonAutoFastContinue != null)
			{
				CheckAutoPlayable(_chapterIndex, _dungeonIndex, _difficulty, _battleType, _isScenario, out var autoPlayable4, out var _);
				bool num2 = _battleType != BattleType.Eclipse;
				bool flag2 = _battleType == BattleType.Eclipse && !systemButtonAutoFastContinue.IsInitAutoButton();
				if (num2 || flag2)
				{
					SystemButtonAutoFastContinue.SystemAutoFastContinueInfo info = new SystemButtonAutoFastContinue.SystemAutoFastContinueInfo
					{
						Listener = this,
						ChapterIndex = _chapterIndex,
						DungeonIndex = _dungeonIndex,
						Difficulty = _difficulty,
						AutoPlayerable = autoPlayable4,
						GameSpeed = GetGameSpeed(_battleType),
						MaxGameSpeed = GetMaxGameSpeed(_battleType),
						IsPossibleChangeGameSpeed = IsPossibleChangeGameSpeed(_battleType)
					};
					systemButtonAutoFastContinue.Open(info);
				}
				autoPlayerable = autoPlayable4;
			}
		}
		if ((_menuType & EBattleMenu.HeroParty) != 0 && isFirstOpen)
		{
			bool enabledSwapParty = (_menuType & EBattleMenu.SwitchCamera) != 0;
			GetInstance<HeroParty>().Open(_playerId, OnClickSkillSlot, autoPlayerable, this, _chapterIndex, _dungeonIndex, enabledSwapParty);
			if ((_menuType & EBattleMenu.MPSDamageMeter) != 0)
			{
				GetInstance<HeroParty>().SetManaAddict(value: true);
			}
		}
		if ((_menuType & EBattleMenu.LootingStatus) != 0)
		{
			GetInstance<LootingStatus>().Open();
		}
		if ((_menuType & EBattleMenu.WaveStatus) != 0)
		{
			GetInstance<WaveStatus>().Open(_chapterIndex, _dungeonIndex, _difficulty, _isScenario, CheckOpenBossStatus, GetCurrentBossType);
		}
		if ((_menuType & EBattleMenu.DeckStatus) != 0)
		{
			GetInstance<DeckStatus>().Open(_battleType);
		}
		if ((_menuType & EBattleMenu.QuestStatus) != 0 && AccountManager.IsLogined() && GlobalAutoBehaviour<AccountManager>.instance.WorldMapEventManager != null)
		{
			QuestDungeon eventData = GlobalAutoBehaviour<AccountManager>.instance.WorldMapEventManager.GetEventData<QuestDungeon>(_chapterIndex, _dungeonIndex);
			if (eventData != null)
			{
				GetInstance<QuestStatus>().Open(this, eventData);
			}
		}
		if ((_menuType & EBattleMenu.BoosterStatus) != 0)
		{
			GetInstance<BoosterStatus>().Init();
		}
		if ((_menuType & EBattleMenu.ExitButton) != 0)
		{
			GetInstance<SystemButtonExit>().Open(this);
		}
		if ((_menuType & EBattleMenu.ExitSmallButton) != 0)
		{
			GetInstance<SystemButtonExitSmall>().Open(this);
		}
		if ((_menuType & EBattleMenu.PartyExitButton) != 0)
		{
			GetInstance<SystemButtonPartyExit>().Open(this);
		}
		if ((_menuType & EBattleMenu.ReplayExitButton) != 0)
		{
			GetInstance<ReplayExitButton>().Open(this);
		}
		if ((_menuType & EBattleMenu.Chat) != 0)
		{
			GetInstance<BattleChat>().OpenChat();
		}
		if ((_menuType & EBattleMenu.Emoti) != 0)
		{
			GetInstance<BattleEmoti>().Open(this);
		}
		if ((_menuType & EBattleMenu.Leave) != 0)
		{
			GetInstance<BattleLeave>().Open(this);
		}
		if ((_menuType & EBattleMenu.SwitchCamera) != 0)
		{
			GetInstance<BattleCameraSwitch>().Open(this);
		}
		if ((_menuType & EBattleMenu.DungeonSkill) != 0 && _dungeonData != null && __Extension_Method_ICollection__.COUNT(_dungeonData.DungeonSkillIndexList) > 0)
		{
			GetInstance<DungeonSkillWindow>().Open(this, _battleType, OnClickDungeonSkillSlot);
		}
		if ((_menuType & EBattleMenu.KillMeter) != 0 && isFirstOpen)
		{
			GetInstance<KillMeter>().Open(TeamType.Left);
		}
		if ((_menuType & EBattleMenu.ShardGauge) != 0 && isFirstOpen && !_usePunishmentBossStatus)
		{
			GetInstance<PunishmentShardGaugeWindow>().Open(_chapterIndex, _dungeonIndex, _battleType, _extraData);
		}
		if ((_menuType & EBattleMenu.EmoticonPopup) != 0 && SystemOptionData.Show_BattleEmoticon && ((GlobalAutoBehaviour<PartyManager>.isValidInstance && GlobalAutoBehaviour<PartyManager>.instance.IsValidParty()) || _battleType == BattleType.Arena || _battleType == BattleType.ArenaBanPick))
		{
			GetInstance<EmoticonPopupWindow>().Open(_battleType);
		}
		if ((_menuType & EBattleMenu.PvpGiveUp) != EBattleMenu.None && IsOnline() && !IsFriendDuel())
		{
			GetInstance<SystemButtonPvpGiveUp>().Open(this);
		}
		if (IsOpened<TacticStatus>())
		{
			GetInstance<TacticStatus>().ClearTactic();
		}
		if (_tacticData != null && !_tacticData.BossTactic)
		{
			GetInstance<TacticStatus>().Open(_chapterIndex, _dungeonIndex, _difficulty);
		}
		if (IsOpened<BossStatus>())
		{
			GetInstance<BossStatus>().Refresh();
		}
		if (IsOpened<PunishmentRaidBossStatus>())
		{
			GetInstance<PunishmentRaidBossStatus>().Refresh();
		}
		if ((_menuType & EBattleMenu.CardPick) != 0 && isFirstOpen)
		{
			GetInstance<KarmaCardSelectWindow>().Openbase(this);
		}
	}

	private int GetGameSpeed(BattleType battleType)
	{
		if (battleType == BattleType.Eclipse)
		{
			if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
			{
				return 1;
			}
			return GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetEclipseRewardInfo()?.OnlineGameSpeedRatio ?? 1;
		}
		return 1;
	}

	private int GetMaxGameSpeed(BattleType battleType)
	{
		if (battleType == BattleType.Eclipse)
		{
			if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
			{
				return 2;
			}
			return GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.MaxGameSpeed;
		}
		return 2;
	}

	private bool IsPossibleChangeGameSpeed(BattleType battleType)
	{
		if (battleType == BattleType.Eclipse)
		{
			return false;
		}
		return true;
	}

	private bool CheckOpenBossStatus()
	{
		if (_usePunishmentBossStatus)
		{
			if (!IsOpened<PunishmentRaidBossStatus>())
			{
				return false;
			}
			if (!GetInstance<PunishmentRaidBossStatus>().IsBossValid())
			{
				return false;
			}
			return true;
		}
		if (!IsOpened<BossStatus>())
		{
			return false;
		}
		if (!GetInstance<BossStatus>().IsBossValid())
		{
			return false;
		}
		return true;
	}

	private MonsterGradeType GetCurrentBossType()
	{
		if (_usePunishmentBossStatus)
		{
			if (!IsOpened<PunishmentRaidBossStatus>())
			{
				return MonsterGradeType.Normal;
			}
			return GetInstance<PunishmentRaidBossStatus>().GetCurrentBossType();
		}
		if (!IsOpened<BossStatus>())
		{
			return MonsterGradeType.Normal;
		}
		return GetInstance<BossStatus>().GetCurrentBossType();
	}

	public void SetMenuHide(EMenuHideFlag flag, bool skipAni = false)
	{
		EMenuHideFlag menuHideFlag = _menuHideFlag;
		_menuHideFlag |= flag;
		if (menuHideFlag == (EMenuHideFlag)0 && _menuHideFlag != 0)
		{
			List<BaseWindow> openedMenuList = GetOpenedMenuList();
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(openedMenuList); i++)
			{
				BaseWindow baseWindow = openedMenuList[i];
				if (!(baseWindow == null))
				{
					baseWindow.PlayWindowCloseAnimation();
				}
			}
		}
		if (skipAni)
		{
			List<BaseWindow> openedMenuList2 = GetOpenedMenuList();
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(openedMenuList2); j++)
			{
				BaseWindow baseWindow2 = openedMenuList2[j];
				if (!(baseWindow2 == null))
				{
					baseWindow2.CompleteWindowAnimation();
				}
			}
		}
		IsMenuHidden = true;
	}

	public void ReleaseMenuHide(EMenuHideFlag flag, bool force = false)
	{
		EMenuHideFlag menuHideFlag = _menuHideFlag;
		_menuHideFlag &= ~flag;
		if ((menuHideFlag != 0 && _menuHideFlag == (EMenuHideFlag)0) || force)
		{
			List<BaseWindow> openedMenuList = GetOpenedMenuList();
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(openedMenuList); i++)
			{
				BaseWindow baseWindow = openedMenuList[i];
				if (!(baseWindow == null))
				{
					baseWindow.PlayWindowOpenAnimation();
				}
			}
		}
		IsMenuHidden = false;
	}

	public void OnBattleEnd()
	{
		if (SceneAutoBehaviour<ChattingManagement>.isValidInstance)
		{
			SceneAutoBehaviour<ChattingManagement>.instance.CloseAllActiveWindow();
		}
		if (ChattingEmoticonSelectPopup.IsOpenedWindow())
		{
			ChattingEmoticonSelectPopup.CloseWindow();
		}
		ClosePvpGiveUpMenu();
	}

	public void OpenPausedMenu()
	{
		if ((BattleMenu & EBattleMenu.SystemButton) != EBattleMenu.None && !CampaignEnd && !IsDisableWorldBossRestart(_chapterIndex, _dungeonIndex))
		{
			_pausedTime = DateTime.UtcNow;
			bool enableRetry = SingletonAuto<BattleContext>.instance.DataProvider.CanRetryBattle(BattleRetryConditionType.Lose);
			GetInstance<PausedMenu>().Open(this, enableRetry);
		}
	}

	public void OpenEclipsePausedMenu()
	{
		GetInstance<EclipsePausedMenu>().Open(this, enableRetry: true);
	}

	public void OpenPatternAlarm(Creature creature)
	{
		if (!IsOpened<PatternAlarm>())
		{
			GetInstance<PatternAlarm>().Open(creature);
		}
	}

	public void OpenBossStatus(Creature creature)
	{
		OpenTacticStatus();
		if (_battleType == BattleType.WorldBoss)
		{
			if (!IsOpened<WorldBossStatus>())
			{
				CloseInstance<WaveStatus>();
				GetInstance<WorldBossStatus>().Open(creature, _chapterIndex, _dungeonIndex, _difficulty, _battleDefineData);
			}
		}
		else if (_usePunishmentBossStatus)
		{
			if (!IsOpened<PunishmentRaidBossStatus>())
			{
				GetInstance<PunishmentRaidBossStatus>().Open(creature, _battleDefineData, showShard: true);
				if (!IsOpened<PunishmentShardGaugeWindow>())
				{
					GetInstance<PunishmentShardGaugeWindow>().Open(_chapterIndex, _dungeonIndex, _battleType, _extraData);
				}
				CloseInstance<WaveStatus>();
				if (CheckShowPunishmentRaidStateBattlePanel && !IsOpened<PunishmentRaidStateBattlePanel>())
				{
					GetInstance<PunishmentRaidStateBattlePanel>().Open(_chapterIndex, _dungeonIndex);
				}
				Transform transform = null;
				transform = ((!IsOpened<PunishmentRaidStateBattlePanel>()) ? GetInstance<PunishmentRaidBossStatus>().GetBuffStatusPoint01() : GetInstance<PunishmentRaidBossStatus>().GetBuffStatusPoint03());
				GetInstance<BossBuffStatus>().Open(creature.Uid, transform);
			}
		}
		else if (!IsOpened<BossStatus>() || _battleType == BattleType.Eclipse)
		{
			if (_battleType != BattleType.Eclipse && _battleType != BattleType.KarmaRaid)
			{
				CloseInstance<WaveStatus>();
			}
			if ((BattleMenu & EBattleMenu.GuildRaidScoreStatus) != 0 || (BattleMenu & EBattleMenu.GuildSuppressScorestatus) != 0)
			{
				GetInstance<GuildRaidScoreStatus>().Open(this, this);
			}
			if ((BattleMenu & EBattleMenu.ChallengeRaidScorestatus) != 0)
			{
				GetInstance<ChallengeRaidScoreStatus>().Open(this, this);
			}
			bool showShard = _battleType == BattleType.KarmaRaid || _battleType == BattleType.PunishmentRaid;
			GetInstance<BossStatus>().Open(creature, _battleDefineData, showShard);
			Transform parent = ((!IsOpened<GuildRaidScoreStatus>() && !IsOpened<ChallengeRaidScoreStatus>() && (_battleType != BattleType.Eclipse || !IsOpened<WaveStatus>()) && (_battleType != BattleType.KarmaRaid || !IsOpened<WaveStatus>())) ? GetInstance<BossStatus>().GetBuffStatusPoint01() : GetInstance<BossStatus>().GetBuffStatusPoint02());
			GetInstance<BossBuffStatus>().Open(creature.Uid, parent);
		}
	}

	public void OpenTacticStatus()
	{
		if (_tacticData == null)
		{
			return;
		}
		if (_tacticData.ScenarioOnly)
		{
			if (_isScenario)
			{
				GetInstance<TacticStatus>().Open(_chapterIndex, _dungeonIndex, _difficulty);
			}
		}
		else if (_tacticData.BossTactic)
		{
			GetInstance<TacticStatus>().Open(_chapterIndex, _dungeonIndex, _difficulty);
		}
	}

	public void ClosePatternAlarm()
	{
		CloseInstance<PatternAlarm>();
	}

	public void CloseBossStatus()
	{
		CloseInstance<BossStatus>();
		CloseInstance<PunishmentRaidBossStatus>();
		if (_tacticData != null && _tacticData.BossTactic)
		{
			CloseInstance<TacticStatus>();
		}
		CloseInstance<BossBuffStatus>();
		CloseInstance<WorldBossStatus>();
		CloseInstance<GuildRaidScoreStatus>();
		CloseInstance<ChallengeRaidScoreStatus>();
	}

	public void CloseShardGuageWindow()
	{
		if (_usePunishmentBossStatus)
		{
			CloseInstance<PunishmentShardGaugeWindow>();
		}
	}

	public void OpenBossNameTag(Creature creature, bool isBossTag)
	{
		if (!(creature == null))
		{
			GetInstance<BossNameTag>().Open(creature, isBossTag);
		}
	}

	public void ShowEmoticonByBattleMessage(BattleMessageNot data)
	{
		if (SystemOptionData.Show_BattleEmoticon && data != null && !String_Extension_Method.IsNullOrEmpty(data.Content))
		{
			int result = 0;
			if (int.TryParse(data.Content, out result))
			{
				ShowBattleEmoticon(data.SenderId, result);
			}
		}
	}

	public void ShowBattleEmoticon(long senderId, int emoticonIndex)
	{
		EmoticonData data = GameRoot.Instance.EmoticonDataContainer.GetData(emoticonIndex);
		if (data != null)
		{
			Creature emoticonCreature = GetEmoticonCreature(senderId);
			if (emoticonCreature != null)
			{
				emoticonCreature.OnMessageFast(data);
			}
		}
	}

	private bool CheckShowEmoticon(ChatInfo chatInfo)
	{
		if (!SystemOptionData.Show_BattleEmoticon)
		{
			return false;
		}
		if (chatInfo == null)
		{
			return false;
		}
		if (!chatInfo.IsShowBattleEmoti)
		{
			return false;
		}
		if (SingletonAuto<BattleFacade>.instance.GetPlayerCache(chatInfo.SenderId) == null)
		{
			return false;
		}
		_ = chatInfo.EmoticonIndex;
		if (chatInfo.EmoticonIndex == 0)
		{
			return false;
		}
		return true;
	}

	void IChatListener.OnChatAdded(ChatInfo chatInfo, bool showPreview, EmotiType emoticon)
	{
		if (!CheckShowEmoticon(chatInfo))
		{
			return;
		}
		int emoticonIndex = chatInfo.EmoticonIndex;
		EmoticonData data = GameRoot.Instance.EmoticonDataContainer.GetData(emoticonIndex);
		if (data != null)
		{
			Creature emoticonCreature = GetEmoticonCreature(chatInfo.SenderId);
			if (emoticonCreature != null)
			{
				emoticonCreature.OnMessageFast(data);
			}
			else
			{
				GetInstance<EmoticonPopupWindow>().ShowEmoticon(data, chatInfo.SenderName);
			}
		}
	}

	void IChatListener.OnWhisperUserAdded(long accountId, string userName)
	{
	}

	void IChatListener.OnWhisperUserRemoved(long accountId)
	{
	}

	void SystemButtonAuto.Listener.OnChangeAuto()
	{
		for (int i = 0; i < _skillAutoReserveDic.Count; i++)
		{
			_skillAutoReserveDic.GetValueByIndex(i).ElapsedTime = 0f;
		}
		SystemButtonAuto systemButtonAuto = GetInstance<SystemButtonAuto>();
		EnableAutoPartySlotComponent(systemButtonAuto.IsAuto());
	}

	void SystemButtonAutoSimple.Listener.OnChangeAuto()
	{
		for (int i = 0; i < _skillAutoReserveDic.Count; i++)
		{
			_skillAutoReserveDic.GetValueByIndex(i).ElapsedTime = 0f;
		}
		SystemButtonAutoSimple systemButtonAutoSimple = GetInstance<SystemButtonAutoSimple>();
		EnableAutoPartySlotComponent(systemButtonAutoSimple.IsAuto());
	}

	void SystemButtonAutoFastContinue.Listener.OnChangeAuto()
	{
		for (int i = 0; i < _skillAutoReserveDic.Count; i++)
		{
			_skillAutoReserveDic.GetValueByIndex(i).ElapsedTime = 0f;
		}
		SystemButtonAutoFastContinue systemButtonAutoFastContinue = GetInstance<SystemButtonAutoFastContinue>();
		EnableAutoPartySlotComponent(systemButtonAutoFastContinue.IsAuto());
	}

	private void EnableAutoPartySlotComponent(bool enable)
	{
		HeroParty heroParty = GetInstance<HeroParty>();
		if (heroParty == null)
		{
			return;
		}
		IncreaseDic<int, PartySlotComponent2> partySlotDic = heroParty.PartySlotDic;
		int num = partySlotDic?.Count ?? 0;
		for (int i = 0; i < num; i++)
		{
			PartySlotComponent2 valueByIndex = partySlotDic.GetValueByIndex(i);
			if (!(valueByIndex == null))
			{
				valueByIndex.EnableAuto(enable);
			}
		}
		if (!heroParty.EnabledSwapParty)
		{
			return;
		}
		IncreaseDic<int, PartySlotComponent2> subPartySlotDic = heroParty.SubPartySlotDic;
		int num2 = subPartySlotDic?.Count ?? 0;
		for (int j = 0; j < num2; j++)
		{
			PartySlotComponent2 valueByIndex2 = subPartySlotDic.GetValueByIndex(j);
			if (!(valueByIndex2 == null))
			{
				valueByIndex2.EnableAuto(enable);
			}
		}
	}

	void SystemButtonAutoFastContinue.Listener.RequestContinuousBattle(bool isContinuous, Action<bool> onSuccess)
	{
		if (_battleType == BattleType.Eclipse)
		{
			RequestEclipseContinuousBattle(isContinuous, onSuccess);
		}
	}

	public void RequestEclipseContinuousBattle(bool isContinuous, Action<bool> onSuccess)
	{
		if (IsOnline())
		{
			EclipseContinuousBattleReq eclipseContinuousBattleReq = new EclipseContinuousBattleReq();
			eclipseContinuousBattleReq.IsContinuousBattle = isContinuous;
			GlobalAutoBehaviour<SocketServiceManager>.instance.BattleConnector.Request(eclipseContinuousBattleReq, delegate(EclipseContinuousBattleRes res)
			{
				if (res == null)
				{
					VespaDebug.LogError("EclipseContinuousBattleRes is null");
					Vespa_System_Action_Extension_Method.fire(onSuccess, i_argA: false);
				}
				else if (res.Result != EclipseContinuousBattleRes.ResultType.Success)
				{
					VespaDebug.LogError($"EclipseContinuousBattleRes : {res.Result}");
					Vespa_System_Action_Extension_Method.fire(onSuccess, i_argA: false);
				}
				else
				{
					Vespa_System_Action_Extension_Method.fire(onSuccess, i_argA: true);
					CenterMessage.Open(LocalizationManager.GetStringDirect(isContinuous ? "ECLIPSE_AUTO_NEXT_TEAM_ON_CENTER_MSG" : "ECLIPSE_AUTO_NEXT_TEAM_OFF_CENTER_MSG"));
				}
			});
		}
		else
		{
			CampaignUpdaterActionT<EclipseOfflineModeUpdater> threadAction = new CampaignUpdaterActionT<EclipseOfflineModeUpdater>(delegate(EclipseOfflineModeUpdater updater)
			{
				updater.SwitchContinousBattle();
			});
			SingletonAuto<BattleFacade>.instance.AddThreadSync(threadAction);
			Vespa_System_Action_Extension_Method.fire(onSuccess, i_argA: true);
			CenterMessage.Open(LocalizationManager.GetStringDirect(isContinuous ? "ECLIPSE_AUTO_NEXT_TEAM_ON_CENTER_MSG" : "ECLIPSE_AUTO_NEXT_TEAM_OFF_CENTER_MSG"));
		}
	}

	void PvpGiveUpMenu.Listener.OnClose()
	{
		ClosePvpGiveUpMenu();
	}

	public void ClosePvpGiveUpMenu()
	{
		PvpGiveUpMenu pvpGiveUpMenu = GetInstance<PvpGiveUpMenu>(openedOnly: true);
		if (pvpGiveUpMenu != null)
		{
			pvpGiveUpMenu.Close();
		}
	}

	void PvpGiveUpMenu.Listener.OnGiveUp(Action<bool> onComplete)
	{
		BattleGiveUpReq data = new BattleGiveUpReq();
		GlobalAutoBehaviour<SocketServiceManager>.instance.BattleConnector.Request(data, delegate(BattleGiveUpRes res)
		{
			if (res == null)
			{
				VespaDebug.LogError("BattleBanPickGiveUpRes is null");
				Vespa_System_Action_Extension_Method.fire(onComplete, i_argA: false);
			}
			else if (res.Result != BattleGiveUpRes.ResultType.Success)
			{
				CenterMessage.Open("BattleBanPickGiveUpRes : " + res.Result);
				Vespa_System_Action_Extension_Method.fire(onComplete, i_argA: false);
			}
			else
			{
				Vespa_System_Action_Extension_Method.fire(onComplete, i_argA: true);
				ClosePvpGiveUpMenu();
			}
		});
	}

	private bool IsFriendDuel()
	{
		if (!SingletonUpdater<MatchManager>.isValidInstance)
		{
			return false;
		}
		return SingletonUpdater<MatchManager>.instance.DuelType == MatchDuelType.Friendly;
	}

	private bool IsOnline()
	{
		if (SingletonAuto<BattleFacade>.isValidInstance && SingletonAuto<BattleFacade>.instance.GetCampaignCache().IsBattleEnd)
		{
			return false;
		}
		return SingletonAuto<BattleContext>.instance.DataProvider?.IsOnline() ?? false;
	}

	int SystemButtonPvpGiveUp.Listener.GetElapsedTime()
	{
		if (!SingletonAuto<BattleFacade>.isValidInstance)
		{
			return 0;
		}
		BattleFacade.BattleCacheData battleCache = SingletonAuto<BattleFacade>.instance.GetBattleCache();
		if (battleCache == null)
		{
			return 0;
		}
		return (battleCache.ElapsedBattleTimeMs + 999) / 1000;
	}

	bool SystemButtonPvpGiveUp.Listener.IsBattleEnabled()
	{
		return IsBattleEnabled();
	}

	private bool IsBattleEnabled()
	{
		if (!SingletonAuto<BattleFacade>.isValidInstance)
		{
			return false;
		}
		return SingletonAuto<BattleFacade>.instance.IsBattleEnabled();
	}

	void SystemButtonPvpGiveUp.Listener.OnPvpGiveUp()
	{
		OpenPvpGiveUpMenu();
	}

	private void OpenPvpGiveUpMenu()
	{
		if ((BattleMenu & EBattleMenu.PvpGiveUp) != EBattleMenu.None)
		{
			PvpGiveUpMenu pvpGiveUpMenu = GetInstance<PvpGiveUpMenu>();
			if (!pvpGiveUpMenu.RequestSuccess && !pvpGiveUpMenu.IsPending)
			{
				pvpGiveUpMenu.Open(this);
			}
		}
	}

	void SystemButtonPause.Listener.OnPause()
	{
		OpenPausedMenu();
	}

	void SystemButtonExit.Listener.OnExit()
	{
		AutoBattle.Stop();
		SingletonAuto<BattleContext>.instance.MoveSceneToLobby(null, EBoolCheck.DontCare);
	}

	void SystemButtonExitSmall.Listener.OnExit()
	{
		if (_battleType == BattleType.Eclipse)
		{
			OpenEclipsePausedMenu();
		}
	}

	void SystemButtonPartyExit.Listener.OnExit()
	{
		if (!SprkPartyRetry.OnExit())
		{
			string stringDirect = LocalizationManager.GetStringDirect("COMMON_NOTIFICATION");
			string stringDirect2 = LocalizationManager.GetStringDirect("EXIT_TEXT_DES");
			MessagePopup3Button.Open_BattlePopup(stringDirect, stringDirect2, delegate
			{
				GlobalAutoBehaviour<PartyManager>.instance.SendLeaveParty();
				LobbyScene2.ListReturnState.Clear();
				SingletonAuto<BattleContext>.instance.MoveSceneToLobby(null, EBoolCheck.DontCare);
			}, null, NShared.NLocalization.Common.COMMON_CONFIRM, NShared.NLocalization.Common.COMMON_CANCEL);
		}
	}

	public void OnLeave()
	{
		MessagePopup3Button.Open(LocalizationManager.GetStringDirect("EXIT_TEXT"), LocalizationManager.GetStringDirect("EXIT_TEXT_DES"), button1Label: LocalizationManager.GetStringDirect("COMMON_CONFIRM"), button2Label: LocalizationManager.GetStringDirect("COMMON_CANCEL"), onButton1: delegate
		{
		}, onButton2: null);
	}

	void PausedMenu.Listener.OnRetry()
	{
		SprkSoloRetry.Retry(SingletonAuto<BattleContext>.instance, null);
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(new OnClickRetry());
	}

	void PausedMenu.Listener.OnReturnToMap()
	{
		AutoBattle.Stop();
		NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.ClearSelectedReward();
		SingletonAuto<BattleContext>.instance.MoveSceneToLobby(null, EBoolCheck.DontCare);
	}

	void PausedMenu.Listener.OnClose()
	{
		CloseInstance<PausedMenu>();
	}

	void BattleEmoti.Listener.SendEmoti(EmotiType emotyType, long receiverId, string receiverName)
	{
		PartyMemberEmoti data = new PartyMemberEmoti
		{
			Emoti = emotyType,
			ChatType = ChatType.Party,
			SenderName = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Nick,
			SenderAvatarIndex = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AvatarHeroIndex,
			SenderTeamLevel = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.TeamLevel,
			ReceiverId = receiverId,
			ReceiverName = receiverName
		};
		SingletonAuto<MessageLogHandler>.instance.SendToParty(data);
	}

	ReplaceSkillData PartySlotComponent2.Listener.GetReplacedSkillData(int creatureUid, int skillSlotIndex)
	{
		BattleFacade.CreatureCacheData creatureCache = SingletonAuto<BattleFacade>.instance.GetCreatureCache(creatureUid);
		SkillData nextSkillData = GetNextSkillData(creatureCache, skillSlotIndex);
		if (nextSkillData != null)
		{
			return new ReplaceSkillData(EReplaceSkillType.NextActiveSkill, nextSkillData.Index);
		}
		int skillIndexBySlot = creatureCache.CreatureData.GetSkillIndexBySlot(skillSlotIndex);
		if (creatureCache.ChangedSkillIndices != null && creatureCache.ChangedSkillIndices.Length != 0)
		{
			int changedSkillndex = creatureCache.GetChangedSkillndex(skillIndexBySlot);
			if (changedSkillndex != skillIndexBySlot)
			{
				return new ReplaceSkillData(EReplaceSkillType.ChangedSkill, changedSkillndex);
			}
		}
		return new ReplaceSkillData(EReplaceSkillType.None, skillIndexBySlot);
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<UserManager.UserStatInfo>.OnChanged(UserManager.UserStatInfo changedInfo)
	{
	}

	void PartyManager.IPartyListener.OnPartyDestroy(bool isMasterLeave)
	{
		if (NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.IsEnabled())
		{
			NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.instance.OnClickStop(sendRoomRepeat: true, PartyRoomRepeat.ResultType.CANCEL_MASTER);
		}
		if (CampaignEnd)
		{
			return;
		}
		SingletonAuto<BattleContext>.instance.StopBattle();
		MemberInfo myPartyMemberInfo = GlobalAutoBehaviour<PartyManager>.instance.GetMyPartyMemberInfo();
		if (myPartyMemberInfo != null && myPartyMemberInfo.IsMaster)
		{
			return;
		}
		GlobalTimer.PauseTimer(GlobalTimer.EPauseFlag.Menu);
		if (isMasterLeave)
		{
			string title = LocalizationManager.GetString(NShared.NLocalization.Common.ERROR);
			string message = LocalizationManager.GetString(Party.PARTY_MASTER_LEFT);
			MessagePopup.Open(title, message, delegate
			{
				GlobalTimer.ResumeTimer(GlobalTimer.EPauseFlag.Menu);
				SingletonAuto<BattleContext>.instance.MoveSceneToLobby(null, EBoolCheck.DontCare);
			});
		}
	}

	void PartyManager.IPartyListener.OnPartyMemberLeave(MemberInfo memberInfo)
	{
		CenterMessage.Open(LocalizationManager.GetString(Party.PARTY_MEMBER_LEFT, memberInfo.Nick));
	}

	void PartyManager.IPartyListener.OnPartyMemberDeckCostumeChanged(long accountId, int heroIndex, CachedHeroInfo heroInfo)
	{
	}

	void PartyManager.IPartyListener.OnPartyMemberJoin(MemberInfo memberInfo)
	{
	}

	void PartyManager.IPartyListener.OnPartyMemberDeckHeroChanged(MemberInfo memberInfo, int heroIndex, CachedHeroInfo heroInfo, bool isMain)
	{
	}

	void PartyManager.IPartyListener.OnPartyMemberReady(MemberInfo memberInfo, bool isReady)
	{
	}

	void PartyManager.IPartyListener.OnPartyMemberChangeAvatar(MemberInfo memberInfo)
	{
	}

	void PartyManager.IPartyListener.OnChangePartyMemberStatus(MemberInfo memberInfo)
	{
	}

	void PartyManager.IPartyListener.OnPartyRoomInfoUpdated(RaidRoomInfo raidRoomInf, bool isViolateBanRule)
	{
	}

	void PartyManager.IPartyListener.OnPartyRoomMasterChanged(MemberInfo memberInfo)
	{
	}

	void PartyManager.IPartyListener.OnPartyMemberResults(MemberInfo memberInfo)
	{
	}

	void PartyManager.IPartyListener.OnPartyRoomSetRepeat(bool IsRepeat)
	{
	}

	void PartyManager.INewPartyListener.OnPartyCreated()
	{
		PartyInfo partyInfo = GlobalAutoBehaviour<PartyManager>.instance.PartyInfo;
		if (!partyInfo.IsValid())
		{
			VespaDebug.LogError("Party created but is invalid");
			return;
		}
		if (GlobalAutoBehaviour<PartyManager>.instance.GetMyPartyMemberInfo() == null)
		{
			VespaDebug.LogError("Party created but cannot find my member info.");
			return;
		}
		if (GameRoot.Instance.RaidDataContainer.GetData(partyInfo.RaidIndex, partyInfo.RaidLevel) == null)
		{
			VespaDebug.LogError("RaidData not found : index=" + partyInfo.RaidIndex + " level=" + partyInfo.RaidLevel);
			return;
		}
		LobbyScene2.ListReturnState.Add(new RaidJoinReturnState(partyInfo.RaidIndex, partyInfo.RaidLevel));
		SingletonAuto<BattleContext>.instance.MoveSceneToLobby(null, EBoolCheck.DontCare);
	}

	void PartyManager.INewPartyListener.OnPartyInvited(long senderId, InvitePartyReq data)
	{
		GlobalAutoBehaviour<InviteNotifyManager>.instance.AddNotify(InviteNotifyType.Raid, data, delegate(bool accepted)
		{
			if (accepted)
			{
				accepted = InviteNotifyManager.IsOpenableNoty(senderId);
			}
			if (accepted)
			{
				GlobalAutoBehaviour<PartyManager>.instance.SendJoinPartyRequest_Accept(senderId, data.RoomNo);
			}
			else
			{
				GlobalAutoBehaviour<PartyManager>.instance.SendJoinPartyRequest_Reject(senderId);
			}
		});
	}

	public void OnAcceptDuel(long requestId, long groupId)
	{
		LobbyScene2.ListReturnState.Add(new AcceptDuelReturnState(requestId, groupId));
		SingletonAuto<BattleContext>.instance.MoveSceneToLobby(null, EBoolCheck.DontCare);
	}

	Transform GuildRaidScoreStatus.IDataProvider.GetBossStatusBottomDummy()
	{
		if (!IsOpened<BossStatus>())
		{
			return null;
		}
		return GetInstance<BossStatus>().GetBottomDummy();
	}

	Transform ChallengeRaidScoreStatus.IDataProvider.GetBossStatusBottomDummy()
	{
		if (!IsOpened<BossStatus>())
		{
			return null;
		}
		return GetInstance<BossStatus>().GetBottomDummy();
	}

	void ReplayExitButton.Listener.OnExit()
	{
		SingletonAuto<BattleContext>.instance.MoveSceneToLobby(null, EBoolCheck.DontCare);
	}

	private void EnableSystemButtonAutoByPartySlotComponent()
	{
		HeroParty heroParty = GetInstance<HeroParty>();
		if (heroParty == null)
		{
			return;
		}
		bool flag = true;
		IncreaseDic<int, PartySlotComponent2> partySlotDic = heroParty.PartySlotDic;
		int num = partySlotDic?.Count ?? 0;
		for (int i = 0; i < num; i++)
		{
			PartySlotComponent2 valueByIndex = partySlotDic.GetValueByIndex(i);
			if (!(valueByIndex == null))
			{
				flag &= valueByIndex.IsAuto;
			}
		}
		if (heroParty.EnabledSwapParty)
		{
			IncreaseDic<int, PartySlotComponent2> subPartySlotDic = heroParty.SubPartySlotDic;
			int num2 = subPartySlotDic?.Count ?? 0;
			for (int j = 0; j < num2; j++)
			{
				PartySlotComponent2 valueByIndex2 = subPartySlotDic.GetValueByIndex(j);
				if (!(valueByIndex2 == null))
				{
					flag &= valueByIndex2.IsAuto;
				}
			}
		}
		if (IsOpened<SystemButtonAuto>())
		{
			SystemButtonAuto systemButtonAuto = GetInstance<SystemButtonAuto>();
			systemButtonAuto.ChangeAutoUI(flag);
			_lastSkillReserveAuto = systemButtonAuto.SkillReserveAuto;
		}
		if (IsOpened<SystemButtonAutoSimple>())
		{
			SystemButtonAutoSimple systemButtonAutoSimple = GetInstance<SystemButtonAutoSimple>();
			systemButtonAutoSimple.ChangeAutoUI(flag);
			_lastSkillReserveAuto = systemButtonAutoSimple.SkillReserveAuto;
		}
		if (IsOpened<SystemButtonAutoFastContinue>())
		{
			SystemButtonAutoFastContinue systemButtonAutoFastContinue = GetInstance<SystemButtonAutoFastContinue>();
			systemButtonAutoFastContinue.ChangeAutoUI(flag);
			_lastSkillReserveAuto = systemButtonAutoFastContinue.SkillReserveAuto;
		}
	}

	private bool GetUsePunishmentRaidBossStatus()
	{
		if (_battleType != BattleType.PunishmentRaid)
		{
			return false;
		}
		RaidData raidDataByDungeon = GameRoot.Instance.RaidDataContainer.GetRaidDataByDungeon(_chapterIndex, _dungeonIndex);
		if (raidDataByDungeon == null)
		{
			return false;
		}
		return GameRoot.Instance.PunishmentRaidDataContainer.GetData(raidDataByDungeon.Index, raidDataByDungeon.Level)?.UsePunishmentRaidBossStatus ?? false;
	}

	public void InitSlot()
	{
		EnableSystemButtonAutoByPartySlotComponent();
	}

	public void OnClickSlot(PartySlotComponent2 partySlotComponent)
	{
		if (!partySlotComponent.IsAuto)
		{
			ClearReserveOnSkill(partySlotComponent.CreatureCache);
		}
		EnableSystemButtonAutoByPartySlotComponent();
	}

	private bool IsDisableWorldBossRestart(int chapterIndex, int dungeonIndex)
	{
		if (_battleType != BattleType.WorldBoss)
		{
			return false;
		}
		string key = "WORLD_BOSS_RESTART_BUTTON";
		return !SingletonAuto<ContentsValueManager>.instance.GetContentsBoolValue(key, defaultValue: true);
	}

	private void OnDrawGUI()
	{
		if (!BattleViewGenericSingleton<CreatureManager>.IsValidInstance)
		{
			return;
		}
		using (new BlockManager().TitleBox("CreatuerList").Vertical())
		{
			int creatureCount = BattleViewGenericSingleton<CreatureManager>.instance.GetCreatureCount();
			for (int i = 0; i < creatureCount; i++)
			{
				Creature creatureByIndex = BattleViewGenericSingleton<CreatureManager>.instance.GetCreatureByIndex(i);
				if (!(creatureByIndex == null))
				{
					string text = creatureByIndex.CreatureTableData.Name ?? (creatureByIndex.name + "?");
					bool flag = creatureByIndex.Uid == _selectedCreaureUID;
					bool flag2 = GUILayout.Toggle(flag, text);
					if (flag2 != flag)
					{
						_selectedCreaureUID = (flag2 ? creatureByIndex.Uid : 0);
					}
				}
			}
		}
		using (new BlockManager().TitleBox("Select Creatuer Info").Vertical())
		{
			if (_selectedCreaureUID > 0)
			{
				if (_getSelectedCreatureStat == null || _getSelectedCreatureStat.CreatureUid != _selectedCreaureUID)
				{
					_getSelectedCreatureStat = new GetCreatureStat(_selectedCreaureUID);
				}
				if (_getSelectedCreatureState == null || _getSelectedCreatureState.CreatureUid != _selectedCreaureUID)
				{
					_getSelectedCreatureState = new GetCreatureState(_selectedCreaureUID);
				}
				if (_getSelectedCreatureOperation == null || _getSelectedCreatureOperation.CreatureUid != _selectedCreaureUID)
				{
					_getSelectedCreatureOperation = new GetCreatureOperation(_selectedCreaureUID);
				}
				_getSelectedCreatureStat.ChainUpdate();
				_getSelectedCreatureState.ChainUpdate();
				_getSelectedCreatureOperation.ChainUpdate();
				using (new BlockManager().TitleBox("CreatureState").Vertical())
				{
					_getSelectedCreatureState.DrawToggleGUI();
				}
				using (new BlockManager().TitleBox("CreatureStat").Vertical())
				{
					_getSelectedCreatureStat.DrawToggleGUI();
				}
				using (new BlockManager().TitleBox("CreatureOperation").Vertical())
				{
					_getSelectedCreatureOperation.DrawToggleGUI();
					return;
				}
			}
		}
	}

	void EclipsePausedMenu.Listener.OnRetry()
	{
	}

	void EclipsePausedMenu.Listener.OnReturnToMap()
	{
		CloseEclipsePausedMenu();
		string stringDirect = LocalizationManager.GetStringDirect("ECLIPSE_BATTLE_ESCAPE_WAVE_POPUP_TITLE");
		string stringDirect2 = LocalizationManager.GetStringDirect("ECLIPSE_BATTLE_ESCAPE_WAVE_POPUP_DESC");
		if (IsOnline())
		{
			MessagePopup3Button.Open_BattlePopup(stringDirect, stringDirect2, delegate
			{
				SingletonAuto<CampaignContext>.instance.SetOnlineBattleCallbackIsBattleEnd(IsBattleEnd: true);
				GlobalAutoBehaviour<SocketServiceManager>.instance.BattleConnector.Close();
				SingletonAuto<BattleContext>.instance.MoveSceneToLobby(null, EBoolCheck.DontCare);
			}, null);
			return;
		}
		MessagePopup3Button.Open_BattlePopup(stringDirect, stringDirect2, delegate
		{
			SingletonAuto<BattleFacade>.instance.AddThreadSync(new CampaignUpdaterActionT<EclipseOfflineModeUpdater>(delegate(EclipseOfflineModeUpdater updater)
			{
				updater.BattleOut();
			}));
		}, null);
	}

	void EclipsePausedMenu.Listener.OnClose()
	{
		CloseEclipsePausedMenu();
	}

	void EclipsePausedMenu.Listener.OnNextTeam()
	{
		CloseEclipsePausedMenu();
		string stringDirect = LocalizationManager.GetStringDirect("ECLIPSE_BATTLE_NEXT_TEAM_POPUP_TITLE");
		string stringDirect2 = LocalizationManager.GetStringDirect("ECLIPSE_BATTLE_ NEXT_TEAM_POPUP_DESC");
		if (IsOnline())
		{
			MessagePopup3Button.Open_BattlePopup(stringDirect, stringDirect2, delegate
			{
				if (GlobalAutoBehaviour<SocketServiceManager>.instance.BattleConnector.State == ConnectState.Connected)
				{
					GlobalAutoBehaviour<SocketServiceManager>.instance.BattleConnector.Request(new WithdrawBattleReq(), delegate(WithdrawBattleRes res)
					{
						if (res.Result == WithdrawBattleRes.ResultType.Success)
						{
							SetMenuHide(EMenuHideFlag.LastKill, skipAni: true);
						}
					});
				}
			}, null);
			return;
		}
		MessagePopup3Button.Open_BattlePopup(stringDirect, stringDirect2, delegate
		{
			SingletonAuto<BattleFacade>.instance.AddThreadSync(new CampaignUpdaterActionT<EclipseOfflineModeUpdater>(delegate(EclipseOfflineModeUpdater updater)
			{
				updater.ForceNextTeam();
			}));
		}, null);
	}

	void BattleCameraSwitch.IListener.SwichingCamera()
	{
		if (SceneAutoBehaviour<CameraManager2>.isValidInstance)
		{
			SceneAutoBehaviour<CameraManager2>.instance.SwapBattleCamera();
		}
		if (Game3DRoot.IsValidInstance)
		{
			GameCamera gameCamera = Game3DRoot.instance.GameCamera;
			if (gameCamera != null)
			{
				gameCamera.ChangeFieldCameraData();
			}
		}
		HeroParty heroParty = GetInstance<HeroParty>();
		if (heroParty != null)
		{
			heroParty.OnClickSwapHeroParty();
		}
	}

	public void OnClickDungeonSkillSlot(TeamType teamType, int skillSlotIndex)
	{
		BattleFacade.CreatureCacheData environmentCreatureCache = SingletonAuto<BattleFacade>.instance.GetEnvironmentCreatureCache(teamType);
		if (environmentCreatureCache != null && environmentCreatureCache.CreatureData.GetSkillIndexBySlot(skillSlotIndex) != 0)
		{
			SoundStock stock = new SoundStock
			{
				Code = "UI.Battle.UsingSkill",
				LocalTimer = GlobalTimer.DefaultLocal
			};
			SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 10f);
			SingletonAuto<BattleFacade>.instance.BattleLogRaiser.ReserveDungeonSkill(teamType, skillSlotIndex);
		}
	}

	private SkillData GetEnhancedDungeonSkillData(BattleFacade.CreatureCacheData creatureCache, int skillSlotIndex)
	{
		if (creatureCache == null || !creatureCache.HasDungeonSkill)
		{
			return null;
		}
		DungeonSkillStateInfo dungeonSkillStateInfo = creatureCache.GetDungeonSkillStateInfo(skillSlotIndex);
		if (dungeonSkillStateInfo == null || dungeonSkillStateInfo.SkillCanUseResult != SkillUseResult.EnhancedSuccess)
		{
			return null;
		}
		if (GameRoot.Instance.SkillDataContainer.GetData(dungeonSkillStateInfo.BaseSkillIndex) == null)
		{
			return null;
		}
		if (dungeonSkillStateInfo.EnhancedSkillIndex == 0)
		{
			return null;
		}
		SkillData data = GameRoot.Instance.SkillDataContainer.GetData(dungeonSkillStateInfo.EnhancedSkillIndex);
		if (data == null || data.TriggerType != SkillTriggerType.None)
		{
			return null;
		}
		return data;
	}

	ReplaceSkillData DungeonSkillWindow.IListener.GetReplacedSkillData(int skillSlotIndex)
	{
		BattleFacade.CreatureCacheData environmentCreatureCache = SingletonAuto<BattleFacade>.instance.GetEnvironmentCreatureCache(TeamType.Left);
		SkillData enhancedDungeonSkillData = GetEnhancedDungeonSkillData(environmentCreatureCache, skillSlotIndex);
		if (enhancedDungeonSkillData != null)
		{
			return new ReplaceSkillData(EReplaceSkillType.EnhancedDungeonSkill, enhancedDungeonSkillData.Index);
		}
		int skillIndexBySlot = environmentCreatureCache.CreatureData.GetSkillIndexBySlot(skillSlotIndex);
		return new ReplaceSkillData(EReplaceSkillType.BaseDungeonSkill, skillIndexBySlot);
	}

	void DamageMeter.Listener.SetHighestTpsUid(int uid, bool forceRefresh)
	{
		if (forceRefresh || _highestTpsUid != uid)
		{
			_highestTpsUid = uid;
			if (IsOpened<DamageMeter>())
			{
				GetInstance<DamageMeter>().SetHighestTpsComp(uid);
			}
			if (IsOpened<HeroParty>())
			{
				GetInstance<HeroParty>().SetHighestTpsEffect(uid);
			}
		}
	}

	void KarmaCardSelectWindow.IListener.OnClickSelect(int slotIndex)
	{
		if (SingletonAuto<BattleFacade>.isValidInstance)
		{
			SingletonAuto<BattleFacade>.instance.AddThreadCommonAction(delegate(BattleInstance battleInstance)
			{
				KarmaSelectCardBattleLog runtime = new KarmaSelectCardBattleLog
				{
					SelectSlot = slotIndex
				};
				battleInstance.BattleLogContainer.AddLog(runtime);
			});
		}
	}
}
