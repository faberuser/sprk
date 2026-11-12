using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NShared;
using NShared.UseNickChangeItem;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWindow;

public class EquipOptionSelectConfirmPopup : BaseWindow
{
	public interface Listener
	{
		void OnUseOptionSelectItem(int useItemIndex, int equipItemIndex, int[] optionIndices);
	}

	public const string PrefabPath = "Prefab2/Window/Inventory/EquipOptionSelectConfirmPopup";

	public UISimpleGrid Grid_Item;

	public UIScrollGrid Grid_Option;

	private Listener _listener;

	private ItemData _itemData;

	private int _useItemIndex;

	private int _equipItemIndex;

	private int[] _optionIndices;

	private ItemIconComponent _selectedItemIcon;

	private List<EquipOptionSelectConfirmPopupComponent> _optionComponentList = new List<EquipOptionSelectConfirmPopupComponent>();

	public void Open(Listener listener, int useItemIndex, int equipItemIndex, int[] optionIndices)
	{
		base.Open();
		if (listener == null)
		{
			VespaDebug.LogError("listener == null");
			return;
		}
		_listener = listener;
		_useItemIndex = useItemIndex;
		_equipItemIndex = equipItemIndex;
		_optionIndices = optionIndices;
	}

	public void SetItem()
	{
		_itemData = GameRoot.Instance.ItemDataContainer.GetData(_equipItemIndex);
		if (_itemData != null)
		{
			_selectedItemIcon = UIHelper.AddGridItem(Grid_Item, ItemIconComponent.PrefabAuto);
			_selectedItemIcon.Open(_itemData);
			int num = 1;
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_optionIndices); i++)
			{
				EquipOptionSelectConfirmPopupComponent equipOptionSelectConfirmPopupComponent = UIHelper.AddGridItem(Grid_Option, EquipOptionSelectConfirmPopupComponent.PrefabAuto);
				equipOptionSelectConfirmPopupComponent.Open(num++, _optionIndices[i]);
				_optionComponentList.Add(equipOptionSelectConfirmPopupComponent);
			}
			UIHelper.RepositionGrid(Grid_Option);
		}
	}

	public override void Reset()
	{
		base.Reset();
		_listener = null;
		_itemData = null;
		FactoryBase.ReleaseComponent(_selectedItemIcon);
		FactoryBase.ReleaseAndClearList(_optionComponentList);
		_selectedItemIcon = null;
	}

	public void OnClickClose()
	{
		Close();
	}

	public void OnClickConfirm()
	{
		if (_listener != null)
		{
			_listener.OnUseOptionSelectItem(_useItemIndex, _equipItemIndex, _optionIndices);
			Close();
		}
	}

	private void OnUseNick(ResultType result)
	{
		Close();
	}
}
