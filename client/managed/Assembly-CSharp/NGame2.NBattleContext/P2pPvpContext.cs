using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NBattleField;
using NGame2.NBattleField.NManager.NBattleCondition;
using NGame2.NBattleField.NManager.NPlay;
using NGame2.NScene.NPreloader;
using NShared;
using NVespa;
using NVespa.NSingleton;
using NVespa.NSocket;
using NVespa.NUtil;

namespace NGame2.NBattleContext;

public class P2pPvpContext : SingletonAuto<P2pPvpContext>, OnlinePlay.IDataProvider, IDataProvider
{
	public class PlayerInfo
	{
		public long AccountId;

		public CachedHeroInfo[] HeroInfos { get; set; }
	}

	private PlayerInfo[] _playerInfos;

	private int _chapterIndex;

	private int _dungeonIndex;

	private DungeonDifficultyType _difficulty;

	private OnlineBattleCallback _onlineBattleCallback;

	private P2pSocketRequester _requester;

	private P2pSocketHandler _handler;

	public void Init(PlayerInfo[] playerInfos, int chapterIndex, int dungeonIndex, DungeonDifficultyType difficulty)
	{
		_playerInfos = playerInfos;
		_chapterIndex = chapterIndex;
		_dungeonIndex = dungeonIndex;
		_difficulty = difficulty;
		List<long> list = new List<long>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_playerInfos); i++)
		{
			long accountId = _playerInfos[i].AccountId;
			list.Add(accountId);
		}
		_requester = new P2pSocketRequester();
		_requester.Init(list.ToArray());
		_handler = new P2pSocketHandler();
		if (_onlineBattleCallback != null)
		{
			_onlineBattleCallback.Close();
			_onlineBattleCallback = null;
		}
		_onlineBattleCallback = new OnlineBattleCallback(_handler);
		_onlineBattleCallback.Init();
	}

	private void WaitBattleEnd(BattleResultNot data, int remainWaitCount)
	{
		BattleFacade.BattleCacheData battleCache = SingletonAuto<BattleFacade>.instance.GetBattleCache();
		if (remainWaitCount <= 0 || (SingletonAuto<BattleFacade>.instance.IsValidBattle() && battleCache.IsBattleEnded))
		{
			if (SingletonAuto<BattleFacade>.instance.IsValidBattle() && !battleCache.IsBattleEnded)
			{
				SingletonAuto<BattleFacade>.instance.ForceStopBattle();
			}
		}
		else
		{
			SingletonUpdater<Executer>.instance.DelayRun(0.5f, this, delegate
			{
				WaitBattleEnd(data, remainWaitCount - 1);
			});
		}
	}

	private void ReturnToLobby()
	{
		SingletonAuto<BattleContext>.instance.ReturnToLobby();
	}

	void IDataProvider.Preload(BattleScenePreloader preloader)
	{
		preloader.EnterDungeon = new EnterDungeonData(_chapterIndex, _dungeonIndex, _dungeonIndex, DungeonDifficultyType.Easy);
		preloader.PreloadDungeonMonster = Vespa_Common_Class_Extension_Method.Interface((IDataProvider)this).GetPrealodCreateMonster();
		if (_playerInfos == null)
		{
			return;
		}
		PlayerInfo[] playerInfos = _playerInfos;
		foreach (PlayerInfo obj in playerInfos)
		{
			bool loadCreatureImage = obj.AccountId == GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId;
			CachedHeroInfo[] heroInfos = obj.HeroInfos;
			foreach (CachedHeroInfo heroInfo in heroInfos)
			{
				preloader.AddCreature(heroInfo, null, loadCreatureImage);
			}
		}
	}

	bool IDataProvider.CanNextDungeon()
	{
		return false;
	}

	EnterDungeonData IDataProvider.GetEnterDungeon()
	{
		return new EnterDungeonData(_chapterIndex, _dungeonIndex, _dungeonIndex, _difficulty);
	}

	void IDataProvider.OnEnterBattle()
	{
	}

	void IDataProvider.OnWaveMoveEnd()
	{
	}

	bool IDataProvider.CanStartBattle()
	{
		return true;
	}

	BattleStartEffectData IDataProvider.GetBattleStartEffectData()
	{
		return null;
	}

	IPlay IDataProvider.GetBattlePlay()
	{
		return SceneAutoBehaviour<OnlinePlay>.instance;
	}

	BattleType IDataProvider.GetBattleType()
	{
		return BattleType.Arena;
	}

	BattleDefineData IDataProvider.GetBattleDefine()
	{
		return GameRoot.Instance.CampaignDungeonDataContainer.GetData(_chapterIndex, _dungeonIndex)?.GetBattleDefine(isScenario: false);
	}

	BattleFacade.BattleConditionBase IDataProvider.CreateBattleCondition()
	{
		return new ArenaCondition();
	}

	bool IDataProvider.IsOnline()
	{
		return true;
	}

	void IDataProvider.OnUpdate()
	{
	}

	bool IDataProvider.IsTowerBattle()
	{
		return false;
	}

	int IDataProvider.GetTowerFloor()
	{
		return 0;
	}

	List<CachedHeroInfo> IDataProvider.GetPreloadBattleLogs()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_playerInfos))
		{
			return null;
		}
		List<CachedHeroInfo> list = new List<CachedHeroInfo>();
		PlayerInfo[] playerInfos = _playerInfos;
		foreach (PlayerInfo playerInfo in playerInfos)
		{
			list.AddRange(playerInfo.HeroInfos);
		}
		return list;
	}

	string IDataProvider.GetPreloadFieldName()
	{
		return null;
	}

	bool IDataProvider.GetPrealodCreateMonster()
	{
		return true;
	}

	int OnlinePlay.IDataProvider.GetChapterIndex()
	{
		return _chapterIndex;
	}

	int OnlinePlay.IDataProvider.GetDungeonIndex()
	{
		return _dungeonIndex;
	}

	DungeonDifficultyType OnlinePlay.IDataProvider.GetDifficulty()
	{
		return _difficulty;
	}

	EBattleMenu IDataProvider.GetBattleMenuType()
	{
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(_chapterIndex, _dungeonIndex);
		if (data == null)
		{
			return EBattleMenu.TimeStatus | EBattleMenu.SystemButton | EBattleMenu.WaveStatus | EBattleMenu.LootingStatus | EBattleMenu.Retry | EBattleMenu.DamageMeter | EBattleMenu.HeroParty | EBattleMenu.BoosterStatus | EBattleMenu.QuestStatus | EBattleMenu.SystemButtonAuto;
		}
		return data.GetBattleDefine(isScenario: false)?.BattleMenu ?? (EBattleMenu.TimeStatus | EBattleMenu.SystemButton | EBattleMenu.WaveStatus | EBattleMenu.LootingStatus | EBattleMenu.Retry | EBattleMenu.DamageMeter | EBattleMenu.HeroParty | EBattleMenu.BoosterStatus | EBattleMenu.QuestStatus | EBattleMenu.SystemButtonAuto);
	}

	GuildFlagData IDataProvider.GetGuildFlagData()
	{
		return null;
	}

	void OnlinePlay.IDataProvider.OnReturnToLobby()
	{
	}

	OnlineBattleCallback OnlinePlay.IDataProvider.GetBattleCallback()
	{
		return _onlineBattleCallback;
	}

	int OnlinePlay.IDataProvider.GetWorldMapEventIndex()
	{
		return 0;
	}

	IRequester OnlinePlay.IDataProvider.GetSocketConnector()
	{
		return _requester;
	}

	string OnlinePlay.IDataProvider.GetError()
	{
		return null;
	}

	int OnlinePlay.IDataProvider.OnlineGameSpeedRatio()
	{
		return 1;
	}

	List<List<BattleCreatureCreateInfo>> OnlinePlay.IDataProvider.GetBattleCreatureCreateInfoListToWaveList()
	{
		return null;
	}

	BattleInstanceExtraData OnlinePlay.IDataProvider.GetExtraData()
	{
		return null;
	}
}
