using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NWindow.NValance;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class ValanceIdentifiedWindow : BaseCinemaWindow, ItemIconComponent.Listener, UIScrollItemBehaviour.IListener, IUserObserver<UserManager.UserStatInfo>, IUserObserver, IUserObserver<NGame2.NAccount.UserEquipItemManager.EquipStatInfo>
{
	public struct SelectedInfo
	{
		private HashSet<int> _hashSlotIndex;

		private int _reqGoldSum;

		private int _selectMaxCount;

		public void SetSelectMaxCount(int maxCount)
		{
			_selectMaxCount = maxCount;
		}

		public bool IsSelectMax(int slotIndex)
		{
			if (_hashSlotIndex != null && _hashSlotIndex.Contains(slotIndex))
			{
				return false;
			}
			return __Extension_Method_ICollection__.COUNT(_hashSlotIndex) == _selectMaxCount;
		}

		public bool IsSelected(int slotIndex)
		{
			if (_hashSlotIndex == null)
			{
				return false;
			}
			return _hashSlotIndex.Contains(slotIndex);
		}

		public bool Selected(EquipItemInfo equipItemInfo)
		{
			if (equipItemInfo == null)
			{
				return false;
			}
			if (_hashSlotIndex == null)
			{
				_hashSlotIndex = new HashSet<int>();
			}
			int slotIndex = equipItemInfo.SlotIndex;
			int reqGold = GetReqGold(equipItemInfo.EquipItemData);
			if (_hashSlotIndex.Contains(slotIndex))
			{
				_hashSlotIndex.Remove(slotIndex);
				_reqGoldSum -= reqGold;
				return false;
			}
			_hashSlotIndex.Add(slotIndex);
			_reqGoldSum += reqGold;
			return true;
		}

		private int GetReqGold(EquipItemData equipItemData)
		{
			if (equipItemData == null)
			{
				return 0;
			}
			int detailIndex = equipItemData.DetailIndex;
			return GameRoot.Instance.ValanceIdentifiedDataContainer.GetData(detailIndex)?.ReqGold ?? 0;
		}

		public bool IsAnySelected()
		{
			return !__Extension_Method_ICollection__.IsNullOrEmpty(_hashSlotIndex);
		}

		public int[] GetSelectedSlotIndieces()
		{
			if (__Extension_Method_ICollection__.IsNullOrEmpty(_hashSlotIndex))
			{
				return null;
			}
			return _hashSlotIndex.ToArray();
		}

		public int GetReqGoldSum()
		{
			return _reqGoldSum;
		}

		public void Clear()
		{
			if (_hashSlotIndex != null)
			{
				_hashSlotIndex.Clear();
			}
			_reqGoldSum = 0;
		}
	}

	public interface IListener
	{
		void OnClickIdentified(int[] slotIndices);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Valance/ValanceIdentifiedWindow";

	public UILabel LabelInfo;

	public UILabel LabelDiscount;

	public UILabel LabelReqGold;

	public GameObject ButtonEventGo;

	public UILabelAligner LabelAligner;

	public GameObject GoEmpty;

	public UIReuseScrollGrid UIReuseScrollGrid;

	public GameObject EquipItemRoot;

	public ValanceEquipFilter FilterPanel;

	public UIButton ButtonIndentfied;

	public UIWidget IconProxyRoot;

	public Color ColorGoldEnough = Color.white;

	public Color ColorGoldNotEnough = Color.red;

	private List<EquipItemInfo> _list;

	private List<ItemIconComponent> _listItemIconCompoenent = new List<ItemIconComponent>();

	private IconProxy _iconProxy;

	private int _selectedLimitCount;

	private IListener _listener;

	private bool _fromInven;

	private EquipItemInfo _fromInvenItem;

	private SelectedInfo _selectedInfo;

	public void Open(IListener listener)
	{
		_listener = listener;
		Open(delegate
		{
			Init(null);
		}, null);
	}

	public void OpenCustomCinema(EquipItemInfo selectedEquipItemInfo, IListener listener)
	{
		_listener = listener;
		OpenCustomCinema("ValanceIdentifiedWindowInven", delegate
		{
			Init(selectedEquipItemInfo);
		}, null);
	}

	public void Init(EquipItemInfo info)
	{
		_fromInven = info != null;
		if (_fromInven)
		{
			_selectedInfo.Selected(info);
			_fromInvenItem = info;
		}
		ShowIdentifiedPanel(isOnItemPanel: true);
		SetSelectableCount();
		SetFilter();
		SetReuseItemIcon();
		SetIconProxy();
		SetReqGoldLabel();
		GlobalAutoBehaviour<AccountManager>.instance.AddUserObserver(this);
	}

	public void SetIconProxy()
	{
		if (!(IconProxyRoot == null))
		{
			if (_iconProxy == null)
			{
				_iconProxy = IconProxy.Create(IconProxyRoot);
			}
			if (_iconProxy != null)
			{
				_iconProxy.SetupCurrencyNoBg(CurrencyType.Gold);
			}
		}
	}

	public void SetSelectableCount()
	{
		_selectedLimitCount = GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxValanceIdentifiedCount", 10);
		_selectedInfo.SetSelectMaxCount(_selectedLimitCount);
		string stringDirect = LocalizationManager.GetStringDirect("VALANCE_IDENTIFIED_LABEL", _selectedLimitCount);
		UIHelper.SetLabelText(LabelInfo, stringDirect);
	}

	public void SetFilter()
	{
		FilterPanel.SetFilterGroupComponent();
	}

	private void ReleaseItemIcons()
	{
		if (UIReuseScrollGrid != null)
		{
			UIReuseScrollGrid.ClearGridItem<ItemIconComponent>();
		}
		if (_list != null)
		{
			_list.Clear();
			_list = null;
		}
		_listItemIconCompoenent.Clear();
	}

	public void SetReuseItemIcon()
	{
		ReleaseItemIcons();
		if (!(UIReuseScrollGrid == null))
		{
			UIHelper.RepositionGrid(UIReuseScrollGrid);
			_list = ValanceHelper.GetFiltedTechnoMagicItems(Filter, _fromInvenItem);
			if (_fromInven)
			{
				_fromInvenItem = null;
				_fromInven = false;
			}
			int totalCount = __Extension_Method_ICollection__.COUNT(_list);
			UIReuseScrollGrid.CreateReuseItem(ItemIconComponent.PrefabAuto, _listItemIconCompoenent, totalCount, this);
			bool enable = __Extension_Method_ICollection__.COUNT(_listItemIconCompoenent) == 0;
			UIHelper.EnableGameObject(GoEmpty, enable);
			UIHelper.RepositionGrid(UIReuseScrollGrid);
		}
	}

	private bool Filter(EquipItemInfo info)
	{
		if (info.GetInventoryType() != InventoryType.Inventory)
		{
			return false;
		}
		if (info.IsIdentified)
		{
			return false;
		}
		return FilterPanel.Filter(info.EquipItemData);
	}

	void UIScrollItemBehaviour.IListener.ChangedItemIndex(Component comp, int index)
	{
		if (!(comp == null) && __Extension_Method_ICollection__.COUNT(_list) > index)
		{
			ItemIconComponent itemIconComponent = comp as ItemIconComponent;
			if (itemIconComponent != null)
			{
				EquipItemInfo equipItemInfo = _list[index];
				itemIconComponent.Open(equipItemInfo, this);
				bool selected = _selectedInfo.IsSelected(equipItemInfo.SlotIndex);
				itemIconComponent.SetSelected(selected);
			}
		}
	}

	private int GetEventDiscount()
	{
		return 0;
	}

	public void SetReqGoldLabel()
	{
		int reqGoldSum = _selectedInfo.GetReqGoldSum();
		long num = reqGoldSum;
		bool flag = num <= GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Gold;
		Color color = (flag ? ColorGoldEnough : ColorGoldNotEnough);
		int eventDiscount = GetEventDiscount();
		long num2 = eventDiscount;
		int num3 = 0;
		bool flag2 = (ulong)num2 > (ulong)num3;
		UIHelper.EnableGameObject(LabelDiscount, flag2);
		UIHelper.EnableGameObject(ButtonEventGo, flag2);
		if (flag2)
		{
			UIHelper.SetLabelText(LabelDiscount, Vespa_Int_Extension_Method.ToCommaString(num2));
		}
		UIHelper.SetLabelColor(LabelReqGold, color);
		UIHelper.SetLabelText(LabelReqGold, Vespa_Int_Extension_Method.ToCommaString(num));
		bool buttonState = _selectedInfo.IsAnySelected() && flag;
		if (LabelAligner != null)
		{
			LabelAligner.Align();
		}
		SetButtonState(buttonState);
	}

	public void SetButtonState(bool isEnable)
	{
		UIHelper.EnableButton(ButtonIndentfied, isEnable, grayscaleIfDisable: true);
	}

	public void Refresh()
	{
		_selectedInfo.Clear();
		SetReuseItemIcon();
		SetReqGoldLabel();
	}

	public override void OnClosing()
	{
		base.OnClosing();
		_selectedInfo.Clear();
		FilterPanel.Close();
		ReleaseIconProxy();
		ReleaseItemIcons();
		GlobalAutoBehaviour<AccountManager>.instance.RemoveUserObserver(this);
	}

	private void ReleaseIconProxy()
	{
		FactoryBase.SafeReleaseComponent(ref _iconProxy);
	}

	public void OnClickIdentified()
	{
		if (_listener != null)
		{
			_listener.OnClickIdentified(_selectedInfo.GetSelectedSlotIndieces());
		}
	}

	private void ShowIdentifiedPanel(bool isOnItemPanel)
	{
		FilterPanel.OnOffRoot(!isOnItemPanel);
		UIHelper.EnableGameObject(EquipItemRoot, isOnItemPanel);
	}

	public void OnClickOpenFilter()
	{
		ShowIdentifiedPanel(isOnItemPanel: false);
	}

	public void OnClickCloseFilter()
	{
		ShowIdentifiedPanel(isOnItemPanel: true);
	}

	private void FilterReset()
	{
		FilterPanel.Reset();
	}

	public void OnClickClearFilter()
	{
		FilterReset();
		Refresh();
	}

	public void OnClickComfirmFilter()
	{
		Refresh();
		OnClickCloseFilter();
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent itemIconComponent)
	{
		if (itemIconComponent == null)
		{
			return;
		}
		EquipItemInfo equipItemInfo = itemIconComponent.EquipItemInfo;
		if (equipItemInfo != null)
		{
			if (_selectedInfo.IsSelectMax(equipItemInfo.SlotIndex))
			{
				string text = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
				string stringDirect = LocalizationManager.GetStringDirect("VALANCE_IDENTIFIED_MAXCOUNTPOPUP", _selectedLimitCount);
				MessagePopup.Open(text, stringDirect);
			}
			else
			{
				bool selected = _selectedInfo.Selected(equipItemInfo);
				itemIconComponent.SetSelected(selected);
				SetReqGoldLabel();
			}
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<UserManager.UserStatInfo>.OnChanged(UserManager.UserStatInfo changedInfo)
	{
		if (_selectedInfo.IsAnySelected() && changedInfo.UserStatType == UserManager.UserStatType.Gold)
		{
			SetReqGoldLabel();
		}
	}

	void IUserObserver<NGame2.NAccount.UserEquipItemManager.EquipStatInfo>.OnChanged(NGame2.NAccount.UserEquipItemManager.EquipStatInfo changedInfo)
	{
		if (changedInfo != null && changedInfo.StatType == NGame2.NAccount.UserEquipItemManager.EquipStatType.Add)
		{
			EquipItemInfo equipItemInfo = changedInfo.EquipItemInfo;
			if (equipItemInfo != null && equipItemInfo.IsTechnoMagic && !equipItemInfo.IsIdentified)
			{
				SetReuseItemIcon();
			}
		}
	}
}
