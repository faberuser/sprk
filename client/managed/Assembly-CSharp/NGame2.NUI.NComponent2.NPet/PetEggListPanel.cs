using System.Collections.Generic;
using NGame2.NAccount;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPet;

public class PetEggListPanel : MonoBehaviour, IIncubatorManagePanel
{
	public interface IListener
	{
		void OnClickStartIncubating();

		void OnClickCancel();

		void OnClickComplete();
	}

	public UIWidget RootWidget;

	public UIWidget Widget_EggListRoot;

	public UIWidget Widget_IncubatingLabelRoot;

	public UILabel Label_EmptyEgg;

	public UIScrollGrid PetEggListGrid;

	public UIButton Button_StartIncubating;

	public UILabel Label_IncubatingState;

	public UIButton Button_CancelIncubating;

	public UIButton Button_CompleteIncubating;

	private IIncubatorDataProvider _dataProvider;

	private IListener _listener;

	private ItemIconComponent.Listener _itemIconComplistener;

	private List<ItemIconComponent> _eggIconCompList = new List<ItemIconComponent>();

	public void Open(IIncubatorDataProvider dataProvider, IListener listener, ItemIconComponent.Listener itemIconCompListener)
	{
		_dataProvider = dataProvider;
		_listener = listener;
		_itemIconComplistener = itemIconCompListener;
		InitEggListUI();
	}

	public void InitEggListUI()
	{
		SetEggList();
		EnableIncubatingBtn(isEnable: false);
		RefreshByState();
	}

	public void RefreshByState()
	{
		EnableIncubatingBtn(isEnable: false);
		if (_dataProvider == null)
		{
			return;
		}
		EIncubatingState incubatingState = _dataProvider.GetIncubatingState();
		bool flag = incubatingState == EIncubatingState.Incubating || incubatingState == EIncubatingState.Complete;
		UIHelper.EnableGameObject(Widget_EggListRoot, !flag);
		UIHelper.EnableGameObject(Widget_IncubatingLabelRoot, flag);
		UIHelper.EnableGameObject(Button_CancelIncubating, enable: false);
		UIHelper.EnableGameObject(Button_CompleteIncubating, enable: false);
		if (flag)
		{
			switch (incubatingState)
			{
			case EIncubatingState.Incubating:
				UIHelper.SetLabelText(Label_IncubatingState, LocalizationManager.GetStringDirect("PETINCUBATOR_HATCHING_PROGRESSING"));
				UIHelper.EnableGameObject(Button_CancelIncubating, enable: true);
				UIHelper.EnableGameObject(Button_CompleteIncubating, enable: false);
				break;
			case EIncubatingState.Complete:
				UIHelper.SetLabelText(Label_IncubatingState, LocalizationManager.GetStringDirect("PETINCUBATOR_HATCHING_COMPLETE_DESC"));
				UIHelper.EnableGameObject(Button_CancelIncubating, enable: false);
				UIHelper.EnableGameObject(Button_CompleteIncubating, enable: true);
				break;
			}
		}
	}

	public void EnableIncubatingBtn(bool isEnable)
	{
		UIHelper.EnableButton(Button_StartIncubating, isEnable);
	}

	public void SetEggList()
	{
		FactoryBase.ReleaseAndClearList(_eggIconCompList);
		List<ItemInfo> itemsByType = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemsByType(ItemType.Egg);
		itemsByType.Sort(delegate(ItemInfo lhs, ItemInfo rhs)
		{
			if (lhs == null)
			{
				return 1;
			}
			if (rhs == null)
			{
				return -1;
			}
			ItemData data2 = GameRoot.Instance.ItemDataContainer.GetData(lhs.ItemIndex);
			ItemData data3 = GameRoot.Instance.ItemDataContainer.GetData(rhs.ItemIndex);
			if (data2 == null)
			{
				return 1;
			}
			if (data3 == null)
			{
				return -1;
			}
			int num3 = data3.Grade.CompareTo(data2.Grade);
			return (num3 != 0) ? num3 : lhs.ItemIndex.CompareTo(rhs.ItemIndex);
		});
		UIHelper.EnableGameObject(Label_EmptyEgg, __Extension_Method_ICollection__.COUNT(itemsByType) == 0);
		if (__Extension_Method_ICollection__.COUNT(itemsByType) != 0)
		{
			for (int num = 0; num < __Extension_Method_ICollection__.COUNT(itemsByType); num++)
			{
				ItemInfo itemInfo = itemsByType[num];
				ItemData data = GameRoot.Instance.ItemDataContainer.GetData(itemInfo.ItemIndex);
				ItemIconComponent itemIconComponent = UIHelper.AddGridItem(PetEggListGrid, ItemIconComponent.PrefabAuto);
				itemIconComponent.Open(data, _itemIconComplistener);
				int count = itemInfo.Count;
				Color? color = null;
				int num2 = count;
				itemIconComponent.SetItemCount(num2, color);
				_eggIconCompList.Add(itemIconComponent);
			}
			UIHelper.RepositionGrid(PetEggListGrid);
		}
	}

	public void EnableRoot(bool isEnable)
	{
		UIHelper.EnableGameObject(RootWidget, isEnable);
	}

	public void Close()
	{
		_dataProvider = null;
		_listener = null;
		_itemIconComplistener = null;
		FactoryBase.ReleaseAndClearList(_eggIconCompList);
	}

	public void OnClickStartIncubating()
	{
		if (_listener != null)
		{
			_listener.OnClickStartIncubating();
		}
	}

	public void OnClickCancel()
	{
		if (_listener != null)
		{
			_listener.OnClickCancel();
		}
	}

	public void OnClickComplete()
	{
		if (_listener != null)
		{
			_listener.OnClickComplete();
		}
	}
}
