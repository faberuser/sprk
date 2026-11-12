using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2.NCostume;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class HeroCostumeSelect : BaseWindow, HeroCostumeComponent.Listener
{
	public interface Listener
	{
		void OnSelectCostume(int useItemIndex, int CostumeIndex);
	}

	public const string PrefabPath = "Prefab2/Window/Inventory/HeroCostumeSelect";

	public UISimpleGrid SelectedCostumeGrid;

	public UIScrollGrid SelectableCostumeGrid;

	public UIButton Button_Select;

	public UILabel Label_CostumeSelect;

	private HeroCostumeComponent _selectedCostume;

	private List<HeroCostumeComponent> _ListSelectableCostume = new List<HeroCostumeComponent>();

	private Listener _listener;

	private ItemData _CostumeSelectItemData;

	public void Open(ItemData itemData, Listener listener)
	{
		base.Open();
		_CostumeSelectItemData = itemData;
		_listener = listener;
		UIHelper.EnableGameObject(Label_CostumeSelect, enable: true);
		UIHelper.EnableButton(Button_Select, enable: false);
		SetupCostumeList();
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(_ListSelectableCostume);
		FactoryBase.SafeReleaseComponent(ref _selectedCostume);
	}

	private void SetupCostumeList()
	{
		CostumeSelectItemData data = GameRoot.Instance.CostumeSelectItemDataContainer.GetData(_CostumeSelectItemData.Code);
		if (data == null)
		{
			VespaDebug.LogError("Costume Select Item Data Not Found, ItemCode = " + _CostumeSelectItemData.Code);
			return;
		}
		if (data.CostumeIndices == null || data.CostumeIndices.Length == 0)
		{
			VespaDebug.LogError("Costume Indices does not exist, ItemCode = " + _CostumeSelectItemData.Code);
			return;
		}
		for (int i = 0; i < data.CostumeIndices.Length; i++)
		{
			int key = data.CostumeIndices[i];
			if (GlobalAutoBehaviour<AccountManager>.instance.CostumeManager.Get(key) == null)
			{
				CostumeData data2 = GameRoot.Instance.CostumeDataContainer.GetData(key);
				if (data2 != null && data2.IsOpen && GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(data2.HeroIndex) != null)
				{
					HeroCostumeComponent heroCostumeComponent = UIHelper.AddGridItem(SelectableCostumeGrid, HeroCostumeComponent.PrefabAuto);
					heroCostumeComponent.Open(data2, this);
					_ListSelectableCostume.Add(heroCostumeComponent);
				}
			}
		}
		UIHelper.RepositionGrid(SelectableCostumeGrid);
	}

	public void OnClickExitButton()
	{
		Close();
	}

	void HeroCostumeComponent.Listener.OnClick(HeroCostumeComponent comp)
	{
		CostumeData costumeData = null;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_ListSelectableCostume); i++)
		{
			if (_selectedCostume != null && _ListSelectableCostume[i].CostumeData.CostumeIndex == _selectedCostume.CostumeData.CostumeIndex)
			{
				_ListSelectableCostume[i].SetSelected(Selected: false);
			}
			if (_ListSelectableCostume[i].CostumeData.CostumeIndex == comp.CostumeData.CostumeIndex)
			{
				_ListSelectableCostume[i].SetSelected(Selected: true);
				costumeData = _ListSelectableCostume[i].CostumeData;
			}
		}
		FactoryBase.SafeReleaseComponent(ref _selectedCostume);
		if (costumeData == null)
		{
			VespaDebug.LogError("Costume data doesn't exist in list, costumeIndex = " + comp.CostumeData.CostumeIndex);
			return;
		}
		_selectedCostume = UIHelper.AddGridItem(SelectedCostumeGrid, HeroCostumeComponent.PrefabAuto);
		_selectedCostume.Open(costumeData, null);
		UIHelper.EnableButton(Button_Select, enable: true);
	}

	public void OnClickSelect()
	{
		if (_listener != null && _selectedCostume != null && _CostumeSelectItemData != null)
		{
			_listener.OnSelectCostume(_CostumeSelectItemData.Index, _selectedCostume.CostumeData.CostumeIndex);
		}
	}

	void HeroCostumeComponent.Listener.OnClickChangeCos(int costumeIndex)
	{
	}
}
