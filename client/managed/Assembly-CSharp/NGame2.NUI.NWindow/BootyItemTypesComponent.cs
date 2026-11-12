using System;
using System.Collections.Generic;
using NGame2.NUI.NWindow.NInventory;
using NShared;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class BootyItemTypesComponent : MonoBehaviour
{
	public interface Listener
	{
		bool NewItemExists(ItemType[] itemTypes, EquipPartType[] equipPartTypes);

		void OnSelectButton(InvenList.EToggle toggle, ItemType[] itemTypes);
	}

	[Serializable]
	public class PopupButtonUIData
	{
		public InvenList.EToggle Type;

		public UIButton Button;

		public UILabel LabelInvenToggle;

		public ItemType[] ItemTypes;

		public EquipPartType[] PartTypes;

		public UIWidget WidgetNew;
	}

	public List<PopupButtonUIData> PopupButtonList;

	public UILabel LabelInvenType;

	public UIWidget InvenTypeNewMark;

	public UIGrid PopupButtonGrid;

	private Listener _listener;

	public InvenList.EToggle SelectedToggle { get; private set; }

	public void Init(Listener listener, InvenList.EToggle toggle)
	{
		_listener = listener;
		SelectCategory(toggle);
		SetupNewTagInButtons();
	}

	private void SelectCategory(InvenList.EToggle toggle)
	{
		PopupButtonUIData popupButtonUIData = GetPopupButtonUIData(toggle);
		SelectCategoryInternal(popupButtonUIData);
	}

	public void SetupNewTagInButtons()
	{
		if (PopupButtonList == null || _listener == null)
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(PopupButtonList); i++)
		{
			PopupButtonUIData popupButtonUIData = PopupButtonList[i];
			bool enable = _listener.NewItemExists(popupButtonUIData.ItemTypes, popupButtonUIData.PartTypes);
			UIHelper.EnableGameObject(popupButtonUIData.WidgetNew, enable);
			if (SelectedToggle == popupButtonUIData.Type)
			{
				UIHelper.EnableGameObject(InvenTypeNewMark, enable);
			}
		}
	}

	public void OnClickPopupButton(GameObject obj)
	{
		PopupButtonUIData popupButtonUIData = GetPopupButtonUIData(obj);
		SelectCategoryInternal(popupButtonUIData);
	}

	private void SelectCategoryInternal(PopupButtonUIData popupButtonUIData)
	{
		if (popupButtonUIData != null)
		{
			SelectedToggle = popupButtonUIData.Type;
			UIHelper.SetLabelText(LabelInvenType, popupButtonUIData.LabelInvenToggle.text);
			if (_listener != null)
			{
				_listener.OnSelectButton(popupButtonUIData.Type, popupButtonUIData.ItemTypes);
			}
		}
	}

	private PopupButtonUIData GetPopupButtonUIData(GameObject buttonObject)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(PopupButtonList); i++)
		{
			PopupButtonUIData popupButtonUIData = PopupButtonList[i];
			if (Vespa_Common_Class_Extension_Method.EQUALS(popupButtonUIData.Button.gameObject, buttonObject))
			{
				return popupButtonUIData;
			}
		}
		return null;
	}

	private PopupButtonUIData GetPopupButtonUIData(InvenList.EToggle toggle)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(PopupButtonList); i++)
		{
			PopupButtonUIData popupButtonUIData = PopupButtonList[i];
			if (popupButtonUIData.Type == toggle)
			{
				return popupButtonUIData;
			}
		}
		return null;
	}

	public ItemType[] GetSelectedBootyItemTypes()
	{
		return GetPopupButtonUIData(SelectedToggle)?.ItemTypes;
	}
}
