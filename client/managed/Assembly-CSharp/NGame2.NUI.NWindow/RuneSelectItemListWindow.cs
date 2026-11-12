using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWindow;

public class RuneSelectItemListWindow : BaseWindow, ItemIconComponent.Listener, IUserObserver<ItemInfo>, IUserObserver
{
	public interface IListener
	{
		void OnOpenItem(int usingItemIndex, int selectedItemIndex, int selectedItemCount);

		void OnOpenOptionFilterWindow(List<Enum> alreadySelectFilterOption, int selectAbleCount);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Inventory/RuneSelectItemListWindow";

	public UILabel LabelItemName;

	public UIScrollGrid GridItemList;

	public UIButton ButtonOpenOptionFilter;

	public UIButton ButtonOpenItem;

	private IListener _listener;

	private int _selectedItemIndex;

	private int _selectedItemCount;

	private int _selectAbleOptionCount;

	private ItemData _usingItemData;

	private List<Enum> _alreadySelectFilterOption = new List<Enum>();

	private List<ItemIconComponent> _listItemComponent = new List<ItemIconComponent>();

	private List<ItemData> _selectAbleRuneList = new List<ItemData>();

	public void Open(ItemData itemData, int itemCount, IListener listener)
	{
		Open(delegate
		{
			Init(itemData, itemCount, listener);
		}, null);
	}

	private void Init(ItemData itemData, int itemCount, IListener listener)
	{
		if (itemData != null)
		{
			_listener = listener;
			_selectedItemCount = itemCount;
			_usingItemData = itemData;
			UIHelper.SetLabelText(LabelItemName, itemData.Name);
			SetSelectAbleRuneInfo(itemData.Code);
			RefreshRuneItemListApplyFilter(null);
		}
	}

	public override void Reset()
	{
		base.Reset();
		_selectedItemIndex = 0;
		_selectAbleRuneList.Clear();
		_alreadySelectFilterOption.Clear();
		FactoryBase.ReleaseAndClearList(_listItemComponent);
	}

	private void SetSelectAbleRuneInfo(string itemCode)
	{
		if (_selectAbleRuneList == null)
		{
			_selectAbleRuneList = new List<ItemData>();
		}
		SelectItemData data = GameRoot.Instance.SelectItemDataContainer.GetData(itemCode);
		if (data == null)
		{
			return;
		}
		_selectAbleOptionCount = data.SelectAbleCount;
		int num = __Extension_Method_ICollection__.COUNT(data.ItemIndices);
		for (int i = 0; i < num; i++)
		{
			int itemIndex = data.ItemIndices[i];
			ItemData<RuneItemData> data2 = GameRoot.Instance.ItemDataContainer.GetData<RuneItemData>(itemIndex);
			if (data2 != null && data2.SubData != null)
			{
				_selectAbleRuneList.Add(data2);
			}
		}
		_selectAbleRuneList.Sort(delegate(ItemData lhs, ItemData rhs)
		{
			RuneItemData subData = lhs.GetSubData<RuneItemData>();
			RuneItemData subData2 = rhs.GetSubData<RuneItemData>();
			return (subData.ColorType != subData2.ColorType) ? subData.ColorType.CompareTo(subData2.ColorType) : lhs.Index.CompareTo(rhs.Index);
		});
	}

	public void RefreshRuneItemListApplyFilter(List<Enum> selectFilterOptionList)
	{
		FactoryBase.ReleaseAndClearList(_listItemComponent);
		_alreadySelectFilterOption.Clear();
		if (__Extension_Method_ICollection__.COUNT(selectFilterOptionList) > 0)
		{
			_alreadySelectFilterOption.AddRange(selectFilterOptionList);
		}
		_selectedItemIndex = 0;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_selectAbleRuneList); i++)
		{
			ItemData itemData = _selectAbleRuneList[i];
			if (itemData != null && IsSatisfyCondition(selectFilterOptionList, itemData))
			{
				ItemIconComponent itemIconComponent = UIHelper.AddGridItem(GridItemList, ItemIconComponent.PrefabAuto);
				if (itemIconComponent == null)
				{
					return;
				}
				itemIconComponent.Open(itemData, this);
				_listItemComponent.Add(itemIconComponent);
			}
		}
		UIHelper.RepositionGrid(GridItemList);
		UpdateConfirmButtonState();
	}

	private bool IsSatisfyCondition(List<Enum> selectFilterOptionList, ItemData itemData)
	{
		if (__Extension_Method_ICollection__.COUNT(selectFilterOptionList) > 0)
		{
			RuneItemData subData = itemData.GetSubData<RuneItemData>();
			if (subData == null)
			{
				return false;
			}
			Dictionary<Enum, long> statTypeAndValue = subData.GetStatTypeAndValue();
			if (__Extension_Method_ICollection__.COUNT(statTypeAndValue) == 0)
			{
				return false;
			}
			bool result = true;
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(selectFilterOptionList); i++)
			{
				Enum key = selectFilterOptionList[i];
				if (!statTypeAndValue.ContainsKey(key))
				{
					result = false;
					break;
				}
			}
			return result;
		}
		return true;
	}

	private void UpdateConfirmButtonState()
	{
		bool enable = _selectedItemIndex > 0;
		UIHelper.EnableButton(ButtonOpenItem, enable, grayscaleIfDisable: true);
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent selectedItemComp)
	{
		if (selectedItemComp.ItemData == _usingItemData)
		{
			return;
		}
		if (selectedItemComp != null)
		{
			_selectedItemIndex = selectedItemComp.ItemIndex;
			selectedItemComp.SetSelected(selected: true);
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listItemComponent); i++)
		{
			ItemIconComponent itemIconComponent = _listItemComponent[i];
			if (!(itemIconComponent == null) && selectedItemComp != itemIconComponent)
			{
				itemIconComponent.SetSelected(selected: false);
			}
		}
		UpdateConfirmButtonState();
	}

	public void OnClickOpen()
	{
		if (_selectedItemIndex != 0 && _usingItemData != null && _listener != null)
		{
			_listener.OnOpenItem(_usingItemData.Index, _selectedItemIndex, _selectedItemCount);
		}
	}

	public void OnClickOptionFilter()
	{
		if (_listener != null)
		{
			_listener.OnOpenOptionFilterWindow(_alreadySelectFilterOption, _selectAbleOptionCount);
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<ItemInfo>.OnChanged(ItemInfo itemInfo)
	{
		if (itemInfo != null && _usingItemData != null && itemInfo.ItemIndex == _usingItemData.Index)
		{
			Close();
		}
	}
}
