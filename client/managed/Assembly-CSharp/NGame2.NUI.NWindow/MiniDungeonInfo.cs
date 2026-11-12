using System.Collections.Generic;
using NGame2.NAccount;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class MiniDungeonInfo : BaseWindow
{
	public enum EAnimation
	{
		None,
		Show,
		Hide
	}

	public const string PrefabPath = "Resources/Prefab2/Lobby/MiniDungeonInfo";

	public const float NormalIconScale = 0.7f;

	public const float BossIconScale = 0.7f;

	public float ExtraAddWidth = 120f;

	public float MinWidth = 420f;

	public float LeftRightMargin = 78f;

	public float AnimationSpeed = 300f;

	public UIWidget PivotWidget;

	public UIWidget RootWidget;

	public UIWidget MonsterPivot;

	public UIGrid ItemGrid;

	public UIWidget BarSprite;

	public UIScrollGrid ItemListGrid;

	private float _firstMonsterLeft;

	private float _lastMonsterRight;

	private List<Component> _itemIconList = new List<Component>();

	private EAnimation _animation;

	public int OpenedChapterIndex { get; private set; }

	public int OpenedDungeonIndex { get; private set; }

	public void Open(int chapterIndex, int dungeonIndex)
	{
		if (IsRewardItemExist(chapterIndex, dungeonIndex))
		{
			OpenedChapterIndex = chapterIndex;
			OpenedDungeonIndex = dungeonIndex;
			ClearAll();
			Open(delegate
			{
				SetupDropItems(OpenedChapterIndex, OpenedDungeonIndex);
				float a = (float)_itemIconList.Count * ItemGrid.cellWidth + LeftRightMargin;
				RootWidget.width = Mathf.RoundToInt(Mathf.Max(a, MinWidth));
				ItemGrid.Reposition();
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(RootWidget.transform);
			}, null);
		}
	}

	public override void Reset()
	{
		base.Reset();
		_animation = (IsOpen() ? EAnimation.Show : EAnimation.Hide);
		ClearAll();
	}

	public override void OnClosing()
	{
		base.OnClosing();
		ClearAll();
	}

	private void ClearAll()
	{
		FactoryBase.ReleaseAndClearList(_itemIconList);
	}

	private bool IsRewardItemExist(int chapterIndex, int dungeonIndex)
	{
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data == null)
		{
			return false;
		}
		int difficulty = Helper.GetDifficulty(chapterIndex, dungeonIndex);
		int rewardIndexOfDifficulty = data.GetRewardIndexOfDifficulty((DungeonDifficultyType)difficulty);
		RewardData data2 = GameRoot.Instance.RewardDataContainer.GetData(rewardIndexOfDifficulty);
		if (data2 == null)
		{
			return false;
		}
		List<string> rewardItemCodeList = data2.GetRewardItemCodeList();
		int num = rewardItemCodeList.Count;
		for (int i = 0; i < rewardItemCodeList.Count; i++)
		{
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(rewardItemCodeList[i]);
			if (dataByCode == null)
			{
				num--;
			}
			else if (dataByCode.Type == ItemType.Hero)
			{
				num--;
			}
		}
		return num > 0;
	}

	private void SetupDropItems(int chapterIndex, int dungeonIndex)
	{
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data != null)
		{
			int maxAvailableDifficulty = Helper.GetMaxAvailableDifficulty(chapterIndex, dungeonIndex);
			int rewardIndexOfDifficulty = data.GetRewardIndexOfDifficulty((DungeonDifficultyType)maxAvailableDifficulty);
			RewardData data2 = GameRoot.Instance.RewardDataContainer.GetData(rewardIndexOfDifficulty);
			if (data2 != null)
			{
				List<Component> collection = BaseDungeonInfo.AddRewardItemToGrid(data2, ItemListGrid, includeCurrency: false, null, data.BattleType);
				_itemIconList.AddRange(collection);
			}
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
