using System.Collections.Generic;
using System.Linq;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class GuildRaidRewardInfo : BaseWindow, GuildRaidLevelComponent.Listener
{
	public const string PrefabPath = "Resources/Prefab2/Window/GuildRaid/GuildRaidRewardInfo";

	public UIScrollGrid LevelScrollGrid;

	public UIScrollGrid RewardScrollGrid;

	public UIScrollGrid StorageScrollGrid;

	public DungeonDifficultyComponent2 DifficultyComponent;

	private List<GuildRaidLevelComponent> LevelComponentList = new List<GuildRaidLevelComponent>();

	private List<Component> RewardIconList = new List<Component>();

	private List<Component> StorageItemIconList = new List<Component>();

	private int Chapter;

	private int CurrentDifficult;

	private int CurrentIndex;

	private bool IsModified;

	private Dictionary<Tuple<int, int>, GuildRaidDungeonData> DungeonDatas = new Dictionary<Tuple<int, int>, GuildRaidDungeonData>();

	private int MaxDifficulty;

	public static void Open(int chapter, int dungoeonIndex = 1, int difficulty = 0)
	{
		WindowPrefab<GuildRaidRewardInfo> windowPrefab = new WindowPrefab<GuildRaidRewardInfo>("Resources/Prefab2/Window/GuildRaid/GuildRaidRewardInfo", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(chapter, dungoeonIndex, difficulty);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	private void OpenInternal(int chapter, int dungoeonIndex, int difficulty)
	{
		base.Open();
		Chapter = chapter;
		MaxDifficulty = GetMaxDifficulty(chapter, dungoeonIndex);
		difficulty = Mathf.Min(difficulty, MaxDifficulty);
		InitRewardInfo(chapter);
		InitLevelComponents();
		DifficultyComponent.Init((DungeonDifficultyType)difficulty, (DungeonDifficultyType)MaxDifficulty);
		SelectLevel(dungoeonIndex);
		SelectDifficult(difficulty);
	}

	private int GetMaxDifficulty(int chapterIndex, int dungeonIndex)
	{
		for (DungeonDifficultyType dungeonDifficultyType = DungeonDifficultyType.Hell; dungeonDifficultyType >= DungeonDifficultyType.Easy; dungeonDifficultyType--)
		{
			if (GameRoot.Instance.GuildRaidDungeonDataContainer.GetData(chapterIndex, dungeonIndex, (int)dungeonDifficultyType) != null)
			{
				return (int)dungeonDifficultyType;
			}
		}
		return 0;
	}

	public override void Reset()
	{
		FactoryBase.ReleaseAndClearList(LevelComponentList);
		FactoryBase.ReleaseAndClearList(RewardIconList);
		FactoryBase.ReleaseAndClearList(StorageItemIconList);
		base.Reset();
	}

	private void Update()
	{
		if (IsModified)
		{
			CreateRewardItemIcons();
			IsModified = false;
		}
	}

	private void InitRewardInfo(int chapter)
	{
		DungeonDatas.Clear();
		_ = from data in GameRoot.Instance.GuildRaidDungeonDataContainer.GetAllData()
			where data.ChapterIndex == chapter
			select data;
	}

	private void InitLevelComponents()
	{
		LevelComponentList.Clear();
		foreach (KeyValuePair<Tuple<int, int>, GuildRaidDungeonData> info in DungeonDatas)
		{
			if (!(LevelComponentList.Find((GuildRaidLevelComponent item) => item.Index == info.Value.DungeonIndex) != null))
			{
				GuildRaidLevelComponent guildRaidLevelComponent = LevelScrollGrid.AddGridItem(GuildRaidLevelComponent.PrefabAuto);
				guildRaidLevelComponent.SetData(info.Value.DungeonIndex, this);
				LevelComponentList.Add(guildRaidLevelComponent);
			}
		}
		LevelScrollGrid.RepositionGrid();
	}

	private void CreateRewardItemIcons()
	{
		Tuple<int, int> key = new Tuple<int, int>(CurrentIndex, CurrentDifficult);
		if (DungeonDatas.ContainsKey(key))
		{
			GuildRaidDungeonData guildRaidDungeonData = DungeonDatas[key];
			FactoryBase.ReleaseAndClearList(RewardIconList);
			FactoryBase.ReleaseAndClearList(StorageItemIconList);
			List<Component> collection = BaseDungeonInfo.AddRewardItemToGrid(GameRoot.Instance.RewardDataContainer.GetData(guildRaidDungeonData.KillRewardIndex), RewardScrollGrid, includeCurrency: true, null, BattleType.GuildRaid);
			RewardIconList.AddRange(collection);
			collection = BaseDungeonInfo.AddRewardItemToGrid(GameRoot.Instance.RewardDataContainer.GetData(guildRaidDungeonData.BuyableRewardIndex), StorageScrollGrid, includeCurrency: true, null, BattleType.GuildRaid);
			StorageItemIconList.AddRange(collection);
			StorageScrollGrid.RepositionGrid();
			RewardScrollGrid.RepositionGrid();
		}
	}

	private void SelectLevel(int index)
	{
		foreach (GuildRaidLevelComponent levelComponent in LevelComponentList)
		{
			levelComponent.ShowSelectImage(index == levelComponent.Index);
		}
		CurrentIndex = index;
		IsModified = true;
	}

	private void SelectDifficult(int difficult)
	{
		if (difficult <= MaxDifficulty)
		{
			if (DifficultyComponent != null)
			{
				DifficultyComponent.SelectDifficuly(difficult);
			}
			CurrentDifficult = difficult;
			IsModified = true;
		}
	}

	public void OnClickDifficultyEasy()
	{
		SelectDifficult(0);
	}

	public void OnClickDifficultyNormal()
	{
		SelectDifficult(1);
	}

	public void OnClickDifficultyHard()
	{
		SelectDifficult(2);
	}

	void GuildRaidLevelComponent.Listener.OnClickLevel(int index)
	{
		SelectLevel(index);
	}
}
