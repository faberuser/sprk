using System.Collections.Generic;
using System.Linq;
using NGame2.NDropBonusEvent;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NSelectRewardPopup;

public class BaseSelectRewardPopup : BaseWindow, ItemIconComponent.Listener
{
	public class BoostedSelectRewardItemInfo
	{
		public SelectRewardData.SelectRewardItemInfo ItemInfo;

		public int BoostedCount;

		public string BoosterCode;
	}

	public UIWidget RootWidget;

	public int MinWidth = 810;

	public int LeftRightMargin = 194;

	public List<SelectRewardButtonInfo> ButtonInfoList = new List<SelectRewardButtonInfo>();

	public SelectRewardItemListInfo SelectRewardItemListInfo;

	protected int _selected;

	protected SelectRewardData _selectRewardData;

	private ItemIconComponent _lastRewardItemIconComponent;

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(SelectRewardItemListInfo.RewardItemIconComponentList);
		_selected = 0;
	}

	protected void SetRewardList(List<BoostedSelectRewardItemInfo> list, SelectRewardItemListInfo info)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(list) || info.CheckIsNull)
		{
			return;
		}
		BoostedSelectRewardItemInfo boostedSelectRewardItemInfo = null;
		ItemData itemData = null;
		ItemIconComponent itemIconComponent = null;
		for (int i = 0; i < list.Count; i++)
		{
			boostedSelectRewardItemInfo = list[i];
			if (boostedSelectRewardItemInfo == null || boostedSelectRewardItemInfo.ItemInfo == null)
			{
				continue;
			}
			itemData = GameRoot.Instance.ItemDataContainer.GetDataByCode(boostedSelectRewardItemInfo.ItemInfo.Code);
			if (itemData != null)
			{
				itemIconComponent = UIHelper.AddGridItem(info.RewardItemGrid, ItemIconComponent.PrefabAuto);
				if (itemData.Type == ItemType.Equip)
				{
					itemIconComponent.Open(itemData, (ItemIconComponent.Listener)null, (bool)this);
				}
				else
				{
					itemIconComponent.Open(itemData, this);
					ItemIconComponent itemIconComponent2 = itemIconComponent;
					int boostedCount = boostedSelectRewardItemInfo.BoostedCount;
					Color? color = null;
					int num = boostedCount;
					itemIconComponent2.SetItemCount(num, color);
					string boosterCode = boostedSelectRewardItemInfo.BoosterCode;
					bool displayBooster = !String_Extension_Method.IsNullOrEmpty(boosterCode);
					itemIconComponent.SetBoosterInReward(displayBooster, boosterCode, displayBoosterLabel: true);
					itemIconComponent.SetInvenCountInReward(displayInvenCount: true, itemData, isLightBackground: true);
				}
				info.RewardItemIconComponentList.Add(itemIconComponent);
			}
		}
		UIHelper.RepositionGrid(info.RewardItemGrid);
	}

	private void UnSelectLastSelected()
	{
		if (_lastRewardItemIconComponent != null)
		{
			_lastRewardItemIconComponent.SetSelected(selected: false);
			_selected--;
			_lastRewardItemIconComponent = null;
		}
	}

	protected void SetButtonType(SelectRewardButtonType buttonType)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(ButtonInfoList))
		{
			return;
		}
		SelectRewardButtonInfo selectRewardButtonInfo = null;
		for (int i = 0; i < ButtonInfoList.Count; i++)
		{
			selectRewardButtonInfo = ButtonInfoList[i];
			if (selectRewardButtonInfo != null)
			{
				if (selectRewardButtonInfo.ButtonRootTranform == null)
				{
					break;
				}
				UIHelper.EnableGameObject(selectRewardButtonInfo.ButtonRootTranform, selectRewardButtonInfo.ButtonType == buttonType);
			}
		}
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent comp)
	{
		if (comp == null)
		{
			return;
		}
		if (!comp.IsSelected())
		{
			if (_selected == _selectRewardData.SelectCount)
			{
				UnSelectLastSelected();
			}
			_selected++;
			comp.SetSelected(selected: true);
			_lastRewardItemIconComponent = comp;
		}
		else
		{
			if (_selected == 0)
			{
				return;
			}
			_selected--;
			comp.SetSelected(selected: false);
			if (_lastRewardItemIconComponent != null && Vespa_Common_Class_Extension_Method.EQUALS(_lastRewardItemIconComponent, comp))
			{
				_lastRewardItemIconComponent = null;
			}
		}
		EnableOKButtonState(_selected == _selectRewardData.SelectCount);
	}

	protected void EnableOKButtonState(bool enable)
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(ButtonInfoList))
		{
			for (int i = 0; ButtonInfoList.Count > i; i++)
			{
				UIHelper.EnableButton(ButtonInfoList[i].OKButton, enable);
			}
		}
	}

	public static List<BoostedSelectRewardItemInfo> ApplyBonusItemCount(BattleType battleType, List<SelectRewardData.SelectRewardItemInfo> infoList)
	{
		List<BoostedSelectRewardItemInfo> list = new List<BoostedSelectRewardItemInfo>();
		int num = 0;
		List<DropBonusEvent> allEffectiveEvent = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetAllEffectiveEvent(EventBonusType.BonusItem);
		string text = null;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(allEffectiveEvent); i++)
		{
			DropBonusEvent dropBonusEvent = allEffectiveEvent[i];
			if (dropBonusEvent.DungeonType == null || dropBonusEvent.DungeonType.Length == 0 || dropBonusEvent.DungeonType.Contains(battleType))
			{
				num += dropBonusEvent.BonusPercentage;
				if (text == null)
				{
					text = BoosterManager.GetEventIconName(dropBonusEvent);
				}
			}
		}
		foreach (SelectRewardData.SelectRewardItemInfo info in infoList)
		{
			BoostedSelectRewardItemInfo item = new BoostedSelectRewardItemInfo
			{
				ItemInfo = info,
				BoostedCount = info.Count
			};
			if (num > 0)
			{
				int num2 = info.Count * num;
				int num3 = num2 / 100;
				if (num2 > num3 * 100)
				{
					num3++;
				}
			}
			list.Add(item);
		}
		return list;
	}
}
