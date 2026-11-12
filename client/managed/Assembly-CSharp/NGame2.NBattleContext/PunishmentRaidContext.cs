using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NBattleField;
using NGame2.NBattleField.NManager;
using NGame2.NBattleField.NManager.NBattleCondition;
using NGame2.NBattleField.NManager.NPlay;
using NGame2.NBattleView;
using NGame2.NContents;
using NGame2.NGuild;
using NGame2.NScene.NPreloader;
using NGame2.NUI.NManager.NBattle;
using NShared;
using NShared.BeginContent;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NBattleContext;

public class PunishmentRaidContext : SingletonAuto<PunishmentRaidContext>, OfflinePlay.IDataProvider, IDataProvider
{
	public int[] HeroIndices;

	public int[] AiHeroIndices;

	public long[] AccountIds;

	public EnterDungeonData EnterDungeon = new EnterDungeonData();

	public BattleInstanceExtraData ExtraData = new BattleInstanceExtraData();

	private OfflinePlay.ResponseData _responseData;

	public int GroupIndex;

	public EPunishmentDungeonType PunishmentDungeonType;

	public void Init(NShared.BeginContent.Request request, NShared.BeginContent.Response response, long[] accountIds)
	{
		int chapterIndex = request.ChapterIndex;
		int dungeonIndex = request.DungeonIndex;
		EnterDungeon = new EnterDungeonData(chapterIndex, dungeonIndex, dungeonIndex);
		HeroIndices = request.HeroIndices;
		AiHeroIndices = request.AiHeroIndices;
		AccountIds = accountIds;
		GroupIndex = request.GroupIndex;
		PunishmentDungeonType = request.DungeonType;
		ExtraData.AffixIndices = request.AffixIndices;
		ExtraData.InfinityLevel = request.Level;
		ExtraData.FlaskItemIndex = request.FlaskItemIndex;
		if (request.DungeonType == EPunishmentDungeonType.Karma)
		{
			ItemInfo itemInfo = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Get(request.FlaskItemIndex);
			if (itemInfo != null)
			{
				ExtraData.FlaskItemCount = itemInfo.Count;
			}
		}
		else if (request.DungeonType == EPunishmentDungeonType.Boss)
		{
			ExtraData.FlaskItemCount = request.FlaskItemCount;
		}
		_responseData = new OfflinePlay.ResponseData();
		_responseData.CheckIntegrity = response.CheckIntegrity;
	}

	private PlayerEnterBattleLog CreateEnterPlayerBattleLog_Me()
	{
		return CampaignContext.CreateMyEnterPlayerBattleLog(HeroIndices, AccountIds, 0, AiHeroIndices, null, EnterDungeon.InitialHeroInfos, GetBattleType());
	}

	private EBattleMenu GetBattleMenuType()
	{
		EBattleMenu eBattleMenu = EBattleMenu.TimeStatus | EBattleMenu.SystemButton | EBattleMenu.WaveStatus | EBattleMenu.LootingStatus | EBattleMenu.Retry | EBattleMenu.DamageMeter | EBattleMenu.HeroParty | EBattleMenu.BoosterStatus | EBattleMenu.QuestStatus | EBattleMenu.SystemButtonAuto;
		CampaignDungeonData linkedDungeonData = EnterDungeon.LinkedDungeonData;
		if (linkedDungeonData != null)
		{
			BattleDefineData battleDefine = linkedDungeonData.GetBattleDefine(EnterDungeon.IsScenario);
			if (battleDefine != null)
			{
				eBattleMenu = battleDefine.BattleMenu;
			}
		}
		if (!IsRepeatable())
		{
			eBattleMenu = (EBattleMenu)((ulong)eBattleMenu & 0xFFFFFFFFFFFFFFEFuL);
		}
		return eBattleMenu;
	}

	private bool IsRepeatable()
	{
		if (EnterDungeon.OriginalDungeonData == null)
		{
			return false;
		}
		if (EnterDungeon.OriginalDungeon != EnterDungeon.LinkedDungeon)
		{
			return false;
		}
		if (SingletonAuto<ContentsOpenChecker>.instance.IsContentsOpenNode(EnterDungeon.Chapter, EnterDungeon.OriginalDungeon, EnterDungeon.Difficulty))
		{
			return false;
		}
		return true;
	}

	private void EndContent(bool win, int star, Dictionary<int, int> killMonsterCount, IList<ResultCreatureInfo> creatureInfos, int pureBattleTime, long elapsedBattleTime, string battleLogString = null)
	{
		if (win)
		{
			EnterDungeon = new EnterDungeonData(EnterDungeon.Chapter, EnterDungeon.OriginalDungeon, EnterDungeon.OriginalDungeon, EnterDungeon.Difficulty, EnterDungeon.IsScenario);
		}
		List<int> list = new List<int>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(creatureInfos); i++)
		{
			ResultCreatureInfo resultCreatureInfo = creatureInfos[i];
			if (resultCreatureInfo != null && resultCreatureInfo.Hp > 0)
			{
				int index = resultCreatureInfo.Index;
				SprkAddPartySurvivor(list, index, HeroIndices, AiHeroIndices);
			}
		}
		SceneAutoBehaviour<PunishEndContentManagement>.instance.OpenbyEndContent(HeroIndices, AiHeroIndices, null, AccountIds, win, star, killMonsterCount, creatureInfos, list, pureBattleTime, elapsedBattleTime, battleLogString);
	}

	private void EndPunishRaid(TeamType winTeamType, BattleInstance battleInstance)
	{
		Dictionary<int, int> killCount = battleInstance.GetKillCount(TeamType.Left);
		bool flag = winTeamType == TeamType.Left;
		int campaignResultStar = battleInstance.GetCampaignResultStar(flag, EnterDungeon.Difficulty);
		List<ResultCreatureInfo> creatureInfoList = BattleInstanceIntegrityChecker.GetCreatureInfoList(battleInstance);
		if (flag)
		{
			SceneAutoBehaviour<CameraManager2>.instance.SwitchCam_Battle();
			SceneAutoBehaviour<WavePositionCollector>.instance.CreateLootingOnly();
		}
		string battleLogString = null;
		int pureBattleTime = (int)battleInstance.GlobalTimer.GetCurrentTimeMs() / 1000;
		long elapsedPureBattleTimeMs = battleInstance.GetElapsedPureBattleTimeMs();
		if (_responseData != null && _responseData.CheckIntegrity)
		{
			battleLogString = ReplayUtil.CreateBattleLogString(battleInstance, JsonMarshaler.EncodeJsonString, compress: true);
		}
		EndContent(flag, campaignResultStar, killCount, creatureInfoList, pureBattleTime, elapsedPureBattleTimeMs, battleLogString);
	}

	private void EndKarma(TeamType winTeamType, BattleInstance battleInstance)
	{
		Dictionary<int, int> killCount = battleInstance.GetKillCount(TeamType.Left);
		bool flag = winTeamType == TeamType.Left;
		if (winTeamType == TeamType.Right && AnyFlaskCollected(battleInstance))
		{
			flag = true;
		}
		int star = 0;
		List<ResultCreatureInfo> creatureInfoList = BattleInstanceIntegrityChecker.GetCreatureInfoList(battleInstance);
		if (flag)
		{
			SceneAutoBehaviour<CameraManager2>.instance.SwitchCam_Battle();
			SceneAutoBehaviour<WavePositionCollector>.instance.CreateLootingOnly();
		}
		string battleLogString = null;
		int pureBattleTime = (int)battleInstance.GlobalTimer.GetCurrentTimeMs() / 1000;
		long elapsedPureBattleTimeMs = battleInstance.GetElapsedPureBattleTimeMs();
		if (_responseData != null && _responseData.CheckIntegrity)
		{
			battleLogString = ReplayUtil.CreateBattleLogString(battleInstance, JsonMarshaler.EncodeJsonString, compress: true);
		}
		EndContent(flag, star, killCount, creatureInfoList, pureBattleTime, elapsedPureBattleTimeMs, battleLogString);
	}

	private bool AnyFlaskCollected(BattleInstance battleInstance)
	{
		int chapterIndex = battleInstance.ChapterIndex;
		int dungeonIndex = battleInstance.DungeonIndex;
		int clearWaveCount = SingletonAuto<BattleFacade>.instance.KarmaCache.ClearWaveCount;
		int endWave = SingletonAuto<BattleFacade>.instance.KarmaCache.ClearWaveCount + 1;
		int currentWaveKillCount = SingletonAuto<BattleFacade>.instance.KarmaCache.CurrentWaveKillCount;
		float flaskExp = PunishmentHelper.GetFlaskExp(chapterIndex, dungeonIndex, clearWaveCount, endWave, currentWaveKillCount);
		int selectedFullFlaksCount = 0;
		int defaultFullFlaskCount = 0;
		PunishmentHelper.GetFullFlask(battleInstance.ExtraData.FlaskItemIndex, battleInstance.ExtraData.FlaskItemCount, flaskExp, out selectedFullFlaksCount, out defaultFullFlaskCount);
		if (selectedFullFlaksCount <= 0)
		{
			return defaultFullFlaskCount > 0;
		}
		return true;
	}

	bool IDataProvider.CanNextDungeon()
	{
		return false;
	}

	bool IDataProvider.CanStartBattle()
	{
		return true;
	}

	BattleFacade.BattleConditionBase IDataProvider.CreateBattleCondition()
	{
		return new CampaignCondition();
	}

	List<IBattleLogRuntime> OfflinePlay.IDataProvider.CreateInitialBattleLog()
	{
		return new List<IBattleLogRuntime> { CreateEnterPlayerBattleLog_Me() };
	}

	List<List<BattleCreatureCreateInfo>> OfflinePlay.IDataProvider.GetBattleCreatureCreateInfoListToWaveList()
	{
		return null;
	}

	BattleDefineData IDataProvider.GetBattleDefine()
	{
		return EnterDungeon.LinkedDungeonData?.GetBattleDefine(EnterDungeon.IsScenario);
	}

	private BattleType GetBattleType()
	{
		CampaignDungeonData linkedDungeonData = EnterDungeon.LinkedDungeonData;
		if (linkedDungeonData == null)
		{
			VespaDebug.LogError("DungeonData not found : chapterIndex=" + EnterDungeon.Chapter + " linkeddungeonIndex=" + EnterDungeon.LinkedDungeon);
			return BattleType.None;
		}
		return linkedDungeonData.BattleType;
	}

	EBattleMenu IDataProvider.GetBattleMenuType()
	{
		return GetBattleMenuType();
	}

	IPlay IDataProvider.GetBattlePlay()
	{
		return SceneAutoBehaviour<OfflinePlay>.instance;
	}

	BattleStartEffectData IDataProvider.GetBattleStartEffectData()
	{
		return null;
	}

	BattleType IDataProvider.GetBattleType()
	{
		return GetBattleType();
	}

	int OfflinePlay.IDataProvider.GetChapterIndex()
	{
		return EnterDungeon.Chapter;
	}

	int OfflinePlay.IDataProvider.GetDungeonIndex()
	{
		return EnterDungeon.LinkedDungeon;
	}

	DungeonDifficultyType OfflinePlay.IDataProvider.GetDifficulty()
	{
		return EnterDungeon.Difficulty;
	}

	bool OfflinePlay.IDataProvider.IsScenarioDungeon()
	{
		return EnterDungeon.IsScenario;
	}

	int OfflinePlay.IDataProvider.GetRequestDungeonIndex()
	{
		return EnterDungeon.OriginalDungeon;
	}

	BattleInstanceExtraData OfflinePlay.IDataProvider.GetExtraData()
	{
		return ExtraData;
	}

	GuildFlagData IDataProvider.GetGuildFlagData()
	{
		CampaignDungeonData linkedDungeonData = EnterDungeon.LinkedDungeonData;
		if (linkedDungeonData == null)
		{
			return null;
		}
		if (!GlobalAutoBehaviour<GuildManager>.isValidInstance || GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager == null)
		{
			return null;
		}
		MyGuildInfo info = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info;
		return new GuildFlagData
		{
			EGuildFlagDisplayType = linkedDungeonData.EGuildFlagDisplayType,
			LeftGuildFlag = (info?.Flag ?? 1),
			RightGuildFlag = 1,
			LeftGuildLogo = (info?.Logo ?? 1),
			RightGuildLogo = 1,
			LeftGuildFlagBG = (info?.Back ?? 1),
			RightGuildFlagBG = 1,
			ExistLeftGuild = (info != null),
			ExistRightGuild = false
		};
	}

	bool IDataProvider.GetPrealodCreateMonster()
	{
		return true;
	}

	List<CachedHeroInfo> IDataProvider.GetPreloadBattleLogs()
	{
		List<CachedHeroInfo> list = new List<CachedHeroInfo>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(HeroIndices); i++)
		{
			list.Add(GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(HeroIndices[i]));
		}
		return list;
	}

	string IDataProvider.GetPreloadFieldName()
	{
		return null;
	}

	OfflinePlay.ResponseData OfflinePlay.IDataProvider.GetResponseData()
	{
		return _responseData;
	}

	int IDataProvider.GetTowerFloor()
	{
		return 0;
	}

	int OfflinePlay.IDataProvider.GetWorldMapEventIndex()
	{
		return 0;
	}

	void OfflinePlay.IDataProvider.HandleBattleBegin()
	{
	}

	void OfflinePlay.IDataProvider.HandleBattleEnd(TeamType winTeamType, BattleInstance battleInstance)
	{
		if (PunishmentDungeonType == EPunishmentDungeonType.Karma)
		{
			EndKarma(winTeamType, battleInstance);
		}
		else
		{
			EndPunishRaid(winTeamType, battleInstance);
		}
	}

	bool IDataProvider.IsOnline()
	{
		return false;
	}

	bool IDataProvider.IsTowerBattle()
	{
		return false;
	}

	void IDataProvider.OnEnterBattle()
	{
	}

	void IDataProvider.OnUpdate()
	{
	}

	void IDataProvider.OnWaveMoveEnd()
	{
	}

	void IDataProvider.Preload(BattleScenePreloader preloader)
	{
		preloader.EnterDungeon = EnterDungeon;
		preloader.PreloadDungeonMonster = Vespa_Common_Class_Extension_Method.Interface((IDataProvider)this).GetPrealodCreateMonster();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(HeroIndices); i++)
		{
			CachedHeroInfo heroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(HeroIndices[i]);
			preloader.AddCreature(heroInfo, null, loadCreatureImage: true);
		}
	}

	EnterDungeonData IDataProvider.GetEnterDungeon()
	{
		return EnterDungeon;
	}

	public static void SprkAddPartySurvivor(List<int> P_0, int P_1, int[] P_2, int[] P_3)
	{
		if (((P_2 != null && Array.IndexOf(P_2, P_1) >= 0) || (P_3 != null && Array.IndexOf(P_3, P_1) >= 0)) && !P_0.Contains(P_1))
		{
			P_0.Add(P_1);
		}
	}
}
