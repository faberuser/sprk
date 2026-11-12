using System;
using System.Collections.Generic;
using System.Text;
using NGame2.NAccount;
using NGame2.NUI.NComponent2.NCostume;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NCustomizing;

public class MySetCostumeViewWidget : WidgetBase, MySetCostumeComponent.Listener
{
	public interface IListener
	{
		void OnClickEquip(MySetCostumeViewWidget caller, Action<bool> OnComplete);

		void OnClickSave(MySetCostumeViewWidget caller);
	}

	public UIButton ButtonSave;

	public UIButton ButtonEquip;

	public UIBaseGrid CostumeGrid;

	private List<MySetCostumeComponent> _mySetComponentList = new List<MySetCostumeComponent>();

	private List<string> _costumeNameList = new List<string>();

	private IListener _listener;

	private CachedHeroInfo _mySetDummyHeroInfo;

	public MySetCostumeComponent.OpenData CurrentComponentOpenData { get; private set; }

	void MySetCostumeComponent.Listener.OnClick(MySetCostumeComponent component)
	{
		SelectCostume(component.ComponentOpenData);
	}

	public void ClearCostumeComponentList()
	{
		FactoryBase.ReleaseAndClearList(_mySetComponentList);
	}

	private void Open()
	{
		if (!CurrentComponentOpenData.IsNull())
		{
			SelectCostumeComponent();
			return;
		}
		InitCostumeStorageList();
		SelectCostumeComponent();
	}

	private void SelectCostumeComponent()
	{
		if (base.HeroInfo == null)
		{
			return;
		}
		CachedHeroInfo localHeroInfo = _renderFacadeController.GetLocalHeroInfo();
		if (localHeroInfo == null)
		{
			return;
		}
		MySetCostumeComponent.OpenData componentOpenData = MySetCostumeComponent.OpenData.CreateCurrentCostume();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_mySetComponentList); i++)
		{
			if (_mySetComponentList[i].ComponentOpenData.ComponentType == MySetCostumeComponent.ComponentType.Storage && localHeroInfo.IsEqualCostumeInfo(_mySetComponentList[i].ComponentOpenData.CostumeStorage))
			{
				componentOpenData = _mySetComponentList[i].ComponentOpenData;
				break;
			}
		}
		SelectCostume(componentOpenData);
	}

	private void InitCostumeStorageList()
	{
		if (base.HeroInfo != null)
		{
			ClearCostumeComponentList();
			CreateCostumeList(base.HeroInfo, CostumeGrid, this);
		}
	}

	private void CreateCostumeList(HeroInfo heroInfo, UIBaseGrid CostumeGrid, MySetCostumeComponent.Listener listener)
	{
		if (heroInfo != null)
		{
			AddCurrentCostumeComponent();
			int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("CostumeSorageSlotMaxCount");
			for (int i = 1; i <= valueInt; i++)
			{
				CostumeStorageSlotInfo costumeStorageSlotInfo = GlobalAutoBehaviour<AccountManager>.instance.CostumeStorageManager.Get(new CostumeStorageSlotInfoKey(heroInfo.HeroIndex, i));
				MySetCostumeComponent mySetCostumeComponent = CostumeGrid.AddGridItem(MySetCostumeComponent.PrefabAuto);
				MySetCostumeComponent.OpenData openData = ((costumeStorageSlotInfo == null) ? MySetCostumeComponent.OpenData.CreateEmptySlot(i) : MySetCostumeComponent.OpenData.CreateStorageSlot(costumeStorageSlotInfo));
				mySetCostumeComponent.Open(openData, this);
				_mySetComponentList.Add(mySetCostumeComponent);
			}
			CostumeGrid.RepositionGrid();
		}
	}

	private void AddCurrentCostumeComponent()
	{
		MySetCostumeComponent.OpenData openData = MySetCostumeComponent.OpenData.CreateCurrentCostume();
		MySetCostumeComponent mySetCostumeComponent = CostumeGrid.AddGridItem(MySetCostumeComponent.PrefabAuto);
		mySetCostumeComponent.Open(openData, this);
		_mySetComponentList.Add(mySetCostumeComponent);
	}

	private void UpdateCostumeListSelectDeselect(MySetCostumeComponent.OpenData selectComponent, MySetCostumeComponent.OpenData deselectComponent, List<MySetCostumeComponent> costumeComponentList)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(costumeComponentList); i++)
		{
			MySetCostumeComponent.OpenData componentOpenData = costumeComponentList[i].ComponentOpenData;
			if (componentOpenData.IsEqual(deselectComponent))
			{
				costumeComponentList[i].SetSelected(Selected: false);
			}
			if (componentOpenData.IsEqual(selectComponent))
			{
				costumeComponentList[i].SetSelected(Selected: true);
			}
		}
	}

	public void SelectCostume(MySetCostumeComponent.OpenData componentOpenData)
	{
		MySetCostumeComponent.OpenData currentComponentOpenData = CurrentComponentOpenData;
		CurrentComponentOpenData = componentOpenData;
		UpdateCostumeListSelectDeselect(CurrentComponentOpenData, currentComponentOpenData, _mySetComponentList);
		UpdateButtonState();
		UpdateCurrentCostumeStatus();
		ShowPreview();
	}

	public void ShowPreview()
	{
		if (CurrentComponentOpenData.ComponentType == MySetCostumeComponent.ComponentType.Empty || CurrentComponentOpenData.ComponentType == MySetCostumeComponent.ComponentType.CurrentCostume)
		{
			ChangeRenderFacadeToLocalHeroInfo();
		}
		else if (CurrentComponentOpenData.ComponentType == MySetCostumeComponent.ComponentType.Storage)
		{
			ChangeRenderFacadeToStorageCostume();
		}
	}

	public void UpdateButtonState()
	{
		if (base.HeroInfo != null)
		{
			bool flag = CurrentComponentOpenData.ComponentType == MySetCostumeComponent.ComponentType.Storage;
			bool flag2 = CurrentComponentOpenData.ComponentType == MySetCostumeComponent.ComponentType.CurrentCostume;
			CachedHeroInfo localHeroInfo = _renderFacadeController.GetLocalHeroInfo();
			UIHelper.EnableButton(ButtonSave, !flag2 && IsOwnHero() && !localHeroInfo.IsEqualCostumeInfo(CurrentComponentOpenData.CostumeStorage), grayscaleIfDisable: true);
			UIHelper.EnableButton(ButtonEquip, IsOwnHero() && flag && !localHeroInfo.IsEqualCostumeInfo(CurrentComponentOpenData.CostumeStorage), grayscaleIfDisable: true);
		}
	}

	public void UpdateCurrentCostumeStatus()
	{
		UIHelper.EnableGameObject(base.Label_AbilityDesc, enable: false);
		UIHelper.EnableGameObject(base.Label_CostumeName, enable: false);
		UIHelper.EnableGameObject(base.WidgetPriceDesc, enable: false);
		UIHelper.EnableGameObject(base.WidgetMileageDesc, enable: false);
		UIHelper.EnableGameObject(base.WidgetNeedCostume, enable: false);
		UIHelper.EnableGameObject(base.WidgetObtainDesc, enable: false);
		UIHelper.EnableGameObject(base.WidgetOwned, enable: false);
		UIHelper.EnableGameObject(base.WidgetEquipped, enable: false);
	}

	private bool IsOwnHero()
	{
		return GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(base.HeroInfo.HeroIndex) != null;
	}

	public override void OnEquipSet()
	{
		base.OnEquipSet();
		if (_listener == null)
		{
			return;
		}
		_costumeNameList.Clear();
		CurrentComponentOpenData.CostumeStorage.SetCostumeNameList(_costumeNameList);
		ShowCostumeListPopup("CONTENTS_CUSTOMIZE_MY_SET_LOAD_POPUP_TITLE", "CONTENTS_CUSTOMIZE_MY_SET_LOAD_POPUP_DESC", _costumeNameList, delegate
		{
			_listener.OnClickEquip(this, delegate(bool result)
			{
				if (result)
				{
					_renderFacadeController.ResetWidgets(this);
					ChangeRenderFacadeToLocalHeroInfo();
				}
			});
		});
	}

	public override void OnSaveSet()
	{
		base.OnSaveSet();
		if (_listener == null)
		{
			return;
		}
		_costumeNameList.Clear();
		_renderFacadeController.GetLocalHeroInfo().SetCostumeNameList(_costumeNameList);
		ShowCostumeListPopup("CONTENTS_CUSTOMIZE_MY_SET_SAVE_POPUP_TITLE", "CONTENTS_CUSTOMIZE_MY_SET_SAVE_POPUP_DESC", _costumeNameList, delegate
		{
			_listener.OnClickSave(this);
			if (CurrentComponentOpenData.ComponentType == MySetCostumeComponent.ComponentType.Storage)
			{
				ChangeRenderFacadeToLocalHeroInfo();
			}
		});
	}

	private void ShowCostumeListPopup(string titleKey, string messageKey, List<string> costumeNameList, Action onConfirm)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (__Extension_Method_ICollection__.COUNT(costumeNameList) <= 0)
		{
			CostumeData dataByHeroStar = GameRoot.Instance.CostumeDataContainer.GetDataByHeroStar(base.HeroInfo.HeroIndex, base.HeroInfo.Star);
			costumeNameList.Add(dataByHeroStar.CostumeName);
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(costumeNameList); i++)
		{
			stringBuilder.Append(LocalizationManager.GetStringDirect("CONTENTS_CUSTOMIZE_MY_SET_NAME_FORM", i + 1, costumeNameList[i]));
			if (i < __Extension_Method_ICollection__.COUNT(costumeNameList) - 1)
			{
				stringBuilder.AppendLine();
			}
		}
		string stringDirect = LocalizationManager.GetStringDirect(titleKey);
		string stringDirect2 = LocalizationManager.GetStringDirect(messageKey, stringBuilder.ToString());
		MessagePopup.Open(stringDirect, stringDirect2, onConfirm, null);
	}

	public void SetListener(IListener listener)
	{
		_listener = listener;
	}

	public override void Reset()
	{
		CurrentComponentOpenData = MySetCostumeComponent.OpenData.NullData;
		_mySetDummyHeroInfo = null;
		ClearCostumeComponentList();
	}

	public override void Open(ToggleType toggleType)
	{
		base.Open(toggleType);
		Open();
	}

	public override void OnClose()
	{
		base.OnClose();
		if (!CurrentComponentOpenData.IsNull())
		{
			ChangeRenderFacadeToLocalHeroInfo();
		}
	}

	public override void OnChanged(NGame2.NAccount.UserHeroManager.ChangeHeroInfo changeHeroInfo)
	{
		if (changeHeroInfo.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.Costume)
		{
			UpdateButtonState();
			UpdateCurrentCostumeStatus();
		}
	}

	public override void OnChanged(CostumeStorageSlotInfo changedInfo)
	{
		CostumeStorageSlotInfo costumeStorageSlotInfo = GlobalAutoBehaviour<AccountManager>.instance.CostumeStorageManager.Get(new CostumeStorageSlotInfoKey(changedInfo.HeroIndex, changedInfo.CostumeStorageSlotIndex));
		if (costumeStorageSlotInfo == null)
		{
			VespaDebug.LogError("OnChanged(NShared.CostumeStorageSlotInfo changedInfo) // costume slot info == null");
			return;
		}
		MySetCostumeComponent mySetCostumeComponent = _mySetComponentList.Find((MySetCostumeComponent component) => component.ComponentOpenData.IsEqual(CurrentComponentOpenData));
		MySetCostumeComponent.OpenData openData = ((costumeStorageSlotInfo == null) ? MySetCostumeComponent.OpenData.CreateEmptySlot(CurrentComponentOpenData.SlotIndex) : MySetCostumeComponent.OpenData.CreateStorageSlot(costumeStorageSlotInfo));
		mySetCostumeComponent.Open(openData, this);
		mySetCostumeComponent.SetSelected(Selected: true);
		CurrentComponentOpenData = openData;
		UpdateButtonState();
	}

	private void ChangeRenderFacadeToStorageCostume()
	{
		CachedHeroInfo localHeroInfo = _renderFacadeController.GetLocalHeroInfo();
		if (_mySetDummyHeroInfo == null || _mySetDummyHeroInfo.HeroIndex != localHeroInfo.HeroIndex)
		{
			_mySetDummyHeroInfo = JsonMarshaler.Clone(_renderFacadeController.GetLocalHeroInfo());
		}
		CostumeStorageSlotInfo costumeStorage = CurrentComponentOpenData.CostumeStorage;
		_renderFacadeController.ChangeRenderFacadeOnly(_mySetDummyHeroInfo.SetCostumeStorageSlotInfo(costumeStorage));
	}

	private void ChangeRenderFacadeToLocalHeroInfo()
	{
		_renderFacadeController.ChangeRenderFacadeOnly(_renderFacadeController.GetLocalHeroInfo());
	}
}
