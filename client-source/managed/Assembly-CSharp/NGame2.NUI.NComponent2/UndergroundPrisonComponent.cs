using System.Collections.Generic;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class UndergroundPrisonComponent : LinkFactoryComponent, ItemIconComponent.Listener
{
	public interface Listener
	{
		void OnClick(UndergroundPrisonComponent comp);
	}

	public const string PrefabPath = "Resources/Prefab2/Component/UndergroundPrison/UndergroundPrisonComponent";

	public static PrefabAuto<UndergroundPrisonComponent> PrefabAuto = new PrefabAuto<UndergroundPrisonComponent>("Resources/Prefab2/Component/UndergroundPrison/UndergroundPrisonComponent");

	public UILabel[] Label_Diffs;

	public UISimpleGrid RewardItemGrid;

	public UILabel Label_Choice;

	public UISprite Sprite_Checked;

	public UISprite Sprite_Selected;

	public UISprite Sprite_NotSelectable;

	private List<ItemIconComponent> _RewardItemIconList = new List<ItemIconComponent>();

	private Listener _listener;

	public int Diff { get; private set; }

	public void Open(int ChapterIndex, int DungeonIndex, bool Selectable, Listener listener)
	{
		Reset();
		_listener = listener;
		SelectRewardData data = GameRoot.Instance.SelectRewardDataContainer.GetData(ChapterIndex, DungeonIndex);
		if (data == null)
		{
			VespaDebug.LogError("SelectRewardData not found");
			return;
		}
		Diff = DungeonIndex;
		SetupDiffText();
		SetupRewardItemIconList(data);
		SetupLabelChoice();
		UIHelper.EnableGameObject(Sprite_NotSelectable, !Selectable);
	}

	public void Reset()
	{
		_listener = null;
		FactoryBase.ReleaseAndClearList(_RewardItemIconList);
	}

	private void SetupDiffText()
	{
		for (int i = 0; i < Label_Diffs.Length; i++)
		{
			UIHelper.EnableGameObject(Label_Diffs[i], i == Diff - 1);
		}
	}

	private void SetupRewardItemIconList(SelectRewardData rewardData)
	{
		if (rewardData == null)
		{
			return;
		}
		rewardData.GetSelectRewardItemList().ForEach(delegate(SelectRewardData.SelectRewardItemInfo info)
		{
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(info.Code);
			if (dataByCode != null)
			{
				ItemIconComponent itemIconComponent = UIHelper.AddGridItem(RewardItemGrid, ItemIconComponent.PrefabAuto);
				itemIconComponent.Open(dataByCode, this);
				int count = info.Count;
				Color? color = null;
				int num = count;
				itemIconComponent.SetItemCount(num, color);
				_RewardItemIconList.Add(itemIconComponent);
			}
		});
		UIHelper.RepositionGrid(RewardItemGrid);
	}

	private void SetupLabelChoice()
	{
		if (!(Label_Choice == null))
		{
			int count = _RewardItemIconList.Count;
			if (count <= 1)
			{
				UIHelper.EnableGameObject(Label_Choice, enable: false);
				return;
			}
			UIHelper.EnableGameObject(Label_Choice, enable: true);
			float cellWidth = RewardItemGrid.Grid.cellWidth;
			float x = RewardItemGrid.transform.localPosition.x + 0.7f * cellWidth + (float)(count - 1) * cellWidth;
			float y = Label_Choice.transform.localPosition.y;
			Label_Choice.transform.localPosition = new Vector3(x, y, 0f);
		}
	}

	public void SetSelected(bool Select)
	{
		UIHelper.EnableGameObject(Sprite_Selected, Select);
		UIHelper.EnableGameObject(Sprite_Checked, Select);
	}

	public void OnClickComponent()
	{
		if (_listener != null)
		{
			_listener.OnClick(this);
		}
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent comp)
	{
		OnClickComponent();
	}
}
