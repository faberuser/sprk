using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NPortrait;
using NNguiExtension;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class ChapterMonsterBook : BaseWindow, ChapterTabComponent.Listener, MonsterPortraitComponent.IListener
{
	public interface Listener
	{
	}

	public const string PrefabPath = "Resources/Prefab2/Window/IllustratedBook/ChapterMonsterBook";

	private Listener _listener;

	public UIScrollGrid DungeonTabGrid;

	public UIScrollGrid PortraitGrid;

	public UIWidget ComboBoxWidget;

	public UIComboBox ComboBox_EtcDungeonType;

	public UISprite Sprite_Progress;

	public UILabel Label_Progress;

	private UIProgressBar Sprite_ProgressBar;

	private List<ChapterTabComponent> DungeonTabComponentList = new List<ChapterTabComponent>();

	private List<MonsterPortraitComponent> portraitComponentList = new List<MonsterPortraitComponent>();

	private int _curChapterIndex;

	private int _maxChapterIndex;

	private EtcDungeonType _curEtcDungeonType;

	private Dictionary<int, List<int>> MonIndexListByTab = new Dictionary<int, List<int>>();

	private List<int> MonList_currentTab = new List<int>();

	private string[] ETC_DUNGEON_LOCALE = new string[5] { "NODE_UNDERPRISON_NAME", "CHALLENGETOWER_TITLE", "MAZE_NAME", "ARCHIVE_ECLIPSE", "MAP_SHAKMEH_CHAPTER" };

	public void Open(int ChapterIndex, Listener listener)
	{
		_listener = listener;
		_curChapterIndex = ChapterIndex;
		_maxChapterIndex = GetLastChapterIndex() + 1;
		base.Open(delegate
		{
			Init(ChapterIndex);
		});
	}

	public override void Reset()
	{
		base.Reset();
		Clear();
		MonIndexListByTab.Clear();
		ComboBox_EtcDungeonType.Clear();
		FactoryBase.ReleaseAndClearList(DungeonTabComponentList);
	}

	public void Clear()
	{
		MonList_currentTab.Clear();
		FactoryBase.ReleaseAndClearList(portraitComponentList);
	}

	public void Init(int ChapterIndex)
	{
		InitDungeonTab(ChapterIndex);
		InitMonListByTab();
		RefreshList();
	}

	private void InitDungeonTab(int ChapterIndex)
	{
		int num = 1;
		int num2 = _maxChapterIndex + num;
		for (int i = 1; i < num2; i++)
		{
			ChapterTabComponent chapterTabComponent = UIHelper.AddGridItem(DungeonTabGrid, ChapterTabComponent.PrefabAuto);
			chapterTabComponent.Init(i, this);
			chapterTabComponent.SelectToggle(ChapterIndex == i);
			DungeonTabComponentList.Add(chapterTabComponent);
		}
		UIHelper.RepositionGrid(DungeonTabGrid);
		ScrollToSelectedComponent();
	}

	private void ScrollToSelectedComponent()
	{
		ChapterTabComponent comp = DungeonTabComponentList.Find((ChapterTabComponent chapterTabComponent) => chapterTabComponent.IsSelected());
		DungeonTabGrid.ScrollTo(comp, 0f, 0f);
	}

	private void InitMonListByTab()
	{
		if (MonIndexListByTab != null)
		{
			for (int i = 1; i <= _maxChapterIndex; i++)
			{
				CampaignDungeonData[] dungeonDatas = GameRoot.Instance.CampaignDungeonDataContainer.GetAllChapterData(i).ToArray();
				List<int> dungeonMonsterIndexList = GetDungeonMonsterIndexList(dungeonDatas);
				MonIndexListByTab.Add(i, dungeonMonsterIndexList);
			}
			EtcDungeonType[] array = Enum.GetValues(typeof(EtcDungeonType)) as EtcDungeonType[];
			foreach (EtcDungeonType curType in array)
			{
				BattleType battleType = GetBattleType(curType);
				IList<CampaignDungeonData> allDataOfBattleType = GameRoot.Instance.CampaignDungeonDataContainer.GetAllDataOfBattleType(battleType);
				List<int> dungeonMonsterIndexList2 = GetDungeonMonsterIndexList(allDataOfBattleType.ToArray());
				int etcTabDetailIndex = GetEtcTabDetailIndex(curType);
				MonIndexListByTab.Add(etcTabDetailIndex, dungeonMonsterIndexList2);
			}
		}
	}

	public void RefreshList()
	{
		SetComboBox();
		if (IsChapterDungeon())
		{
			SetChapterMonsterList(_curChapterIndex);
			SetGauge(_curChapterIndex);
		}
		else
		{
			SetEtcDungeonList(_curEtcDungeonType);
			SetGauge(GetEtcTabDetailIndex(_curEtcDungeonType));
		}
		UIHelper.RepositionGrid(PortraitGrid);
		PortraitGrid.ResetScroll();
	}

	private void SetComboBox()
	{
		if (!(ComboBox_EtcDungeonType == null))
		{
			bool flag = !IsChapterDungeon();
			UIHelper.EnableGameObject(ComboBox_EtcDungeonType.SelectedLabel.gameObject, flag);
			UIHelper.SetWidgetGrayScale(ComboBoxWidget, !flag, setColliders: true);
			UIHelper.EnableCollider(ComboBoxWidget, flag);
			ComboBox_EtcDungeonType.enabled = flag;
			if (flag)
			{
				SetEtcDungeonTabComboBox();
			}
		}
	}

	private void SetEtcDungeonTabComboBox()
	{
		ComboBox_EtcDungeonType.Clear();
		ComboBox_EtcDungeonType.OnChanged = OnChangeEtcDungeonType;
		EtcDungeonType[] array = Enum.GetValues(typeof(EtcDungeonType)) as EtcDungeonType[];
		for (int i = 0; i < array.Length; i++)
		{
			EtcDungeonType etcDungeonType = array[i];
			int num = (int)etcDungeonType;
			string stringDirect = LocalizationManager.GetStringDirect(Vespa_Generic_IList_Extension_Method.IsValidIndex(ETC_DUNGEON_LOCALE, num) ? ETC_DUNGEON_LOCALE[num] : etcDungeonType.ToString());
			ComboBox_EtcDungeonType.AddItem(num, stringDirect);
		}
		ComboBox_EtcDungeonType.SelectItem((int)_curEtcDungeonType, isNotify: false);
		ComboBox_EtcDungeonType.Reposition();
	}

	private void SetChapterMonsterList(int chapterIndex)
	{
		Clear();
		foreach (CampaignDungeonData allChapterDatum in GameRoot.Instance.CampaignDungeonDataContainer.GetAllChapterData(chapterIndex))
		{
			if (allChapterDatum.BattleDefineCode == (HashString)"Tutorial" || allChapterDatum.BattleDefineCode == (HashString)"Tutorial2")
			{
				continue;
			}
			DungeonDifficultyType[] obj = Enum.GetValues(typeof(DungeonDifficultyType)) as DungeonDifficultyType[];
			DungeonDifficultyType maxDifficulty = GameRoot.Instance.CampaignWaveDataContainer.GetWaveCollection(allChapterDatum.ChapterIndex, allChapterDatum.DungeonIndex).MaxDifficulty;
			DungeonDifficultyType[] array = obj;
			foreach (DungeonDifficultyType dungeonDifficultyType in array)
			{
				if (dungeonDifficultyType <= maxDifficulty)
				{
					List<MonsterHeroInfo> monsterInfoFromDungeon = UIHelper.GetMonsterInfoFromDungeon(new EnterDungeonData(allChapterDatum.ChapterIndex, allChapterDatum.DungeonIndex, 0, dungeonDifficultyType));
					SetMonsterPortrait(chapterIndex, dungeonDifficultyType, monsterInfoFromDungeon);
				}
			}
		}
	}

	private void SetUnderPrisonMonsterList()
	{
		IList<UnderPrisonDungeonData> allData = GameRoot.Instance.UnderPrisonDungeonDataContainer.GetAllData();
		if (allData == null)
		{
			return;
		}
		UnderPrisonDungeonData underPrisonDungeonData = null;
		EnterDungeonData enterDungeonData = null;
		for (int i = 0; i < allData.Count; i++)
		{
			underPrisonDungeonData = allData[i];
			if (underPrisonDungeonData != null)
			{
				enterDungeonData = new EnterDungeonData(underPrisonDungeonData.ChapterIndex, underPrisonDungeonData.DungeonIndex, underPrisonDungeonData.Diff, DungeonDifficultyType.Easy);
				SetMonsterPortrait(GetEtcTabDetailIndex(EtcDungeonType.Prison), DungeonDifficultyType.Easy, UIHelper.GetMonsterInfoFromDungeon(enterDungeonData));
			}
		}
	}

	private void SetShakmehMonsterList()
	{
		int chapterIndex = GameRoot.Instance.ShakmehBossDataContainer.GetDataByBossType(ShakmehBossType.Final).ChapterIndex;
		foreach (CampaignDungeonData allChapterDatum in GameRoot.Instance.CampaignDungeonDataContainer.GetAllChapterData(chapterIndex))
		{
			if (!(allChapterDatum.BattleDefineCode == (HashString)"Tutorial") && !(allChapterDatum.BattleDefineCode == (HashString)"Tutorial2"))
			{
				DungeonDifficultyType[] array = Enum.GetValues(typeof(DungeonDifficultyType)) as DungeonDifficultyType[];
				foreach (DungeonDifficultyType difficulty in array)
				{
					UIHelper.GetMonsterInfoFromDungeon(new EnterDungeonData(allChapterDatum.ChapterIndex, allChapterDatum.DungeonIndex, 0, difficulty));
				}
			}
		}
	}

	private void SetEtcDungeonList(EtcDungeonType etcDungeonType)
	{
		Clear();
		BattleType battleType = GetBattleType(etcDungeonType);
		if (battleType == BattleType.UnderPrison)
		{
			SetUnderPrisonMonsterList();
			return;
		}
		foreach (CampaignDungeonData item in GameRoot.Instance.CampaignDungeonDataContainer.GetAllDataOfBattleType(battleType))
		{
			if (!(item.BattleDefineCode == (HashString)"Tutorial") && !(item.BattleDefineCode == (HashString)"Tutorial2"))
			{
				DungeonDifficultyType[] array = Enum.GetValues(typeof(DungeonDifficultyType)) as DungeonDifficultyType[];
				foreach (DungeonDifficultyType difficulty in array)
				{
					List<MonsterHeroInfo> monsterInfoFromDungeon = UIHelper.GetMonsterInfoFromDungeon(new EnterDungeonData(item.ChapterIndex, item.DungeonIndex, 0, difficulty));
					SetMonsterPortrait(GetEtcTabDetailIndex(etcDungeonType), difficulty, monsterInfoFromDungeon);
				}
			}
		}
	}

	private void SetMonsterPortrait(int sortIndex, DungeonDifficultyType difficulty, List<MonsterHeroInfo> MonInfoArray)
	{
		foreach (MonsterHeroInfo item in MonInfoArray)
		{
			int heroIndex = item.HeroInfo.HeroIndex;
			if (GameRoot.Instance.CreatureDataContainer.GetData(heroIndex).ArchiveType != ArchiveType.Normal || IsAlreadyContain(sortIndex, heroIndex))
			{
				continue;
			}
			if (!MonList_currentTab.Contains(heroIndex))
			{
				MonsterPortraitComponent monsterPortraitComponent = UIHelper.AddGridItem(PortraitGrid, MonsterPortraitComponent.SmallPrefabAuto);
				bool flag = GlobalAutoBehaviour<AccountManager>.instance.ArchiveManager.isClearMonster(ArchiveType.Normal, heroIndex);
				NormalMonsterArchiveData normalMonsterArchiveData = new NormalMonsterArchiveData(heroIndex, difficulty);
				normalMonsterArchiveData.AddMonsterInfo(difficulty, item);
				monsterPortraitComponent.Open(ArchiveType.Normal, normalMonsterArchiveData, this, !flag);
				portraitComponentList.Add(monsterPortraitComponent);
				MonList_currentTab.Add(heroIndex);
				continue;
			}
			foreach (MonsterPortraitComponent portraitComponent in portraitComponentList)
			{
				if (portraitComponent.MonsterArchiveData.MonsterIndex == heroIndex)
				{
					portraitComponent.MonsterArchiveData.AddMonsterInfo(difficulty, item);
					portraitComponent.MonsterArchiveData.MainDifficulty = difficulty;
					break;
				}
			}
		}
	}

	private void SetGauge(int tabIndex)
	{
		if (MonIndexListByTab == null || !MonIndexListByTab.ContainsKey(tabIndex))
		{
			return;
		}
		if (Sprite_ProgressBar == null)
		{
			Sprite_ProgressBar = Sprite_Progress.gameObject.AddComponent<UIProgressBar>();
			Sprite_ProgressBar.foregroundWidget = Sprite_Progress;
		}
		int num = 0;
		List<int> monsterList = GlobalAutoBehaviour<AccountManager>.instance.ArchiveManager.GetMonsterList(ArchiveType.Normal);
		int num2 = monsterList?.Count ?? 0;
		int count = MonList_currentTab.Count;
		for (int i = 0; i < num2; i++)
		{
			if (MonList_currentTab.Contains(monsterList[i]))
			{
				num++;
			}
		}
		Sprite_ProgressBar.value = (float)num / (float)count;
		string text = $"{Sprite_ProgressBar.value * 100f:N2}% " + $"({num}/{count})";
		UIHelper.SetLabelText(Label_Progress, text);
	}

	private bool IsChapterDungeon()
	{
		return _curChapterIndex < _maxChapterIndex;
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

	private int GetEtcTabDetailIndex(EtcDungeonType curType)
	{
		return (int)(_maxChapterIndex + curType + 1);
	}

	private BattleType GetBattleType(EtcDungeonType curType)
	{
		return curType switch
		{
			EtcDungeonType.ChallengeTower => BattleType.ChallengeTower, 
			EtcDungeonType.Prison => BattleType.UnderPrison, 
			_ => BattleType.Hideout, 
		};
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

	private bool IsAlreadyContain(int typeIndex, int monIndex)
	{
		for (int i = 1; i < typeIndex; i++)
		{
			if (MonIndexListByTab.ContainsKey(i) && MonIndexListByTab[i].Contains(monIndex))
			{
				return true;
			}
		}
		return false;
	}

	private void OnChangeEtcDungeonType(int value)
	{
		_curEtcDungeonType = (EtcDungeonType)value;
		RefreshList();
	}

	public void OnChapterIndexClick(int chapterIndex)
	{
		if (chapterIndex != _curChapterIndex)
		{
			_curChapterIndex = chapterIndex;
			RefreshList();
		}
	}

	public void OnClickChapterMonsterInfo(NormalMonsterArchiveData MonArchiveData)
	{
		if (MonArchiveData != null)
		{
			MonsterArchiveInfoPopup.Open(MonArchiveData, _curChapterIndex, IsChapterDungeon());
		}
	}

	public void OnClickBossViewInfo(IMonsterPortraitProvider iProvider)
	{
	}

	public void OnClickChapterBook(int ChapterIndex)
	{
	}
}
