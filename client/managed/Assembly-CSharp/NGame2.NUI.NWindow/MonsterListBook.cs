using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NPortrait;
using NGame2.NUI.NManager;
using NNguiExtension;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class MonsterListBook : BaseWindow, MonsterContentsTabComponent.Listener, MonsterPortraitComponent.IListener
{
	public interface Listener
	{
		void OnEnterBossInfo(ArchiveType ArchiveType, IMonsterPortraitProvider iProvider);

		void OnEnterChapterBook(int ChapterIndex);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/IllustratedBook/MonsterListBook";

	private const int GuildRaidEtcKey = 0;

	private Listener _listener;

	private ArchiveType _curArchiveType;

	private int _currentDifficulty;

	private List<MonsterContentsTabComponent> ContentsComponentList = new List<MonsterContentsTabComponent>();

	private List<MonsterPortraitComponent> portraitComponentList = new List<MonsterPortraitComponent>();

	public UIScrollGrid TabGrid;

	public UIScrollGrid PortraitGrid;

	public UIWidget ComboBoxWidget;

	public UIComboBox ComboBox_Diff;

	public UISprite Sprite_Progress;

	public UIProgressBar Sprite_ProgressBar;

	public UILabel Label_Progress;

	public List<MonsterPortraitComponent> PortraitComponentList => portraitComponentList;

	public void Open(Listener listener)
	{
		_listener = listener;
		ComboBox_Diff.OnChanged = OnChangeDifficulty;
		base.Open(delegate
		{
			Init();
		});
	}

	public void Init()
	{
		PlayerArchiveData[] dataByMainType = GameRoot.Instance.PlayerArchiveDataContainer.GetDataByMainType(ArchiveMainType.Monster);
		if (dataByMainType.Length != 0)
		{
			_curArchiveType = (ArchiveType)dataByMainType[0].SubIndex;
		}
		else
		{
			_curArchiveType = ArchiveType.Raid;
		}
		InitArchiveTypeTab(_curArchiveType);
		InitComboBoxItem();
		RefreshList();
	}

	public void RefreshList()
	{
		SetDifficultyBox();
		SetMonsterList();
		SetArchiveGauge();
	}

	public override void Reset()
	{
		base.Reset();
		PlayerArchiveData[] dataByMainType = GameRoot.Instance.PlayerArchiveDataContainer.GetDataByMainType(ArchiveMainType.Monster);
		if (dataByMainType.Length != 0)
		{
			_curArchiveType = (ArchiveType)dataByMainType[0].SubIndex;
		}
		else
		{
			_curArchiveType = ArchiveType.Raid;
		}
		ComboBox_Diff.Clear();
		FactoryBase.ReleaseAndClearList(ContentsComponentList);
		Clear();
	}

	public void Clear()
	{
		FactoryBase.ReleaseAndClearList(portraitComponentList);
	}

	private void InitArchiveTypeTab(ArchiveType ArchiveType)
	{
		PlayerArchiveData[] dataByMainType = GameRoot.Instance.PlayerArchiveDataContainer.GetDataByMainType(ArchiveMainType.Monster);
		Array.Sort(dataByMainType, (PlayerArchiveData data1, PlayerArchiveData data2) => data1.ShowIndex.CompareTo(data2.ShowIndex));
		PlayerArchiveData[] array = dataByMainType;
		for (int num = 0; num < array.Length; num++)
		{
			ArchiveType subIndex = (ArchiveType)array[num].SubIndex;
			MonsterContentsTabComponent monsterContentsTabComponent = UIHelper.AddGridItem(TabGrid, MonsterContentsTabComponent.PrefabAuto);
			monsterContentsTabComponent.Init(subIndex, this);
			monsterContentsTabComponent.SelectToggle(subIndex == ArchiveType);
			ContentsComponentList.Add(monsterContentsTabComponent);
		}
		UIHelper.RepositionGrid(TabGrid);
	}

	private void InitComboBoxItem()
	{
		if (!(ComboBox_Diff == null))
		{
			ComboBox_Diff.Clear();
			if (_curArchiveType == ArchiveType.Raid)
			{
				SetDifficultyComboBox_Raid();
			}
			else if (_curArchiveType == ArchiveType.GuildRaid)
			{
				SetDifficultyComboBox_GuildRaid();
			}
			else
			{
				SetDifficultyComboBox_Normal();
			}
			ComboBox_Diff.SelectFirstItem();
			ComboBox_Diff.Reposition();
		}
	}

	private void SetDifficultyBox()
	{
		bool flag = IsShowDifficultyArchiveType();
		UIHelper.EnableGameObject(ComboBox_Diff.SelectedLabel.gameObject, flag);
		UIHelper.EnableCollider(ComboBox_Diff, flag);
		UIHelper.SetWidgetGrayScale(ComboBoxWidget, !flag);
	}

	private bool IsShowDifficultyArchiveType()
	{
		if (_curArchiveType != ArchiveType.Raid)
		{
			return _curArchiveType == ArchiveType.GuildRaid;
		}
		return true;
	}

	private void SetMonsterList()
	{
		Clear();
		switch (_curArchiveType)
		{
		case ArchiveType.Raid:
			SetDragonRaid();
			break;
		case ArchiveType.ChallengeRaid:
			SetChallengeRaid();
			break;
		case ArchiveType.GuildRaid:
			SetGuildRaid();
			break;
		case ArchiveType.GuildSuppress:
			SetGuildSuppress();
			break;
		case ArchiveType.WorldBoss:
			SetWorldBoss();
			break;
		case ArchiveType.ChapterBoss:
			SetChapterLastBoss();
			break;
		case ArchiveType.Conquest:
			SetConquestBoss();
			break;
		case ArchiveType.GodkingTrial:
			SetGodkingTrialBoss();
			break;
		case ArchiveType.Eclipse:
			SetEclipseBoss();
			break;
		case ArchiveType.ShakmehDungeon:
			SetShakmehBoss();
			break;
		case ArchiveType.TechnoMagic:
			SetTechnoMagicBoss();
			break;
		case ArchiveType.PunishmentRaid:
			SetPunishmentRaidBoss();
			break;
		case ArchiveType.Normal:
			SetNormalMonster();
			break;
		}
	}

	private void SetArchiveGauge()
	{
		int num = 0;
		IList<CreatureData> allData = GameRoot.Instance.CreatureDataContainer.GetAllData();
		if (_curArchiveType != ArchiveType.GuildRaid)
		{
			foreach (CreatureData item in allData)
			{
				if (item.ArchiveType == _curArchiveType)
				{
					num++;
				}
			}
		}
		else
		{
			num = GameRoot.Instance.GuildRaidDungeonDataContainer.GetAllData().Count;
			num += GameRoot.Instance.GuildRaidBonusDungeonDataContainer.GetAllData().Count;
			num += GameRoot.Instance.GuildRaidDummyDungeonDataContainer.GetAllData().Count;
		}
		int subArchiveCount = GlobalAutoBehaviour<AccountManager>.instance.ArchiveManager.GetSubArchiveCount(_curArchiveType);
		if (Sprite_ProgressBar == null)
		{
			Sprite_ProgressBar = Sprite_Progress.gameObject.AddComponent<UIProgressBar>();
			Sprite_ProgressBar.foregroundWidget = Sprite_Progress;
		}
		if (_curArchiveType == ArchiveType.Normal)
		{
			num = 0;
			for (int i = 1; i <= GetLastChapterIndex() + 1; i++)
			{
				CampaignDungeonData[] dungeonDatas = GameRoot.Instance.CampaignDungeonDataContainer.GetAllChapterData(i).ToArray();
				List<int> dungeonMonsterIndexList = GetDungeonMonsterIndexList(dungeonDatas);
				num += dungeonMonsterIndexList.Count;
			}
		}
		Sprite_ProgressBar.value = (float)subArchiveCount / (float)num;
		string text = $"{Sprite_ProgressBar.value * 100f:N2}% " + $"({subArchiveCount}/{num})";
		UIHelper.SetLabelText(Label_Progress, text);
	}

	private List<int> GetDungeonMonsterIndexList(CampaignDungeonData[] dungeonDatas)
	{
		List<int> list = new List<int>();
		foreach (CampaignDungeonData campaignDungeonData in dungeonDatas)
		{
			if (campaignDungeonData.BattleDefineCode == (HashString)"Tutorial" || campaignDungeonData.BattleDefineCode == (HashString)"Tutorial2" || (campaignDungeonData.BattleType == BattleType.GuildSuppress && campaignDungeonData.BattleDefineCode != (HashString)"ChallengeRaid"))
			{
				continue;
			}
			List<MonsterHeroInfo> list2 = new List<MonsterHeroInfo>();
			DungeonDifficultyType[] array = Enum.GetValues(typeof(DungeonDifficultyType)) as DungeonDifficultyType[];
			foreach (DungeonDifficultyType difficulty in array)
			{
				EnterDungeonData enterDungeon = new EnterDungeonData(campaignDungeonData.ChapterIndex, campaignDungeonData.DungeonIndex, 0, difficulty);
				list2.AddRange(UIHelper.GetMonsterInfoFromDungeon(enterDungeon));
				for (int k = 0; k < list2.Count; k++)
				{
					if (!list.Contains(list2[k].HeroInfo.HeroIndex))
					{
						list.Add(list2[k].HeroInfo.HeroIndex);
					}
				}
			}
		}
		return list;
	}

	private RaidType GetDragonRaidDifficulty(DragonRaidDifficulty dragonRaidType)
	{
		return EnumAttributeContainer<DragonRaidDifficulty, DragonRaidDifficultyAttribute>.Get(dragonRaidType)?.DragonRaidType ?? RaidType.DragonNormal;
	}

	private void SetDifficultyComboBox_Raid()
	{
		DragonRaidDifficulty[] array = Enum.GetValues(typeof(DragonRaidDifficulty)) as DragonRaidDifficulty[];
		for (int i = 0; i < array.Length; i++)
		{
			int num = (int)array[i];
			string difficultyLocale = SceneAutoBehaviour<NGame2.NUI.NManager.IllustratedBookManagement>.instance.GetDifficultyLocale(_curArchiveType, num);
			ComboBox_Diff.AddItem(num, difficultyLocale);
		}
	}

	private void SetDifficultyComboBox_GuildRaid()
	{
		GuildRaidChapterData firstData = GameRoot.Instance.GuildRaidChapterDataContainer.GetFirstData();
		if (firstData == null)
		{
			return;
		}
		IList<GuildRaidDungeonData> guildRaidDungeonDataList = GameRoot.Instance.GuildRaidDungeonDataContainer.GetGuildRaidDungeonDataList(firstData.ChapterIndex);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(guildRaidDungeonDataList))
		{
			return;
		}
		GuildRaidDungeonData guildRaidDungeonData = null;
		for (int i = 0; i < guildRaidDungeonDataList.Count; i++)
		{
			guildRaidDungeonData = guildRaidDungeonDataList[i];
			if (guildRaidDungeonData != null)
			{
				ComboBox_Diff.AddItem(guildRaidDungeonData.Step, LocalizationManager.GetStringDirect("GUILD_RAID_STEP_STAGES", guildRaidDungeonData.Step));
			}
		}
		ComboBox_Diff.AddItem(0, "Etc");
	}

	private void SetDifficultyComboBox_Normal()
	{
		DungeonDifficultyType[] array = Enum.GetValues(typeof(DungeonDifficultyType)) as DungeonDifficultyType[];
		for (int i = 0; i < array.Length; i++)
		{
			int num = (int)array[i];
			string difficultyLocale = SceneAutoBehaviour<NGame2.NUI.NManager.IllustratedBookManagement>.instance.GetDifficultyLocale(_curArchiveType, num);
			ComboBox_Diff.AddItem(num, difficultyLocale);
		}
	}

	private void OnChangeDifficulty(int value)
	{
		_currentDifficulty = value;
		RefreshList();
	}

	private void SetDragonRaid()
	{
		RaidType dragonRaidDifficulty = GetDragonRaidDifficulty((DragonRaidDifficulty)_currentDifficulty);
		ICollection<int> raidIndices = GameRoot.Instance.RaidDataContainer.GetRaidIndices(dragonRaidDifficulty);
		if (raidIndices == null)
		{
			return;
		}
		foreach (int item in raidIndices)
		{
			int raidMinLevel = GameRoot.Instance.RaidDataContainer.GetRaidMinLevel(item);
			RaidData data = GameRoot.Instance.RaidDataContainer.GetData(item, raidMinLevel);
			if (data != null)
			{
				List<MonsterHeroInfo> monsterInfoFromDungeon = UIHelper.GetMonsterInfoFromDungeon(new EnterDungeonData(data.ChapterIndex, data.DungeonIndex, 0));
				SetMonsterPortrait(monsterInfoFromDungeon, onlyBoss: true);
			}
		}
		UIHelper.RepositionGrid(PortraitGrid);
	}

	private void SetChallengeRaid()
	{
		ICollection<int> raidIndices = GameRoot.Instance.RaidDataContainer.GetRaidIndices(RaidType.Challenge);
		if (raidIndices == null)
		{
			return;
		}
		int level = 1;
		foreach (int item in raidIndices)
		{
			RaidData data = GameRoot.Instance.RaidDataContainer.GetData(item, level);
			if (data != null)
			{
				List<MonsterHeroInfo> monsterInfoFromDungeon = UIHelper.GetMonsterInfoFromDungeon(new EnterDungeonData(data.ChapterIndex, data.DungeonIndex, 0));
				SetMonsterPortrait(monsterInfoFromDungeon, onlyBoss: true);
			}
		}
		UIHelper.RepositionGrid(PortraitGrid);
	}

	private void SetGuildRaid()
	{
		SetGuildRaidDungeon();
		SetGuildRaidBonusDungeon();
		SetGuildRaidDummyDungeon();
	}

	private void SetGuildRaidDungeon()
	{
		if (ComboBox_Diff.CurrentKey == 0)
		{
			return;
		}
		IList<GuildRaidDungeonData> allData = GameRoot.Instance.GuildRaidDungeonDataContainer.GetAllData();
		if (allData == null)
		{
			return;
		}
		foreach (GuildRaidDungeonData item in allData)
		{
			if (item != null && ComboBox_Diff.CurrentKey == item.Step)
			{
				List<MonsterHeroInfo> monsterInfoFromDungeon = UIHelper.GetMonsterInfoFromDungeon(new EnterDungeonData(item.ChapterIndex, item.DungeonIndex, 0, item.Difficulty));
				SetMonsterPortrait(monsterInfoFromDungeon, onlyBoss: true, item.PreviewBg);
			}
		}
		UIHelper.RepositionGrid(PortraitGrid);
	}

	private void SetGuildRaidBonusDungeon()
	{
		if (ComboBox_Diff.CurrentKey != 0)
		{
			return;
		}
		IList<GuildRaidBonusDungeonData> allData = GameRoot.Instance.GuildRaidBonusDungeonDataContainer.GetAllData();
		if (allData == null)
		{
			return;
		}
		GuildRaidBonusDungeonData guildRaidBonusDungeonData = null;
		for (int i = 0; i < allData.Count; i++)
		{
			guildRaidBonusDungeonData = allData[i];
			if (guildRaidBonusDungeonData != null)
			{
				List<MonsterHeroInfo> monsterInfoFromDungeon = UIHelper.GetMonsterInfoFromDungeon(new EnterDungeonData(guildRaidBonusDungeonData.ChapterIndex, guildRaidBonusDungeonData.DungeonIndex, 0, guildRaidBonusDungeonData.Difficulty));
				SetMonsterPortrait(monsterInfoFromDungeon, onlyBoss: true, guildRaidBonusDungeonData.PreviewBg);
			}
		}
		UIHelper.RepositionGrid(PortraitGrid);
	}

	private void SetGuildRaidDummyDungeon()
	{
		if (ComboBox_Diff.CurrentKey != 0)
		{
			return;
		}
		IList<GuildRaidDummyDungeonData> allData = GameRoot.Instance.GuildRaidDummyDungeonDataContainer.GetAllData();
		if (allData == null)
		{
			return;
		}
		GuildRaidDummyDungeonData guildRaidDummyDungeonData = null;
		for (int i = 0; i < allData.Count; i++)
		{
			guildRaidDummyDungeonData = allData[i];
			if (guildRaidDummyDungeonData != null)
			{
				List<MonsterHeroInfo> monsterInfoFromDungeon = UIHelper.GetMonsterInfoFromDungeon(new EnterDungeonData(guildRaidDummyDungeonData.ChapterIndex, guildRaidDummyDungeonData.DungeonIndex, 0, guildRaidDummyDungeonData.Difficulty));
				SetMonsterPortrait(monsterInfoFromDungeon, onlyBoss: true, guildRaidDummyDungeonData.PreviewBg);
			}
		}
		UIHelper.RepositionGrid(PortraitGrid);
	}

	private void SetGuildSuppress()
	{
		ICollection<int> raidIndices = GameRoot.Instance.RaidDataContainer.GetRaidIndices(RaidType.GuildSuppress);
		if (raidIndices == null)
		{
			return;
		}
		foreach (int item in raidIndices)
		{
			foreach (int raidLevel in GameRoot.Instance.RaidDataContainer.GetRaidLevels(item))
			{
				RaidData data = GameRoot.Instance.RaidDataContainer.GetData(item, raidLevel);
				if (data != null && data.Type == RaidType.GuildSuppress)
				{
					List<MonsterHeroInfo> monsterInfoFromDungeon = UIHelper.GetMonsterInfoFromDungeon(new EnterDungeonData(data.ChapterIndex, data.DungeonIndex, 0));
					SetMonsterPortrait(monsterInfoFromDungeon, onlyBoss: true);
				}
			}
		}
		UIHelper.RepositionGrid(PortraitGrid);
	}

	private void SetWorldBoss()
	{
		foreach (WorldBossData allDatum in GameRoot.Instance.WorldBossDataContainer.GetAllData())
		{
			if (allDatum.IsGlobal)
			{
				List<MonsterHeroInfo> monsterInfoFromDungeon = UIHelper.GetMonsterInfoFromDungeon(new EnterDungeonData(allDatum.ChapterIndex, allDatum.DungeonIndex, 0));
				SetMonsterPortrait(monsterInfoFromDungeon);
			}
		}
		UIHelper.RepositionGrid(PortraitGrid);
	}

	private void SetChapterLastBoss()
	{
		foreach (CampaignDungeonData item in GameRoot.Instance.CampaignDungeonDataContainer.GetAllDataOfBattleType(BattleType.Boss))
		{
			_currentDifficulty = 1;
			EnterDungeonData enterDungeonData = new EnterDungeonData(item.ChapterIndex, item.DungeonIndex, 0, (DungeonDifficultyType)_currentDifficulty);
			if (enterDungeonData.LinkedDungeonData.NextDungeonIndex == 0)
			{
				List<MonsterHeroInfo> monsterInfoFromDungeon = UIHelper.GetMonsterInfoFromDungeon(enterDungeonData);
				SetMonsterPortrait(monsterInfoFromDungeon, onlyBoss: true);
			}
		}
		UIHelper.RepositionGrid(PortraitGrid);
	}

	private void SetConquestBoss()
	{
		foreach (CampaignDungeonData item in GameRoot.Instance.CampaignDungeonDataContainer.GetAllDataOfBattleType(BattleType.Conquest))
		{
			_currentDifficulty = 1;
			List<MonsterHeroInfo> monsterInfoFromDungeon = UIHelper.GetMonsterInfoFromDungeon(new EnterDungeonData(item.ChapterIndex, item.DungeonIndex, 0, (DungeonDifficultyType)_currentDifficulty));
			SetMonsterPortrait(monsterInfoFromDungeon, onlyBoss: true);
		}
		UIHelper.RepositionGrid(PortraitGrid);
	}

	private void SetGodkingTrialBoss()
	{
		IList<CampaignDungeonData> allDataOfBattleType = GameRoot.Instance.CampaignDungeonDataContainer.GetAllDataOfBattleType(BattleType.GodkingTrial);
		List<MonsterHeroInfo> list = new List<MonsterHeroInfo>();
		foreach (CampaignDungeonData item in allDataOfBattleType)
		{
			_currentDifficulty = 0;
			List<MonsterHeroInfo> monsterInfoFromDungeon = UIHelper.GetMonsterInfoFromDungeon(new EnterDungeonData(item.ChapterIndex, item.DungeonIndex, 0, (DungeonDifficultyType)_currentDifficulty));
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(monsterInfoFromDungeon); i++)
			{
				MonsterHeroInfo mon = monsterInfoFromDungeon[i];
				if (list.Find((MonsterHeroInfo boss) => boss.HeroInfo.CreatureData.Index == mon.HeroInfo.CreatureData.Index) == null)
				{
					list.Add(mon);
				}
			}
		}
		SetMonsterPortrait(list, onlyBoss: true);
		UIHelper.RepositionGrid(PortraitGrid);
	}

	private void SetEclipseBoss()
	{
		IList<CampaignDungeonData> allDataOfBattleType = GameRoot.Instance.CampaignDungeonDataContainer.GetAllDataOfBattleType(BattleType.Eclipse);
		List<MonsterHeroInfo> list = new List<MonsterHeroInfo>();
		foreach (CampaignDungeonData item in allDataOfBattleType)
		{
			_currentDifficulty = 0;
			List<MonsterHeroInfo> monsterInfoFromDungeon = UIHelper.GetMonsterInfoFromDungeon(new EnterDungeonData(item.ChapterIndex, item.DungeonIndex, 0, (DungeonDifficultyType)_currentDifficulty));
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(monsterInfoFromDungeon); i++)
			{
				MonsterHeroInfo mon = monsterInfoFromDungeon[i];
				if (list.Find((MonsterHeroInfo boss) => boss.HeroInfo.CreatureData.Index == mon.HeroInfo.CreatureData.Index) == null)
				{
					list.Add(mon);
				}
			}
		}
		SetMonsterPortrait(list, onlyBoss: true);
		UIHelper.RepositionGrid(PortraitGrid);
	}

	private void SetShakmehBoss()
	{
		IList<CampaignDungeonData> allDataOfBattleType = GameRoot.Instance.CampaignDungeonDataContainer.GetAllDataOfBattleType(BattleType.Shakmeh);
		List<MonsterHeroInfo> list = new List<MonsterHeroInfo>();
		foreach (CampaignDungeonData item in allDataOfBattleType)
		{
			_currentDifficulty = 0;
			List<MonsterHeroInfo> monsterInfoFromDungeon = UIHelper.GetMonsterInfoFromDungeon(new EnterDungeonData(item.ChapterIndex, item.DungeonIndex, 0, (DungeonDifficultyType)_currentDifficulty));
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(monsterInfoFromDungeon); i++)
			{
				MonsterHeroInfo mon = monsterInfoFromDungeon[i];
				if (list.Find((MonsterHeroInfo boss) => boss.HeroInfo.CreatureData.Index == mon.HeroInfo.CreatureData.Index) == null)
				{
					list.Add(mon);
				}
			}
		}
		SetMonsterPortrait(list, onlyBoss: true);
		UIHelper.RepositionGrid(PortraitGrid);
	}

	private void SetTechnoMagicBoss()
	{
		RaidType[] array = new RaidType[2]
		{
			RaidType.TechnoMagic,
			RaidType.TechnoEnchant
		};
		ICollection<int> collection = null;
		for (int i = 0; i < array.Length; i++)
		{
			collection = GameRoot.Instance.RaidDataContainer.GetRaidIndices(array[i]);
			if (collection == null)
			{
				continue;
			}
			foreach (int item in collection)
			{
				int raidMinLevel = GameRoot.Instance.RaidDataContainer.GetRaidMinLevel(item);
				RaidData data = GameRoot.Instance.RaidDataContainer.GetData(item, raidMinLevel);
				if (data != null)
				{
					List<MonsterHeroInfo> monsterInfoFromDungeon = UIHelper.GetMonsterInfoFromDungeon(new EnterDungeonData(data.ChapterIndex, data.DungeonIndex, 0));
					SetMonsterPortrait(monsterInfoFromDungeon, onlyBoss: true);
				}
			}
		}
		UIHelper.RepositionGrid(PortraitGrid);
	}

	private void SetPunishmentRaidBoss()
	{
		foreach (int raidIndex in GameRoot.Instance.RaidDataContainer.GetRaidIndices(RaidType.PunishmentRaid))
		{
			int raidMinLevel = GameRoot.Instance.RaidDataContainer.GetRaidMinLevel(raidIndex);
			RaidData data = GameRoot.Instance.RaidDataContainer.GetData(raidIndex, raidMinLevel);
			if (data == null)
			{
				continue;
			}
			List<MonsterHeroInfo> monsterInfoFromDungeon = UIHelper.GetMonsterInfoFromDungeon(new EnterDungeonData(data.ChapterIndex, data.DungeonIndex, 0));
			if (!String_Extension_Method.IsNullOrEmpty(data.RaidInfoCustomIdle))
			{
				foreach (MonsterHeroInfo item in monsterInfoFromDungeon)
				{
					item.BookCustomIdle = data.RaidInfoCustomIdle;
				}
			}
			SetMonsterPortrait(monsterInfoFromDungeon, onlyBoss: true);
		}
		UIHelper.RepositionGrid(PortraitGrid);
	}

	private void SetNormalMonster()
	{
		int lastChapterIndex = GetLastChapterIndex();
		int num = 1;
		int num2 = lastChapterIndex + num;
		for (int i = 0; i < num2; i++)
		{
			MonsterPortraitComponent monsterPortraitComponent = UIHelper.AddGridItem(PortraitGrid, MonsterPortraitComponent.PortraitPrefabAuto);
			if (i < lastChapterIndex)
			{
				monsterPortraitComponent.OpenChapter(i, this);
			}
			else
			{
				monsterPortraitComponent.OpenEtcDungeon(i, this);
			}
			portraitComponentList.Add(monsterPortraitComponent);
		}
		UIHelper.RepositionGrid(PortraitGrid);
	}

	private void SetMonsterPortrait(List<MonsterHeroInfo> MonInfoArray, bool onlyBoss = false, string previewBg = "")
	{
		MonsterPortraitComponent.MonsterPortraitInfo value = default(MonsterPortraitComponent.MonsterPortraitInfo);
		foreach (MonsterHeroInfo item in MonInfoArray)
		{
			if ((!onlyBoss || item.IsBoss) && item.HeroInfo != null && item.HeroInfo.CreatureData != null && item.HeroInfo.CreatureData.ArchiveType == _curArchiveType)
			{
				MonsterPortraitComponent monsterPortraitComponent = UIHelper.AddGridItem(PortraitGrid, MonsterPortraitComponent.PortraitPrefabAuto);
				bool flag = GlobalAutoBehaviour<AccountManager>.instance.ArchiveManager.isClearMonster(_curArchiveType, item.HeroInfo.HeroIndex);
				value.MonsterHeroInfo = item;
				value.IListener = this;
				value.PreviewBg = previewBg;
				value.CreatureData = item.HeroInfo.CreatureData;
				monsterPortraitComponent.Open(_curArchiveType, value, !flag, -1);
				portraitComponentList.Add(monsterPortraitComponent);
			}
		}
	}

	private int GetLastChapterIndex()
	{
		int num = 0;
		foreach (CampaignChapterData allDatum in GameRoot.Instance.CampaignChapterDataContainer.GetAllData())
		{
			if (allDatum.Type == ChapterType.WorldMap && allDatum.MainField && allDatum.IsOpen)
			{
				num++;
			}
		}
		return num;
	}

	public void OnArchiveTypeClick(ArchiveType tagType)
	{
		if (tagType != _curArchiveType)
		{
			_curArchiveType = tagType;
			InitComboBoxItem();
			RefreshList();
		}
	}

	public void OnClickBossViewInfo(IMonsterPortraitProvider iProvider)
	{
		if (_listener != null)
		{
			_listener.OnEnterBossInfo(_curArchiveType, iProvider);
		}
	}

	public void OnClickChapterBook(int ChapterIndex)
	{
		if (_listener != null)
		{
			_listener.OnEnterChapterBook(ChapterIndex);
		}
	}

	public void OnClickChapterMonsterInfo(NormalMonsterArchiveData MonArchiveData)
	{
	}
}
