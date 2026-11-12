using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NSoulWeapon;

[Serializable]
public class SoulWeaponReinforceMaterialListInfo : ItemIconComponent.Listener
{
	public interface IListener
	{
		void SelectReinforceMaterial(ItemData selectItemData);
	}

	public UIScrollGrid ReinforceMaterialScrollGrid;

	private List<ItemIconComponent> _reinforceMaterialItemIconComponentList = new List<ItemIconComponent>();

	private ItemIconComponent _selectItemIconComponent;

	private IListener _iListener;

	public SoulWeaponEtherData SoulWeaponEhterData
	{
		get
		{
			if (_selectItemIconComponent == null || _selectItemIconComponent.ItemData == null)
			{
				return null;
			}
			return GameRoot.Instance.SoulWeaponEtherDataContainer.GetData(_selectItemIconComponent.ItemData.Code);
		}
	}

	public int ReinforceMaterialMaxCount
	{
		get
		{
			if (_selectItemIconComponent == null || _selectItemIconComponent.ItemData == null)
			{
				return 0;
			}
			return GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(_selectItemIconComponent.ItemData.Code);
		}
	}

	public void Open(IListener iListener, int reinforceLevel)
	{
		Init(iListener, reinforceLevel);
	}

	private void Init(IListener iListener, int reinforceLevel)
	{
		_iListener = iListener;
		SetReinforceMaterialList(reinforceLevel);
	}

	public void Close()
	{
		ClearList();
		_selectItemIconComponent = null;
	}

	private void ClearList()
	{
		FactoryBase.ReleaseAndClearList(_reinforceMaterialItemIconComponentList);
	}

	private void SetReinforceMaterialList(int reinforceLevel)
	{
		ClearList();
		IList<SoulWeaponEtherData> allData = GameRoot.Instance.SoulWeaponEtherDataContainer.GetAllData();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(allData))
		{
			return;
		}
		SoulWeaponReinforceData data = GameRoot.Instance.SoulWeaponReinforceDataContainer.GetData(reinforceLevel);
		if (data != null)
		{
			for (int i = 0; i < allData.Count; i++)
			{
				AddSoulWeaponReinforceMateralComponent(allData[i], data.UnAvailableItemCode);
			}
			ReinforceMaterialScrollGrid.RepositionGrid();
		}
	}

	private void AddSoulWeaponReinforceMateralComponent(SoulWeaponEtherData data, string[] unAvailableItemCodes)
	{
		if (data == null || (!__Extension_Method_ICollection__.IsNullOrEmpty(unAvailableItemCodes) && unAvailableItemCodes.ToList().Contains(data.ItemCode)))
		{
			return;
		}
		int itemCount = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(data.ItemCode);
		long num = itemCount;
		int num2 = 0;
		if (num <= num2)
		{
			return;
		}
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(data.ItemCode);
		if (dataByCode != null)
		{
			ItemIconComponent itemIconComponent = UIHelper.AddGridItem(ReinforceMaterialScrollGrid, ItemIconComponent.PrefabAuto);
			if (!(itemIconComponent == null))
			{
				itemIconComponent.Open(dataByCode, this);
				itemIconComponent.SetItemCount(num);
				_reinforceMaterialItemIconComponentList.Add(itemIconComponent);
			}
		}
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent itemIconComponent)
	{
		if (!(itemIconComponent == null))
		{
			if (_selectItemIconComponent != null)
			{
				_selectItemIconComponent.SetSelected(selected: false);
			}
			_selectItemIconComponent = itemIconComponent;
			_selectItemIconComponent.SetSelected(selected: true);
			if (_iListener != null)
			{
				_iListener.SelectReinforceMaterial(_selectItemIconComponent.ItemData);
			}
		}
	}
}
