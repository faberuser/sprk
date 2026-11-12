using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NNguiExtension;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class InvenUsePetSelect : BaseWindow, PetPortraitComponent.IListener
{
	public interface IListener
	{
		void OnConfirmPet(int itemIndex, int petIndex, Action onComplete);

		void OpenPetDetailView(int petIndex);
	}

	public const string PrefabPath = "Prefab2/Window/Inventory/InvenUsePetSelect";

	public UIComboBox ComboBoxFilterMain;

	public Transform SelectedPetRoot;

	public UILabel LabelTouchPet;

	public UILabel LabelNonePet;

	public UIButton ButtonConfirm;

	public UILabel LabelConfirm;

	public UIScrollGrid PetListGrid;

	private IListener _iListener;

	private ItemData _itemData;

	private PetSelectItemData _petSelectItemData;

	private PetPortraitComponent _selectedPetPortrait;

	private List<PetPortraitComponent> _petPortraitList = new List<PetPortraitComponent>();

	public void Open(IListener listener, ItemData itemData)
	{
		Open(delegate
		{
			Init(listener, itemData);
		}, null);
	}

	private void Init(IListener listener, ItemData itemData, CreatureData selectedPetData = null)
	{
		_iListener = listener;
		if (itemData != null)
		{
			_itemData = itemData;
			_petSelectItemData = GameRoot.Instance.PetSelectItemDataContainer.GetData(itemData.Code);
			if (_petSelectItemData != null)
			{
				InitFilterComboBox();
				CreatePetPortraitList(selectedPetData);
			}
		}
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.SafeReleaseComponent(ref _selectedPetPortrait);
		ClearPetPortraitList();
		UpdateInfoLabel(isExist: false);
		UpdateConfirmButtonState(isSelect: false);
	}

	private void CreatePetPortraitList(CreatureData selectedPetData = null)
	{
		RefreshPetList();
		SelectPet(selectedPetData, isScrollTo: true);
	}

	private void OnChangePetList(int val)
	{
		RefreshPetList();
	}

	private void RefreshPetList()
	{
		ClearPetPortraitList();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_petSelectItemData.PetIndices); i++)
		{
			int petIndex = _petSelectItemData.PetIndices[i];
			AddPetComponentByFilter(petIndex);
		}
		UIHelper.RepositionGrid(PetListGrid);
		UIHelper.EnableGameObject(LabelNonePet, __Extension_Method_ICollection__.COUNT(_petPortraitList) == 0);
	}

	private void AddPetComponentByFilter(int petIndex)
	{
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(petIndex);
		if (data == null)
		{
			return;
		}
		SelectFilter.ComboBoxType currentKey = (SelectFilter.ComboBoxType)ComboBoxFilterMain.CurrentKey;
		bool flag = GlobalAutoBehaviour<AccountManager>.instance.PetManager.Get(petIndex) != null;
		bool flag2 = _selectedPetPortrait != null && _selectedPetPortrait.CreatureData != null && _selectedPetPortrait.CreatureData.Index == data.Index;
		if ((currentKey != SelectFilter.ComboBoxType.OWNED || flag) && (currentKey != SelectFilter.ComboBoxType.NOT_OWNED || !flag))
		{
			PetPortraitComponent petPortraitComponent = UIHelper.AddGridItem(PetListGrid, PetPortraitComponent.SmallPrefabAuto);
			if (!(petPortraitComponent == null))
			{
				petPortraitComponent.Open(data, this);
				petPortraitComponent.SetOwned(flag);
				petPortraitComponent.SetSelection(flag2);
				_petPortraitList.Add(petPortraitComponent);
			}
		}
	}

	public void SelectPet(CreatureData petData, bool isScrollTo = false)
	{
		if (petData != null)
		{
			UnselectAllPetPortraitComponent();
			SelectPetPortraitComponentList(petData, isScrollTo);
			SelectPetPortraitLargeComponent(petData);
			bool isExist = GlobalAutoBehaviour<AccountManager>.instance.PetManager.Get(petData.Index) != null;
			UpdateInfoLabel(isExist);
			bool isSelect = _selectedPetPortrait != null;
			UpdateConfirmButtonState(isSelect);
		}
	}

	public void OnClickSelectConfirm()
	{
		if (_iListener != null)
		{
			int itemIndex = ((_itemData != null) ? _itemData.Index : 0);
			int petIndex = ((_selectedPetPortrait != null && _selectedPetPortrait.CreatureData != null) ? _selectedPetPortrait.CreatureData.Index : 0);
			_iListener.OnConfirmPet(itemIndex, petIndex, Close);
		}
	}

	void PetPortraitComponent.IListener.OnClick(PetPortraitComponent comp)
	{
		if (comp == null || comp.CreatureData == null)
		{
			return;
		}
		CreatureData creatureData = comp.CreatureData;
		if (_selectedPetPortrait == comp)
		{
			if (_iListener != null)
			{
				_iListener.OpenPetDetailView(creatureData.Index);
			}
		}
		else
		{
			SelectPet(creatureData);
		}
	}

	private void InitFilterComboBox()
	{
		ComboBoxFilterMain.Clear();
		ComboBoxFilterMain.AddItem(0, LocalizationManager.GetString(FilterSelect.FILTER_SELECT_ALL));
		ComboBoxFilterMain.AddItem(5, LocalizationManager.GetString(FilterSelect.FILTER_SELECT_OWNED));
		ComboBoxFilterMain.AddItem(4, LocalizationManager.GetString(FilterSelect.FILTER_SELECT_NOT_OWNED));
		ComboBoxFilterMain.OnChanged = OnChangePetList;
		ComboBoxFilterMain.Reposition();
		ComboBoxFilterMain.SelectFirstItem();
	}

	private void UnselectAllPetPortraitComponent()
	{
		FactoryBase.SafeReleaseComponent(ref _selectedPetPortrait);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_petPortraitList); i++)
		{
			PetPortraitComponent petPortraitComponent = _petPortraitList[i];
			if (!(petPortraitComponent == null))
			{
				petPortraitComponent.SetSelection(select: false);
			}
		}
	}

	private void SelectPetPortraitLargeComponent(CreatureData petData)
	{
		_selectedPetPortrait = PetPortraitComponent.LargePrefabAuto.Factory.Create(SelectedPetRoot);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_selectedPetPortrait.transform);
		_selectedPetPortrait.Open(petData, this, PetPortraitComponent.EPortraitMode.Long);
		_selectedPetPortrait.ShowPetName();
	}

	private void SelectPetPortraitComponentList(CreatureData creatureData, bool isScrollTo)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_petPortraitList); i++)
		{
			PetPortraitComponent petPortraitComponent = _petPortraitList[i];
			if (!(petPortraitComponent == null) && petPortraitComponent.CreatureData != null && petPortraitComponent.CreatureData.Index == creatureData.Index)
			{
				petPortraitComponent.SetSelection(select: true);
				if (PetListGrid != null && isScrollTo)
				{
					PetListGrid.ScrollTo(petPortraitComponent, -60f, 0f);
				}
				break;
			}
		}
	}

	private void UpdateInfoLabel(bool isExist)
	{
		string stringDirect = LocalizationManager.GetStringDirect(isExist ? "GET_RAIDERSOUL_SELECT_BUTTON" : "GET_RAIDER_SELECT_BUTTON");
		UIHelper.SetLabelText(LabelConfirm, stringDirect);
	}

	private void UpdateConfirmButtonState(bool isSelect)
	{
		UIHelper.EnableButton(ButtonConfirm, isSelect, grayscaleIfDisable: true);
	}

	private void ClearPetPortraitList()
	{
		FactoryBase.ReleaseAndClearList(_petPortraitList);
	}
}
